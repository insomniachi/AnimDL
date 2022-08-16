using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.StreamProviders;

internal class YugenAnimeStreamProvider : BaseStreamProvider
{
    const string BASE_URL = "https://yugen.to/";
    const string EMBED_URL = "https://yugen.to/api/embed/";
    public YugenAnimeStreamProvider(HttpClient client) : base(client)
    {
    }

    public override Task<int> GetNumberOfStreams(string url)
    {
        return base.GetNumberOfStreams(url);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url + "watch");

        var epSection = doc.QuerySelectorAll(".data")
                           .Select(x => x.InnerText)
                           .Where(x => x?.Contains("Episodes") == true)
                           .First();

        var match = Regex.Match(epSection, @"(\d+)");

        if(!match.Success)
        {
            yield break;
        }

        var totalEpisodes = int.Parse(match.Groups[1].Value);
        var (start, end) = range.Extract(totalEpisodes);

        var uri = new UriBuilder(url).Uri;
        var slug = uri.Segments[^1].TrimEnd('/');
        var number = uri.Segments[^2].TrimEnd('/');

        for (int i = start; i <= end; i++)
        {
            var epUrl = $"{BASE_URL}watch/{number}/{slug}/{i}/";
            doc = await Load(epUrl);
            var text = doc.Text;
            var iframeUrl = doc.QuerySelector("iframe").Attributes["src"].Value;

            uri = new UriBuilder(iframeUrl).Uri;
            var key = uri.Segments[^1].TrimEnd('/');

            var json = await _client.PostFormUrlEncoded(EMBED_URL, new() 
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
                Episode = i,
                Qualities = new Dictionary<string, VideoStream>
                {
                    ["default"] = new VideoStream
                    {
                        Quality = "default",
                        Url = jObject!["hls"]!.ToString()
                    }
                }
            };
        }

        yield break;
    }
}
