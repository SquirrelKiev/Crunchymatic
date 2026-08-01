using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crunchymatic.Analyzers;
using NodaTime;

namespace Crunchymatic.Web.Models;

public class CheckedSubtitle
{
    // TODO: figure out what to do with CCs - separate model?

    /// <remarks>in BCP-47.</remarks>
    [MaxLength(8)]
    public required string LanguageCode { get; set; }

    public SubtitlePipeline SubtitlePipeline { get; set; } = SubtitlePipeline.Unchecked;

    public SubtitleTypesettingAnalyzerResult.TypesettingStyle TypesettingStyle { get; set; } =
        SubtitleTypesettingAnalyzerResult.TypesettingStyle.Unchecked;

    public const int MaxFileNameLength = 260;
    
    /// <summary>
    /// Original file name of the subtitle file that was uploaded. Null if no file.
    /// </summary>
    [MaxLength(MaxFileNameLength)]
    public string? OriginalFileName { get; private set; }

    /// <summary>
    /// Size of the file before compression, in bytes. Zero when nothing has been uploaded yet.
    /// </summary>
    public int OriginalLength { get; private set; }

    /// <summary>
    /// When the file was uploaded, or null if there isn't one.
    /// </summary>
    public Instant? UploadedAt { get; private set; }

    /// <summary>
    /// Whether a file has been uploaded for this subtitle.
    /// </summary>
    [NotMapped]
    public bool HasFile => UploadedAt is not null;

    /// <summary>
    /// The subtitle file itself. Null unless pulled in with an explicit <c>Include</c>. If you want to check if
    /// a file exists, use <see cref="HasFile"/>.
    /// </summary>
    public SubtitleFileContent? Content { get; private set; }

    [ForeignKey(nameof(EpisodeCheck))]
    public int EpisodeCheckId { get; set; }
    public EpisodeCheck? EpisodeCheck { get; set; }

    /// <summary>
    /// Attaches an uploaded file, replacing any existing one.
    /// </summary>
    public void AttachFile(string originalFileName, byte[] compressedContent, int originalLength, Instant uploadedAt)
    {
        OriginalFileName = originalFileName;
        OriginalLength = originalLength;
        UploadedAt = uploadedAt;
        Content = new SubtitleFileContent
        {
            EpisodeCheckId = EpisodeCheckId,
            LanguageCode = LanguageCode,
            CompressedContent = compressedContent
        };
    }
}