using AnimDL.Core.Models.Interfaces;

namespace AnimDL.Core.Models;

public class AnimixPlaySearchResult : SearchResult, IHaveImage
{
    public string Image { get; set; } = string.Empty;
}
