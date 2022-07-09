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
        var result = await _client.PostFormUrlEncoded("https://cachecow.eu/api/search", new(){ ["qfast"] = query });
        
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
}


