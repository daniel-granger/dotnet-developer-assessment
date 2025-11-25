using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeveloperAssessment.Web.Models;
using DeveloperAssessment.Web.Repositories;
using DeveloperAssessment.Web.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace DeveloperAssessment.Tests.Services
{
    public class BlogServiceTests
    {
        private readonly Mock<IBlogRepository> _repoMock;
        private readonly BlogService _service;

        public BlogServiceTests()
        {
            _repoMock = new Mock<IBlogRepository>();
            _service = new BlogService(_repoMock.Object);
        }

        [Fact]
        public async Task GetPostByIdAsync_WhenPostExists_ReturnsPost()
        {
            // Arrange
            var posts = new List<BlogPost>
            {
                new() { Id = 1, Title = "Blog Post 1" },
                new() { Id = 2, Title = "Blog Post 2" }
            };
            _repoMock
                .Setup(r => r.GetAllPostsAsync(It.IsAny<bool>()))
                .ReturnsAsync(posts);

            // Act
            var result = await _service.GetPostByIdAsync(2);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(2);
            result.Title.Should().Be("Blog Post 2");
        }

        [Fact]
        public async Task GetPostByIdAsync_WhenPostDoesNotExist_ReturnsEmptyBlogPost()
        {
            // Arrange
            var posts = new List<BlogPost>
            {
                new() { Id = 1, Title = "Blog Post 1" }
            };
            _repoMock
                .Setup(r => r.GetAllPostsAsync(It.IsAny<bool>()))
                .ReturnsAsync(posts);

            // Act
            var result = await _service.GetPostByIdAsync(99);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeNull();
        }

        [Fact]
        public async Task AddCommentToPostAsync_SetsDateAndCallsRepository()
        {
            // Arrange
            Comment captured = null!;
            _repoMock
                .Setup(r => r.AddCommentAsync(5, It.IsAny<Comment>(), "parent"))
                .Callback<int, Comment, string?>((pid, c, p) => captured = c)
                .Returns(Task.CompletedTask);

            var comment = new Comment
            {
                Id = "Test ID",
                Message = "This is a test comment.",
            };

            // Act
            var before = DateTime.UtcNow;
            await _service.AddCommentToPostAsync(5, comment, "parent");
            var after = DateTime.UtcNow;

            // Assert
            _repoMock.Verify(r => r.AddCommentAsync(5, It.IsAny<Comment>(), "parent"), Times.Once);
            captured.Should().NotBeNull();
            captured.Id.Should().Be("Test ID");
            captured.Message.Should().Be("This is a test comment.");

            captured.Date.Should().BeOnOrAfter(before);
            captured.Date.Should().BeOnOrBefore(after.AddSeconds(5));
        }

        [Fact]
        public async Task GetPaginatedPostsAsync_ReturnsCorrectPageAndMetadata()
        {
            // Arrange
            var total = 10;
            var posts = Enumerable.Range(1, total)
                .Select(i => new BlogPost { Id = i, Title = $"Post {i}" })
                .ToList();
            _repoMock.Setup(r => r.GetAllPostsAsync(It.IsAny<bool>())).ReturnsAsync(posts);
            
            int[] ints = { 4, 5, 6 };
            int pageNumber = 2;
            int pageSize = 3;

            // Act
            var result = await _service.GetPaginatedPostsAsync(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.CurrentPage.Should().Be(pageNumber);
            result.TotalPages.Should().Be((int)Math.Ceiling(total / (double)pageSize));
            result.BlogPosts.Should().HaveCount(pageSize);

            // Page 2 should contain posts 4, 5 & 6
            result.BlogPosts.Select(p => p.Id).Should().ContainInOrder(ints.Cast<int?>());
        }
    }
}
