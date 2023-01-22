using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
        return totalEpisoes ?? ((jObject["episodes"]?.AsArray() ?? new JsonArray()).Count) * (int)jObject["episodePages"]?.AsValue();
    }

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var json = await _httpClient.GetStringAsync(GetInfoApiUrl(url));
        var jObject = JsonNode.Parse(json);
        var totalEpisoes = (int?)jObject["totalEpisodes"]?.AsValue();
        totalEpisoes ??= ((jObject["episodes"]?.AsArray() ?? new JsonArray()).Count) * (int)jObject["episodePages"]?.AsValue();

        var (start, end) = range.Extract(totalEpisoes ?? 0);

        foreach (var item in jObject["episodes"]?.AsArray() ?? new JsonArray())
        {
            var epId = $"{item?["id"]}";
            int number = (int)item["number"].AsValue();

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
            "animepahe" => $"https://api.consumet.org/anime/animepahe/info/{id}",
            "zoro" => $"https://api.consumet.org/anime/zoro/info?id={id}",
            _ => throw new NotSupportedException()
        };
    }

    private static string GetStreamApiUrl(string id)
    {
        return Config.Provider switch
        {
            "animepahe" => $"https://api.consumet.org/anime/animepahe/watch/{id}",
            "zoro" => $"https://api.consumet.org/anime/zoro/watch?episodeId={id}",
            _ => throw new NotSupportedException()
        };
    }
}
