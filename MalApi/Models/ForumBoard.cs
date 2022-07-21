using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumBoard
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("subboards")]
        public ForumSubboard[] Subboards { get; set; }
    }
}
