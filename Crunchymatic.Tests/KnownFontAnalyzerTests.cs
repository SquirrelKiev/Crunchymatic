using AssCS.IO;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

public class KnownFontAnalyzerTests
{
    [Fact]
    public void KnownFontAnalyzer_FileWithKnownFonts_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.directFullFile);
        var document = parser.Parse(reader);
        
        // act
        var trait =  KnownFontAnalyzer.Analyze(document);
        
        // assert
        trait.StylesWithUnknownFonts.Should().BeEmpty();
        trait.EventsWithUnknownFonts.Should().BeEmpty();
    }
    
    [Fact]
    public void KnownFontAnalyzer_FileWithUnknownFontsInStyles_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.unknownFontInStyleFile);
        var document = parser.Parse(reader);
        
        // act
        var trait =  KnownFontAnalyzer.Analyze(document);
        
        // assert
        trait.StylesWithUnknownFonts.Should().HaveCount(1);
        var style = trait.StylesWithUnknownFonts[0];
        style.FontFamily.Should().Be("Gandhi Sans");
    }
    
    [Fact]
    public void KnownFontAnalyzer_FileWithUnknownFontsInTags_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.unknownFontInTagFile);
        var document = parser.Parse(reader);
        
        // act
        var trait =  KnownFontAnalyzer.Analyze(document);
        
        // assert
        trait.EventsWithUnknownFonts.Should().HaveCount(1);
        var subtitleEvent = trait.EventsWithUnknownFonts[0];
        subtitleEvent.Font.Should().Be("Gandhi Sans");
    }
}