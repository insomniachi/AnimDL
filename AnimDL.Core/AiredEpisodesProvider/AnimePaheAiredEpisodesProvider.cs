using System.Globalization;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace AnimDL.Core.AiredEpisodesProvider;

internal class AnimePaheAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _api;
    private readonly string _baseAnimeUrl;

    class AnimePaheAiredEpisode : AiredEpisode , IHaveCreatedTime
    {
        public DateTime CreatedAt { get; set; }
    }

    public AnimePaheAiredEpisodesProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var uriBuilder = new UriBuilder(DefaultUrl.AnimePahe) { Path = "/api" };
        _api = uriBuilder.Uri.AbsoluteUri;
        uriBuilder.Path = "/anime";
        _baseAnimeUrl = uriBuilder.Uri.AbsoluteUri;
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        var json = await _httpClient.GetStringAsync(_api, new Dictionary<string, string>
        {
            ["m"] = "airing",
            ["page"] = page.ToString()
        });

        var jObject = JsonNode.Parse(json);
        var data = jObject!["data"]!.AsArray();

        return data.Select(x => new AnimePaheAiredEpisode
        {
            Title = $"{x!["anime_title"]}",
            Image = $"{x["snapshot"]}",
            Url = $"{_baseAnimeUrl}/{x["anime_session"]}",
            Episode = (int)x!["episode"]!.AsValue(),
            CreatedAt = DateTime.ParseExact($"{x["created_at"]}","yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime(),
        });
    }
}
