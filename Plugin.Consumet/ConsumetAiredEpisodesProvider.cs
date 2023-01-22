using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class ConsumetAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly List<string> _unavailableProviders = new() { "animepahe" };
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    internal class ConsumetAiredEpisode : AiredEpisode { }

    public ConsumetAiredEpisodesProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        if(_unavailableProviders.Contains(Config.Provider))
        {
            return Enumerable.Empty<AiredEpisode>();
        }

        var json = await _httpClient.GetStringAsync($"https://api.consumet.org/anime/{Config.Provider}/recent-episodes?page={page}");

        return JsonNode.Parse(json)?["results"]?.AsArray()?.Select(x => x.Deserialize<ConsumetZoroAiredEpisode>(_options).ToConsumetAiredEpisode());
    }

}
