﻿using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Plugin.GogoAnime;

public class GogoAnimeCatalog : ICatalog
{
    private readonly HttpClient _client;

    public GogoAnimeCatalog(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<SearchResult> Search(string query)
    {
        var baseUrl = Config.BaseUrl.TrimEnd('/');
        var html = await _client.GetStreamAsync(baseUrl + "/search.html", parameters: new() { ["keyword"] = query });

        var doc = new HtmlDocument();
        doc.Load(html);

        foreach (var item in doc.DocumentNode.QuerySelectorAll("ul.items li"))
        {
            var title = item.QuerySelector(".name a").Attributes["title"].Value;
            var image = item.QuerySelector("img").Attributes["src"].Value;
            var url = item.QuerySelector(".name a").Attributes["href"].Value;
            var year = item.QuerySelector(".released")?.InnerText?.Split(':')?.LastOrDefault()?.Trim() ?? string.Empty;

            yield return new GogoAnimeSearchResult
            {
                Title = title,
                Url = baseUrl + url,
                Image = image,
                Year = year
            };
        }
    }
}
