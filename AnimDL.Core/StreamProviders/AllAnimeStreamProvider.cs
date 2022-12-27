using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Splat;

namespace AnimDL.Core.StreamProviders;

internal partial class AllAnimeStreamProvider : BaseStreamProvider
{
    class GetVersionResponse
    {
        public string data { get; set; } = string.Empty;
        public string episodeIframeHead { get; set; } = string.Empty;
    }

    class EpisodeDetails
    {
        public List<string> sub { get; set; } = new();
        public List<string> dub { get; set; } = new();
        public List<string> raw { get; set; } = new();
    }

    class StreamLink
    {
        public string link { get; set; } = string.Empty;
        public bool hls { get; set; }
        public string resolutionStr { get; set; } = string.Empty;
        public string src { get; set; } = string.Empty;
    }

    [GeneratedRegex(@"\\""availableEpisodesDetail\\"":({.+?})")]
    private static partial Regex EpisodesRegex();

    [GeneratedRegex(@"<iframe id=""episode-frame"" .+? src=""(.+?)""")]
    private static partial Regex SourceEmbedRegex();

    [GeneratedRegex(@"^#EXT-X-STREAM-INF:.*?RESOLUTION=\d+x(?'resolution'\d+).*?\n(?'url'.+?)$", RegexOptions.Multiline)]
    private static partial Regex VrvResponseRegex();

    [GeneratedRegex(@"https://.+?/(?'base'.+?/),(?'resolutions'(?:\d+p,)+)")]
    private static partial Regex WixMpUrlRegex();

    public AllAnimeStreamProvider(HttpClient client) : base(client)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(ServiceCollectionExtensions.USER_AGENT);
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var html = await _client.GetStringAsync(url);

        var match = EpisodesRegex().Match(html);

        if (!match.Success)
        {
            this.Log().Error("availableEpisodesDetail not found");
        }

        var episodesDetail = JsonSerializer.Deserialize<EpisodeDetails>(match.Groups[1].Value.Replace("\\\"", "\""));
        var sorted = GetEpisodes(episodesDetail!).OrderBy(x => x.Length).ThenBy(x => x).ToList();
        var total = int.Parse(sorted.LastOrDefault(x => int.TryParse(x, out int e))!);

        return total;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var uriBuilder = new UriBuilder(DefaultUrl.AllAnime);
        //uriBuilder.Path = "/getVersion";
        //var versionResponse = await _client.GetFromJsonAsync<GetVersionResponse>(uriBuilder.Uri.AbsoluteUri);

        var html = await _client.GetStringAsync(url);

        var match = EpisodesRegex().Match(html);

        if(!match.Success)
        {
            this.Log().Error("availableEpisodesDetail not found");
        }

        var episodesDetail = JsonSerializer.Deserialize<EpisodeDetails>(match.Groups[1].Value.Replace("\\\"", "\""));
        var sorted = GetEpisodes(episodesDetail!).OrderBy(x => x.Length).ThenBy(x => x).ToList();
        var total = int.Parse(sorted.LastOrDefault(x => int.TryParse(x, out int e))!);
        var (start, end) = range.Extract(total);
        foreach (var ep in sorted)
        {
            if(int.TryParse(ep, out int e))
            {
                if(e < start)
                {
                    continue;
                }
                else if(e > end)
                {
                    break;
                }
            }

            var epUrl = $"{url}/episodes/{GlobalConfig.AudioType}/{ep}";
            var content = await _client.GetStringAsync(epUrl);

            var hasEmbedMatch = SourceEmbedRegex().Match(content);

            if(hasEmbedMatch.Success)
            {
                uriBuilder = new UriBuilder(hasEmbedMatch.Groups[1].Value);
                uriBuilder.Path += ".json";
                var directUrl = uriBuilder.Uri.AbsoluteUri;
                var stream = await Extract(directUrl);

                if(stream is { })
                {
                    stream.Episode = e;
                    stream.EpisodeString = ep;
                    yield return stream;
                }
            }
            else
            {
                this.Log().Error("not implemented");
            }
        }
    }

    private async Task<VideoStreamsForEpisode?> Extract(string url)
    {
        var json = await _client.GetStringAsync(url);
        var jObject = JsonNode.Parse(json);
        var links = jObject!["links"]!.Deserialize<List<StreamLink>>()!;

        var uri = new Uri(links[0].link);
        return uri.Host switch
        {
            "v.vrv.co" => await VrvUnpack(uri),
            "repackager.wixmp.com" => WixMpUnpack(uri),
            _ => new VideoStreamsForEpisode
            {
                Qualities = new Dictionary<string, VideoStream>
                {
                    ["default"] = new VideoStream
                    {
                        Url = uri.AbsoluteUri,
                        Quality = "default"
                    }
                }
            }
        };
    }

    private async Task<VideoStreamsForEpisode> VrvUnpack(Uri uri)
    {
        var response = await _client.GetStringAsync(uri);

        var result = new VideoStreamsForEpisode();
        foreach (var match in VrvResponseRegex().Matches(response).Cast<Match>())
        {
            var streamUrl = match.Groups["url"].Value.Replace("/index-v1-a1.m3u8", "");
            var quality = match.Groups["resolution"].Value;
            result.Qualities.Add(quality, new VideoStream { Quality = quality, Url = streamUrl });
        }

        return result;
    }

    private VideoStreamsForEpisode? WixMpUnpack(Uri uri)
    {
        var match = WixMpUrlRegex().Match(uri.AbsoluteUri);
        
        if(!match.Success)
        {
            return null;
        }

        var baseUrl = match.Groups["base"].Value;

        var result = new VideoStreamsForEpisode();

        foreach (var resolution in match.Groups["resolutions"].Value.Split(",", StringSplitOptions.RemoveEmptyEntries))
        {
            result.Qualities.Add(resolution, new VideoStream { Url = $"https://{baseUrl}{resolution}/mp4/file.mp4", Quality = resolution });
        }

        return result;
    }

    private static List<string> GetEpisodes(EpisodeDetails episodeDetails)
    {
        return GlobalConfig.AudioType switch
        {
            "sub" => episodeDetails.sub,
            "dub" => episodeDetails.dub,
            "raw" => episodeDetails.raw,
            _ => throw new NotSupportedException()
        };
    }
}
