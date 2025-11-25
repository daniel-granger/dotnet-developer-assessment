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

        public async Task AddCommentToPostAsync(int postId, Comment comment, string parentId = null)
        {
            comment.Date = DateTime.UtcNow;
            await _blogRepository.AddCommentAsync(postId, comment, parentId);
        }

        public async Task<BlogListViewModel> GetPaginatedPostsAsync(int pageNumber, int pageSize)
        {
            var allPosts = await _blogRepository.GetAllPostsAsync();

            var totalPosts = allPosts.Count();
            var totalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

            var paginatedPosts = allPosts
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return new BlogListViewModel
            {
                BlogPosts = paginatedPosts,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }

    }
}
