using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class ConsumetCatalog : ICatalog
{
    private readonly HttpClient _httpClient;
    protected readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConsumetCatalog(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var response = await _httpClient.GetStringAsync($"https://api.consumet.org/anime/{Config.Provider}/{query}");
        var jObject = JsonNode.Parse(response);

        var results = jObject?["results"]?.AsArray() ?? new JsonArray();

        foreach (var item in results)
        {
            yield return ConvertModel(item);
        }
    }

    private SearchResult ConvertModel(JsonNode node)
    {
        return Config.Provider switch
        {
            "animepahe" => node.Deserialize<ConsumetAnimePaheSearchResult>(_options).ToSearchResult(),
            "zoro" => node.Deserialize<ConsumetZoroSearchResult>(_options).ToSearchResult(),
            _ => throw new NotSupportedException()
        };
    }
}

