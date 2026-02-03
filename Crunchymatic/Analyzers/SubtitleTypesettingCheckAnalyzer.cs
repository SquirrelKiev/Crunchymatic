using AssCS;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;
using Document = AssCS.Document;

namespace Crunchymatic.Analyzers;

public static class SubtitleTypesettingAnalyzer
{
    public static SubtitleTypesettingAnalyzerResult Analyze(Document document, DocumentCommonAnalysis commonAnalysis)
    {
        var events = document.EventManager.Events;

        HashSet<Event> typesetEvents = [];
        HashSet<Event> signs = [];

        foreach (var subtitleEvent in events)
        {
            switch (commonAnalysis.EventIsSign(subtitleEvent))
            {
                default:
                case DocumentCommonAnalysis.SignType.IsntSign:
                    break;
                case DocumentCommonAnalysis.SignType.Sign:
                    signs.Add(subtitleEvent);
                    break;
                case DocumentCommonAnalysis.SignType.TypesetSign:
                    signs.Add(subtitleEvent);
                    typesetEvents.Add(subtitleEvent);
                    break;
            }
        }

        if (typesetEvents.Count > 0)
        {
            return new SubtitleTypesettingAnalyzerResult(SubtitleTypesettingAnalyzerResult.TypesettingStyle.Full,
                signs, typesetEvents);
        }

        // TODO: if theres a \pos tag, give Lite classification
        return new SubtitleTypesettingAnalyzerResult(
            commonAnalysis.IsOverlaps()
                ? SubtitleTypesettingAnalyzerResult.TypesettingStyle.Lite
                : SubtitleTypesettingAnalyzerResult.TypesettingStyle.None, signs, typesetEvents);
    }
}

public record SubtitleTypesettingAnalyzerResult(
    SubtitleTypesettingAnalyzerResult.TypesettingStyle Style,
    HashSet<Event> Signs,
    HashSet<Event> SignsWithTypesetting)
{
    public enum TypesettingStyle
    {
        /// <summary>
        /// No event overlaps, no style overrides or \pos.
        /// </summary>
        None,

        /// <summary>
        /// Is overlaps, typesetting is limited to \an and \pos.
        /// </summary>
        Lite,

        /// <summary>
        /// Actual styled typesetting. \fn, \blur, \c, etc. are all hallmarks of actual typesetting.
        /// </summary>
        Full,
    }
}