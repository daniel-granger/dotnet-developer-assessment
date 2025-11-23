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
        public async Task<IActionResult> AddComment(int id, [FromForm] Comment comment, string? parentId = null)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _blogService.AddCommentToPostAsync(id, comment, parentId);
            return StatusCode(201, new
            {
                id = comment.Id,
                name = comment.Name,
                date = comment.Date,
                emailAddress = comment.EmailAddress,
                message = comment.Message
            });
        }
    }
}
