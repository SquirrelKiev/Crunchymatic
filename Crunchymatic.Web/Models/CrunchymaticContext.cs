using Microsoft.EntityFrameworkCore;

namespace Crunchymatic.Web.Models;

public class CrunchymaticContext(DbContextOptions<CrunchymaticContext> contextOptions) : DbContext(contextOptions)
{
    public DbSet<Anime> Anime { get; set; }
    public DbSet<EpisodeCheck> EpisodeChecks { get; set; }
    public DbSet<CheckedSubtitle> CheckedSubtitles { get; set; }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        MarkReplacedSubtitleFilesAsUpdates();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        MarkReplacedSubtitleFilesAsUpdates();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    /// <summary>
    /// We try to avoid loading <see cref="CheckedSubtitle.Content"/> if we don't need it, as it's a relatively
    /// large amount of data. Downside is that EF then has nothing to change track when a file gets replaced -
    /// <see cref="CheckedSubtitle.AttachFile"/> hands it a brand new <see cref="SubtitleFileContent"/>, it assumes
    /// that means an insert, and trips over the row already sat in SubtitleFiles. Luckily, the owner can tell us
    /// whether that row exists without us reading it: if <see cref="CheckedSubtitle.UploadedAt"/> had a value when
    /// we loaded it, there's a file down there. Those get flipped to updates.
    /// </summary>
    private void MarkReplacedSubtitleFilesAsUpdates()
    {
        foreach (var entry in ChangeTracker.Entries<SubtitleFileContent>().ToList())
        {
            if (entry.State != EntityState.Added || entry.Entity.CheckedSubtitle is not { } owner)
                continue;

            var ownerEntry = Entry(owner);

            // a brand new CheckedSubtitle has nothing to collide with
            if (ownerEntry.State is EntityState.Added or EntityState.Detached)
                continue;

            // no file (or UploadedAt date, which is our proxy) on the owner when we loaded it means this really is a new row
            if (ownerEntry.Property(x => x.UploadedAt).OriginalValue is null)
                continue;

            // else we do a little trolling and modify the existing entry instead
            entry.State = EntityState.Modified;
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckedSubtitle>(builder =>
        {
            builder.HasKey(x => new { x.EpisodeCheckId, x.LanguageCode });

            builder.ToTable("CheckedSubtitles", table => table.HasCheckConstraint(
                "CK_CheckedSubtitles_FileMetadataIsConsistent",
                """("UploadedAt" IS NULL) = ("OriginalFileName" IS NULL)"""));

            // file contents live in their own table, so they're never pulled in unless I meant to
            builder.HasOne(x => x.Content)
                .WithOne(x => x.CheckedSubtitle)
                .HasForeignKey<SubtitleFileContent>(x => new { x.EpisodeCheckId, x.LanguageCode })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubtitleFileContent>(builder =>
        {
            builder.ToTable("SubtitleFiles");

            builder.HasKey(x => new { x.EpisodeCheckId, x.LanguageCode });
        });
    }
}