using DeveloperAssessment.Web.Models;
using DeveloperAssessment.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperAssessment.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
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
        public async Task<IActionResult> AddComment(int id, [FromForm] Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Name) || string.IsNullOrWhiteSpace(comment.EmailAddress) || string.IsNullOrWhiteSpace(comment.Message))
            {
                ModelState.AddModelError(string.Empty, "Name, email and your message are required.");
                BlogPost post = await _blogService.GetPostByIdAsync(id);
                return View("Index", post);
            }
            await _blogService.AddCommentToPostAsync(id, comment);
            return RedirectToAction("Index", new { id });
        }
    }
}
