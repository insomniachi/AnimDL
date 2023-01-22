using System.Text.Json.Serialization;
using AnimDL.Core.Models;

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

public class ConsumetGenericResult
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }

    public SearchResult ToSearchResult()
    {
        return new GenericSearchResult
        {
            Url = Id,
            Title = Title,
            Image = Image,
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

public class ConsumetEnimeSearchResult
{
    public class Mappings
    {
        public int Mal { get; set; }
        public int Anidb { get; set; }
        public int Kitsu { get; set; }
        public int Anilist { get; set; }
        public int Thetvdb { get; set; }
        public int Anisearch { get; set; }
        public int Livechart { get; set; }
        [JsonPropertyName("notify.moe")] public string NotifyMoe { get; set; }
        [JsonPropertyName("anime-planet")] public string AnimePlanet { get; set; }
    }

    public string Id { get; set; }
    public int AnilistId { get; set; }
    public int MalId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public string Cover { get; set; }
    public int Rating { get; set; }
    public int ReleaseDate { get; set; }
    public string[] Genres { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    [JsonPropertyName("mappings")] public Mappings Map { get; set; }

    public SearchResult ToSearchResult()
    {
        return new EnimeSearchResult
        {
            Title = Title,
            Url = Id,
            Image = Image,
            Year = ReleaseDate.ToString(),
            Type = Type,
            MalId = MalId,
            AnilistId = AnilistId,
            Rating = Rating.ToString()
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