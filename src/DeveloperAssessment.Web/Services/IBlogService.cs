using DeveloperAssessment.Web.Models;

namespace DeveloperAssessment.Web.Services
{
    public interface IBlogService
    {
        Task<BlogPost> GetPostByIdAsync(int id);
        Task AddCommentToPostAsync(int postId, Comment comment, string parentId);
        Task<BlogListViewModel> GetPaginatedPostsAsync(int pageNumber, int pageSize);
    }
}
