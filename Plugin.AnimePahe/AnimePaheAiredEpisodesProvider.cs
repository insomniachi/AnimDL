using System.Globalization;
using System.Text.Json.Nodes;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.AnimePahe;

public class AnimePaheAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HttpClient _httpClient;

    class AnimePaheAiredEpisode : AiredEpisode, IHaveCreatedTime
    {
        public DateTime CreatedAt { get; set; }
    }

    public AnimePaheAiredEpisodesProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        var json = await _httpClient.GetStringAsync(Config.BaseUrl.TrimEnd('/') + "/api", new Dictionary<string, string>
        {
            ["m"] = "airing",
            ["page"] = page.ToString()
        });

        var jObject = JsonNode.Parse(json);
        var data = jObject!["data"]!.AsArray();

        var baseAnimeUrl = Config.BaseUrl.TrimEnd('/') + "/anime";
        return data.Select(x => new AnimePaheAiredEpisode
        {
            Title = $"{x!["anime_title"]}",
            Image = $"{x["snapshot"]}",
            Url = $"{baseAnimeUrl}/{x["anime_session"]}",
            Episode = (int)x!["episode"]!.AsValue(),
            CreatedAt = DateTime.ParseExact($"{x["created_at"]}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).ToLocalTime(),
        });
    }
}
