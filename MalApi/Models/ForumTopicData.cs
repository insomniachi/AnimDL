using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("posts")]
        public ForumTopicPost[] Posts { get; set; }

        [JsonPropertyName("poll")]
        public ForumTopicPoll[] Poll { get; set; }
    }
}
