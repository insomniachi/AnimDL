using System.Text;

namespace AnimDL.Core.Helpers;

internal class EncodingHelper
{
    public static string ToBase64String(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    public static string FromBase64String(string str) => Encoding.UTF8.GetString(Convert.FromBase64String(str));
}
