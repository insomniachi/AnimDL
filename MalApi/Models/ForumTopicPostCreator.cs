using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicPostCreator
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("forum_avator")]
        public string ForumAvator { get; set; }
    }
}
