using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Catalog;

public class TenshiMoeCatalog : ICatalog
{
    const string BASE_URL = "https://tenshi.moe/";
    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        using var client = new HttpClient();
        await client.BypassDDoS(BASE_URL);
        var uri = QueryHelpers.AddQueryString(BASE_URL + "anime", new Dictionary<string, string> { ["q"] = query });
        var result = await client.GetAsync(uri);
        var html = await result.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);
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
