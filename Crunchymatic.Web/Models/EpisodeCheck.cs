using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crunchymatic.Web.Models;

public class EpisodeCheck
{
    [Key] public int Id { get; set; }

    public NodaTime.Instant EpisodeReleased { get; set; }

    [MinLength(1), MaxLength(16)] public required string Episode { get; set; }

    [MinLength(1), MaxLength(8)] public string AudioLanguage { get; set; } = "jpn";

    [MaxLength(32)] public required string CrunchyrollEpisodeId { get; set; }


    public NodaTime.Instant TimeSubtitlesGrabbed { get; set; }

    public ICollection<CheckedSubtitle> CheckedSubtitles { get; set; } = [];

    [ForeignKey(nameof(LinkedAnime))] public int AnimeId { get; set; }
    public Anime? LinkedAnime { get; set; }
}