using System.Text.RegularExpressions;

namespace AnimDL.Core.Helpers;

public static class BypassHelper
{
    public static async Task<HttpClient> BypassDDoS(string uri)
    {
        var client = new HttpClient();
        var text = client.GetStringAsync("https://check.ddos-guard.net/check.js").Result;
        var match = Regex.Match(text, "'(.*?)'");
        await client.GetAsync(uri + match.Groups[1].Value);
        return client;
    }
}
