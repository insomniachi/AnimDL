using System;
using System.Text.Json.Serialization;
using MalApi.JsonConverters;
using MalApi.Models;

namespace MalApi
{
    public class UserAnimeStatus : BindableBase
    {
        private AnimeStatus _status;
        private int _watchedEpisodes;
        private int _score;
        private DateTime _updatedAt;
        private bool _isRewatching;

        [JsonPropertyName("status")]
        [JsonConverter(typeof(AnimeStatusConverter))]
        public AnimeStatus Status 
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        [JsonPropertyName("score")]
        public int Score
        {
            get => _score;
            set => SetProperty(ref _score, value);
        }


        [JsonPropertyName("num_episodes_watched")]
        public int WatchedEpisodes
        {
            get => _watchedEpisodes;
            set => SetProperty(ref _watchedEpisodes, value);
        }

        [JsonPropertyName("is_rewatching")]
        public bool IsRewatching 
        {
            get => _isRewatching;
            set => SetProperty(ref _isRewatching, value);
        }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("finish_date")]
        public DateTime FinishDate { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("num_times_rewatched")]
        public int RewatchCount { get; set; }

        [JsonPropertyName("rewatch_value")]
        public int RewatchValue { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("comments")]
        public string Comments { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }
    }
}
