using System.Text.Json.Nodes;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace AnimDL.Core.Catalog
{
    public class AnimePaheCatalog : ICatalog, IEnableLogger
    {
        private readonly string _baseAnimeUrl;
        private readonly string _api;
        private readonly HttpClient _client;

        public AnimePaheCatalog(HttpClient client)
        {
            var urlBuilder = new UriBuilder(DefaultUrl.AnimePahe);
            urlBuilder.Path = "/anime";
            _baseAnimeUrl = urlBuilder.Uri.AbsoluteUri;
            urlBuilder.Path = "/api";
            _api = urlBuilder.Uri.AbsoluteUri;

            _client = client;
        }

        public async IAsyncEnumerable<SearchResult> Search(string query)
        {
            var json = await _client.GetStringAsync(_api, parameters: new()
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
                    Url = _baseAnimeUrl + "/" + item!["session"]!.ToString(),
                    Status = item!["status"]!.ToString(),
                    Season = item!["season"]!.ToString()
                };
            }

        }
    }
}
