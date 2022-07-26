using System.Text.RegularExpressions;
using AnimDL.Core.Extractors;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.StreamProviders;

internal class YugenAnimeStreamProvider : BaseStreamProvider
{
    const string BASE_URL = "https://yugen.to/";
    public YugenAnimeStreamProvider(HttpClient client) : base(client)
    {
    }

    public override Task<int> GetNumberOfStreams(string url)
    {
        return base.GetNumberOfStreams(url);
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url + "watch");

        var epSection = doc.QuerySelectorAll(".data")
                           .Select(x => x.InnerText)
                           .Where(x => x?.Contains("Episodes") == true)
                           .First();

        var match = Regex.Match(epSection, @"(\d+)");

        if(!match.Success)
        {
            yield break;
        }

        var totalEpisodes = int.Parse(match.Groups[1].Value);
        var (start, end) = range.Extract(totalEpisodes);

        var uri = new UriBuilder(url).Uri;
        var length = uri.Segments.Length;
        var slug = uri.Segments[length - 1].TrimEnd('/');
        var number = uri.Segments[length - 2].TrimEnd('/');

        for (int i = start; i <= end; i++)
        {
            var epUrl = $"{BASE_URL}watch/{number}/{slug}/{i}/";
            doc = await Load(epUrl);
            var text = doc.Text;
            var iframeUrl = doc.QuerySelector("iframe").Attributes["src"].Value;

            var iframPage = await _client.GetStringAsync(iframeUrl);
        }

        yield break;
    }
}
