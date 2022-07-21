using System.Text.Json.Serialization;

namespace MalApi
{
    public class UserStatistics
    {
        [JsonPropertyName("num_items_watching")]
        public int WatchingCount { get; set; }

        [JsonPropertyName("num_items_completed")]
        public int CompletedCount { get; set; }

        [JsonPropertyName("num_items_on_hold")]
        public int OnHoldCount { get; set; }

        [JsonPropertyName("num_items_dropped")]
        public int DroppedCount { get; set; }

        [JsonPropertyName("num_items_plan_to_watch")]
        public int PlanedToWatchCount { get; set; }

        [JsonPropertyName("num_items")]
        public int TotalCount { get; set; }

        [JsonPropertyName("num_days_watching")]
        public float WatchingDays { get; set; }

        [JsonPropertyName("num_days_completed")]
        public float CompletedDays { get; set; }

        [JsonPropertyName("num_days_on_hold")]
        public float OnHoldDays { get; set; }

        [JsonPropertyName("num_days_dropped")]
        public float DroppedDays { get; set; }

        [JsonPropertyName("num_days_watched")]
        public float WatchedDays { get; set; }

        [JsonPropertyName("num_days")]
        public float TotalDays { get; set; }

        [JsonPropertyName("num_episodes")]
        public int TotalEpisodes { get; set; }

        [JsonPropertyName("num_times_rewatched")]
        public int TotalRewatch { get; set; }

        [JsonPropertyName("mean_score")]
        public float MeanScore { get; set; }
    }
}
