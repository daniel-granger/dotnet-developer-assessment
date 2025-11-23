using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DeveloperAssessment.Web.Models
{
    public class Comment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Your name must be provided.")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date")]
        public DateTime? Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Your email must be provided.")]
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Your message must be provided.")]
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("replies")]
        public List<Comment> Replies { get; set; } = new();
    }
}
