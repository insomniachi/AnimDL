using AnimDL.Core.Api;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json.Nodes;

namespace AnimDL.Core.Catalog
{
    public class AnimePaheCatalog : ICatalog
    {
        const string BASE_URL = "https://animepahe.com/anime/";
        const string API = "https://animepahe.com/api";
        public async IAsyncEnumerable<SearchResult> Search(string query)
        {
            var request = QueryHelpers.AddQueryString(API, new Dictionary<string, string> { ["m"] = "search", ["q"] = query, ["l"] = "8" });
            using var client = new HttpClient();
            var result = await client.GetAsync(request);
            var json = await result.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(json))
            {
                yield break;
            }

            var jObject = JsonNode.Parse(json);

            if (jObject is null)
            {
                yield break;
            }

            var results = jObject["data"]!.AsArray();

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
