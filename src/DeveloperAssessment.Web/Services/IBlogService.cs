using DeveloperAssessment.Web.Models;

namespace DeveloperAssessment.Web.Services
{
    public interface IBlogService
    {
        Task<BlogPost> GetPostByIdAsync(int id);
    }
}
