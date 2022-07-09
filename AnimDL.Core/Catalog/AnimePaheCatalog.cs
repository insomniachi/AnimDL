using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Catalog
{
    public class AnimePaheCatalog : ICatalog
    {
        const string BASE_URL = "https://animepahe.com/anime/";
        const string API = "https://animepahe.com/api";
        private readonly HttpClient _client;
        private readonly ILogger<AnimeOutCatalog> _logger;

        public AnimePaheCatalog(HttpClient client, ILogger<AnimeOutCatalog> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async IAsyncEnumerable<SearchResult> Search(string query)
        {
            var json = await _client.GetStringAsync(API, parameters: new()
            {
                ["m"] = "search",
                ["q"] = query,
                ["l"] = "8" }
            );

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogError("did not get a response");
                yield break;
            }

            var jObject = JsonNode.Parse(json);

            if (jObject is null)
            {
                _logger.LogError("unable to parse {Json}", json);
                yield break;
            }

            if (jObject["data"]?.AsArray() is not { } results)
            {
                _logger.LogError("there is not data");
                yield break;
            }

            foreach (var item in results)
            {
                yield return new AnimePaheSearchResult
                {
                    Title = item!["title"]!.ToString(),
                    Image = item!["poster"]!.ToString(),
                    Year = int.Parse(item!["year"]!.ToString()),
                    Url = BASE_URL + item!["session"]!.ToString(),
                    Status = item!["status"]!.ToString(),
                    Season = item!["season"]!.ToString()
                };
            }

        }
    }
}
