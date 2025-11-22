using DeveloperAssessment.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DeveloperAssessment.Web.Repositories
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "BlogPost_Cache";
        public BlogRepository(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        public async Task<List<BlogPost>> GetAllPostsAsync()
        {
            if (!_memoryCache.TryGetValue(CacheKey, out List<BlogPost> blogPosts))
            {
                // No cache hit, load data from JSON file.

                var client = _httpClientFactory.CreateClient();

                var res = await client.GetStringAsync("[replacewithJSONfile]");

                var root = JsonSerializer.Deserialize<BlogPostRoot>(res);
                blogPosts = root?.BlogPosts ?? new List<BlogPost>();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                _memoryCache.Set(CacheKey, blogPosts, cacheOptions);
            }

            return blogPosts;
        }
    }
}
