using System.Text.Json.Nodes;
using System.Web;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.StreamProviders;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Splat;

namespace Plugin.Marin;

public class MarinStreamProvider : BaseStreamProvider
{
    private static readonly Dictionary<string, string> _inertiaHeaders = new Dictionary<string, string>
    {
        ["x-inertia-version"] = "5ec023866657ee7f8f9db8b2040b3ffc",
        ["x-inertia"] = "true"
    };

    public MarinStreamProvider(HttpClient client) : base(client)
    {
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var stream = await _client.GetStreamAsync(url, _inertiaHeaders);
        var doc = new HtmlDocument();
        doc.Load(stream);
        var json = HttpUtility.HtmlDecode(doc.QuerySelector("#app").Attributes["data-page"].Value);

        if (string.IsNullOrEmpty(json))
        {
            this.Log().Error("no data found");
            return 0;
        }

        var jObject = JsonNode.Parse(json);
        int.TryParse(jObject?["props"]?["anime"]?["last_episode"]?.ToString(), out int lastEpisode);
        return lastEpisode;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var lastEpisode = await GetNumberOfStreams(url);
        var (start, end) = range.Extract(lastEpisode);

        var doc = new HtmlDocument();
        for (int ep = start; ep <= end; ep++)
        {
            var stream = await _client.GetStreamAsync($"{url}/{ep}", _inertiaHeaders);
            doc.Load(stream);
            var json = HttpUtility.HtmlDecode(doc.QuerySelector("#app").Attributes["data-page"].Value);
            var jObject = JsonNode.Parse(json);
            var title = GetTitle(jObject?["props"]?["episode"]?["data"]?["title"]?.AsArray() ?? new JsonArray());

            var videoStreamForEp = new VideoStreamsForEpisode() { Episode = ep };
            videoStreamForEp.AdditionalInformation.Add("title-en", title);
            foreach (var epStream in jObject?["props"]?["video"]?["data"]?["mirror"]?.AsArray() ?? new JsonArray())
            {
                var resolution = $"{epStream?["resolution"]}";
                var streamUrl = $"{epStream?["code"]?["file"]}";
                
                if (videoStreamForEp.Qualities.ContainsKey(resolution))
                {
                    continue;
                }

                videoStreamForEp.Qualities.Add(resolution, new VideoStream
                {
                    Quality = resolution,
                    Url = streamUrl
                });

            }

            yield return videoStreamForEp;
        }
    }

    private static string GetTitle(JsonArray titleNode)
    {
        foreach (var item in titleNode)
        {
            if ($"{item?["language"]?["id"]}" == "3")
            {
                return $"{item?["text"]}";
            }
        }

        return string.Empty;
    }
}
