using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders
{
    public class TenshiMoeStreamProvider : BaseStreamProvider
    {
        const string BASE_URL = "https://tenshi.moe/";

        private readonly Regex _streamRegex = new("src: '(.*)',[\x00-\x7F]*?size: (\\d+)", RegexOptions.Compiled);

        public override async IAsyncEnumerable<HlsStreams> GetStreams(string url)
        {
            var client = await BypassHelper.BypassDDoS(BASE_URL);
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var count = int.Parse(doc.QuerySelector("span.badge").InnerText);

            foreach (var ep in Enumerable.Range(1, count - 1))
            {
                if (await ExtractUrls(client, $"{url}/{ep}") is HlsStreams streams)
                {
                    streams.episode = ep;
                    yield return streams;
                }
            }
        }

        private async Task<HlsStreams?> ExtractUrls(HttpClient client, string url)
        {
            var html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var embedStream = doc.QuerySelector("iframe")?.Attributes["src"].Value;

            if (string.IsNullOrEmpty(embedStream))
            {
                return null;
            }

            html = await client.GetStringAsync(embedStream);

            var streams = new HlsStreams() { streams = new() };
            foreach (Match match in _streamRegex.Matches(html))
            {
                streams.streams.Add(new HlsStreamInfo { quality = match.Groups[2].Value, stream_url = match.Groups[1].Value });
            }

            return streams;
        }
    }
}
