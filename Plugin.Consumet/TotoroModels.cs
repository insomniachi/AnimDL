using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.Consumet;

public class AnimePaheSearchResult : SearchResult, IHaveImage, IHaveRating, IHaveYear, IHaveType
{
    public string Image { get; set; }
    public string Rating { get; set; }
    public string Year { get; set; }
    public string Type { get; set; }
}

public class ZoroSearchResult : SearchResult, IHaveImage, IHaveType
{
    public string Image { get; set; }
    public string Type { get; set; }
}

public class GenericSearchResult : SearchResult, IHaveImage
{
    public string Image { get; set; }
}

public class EnimeSearchResult : SearchResult, IHaveImage, IHaveRating, IHaveYear, IHaveType
{
    public string Image { get; set; }
    public string Rating { get; set; }
    public string Year { get; set; }
    public string Type { get; set; }
    public long MalId { get; set; }
    public long AnilistId { get; set; }
}