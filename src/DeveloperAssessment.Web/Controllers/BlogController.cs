using DeveloperAssessment.Web.Models;
using DeveloperAssessment.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DeveloperAssessment.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IWebHostEnvironment _env;
        public BlogController(IBlogService blogService, ICompositeViewEngine viewEngine, IWebHostEnvironment env)
        {
            _blogService = blogService;
            _viewEngine = viewEngine;
            _env = env;
        }

        [Route("blog/{id:int}")]
        public async Task<IActionResult> Index(int id)
        {
            BlogPost post = await _blogService.GetPostByIdAsync(id);

            if (!post.Id.HasValue)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        [Route("blog/{id:int}/comment")]
        public async Task<IActionResult> AddComment(int id, [FromForm] Comment comment, IFormFile? commentAttachment, string? parentId = null)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (commentAttachment != null && commentAttachment.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "comment-attachments");

                if (!Directory.Exists(uploadsFolder)) { Directory.CreateDirectory(uploadsFolder); }

                // Get extension of filename
                var ext = Path.GetExtension(commentAttachment.FileName).ToLowerInvariant();
                
                string uniqueFileName = Guid.NewGuid().ToString() + ext;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await commentAttachment.CopyToAsync(fileStream);
                }

                comment.AttachmentPath = "/comment-attachments/" + uniqueFileName;
                // Preserve the original file name to display to users
                comment.AttachmentName = commentAttachment.FileName;
            }

            await _blogService.AddCommentToPostAsync(id, comment, parentId);

            string htmlContent = await RenderViewToStringAsync("_Comment", comment);

            return StatusCode(201, new
            {
                html = htmlContent,
                parentId
            });
        }

        [Route("blog")]
        public async Task<IActionResult> List(int page = 1)
        {
            const int pageSize = 6;

            BlogListViewModel model = await _blogService.GetPaginatedPostsAsync(page, pageSize);

            return View("List", model);
        }

        private async Task<string> RenderViewToStringAsync(string viewName, Comment model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                IViewEngine viewEngine = _viewEngine;
                ViewEngineResult viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.Success == false)
                {
                    return $"A view with the name {viewName} could not be found";
                }

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
