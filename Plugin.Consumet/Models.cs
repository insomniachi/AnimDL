using AnimDL.Core.Models;
using AnimDL.Core.Models.Interfaces;

namespace Plugin.Consumet;

public class ConsumetZoroSearchResult
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public string Type { get; set; }

    public SearchResult ToSearchResult()
    {
        return new ZoroSearchResult
        {
            Image = Image,
            Url = Id,
            Title = Title,
            Type = Type,
        };
    }
}

public class ConsumetAnimePaheSearchResult
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public float Rating { get; set; }
    public int ReleaseDate { get; set; }
    public string Type { get; set; }

    public SearchResult ToSearchResult()
    {
        return new AnimePaheSearchResult
        {
            Image = Image,
            Url = Id,
            Title = Title,
            Type = Type,
            Rating = $"{Rating:N2}",
            Year = ReleaseDate.ToString()
        };
    }
}


public class ConsumetZoroAiredEpisode
{
    public string Id { get; set; }
    public string Image { get; set; }
    public string Title { get; set; }
    public int Episode { get; set; }

    public AiredEpisode ToConsumetAiredEpisode()
    {
        return new ConsumetAiredEpisodesProvider.ConsumetAiredEpisode
        {
            Title = Title,
            Url = Id,
            Image = Image,
            Episode = Episode,
        };
    }
}

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