using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.Models.Internal;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Splat;

namespace AnimDL.Core.StreamProviders;

public partial class AnimePaheStreamProvider : BaseStreamProvider
{
    [GeneratedRegex("let id = \"(.+?)\"", RegexOptions.Compiled)]
    private static partial Regex IdRegex();

    [GeneratedRegex("<a href=\"(.+?)\" .+?>Redirect me</a>")]
    private static partial Regex KwikRedirectionRegex();

    [GeneratedRegex(@"\(""(\w+)"",\d+,""(\w+)"",(\d+),(\d+),\d+\)")]
    private static partial Regex KwikParamsRegex();

    [GeneratedRegex(@"action=""(.+?)""")]
    private static partial Regex KwikDecryptUrlRegex();

    [GeneratedRegex(@"value=""(.+?)""")]
    private static partial Regex KwikDecryptTokenRegex();

    [GeneratedRegex(@"\('h o=\\'(.*)\\';.+,(\d+),(\d+),'(.+)'.split\('\|'\),.+,.+\)")]
    private static partial Regex KwiwRegex();

    public readonly string API = DefaultUrl.AnimePahe + "api";
    public const string CHARACTER_MAP = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/";
    private readonly HttpClient _clientInternal = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });

    public AnimePaheStreamProvider(HttpClient client) : base(client)
    {
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var doc = await Load(url);
        var releaseId = IdRegex().Match(doc.Text).Groups[1].Value;

        return (await GetSessionPage("1", releaseId)).total;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url);
        var releaseId = IdRegex().Match(doc.Text).Groups[1].Value;

        var fpd = GetSessionPage("1", releaseId).Result;
        var (start, end) = range.Extract(fpd.total);
        if (fpd.last_page == 1)
        {
            foreach (var item in fpd.data.Where(x => x.episode >= start && x.episode <= end))
            {
                var streams = new VideoStreamsForEpisode
                {
                    Episode = item.episode
                };

                var stringUrls = await GetStreamUrl(releaseId, item.session);

                foreach (var kv in stringUrls)
                {
                    try
                    {
                        streams.Qualities.Add(kv.Key, new VideoStream
                        {
                            Quality = kv.Key,
                            Headers = new Dictionary<string, string> { [Headers.Referer] = kv.Value.kwik },
                            Url = GetStreamFromEmbedUrl(kv.Value.kwik)
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Log().Error(ex, ex.Message);
                    }
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

    private async Task<Dictionary<string, AnimePaheEpisodeStream>> GetStreamUrl(string releaseId, string streamSession)
    {
        var json = await _client.GetStreamAsync(API, parameters: new()
        {
            ["m"] = "links",
            ["id"] = releaseId,
            ["session"] = streamSession,
            ["p"] = "kwik"
        });

        var jObject = JsonNode.Parse(json);
        var data = jObject!["data"]!.AsArray();

        var result = data.ToDictionary(x => x!.AsObject().ElementAt(0).Key, x => x!.AsObject().ElementAt(0).Value.Deserialize<AnimePaheEpisodeStream>());
        return result!;
    }

    private async Task<string> GetStreamFromEmbedUrlMp4(string kwikPahewin)
    {
        var response = await _clientInternal.GetStringAsync(kwikPahewin);
        var url = KwikRedirectionRegex().Match(response).Groups[1].Value;
        var downloadPage = await _clientInternal.GetStringAsync(url);
        var match = KwikParamsRegex().Match(downloadPage);

        if(!match.Success)
        {
            this.Log().Error("unable to find decryption paramters");
        }

        var fullKey = match.Groups[1].Value;
        var key = match.Groups[2].Value;
        var v1 = match.Groups[3].Value;
        var v2 = match.Groups[4].Value;

        var decrypted = Decrypt(fullKey, key, int.Parse(v1), int.Parse(v2));

        var postUrl = KwikDecryptUrlRegex().Match(decrypted).Groups[1].Value;
        var token = KwikDecryptTokenRegex().Match(decrypted).Groups[1].Value;

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_token"] = token
        });
        _clientInternal.DefaultRequestHeaders.Add("referer", "https://kwik.cx/");
        var httpResponse = await _clientInternal.PostAsync(postUrl, content);

        return string.Empty;
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

        if(!match.Success)
        {
            this.Log().Error("Unable to parse url");
            return string.Empty;
        }

        var format = match.Groups[1].Value;
        var a = int.Parse(match.Groups[2].Value);
        var count = int.Parse(match.Groups[3].Value);
        var items = match.Groups[4].Value.Split("|").ToArray();
        count--;
        var sb = new StringBuilder();
        while(count > 0)
        {
            var value = items[count];
            if (!string.IsNullOrEmpty(value))
            {
                var token = GetToken(count, a);
                format = Regex.Replace(format, $@"\b{token}\b", value);
            }
            count--;
        }
        return format;
    }

    static string Encode(int nIn, int nBase)
    {
        int n = nIn / nBase;
        char c = "0123456789abcdefghijklmnopqrstuvwxyz"[nIn % nBase];
        return n > 0 ? Encode(n, nBase) + c : c.ToString();
    }

    private static string GetToken(int c, int a)
    {

        var part1 = c < a ? string.Empty : (c/a).ToString();
        c %= a;
        var part2 = c > 35 ? ((char)(c + 29)).ToString() : Encode(c,36);
        return part1 + part2;
    }

    private static string Decrypt(string fullString, string key, int v1, int v2)
    {
        var r = "";
        var i = 0;
        while (i < fullString.Length)
        {
            var s = "";
            while (fullString[i] != key[v2])
            {
                s+= fullString[i];
                i++;
            }
            var j = 0;
            while(j < key.Length)
            {
                s = s.Replace(key[j].ToString(), j.ToString());
                j++;
            }
            r += (char)(int.Parse(GetString(s, v2, 10)) - v1);
            i++;
        }

        return r;
    }

    private static string GetString(string content, int s1, int s2)
    {
        var slice = CHARACTER_MAP[0..s2];
        var acc = 0;
        var index = 0;
        foreach (var item in content.Reverse())
        {
            acc += (char.IsDigit(item) ? int.Parse(item.ToString()) : 0) * (int)Math.Pow(s1, index);
            index++;
        }

        var k = "";
        while(acc > 0)
        {
            k = slice[acc % s2] + k;
            acc = (acc - (acc % s2)) / s2;
        }

        return string.IsNullOrEmpty(k) ? "0" : k;
    }
}
