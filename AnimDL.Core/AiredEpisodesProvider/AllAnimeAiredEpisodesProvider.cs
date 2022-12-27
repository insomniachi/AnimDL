using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Catalog;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.AiredEpisodesProvider;

internal class AllAnimeAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HtmlWeb _web = new();
    private readonly string _api;

    class AllAnimeAiredEpisode : AiredEpisode, IHaveCreatedTime
    {
        public DateTime CreatedAt { get; set; }
    }


    public AllAnimeAiredEpisodesProvider()
    {
        var uriBuilder = new UriBuilder(DefaultUrl.AllAnime) { Path = "/allanimeapi" };
        uriBuilder.ToString();
        _api = uriBuilder.Uri.AbsoluteUri;
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        var variables = new
        {
            search = new
            {
                allowAdult = false,
                allowUnknown = false,
            },
            limit = 26,
            page,
            translationType = "sub",
            countryOrigin = "ALL"
        };

        var queryParams = new Dictionary<string, string>
        {
            ["variables"] = JsonSerializer.Serialize(variables),
            ["extensions"] = JsonSerializer.Serialize(AllAnimeCatalog._extensions)
        };

        var url = QueryHelpers.AddQueryString(_api, queryParams);
        var response = await _web.LoadFromWebAsync(url);
        var jObject = JsonNode.Parse(response.Text);

        var uriBuilder = new UriBuilder(DefaultUrl.AllAnime);
        var result = new List<AiredEpisode>();
        foreach (var item in jObject?["data"]?["shows"]?["edges"]?.AsArray() ?? new JsonArray())
        {
            var title = $"{item?["name"]}";
            var image = $"{item?["thumbnail"]}";
            var year = int.Parse($"{item?["lastEpisodeDate"]?["sub"]?["year"]}");
            var month = int.Parse($"{item?["lastEpisodeDate"]?["sub"]?["month"]}") + 1; // months are from 0-11
            var day = int.Parse($"{item?["lastEpisodeDate"]?["sub"]?["date"]}");
            var hour = int.Parse($"{item?["lastEpisodeDate"]?["sub"]?["hour"]}");
            var min = int.Parse($"{item?["lastEpisodeDate"]?["sub"]?["minute"]}");
            var ep = $"{item?["lastEpisodeInfo"]?["sub"]?["episodeString"]}";
            int.TryParse(ep, out int epInt);
            var datetime = new DateTime(year, month, day, hour, min, 0).ToLocalTime();
            uriBuilder.Path = $"/anime/{item?["_id"]}";
            var animeUrl = uriBuilder.Uri.AbsoluteUri;
            
            result.Add(new AllAnimeAiredEpisode
            {
                Title = title,
                Image = image,
                CreatedAt = datetime,
                Episode = epInt,
                EpisodeString = ep,
                Url = animeUrl,
            });
        }

        return result;
    }
}
