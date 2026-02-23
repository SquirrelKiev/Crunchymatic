using Microsoft.EntityFrameworkCore;

namespace Crunchymatic.Web.Models;

public class CrunchymaticContext(DbContextOptions<CrunchymaticContext> contextOptions) : DbContext(contextOptions)
{
    public DbSet<Anime> Anime { get; set; }
    public DbSet<EpisodeCheck> EpisodeChecks { get; set; }
    public DbSet<CheckedSubtitle> CheckedSubtitles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CheckedSubtitle>()
            .HasKey(x => new { x.EpisodeCheckId, x.LanguageCode });
    }
}