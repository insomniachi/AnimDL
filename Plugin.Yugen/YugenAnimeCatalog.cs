using System.Text.RegularExpressions;
using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Splat;

namespace Plugin.Yugen;

public partial class YugenAnimeCatalog : ICatalog, ICanParseMalId, IEnableLogger
{
    private readonly HttpClient _client;
    
    public YugenAnimeCatalog(HttpClient client)
    {
        _client = client;
    }

    public async Task<long> GetMalId(string url)
    {
        try
        {
            var html = await _client.GetStringAsync(url);
            var match = YugenMalIdRegex().Match(html);
            return match.Success ? long.Parse(match.Groups[1].Value) : 0;
        }
        catch (Exception ex)
        {
            this.Log().Error(ex);
            return 0;
        }
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var baseUrl = Config.BaseUrl.TrimEnd('/');
        var stream = await _client.GetStreamAsync(baseUrl + "/search", parameters: new() { ["q"] = query });
        var doc = new HtmlDocument();
        doc.Load(stream);

        var nodes = doc.QuerySelectorAll(".anime-meta");

        if (nodes is null)
        {
            this.Log().Error("no results found");
            yield break;
        }

        foreach (var node in nodes)
        {
            var title = node.Attributes["title"].Value;
            var url = baseUrl + node.Attributes["href"].Value;
            var image = node.QuerySelector("img").Attributes["data-src"].Value;
            var season = node.QuerySelector(".anime-details span").InnerText.Split(" ");
            var rating = node.QuerySelector(".option")?.ChildNodes[1]?.InnerText?.Trim() ?? string.Empty;
            
            yield return new YugenAnimeSearchResult
            {
                Url = url,
                Title = title,
                Image = image,
                Season = season[0],
                Rating = rating,
                Year = season.Length >= 2 ? season[1] : string.Empty,
            };
        }
    }

    [GeneratedRegex("\"mal_id\":(\\d+)")]
    private static partial Regex YugenMalIdRegex();
}
