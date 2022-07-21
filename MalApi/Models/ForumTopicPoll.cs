using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicPoll
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("question")]
        public string Question { get; set; }

        [JsonPropertyName("close")]
        public bool Close { get; set; }

        [JsonPropertyName("options")]
        public ForumTopicPollOption[] Options { get; set; }

    }
}
