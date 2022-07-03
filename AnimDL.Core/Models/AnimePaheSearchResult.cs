using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models;

public class AnimePaheSearchResult : SearchResult, IHaveImage
{
    public string Image { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Season { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
