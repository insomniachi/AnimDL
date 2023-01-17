using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.SearchResults;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core
{
    public class KamyRollClient
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _api = "https://api.kamyroll.tech";


        public KamyRollClient()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ServiceCollectionExtensions.USER_AGENT);
        }

        public async Task<KamyRollToken> Authenticate()
        {
            var url = QueryHelpers.AddQueryString(_api + "/auth/v1/token", new Dictionary<string, string>
            {
                ["device_id"] = "whatvalueshouldbeforweb",
                ["device_type"] = "com.service.data",
                ["access_token"] = "HMbQeThWmZq4t7w"
            });
            var stream = await _httpClient.GetStreamAsync(url); 
            var token = JsonSerializer.Deserialize(stream, KamyRollTokenSerializationContext.Default.KamyRollToken);
            SetAccessToken(token!.AccessToken);
            return token!;
        }

        public async IAsyncEnumerable<SearchResult> Search(string query)
        {
            var result = await MakeRequest(_api + "/content/v1/search", new Dictionary<string, string>
            {
                ["query"] = query,
                ["limit"] = "5"
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

        public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
        {
            var json = await MakeRequest(_api + "/content/v1/media", new Dictionary<string, string> { ["id"] = url });

            int.TryParse(JsonNode.Parse(json)?["episode_count"]?.ToString(), out int epCount);
            var (start, end) = range.Extract(epCount);

            json = await MakeRequest(_api + "/content/v1/seasons", new Dictionary<string, string>
            {
                ["id"] = url
            });

            foreach (var item in JsonNode.Parse(json)?["items"]?.AsArray().First()?["episodes"]?.AsArray() ?? new JsonArray())
            {
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
                        ["title-en"] = $"{item?["title"]}"
                    }
                };
            }
        }

        private async Task<string> MakeRequest(string endpoint, Dictionary<string,string>? @params = null)
        {
            var reqParams = new Dictionary<string, string>
            {
                ["channel_id"] = "crunchyroll",
                ["locale"] = "en-US"
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

        public void SetAccessToken(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
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
}
