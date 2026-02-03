using AssCS.IO;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

public static class MetadataAnalyzerTests
{
    [Fact]
    public static void MetadataAnalyzer_ExpectedFile_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.directFullFile);
        var document = parser.Parse(reader);
        
        // act
        var res = MetadataAnalyzer.Analyze(document);
        
        // assert
        res.layoutResIsMissing.Should().BeTrue();
        res.playResIs360p.Should().BeTrue();
        res.ycbcrMatrixIsUnmarkedOr609.Should().BeTrue();
    }
    
    [Fact]
    public static void MetadataAnalyzer_UnexpectedFile_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.nonstandardCrunchyMetadataFile);
        var document = parser.Parse(reader);
        
        // act
        var res = MetadataAnalyzer.Analyze(document);
        
        // assert
        res.layoutResIsMissing.Should().BeFalse();
        res.playResIs360p.Should().BeFalse();
        res.ycbcrMatrixIsUnmarkedOr609.Should().BeFalse();
    }
}