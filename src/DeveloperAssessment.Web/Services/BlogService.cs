using DeveloperAssessment.Web.Repositories;
using DeveloperAssessment.Web.Models;

namespace DeveloperAssessment.Web.Services
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;

        public BlogService(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }

        public async Task<BlogPost> GetPostByIdAsync(int id)
        {
            var posts = await _blogRepository.GetAllPostsAsync();
            return posts.FirstOrDefault(post => post.Id.HasValue && post.Id.Value == id) ?? new BlogPost();
        }

    }
}
