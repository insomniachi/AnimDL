using System.Net;
using System.Text.Json.Nodes;
using System.Web;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace Plugin.Marin;

public class MarinCatalog : ICatalog, IEnableLogger
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer = new();

    public MarinCatalog()
    {
        var clientHandler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        _httpClient = new HttpClient(clientHandler);
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        await _httpClient.GetStringAsync(DefaultUrl.Marin, new Dictionary<string, string> { ["range"] = "bytes=0-0" });
        var xsrfToken = HttpUtility.UrlDecode(_cookieContainer.GetCookies(new Uri(DefaultUrl.Marin)).FirstOrDefault(x => x.Name == "XSRF-TOKEN")?.Value);

        if (string.IsNullOrEmpty(xsrfToken))
        {
            this.Log().Fatal("unable to get XSRF-TOKEN from cookies");
            yield break;
        }

        var json = await _httpClient.PostFormUrlEncoded(DefaultUrl.Marin.TrimEnd('/') + "/anime", GetSearchParameters(query), GetHeaders(xsrfToken));

        if (string.IsNullOrEmpty(json))
        {
            this.Log().Error("no data returned");
            yield break;
        }

        var jObject = JsonNode.Parse(json);
        Config.Version = jObject["version"]?.ToString();

        foreach (var item in jObject?["props"]?["anime_list"]?["data"]?.AsArray() ?? new JsonArray())
        {
            yield return new MarinSearchResult
            {
                Title = $"{item?["title"]}",
                Url = $"{DefaultUrl.Marin.TrimEnd('/')}/anime/{item?["slug"]}",
                Image = $"{item?["cover"]}",
                Year = $"{item?["year"]}",
                Type = $"{item?["type"]}"
            };
        }
    }

    private static Dictionary<string, string> GetSearchParameters(string query) => new() { ["search"] = query };
    private static Dictionary<string, string> GetHeaders(string xsrfToken) => new()
    {
        ["x-inertia"] = "true",
        ["x-xsrf-token"] = xsrfToken
    };
}
