using AssCS;
using AssCS.Overrides.Blocks;

namespace Crunchymatic.Analyzers;

public static class CommentsAnalyzer
{
    public static CommentsTrait Analyze(Document document)
    {
        var events = document.EventManager.Events;
        var eventsWithComments = new List<CommentsTrait.CommentEvent>();
        
        foreach (var subtitleEvent in events)
        {
            var blocks = subtitleEvent.ParseBlocks();

            if (subtitleEvent.IsComment)
            {
                eventsWithComments.Add(new CommentsTrait.CommentEvent(subtitleEvent.Text, subtitleEvent));
                continue;
            }

            if (!string.IsNullOrEmpty(subtitleEvent.Effect))
            {
                eventsWithComments.Add(new CommentsTrait.CommentEvent(subtitleEvent.Effect, subtitleEvent));
            }

            foreach (var block in blocks)
            {
                if (block is CommentBlock)
                {
                    eventsWithComments.Add(new CommentsTrait.CommentEvent(block.Text, subtitleEvent));
                }
            }
        }
        
        return new CommentsTrait(eventsWithComments);
    }
}

public record CommentsTrait(List<CommentsTrait.CommentEvent> EventsWithComments)
{
    public record CommentEvent(string Comment, Event Event);
}