namespace DeveloperAssessment.Web.Models
{
    public class BlogListViewModel
    {

        public IEnumerable<BlogPost> BlogPosts { get; set; } = Enumerable.Empty<BlogPost>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

    }
}
