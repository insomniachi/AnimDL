using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.Marin;

internal class MarinSearchResult : SearchResult, IHaveImage, IHaveYear, IHaveType
{
    public string Type { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
