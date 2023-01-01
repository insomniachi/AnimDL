using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace AnimDL.Core.Helpers;

public static partial class Bypass
{
    public const string RECAPTCHA_API_JS = "https://www.google.com/recaptcha/api.js";

    [GeneratedRegex("'(.*?)'")]
    private static partial Regex BypassRegex();

    [GeneratedRegex("recaptchaSiteKey = '(.+?)'")]
    private static partial Regex RecaptchaSiteKeyRegex();

    [GeneratedRegex("recaptchaNumber = '(\\d+?)'")]
    private static partial Regex RecaptchaNumberRegex();

    [GeneratedRegex("releases/([^/&?#]+)")]
    private static partial Regex RecaptchaVersionRegex();

    [GeneratedRegex("recaptcha-token.+?=\"(.+?)\"")]
    private static partial Regex RecaptchaTokenRegex();

    [GeneratedRegex("rresp\",\"(.+?)\"")]
    private static partial Regex RecaptchaResponseTokenRegex();

    public static async Task BypassDDoS(this HttpClient client, string uri)
    {
        var text = await client.GetStringAsync("https://check.ddos-guard.net/check.js");
        var match = BypassRegex().Match(text);
        var result = await client.GetStringAsync(uri + match.Groups[1].Value);
    }

    public static async Task<(string Token, string Number)?> BypassRecaptcha(this HttpClient client, string url, Dictionary<string, string>? headers = null)
    {
        var uri = new Uri(url);
        var referer = $"{uri.Scheme}://{uri.Host}:443";
        var domain = EncodingHelper.ToBase64String(referer).TrimEnd('=') + ".";

        var initialPage = await client.GetStringAsync(url);
        var match = RecaptchaSiteKeyRegex().Match(initialPage);

        if (!match.Success)
        {
            return null;
        }

        var token = await GetTokenRecaptcha(client, domain, match.Groups[1].Value, referer);
        match = RecaptchaNumberRegex().Match(initialPage);

        if (!match.Success)
        {
            return null;
        }

        return (token, match.Groups[1].Value);
    }

    private static async Task<string> GetTokenRecaptcha(HttpClient client, string domain, string key, string referer)
    {
        var recaptchaOut = await client.GetStringAsync(RECAPTCHA_API_JS,
            parameters: new() { ["render"] = "key" },
            headers: new() { ["referer"] = referer });

        var match = RecaptchaVersionRegex().Match(recaptchaOut);

        if (!match.Success)
        {
            return "";
        }

        var version = match.Groups[1].Value;

        var anchorOut = await client.GetStringAsync("https://www.google.com/recaptcha/api2/anchor", parameters: new()
        {
            ["ar"] = "1",
            ["k"] = key,
            ["co"] = domain,
            ["hl"] = "en",
            ["v"] = version,
            ["size"] = "invisible",
            ["cb"] = "kr42069kr"
        });

        match = RecaptchaTokenRegex().Match(anchorOut);

        if (!match.Success)
        {
            return "";
        }

        var recaptchaToken = match.Groups[1].Value;

        var reloadUrl = QueryHelpers.AddQueryString("https://www.google.com/recaptcha/api2/reload", new Dictionary<string, string> { ["k"] = key });
        client.DefaultRequestHeaders.Referrer = new Uri("https://www.google.com/recaptcha/api2");

        var tokenOut = await client.PostFormUrlEncoded(reloadUrl, new Dictionary<string, string>
        {
            ["v"] = version,
            ["reason"] = "q",
            ["k"] = key,
            ["c"] = recaptchaToken,
            ["sa"] = "",
            ["co"] = domain
        });

        match = RecaptchaResponseTokenRegex().Match(tokenOut);

        if (!match.Success)
        {
            return "";
        }

        return match.Groups[1].Value;
    }
}

