using AnimDL.Core.Models;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AnimDL.Core.StreamProviders;

public class AnimixPlayStreamProvider : BaseStreamProvider
{
    const string BASE_URL = "https://animixplay.to/api/live";
    private static readonly Regex _videoMatcherRegex = new("iframesrc=\"(.+?)\"", RegexOptions.Compiled);
    private static readonly Regex _m3u8MatcherRegex = new("player\\.html[?#](.+?)#", RegexOptions.Compiled);
    private static readonly Regex _embedB64MatcherRegex = new("#(aHR0[^#]+)", RegexOptions.Compiled);
    private static readonly IReadOnlyDictionary<string, string> _urlAliases = new Dictionary<string, string>()
    {
        ["bestanimescdn"] = "omega.kawaiifucdn.xyz/anime3",
        ["anicdn.stream"] = "gogocdn.club",
        ["ssload.info"] = "gogocdn.club",
    };

    public int GetEpisodeCount(string url)
    {
        var doc = _session.Load(url);
        var eps = doc.GetElementbyId("epslistplace")?.InnerText;

        if (string.IsNullOrEmpty(eps))
        {
            return 0;
        }

        JsonObject json = JsonNode.Parse(eps)!.AsObject();
        return json["eptotal"]!.GetValue<int>();
    }

    public override async IAsyncEnumerable<HlsStreams> GetStreams(string url)
    {
        var doc = await Load(url);

        var eps = doc.GetElementbyId("epslistplace")?.InnerText;

        if (string.IsNullOrEmpty(eps))
        {
            yield break;
        }

        JsonObject json = JsonNode.Parse(eps)!.AsObject();
        var epTotal = json["eptotal"]!.GetValue<int>();

        foreach (var ep in Enumerable.Range(0, epTotal).Select(x => x.ToString()))
        {
            var epUrl = GetStreamUrl(json[ep]!.ToString());
            if (!string.IsNullOrEmpty(epUrl))
            {
                yield return new()
                {
                    episode = int.Parse(ep),
                    streams = new()
                    {
                        new(){ stream_url = epUrl}
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
            return string.Empty;
        }

        var match = _videoMatcherRegex.Match(doc.Text);

        return match.Success
            ? ExtractFromUrl(match.Groups[1].Value.ToString())
            : ExtractFromUrl(_session.ResponseUri.ToString());
    }

    private static string ExtractFromUrl(string url)
    {
        var match = _m3u8MatcherRegex.Match(url);
        if (!match.Success)
        {
            match = _embedB64MatcherRegex.Match(url);
        }
        if (!match.Success)
        {
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
        return $"{BASE_URL}{Convert.ToBase64String(Encoding.UTF8.GetBytes($"{contentId}LTXs3GrU8we9O{Convert.ToBase64String(Encoding.UTF8.GetBytes(contentId))}"))}";
    }
}
