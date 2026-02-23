using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crunchymatic.Web.Models;

public class EpisodeCheck
{
    [Key]
    public int Id { get; set; }
    
    public NodaTime.Instant EpisodeReleased { get; set; }

    public required int EpisodeNumber { get; set; }

    [MaxLength(8)]
    public string AudioLanguage { get; set; } = "jpn";
    
    
    public NodaTime.Instant TimeSubtitlesGrabbed { get; set; }
    
    public string? SubtitleArchivePath { get; set; }
    
    public ICollection<CheckedSubtitle> CheckedSubtitles { get; set; } = [];
    
    [ForeignKey(nameof(LinkedAnime))]
    public int AnimeId { get; set; }
    public Anime? LinkedAnime { get; set; }
}