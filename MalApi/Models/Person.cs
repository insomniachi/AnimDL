using System.Text.Json.Serialization;

namespace MalApi
{
    public class Person
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
