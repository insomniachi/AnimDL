using System.Text;
using System.Text.RegularExpressions;

namespace AnimDL.Core.Helpers;

internal class EncodingHelper
{
    public static string ToBase64String(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    public static string FromBase64String(string str) => Encoding.UTF8.GetString(Convert.FromBase64String(str));

    //return regex.compile(
    //"(?:https?://)?(?:\\S+\\.)*{}".format(
    //    regex.escape(
    //        regex.search(r"(?:https?://)?((?:\S+\.)+[^/]+)/?", site_url).group(1)
    //    )
    //    + extra
    //)
    //+ extra_regex )

    public static Regex SiteBasedRegex(string siteUrl, string extra = "", string extraRegex = "")
    {
        var escape = Regex.Escape(Regex.Match(@"(?:https?://)?((?:\S+\.)+[^/]+)/?", siteUrl).Groups[1].Value.ToLower());
        var regex = string.Format("(?:https?://)?(?:\\S+\\.)*{0}", escape + extra) + extraRegex;

        return new Regex(regex, RegexOptions.Compiled);
    }
}
