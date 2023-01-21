using System.Net;
using System.Text.Json.Nodes;
using System.Web;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace Plugin.Marin;

public class MarinAiredEpisodesProvider : IAiredEpisodeProvider, IEnableLogger
{
    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer = new();

    class MarinAiredEpisode : AiredEpisode, IHaveHumanizedCreatedTime
    {
        public string CreatedAtHumanized { get; set; } = string.Empty;
    }

    public MarinAiredEpisodesProvider()
    {
        var clientHandler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        _httpClient = new HttpClient(clientHandler);
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        await _httpClient.GetAsync(Config.BaseUrl);
        var xsrfToken = HttpUtility.UrlDecode(_cookieContainer.GetCookies(new Uri(Config.BaseUrl)).FirstOrDefault(x => x.Name == "XSRF-TOKEN")?.Value);

        if (string.IsNullOrEmpty(xsrfToken))
        {
            this.Log().Fatal("unable to get XSRF-TOKEN from cookies");
            return Enumerable.Empty<AiredEpisode>();
        }

        var baseUrl = Config.BaseUrl.TrimEnd('/');
        var json = await _httpClient.PostFormUrlEncoded(baseUrl + "/episode", GetParameters(), GetHeaders(xsrfToken));

        if (string.IsNullOrEmpty(json))
        {
            return Enumerable.Empty<AiredEpisode>();
        }

        var jObject = JsonNode.Parse(json);

        var result = new List<AiredEpisode>();

        foreach (var item in jObject?["props"]?["episode_list"]?["data"]?.AsArray() ?? new JsonArray())
        {
            result.Add(new MarinAiredEpisode
            {
                Episode = int.Parse($"{item?["slug"]}"),
                Image = $"{item?["cover"]}",
                Title = $"{item?["anime"]?["title"]}",
                Url = $"{baseUrl}/anime/{item?["anime"]?["slug"]}",
                CreatedAtHumanized = $"{item?["release_ago"]}"
            });
        }

        return result;
    }

    private static Dictionary<string, string> GetParameters() => new() { ["sort"] = "rel-d" };
    private static Dictionary<string, string> GetHeaders(string xsrfToken) => new()
    {
        ["x-inertia"] = "true",
        ["x-xsrf-token"] = xsrfToken
    };
}
