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
        HashSet<Event> positionedEvents = [];
        HashSet<Event> coloredEvents = [];
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
                case DocumentCommonAnalysis.SignType.ColoredSign:
                    signs.Add(subtitleEvent);
                    coloredEvents.Add(subtitleEvent);
                    break;
                case DocumentCommonAnalysis.SignType.PositionedSign:
                    signs.Add(subtitleEvent);
                    positionedEvents.Add(subtitleEvent);
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
                signs, typesetEvents, coloredEvents);
        }
        
        if(positionedEvents.Count > 0 || coloredEvents.Count > 0)
        {
            return new SubtitleTypesettingAnalyzerResult(SubtitleTypesettingAnalyzerResult.TypesettingStyle.LitePlus,
                signs, typesetEvents, coloredEvents);
        }

        return new SubtitleTypesettingAnalyzerResult(
            commonAnalysis.GetOverlaps().Count > 0
                ? SubtitleTypesettingAnalyzerResult.TypesettingStyle.Lite
                : SubtitleTypesettingAnalyzerResult.TypesettingStyle.NoOverlaps, signs, typesetEvents, coloredEvents);
    }
}

public record SubtitleTypesettingAnalyzerResult(
    SubtitleTypesettingAnalyzerResult.TypesettingStyle Style,
    HashSet<Event> Signs,
    HashSet<Event> SignsWithTypesetting,
    HashSet<Event> SignsWithColoring)
{
    public enum TypesettingStyle
    {
        Unchecked,
        
        /// <summary>
        /// No event overlaps, no style overrides or \pos.
        /// </summary>
        NoOverlaps,

        /// <summary>
        /// Is overlaps, typesetting is limited to \an.
        /// </summary>
        Lite,

        /// <summary>
        /// Actual styled typesetting. \fn, \blur, \c, etc. are all hallmarks of actual typesetting.
        /// </summary>
        Full,
        
        /// <summary>
        /// Has overlaps/TS with \an8 but very little vs other languages
        /// </summary>
        LiteMinus,
        
        /// <summary>
        /// Same as <see cref="Lite"/>, with \pos or \c.
        /// </summary>
        LitePlus,
        
        /// <summary>
        /// Actual typesetting but very little signs total compared to other languages, or very basic signs
        /// </summary>
        FullMinus,
        
        /// <summary>
        /// Same as <see cref="Full"/>, with frame-by-frame typesetting (fbf)
        /// </summary>
        FullPlus,
        
        /// <summary>
        /// No signs in the show to be able to tell, or the signs that are in the show have a reasonable excuse not to be typeset (e.g. already in english, is the show name).
        /// At the very least something with this isn't Netflix quality.
        /// </summary>
        Unknown,
    }
}