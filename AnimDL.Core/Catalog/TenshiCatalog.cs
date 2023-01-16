using System.Net;
using System.Text.RegularExpressions;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.SearchResults;
using Newtonsoft.Json.Linq;

namespace AnimDL.Core.Catalog;

[Obsolete("Rebranded to Marin")]
internal partial class TenshiCatalog : ICatalog
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer = new();

    public TenshiCatalog()
    {
        var clientHandler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        _client = new HttpClient(clientHandler);
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        await _client.BypassDDoS(DefaultUrl.Tenshi);
        var html = await _client.GetStringAsync(DefaultUrl.Tenshi);
        var csrfToken = CsrfTokenRegex().Match(html).Groups[1].Value;
        var cookie = string.Join(";",_cookieContainer.GetCookies(new Uri(DefaultUrl.Tenshi)).Cast<Cookie>().Select(item => $"{item.Name}={item.Value}"));

        var json = await _client.PostFormUrlEncoded("https://tenshi.moe/anime/search", new Dictionary<string, string>() { ["q"] = query }, new() 
        {
            [Headers.Cookie] = cookie,
            ["x-requested-with"] = "XMLHttpRequest",
            ["x-csrf-token"] = csrfToken
        });

        foreach (var item in JArray.Parse(json))
        {
            var url = $"{item["url"]}";
            var title = $"{item["title"]}";
            var image = $"{item["cover"]}";
            var genre = $"{item["genre"]}";
            var year = $"{item["year"]}";
            var type = $"{item["type"]}";
            var eps = $"{item["eps"]}";

            yield return new TenshiSearchResult
            {
                Title = title,
                Url = url,
                Image = image,
                Genre = genre,
                Type = type,
                Episodes = eps,
                Year = year,
            };
        }
    }

    [GeneratedRegex(@"<meta name=""csrf-token"" content=""(.+)"">")]
    private static partial Regex CsrfTokenRegex();
}
