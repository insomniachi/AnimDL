using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Catalog;

public class AnimixPlayCatalog : ICatalog
{

    const string BASE_URL = "https://animixplay.to";

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var result = await HttpHelper.PostFormUrlEncoded("https://cachecow.eu/api/search", new Dictionary<string, string> { ["qfast"] = query });
        var resultData = JsonNode.Parse(result);

        if (resultData is null)
        {
            yield break;
        }

        if (resultData.AsObject()["result"]?.ToString() is not string html)
        {
            yield break;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var item in doc.DocumentNode.SelectNodes("li"))
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


