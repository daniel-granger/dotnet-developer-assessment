using DeveloperAssessment.Web.Models;
using Microsoft.AspNetCore.Components.Forms.Mapping;

namespace DeveloperAssessment.Web.Repositories
{
    public interface IBlogRepository
    {
        Task<List<BlogPost>> GetAllPostsAsync(bool force = true);
        Task AddCommentAsync(int postId, Comment comment, string parentId);
    }
}
