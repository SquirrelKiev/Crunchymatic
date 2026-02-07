using AssCS.IO;
using CommandLine;
using Crunchymatic.Analyzers;
using Humanizer;
using Spectre.Console;

namespace Crunchymatic.Console;

public static class Entrypoint
{
    public record Options
    {
        [Value(0, Required = true, MetaName = "subtitle-file-path",
            HelpText = "The path to the subtitle file to analyze.")]
        public required string SubtitleFilePath { get; set; }
    }

    public static void Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        Parser.Default.ParseArguments<Options>(args).WithParsed(x =>
        {
            var subtitleFile = File.ReadAllText(x.SubtitleFilePath);

            // why loadDefaults isn't false by default is an enigma, 9volt pls
            var parser = new AssParser(false);
            using var reader = new StringReader(subtitleFile);
            var document = parser.Parse(reader);
            var commonAnalysis = new DocumentCommonAnalysis(document);

            var commentsRes = CommentsAnalyzer.Analyze(document);
            var knownFontRes = KnownFontAnalyzer.Analyze(document);
            var metadataRes = MetadataAnalyzer.Analyze(document);
            var pipelineRes = PipelineAnalyzer.Analyze(document, commonAnalysis, subtitleFile);
            var timingRes = SubtitleTimingAnalyzer.Analyze(commonAnalysis);
            var typesettingRes = SubtitleTypesettingAnalyzer.Analyze(document, commonAnalysis);

            switch (pipelineRes.Pipeline)
            {
                case SubtitlePipeline.Old:
                    AnsiConsole.MarkupLine("[blue]ⓘ Pipeline: Old[/]");
                    break;
                case SubtitlePipeline.ClosedCaptionConverter:
                    AnsiConsole.MarkupLine("[red]ⓘ Pipeline: CCC[/]");
                    break;
                case SubtitlePipeline.Direct:
                    AnsiConsole.MarkupLine("[blue]ⓘ Pipeline: Direct[/]");
                    AnsiConsole.MarkupLineInterpolated($"[gray]{pipelineRes.GeneratedByComment}[/]");
                    break;
            }

            switch (typesettingRes.Style)
            {
                case SubtitleTypesettingAnalyzerResult.TypesettingStyle.None:
                    AnsiConsole.Markup("[red]⚠ Sign Style: None[/]");
                    break;
                case SubtitleTypesettingAnalyzerResult.TypesettingStyle.Lite:
                    AnsiConsole.Markup("[yellow]⚠ Sign Style: Lite[/]");
                    break;
                case SubtitleTypesettingAnalyzerResult.TypesettingStyle.Full:
                    AnsiConsole.Markup("[green]✓ Sign Style: Full[/]");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AnsiConsole.MarkupLineInterpolated(
                $" [gray]{typesettingRes.Signs.Count} signs, {typesettingRes.SignsWithTypesetting.Count} of which had typesetting[/]");

            AnsiConsole.MarkupLineInterpolated(
                $"[blue]ⓘ {timingRes.ChronologicalEventsWithGaps.Count}/{document.EventManager.Events.Count} ({(double)timingRes.ChronologicalEventsWithGaps.Count / document.EventManager.Events.Count:P}) events with small timing gaps[/]");

            if (metadataRes is { layoutResIsMissing: true, playResIs360p: true, ycbcrMatrixIsUnmarkedOr609: true })
            {
                AnsiConsole.MarkupLine("[green]✓ Metadata is all as expected[/]");
            }
            else
            {
                List<string> metadataIssues = [];
                if (!metadataRes.layoutResIsMissing)
                {
                    var layoutX = document.ScriptInfoManager.Get("LayoutResX");
                    var layoutY = document.ScriptInfoManager.Get("LayoutResY");
                    metadataIssues.Add($"LayoutRes is present and set to {layoutX}x{layoutY}");
                }

                if (!metadataRes.playResIs360p)
                {
                    var playX = document.ScriptInfoManager.Get("PlayResX");
                    var playY = document.ScriptInfoManager.Get("PlayResY");
                    metadataIssues.Add($"PlayRes is set to {playX}x{playY}");
                }

                if (!metadataRes.ycbcrMatrixIsUnmarkedOr609)
                {
                    var ycbcrMatrix = document.ScriptInfoManager.Get("YCbCr Matrix");
                    metadataIssues.Add($"YCbCr Matrix is set to {ycbcrMatrix}");
                }

                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow]⚠ Metadata is different from usual, {metadataIssues.Humanize()}[/]");
            }

            if (knownFontRes.EventsWithUnknownFonts.Count == 0 && knownFontRes.StylesWithUnknownFonts.Count == 0)
            {
                AnsiConsole.MarkupLineInterpolated($"[green]✓ Fonts are all valid[/] [gray]{knownFontRes.AllSeenFontNames.Humanize()}[/]");
            }

            AnsiConsole.MarkupLineInterpolated($"[blue]ⓘ Found {commentsRes.EventsWithComments.Count} Comments[/]");
        });
    }
}