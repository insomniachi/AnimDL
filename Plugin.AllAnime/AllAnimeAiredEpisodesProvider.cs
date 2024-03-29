﻿using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;

namespace Plugin.AllAnime;

public class AllAnimeAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HtmlWeb _web = new();

    class AllAnimeAiredEpisode : AiredEpisode, IHaveCreatedTime
    {
        public DateTime CreatedAt { get; set; }
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
            countryOrigin = Config.CountryOfOrigin
        };

        var queryParams = new Dictionary<string, string>
        {
            ["variables"] = JsonSerializer.Serialize(variables),
            ["extensions"] = JsonSerializer.Serialize(AllAnimeCatalog._extensions)
        };

        var url = QueryHelpers.AddQueryString(Config.BaseUrl.TrimEnd('/') + "/allanimeapi", queryParams);
        var response = await _web.LoadFromWebAsync(url);
        var jObject = JsonNode.Parse(response.Text);

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
            _ = int.TryParse(ep, out int epInt);
            var datetime = new DateTime(year, month, day, hour, min, 0).ToLocalTime();
            var animeUrl = Config.BaseUrl.TrimEnd('/') + $"/anime/{item?["_id"]}";

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
