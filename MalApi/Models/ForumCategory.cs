using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MalApi
{
    public class ForumCategory
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("boards")]
        public ForumBoard[] Boards { get; set; }
    }
}
