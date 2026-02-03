using AssCS;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;

namespace Crunchymatic;

public class DocumentCommonAnalysis(Document document)
{
    private List<Event>? chronologicalEvents;
    private readonly Dictionary<string, bool> styleIsTypesetting = [];
    private bool? isOverlaps;
    
    private static readonly string[] DialogueFonts = ["trebuchet", "arial", "noto", "adobe arabic", "tahoma"];

    /// <summary>
    /// Returns the document's events, sorted by their start time.
    /// </summary>
    /// <returns></returns>
    public List<Event> GetChronologicalEvents()
    {
        if (chronologicalEvents != null)
        {
            return chronologicalEvents;
        }

        var events = document.EventManager.Events;
        events.Sort((x, y) => x.Start.CompareTo(y.Start));

        chronologicalEvents = events;

        return chronologicalEvents;
    }

    /// <summary>
    /// If the document has any overlapping events (events that start before another event finishes playing).
    /// </summary>
    /// <returns></returns>
    public bool IsOverlaps()
    {
        if (isOverlaps.HasValue)
        {
            return isOverlaps.Value;
        }

        isOverlaps = IsOverlaps(GetChronologicalEvents());
        return isOverlaps.Value;
    }

    public SignType EventIsSign(Event subtitleEvent)
    {
        if (styleIsTypesetting.TryGetValue(subtitleEvent.Style, out var isTypesettingStyle))
        {
            if (isTypesettingStyle)
            {
                return SignType.TypesetSign;
            }
        }
        else if (document.StyleManager.TryGet(subtitleEvent.Style, out var style))
        {
            if (DialogueFonts.All(potentialFont =>
                    !style.FontFamily.Contains(potentialFont, StringComparison.InvariantCultureIgnoreCase)))
            {
                styleIsTypesetting[subtitleEvent.Style] = true;
                return SignType.TypesetSign;
            }

            styleIsTypesetting[subtitleEvent.Style] = false;
        }

        foreach (var block in subtitleEvent.ParseBlocks())
        {
            if (block is not OverrideBlock overrideBlock) continue;

            foreach (var tag in overrideBlock.Tags)
            {
                if (IsTagTypesetting(tag))
                {
                    return SignType.TypesetSign;
                }
            }
        }
        
        if (subtitleEvent.Actor.Equals("sign", StringComparison.InvariantCultureIgnoreCase) ||
            subtitleEvent.Style.Contains("sign", StringComparison.InvariantCultureIgnoreCase) ||
            subtitleEvent.Style.Contains("TypePlaceholder", StringComparison.InvariantCultureIgnoreCase) || // ger
            subtitleEvent.Style.StartsWith("Cart_", StringComparison.InvariantCultureIgnoreCase) || // spa
            subtitleEvent.Actor.Equals("signs", StringComparison.InvariantCultureIgnoreCase) ||
            subtitleEvent.Actor.Contains("Надпись", StringComparison.InvariantCultureIgnoreCase) || // rus
            subtitleEvent.Text.All(static x => !char.IsLower(x)))
        {
            return SignType.Sign;
        }

        return SignType.IsntSign;
    }

    public enum SignType
    {
        IsntSign,
        Sign,
        TypesetSign
    }
    
    private static bool IsTagTypesetting(OverrideTag tag)
    {
        return tag is OverrideTag.A1 or OverrideTag.A2 or OverrideTag.A3 or OverrideTag.A4 or OverrideTag.Alpha or
            OverrideTag.Be or OverrideTag.Blur or OverrideTag.Bord or
            OverrideTag.C or OverrideTag.C1 or OverrideTag.C2 or OverrideTag.C3 or OverrideTag.C4 or OverrideTag.Clip or
            OverrideTag.FaX or OverrideTag.FaY or OverrideTag.Fn or OverrideTag.Fr or OverrideTag.FrX or OverrideTag.FrY
            or OverrideTag.FrZ or
            OverrideTag.FscX or OverrideTag.FscY or OverrideTag.IClip or OverrideTag.K or OverrideTag.Move or
            OverrideTag.Org or OverrideTag.P or OverrideTag.Pbo or OverrideTag.Shad or OverrideTag.T or
            OverrideTag.XBord or OverrideTag.XShad or OverrideTag.YBord or OverrideTag.YShad;
    }

    /// <param name="events">List of events. Expected to be sorted by start time.</param>
    private static bool IsOverlaps(List<Event> events)
    {
        var maxEndTime = Time.Zero;
        foreach (var subtitleEvent in events)
        {
            if (subtitleEvent.Start < maxEndTime)
            {
                return true;
            }

            if (subtitleEvent.End > maxEndTime)
            {
                maxEndTime = subtitleEvent.End;
            }
        }

        return false;
    }
}