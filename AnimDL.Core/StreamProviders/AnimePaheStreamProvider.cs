using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.Internal;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

public class AnimePaheStreamProvider : BaseStreamProvider
{
    private readonly Regex _playerRegex;
    private readonly Regex _idRegex = new("let id = \"(.+?)\"", RegexOptions.Compiled);
    private readonly Regex _kquiRegex = new("Plyr\\|(.+?)'", RegexOptions.Compiled);
    const string BASE_URL = "https://animepahe.com/";
    const string API = "https://animepahe.com/api";

    public AnimePaheStreamProvider()
    {
        _playerRegex = EncodingHelper.SiteBasedRegex(BASE_URL, extraRegex: "/play/([^?&/]+)");
    }

    public override async IAsyncEnumerable<HlsStreams> GetStreams(string url)
    {
        var match = _playerRegex.Match(url);

        if (match.Success)
        {
            url = $"https://www.animepahe.com/anime/{match.Groups[1].Value}";
        }

        var doc = await Load(url);
        var releaseId = _idRegex.Match(doc.Text).Groups[1].Value;

        var fpd = GetSessionPage("1", releaseId).Result;
        if (fpd.last_page == 1)
        {
            foreach (var item in fpd.data)
            {
                var streams = new HlsStreams
                {
                    episode = item.episode
                };

                var stringUrls = GetStreamUrl(releaseId, item.session).Result;
                foreach (var kv in stringUrls)
                {
                    streams.streams.Add(new HlsStreamInfo
                    {
                        headers = new RequestHeaders { referer = kv.Value.kwik },
                        quality = kv.Key,
                        stream_url = GetStreamFromEmbedUrl(kv.Value.kwik)
                    });
                }

                yield return streams;
            }
        }
        else
        {
            // TODO
        }
    }

    private async Task<AnimePaheEpisodePage> GetSessionPage(string page, string releaseId)
    {
        var query = QueryHelpers.AddQueryString(API, new Dictionary<string, string>
        {
            ["m"] = "release",
            ["id"] = releaseId,
            ["sort"] = "episode_asc",
            ["page"] = page
        });

        using var client = new HttpClient();
        var response = await client.GetAsync(query).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<AnimePaheEpisodePage>(content) ?? new();
    }

    private async Task<Dictionary<string, Quality>> GetStreamUrl(string releaseId, string streamSession)
    {
        var query = QueryHelpers.AddQueryString(API, new Dictionary<string, string>
        {
            ["m"] = "links",
            ["id"] = releaseId,
            ["session"] = streamSession,
            ["p"] = "kwik"
        });

        using var client = new HttpClient();
        var response = await client.GetAsync(query).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var result = JsonSerializer.Deserialize<AnimePaheQualityModel>(content) ?? new();
        return result.GetQualities();
    }

    private string GetStreamFromEmbedUrl(string kwik)
    {
        var web = new HtmlWeb();
        web.PreRequest += (request) =>
        {
            request.Headers.Add("referer", kwik);
            return true;
        };

        var doc = web.Load(kwik);
        var match = _kquiRegex.Match(doc.Text);

        if (!match.Success)
        {
            return string.Empty;
        }

        var values = match.Groups[1].Value.Split("|").Reverse().ToList();
        return string.Format("{0}://{1}-{2}.{3}.{4}.{5}/{6}/{7}/{8}/{9}.{10}",
            values[0],
            values[1],
            values[2],
            values[3],
            values[4],
            values[5],
            values[6],
            values[7],
            values[8],
            values[9],
            values[10]);
    }
}
