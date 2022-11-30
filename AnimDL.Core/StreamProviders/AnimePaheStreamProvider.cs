using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.Internal;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

internal partial class AnimePaheStreamProvider : BaseStreamProvider
{
    [GeneratedRegex("let id = \"(.+?)\"", RegexOptions.Compiled)]
    private static partial Regex IdRegex();

    [GeneratedRegex("Plyr\\|(.+?)'", RegexOptions.Compiled)]
    private static partial Regex KwiwRegex();

    public readonly string API = Constants.AnimePahe + "api";
    private readonly ILogger<AnimePaheStreamProvider> _logger;

    public AnimePaheStreamProvider(ILogger<AnimePaheStreamProvider> logger, HttpClient client) : base(client)
    {
        _logger = logger;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url);
        var releaseId = IdRegex().Match(doc.Text).Groups[1].Value;

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
                        Headers = new Dictionary<string, string> { [Headers.Referer] = kv.Value.kwik },
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
        var content = await _client.GetStreamAsync(API, parameters: new() 
        {
            ["m"] = "release",
            ["id"] = releaseId,
            ["sort"] = "episode_asc",
            ["page"] = page
        });

        return await JsonSerializer.DeserializeAsync<AnimePaheEpisodePage>(content) ?? new();
    }

    private async Task<Dictionary<string, Quality>> GetStreamUrl(string releaseId, string streamSession)
    {
        var content = await _client.GetStreamAsync(API, parameters: new()
        {
            ["m"] = "links",
            ["id"] = releaseId,
            ["session"] = streamSession,
            ["p"] = "kwik"
        });

        var result = await JsonSerializer.DeserializeAsync<AnimePaheQualityModel>(content) ?? new();
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
        var match = KwiwRegex().Match(doc.Text);

        if (!match.Success)
        {
            return string.Empty;
        }

        var p = match.Groups[1].Value.Split("|").Reverse().ToList();

        return $"{p[0]}://{p[1]}-{p[2]}.{p[3]}.{p[4]}.{p[5]}/{p[6]}/{p[7]}/{p[8]}/{p[9]}.{p[10]}";
    }
}
