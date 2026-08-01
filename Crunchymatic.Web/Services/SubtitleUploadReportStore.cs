using Microsoft.Extensions.Caching.Memory;

namespace Crunchymatic.Web.Services;

/// <summary>
/// Carries an upload report across a redirect, since the page that performs an upload isn't always the page that
/// reports on it.
/// </summary>
public sealed class SubtitleUploadReportStore(IMemoryCache cache)
{
    /// <remarks>Only ever spans a single redirect, so this just needs to outlive the round trip.</remarks>
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    public Guid Stash(IReadOnlyList<SubtitleUploadResult> results)
    {
        var key = Guid.NewGuid();

        cache.Set(CacheKey(key), results, Lifetime);

        return key;
    }

    /// <summary>
    /// Reads a report and drops it, so refreshing the page it was reported on doesn't show it a second time.
    /// </summary>
    /// <returns>The report, or an empty one if it expired or never existed.</returns>
    public IReadOnlyList<SubtitleUploadResult> Take(Guid key)
    {
        if (!cache.TryGetValue(CacheKey(key), out IReadOnlyList<SubtitleUploadResult>? results) || results == null)
        {
            return [];
        }

        cache.Remove(CacheKey(key));

        return results;
    }

    private static string CacheKey(Guid key) => $"subtitle-upload-report:{key}";
}
