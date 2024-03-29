﻿using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Plugin.GogoAnime;

public partial class GogoAnimeEpisodesProvider : IAiredEpisodeProvider
{
    public const string AJAX_URL = "https://ajax.gogo-load.com/ajax/page-recent-release.html?page={0}&type=1";
    private readonly HtmlWeb _web = new();

    class GogoAnimeAiredEpisode : AiredEpisode { }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        var doc = await _web.LoadFromWebAsync(string.Format(AJAX_URL, page));

        var nodes = doc.QuerySelectorAll(".items li");
        var list = new List<AiredEpisode>();

        var baseUrl = Config.BaseUrl.TrimEnd('/');
        foreach (var item in nodes)
        {
            var title = item.SelectSingleNode("div/a").Attributes["title"].Value;
            var path = item.SelectSingleNode("div/a").Attributes["href"].Value;
            var url = $"{baseUrl}/{path}";
            var img = item.SelectSingleNode("div/a/img").Attributes["src"].Value;
            list.Add(new GogoAnimeAiredEpisode
            {
                Title = title,
                Url = url,
                Image = img,
                Episode = int.Parse(AiredEpisode.EpisodeRegex().Match(url).Groups[1].Value),
            });
        }

        return list;
    }
}
