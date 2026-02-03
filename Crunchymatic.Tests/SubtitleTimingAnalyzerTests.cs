using AssCS.IO;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

public static class SubtitleTimingAnalyzerTests
{
    [Theory]
    [InlineData(AssFiles.cccFile, 172)]
    [InlineData(AssFiles.directFullFile, 15)]
    public static void SubtitleTimingAnalyzer_Timing_DetectsCorrectly(string assFile, int frameGapEvents)
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(assFile);
        var document = parser.Parse(reader);
        var commonAnalysis = new DocumentCommonAnalysis(document);

        // act
        var trait = SubtitleTimingAnalyzer.Analyze(commonAnalysis);
        
        // assert
        trait.ChronologicalEventsWithGaps.Should().HaveCount(frameGapEvents);
    }
}