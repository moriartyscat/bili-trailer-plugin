using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.BiliTrailers.Configuration;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.BiliTrailers.Providers;

/// <summary>
/// Metadata provider that attaches B绔?trailers to movies.
/// Runs as a remote metadata provider 鈥?Jellyfin calls it during library scan / refresh.
/// </summary>
public class BiliTrailerMetadataProvider : IRemoteMetadataProvider<Movie, MovieInfo>
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<BiliTrailerMetadataProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiliTrailerMetadataProvider"/> class.
    /// </summary>
    public BiliTrailerMetadataProvider(
        IHttpClientFactory httpFactory,
        ILogger<BiliTrailerMetadataProvider> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "B绔?Trailers";

    /// <inheritdoc />
    public async Task<MetadataResult<Movie>> GetMetadata(
        MovieInfo info,
        CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Movie> { HasMetadata = false };

        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.AutoMatch)
            return result;

        // Build search keyword from movie metadata
        var movieName = info.Name;
        var year = info.Year ?? 0;
        var keyword = year > 0 ? $"{movieName} {year}" : movieName;

        _logger.LogInformation(
            "B绔?Trailers: searching for '{Keyword}'", keyword);

        var trailers = await FetchBiliTrailers(config.ServiceBaseUrl, keyword, cancellationToken);
        if (trailers is null || trailers.Count == 0)
        {
            _logger.LogDebug("No B绔?trailers found for '{Keyword}'", keyword);
            return result;
        }

        // Limit count
        var max = Math.Max(1, config.MaxTrailersPerMovie);
        if (trailers.Count > max)
            trailers = trailers.GetRange(0, max);

        var movie = new Movie();
        foreach (var trailer in trailers)
        {
            movie.Trailers.Add(trailer);
        }

        result.Item = movie;
        result.HasMetadata = true;

        _logger.LogInformation(
            "Attached {Count} B绔?trailers to '{Keyword}'", trailers.Count, keyword);

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(
        MovieInfo searchInfo,
        CancellationToken cancellationToken)
    {
        // Not providing alternative search results 鈥?we only add trailers.
        return await Task.FromResult(Array.Empty<RemoteSearchResult>());
    }

    public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseInfo
        {
            StatusCode = System.Net.HttpStatusCode.NotFound
        });
    }

    (
        string baseUrl,
        string keyword,
        CancellationToken ct)
    {
        try
        {
            var client = _httpFactory.CreateClient("BiliTrailers");
            client.Timeout = TimeSpan.FromSeconds(10);

            var encoded = Uri.EscapeDataString(keyword);
            var url = $"{baseUrl.TrimEnd('/')}/trailer/search/{encoded}";

            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var results = doc.RootElement.GetProperty("results");

            var urls = new List<string>();
            foreach (var item in results.EnumerateArray())
            {
                var bvid = item.GetProperty("bvid").GetString();
                if (!string.IsNullOrWhiteSpace(bvid))
                {
                    urls.Add($"https://www.bilibili.com/video/{bvid}");
                }
            }

            return urls;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to fetch B绔?trailers for '{Keyword}' from {BaseUrl}",
                keyword, baseUrl);
            return null;
        }
    }
}
