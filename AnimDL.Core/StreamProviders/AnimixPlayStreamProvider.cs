using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using Splat;

namespace AnimDL.Core.StreamProviders;

[Obsolete("RIP")]
internal partial class AnimixPlayStreamProvider : BaseStreamProvider
{
    public static readonly string API = DefaultUrl.AnimixPlay + "api/cW9";

    [GeneratedRegex("iframesrc=\"(.+?)\"", RegexOptions.Compiled)]
    private static partial Regex VideoMatcherRegex();

    [GeneratedRegex("player\\.html[?#](.+?)#", RegexOptions.Compiled)]
    private static partial Regex M3U8MatcherRegex();

    [GeneratedRegex("#(aHR0[^#]+)", RegexOptions.Compiled)]
    private static partial Regex EmbedB64MatcherRegex();

    private static readonly IReadOnlyDictionary<string, string> _urlAliases = new Dictionary<string, string>()
    {
        ["bestanimescdn"] = "omega.kawaiifucdn.xyz/anime3",
        ["anicdn.stream"] = "gogocdn.club",
        ["ssload.info"] = "gogocdn.club",
    };

    public AnimixPlayStreamProvider(HttpClient client) : base(client)
    {
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var doc = await Load(url);
        var eps = doc.GetElementbyId("epslistplace")?.InnerText;

        if (string.IsNullOrEmpty(eps))
        {
            return 0;
        }

        JsonObject json = JsonNode.Parse(eps)!.AsObject();
        return json["eptotal"]!.GetValue<int>();
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url);
        var eps = doc.GetElementbyId("epslistplace")?.InnerText;

        if (string.IsNullOrEmpty(eps))
        {
            this.Log().Error("unable to find element \"epslistplace\"");
            yield break;
        }

        if (JsonNode.Parse(eps) is not { } node)
        {
            this.Log().Error("unable to parse {Json}", eps);
            yield break;
        }

        JsonObject jobject = node.AsObject();
        if (jobject["eptotal"] is not JsonNode epTotalNode)
        {
            this.Log().Error("unable to find total number of episdes");
            yield break;
        }

        var epTotal = epTotalNode.GetValue<int>();
        (int start, int end) = range.Extract(epTotal);

        foreach (var ep in Enumerable.Range(start - 1, end - start + 1).Select(x => x.ToString()))
        {
            if (jobject[ep] is not JsonNode epNode)
            {
                this.Log().Error("unable to find data for episode {EP}", ep);
                continue;
            }

            var epUrl = GetStreamUrl(epNode.ToString());

            if (string.IsNullOrEmpty(epUrl))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(epUrl))
            {
                yield return new()
                {
                    Episode = int.Parse(ep) + 1, // start at 1
                    Qualities = new()
                    {
                        ["default"] = new VideoStream
                        {
                            Quality = "default",
                            Url = epUrl,
                        }
                    }
                };
            }
        }
    }
    private string GetStreamUrl(string url)
    {
        var uri = new Uri(url);
        if (uri.Host == "www.dailymotion.com")
        {
            // TODO
        }

        var contentId = QueryHelpers.ParseQuery(url).First().Value.ToString();
        if (string.IsNullOrEmpty(contentId))
        {
            // TODO
        }

        return ExtractFromEmbed(CreateUrl(contentId));
    }

    private string ExtractFromEmbed(string url)
    {
        var doc = _session.Load(url);
        while (_statusCode == HttpStatusCode.TooManyRequests)
        {
            doc = _session.Load(url);
        }

        if (_statusCode == HttpStatusCode.Forbidden)
        {
            this.Log().Error("unable to extract from embed, Status - {Status}", HttpStatusCode.Forbidden);
        }

        var match = VideoMatcherRegex().Match(doc.Text);

        return match.Success
            ? ExtractFromUrl(match.Groups[1].Value.ToString())
            : ExtractFromUrl(_session.ResponseUri.ToString());
    }

    private string ExtractFromUrl(string url)
    {
        var match = M3U8MatcherRegex().Match(url);
        if (!match.Success)
        {
            match = EmbedB64MatcherRegex().Match(url);
        }
        if (!match.Success)
        {
            this.Log().Error("unable to match m3u8 or b64 regex");
            return string.Empty;
        }

        var encodedurl = match.Groups[1].Value.ToString();
        var result = Encoding.UTF8.GetString(Convert.FromBase64String(encodedurl));
        return UpdateUrl(result);
    }

    private static string UpdateUrl(string url)
    {
        var result = url;
        foreach (var item in _urlAliases)
        {
            if (result.Contains(item.Key))
            {
                result = result.Replace(item.Key, item.Value);
            }
        }
        return result;
    }

    private static string CreateUrl(string contentId)
    {
        return $"{API}{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{contentId}LTXs3GrU8we9O{Convert.ToBase64String(Encoding.UTF8.GetBytes(contentId))}"))}";
    }
}
