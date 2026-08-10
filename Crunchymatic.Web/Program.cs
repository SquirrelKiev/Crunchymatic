using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Crunchymatic.Web.Components;
using Crunchymatic.Web.Models;
using Crunchymatic.Web.Services;
using Vite.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration().WriteTo
    .Console(
        outputTemplate: "[FALLBACK] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Sixteen)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen));

builder.Services.AddDbContextFactory<CrunchymaticContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString(nameof(CrunchymaticContext)) ??
                           throw new InvalidOperationException(
                               $"Connection string '{nameof(CrunchymaticContext)}' not found.");

    options.UseNpgsql(connectionString,
        x => x.UseNodaTime().UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddViteServices();

// Add services to the container.
// TODO: This shouldn't be the default really. This is only here for /api/check/{checkId:int}
builder.Services.AddResponseCompression(x => x.EnableForHttps = true);
builder.Services.Configure<BrotliCompressionProviderOptions>(x => x.Level = CompressionLevel.Optimal);
builder.Services.ConfigureHttpJsonOptions(x =>
{
    x.SerializerOptions.ConfigureForNodaTime(new NodaJsonSettings(DateTimeZoneProviders.Tzdb));
    x.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

builder.Services.AddRazorComponents();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddSingleton<SubtitleStorageService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<SubtitleUploadReportStore>();

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = SubtitleStorageService.MaxRequestSize);

builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = SubtitleStorageService.MaxRequestSize);

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    
    app.UseWebSockets();
    app.UseViteDevelopmentServer(true);

    var dbMode = app.Configuration["NukeDb"];

    if (dbMode is not null)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CrunchymaticContext>>();

        await using var context = await factory.CreateDbContextAsync();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseHostFiltering();

app.UseAntiforgery();

app.UseResponseCompression();
app.MapStaticAssets();
app.MapRazorComponents<App>();

app.MapGet("/check/{checkId:int}/subtitle/{languageCode}", async (
    int checkId,
    string languageCode,
    IDbContextFactory<CrunchymaticContext> dbFactory,
    CancellationToken cancellationToken) =>
{
    await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);

    var file = await context.CheckedSubtitles
        .Include(x => x.Content)
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.EpisodeCheckId == checkId && x.LanguageCode == languageCode,
            cancellationToken);

    if (file?.Content == null)
        return Results.NotFound();

    var content = await SubtitleStorageService.DecompressAsync(file.Content.CompressedContent, file.OriginalLength,
        cancellationToken);

    return Results.File(content, "application/octet-stream", file.OriginalFileName);
});

app.MapGet("/api/check/{checkId:int}", async (
    int checkId,
    IDbContextFactory<CrunchymaticContext> dbFactory,
    CancellationToken cancellationToken) =>
{
    await using var context = await dbFactory.CreateDbContextAsync(cancellationToken);
    
    var check = await context.EpisodeChecks
        .Include(x => x.LinkedAnime)
        .Include(x => x.CheckedSubtitles.Where(s => s.UploadedAt != null)) // TODO: allow entries with no subs to be marked in some way (e.g. hardsub only)
        .ThenInclude(s => s.Content)
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == checkId, cancellationToken);
    
    if(check is null)
        return Results.NotFound(); // this would return HTML which isn't ideal for an endpoint that outputs JSON usually
    
    var subtitles = new List<SubtitleDto>();
    foreach (var checkedSubtitle in check.CheckedSubtitles.OrderBy(x => x.LanguageCode))
    {
        if (checkedSubtitle.Content is null) continue;
        
        var bytes = await SubtitleStorageService.DecompressAsync(checkedSubtitle.Content.CompressedContent, checkedSubtitle.OriginalLength, cancellationToken);
        
        subtitles.Add(new SubtitleDto(checkedSubtitle.LanguageCode,
            checkedSubtitle.OriginalFileName ?? $"{checkedSubtitle.LanguageCode}.ass",
            checkedSubtitle.SubtitlePipeline.ToString(), // TODO: This all needs hooking up to the actual analyzer really
            checkedSubtitle.TypesettingStyle.ToString(),
            Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF')));
    }
    
    return Results.Ok(new CheckPayloadDto(
        check.LinkedAnime?.EnglishTitle ?? "(unknown)",
        check.Episode,
        check.TimeSubtitlesGrabbed,
        subtitles));
});

app.Run();

record SubtitleDto(string LanguageCode, string FileName, string AutoPipeline, string AutoTypesetting, string Content);

record CheckPayloadDto(
    string AnimeTitle,
    string Episode,
    NodaTime.Instant SubtitlesGrabbed,
    IReadOnlyList<SubtitleDto> Subtitles);