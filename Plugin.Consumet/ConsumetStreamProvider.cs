using System.Text.Json;
using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;

namespace Plugin.Consumet;

public class ConsumetStreamProvider : IStreamProvider, IMultiAudioStreamProvider
{
    private readonly HttpClient _httpClient;

    public ConsumetStreamProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<int> GetNumberOfStreams(string url) => GetNumberOfStreams(url, Config.CrunchyrollStreamType);

    public async Task<int> GetNumberOfStreams(string url, string streamType)
    {
        var requestUrl = GetInfoApiUrl(url);
        var json = await _httpClient.GetStringAsync(requestUrl);
        var jObject = JsonNode.Parse(json);
        var totalEpisoes = (int?)jObject["totalEpisodes"]?.AsValue();
        return totalEpisoes ?? CalculateEpisodes(jObject, streamType);
    }

    private static int CalculateEpisodes(JsonNode jObject, string streamType)
    {
        if (jObject["episodes"] is JsonArray ja)
        {
            var episodesPerPage = (int?)jObject["episodePages"]?.AsValue();
            return ja.Count * (episodesPerPage ?? 1);
        }

        return jObject["episodes"]?[streamType]?.AsArray().Count ?? 0;
    }

    private static JsonArray GetEpisodesArray(JsonNode jObject, string streamType)
    {
        if (jObject["episodes"] is JsonArray ja)
        {
            return ja;
        }

        return jObject["episodes"][streamType]?.AsArray() ?? new JsonArray();
    }

    private static IEnumerable<string> GetStreamTypes(JsonNode jObject)
    {
        if (jObject["episodes"] is JsonArray ja)
        {
            return Enumerable.Empty<string>();
        }

        return jObject["episodes"]?.AsObject()?.Select(x => x.Key);
    }


    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range, string streamType)
    {
        var json = await _httpClient.GetStringAsync(GetInfoApiUrl(url));
        var jObject = JsonNode.Parse(json);
        var totalEpisoes = (int?)jObject["totalEpisodes"]?.AsValue();
        totalEpisoes ??= CalculateEpisodes(jObject, streamType);

        var (start, end) = range.Extract(totalEpisoes ?? 0);

        foreach (var item in GetEpisodesArray(jObject, streamType))
        {
            var epId = $"{item?["id"]}";
            var numberString = item["number"]?.ToString() ?? item["episode_number"]?.ToString();

            if(!int.TryParse(numberString, out int number))
            {
                continue;
            }

            if (number < start)
            {
                continue;
            }

            if (number > end)
            {
                break;
            }

            var videoStreamForEpisodes = new VideoStreamsForEpisode()
            {
                Episode = number,
                StreamTypes = GetStreamTypes(jObject),
            };

            var streams = await _httpClient.GetStringAsync(GetStreamApiUrl(epId));
            var streamsObj = JsonNode.Parse(streams);
            var headers = new Dictionary<string, string>();

            foreach (var header in streamsObj?["headers"]?.AsObject() ?? new JsonObject())
            {
                headers.Add(header.Key, header.Value.ToString());
            }

            var subtitles = streamsObj?["subtitles"]?.AsArray().ToDictionary(x => x["lang"].ToString(), x => x["url"].ToString());

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

                if(subtitles is { Count : >0})
                {
                    videoStreamForEpisodes.AdditionalInformation["subtitles"] = JsonSerializer.Serialize(subtitles);
                }
            }

            yield return videoStreamForEpisodes;
        }
    }

    public IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range) => GetStreams(url, range, Config.CrunchyrollStreamType);

    private static string GetInfoApiUrl(string id)
    {
        return Config.Provider switch
        {
            "animepahe" or "gogoanime" => PathInfoParam(id),
            "crunchyroll" => $"https://api.consumet.org/anime/{Config.Provider}/info?id={id}&mediaType={Config.CrunchyrollMediaType}",
            _ => QueryInfoParam(id),
        };
    }

    private static string GetStreamApiUrl(string id)
    {
        return Config.Provider switch
        {
            "animepahe" or "gogoanime" => PathStreamParam(id),
            _ => QueryStreamParam(id)
        };
    }

    private static string PathInfoParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/info/{id}";
    private static string QueryInfoParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/info?id={id}";
    private static string PathStreamParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/watch/{id}";
    private static string QueryStreamParam(string id) => $"https://api.consumet.org/anime/{Config.Provider}/watch?episodeId={id}";
}
