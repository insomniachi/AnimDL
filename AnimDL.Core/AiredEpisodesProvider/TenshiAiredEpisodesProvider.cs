using System.Net;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.AiredEpisodesProvider;

[Obsolete("Rebranded to Marin")]
internal class TenshiAiredEpisodesProvider : IAiredEpisodeProvider
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer = new();

    class TenshiAiredEpisode : AiredEpisode, IHaveHumanizedCreatedTime
    {
        public string CreatedAtHumanized { get; set; } = string.Empty;
    }

    public TenshiAiredEpisodesProvider()
    {
        var clientHandler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        _client = new HttpClient(clientHandler);
    }

    public async Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1)
    {
        await _client.BypassDDoS(DefaultUrl.Tenshi);
        var cookie = string.Join(";", _cookieContainer.GetCookies(new Uri(DefaultUrl.Tenshi)).Cast<Cookie>().Select(item => $"{item.Name}={item.Value}"));
        cookie += ";loop-view=thumb";

        var stream = await _client.GetStreamAsync(DefaultUrl.Tenshi.TrimEnd('/') + $"/episode?s=rel-d&page={page}", headers: new Dictionary<string, string>
        {
            [Headers.Cookie] = cookie
        });

        var doc = new HtmlDocument();
        doc.Load(stream);

        var result = new List<AiredEpisode>();
        foreach (var item in doc.QuerySelectorAll("ul.episode-loop li"))
        {
            var url = item.QuerySelector(".film-grain").Attributes["href"].Value;
            var image = item.QuerySelector(".image").Attributes["src"].Value;
            var title = item.QuerySelector(".title").InnerText.Trim();
            var time = item.QuerySelector(".episode-date").Attributes["title"].Value;
            var episode = AiredEpisode.EpisodeRegex().Match(url).Groups[1].Value;
            var animeUrl = url.Replace(episode, string.Empty).TrimEnd('/');

            result.Add(new TenshiAiredEpisode
            {
                Url = animeUrl,
                Image = image,
                Title = title,
                CreatedAtHumanized = time,
                Episode = int.Parse(episode)
            });
        }

        return result;
    }
}
