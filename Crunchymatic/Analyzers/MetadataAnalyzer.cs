using AssCS;

namespace Crunchymatic.Analyzers;

public static class MetadataAnalyzer
{
    public static MetadataAnalyzerResult Analyze(Document document)
    {
        var playResIs360p = document.ScriptInfoManager.Get("PlayResX") == "640" && document.ScriptInfoManager.Get("PlayResY") == "360";
        
        var ycbcrMatrix = document.ScriptInfoManager.Get("YCbCr Matrix");
        var ycbcrMatrixIsUnmarkedOr609 = ycbcrMatrix is null or "TV.601";
        
        var layoutResIsMissing = document.ScriptInfoManager.Get("LayoutResX") is null && document.ScriptInfoManager.Get("LayoutResY") is null;
        
        return new MetadataAnalyzerResult(playResIs360p, ycbcrMatrixIsUnmarkedOr609, layoutResIsMissing);
    }
}

public record MetadataAnalyzerResult(bool playResIs360p, bool ycbcrMatrixIsUnmarkedOr609, bool layoutResIsMissing);