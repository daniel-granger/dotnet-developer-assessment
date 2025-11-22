using DeveloperAssessment.Web.Models;

namespace DeveloperAssessment.Web.Repositories
{
    public interface IBlogRepository
    {
        Task<List<BlogPost>> GetAllPostsAsync();
    }
}
