using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Catalog;

public class GogoAnimeCatalog : ICatalog
{
    const string SEARCH_URL = "https://gogoanime.lu/search.html";
    const string BASE_URL = "https://gogoanime.lu/";

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var url = QueryHelpers.AddQueryString(SEARCH_URL, new Dictionary<string, string> { ["keyword"] = query });

        var client = new HttpClient();
        var html = await client.GetStringAsync(url);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var item in doc.DocumentNode.SelectNodes("//p[@class=\"name\"]/a"))
        {
            yield return new SearchResult
            {
                Title = item.Attributes["title"].Value,
                Url = BASE_URL + item.Attributes["href"].Value,
            };
        }
    }
}
