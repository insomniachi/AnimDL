using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Catalog;

public class AnimixPlayCatalog : ICatalog, IMalCatalog
{
    const string BASE_URL = "https://animixplay.to";
    private readonly HttpClient _client;
    private readonly ILogger<AnimixPlayCatalog> _logger;

    public AnimixPlayCatalog(HttpClient client, ILogger<AnimixPlayCatalog> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var result = await _client.PostFormUrlEncoded("https://cachecow.eu/api/search", new() { ["qfast"] = query });

        var resultData = JsonNode.Parse(result);

        if (resultData is null)
        {
            _logger.LogError("unable to parse {Json}", resultData);
            yield break;
        }

        if (resultData.AsObject()["result"]?.ToString() is not string html)
        {
            _logger.LogError("Json does not contain \"result\" property");
            yield break;
        }

        if(string.IsNullOrEmpty(html))
        {
            _logger.LogError("result does not contain data.");
            yield break;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var item in doc.DocumentNode.SelectNodes("a"))
        {
            var url = item.Attributes["href"].Value;
            var title = item.Attributes["title"].Value;
            var image = item.SelectSingleNode("li/div/img").Attributes["src"].Value;

            yield return new AnimixPlaySearchResult()
            {
                Title = title,
                Image = image,
                Url = BASE_URL + url
            };
        }
    }

    public async Task<SearchResult> SearchByMalId(long id)
    {
        var json = await _client.PostFormUrlEncoded("https://animixplay.to/api/search", new() { ["recomended"] = $"{id}" });

        var jObject = JsonNode.Parse(json);

        var stream = jObject!["data"]![0]!["items"]![0];

        return new AnimixPlaySearchResult
        {
            Title = stream!["title"]!.ToString(),
            Url = BASE_URL + stream!["url"]!.ToString(),
            Image = stream!["img"]!.ToString(),
        };
    }
}


