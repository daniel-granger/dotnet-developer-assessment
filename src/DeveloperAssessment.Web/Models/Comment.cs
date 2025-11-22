using System.Text.Json.Serialization;

namespace DeveloperAssessment.Web.Models
{
    public class Comment
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
