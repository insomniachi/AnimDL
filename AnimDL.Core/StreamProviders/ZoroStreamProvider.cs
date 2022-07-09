using AnimDL.Core.Extractors;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

internal class ZoroStreamProvider : BaseStreamProvider
{
    private readonly ILogger<ZoroStreamProvider> _logger;
    private readonly Dictionary<int, string> _serverIds = new()
    {
        [1] = "rapidvideo",
        [3] = "streamtape",
        [4] = "rapidvideo",
        [5] = "streamsb"
    };

    public ZoroStreamProvider(ILogger<ZoroStreamProvider> logger)
    {
        _logger = logger;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url)
    {
        var match = Regex.Match(url, "(/watch)?/[\\w-]+-(\\d+)");

        if (!match.Success)
        {
            _logger.LogError("unable to find anime slug");
            yield break;
        }

        var slug = match.Groups[2].Value;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        client.DefaultRequestHeaders.Referrer = new(Constants.Zoro);

        var ajax = $"{Constants.Zoro}ajax/v2/episode/list/{slug}";
        var jsonString = await client.GetStringAsync(ajax);

        if (JsonNode.Parse(jsonString) is not { } responseNode)
        {
            _logger.LogError("unable to parse json {Json}", jsonString);
            yield break;
        }

        if (responseNode["html"] is not { } htmlNode)
        {
            _logger.LogError("reponse doesn't contain html property");
            yield break;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlNode.ToString());

        foreach (var item in doc.QuerySelectorAll("a[title][data-number][data-id]"))
        {
            var ep = -1;
            if (item.Attributes["data-number"] is not { } epAttr)
            {
                _logger.LogWarning("episode number not found");
            }
            else
            {
                ep = int.Parse(epAttr.Value);
            }

            var dataId = item.Attributes["data-id"].Value;
            var title = item.Attributes["title"].Value;

            await ExtractEpisodes(client, dataId, title);
        }
    }

    private async Task ExtractEpisodes(HttpClient client, string dataId, string title)
    {
        var url = QueryHelpers.AddQueryString(Constants.Zoro + "ajax/v2/episode/servers", new Dictionary<string, string> { ["episodeId"] = dataId });
        var jsonString = await client.GetStringAsync(url);

        if (JsonNode.Parse(jsonString) is not { } responseNode)
        {
            _logger.LogError("ExtractEpisodes:: unable to parse json {Json}", jsonString);
            return;
        }

        if (responseNode["html"] is not { } htmlNode)
        {
            _logger.LogError("ExtractEpisodes:: reponse doesn't contain html property");
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlNode.ToString());

        foreach (var item in doc.QuerySelectorAll("div.server-item"))
        {
            var sourceUrl = QueryHelpers.AddQueryString(Constants.Zoro + "ajax/v2/episode/sources", new Dictionary<string, string>
            {
                ["id"] = item.Attributes["data-id"].Value
            });

            jsonString = await client.GetStringAsync(sourceUrl);

            if (JsonNode.Parse(jsonString) is not { } sourceNode)
            {
                _logger.LogError("ExtractEpisodes:: unable to parse json {Json}", jsonString);
                return;
            }

            var type = sourceNode["type"]!.ToString();

            var link = $"{sourceNode["link"]}";
            var epTitle = $"{sourceNode["data-type"]} - {title}";

            if (type != "iframe")
            {
                return;
            }

            if (sourceNode["server"] is not { } serverNode)
            {
                _logger.LogWarning("server not found");
                continue;
            }

            var serverId = int.Parse(serverNode.ToString());
            var server = _serverIds[serverId];
            if (new[] { "streamsb", "streamtape" }.Contains(server))
            {
                _logger.LogWarning("server not supported {Server}", server);
                continue;
            }

            var extractor = new RapidVideoExtractor();
            var result = await extractor.Extract(link);
        }

    }
}
