using System.Text.RegularExpressions;

namespace AnimDL.Core.Helpers;

internal class RegexHelper
{
    internal static Regex SiteBasedRegex(string siteUrl, string extra = "", string extraRegex = "")
    {
        var escape = Regex.Escape(Regex.Match(@"(?:https?://)?((?:\S+\.)+[^/]+)/?", siteUrl).Groups[1].Value.ToLower());
        var regex = string.Format("(?:https?://)?(?:\\S+\\.)*{0}", escape + extra) + extraRegex;

        return new Regex(regex, RegexOptions.Compiled);
    }
}
