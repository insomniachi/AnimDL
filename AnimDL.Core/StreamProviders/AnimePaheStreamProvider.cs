using AnimDL.Core.Models;
using AnimDL.Core.Models.Internal;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

internal class AnimePaheStreamProvider : BaseStreamProvider
{
    private readonly Regex _idRegex = new("let id = \"(.+?)\"", RegexOptions.Compiled);
    private readonly Regex _kwikRegex = new("Plyr\\|(.+?)'", RegexOptions.Compiled);
    readonly string API = Constants.AnimePahe + "api";
    private readonly ILogger<AnimePaheStreamProvider> _logger;

    public AnimePaheStreamProvider(ILogger<AnimePaheStreamProvider> logger)
    {
        _logger = logger;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url)
    {
        var doc = await Load(url);
        var releaseId = _idRegex.Match(doc.Text).Groups[1].Value;

        var fpd = GetSessionPage("1", releaseId).Result;
        if (fpd.last_page == 1)
        {
            foreach (var item in fpd.data)
            {
                var streams = new VideoStreamsForEpisode
                {
                    Episode = item.episode
                };

                var stringUrls = await GetStreamUrl(releaseId, item.session);
                
                foreach (var kv in stringUrls)
                {
                    streams.Qualities.Add(kv.Key, new VideoStream
                    {
                        Quality = kv.Key,
                        Headers = new Dictionary<string, string> { ["referer"] = kv.Value.kwik },
                        Url = GetStreamFromEmbedUrl(kv.Value.kwik)
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
        var content = await client.GetStringAsync(query);
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
        var content = await client.GetStringAsync(query);
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
        var match = _kwikRegex.Match(doc.Text);

        if (!match.Success)
        {
            return string.Empty;
        }

        var p = match.Groups[1].Value.Split("|").Reverse().ToList();

        return $"{p[0]}://{p[1]}-{p[2]}.{p[3]}.{p[4]}.{p[5]}/{p[6]}/{p[7]}/{p[8]}/{p[9]}.{p[10]}";
    }
}
