namespace Crunchymatic.Web.Models;

public static class SupportedLanguages
{
    public static readonly Dictionary<string, string> LanguageCodeToEnglishName = new()
    {
        { "eng", "English" },
        { "spa-419", "Spanish/LA" },
        { "spa-ES", "Spanish" },
        { "por", "Portuguese" },
        { "fra", "French" },
        { "deu", "German" },
        { "ara", "Arabic" },
        { "ita", "Italian" },
        { "rus", "Russian" },
        
        { "zho", "Chinese" },
        { "zh-HK", "Chinese" },
        { "tha", "Thai" },
        { "may", "Malay" },
        { "vie", "Vietnamese" },
        { "ind", "Indonesian" }
    };
}