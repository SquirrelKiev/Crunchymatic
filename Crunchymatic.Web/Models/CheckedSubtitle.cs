using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crunchymatic.Web.Models;

public class CheckedSubtitle
{
    // TODO: figure out what to do with CCs - separate model?
    
    /// <remarks>in ISO 639-2.</remarks>
    [MaxLength(8)]
    public required string LanguageCode { get; set; }

    public Analyzers.SubtitlePipeline SubtitlePipeline { get; set; }
    
    public Analyzers.SubtitleTypesettingAnalyzerResult.TypesettingStyle TypesettingStyle { get; set; }
    
    [ForeignKey(nameof(EpisodeCheck))]
    public int EpisodeCheckId { get; set; }
    public EpisodeCheck? EpisodeCheck { get; set; }
}