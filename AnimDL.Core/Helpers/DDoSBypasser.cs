using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;

namespace AnimDL.Core.Helpers;

public static class BypassHelper
{
    const string RECAPTCHA_API_JS = "https://www.google.com/recaptcha/api.js";

    public static async Task<HttpClient> BypassDDoS(string uri)
    {
        var client = new HttpClient();
        var text = client.GetStringAsync("https://check.ddos-guard.net/check.js").Result;
        var match = Regex.Match(text, "'(.*?)'");
        await client.GetAsync(uri + match.Groups[1].Value);
        return client;
    }

    public static async Task<(string Token, string Number)?> BypassRecaptcha(HttpClient client, string url, Dictionary<string, string>? headers = null)
    {
        var uri = new Uri(url);
        var referer = $"{uri.Scheme}://{uri.Host}:443";
        var domain = EncodingHelper.ToBase64String(referer).TrimEnd('=') + ".";

        var initialPage = await client.GetStringAsync(url);
        var match = Regex.Match(initialPage, "recaptchaSiteKey = '(.+?)'");

        if (!match.Success)
        {
            return null;
        }

        var token = await GetTokenRecaptcha(client, domain, match.Groups[1].Value, referer);
        match = Regex.Match(initialPage, "recaptchaNumber = \"(\\d+?)\"");

        if(!match.Success)
        {
            return null;
        }

        return (token, match.Groups[1].Value);
    }

    private static async Task<string> GetTokenRecaptcha(HttpClient client, string domain, string key, string referer)
    {
        var recaptchaUrl = QueryHelpers.AddQueryString(RECAPTCHA_API_JS, new Dictionary<string, string> { ["render"] = "key" });
        client.DefaultRequestHeaders.Referrer = new Uri(referer);

        var recaptchaOut = await client.GetStringAsync(recaptchaUrl);

        var match = Regex.Match(recaptchaOut, "releases/([^/&?#]+)");

        if(!match.Success)
        {
            return "";
        }

        var token = match.Groups[1].Value;

        var anchorOutUrl = QueryHelpers.AddQueryString("https://www.google.com/recaptcha/api2/anchor", new Dictionary<string, string>
        {
            ["ar"] = "1",
            ["k"] = key,
            ["co"] = domain,
            ["hl"] = "en",
            ["v"] = token,
            ["size"] = "invisible",
            ["cb"] = "kr42069kr"
        });

        var anchorOut = await client.GetStringAsync(anchorOutUrl);

        match = Regex.Match(anchorOut, "recaptcha-token.+?=\"(.+?)\"");

        if(!match.Success)
        {
            return "";
        }

        var recaptchaToken = match.Groups[1].Value;

        var reloadUrl = QueryHelpers.AddQueryString("https://www.google.com/recaptcha/api2/reload", new Dictionary<string, string> { ["k"] = key });
        client.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com/recaptcha/api2");

        var tokenOut = await HttpHelper.PostFormUrlEncoded(client, reloadUrl, new Dictionary<string, string>
        {
            ["v"] = token,
            ["reason"] = "q",
            ["k"] = key,
            ["c"] = recaptchaToken,
            ["sa"] = "",
            ["co"] = domain
        });

        match = Regex.Match(tokenOut, "rresp\",\"(.+?)\"");

        if(!match.Success)
        {
            return "";
        }    

        return match.Groups[1].Value;
    }
}

