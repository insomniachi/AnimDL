using AnimDL.Core.Extractors;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

public partial class GogoAnimeStreamProvider : BaseStreamProvider
{
    private readonly string _baseUrlStripped;
    public const string EPISODE_LOAD_AJAX = "https://ajax.gogo-load.com/ajax/load-list-episode";
    private readonly GogoPlayExtractor _extractor;
    private readonly ILogger<GogoAnimeStreamProvider> _logger;

    [GeneratedRegex("<input.*?value=\"([0-9]+)\".*?id=\"movie_id\"", RegexOptions.Compiled)]
    private static partial Regex AnimeIdRegex();

    public GogoAnimeStreamProvider(GogoPlayExtractor extractor, ILogger<GogoAnimeStreamProvider> logger, HttpClient client) : base(client)
    {
        _extractor = extractor;
        _logger = logger;

        _baseUrlStripped = DefaultUrl.GogoAnime.EndsWith("/") || DefaultUrl.GogoAnime.EndsWith("\\") ? DefaultUrl.GogoAnime[..^1] : DefaultUrl.GogoAnime;
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var html = await _client.GetStringAsync(ConvertHost(url));

        var match = AnimeIdRegex().Match(html);

        if (!match.Success)
        {
            _logger.LogError("unable to match id regex");
            return 0;
        }

        var contentId = match.Groups[1].Value;

        html = await _client.GetStringAsync(EPISODE_LOAD_AJAX, parameters: new()
        {
            ["ep_start"] = "0",
            ["ep_end"] = "100000",
            ["id"] = contentId
        });

        var epMatch = EpisodeRegex().Match(html);

        if(!epMatch.Success)
        {
            return 0;
        }

        return int.Parse(epMatch.Groups[1].Value);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var html = await _client.GetStringAsync(ConvertHost(url));

        var match = AnimeIdRegex().Match(html);

        if(!match.Success)
        {
            _logger.LogError("unable to match id regex");
            yield break;
        }

        var contentId = match.Groups[1].Value;

        html = await _client.GetStringAsync(EPISODE_LOAD_AJAX, parameters: new()
        {
            ["ep_start"] = "0",
            ["ep_end"] = "100000",
            ["id"] = contentId
        });

        var epMatch = EpisodeRegex().Match(html);

        int start = 0;
        int end = 100000;
        if(!epMatch.Success)
        {
            _logger.LogWarning("did not find any episode number");
        }
        else
        {
            var count = int.Parse(epMatch.Groups[1].Value);
            (start, end) = range.Extract(count);
        }

        await foreach (var (ep, embedUrl) in GetEpisodeList(_client, contentId, start, end))
        {
            var embedPageUrl = await GetEmbedPage(_client,embedUrl);
            var stream = await _extractor.Extract(embedPageUrl);

            if(stream is null)
            {
                _logger.LogWarning("unable to find stream {Url}", embedUrl);
                continue;
            }

            stream.Episode = ep;
            yield return stream;
        }
    }

    private async IAsyncEnumerable<(int ep, string embedUrl)> GetEpisodeList(HttpClient client, string contentId, int start, int end)
    {
        var html = await client.GetStreamAsync(EPISODE_LOAD_AJAX, parameters: new()
        {
            ["ep_start"] = $"{start}",
            ["ep_end"] = $"{end}",
            ["id"] = contentId
        });

        var doc = new HtmlDocument();
        doc.Load(html);

        foreach (var item in doc.QuerySelectorAll("a[class=\"\"] , a[class=\"\"]").Reverse())
        {
            var embedUrl = _baseUrlStripped + item.Attributes["href"].Value.Trim();
            var epMatch = EpisodeRegex().Match(item.InnerHtml);
            int ep = -1;
            
            if(epMatch.Success)
            {
                ep = int.Parse(epMatch.Groups[1].Value);
            }
            else
            {
                _logger.LogWarning("unable to match episode number");
            }

            yield return (ep, embedUrl);
        }
    }

    private static async Task<string> GetEmbedPage(HttpClient client, string url)
    {
        var html = await client.GetStreamAsync(url);
        var doc = new HtmlDocument();
        doc.Load(html);
        return $"https:{doc.QuerySelector("iframe").Attributes["src"].Value}";
    }

    private static string ConvertHost(string url)
    {
        var converted = UrlRegex().Replace(url, DefaultUrl.GogoAnime);
        return converted;
    }

    [GeneratedRegex("EP.*?(\\d+)")]
    private static partial Regex EpisodeRegex();
   
    [GeneratedRegex("https?://gogoanime.\\w*/")]
    private static partial Regex UrlRegex();
}
