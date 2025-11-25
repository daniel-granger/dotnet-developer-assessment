using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeveloperAssessment.Web.Controllers;
using DeveloperAssessment.Web.Models;
using DeveloperAssessment.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Xunit;

namespace DeveloperAssessment.Tests.Controllers
{
    public class BlogControllerTests
    {
        [Fact]
        public async Task Index_WhenPostExists_ReturnsViewWithModel()
        {
            // Arrange
            var svcMock = new Mock<IBlogService>();
            svcMock.Setup(s => s.GetPostByIdAsync(5))
                   .ReturnsAsync(new BlogPost { Id = 5, Title = "Test Title", HtmlContent = "Test Content" });

            var viewEngine = new Mock<ICompositeViewEngine>();
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            var controller = new BlogController(svcMock.Object, viewEngine.Object, env.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            // Act
            var result = await controller.Index(5);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var vr = (ViewResult)result;
            vr.Model.Should().BeOfType<BlogPost>().Which.Id.Should().Be(5);
        }

        [Fact]
        public async Task Index_WhenPostMissing_ReturnsNotFound()
        {
            // Arrange
            var svcMock = new Mock<IBlogService>();
            svcMock.Setup(s => s.GetPostByIdAsync(99)).ReturnsAsync(new BlogPost()); // Id null

            var viewEngine = new Mock<ICompositeViewEngine>();
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            var controller = new BlogController(svcMock.Object, viewEngine.Object, env.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            // Act
            var result = await controller.Index(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task List_ReturnsListViewWithModel()
        {
            // Arrange
            var svcMock = new Mock<IBlogService>();
            var model = new BlogListViewModel();
            svcMock.Setup(s => s.GetPaginatedPostsAsync(1, 6)).ReturnsAsync(model);

            var viewEngine = new Mock<ICompositeViewEngine>();
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            var controller = new BlogController(svcMock.Object, viewEngine.Object, env.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            // Act
            var result = await controller.List(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var vr = (ViewResult)result;
            vr.ViewName.Should().Be("List");
            vr.Model.Should().BeSameAs(model);
        }

        [Fact]
        public async Task AddComment_WhenModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            var svcMock = new Mock<IBlogService>();
            var viewEngine = new Mock<ICompositeViewEngine>();
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            var controller = new BlogController(svcMock.Object, viewEngine.Object, env.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            controller.ModelState.AddModelError("Author", "Required");

            var comment = new Comment { Message = "Test Message" };

            // Act
            var result = await controller.AddComment(1, comment, null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddComment_WithAttachment_SavesFile_CallsService_AndReturns201WithHtml()
        {
            // Arrange
            var svcMock = new Mock<IBlogService>();
            var viewEngine = new Mock<ICompositeViewEngine>();
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();

            // Set up temporary web root
            var tempRoot = Path.Combine(Path.GetTempPath(), "blogtests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);
            env.SetupGet(e => e.WebRootPath).Returns(tempRoot);

            // Fake view that renders deterministic HTML
            var fakeView = new FakeView("<div>COMMENT_HTML</div>");
            viewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), "_Comment", false))
                      .Returns(ViewEngineResult.Found("_Comment", fakeView));

            // Capture comment passed to service
            Comment captured = null!;
            svcMock.Setup(s => s.AddCommentToPostAsync(10, It.IsAny<Comment>(), null))
                   .Callback<int, Comment, string?>((id, c, p) => captured = c)
                   .Returns(Task.CompletedTask);

            // Prepare in-memory form file
            var content = "Test File Contents";
            var bytes = Encoding.UTF8.GetBytes(content);
            using var ms = new MemoryStream(bytes);
            var formFile = new FormFile(ms, 0, ms.Length, "commentAttachment", "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var controller = new BlogController(svcMock.Object, viewEngine.Object, env.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            var commentModel = new Comment { Name = "Test Name", Message = "Test Message" };

            // Act
            var result = await controller.AddComment(10, commentModel, formFile, null);

            try
            {
                // Assert repository/service called
                svcMock.Verify(s => s.AddCommentToPostAsync(10, It.IsAny<Comment>(), null), Times.Once);

                // Attachment metadata set on comment passed to service
                captured.Should().NotBeNull();
                captured.AttachmentName.Should().Be("test.txt");
                captured.AttachmentPath.Should().StartWith("/comment-attachments/");

                // File exists on disk under temp root
                var attachmentsDir = Path.Combine(tempRoot, "comment-attachments");
                Directory.Exists(attachmentsDir).Should().BeTrue();
                Directory.GetFiles(attachmentsDir).Should().NotBeEmpty();

                // Result is 201 containing html and parentId
                result.Should().BeOfType<ObjectResult>();
                var obj = (ObjectResult)result;
                obj.StatusCode.Should().Be(201);
                var htmlProp = obj.Value!.GetType().GetProperty("html")!;
                var parentProp = obj.Value.GetType().GetProperty("parentId")!;
                htmlProp.GetValue(obj.Value).Should().Be("<div>COMMENT_HTML</div>");
                parentProp.GetValue(obj.Value).Should().BeNull();
            }
            finally
            {
                // Cleanup
                try { Directory.Delete(tempRoot, true); } catch { }
            }
        }

        // Minimal IView implementation to return predictable HTML during tests
        private class FakeView : IView
        {
            private readonly string _output;
            public FakeView(string output) => _output = output;
            public string Path => "_Comment";

            public Task RenderAsync(ViewContext context)
            {
                // Write directly to the view writer
                context.Writer.Write(_output);
                return Task.CompletedTask;
            }
        }
    }
}
