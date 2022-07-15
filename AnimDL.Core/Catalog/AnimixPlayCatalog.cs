using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Catalog;

public class AnimixPlayCatalog : ICatalog
{
    const string WORKER = "https://v1.bgmvcle5cq9kjycjokrtwii9e.workers.dev/";
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
        //var result = await _client.PostFormUrlEncoded("https://cachecow.eu/api/search", new(){ ["qfast"] = query });
        
        var result = await _client.PostFormUrlEncoded(WORKER, new()
        {
            ["q2"] = query,
            ["origin"] = "1",
            ["root"] = "animixplay.to",
            ["d"] = "gogoanime.gg"
        });
        
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

        foreach (var item in doc.DocumentNode.SelectNodes("ul/li"))
        {
            var url = item.SelectSingleNode("div/a").Attributes["href"].Value;
            var title = item.SelectSingleNode("div/a").Attributes["title"].Value;
            var image = item.SelectSingleNode("div/a/img").Attributes["src"].Value;

            yield return new AnimixPlaySearchResult()
            {
                Title = title,
                Image = image,
                Url = BASE_URL + url
            };
        }
    }
}


