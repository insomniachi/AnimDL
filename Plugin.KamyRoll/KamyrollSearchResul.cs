using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.KamyRoll;

internal class KamyrollSearchResult : SearchResult, IHaveImage
{
    public string Image { get; set; } = string.Empty;
}
