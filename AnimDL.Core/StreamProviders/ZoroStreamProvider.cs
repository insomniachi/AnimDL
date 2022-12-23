using AnimDL.Core.Extractors;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

internal partial class ZoroStreamProvider : BaseStreamProvider
{
    private readonly ILogger<ZoroStreamProvider> _logger;
    private readonly RapidVideoExtractor _extractor;
    private readonly Dictionary<int, string> _serverIds = new()
    {
        [1] = "rapidvideo",
        [3] = "streamtape",
        [4] = "rapidvideo",
        [5] = "streamsb"
    };
    private readonly List<string> _unsupported = new()
    {
        "streamsb",
        "streamtape"
    };

    public ZoroStreamProvider(ILogger<ZoroStreamProvider> logger,
                              HttpClient client,
                              RapidVideoExtractor extractor) : base(client)
    {
        _logger = logger;
        _extractor = extractor;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var match = SlugRegex().Match(url);

        if (!match.Success)
        {
            _logger.LogError("unable to find anime slug");
            yield break;
        }

        var slug = match.Groups[2].Value;
        var ajax = $"{DefaultUrl.Zoro}ajax/v2/episode/list/{slug}";
        var jsonString = await _client.GetStringAsync(ajax, headers: new()
        {
            ["X-Requested-With"] = "XMLHttpRequest",
            ["Referer"] = DefaultUrl.Zoro
        });

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
        var eps = doc.QuerySelectorAll("a[title][data-number][data-id]").ToList();
        var count = eps.Count;
        var (start, end) = range.Extract(count);

        foreach (var item in eps)
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

            if(ep < start && ep > end)
            {
                continue;
            }

            var dataId = item.Attributes["data-id"].Value;
            var title = item.Attributes["title"].Value;

            await foreach (var stream in ExtractEpisodes(dataId, title))
            {
                if(stream is null)
                {
                    _logger.LogWarning("unable to extract episode {EP}", ep);
                    continue;
                }

                stream.Episode = ep;
                yield return stream;
            }
        }
    }

    private async IAsyncEnumerable<VideoStreamsForEpisode?> ExtractEpisodes(string dataId, string title)
    {
        var jsonString = await _client.GetStringAsync(DefaultUrl.Zoro + "ajax/v2/episode/servers", parameters: new() { ["episodeId"] = dataId });

        if (JsonNode.Parse(jsonString) is not { } responseNode)
        {
            _logger.LogError("ExtractEpisodes:: unable to parse json {Json}", jsonString);
            yield break;
        }

        if (responseNode["html"] is not { } htmlNode)
        {
            _logger.LogError("ExtractEpisodes:: reponse doesn't contain html property");
            yield break;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlNode.ToString());

        foreach (var item in doc.QuerySelectorAll("div.server-item"))
        {
            jsonString = await _client.GetStringAsync(DefaultUrl.Zoro + "ajax/v2/episode/sources", parameters: new()
            {
                ["id"] = item.Attributes["data-id"].Value
            });

            if (JsonNode.Parse(jsonString) is not { } sourceNode)
            {
                _logger.LogError("ExtractEpisodes:: unable to parse json {Json}", jsonString);
                yield break;
            }

            var type = sourceNode["type"]!.ToString();

            var link = $"{sourceNode["link"]}";
            var epTitle = $"{sourceNode["data-type"]} - {title}";

            if (type != "iframe")
            {
                yield break;
            }

            if (sourceNode["server"] is not { } serverNode)
            {
                _logger.LogWarning("server not found");
                continue;
            }

            var serverId = int.Parse(serverNode.ToString());
            var server = _serverIds[serverId];
            if (_unsupported.Contains(server))
            {
                //_logger.LogWarning("server not supported {Server}", server);
                continue;
            }

            yield return await _extractor.Extract(link);
        }

    }

    [GeneratedRegex("(/watch)?/[\\w-]+-(\\d+)")]
    private static partial Regex SlugRegex();
}
