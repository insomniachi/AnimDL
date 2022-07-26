using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AnimDL.Core.Catalog;

internal class YugenAnimeCatalog : ICatalog
{
    const string BASE_URL = "https://yugen.to/";
    private readonly HttpClient _client;
    private readonly ILogger<YugenAnimeCatalog> _logger;

    public YugenAnimeCatalog(HttpClient client, ILogger<YugenAnimeCatalog> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var stream = await _client.GetStreamAsync(BASE_URL + "search/", parameters: new() { ["q"] = query });
        var doc = new HtmlDocument();
        doc.Load(stream);

        var nodes = doc.QuerySelectorAll(".anime-meta");

        if(nodes is null)
        {
            _logger.LogError("no results found");
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
}
