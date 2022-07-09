using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Catalog;

internal class ZoroCatalog : ICatalog
{
    readonly string SEARCH_URL = Constants.Zoro + "search";

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var url = QueryHelpers.AddQueryString(SEARCH_URL, new Dictionary<string, string> { ["keyword"] = query });
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);

        foreach (var item in doc.QuerySelectorAll("a.item-qtip[title][data-id]"))
        {
            var animePageUrl = item.Attributes["href"].Value;
            yield return new SearchResult
            {
                Title = item.Attributes["title"].Value,
                Url = string.Concat(Constants.Zoro.TrimEnd('/'), animePageUrl.AsSpan(0, animePageUrl.Length - 11))
            };
        }
    }
}
