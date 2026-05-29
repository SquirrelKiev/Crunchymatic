using System.ComponentModel.DataAnnotations;

namespace Crunchymatic.Web.Models;

public class Anime
{
    [Key] public int Id { get; set; }

    [MaxLength(256)] public required string EnglishTitle { get; set; }

    [MaxLength(128)] public string? SheetName { get; set; }

    [MaxLength(32)] public required string CrunchyrollSeasonId { get; set; }
    [MaxLength(32)] public required string CrunchyrollSeriesId { get; set; }

    public ICollection<EpisodeCheck> EpisodeChecks { get; set; } = [];
}