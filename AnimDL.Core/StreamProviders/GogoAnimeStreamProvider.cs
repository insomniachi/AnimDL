using AnimDL.Core.Extractors;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

public class GogoAnimeStreamProvider : BaseStreamProvider
{
    const string BASE_URL = "https://gogoanime.lu/";
    const string BASE_URL_STRIPPED = "https://gogoanime.lu";
    const string EPISODE_LOAD_AJAX = "https://ajax.gogo-load.com/ajax/load-list-episode";
    private readonly Regex _regex;
    private readonly Regex _animeIdRegex = new("<input.*?value=\"([0-9]+)\".*?id=\"movie_id\"", RegexOptions.Compiled);
    private readonly GogoPlayExtractor _extractor;
    public GogoAnimeStreamProvider(GogoPlayExtractor extractor)
    {
        _regex = RegexHelper.SiteBasedRegex(BASE_URL, extraRegex: "(?:(?'episode_anime_slug'[^&?]+)-episode-[\\d-]+|category(?'anime_slug'[^&?]+))");
        _extractor = extractor;
    }

    public override async IAsyncEnumerable<HlsStreams> GetStreams(string url)
    {
        var match = _regex.Match(url);

        if(!match.Success)
        {
            yield break;
        }

        var client = new HttpClient();
        var html = await client.GetStringAsync(url);

        match = _animeIdRegex.Match(html);

        if(!match.Success)
        {
            yield break;
        }

        var contentId = match.Groups[1].Value;

        await foreach (var item in GetEpisodeList(client, contentId))
        {
            var embedPageUrl = await GetEmbedPage(client,item.embedUrl);
            var stream = await _extractor.Extract(embedPageUrl);
        }
    }

    private async IAsyncEnumerable<(int ep, string embedUrl)> GetEpisodeList(HttpClient client, string contentId)
    {
        var url = QueryHelpers.AddQueryString(EPISODE_LOAD_AJAX, new Dictionary<string, string>
        {
            ["ep_start"] = "0",
            ["ep_end"] = "100000",
            ["id"] = contentId
        });

        var html = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
 
        foreach (var item in doc.QuerySelectorAll("a[class=\"\"] , a[class=\"\"]").Reverse())
        {
            var embedUrl = BASE_URL_STRIPPED + item.Attributes["href"].Value.Trim();
            var epMatch = Regex.Match(item.InnerHtml, "EP.*(\\d+)");
            int ep = -1;
            if(epMatch.Success)
            {
                ep = int.Parse(epMatch.Groups[1].Value);
            }

            yield return (ep, embedUrl);
        }
    }

    private static async Task<string> GetEmbedPage(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return $"https:{doc.QuerySelector("iframe").Attributes["src"].Value}";
    }
}
