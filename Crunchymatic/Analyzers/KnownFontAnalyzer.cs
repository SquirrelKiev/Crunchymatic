using AssCS;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;

namespace Crunchymatic.Analyzers;

/// <summary>
/// Checks if any styles or overrides have fonts we've not heard of before.
/// </summary>
public static class KnownFontAnalyzer
{
    /// <summary>
    /// Every font Crunchyroll's video player supports.
    /// </summary>
    private static readonly string[] KnownFonts =
    [
        "adobe arabic",
        "andale mono",
        "arial",
        "arial black",
        "arial bold",
        "arial bold italic",
        "arial italic",
        "arial unicode ms",
        "comic sans ms",
        "comic sans ms bold",
        "courier new",
        "courier new bold",
        "courier new bold italic",
        "courier new italic",
        "dejavu lgc sans mono",
        "dejavu lgc sans mono bold",
        "dejavu lgc sans mono bold oblique",
        "dejavu lgc sans mono oblique",
        "dejavu sans",
        "dejavu sans bold",
        "dejavu sans bold oblique",
        "dejavu sans condensed",
        "dejavu sans condensed bold",
        "dejavu sans condensed bold oblique",
        "dejavu sans condensed oblique",
        "dejavu sans extralight",
        "dejavu sans mono",
        "dejavu sans mono bold",
        "dejavu sans mono bold oblique",
        "dejavu sans mono oblique",
        "dejavu sans oblique",
        "gautami",
        "georgia",
        "georgia bold",
        "georgia bold italic",
        "georgia italic",
        "impact",
        "mangal",
        "meera inimai",
        "noto sans tamil",
        "noto sans telugu",
        "noto sans thai",
        "rubik",
        "rubik black",
        "rubik black italic",
        "rubik bold",
        "rubik bold italic",
        "rubik italic",
        "rubik light",
        "rubik light italic",
        "rubik medium",
        "rubik medium italic",
        "tahoma",
        "times new roman",
        "times new roman bold",
        "times new roman bold italic",
        "times new roman italic",
        "trebuchet ms",
        "trebuchet ms bold",
        "trebuchet ms bold italic",
        "trebuchet ms italic",
        "verdana",
        "verdana bold",
        "verdana bold italic",
        "verdana italic",
        "vrinda",
        "vrinda bold",
        "webdings",
    ];

    public static KnownFontAnalyzerResult Analyze(Document document)
    {
        List<Style> stylesWithUnknownFonts = [];
        List<KnownFontAnalyzerResult.EventWithFontInfo> eventsWithUnknownFonts = [];
        HashSet<string> seenFonts = new HashSet<string>();

        foreach (var style in document.StyleManager.Styles)
        {
            seenFonts.Add(style.FontFamily);
            
            if (KnownFonts.Any(font => font.Equals(style.FontFamily, StringComparison.InvariantCultureIgnoreCase)))
                continue;

            stylesWithUnknownFonts.Add(style);
        }

        foreach (var subtitleEvent in document.EventManager.Events)
        {
            foreach (var block in subtitleEvent.ParseBlocks())
            {
                if (block is not OverrideBlock overrideBlock) continue;

                foreach (var tag in overrideBlock.Tags)
                {
                    if (tag is not OverrideTag.Fn fontTag || fontTag.Value == null)
                        continue;
                    
                    seenFonts.Add(fontTag.Value);

                    if (KnownFonts.Any(font => font.Equals(fontTag.Value, StringComparison.InvariantCultureIgnoreCase)))
                        continue;

                    var info = new KnownFontAnalyzerResult.EventWithFontInfo(subtitleEvent, fontTag.Value);
                    eventsWithUnknownFonts.Add(info);
                }
            }
        }

        return new KnownFontAnalyzerResult(stylesWithUnknownFonts, eventsWithUnknownFonts, seenFonts);
    }
}

public record KnownFontAnalyzerResult(
    List<Style> StylesWithUnknownFonts,
    List<KnownFontAnalyzerResult.EventWithFontInfo> EventsWithUnknownFonts,
    HashSet<string> AllSeenFontNames)
{
    public record EventWithFontInfo(Event Event, string Font);
}