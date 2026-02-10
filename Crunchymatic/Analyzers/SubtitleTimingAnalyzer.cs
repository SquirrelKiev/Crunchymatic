using AssCS;

namespace Crunchymatic.Analyzers;

public static class SubtitleTimingAnalyzer
{
    // in milliseconds
    private const int Overlap = 50;
    private const int AdjacentGap = 300;
    
    public static SubtitleTimingAnalyzerResult Analyze(DocumentCommonAnalysis commonAnalysis)
    {
        var chronologicalEvents = commonAnalysis.GetChronologicalEvents();

        var detectedGaps = new List<LinkedEvents>();

        // https://github.com/TypesettingTools/Aegisub/blob/41a37d2/src/dialog_timing_processor.cpp#L406
        for (var i = 1; i < chronologicalEvents.Count; ++i)
        {
            Event previousEvent = chronologicalEvents[i - 1];
            Event currentEvent = chronologicalEvents[i];

            var distance = currentEvent.Start.TotalMilliseconds - previousEvent.End.TotalMilliseconds;
            if ((distance < 0 && -distance <= Overlap) || distance is > 0 and <= AdjacentGap)
            {
                detectedGaps.Add(new LinkedEvents(previousEvent, currentEvent));
            }
        }

        return new SubtitleTimingAnalyzerResult(detectedGaps);
    }
}

public record SubtitleTimingAnalyzerResult(List<LinkedEvents> ChronologicalEventsWithGaps)
{
}

public record LinkedEvents(AssCS.Event Start, AssCS.Event End);