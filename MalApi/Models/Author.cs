using System.Text.Json.Serialization;

namespace MalApi
{
    public class Author
    {
        [JsonPropertyName("node")]
        public Person Person { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        public override string ToString()
        {
            return Person.ToString();
        }
    }
}
