using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Catalog;

internal class AllAnimeCatalog : ICatalog
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
        var uriBuilder = new UriBuilder(DefaultUrl.AllAnime);
        foreach (var item in jObject?["data"]?["shows"]?["edges"]?.AsArray() ?? new JsonArray())
        {
            uriBuilder.Path = $"/anime/{item?["_id"]}";
            yield return new SearchResult
            {
                Title = $"{item?["name"]}",
                Url = uriBuilder.Uri.AbsoluteUri,
            };
        }

        yield break;
    }
}
