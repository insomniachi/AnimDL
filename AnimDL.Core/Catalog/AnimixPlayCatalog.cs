using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Splat;

namespace AnimDL.Core.Catalog;

[Obsolete("RIP")]
public class AnimixPlayCatalog : ICatalog, IMalCatalog, IEnableLogger
{
    const string BASE_URL = "https://animixplay.to";
    private readonly HttpClient _client;

    public AnimixPlayCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var result = await _client.PostFormUrlEncoded("https://cachecow.eu/api/search", new() { ["qfast"] = query });

        var resultData = JsonNode.Parse(result);

        if (resultData is null)
        {
            this.Log().Error("unable to parse {Json}", result);
            yield break;
        }

        if (resultData.AsObject()["result"]?.ToString() is not string html)
        {
            this.Log().Error("Json does not contain \"result\" property");
            yield break;
        }

        if (string.IsNullOrEmpty(html))
        {
            this.Log().Error("result does not contain data.");
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

    public async Task<(SearchResult Sub, SearchResult? Dub)> SearchByMalId(long id)
    {
        var json = await _client.PostFormUrlEncoded("https://animixplay.to/api/search", new() { ["recomended"] = $"{id}" });
        var jObject = JsonNode.Parse(json);
        var streams = jObject!["data"]![0]!["items"]!.AsArray();

        var first = new AnimixPlaySearchResult
        {
            Title = streams[0]!["title"]!.ToString(),
            Url = BASE_URL + streams[0]!["url"]!.ToString(),
            Image = streams[0]!["img"]!.ToString(),
        };

        if (streams.Count == 1)
        {
            return (first, null);
        }
        else
        {
            var second = new AnimixPlaySearchResult
            {
                Title = streams[1]!["title"]!.ToString(),
                Url = BASE_URL + streams[1]!["url"]!.ToString(),
                Image = streams[1]!["img"]!.ToString(),
            };

            var dub = first.Url.EndsWith("-dub") ? first : second;
            var sub = dub == first ? second : first;

            return (sub, dub);
        }
    }
}


