using System.Text.RegularExpressions;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Splat;

namespace AnimDL.Core.Catalog;

internal partial class YugenAnimeCatalog : ICatalog, ICanParseMalId, IEnableLogger
{
    const string BASE_URL = "https://yugen.to/";
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
        var stream = await _client.GetStreamAsync(BASE_URL + "search/", parameters: new() { ["q"] = query });
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
            yield return new SearchResult
            {
                Url = BASE_URL.TrimEnd('/') + node.Attributes["href"].Value,
                Title = node.Attributes["title"].Value
            };
        }
    }

    [GeneratedRegex("\"mal_id\":(\\d+)")]
    private static partial Regex YugenMalIdRegex();
}
