using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults;

internal class AnimePaheSearchResult : SearchResult, IHaveImage, IHaveSeason
{
    public string Image { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
}
