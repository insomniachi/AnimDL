using AnimDL.Core;
using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.AspNetCore.WebUtilities;

namespace Plugin.Yugen;

public class YugenAnimeAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HtmlWeb _web = new();

    class YugenAnimeAiredEpisode : AiredEpisode, IHaveCreatedTime
    {
        public DateTime CreatedAt { get; set; }
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        var urlBuilder = new UriBuilder(DefaultUrl.Yugen)
        {
            Path = $"/latest",
        };

        var doc = await _web.LoadFromWebAsync(QueryHelpers.AddQueryString(urlBuilder.Uri.AbsoluteUri, new Dictionary<string, string>() { ["page"] = page.ToString() }));

        var nodes = doc.QuerySelectorAll(".ep-card");
        var list = new List<AiredEpisode>();

        foreach (var item in nodes)
        {
            var title = item.QuerySelector(".ep-origin-name").InnerText.Trim();
            urlBuilder.Path = item.QuerySelector(".ep-thumbnail").Attributes["href"].Value;
            var url = urlBuilder.Uri.AbsoluteUri;
            var img = item.QuerySelector("img").Attributes["data-src"].Value;
            var time = item.QuerySelector("time").Attributes["datetime"].Value;
            list.Add(new YugenAnimeAiredEpisode
            {
                Title = title,
                Url = url,
                Image = img,
                CreatedAt = DateTime.Parse(time).ToLocalTime(),
                Episode = int.Parse(AiredEpisode.EpisodeRegex().Match(url).Groups[1].Value),
            });
        }

        return list;
    }
}
