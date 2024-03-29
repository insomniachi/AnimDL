﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AnimDL.Core;
using AnimDL.Core.Helpers;
using AnimDL.Core.Models;
using AnimDL.Core.StreamProviders;
using HtmlAgilityPack;
using ReactiveUI;
using Splat;

namespace Plugin.AnimePahe;

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

    [GeneratedRegex(@"<a href=""(?<url>.+?)"" .+? class=""dropdown-item"">.+? (?<resolution>\d+)p.+?</a>")]
    private static partial Regex StreamsRegex();

    public const string CHARACTER_MAP = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/";
    private readonly HttpClient _clientInternal = new(new HttpClientHandler { AllowAutoRedirect = false });

    public AnimePaheStreamProvider(HttpClient client) : base(client)
    {
    }

    public override async Task<int> GetNumberOfStreams(string url)
    {
        var html = await _clientInternal.GetStringAsync(url);
        var releaseId = IdRegex().Match(html).Groups[1].Value;

        return (await GetSessionPage(releaseId, 1)).total;
    }

    public override async IAsyncEnumerable<VideoStreamsForEpisode> GetStreams(string url, Range range)
    {
        var doc = await Load(url);
        var releaseId = IdRegex().Match(doc.Text).Groups[1].Value;

        var firstPage = await GetSessionPage(releaseId, 1);
        var (start, end) = range.Extract(firstPage.total);
        var startPage = (start + firstPage.per_page - 1) / firstPage.per_page;
        var endPage = (end + firstPage.per_page - 1) / firstPage.per_page;

        if (startPage == 1)
        {
            await foreach(var item in GetStreamFromPage(firstPage, releaseId, start, end))
            {
                yield return item;
            }
        }
        for (int i = startPage == 1 ? startPage + 1 : startPage; i <= endPage; i++)
        {
            var page = await GetSessionPage(releaseId, i);
            await foreach (var item in GetStreamFromPage(page, releaseId, start, end))
            {
                yield return item;
            }
        }

    }

    private async IAsyncEnumerable<VideoStreamsForEpisode> GetStreamFromPage(AnimePaheEpisodePage page, string releaseId, int start, int end)
    {
        foreach (var item in page.data.Where(x => x.episode >= start && x.episode <= end))
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
                    var url = await GetDirectLink(kv.Value);
                    if(string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    streams.Qualities.Add(kv.Key, new VideoStream
                    {
                        Quality = kv.Key,
                        Url = url
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

    private async Task<AnimePaheEpisodePage> GetSessionPage(string releaseId, int page)
    {
        var content = await _client.GetStreamAsync(Config.BaseUrl.TrimEnd('/') + "/api", parameters: new()
        {
            ["m"] = "release",
            ["id"] = releaseId,
            ["sort"] = "episode_asc",
            ["page"] = page.ToString()
        });

        return await JsonSerializer.DeserializeAsync<AnimePaheEpisodePage>(content) ?? new();
    }

    private async Task<Dictionary<string, string>> GetStreamUrl(string releaseId, string streamSession)
    {
        var streamData = await _client.GetStringAsync(Config.BaseUrl.TrimEnd('/') + $"/play/{releaseId}/{streamSession}");
        var result = new Dictionary<string, string>();
        
        foreach (var match in StreamsRegex().Matches(streamData).Cast<Match>())
        {
            result.TryAdd(match.Groups["resolution"].Value, match.Groups["url"].Value);
        }

        return result;
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
            this.Log().Error("Unable to parse url");
            return string.Empty;
        }

        var format = match.Groups[1].Value;
        var a = int.Parse(match.Groups[2].Value);
        var count = int.Parse(match.Groups[3].Value);
        var items = match.Groups[4].Value.Split("|").ToArray();
        count--;
        while (count > 0)
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

    private static string Encode(int number, int @base)
    {
        int n = number / @base;
        char c = CHARACTER_MAP[number % @base];
        return n > 0 ? Encode(n, @base) + c : c.ToString();
    }

    private static string GetToken(int c, int a)
    {
        var part1 = c < a ? string.Empty : (c / a).ToString();
        c %= a;
        var part2 = c > 35 ? ((char)(c + 29)).ToString() : Encode(c, 36);
        return part1 + part2;
    }


    private async Task<string> GetDirectLink(string kwikPahewin)
    {
        var response = await _clientInternal.GetStringAsync(kwikPahewin);
        var url = KwikRedirectionRegex().Match(response).Groups[1].Value;
        var downloadPage = await _clientInternal.GetStringAsync(url);
        var match = KwikParamsRegex().Match(downloadPage);

        if (!match.Success)
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
        _clientInternal.DefaultRequestHeaders.Referrer = new("https://kwik.cx/");
        _clientInternal.DefaultRequestHeaders.UserAgent.ParseAdd(ServiceCollectionExtensions.USER_AGENT);
        var httpResponse = await _clientInternal.PostAsync(postUrl, content);

        if(httpResponse.StatusCode == System.Net.HttpStatusCode.Found)
        {
            return httpResponse!.Headers!.Location!.AbsoluteUri;
        }

        return string.Empty;
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
                s += fullString[i];
                i++;
            }
            var j = 0;
            while (j < key.Length)
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
        while (acc > 0)
        {
            k = slice[acc % s2] + k;
            acc = (acc - (acc % s2)) / s2;
        }

        return string.IsNullOrEmpty(k) ? "0" : k;
    }
}
