using DeveloperAssessment.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DeveloperAssessment.Web.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "BlogPost_Cache";
        private string _jsonFilePath { get; set; }
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        public BlogRepository(IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            _env = env;
            _memoryCache = memoryCache;
            _jsonFilePath = Path.Combine(_env.ContentRootPath, "Data", "Blog-Posts.json");
        }

        public async Task<List<BlogPost>> GetAllPostsAsync(bool force = false)
        {
            if (!_memoryCache.TryGetValue(CacheKey, out List<BlogPost> blogPosts) || force)
            {
                // No cache hit, load data from JSON file.

                if (!File.Exists(_jsonFilePath))
                {
                    // Invalid/missing file.
                    return new List<BlogPost>();
                }

                var res = await File.ReadAllTextAsync(_jsonFilePath);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var root = JsonSerializer.Deserialize<BlogPostRoot>(res, options);
                blogPosts = root?.BlogPosts ?? new List<BlogPost>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(CacheKey, blogPosts, cacheOptions);
            }

            return blogPosts;
        }

        public async Task AddCommentAsync(int postId, Comment comment, string parentId)
        {
            // Relatively simple approach to avoid concurrency issues when writing to the file.
            await _fileLock.WaitAsync();

            try
            {
                var blogPosts = await GetAllPostsAsync(true); // Force reload to get the latest data

                var post = blogPosts.FirstOrDefault(p => p.Id == postId);

                if (post != null)
                {

                    if (!string.IsNullOrWhiteSpace(parentId))
                    {
                        var parentComment = FindCommentRecursive(post.Comments, parentId);
                        if (parentComment != null)
                        {
                            parentComment.Replies.Add(comment);
                        }
                        else
                        {
                            // Parent comment not found
                            return;
                        }
                    }
                    else
                    {
                        // Add as a top-level comment
                        post.Comments.Add(comment);
                    }

                    BlogPostRoot root = new BlogPostRoot
                    {
                        BlogPosts = blogPosts
                    };

                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    var json = JsonSerializer.Serialize(root, options);

                    // Write to file
                    await File.WriteAllTextAsync(_jsonFilePath, json);

                    // Update cache with new data - saves another user needing to request this.
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    _memoryCache.Set(CacheKey, blogPosts, cacheOptions);

                    var file = Path.Combine(_env.ContentRootPath, "Data", "Blog-Posts.json");
                }
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private Comment FindCommentRecursive(List<Comment> comments, string targetId)
        {
            if (comments is null) return null;

            foreach (var comment in comments)
            {
                if (comment.Id == targetId)
                {
                    return comment;
                }
                var foundInReplies = FindCommentRecursive(comment.Replies, targetId);
                if (foundInReplies != null)
                {
                    return foundInReplies;
                }
            }
            return null;
        }
    }
}
