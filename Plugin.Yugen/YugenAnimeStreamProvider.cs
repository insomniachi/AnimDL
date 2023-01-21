using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack.CssSelectors.NetCore;
using AnimDL.Core.StreamProviders;
using AnimDL.Core;

namespace Plugin.Yugen;

public partial class YugenAnimeStreamProvider : BaseStreamProvider
{
    public YugenAnimeStreamProvider(HttpClient client) : base(client)
    {
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var doc = await Load(url + "watch");

        var epSection = doc.QuerySelectorAll(".data")
                           .Select(x => x.InnerText)
                           .Where(x => x?.Contains("Episodes") == true)
                           .First();

        var match = EpisodeRegex().Match(epSection);

        if (!match.Success)
        {
            return 0;
        }

        return int.Parse(match.Groups[1].Value);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url + "watch");

        var epSection = doc.QuerySelectorAll(".data")
                           .Select(x => x.InnerText)
                           .Where(x => x?.Contains("Episodes") == true)
                           .First();

        var match = EpisodeRegex().Match(epSection);

        if (!match.Success)
        {
            yield break;
        }

        var totalEpisodes = int.Parse(match.Groups[1].Value);
        var (start, end) = range.Extract(totalEpisodes);

        var uri = new UriBuilder(url).Uri;
        var slug = uri.Segments[^1].TrimEnd('/');
        var number = uri.Segments[^2].TrimEnd('/');

        var baseUrl = Config.BaseUrl.TrimEnd('/');
        for (int ep = start; ep <= end; ep++)
        {
            var epUrl = $"{baseUrl}/watch/{number}/{slug}/{ep}/";
            doc = await Load(epUrl);
            var text = doc.Text;
            var iframeUrl = doc.QuerySelector("iframe").Attributes["src"].Value;

            if(!iframeUrl.StartsWith("https:"))
            {
                iframeUrl = $"https:{iframeUrl}";
            }

            uri = new UriBuilder(iframeUrl).Uri;
            var key = uri.Segments[^1].TrimEnd('/');

            var json = await _client.PostFormUrlEncoded($"{baseUrl}/api/embed/", new()
            {
                ["id"] = key,
                ["ac"] = "0"
            }, new Dictionary<string, string>()
            {
                ["referer"] = iframeUrl,
                ["X-Requested-With"] = "XMLHttpRequest",
            });
            var jObject = JsonNode.Parse(json);

            yield return new VideoStreamsForEpisode
            {
                Episode = ep,
                Qualities = new Dictionary<string, VideoStream>
                {
                    ["default"] = new VideoStream
                    {
                        Quality = "default",
                        Url = jObject!["hls"]!.AsArray()!.FirstOrDefault()!.ToString(),
                    }
                }
            };
        }

        yield break;
    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex EpisodeRegex();
}
