using System.Diagnostics.CodeAnalysis;
using Crunchymatic.Web.Models;

namespace Crunchymatic.Web.Services;

// there's probably some fuzzy names that this would let through but assuming relatively standard naming this should do
public static class LanguageMatcher
{
    private static readonly Dictionary<string, string> AliasToLanguageCode =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "eng", "en-US" },
            { "spa-419", "es-419" },
            { "spa-ES", "es-ES" },
            { "por", "pt-BR" },
            { "fra", "fr-FR" },
            { "deu", "de-DE" },
            { "ara", "ar-SA" },
            { "ita", "it-IT" },
            { "rus", "ru-RU" },
            { "zho", "zh-CN" },
            { "tha", "th-TH" },
            { "may", "ms-MY" },
            { "vie", "vi-VN" },
            { "ind", "id-ID" },
            { "pol", "pl-PL" },
        };

    /// <summary>
    /// Translation table from a case-insensitive language code to the case-sensitive version.
    /// </summary>
    private static readonly Dictionary<string, string> CanonicalLanguageCodes =
        SupportedLanguages.LanguageCodeToEnglishName.Keys.ToDictionary(x => x, x => x, StringComparer.OrdinalIgnoreCase);

    private static readonly char[] Delimiters =
        ['.', '_', '-', '[', ']', '(', ')', ' ', '【', '】'];
    
    public static bool TryMatch(string fileName, [NotNullWhen(true)] out string? languageCode)
    {
        languageCode = null;

        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var stem = Path.GetFileNameWithoutExtension(fileName);
        var tokens = stem.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);

        // - is a delimiter, so e.g. en-US becomes two tokens, en and US. recombines adjacent tokens with a dash so
        // they can be matched correctly.
        var pairs = tokens
            .Zip(tokens.Skip(1), (first, second) => $"{first}-{second}")
            .Select((candidate, index) => (Candidate: candidate, Index: index));
        
        // the language is usually at the end of the file name
        var candidates = tokens.Select((candidate, index) => (Candidate: candidate, Index: index))
            .Concat(pairs)
            .OrderByDescending(x => x.Index)
            .ThenByDescending(x => x.Candidate.Length)
            .Select(x => x.Candidate)
            .ToList();

        foreach (var candidate in candidates)
        {
            if (CanonicalLanguageCodes.TryGetValue(candidate, out languageCode))
                return true;
            
            if(AliasToLanguageCode.TryGetValue(candidate, out languageCode))
                return true;
        }

        return false;
    }
}
