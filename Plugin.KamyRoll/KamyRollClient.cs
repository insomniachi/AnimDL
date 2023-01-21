using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Plugin.KamyRoll;

public class KamyRollClient : ICatalog, IStreamProvider, IProvider
{
    private static readonly HttpClient _httpClient = new();
    private static readonly string _api = "https://api.kamyroll.tech";

    public ICatalog Catalog => this;
    public IStreamProvider StreamProvider => this;
    public IAiredEpisodeProvider AiredEpisodesProvider => null!;

    public static async Task AuthenticateIfNot()
    {
        if(!string.IsNullOrEmpty(Config.AccessToken))
        {
            return;
        }

        var url = QueryHelpers.AddQueryString(_api + "/auth/v1/token", new Dictionary<string, string>
        {
            ["device_id"] = "whatvalueshouldbeforweb",
            ["device_type"] = "com.service.data",
            ["access_token"] = "HMbQeThWmZq4t7w"
        });
        var stream = await HttpHelper.Client.GetStreamAsync(url); 
        var token = JsonSerializer.Deserialize(stream, KamyRollTokenSerializationContext.Default.KamyRollToken);
        Config.AccessToken = token.AccessToken;
        SetAccessToken();
    }

    public static void SetAccessToken()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Config.AccessToken);
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        await AuthenticateIfNot();

        var result = await MakeRequest(_api + "/content/v1/search", new Dictionary<string, string>
        {
            ["query"] = query,
            ["limit"] = Config.SearchLimit
        });

        var jObject = JsonNode.Parse(result);

        foreach (var item in jObject?["items"]?.AsArray()?.SelectMany(x => x?["items"]?.AsArray() ?? new JsonArray()) ?? new JsonArray())
        {
            var title = $"{item?["title"]}";
            var image = $"{item?["images"]?["poster_tall"]?.AsArray().LastOrDefault()?["source"]}";
            var url = $"{item?["id"]}";

            yield return new KamyrollSearchResult
            {
                Title = title,
                Image = image,
                Url = url
            };
        }
    }

    public async Task<int> GetNumberOfStreams(string url)
    {
        await AuthenticateIfNot();
        var json = await MakeRequest(_api + "/content/v1/media", new Dictionary<string, string> { ["id"] = url });
        var jObject = JsonNode.Parse(json);
        _ = int.TryParse(jObject?["episode_count"]?.ToString(), out int epCount);
        var isDubbed = (bool)jObject?["is_dubbed"]?.AsValue();

        return isDubbed ? epCount/2 : epCount;
    }

    private static bool IsCorrectStreamType(JsonNode epObject)
    {
        bool isSub = (bool)epObject["is_subbed"].AsValue();
        return Config.StreamType == "sub"
            ? isSub
            : !isSub;
    }

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        await AuthenticateIfNot();
        var json = await MakeRequest(_api + "/content/v1/media", new Dictionary<string, string> { ["id"] = url });
        _ = int.TryParse(JsonNode.Parse(json)?["episode_count"]?.ToString(), out int epCount);
        var (start, end) = range.Extract(epCount);

        json = await MakeRequest(_api + "/content/v1/seasons", new Dictionary<string, string>
        {
            ["id"] = url
        });

        foreach (var item in JsonNode.Parse(json)?["items"]?.AsArray()?.SelectMany(x => x?["episodes"]?.AsArray()) ?? new JsonArray())
        {
            if(!IsCorrectStreamType(item))
            {
                continue;
            }

            if (!int.TryParse($"{item?["sequence_number"]}", out int ep))
            {
                continue;
            }

            if(ep < start)
            {
                continue;
            }

            if(ep > end)
            {
                break;
            }

            var mediaId = $"{item?["id"]}";

            json = await MakeRequest(_api + "/videos/v1/streams", new Dictionary<string, string>
            {
                ["id"] = mediaId
            });

            var streamUrl = JsonNode.Parse(json)?["streams"]?.AsArray()?.FirstOrDefault(x => $"{x?["hardsub_locale"]}" == "en-US")?["url"]?.ToString() ?? string.Empty;

            if(string.IsNullOrEmpty(streamUrl))
            {
                continue;
            }

            yield return new VideoStreamsForEpisode
            {
                Episode = ep,
                Qualities = new Dictionary<string, VideoStream>
                {
                    ["default"] = new VideoStream
                    {
                        Quality = "default",
                        Url = streamUrl,
                    }
                },
                AdditionalInformation = new Dictionary<string, string>
                {
                    ["title-en"] = $"{item?["title"]}",
                    ["description"] = $"{item?["description"]}"
                }
            };
        }
    }

    private static async Task<string> MakeRequest(string endpoint, Dictionary<string,string> @params = null)
    {
        var reqParams = new Dictionary<string, string>
        {
            ["channel_id"] = Config.Channel,
            ["locale"] = Config.Locale
        };

        if(@params is not null)
        {
            foreach (var item in @params)
            {
                reqParams.Add(item.Key, item.Value);
            }
        }

        var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString(endpoint, reqParams));
        return await response.Content.ReadAsStringAsync();
    }
}

public class KamyRollToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
}

[JsonSerializable(typeof(KamyRollToken))]
public partial class KamyRollTokenSerializationContext : JsonSerializerContext { }
