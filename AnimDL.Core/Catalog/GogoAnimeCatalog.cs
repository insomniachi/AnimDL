using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;

namespace AnimDL.Core.Catalog;

public class GogoAnimeCatalog : ICatalog
{
    readonly string SEARCH_URL = $"{DefaultUrl.GogoAnime}search.html";
    private readonly HttpClient _client;

    public GogoAnimeCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var html = await _client.GetStreamAsync(SEARCH_URL, parameters: new() { ["keyword"] = query });

        var doc = new HtmlDocument();
        doc.Load(html);

        foreach (var item in doc.DocumentNode.SelectNodes("//p[@class=\"name\"]/a"))
        {
            yield return new SearchResult
            {
                Title = item.Attributes["title"].Value,
                Url = DefaultUrl.GogoAnime + item.Attributes["href"].Value,
            };
        }
    }
}
