using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

internal class TenshiMoeStreamProvider : BaseStreamProvider
{
    readonly string BASE_URL = Constants.Tenshi;
    private readonly Regex _streamRegex = new("src: '(.*)',[\x00-\x7F]*?size: (\\d+)", RegexOptions.Compiled);
    private readonly ILogger<TenshiMoeStreamProvider> _logger;

    public TenshiMoeStreamProvider(ILogger<TenshiMoeStreamProvider> logger, HttpClient client) : base(client)
    {
        _logger = logger;
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        await _client.BypassDDoS(BASE_URL);
        var html = await _client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return int.Parse(doc.QuerySelector("span.badge").InnerText);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        await _client.BypassDDoS(BASE_URL);
        var html = await _client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var count = int.Parse(doc.QuerySelector("span.badge").InnerText);
        (int start, int end) = range.Extract(count);

        foreach (var ep in Enumerable.Range(start, end - start + 1))
        {
            if (await ExtractUrls(_client, $"{url}/{ep}") is VideoStreamsForEpisode streamForEp)
            {
                streamForEp.Episode = ep;
                yield return streamForEp;
            }
        }
    }

    private async Task<VideoStreamsForEpisode?> ExtractUrls(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var embedStream = doc.QuerySelector("iframe")?.Attributes["src"].Value;

        if (string.IsNullOrEmpty(embedStream))
        {
            _logger.LogError("unable to find embed stream");
            return null;
        }

        html = await client.GetStringAsync(embedStream);

        var streamsForEp = new VideoStreamsForEpisode();
        foreach (Match match in _streamRegex.Matches(html))
        {
            var quality = match.Groups[2].Value;

            streamsForEp.Qualities.Add(quality, new VideoStream
            {
                Quality = quality,
                Url = match.Groups[1].Value
            });
        }

        return streamsForEp;
    }
}
