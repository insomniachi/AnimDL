using AnimDL.Core.Api;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.AiredEpisodesProvider;

internal class YugenAnimeAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HtmlWeb _web = new();

    class YugenAnimeAiredEpisode : AiredEpisode 
    {
        public DateTime TimeOfAiring { get; set; }
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes()
    {
        var urlBuilder = new UriBuilder(DefaultUrl.Yugen)
        {
            Path = "/latest"
        };

        var doc = await _web.LoadFromWebAsync(urlBuilder.Uri.AbsoluteUri);

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
                TimeOfAiring = DateTime.Parse(time).ToLocalTime()
            });
        }

        return list;
    }
}
