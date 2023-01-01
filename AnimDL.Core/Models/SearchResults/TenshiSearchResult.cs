using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models.SearchResults;

internal class TenshiSearchResult : SearchResult, IHaveImage, IHaveYear, IHaveEpisodes, IHaveGenre, IHaveType
{
    public string Year { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Episodes { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
