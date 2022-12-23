using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.Catalog;

public class TenshiCatalog : ICatalog
{
    private readonly HttpClient _client;

    public TenshiCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        await _client.BypassDDoS(DefaultUrl.Tenshi);
        var html = await _client.GetStreamAsync(DefaultUrl.Tenshi + "anime", parameters: new() { ["q"] = query });

        var doc = new HtmlDocument();
        doc.Load(html);
        foreach (var item in doc.QuerySelectorAll(".list > li > a"))
        {
            yield return new SearchResult
            {
                Title = item.Attributes["title"].Value,
                Url = item.Attributes["href"].Value,
            };
        }
    }
}
