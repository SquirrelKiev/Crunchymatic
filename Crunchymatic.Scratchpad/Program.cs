using System.Text;
using AssCS.IO;
using AssCS.Overrides;
using AssCS.Overrides.Blocks;
using Crunchymatic;
using Crunchymatic.Analyzers;
using Crunchymatic.Tests;

if (args.Length != 1)
{
    Console.WriteLine("path of folder to search needs to be specified as an argument");
    return;
}

string baseDir = args[0];

var parser = new AssParser();
using var reader = new StringReader(AssFiles.directFullFile);
var document = parser.Parse(reader);
var commonAnalysis = new DocumentCommonAnalysis(document);

var trait = SubtitleTimingAnalyzer.Analyze(commonAnalysis);

foreach (var frameGapEvent in trait.ChronologicalEventsWithGaps)
{
    frameGapEvent.End.Text += "CHANGED";
}

var writer = new AssWriter(document, new ConsumerInfo("Trolling", "", "https://example.com"));
var stringWriter = new StringWriter();
writer.Write(stringWriter);
await File.WriteAllTextAsync("F:\\thing.ass", stringWriter.ToString());

// var parser = new AssParser();
//
// var allFontsSeen = new Dictionary<string, int>();
// foreach (var path in Directory.EnumerateFiles(baseDir, "*.*", SearchOption.AllDirectories))
// {
//     try
//     {
//         await using var fileStream = File.OpenRead(path);
//         using var data = new StreamReader(fileStream);
//
//         var document = parser.Parse(data);
//
//         foreach (var style in document.StyleManager.Styles)
//         {
//             var font = style.FontFamily.ToLowerInvariant();
//
//             allFontsSeen.TryAdd(font, 0);
//             
//             allFontsSeen[font]++;
//         }
//
//         foreach (var subtitleEvent in document.EventManager.Events)
//         {
//             foreach (var block in subtitleEvent.ParseBlocks())
//             {
//                 if (block is not OverrideBlock overrideBlock) continue;
//
//                 foreach (var tag in overrideBlock.Tags)
//                 {
//                     if (tag is not OverrideTag.Fn fontTag || fontTag.Value == null)
//                         continue;
//
//                     // var info = new KnownFontAnalyzerResult.EventWithFontInfo(subtitleEvent, fontTag.Value);
//                     var font = fontTag.Value.ToLowerInvariant();
//                     
//                     allFontsSeen.TryAdd(font, 0);
//             
//                     allFontsSeen[font]++;
//                 }
//             }
//         }
//     }
//     catch (Exception e)
//     {
//         Console.WriteLine($"{path}, {e}");
//     }
// }
//
// var sb2 = new StringBuilder();
// foreach (var font in allFontsSeen.OrderByDescending(f => f.Value))
// {
//     sb2.Append(font.Key);
//     sb2.Append(" = ");
//     sb2.Append(font.Value);
//     sb2.AppendLine();
// }
//
// Console.WriteLine($"fonts({allFontsSeen.Count}) = {sb2}");