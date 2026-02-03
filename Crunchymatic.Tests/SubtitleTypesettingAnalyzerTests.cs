using AssCS.IO;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

// this should check that the sign lists are as expected really
public class SubtitleTypesettingAnalyzerTests
{
    [Theory]
    [InlineData(AssFiles.directFullFile)]
    [InlineData(AssFiles.directFullViaStylesFile)]
    public void SubtitleTypesettingAnalyzer_Full_DetectsCorrectly(string data)
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(data);
        var document = parser.Parse(reader);
        
        var commonAnalysis = new DocumentCommonAnalysis(document);
        
        // act
        var trait = SubtitleTypesettingAnalyzer.Analyze(document, commonAnalysis);
        
        // assert
        trait.Style.Should().Be(SubtitleTypesettingAnalyzerResult.TypesettingStyle.Full);
    }
    
    [Fact]
    public void SubtitleTypesettingAnalyzer_Lite_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.oldLiteFile);
        var document = parser.Parse(reader);
        
        var commonAnalysis = new DocumentCommonAnalysis(document);
        
        // act
        var trait = SubtitleTypesettingAnalyzer.Analyze(document, commonAnalysis);
        
        // assert
        trait.Style.Should().Be(SubtitleTypesettingAnalyzerResult.TypesettingStyle.Lite);
    }
    
    [Fact]
    public void SubtitleTypesettingAnalyzer_None_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.cccFile);
        var document = parser.Parse(reader);
        
        var commonAnalysis = new DocumentCommonAnalysis(document);
        
        // act
        var trait = SubtitleTypesettingAnalyzer.Analyze(document, commonAnalysis);
        
        // assert
        trait.Style.Should().Be(SubtitleTypesettingAnalyzerResult.TypesettingStyle.None);
    }
}