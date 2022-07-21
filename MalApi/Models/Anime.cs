using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using MalApi.JsonConverters;
using MalApi.Models;

namespace MalApi
{
    public class Anime : BindableBase
    {
        private UserAnimeStatus _userStatus;

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
        public float MeanScore { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("popularity")]
        public int Popularity { get; set; }

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
        public string CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonPropertyName("media_type")]
        [JsonConverter(typeof(AnimeMediaTypeConverter))]
        public AnimeMediaType MediaType { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(AiringStatusConverter))]
        public AiringStatus Status { get; set; }

        [JsonPropertyName("my_list_status")]
        public UserAnimeStatus UserStatus
        {
            get => _userStatus;
            set 
            {
                if (_userStatus != null)
                {
                    _userStatus.PropertyChanged -= UserStatus_PropertyChanged;
                }
                _userStatus = value;

                _userStatus.PropertyChanged += UserStatus_PropertyChanged;
            }
        }

        [JsonPropertyName("num_episodes")]
        public int EpisodeCount { get; set; }

        [JsonPropertyName("start_season")]
        public Season StartSeason { get; set; }

        [JsonPropertyName("broadcast")]
        public BroadcastTime Broadcast { get; set; }

        [JsonPropertyName("source")]
        [JsonConverter(typeof(AnimeSourceConverter))]
        public AnimeSource Source { get; set; }

        [JsonPropertyName("average_episode_duration")]
        public int AverageEpisodeDuration { get; set; }

        [JsonPropertyName("rating")]
        public string CensorRating { get; set; }

        [JsonPropertyName("studios")]
        public Studio[] Studios { get; set; }

        [JsonPropertyName("pictures")]
        public Picture[] Pictures { get; set; }

        [JsonPropertyName("background")]
        public string Background { get; set; }

        public override string ToString() => Title;

        private void UserStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged($"UserStatus.{e.PropertyName}");
        }
    }
}
