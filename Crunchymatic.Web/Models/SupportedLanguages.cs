namespace Crunchymatic.Web.Models;

public static class SupportedLanguages
{
    // see https://static.crunchyroll.com/config/i18n/v3/timed_text_languages.json
    public static readonly Dictionary<string, string> LanguageCodeToEnglishName = new()
    {
        { "en-US", "English" },
        { "es-419", "Spanish/LA" },
        { "es-ES", "Spanish" },
        { "pt-BR", "Portuguese" },
        { "fr-FR", "French" },
        { "de-DE", "German" },
        { "ar-SA", "Arabic" },
        { "it-IT", "Italian" },
        { "ru-RU", "Russian" },
        
        { "zh-CN", "Chinese (Simplified)" },
        { "zh-HK", "Chinese (Traditional)" },
        { "th-TH", "Thai" },
        { "ms-MY", "Malay" },
        { "vi-VN", "Vietnamese" },
        { "id-ID", "Indonesian" },
        { "pl-PL", "Polish" },
    };
}