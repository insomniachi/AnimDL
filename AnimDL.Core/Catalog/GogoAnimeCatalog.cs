using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace AnimDL.Core.Catalog;

public class GogoAnimeCatalog : ICatalog
{
    const string SEARCH_URL = "https://gogoanime.lu/search.html";
    const string BASE_URL = "https://gogoanime.lu/";
    private readonly HttpClient _client;

    public GogoAnimeCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var html = await _client.GetStringAsync(SEARCH_URL, parameters: new() { ["keyword"] = query });

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
