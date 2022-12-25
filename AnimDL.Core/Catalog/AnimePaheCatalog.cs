using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace AnimDL.Core.Catalog
{
    public class AnimePaheCatalog : ICatalog, IEnableLogger
    {
        const string BASE_URL = "https://animepahe.com/anime/";
        const string API = "https://animepahe.com/api";
        private readonly HttpClient _client;

        public AnimePaheCatalog(HttpClient client)
        {
            _client = client;
        }

        public async IAsyncEnumerable<SearchResult> Search(string query)
        {
            var json = await _client.GetStringAsync(API, parameters: new()
            {
                ["m"] = "search",
                ["q"] = query,
                ["l"] = "8"
            }
            );

            if (string.IsNullOrEmpty(json))
            {
                this.Log().Error("did not get a response");
                yield break;
            }

            var jObject = JsonNode.Parse(json);

            if (jObject is null)
            {
                this.Log().Error("unable to parse {Json}", json);
                yield break;
            }

            if (jObject["data"]?.AsArray() is not { } results)
            {
                this.Log().Error("there is not data");
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
