using System.Net;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Splat;

namespace AnimDL.Core.StreamProviders;

[Obsolete("Rebranded to Marin")]
internal partial class TenshiMoeStreamProvider : BaseStreamProvider
{
    public readonly string BASE_URL = DefaultUrl.Tenshi;
    private readonly HttpClient _cookieSavingClient;
    private readonly CookieContainer _cookieContainer = new();

    public TenshiMoeStreamProvider(HttpClient client) : base(client)
    {
        var clientHandler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer
        };
        _cookieSavingClient = new HttpClient(clientHandler);
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        await _cookieSavingClient.BypassDDoS(BASE_URL);
        var html = await _cookieSavingClient.GetStreamAsync(url);

        var doc = new HtmlDocument();
        doc.Load(html);

        return int.Parse(doc.QuerySelector("span.badge").InnerText);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        await _cookieSavingClient.BypassDDoS(BASE_URL);
        var html = await _cookieSavingClient.GetStreamAsync(url);

        var doc = new HtmlDocument();
        doc.Load(html);

        var count = int.Parse(doc.QuerySelector("span.badge").InnerText);
        (int start, int end) = range.Extract(count);

        foreach (var ep in Enumerable.Range(start, end - start + 1))
        {
            if (await ExtractUrls($"{url}/{ep}") is VideoStreamsForEpisode streamForEp)
            {
                streamForEp.Episode = ep;
                yield return streamForEp;
            }
        }
    }

    private async Task<VideoStreamsForEpisode?> ExtractUrls(string url)
    {
        var htmlStream = await _cookieSavingClient.GetStreamAsync(url);

        var doc = new HtmlDocument();
        doc.Load(htmlStream);
        var embedStream = doc.QuerySelector("iframe")?.Attributes["src"].Value;

        if (string.IsNullOrEmpty(embedStream))
        {
            this.Log().Error("unable to find embed stream");
            return null;
        }

        var html = await _cookieSavingClient.GetStringAsync(embedStream);
        var cookieCollection = _cookieContainer.GetCookies(new Uri(embedStream));
        var ddg1 = string.Empty;
        var ddg2 = string.Empty;
        foreach (var item in cookieCollection.Cast<Cookie>())
        {
            switch (item.Name)
            {
                case "__ddg1_": ddg1 = item.Value; break;
                case "__ddg2_": ddg2 = item.Value; break;
            }
        }
        var cookie = $"__ddg1={ddg1}; __ddg2={ddg2}; __ddg2_={ddg2}";

        var streamsForEp = new VideoStreamsForEpisode();
        foreach (var match in StreamRegex().Matches(html).Cast<Match>())
        {
            var quality = match.Groups[2].Value;

            streamsForEp.Qualities.Add(quality, new VideoStream
            {
                Quality = quality,
                Url = match.Groups[1].Value,
                Headers = new Dictionary<string, string>
                {
                    [Headers.Cookie] = cookie
                }
            });
        }

        return streamsForEp;
    }

    [GeneratedRegex(@"<source src=""(.+?)"" .+? size=""([0-9]+)"">", RegexOptions.Compiled)]
    private static partial Regex StreamRegex();
}
