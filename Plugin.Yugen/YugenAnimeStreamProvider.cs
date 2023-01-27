using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.StreamProviders;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Plugin.Yugen;

public partial class YugenAnimeStreamProvider : BaseStreamProvider, IMultiAudioStreamProvider
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

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range, string streamType)
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
            var hasDub = doc.DocumentNode.SelectSingleNode(@"/html/body/main/div/div/div[1]/div[2]/div[2]/div[2]/a")?.InnerText.Trim().StartsWith("Dub") == true;
            var json = await _client.PostFormUrlEncoded($"{baseUrl}/api/embed/", new()
            {
                ["id"] = GetStreamKey(number, ep, streamType),
                ["ac"] = "0"
            }, new Dictionary<string, string>()
            {
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
                },
                StreamTypes = hasDub ? new[] { "sub", "dub" } : Enumerable.Empty<string>()
            };
        }
    }

    public override IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range) => GetStreams(url, range, Config.StreamType);

    private static string ToBase64String(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    private static string GetStreamKey(string id, int ep, string streamType)
    {
        if(streamType == "sub")
        {
            return ToBase64String($"{id}|{ep}");
        }
        else
        {
            return ToBase64String($"{id}|{ep}|dub");
        }
    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex EpisodeRegex();
}
