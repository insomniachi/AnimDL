using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicPollOption
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("votes")]
        public int Votes { get; set; }
    }
}
