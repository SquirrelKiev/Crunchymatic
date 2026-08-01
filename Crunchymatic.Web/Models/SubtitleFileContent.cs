using System.ComponentModel.DataAnnotations;
using Crunchymatic.Web.Services;

namespace Crunchymatic.Web.Models;

public class SubtitleFileContent
{
    public int EpisodeCheckId { get; set; }

    /// <inheritdoc cref="CheckedSubtitle.LanguageCode"/>
    [MaxLength(8)]
    public required string LanguageCode { get; set; }

    /// <summary>
    /// Brotli-compressed subtitle file. Decompress with <see cref="SubtitleStorageService.DecompressAsync"/>.
    /// </summary>
    public required byte[] CompressedContent { get; set; }

    public CheckedSubtitle? CheckedSubtitle { get; set; }
}