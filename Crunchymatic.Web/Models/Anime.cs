using System.ComponentModel.DataAnnotations;

namespace Crunchymatic.Web.Models;

public record Anime
{
    [Key]
    public int Id { get; set; }
    
    public required string EnglishTitle { get; set; }
    
    public string? SheetName { get; set; }
    
    public required string CrunchyrollSeasonId { get; set; }
    public required string CrunchyrollSeriesId { get; set; }
    
    public ICollection<EpisodeCheck> EpisodeChecks { get; set; } = [];
}
