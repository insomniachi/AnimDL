using System.Text.Json.Nodes;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace Plugin.AnimePahe;

public class AnimePaheCatalog : ICatalog, IEnableLogger
{
    private readonly HttpClient _client;

    public AnimePaheCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var json = await _client.GetStringAsync(Config.BaseUrl.TrimEnd('/') + "/api", parameters: new()
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

        var baseAnimeUrl = Config.BaseUrl.TrimEnd('/') + "/anime";
        foreach (var item in results)
        {
            yield return new AnimePaheSearchResult
            {
                Title = $"{item?["title"]}",
                Image = $"{item?["poster"]}",
                Url = baseAnimeUrl + $"/{item?["session"]}",
                Status = $"{item?["status"]}",
                Season = $"{item?["season"]}",
                Year = $"{item?["year"]}"
            };
        }

    }
}
