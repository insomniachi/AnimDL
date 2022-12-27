﻿using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace AnimDL.Core.StreamProviders;

internal class AnimeOutStreamProvider : BaseStreamProvider
{
    private readonly Dictionary<string, string> _publicDomains = new()
    {
        ["nimbus"] = "pub9",
        ["chomusuke"] = "pub8",
        ["chunchunmaru"] = "pub7",
        ["ains"] = "pub6",
        ["yotsuba"] = "pub5",
        ["slaine"] = "pub4",
        ["jibril"] = "pub3",
        ["sv1"] = "pub2",
        ["sv4"] = "pub2",
        ["download"] = "pub1"
    };

    public AnimeOutStreamProvider(HttpClient client) : base(client)
    {
    }

    public override Task<int> GetNumberOfStreams(string url)
    {
        throw new NotImplementedException();
    }

    public async override IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range stream)
    {
        var doc = new HtmlDocument();
        var htmlStream = await _client.CfGetStreamAsync(url);
        doc.Load(htmlStream);

        foreach (var item in doc.QuerySelectorAll(".article-content a[href$=\"mkv\"]").Where(x => x?.InnerText?.Contains("Download") == true))
        {
            var streamUrl = GetStreamUrl(new Uri(item.Attributes["href"].Value));
            var contents = ParseFromContent(item.InnerText);
            var epStream = new VideoStreamsForEpisode();

            if (contents.TryGetValue("ep", out string? value))
            {
                epStream.Episode = int.Parse(value);
            }

            var quality = "default";
            if (contents.ContainsKey("quality"))
            {
                quality = contents["quality"];
            }

            epStream.Qualities.Add(quality, new VideoStream
            {
                Quality = quality,
                Url = streamUrl
            });

            yield return epStream;
        }
    }

    private string GetStreamUrl(Uri uri)
    {
        var host_prefix = uri.Host.Split(".", 2);

        if (_publicDomains.TryGetValue(host_prefix[0], out string? value))
        {
            var uriBuilder = new UriBuilder(uri)
            {
                Host = $"{value}.{host_prefix[1]}"
            };
            uri = uriBuilder.Uri;
        }

        return uri.ToString();
    }

    private Dictionary<string, string> ParseFromContent(string content)
    {
        var result = new Dictionary<string, string>();
        var anitomyResult = AnitomySharp.AnitomySharp.Parse(content);

        var res = anitomyResult.FirstOrDefault(x => x.Category == AnitomySharp.Element.ElementCategory.ElementVideoResolution)?.Value;

        if (!string.IsNullOrEmpty(res))
        {
            var quality = res.TrimEnd('p');
            if (int.TryParse(quality, out _))
            {
                result.Add("quality", quality);
            }
        }

        var ep = anitomyResult.FirstOrDefault(x => x.Category == AnitomySharp.Element.ElementCategory.ElementEpisodeNumber)?.Value;

        if (!string.IsNullOrEmpty(ep))
        {
            result.Add("ep", ep);
        }

        return result;
    }


}
