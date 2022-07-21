using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MalApi
{

    internal class AnimeRoot
    {
        [JsonPropertyName("node")]
        public Anime Anime { get; set; }

        [JsonPropertyName("ranking")]
        public Ranking Ranking { get; set; }
    }

    internal class AnimeListRoot
    {
        [JsonPropertyName("data")]
        public AnimeRoot[] AnimeList { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    internal class Paging
    {
        [JsonPropertyName("next")]
        public string Next { get; set; }

        [JsonPropertyName("previous")]
        public string Previous { get; set; }
    }

    internal class ForumCategoryRoot
    {
        [JsonPropertyName("categories")]
        public ForumCategory[] Categories { get; set; }
    }

    internal class ForumTopicDataRoot
    {
        [JsonPropertyName("data")]
        public ForumTopicData TopicData { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    internal class ForumTopicDetailRoot
    {
        [JsonPropertyName("data")]
        public ForumTopicDetails[] Details { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    internal class MangaListRoot
    {
        [JsonPropertyName("data")]
        public MangaRoot[] MangaList { get; set; }

        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }
    }

    internal class MangaRoot
    {
        [JsonPropertyName("node")]
        public Manga Manga { get; set; }

        [JsonPropertyName("ranking")]
        public Ranking Ranking { get; set; }
    }
}
