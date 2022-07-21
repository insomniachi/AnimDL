using System;
using System.Text.Json.Serialization;
using MalApi.JsonConverters;
using MalApi.Models;

namespace MalApi
{
    public class Manga : BindableBase
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("main_picture")]
        public Picture MainPicture { get; set; }

        [JsonPropertyName("alternative_titles")]
        public AlternativeTitles AlternativeTitles { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("synopsis")]
        public string Synopsis { get; set; }

        [JsonPropertyName("mean")]
        public float Mean { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("popularity")]
        public int Populartiy { get; set; }

        [JsonPropertyName("num_list_users")]
        public int NumberOfUsers { get; set; }

        [JsonPropertyName("num_scoring_users")]
        public int NumberOfScoringUsers { get; set; }

        [JsonPropertyName("nsfw")]
        [JsonConverter(typeof(NsfwConverter))]
        public NsfwLevel NsfwLevel { get; set; }

        [JsonPropertyName("genres")]
        public Genre[] Genres { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("media_type")]
        [JsonConverter(typeof(MangaMediaTypeConverter))]
        public MangaMediaType MediaType { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(MangaStatusConverter))]
        public MangaStatus Status { get; set; }

        [JsonPropertyName("my_list_status")]
        public UserMangaStatus UserStatus { get; set; }

        [JsonPropertyName("num_volumes")]
        public int TotalVolumes { get; set; }

        [JsonPropertyName("num_chapters")]
        public int TotalChapters { get; set; }

        [JsonPropertyName("authors")]
        public Author[] Authors { get; set; }

        [JsonPropertyName("pictures")]
        public Picture[] Pictures { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
