using System.Diagnostics;
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
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Splat;

namespace Plugin.AllAnime;

public partial class AllAnimeStreamProvider : BaseStreamProvider, IMultiAudioStreamProvider
{
    private readonly HtmlWeb _web = new();
    internal static readonly object _extensionsStream = new
    {
        persistedQuery = new
        {
            version = 1,
            sha256Hash = "1f0a5d6c9ce6cd3127ee4efd304349345b0737fbf5ec33a60bbc3d18e3bb7c61"
        }
    };
    internal static readonly object _extensionsShow = new
    {
        persistedQuery = new
        {
            version = 1,
            sha256Hash = "f73a8347df0e3e794f8955a18de6e85ac25dfc6b74af8ad613edf87bb446a854"
        }
    };
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
        public PortData portData { get; set; }
    }

    [DebuggerDisplay("{priority} - {sourceUrl} - {type}")]
    class SourceUrlObj
    {
        public string sourceUrl { get; set; }
        public double priority { get; set; }
        public string type { get; set; }
    }

    class PortDataStream
    {
        public string format { get; set; }
        public string url { get; set; }
        public string audio_lang { get; set; }
        public string hardsub_lang { get; set; }
    }

    class PortData
    {
        public List<PortDataStream> streams { get; set; }
    }

    [GeneratedRegex(@"^#EXT-X-STREAM-INF:.*?RESOLUTION=\d+x(?'resolution'\d+).*?\n(?'url'.+?)$", RegexOptions.Multiline)]
    private static partial Regex VrvResponseRegex();

    [GeneratedRegex(@"https://.+?/(?'base'.+?/),(?'resolutions'(?:\d+p,)+)")]
    private static partial Regex WixMpUrlRegex();

    public AllAnimeStreamProvider(HttpClient client) : base(client) { }

    public override Task<int> GetNumberOfStreams(string url) => GetNumberOfStreams(url, Config.StreamType);

    public async Task<int> GetNumberOfStreams(string url, string streamType)
    {
        var searchVariables = new
        {
            _id = url.Split('/').LastOrDefault()?.Trim()
        };
        var showApi = QueryHelpers.AddQueryString("https://api.allanime.co/allanimeapi", new Dictionary<string, string>
        {
            ["variables"] = JsonSerializer.Serialize(searchVariables),
            ["extensions"] = JsonSerializer.Serialize(_extensionsShow)
        });

        var doc = await _web.LoadFromWebAsync(showApi);
        var episodeDetails = JsonNode.Parse(doc.Text)?["data"]?["show"]?["availableEpisodesDetail"];
        var sorted = GetEpisodes(episodeDetails?.Deserialize<EpisodeDetails>() ?? new(), streamType).OrderBy(x => x.Length).ThenBy(x => x).ToList();
        var total = int.Parse(sorted.LastOrDefault(x => int.TryParse(x, out int e))!);

        return total;
    }

    public async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range, string streamType)
    {
        var versionResponse = await _client.GetFromJsonAsync<GetVersionResponse>(Config.BaseUrl.TrimEnd('/') + "/getVersion");
        var apiEndPoint = new UriBuilder(versionResponse?.episodeIframeHead ?? "");
        var id = url.Split('/').LastOrDefault()?.Trim();
        var searchVariables = new
        {
            _id = id
        };
        var showApi = QueryHelpers.AddQueryString("https://api.allanime.co/allanimeapi", new Dictionary<string,string>
        {
            ["variables"] = JsonSerializer.Serialize(searchVariables),
            ["extensions"] = JsonSerializer.Serialize(_extensionsShow)
        });

        var doc = await _web.LoadFromWebAsync(showApi);
        var episodeDetails = JsonNode.Parse(doc.Text)?["data"]?["show"]?["availableEpisodesDetail"];

        if (episodeDetails is null)
        {
            this.Log().Error("availableEpisodesDetail not found");
            yield break;
        }

        var episodesDetail = episodeDetails.Deserialize<EpisodeDetails>();
        var sorted = GetEpisodes(episodesDetail!, Config.StreamType).OrderBy(x => x.Length).ThenBy(x => x).ToList();
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

            var variables = new
            {
                showId = url.Split('/').LastOrDefault()?.Trim(),
                translationType = streamType,
                episodeString = ep,
            };

            var queryParams = new Dictionary<string, string>
            {
                ["variables"] = JsonSerializer.Serialize(variables),
                ["extensions"] = JsonSerializer.Serialize(_extensionsStream)
            };

            var api = QueryHelpers.AddQueryString("https://api.allanime.co/allanimeapi", queryParams);
            var response = await _web.LoadFromWebAsync(api);
            var jsonNode = JsonNode.Parse(response.Text);
            var sourceArray = jsonNode?["data"]?["episode"]?["sourceUrls"].AsArray();
            var sourceObjs = sourceArray.Deserialize<List<SourceUrlObj>>() ?? new List<SourceUrlObj>();

            var source = sourceObjs.Where(x => x.type == "iframe").OrderByDescending(x => x.priority).First();
            var parsedUrl = DecodeEncodedNonAsciiCharacters(HttpUtility.UrlDecode(source.sourceUrl.Replace("clock", "clock.json")));
            var uri = new Uri(parsedUrl, UriKind.RelativeOrAbsolute);

            var streamUrl = parsedUrl;
            if (!uri.IsAbsoluteUri)
            {
                streamUrl = apiEndPoint.Host + parsedUrl;
            }

            var stream = await Extract(streamUrl);
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

        if(string.IsNullOrEmpty(links[0].link))
        {
            var result = new VideoStreamsForEpisode();
            var resolutionStr = GetResolution(links[0]);
            if (links[0].portData.streams.FirstOrDefault(x => x is { hardsub_lang: "en-US", format: "adaptive_hls" or "hls" }) is { } obj)
            {
                var stream = new VideoStream
                {
                    Url = obj.url,
                    Quality = resolutionStr
                };
                result.Qualities.Add(resolutionStr, stream);

            }
            return result;
        }

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

    private static List<string> GetEpisodes(EpisodeDetails episodeDetails, string streamType)
    {
        return streamType switch
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
