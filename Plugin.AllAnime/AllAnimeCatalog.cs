using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace Plugin.AllAnime;

public class AllAnimeCatalog : ICatalog
{
    private readonly HtmlWeb _web = new();
    internal static readonly object _extensions = new
    {
        persistedQuery = new
        {
            version = 1,
            sha256Hash = "9c7a8bc1e095a34f2972699e8105f7aaf9082c6e1ccd56eab99c2f1a971152c6"
        }
    };

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

        var baseUrl = Config.BaseUrl.TrimEnd('/');
        var url = QueryHelpers.AddQueryString("https://api.allanime.co/allanimeapi", queryParams);
        var response = await _web.LoadFromWebAsync(url);
        var jObject = JsonNode.Parse(response.Text);
        foreach (var item in jObject?["data"]?["shows"]?["edges"]?.AsArray() ?? new JsonArray())
        {
            yield return new AllAnimeSearchResult
            {
                Title = $"{item?["name"]}",
                Url = baseUrl + $"/anime/{item?["_id"]}",
                Season = $"{item?["season"]?["quarter"]}",
                Year = $"{item?["season"]?["year"]}",
                Rating = $"{item?["score"]}",
                Image = $"{item?["thumbnail"]}"
            };
        }
    }
}
