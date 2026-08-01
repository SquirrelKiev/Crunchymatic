using System.IO.Compression;
using Crunchymatic.Web.Models;
using NodaTime;

namespace Crunchymatic.Web.Services;

// TODO: IFormFile needs swapping for something more generic so automated uploader services can do stuff 
public class SubtitleStorageService(ILogger<SubtitleStorageService> logger, IClock clock)
{
    // biggest subtitle file I've seen on CR at present is just a bit over 3 MiB.
    public const int MaxFileSize = 8 * 1024 * 1024; // 8 MiB
    /// <remarks>CR supports 23 subtitle languages at the moment of writing this (2026-07-31).</remarks>
    public const int MaxBatchSize = 24;
    public const long MaxRequestSize = (long)MaxFileSize * MaxBatchSize + 1024 * 1024;

    public async Task<IReadOnlyList<SubtitleUploadResult>> StoreFilesAsync(
        IReadOnlyList<IFormFile> files,
        EpisodeCheck episodeCheck,
        CancellationToken token = default)
    {
        // feel like this shouldn't be scoped per file
        if (files.Count > MaxBatchSize)
        {
            return
            [
                ..files.Index().Select(x => new SubtitleUploadResult(x.Index, x.Item.FileName, null,
                    SubtitleUploadStatus.BatchTooLarge))
            ];
        }

        var results = new List<SubtitleUploadResult>();
        var languagesInBatch = new HashSet<string>();

        foreach (var (index, file) in files.Index())
        {
            var res = await StoreSingleFileAsync(index, file, episodeCheck, languagesInBatch, null, token);

            results.Add(res);
        }

        return results;
    }

    private async Task<SubtitleUploadResult> StoreSingleFileAsync(
        int index,
        IFormFile file,
        EpisodeCheck episodeCheck,
        HashSet<string> languagesInBatch,
        string? forcedLanguageCode = null,
        CancellationToken cancellationToken = default)
    {
        if (file.Length > MaxFileSize)
        {
            return new SubtitleUploadResult(index, file.FileName, null, SubtitleUploadStatus.FileTooLarge);
        }

        if (file.FileName.Length > CheckedSubtitle.MaxFileNameLength)
        {
            return new SubtitleUploadResult(index, file.FileName, null, SubtitleUploadStatus.FileNameTooLong);
        }

        if (forcedLanguageCode != null && !SupportedLanguages.LanguageCodeToEnglishName.ContainsKey(forcedLanguageCode))
        {
            return new SubtitleUploadResult(index, file.FileName, forcedLanguageCode, SubtitleUploadStatus.UnknownLanguage);
        }

        var languageCode = forcedLanguageCode;

        if (languageCode == null && !LanguageMatcher.TryMatch(file.FileName, out languageCode))
            return new SubtitleUploadResult(index, file.FileName, null, SubtitleUploadStatus.NoLanguageMatch);

        if (!languagesInBatch.Add(languageCode))
            return new SubtitleUploadResult(index, file.FileName, languageCode, SubtitleUploadStatus.DuplicateLanguage);
        
        var compressed = await CompressAsync(file, cancellationToken);
        
        var subtitleCheck = episodeCheck.CheckedSubtitles.FirstOrDefault(x => x.LanguageCode == languageCode);
        if (subtitleCheck == null)
        {
            subtitleCheck = new CheckedSubtitle
            {
                EpisodeCheckId = episodeCheck.Id,
                LanguageCode = languageCode
            };
            
            episodeCheck.CheckedSubtitles.Add(subtitleCheck);
        }

        // TODO: include a hash of the file. main intended use is for checking if a file has changed
        subtitleCheck.AttachFile(file.FileName, compressed, (int)Math.Min(file.Length, int.MaxValue), clock.GetCurrentInstant());

        return new SubtitleUploadResult(index, file.FileName, languageCode, SubtitleUploadStatus.Stored);
    }
    
    private static async Task<byte[]> CompressAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var source = file.OpenReadStream();
        
        using var destination = new MemoryStream();
        
        await using (var brotli = new BrotliStream(destination, CompressionLevel.Optimal, leaveOpen: true))
        {
            await source.CopyToAsync(brotli, cancellationToken);
        }

        return destination.ToArray();
    }
    
    public static async Task<byte[]> DecompressAsync(byte[] compressedContent, int originalLength,
        CancellationToken cancellationToken = default)
    {
        using var source = new MemoryStream(compressedContent);
        using var destination = new MemoryStream(originalLength);
        await using var brotli = new BrotliStream(source, CompressionMode.Decompress);

        await brotli.CopyToAsync(destination, cancellationToken);

        return destination.ToArray();
    }
}

public enum SubtitleUploadStatus
{
    Stored,
    NoLanguageMatch,
    FileTooLarge,
    DuplicateLanguage,
    BatchTooLarge,
    UnknownLanguage,
    FileNameTooLong
}

public record SubtitleUploadResult(int Index, string FileName, string? LanguageCode, SubtitleUploadStatus Status)
{
    public bool Succeeded => Status == SubtitleUploadStatus.Stored;

    public string DisplayFileName => string.IsNullOrWhiteSpace(FileName) ? $"file-{Index + 1}.ass" : FileName;

    public string Describe() => Status switch
    {
        SubtitleUploadStatus.Stored => $"{SupportedLanguages.LanguageCodeToEnglishName[LanguageCode!]} ({LanguageCode})",
        SubtitleUploadStatus.NoLanguageMatch => "couldn't figure out what language the subtitle file was based off the title",
        SubtitleUploadStatus.FileTooLarge => $"file was larger than the {MaxFileLengthInMebibytes}MiB limit",
        SubtitleUploadStatus.DuplicateLanguage =>
            $"another file in this upload was already for {LanguageCode}. Crunchyroll only supports one subtitle per language, and naturally we do too",
        SubtitleUploadStatus.BatchTooLarge => $"too many files. should be no more than {SubtitleStorageService.MaxBatchSize} files",
        SubtitleUploadStatus.UnknownLanguage => $"'{LanguageCode}' isn't a supported language",
        SubtitleUploadStatus.FileNameTooLong => $"The file name is too long. Must be under {CheckedSubtitle.MaxFileNameLength} characters",
        _ => Status.ToString()
    };

    private const int MaxFileLengthInMebibytes = SubtitleStorageService.MaxFileSize / (1024 * 1024);
}
