using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.Marin;

public class MarinSearchResult : SearchResult, IHaveImage, IHaveYear, IHaveType
{
    public string Type { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
