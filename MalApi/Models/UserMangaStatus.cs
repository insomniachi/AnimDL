using System;
using System.Text.Json.Serialization;

namespace MalApi
{
    public class UserMangaStatus
    {
        [JsonPropertyName("status")]
        public MangaStatus Status { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("num_volumes_read")]
        public int VolumesRead { get; set; }

        [JsonPropertyName("num_chapters_read")]
        public int ChaptersRead { get; set; }

        [JsonPropertyName("is_rereading")]
        public bool IsReReading { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("finish_date")]
        public DateTime FinishDate { get; set; }

        [JsonPropertyName("priority")]
        public int Prority { get; set; }

        [JsonPropertyName("num_times_reread")]
        public int ReReadCount { get; set; }

        [JsonPropertyName("reread_value")]
        public int ReReadValue { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
