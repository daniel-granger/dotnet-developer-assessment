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
        public BlogRepository(IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            _env = env;
            _memoryCache = memoryCache;
        }

        public async Task<List<BlogPost>> GetAllPostsAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out List<BlogPost> blogPosts))
            {
                // No cache hit, load data from JSON file.

                var file = Path.Combine(_env.ContentRootPath, "Data", "Blog-Posts.json");

                if (!File.Exists(file))
                {
                    // Invalid/missing file.
                    return new List<BlogPost>();
                }

                var res = await File.ReadAllTextAsync(file);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var root = JsonSerializer.Deserialize<BlogPostRoot>(res, options);
                blogPosts = root?.BlogPosts ?? new List<BlogPost>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(CacheKey, blogPosts, cacheOptions);
            }

            return blogPosts;
        }
    }
}
