using System;
using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicDetails
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        public ForumTopicPostCreator CreatedBy { get; set; }

        [JsonPropertyName("number_of_posts")]
        public int TotalPosts { get; set; }

        [JsonPropertyName("last_post_created_at")]
        public DateTime LastPostCreatedAt { get; set; }

        [JsonPropertyName("last_post_created_by")]
        public ForumTopicPostCreator LastPostCreatedBy { get; set; }

        [JsonPropertyName("is_locked")]
        public bool IsLocked { get; set; }
    }
}
