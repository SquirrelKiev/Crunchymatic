using AssCS.IO;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

public static class PipelineAnalyzerTests
{
    [Fact]
    public static void SubtitlePipelineAnalyzer_ClosedCaptionConverter_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.cccFile);
        var document = parser.Parse(reader);
        var commonAnalysis = new DocumentCommonAnalysis(document);

        // act
        var result = PipelineAnalyzer.Analyze(document, commonAnalysis, AssFiles.cccFile);

        // assert
        result.Pipeline.Should().Be(SubtitlePipeline.ClosedCaptionConverter);
    }

    [Fact]
    public static void SubtitlePipelineAnalyzer_Direct_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.directFullFile);
        var document = parser.Parse(reader);
        var commonAnalysis = new DocumentCommonAnalysis(document);

        // act
        var result = PipelineAnalyzer.Analyze(document, commonAnalysis, AssFiles.directFullFile);

        // assert
        result.Pipeline.Should().Be(SubtitlePipeline.Direct);
    }

    [Fact]
    public static void SubtitlePipelineAnalyzer_Old_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.oldLiteFile);
        var document = parser.Parse(reader);
        var commonAnalysis = new DocumentCommonAnalysis(document);

        // act
        var result = PipelineAnalyzer.Analyze(document, commonAnalysis, AssFiles.oldLiteFile);

        // assert
        result.Pipeline.Should().Be(SubtitlePipeline.Old);
    }
}