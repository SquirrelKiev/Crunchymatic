using Crunchymatic.Web.Components;
using Crunchymatic.Web.Models;
using Crunchymatic.Web.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using NodaTime;
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

// Add services to the container.
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
        .FirstOrDefaultAsync(x => x.EpisodeCheckId == checkId && x.LanguageCode == languageCode,
            cancellationToken);

    if (file?.Content == null)
        return Results.NotFound();

    var content = await SubtitleStorageService.DecompressAsync(file.Content.CompressedContent, file.OriginalLength,
        cancellationToken);

    return Results.File(content, "application/octet-stream", file.OriginalFileName);
});

app.Run();