using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using AnimDL.Core.Api;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.StreamProviders;
using Splat;

namespace Plugin.AllAnime;

public partial class AllAnimeStreamProvider : BaseStreamProvider, IMultiAudioStreamProvider
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
        public int resolution { get; set; } = 0;
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

    [GeneratedRegex("sourceUrl[:=]\"(?<url>.+?)\"[;,](?:.+?\\.)?priority[:=](?<priority>.+?)[;,](?:.+?\\.)?sourceName[:=](?<name>.+?)[,;]")]
    private static partial Regex SourceRegex();

    public AllAnimeStreamProvider(HttpClient client) : base(client) { }

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

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range, string streamType)
    {
        var versionResponse = await _client.GetFromJsonAsync<GetVersionResponse>(Config.BaseUrl.TrimEnd('/') + "/getVersion");
        var apiEndPoint = new UriBuilder(versionResponse?.episodeIframeHead ?? "");

        var html = await _client.GetStringAsync(url);

        var match = EpisodesRegex().Match(html);

        if (!match.Success)
        {
            this.Log().Error("availableEpisodesDetail not found");
        }

        var episodesDetail = JsonSerializer.Deserialize<EpisodeDetails>(match.Groups[1].Value.Replace("\\\"", "\""));
        var sorted = GetEpisodes(episodesDetail!).OrderBy(x => x.Length).ThenBy(x => x).ToList();
        var total = int.Parse(sorted.LastOrDefault(x => int.TryParse(x, out int e))!);
        var (start, end) = range.Extract(total);
        foreach (var ep in sorted)
        {
            if (int.TryParse(ep, out int e))
            {
                if (e < start)
                {
                    continue;
                }
                else if (e > end)
                {
                    break;
                }
            }

            var streamTypes = new List<string>() { "sub" };

            if(episodesDetail.dub?.Contains(ep) == true)
            {
                streamTypes.Add("dub");
            }
            if(episodesDetail.raw?.Contains(ep) == true)
            {
                streamTypes.Add("raw");
            }


            var epUrl = $"{url}/episodes/{streamType}/{ep}";
            var content = await _client.GetStringAsync(epUrl);

            var hasEmbedMatch = SourceEmbedRegex().Match(content);

            var directProvider = new List<(string, string)>();
            var embedProvider = new List<string>();

            if (hasEmbedMatch.Success)
            {
                var rawUrl = hasEmbedMatch.Groups[1].Value;
                var directUrl = rawUrl.Replace("clock", "clock.json");
                directProvider.Add(new(directUrl, ""));
            }
            else
            {
                foreach (var sourceMatch in SourceRegex().Matches(content).Cast<Match>())
                {
                    var rawUrl = sourceMatch.Groups[1].Value;
                    var priority = sourceMatch.Groups["priority"].Value;
                    var parsedUrl = DecodeEncodedNonAsciiCharacters(HttpUtility.UrlDecode(rawUrl.Replace("clock", "clock.json")));
                    var uri = new Uri(parsedUrl, UriKind.RelativeOrAbsolute);

                    if (!uri.IsAbsoluteUri)
                    {
                        var directUrl = apiEndPoint.Host + parsedUrl;
                        directProvider.Add(new(directUrl, priority));
                    }
                    else
                    {
                        embedProvider.Add(parsedUrl);
                    }
                }

            }

            if (directProvider.Count == 0)
            {
                continue;
            }

            var stream = await Extract(directProvider.OrderBy(x => x.Item2).FirstOrDefault().Item1);
            if (stream is { })
            {
                stream.Episode = e;
                stream.EpisodeString = ep;
                stream.StreamTypes = streamTypes;
                yield return stream;
            }
        }
    }

    public override IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range) =>  GetStreams(url, range, Config.StreamType);

    private async Task<VideoStreamsForEpisode> Extract(string url)
    {
        if(string.IsNullOrEmpty(url))
        {
            return null;
        }

        if(!url.StartsWith("https"))
        {
            url = $"https://{url}";
        }

        var json = await _client.GetStringAsync(url);
        var jObject = JsonNode.Parse(json);
        var links = jObject!["links"]!.Deserialize<List<StreamLink>>()!;
        var uri = new Uri(links[0].link);
        return uri.Host switch
        {
            "v.vrv.co" => await VrvUnpack(uri),
            "repackager.wixmp.com" => WixMpUnpack(uri),
            _ => GetDefault(links)
        };
    }

    private static string GetResolution(StreamLink link)
    {
        if(link.resolution > 0)
        {
            return link.resolution.ToString();
        }
        if(!string.IsNullOrEmpty(link.resolutionStr))
        {
            return link.resolutionStr;
        }
        return "default";
    }

    private static VideoStreamsForEpisode GetDefault(IEnumerable<StreamLink> links)
    {
        var result = new VideoStreamsForEpisode();
        foreach (var item in links)
        {
            var resolutionStr = GetResolution(item);
            var uri = new Uri(item.link);
            var stream = new VideoStream
            {
                Url = uri.AbsoluteUri,
                Quality = resolutionStr
            };
            result.Qualities.Add(resolutionStr, stream);
        }
        return result;
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

    private static VideoStreamsForEpisode WixMpUnpack(Uri uri)
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
        return Config.StreamType switch
        {
            "sub" => episodeDetails.sub,
            "dub" => episodeDetails.dub,
            "raw" => episodeDetails.raw,
            _ => throw new NotSupportedException()
        };
    }

    static string DecodeEncodedNonAsciiCharacters(string value)
    {
        return UtfEncodedStringRegex().Replace(value, m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
    }

    [GeneratedRegex("\\\\u(?<Value>[a-zA-Z0-9]{4})")]
    private static partial Regex UtfEncodedStringRegex();
}
