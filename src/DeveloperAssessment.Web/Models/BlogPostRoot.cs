using System.Text.Json.Serialization;

namespace DeveloperAssessment.Web.Models
{
    public class BlogPostRoot
    {
        [JsonPropertyName("blogPosts")]
        public List<BlogPost> BlogPosts { get; set; }
    }
}
