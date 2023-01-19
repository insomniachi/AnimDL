using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace Plugin.AllAnime;

public class AllAnimeCatalog : ICatalog
{
    private readonly HttpClient _httpClient;
    private readonly HtmlWeb _web = new();
    private readonly string _api;
    internal static readonly object _extensions = new
    {
        persistedQuery = new
        {
            version = 1,
            sha256Hash = "9c7a8bc1e095a34f2972699e8105f7aaf9082c6e1ccd56eab99c2f1a971152c6"
        }
    };

    public AllAnimeCatalog(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ServiceCollectionExtensions.USER_AGENT);
        var uriBuilder = new UriBuilder(DefaultUrl.AllAnime) { Path = "/allanimeapi" };
        uriBuilder.ToString();
        _api = uriBuilder.Uri.AbsoluteUri;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var variables = new
        {
            search = new
            {
                allowAdult = true,
                allowUnknown = true,
                query
            },
            limit = 40
        };

        var queryParams = new Dictionary<string, string>
        {
            ["variables"] = JsonSerializer.Serialize(variables),
            ["extensions"] = JsonSerializer.Serialize(_extensions)
        };

        var url = QueryHelpers.AddQueryString(_api, queryParams);
        var response = await _web.LoadFromWebAsync(url);
        var jObject = JsonNode.Parse(response.Text);
        foreach (var item in jObject?["data"]?["shows"]?["edges"]?.AsArray() ?? new JsonArray())
        {
            yield return new AllAnimeSearchResult
            {
                Title = $"{item?["name"]}",
                Url = DefaultUrl.AllAnime.TrimEnd('/') + $"/anime/{item?["_id"]}",
                Season = $"{item?["season"]?["quarter"]}",
                Year = $"{item?["season"]?["year"]}",
                Rating = $"{item?["score"]}",
                Image = $"{item?["thumbnail"]}"
            };
        }
    }
}
