using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class ConsumetStreamProvider : IStreamProvider
{
    private readonly HttpClient _httpClient;

    public ConsumetStreamProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetNumberOfStreams(string url)
    {
        var requestUrl = GetInfoApiUrl(url);
        var json = await _httpClient.GetStringAsync(requestUrl);
        var jObject = JsonNode.Parse(json);
        var totalEpisoes = (int?)jObject["totalEpisodes"]?.AsValue();
        return totalEpisoes ?? CalculateEpisodes(jObject);
    }

    private static int CalculateEpisodes(JsonNode jObject)
    {
        if (jObject["episodes"] is JsonArray ja)
        {
            var episodesPerPage = (int?)jObject["episodePages"]?.AsValue();
            return ja.Count * (episodesPerPage ?? 1);
        }

        var @obj = jObject["episodes"];
        return Config.CrunchyrollStreamType == "sub"
            ? obj["subbed1"]?.AsArray().Count ?? 0
            : obj["English Dub1"]?.AsArray().Count ?? 0;
    }

    private static JsonArray GetEpisodesArray(JsonNode jObject)
    {
        if (jObject["episodes"] is JsonArray ja)
        {
            return ja;
        }

        return Config.CrunchyrollStreamType == "sub"
            ? jObject["episodes"]["subbed1"]?.AsArray() ?? new JsonArray()
            : jObject["episodes"]["English Dub1"]?.AsArray() ?? new JsonArray();
    }

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var json = await _httpClient.GetStringAsync(GetInfoApiUrl(url));
        var jObject = JsonNode.Parse(json);
        var totalEpisoes = (int?)jObject["totalEpisodes"]?.AsValue();
        totalEpisoes ??= CalculateEpisodes(jObject);

        var (start, end) = range.Extract(totalEpisoes ?? 0);

        foreach (var item in GetEpisodesArray(jObject))
        {
            var epId = $"{item?["id"]}";
            int number = (int?)item["number"]?.AsValue() ?? ((int)item["episode_number"]?.AsValue());

            if(number < start)
            {
                continue;
            }

            if(number > end)
            {
                break;
            }

            var videoStreamForEpisodes = new VideoStreamsForEpisode()
            {
                Episode = number
            };

            var streams = await _httpClient.GetStringAsync(GetStreamApiUrl(epId));
            var streamsObj = JsonNode.Parse(streams);
            var headers = new Dictionary<string, string>();

            foreach (var header in streamsObj?["headers"]?.AsObject() ?? new JsonObject())
            {
                headers.Add(header.Key, header.Value.ToString());
            }

            foreach (var stream in streamsObj?["sources"]?.AsArray() ?? new JsonArray())
            {
                var quality = stream["quality"].ToString();
                var streamUrl = stream["url"].ToString();
                videoStreamForEpisodes.Qualities.Add(quality, new VideoStream
                {
                    Url = streamUrl,
                    Quality = quality,
                    Headers = headers
                });
            }

            yield return videoStreamForEpisodes;
        }

    }

    private static string GetInfoApiUrl(string id)
    {
        return Config.Provider switch
        {
            "animepahe" => PathInfoParam(id),
            "crunchyroll" => $"https://api.consumet.org/anime/{Config.Provider}/info?id={id}&mediaType={Config.CrunchyrollMediaType}",
            _ => QueryInfoParam(id),
        };
    }

    private static string GetStreamApiUrl(string id)
    {
        return Config.Provider switch
        {
            "animepahe" => PathStreamParam(id),
            _ => QueryStreamParam(id)
        };
    }

    private static string PathInfoParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/info/{id}";
    private static string QueryInfoParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/info?id={id}";
    private static string PathStreamParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/watch/{id}";
    private static string QueryStreamParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/watch?episodeId={id}";
}
