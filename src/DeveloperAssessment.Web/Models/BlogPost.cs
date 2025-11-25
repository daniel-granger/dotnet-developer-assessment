using System.Text.Json.Serialization;

namespace DeveloperAssessment.Web.Models
{
    public sealed class BlogPost
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; set; }
        [JsonPropertyName("comments")]
        public List<Comment> Comments { get; set; } = new();
    }
}
