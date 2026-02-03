using AssCS.IO;
using AssCS.Overrides.Blocks;
using AwesomeAssertions;
using Crunchymatic.Analyzers;

namespace Crunchymatic.Tests;

public static class CommentsAnalyzerTests
{
    [Fact]
    public static void CommentsAnalyzer_BlockComment_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.commentsFile);
        var document = parser.Parse(reader);
        
        // act
        var trait = CommentsAnalyzer.Analyze(document);
        
        // assert
        var subtitleEvent = trait.EventsWithComments[0];
        
        subtitleEvent.Event.ParseBlocks().Any(x => x.Type == BlockType.Comment).Should().BeTrue();
    }
    
    [Fact]
    public static void CommentsAnalyzer_CommentLine_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.commentsFile);
        var document = parser.Parse(reader);
        
        // act
        var trait = CommentsAnalyzer.Analyze(document);
        
        // assert
        var subtitleEvent = trait.EventsWithComments[1];

        subtitleEvent.Event.IsComment.Should().BeTrue();
    }
    
    [Fact]
    public static void CommentsAnalyzer_FileWithoutComments_DetectsCorrectly()
    {
        // arrange
        var parser = new AssParser();
        using var reader = new StringReader(AssFiles.directFullViaStylesFile);
        var document = parser.Parse(reader);
        
        // act
        var trait = CommentsAnalyzer.Analyze(document);
        
        // assert
        trait.EventsWithComments.Should().BeEmpty();
    }
}