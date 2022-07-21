using System;
using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumTopicPost
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        public ForumTopicPostCreator CreatedBy { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

    }
}
