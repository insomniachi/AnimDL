using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumSubboard
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
