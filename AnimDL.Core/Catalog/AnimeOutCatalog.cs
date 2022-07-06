using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Catalog;

public class AnimeOutCatalog : ICatalog
{
    const string BASE_URL = "https://www.animeout.xyz/";
    const string CORS_PROXY = "https://corsproxy.io/";

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var url = QueryHelpers.AddQueryString(BASE_URL, new Dictionary<string, string> { ["s"] = query});
        url = CORS_PROXY + "?" + url;

        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(url);

        foreach (var item in doc.DocumentNode.SelectNodes("//h3[@class=\"post-title entry-title\"]/a"))
        {
            yield return new SearchResult
            {
                Url = item.Attributes["href"].Value,
                Title = item.InnerHtml
            };
        }
    }
}
