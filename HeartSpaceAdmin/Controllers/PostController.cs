using HeartSpaceAdmin.Models;
using HeartSpaceAdmin.Models.EFModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartSpaceAdmin.Controllers
{
	public class PostController : Controller
	{
		private readonly AppDbContext _context;

		public PostController(AppDbContext context)
		{
			_context = context;
		}

		// Index
		public IActionResult Index()
		{
			var posts = _context.Posts
				.Select(p => new PostViewModel
				{
					Id = p.Id,
					Title = p.Title,
					PublishTime = p.PublishTime,
					AuthorName = p.Member.Name,
					Disabled = p.Disabled
				})
				.ToList();

			return View(posts);
		}

		// Edit
		public IActionResult Edit(int id)
		{
			var post = _context.Posts
				.Include(p => p.PostComments)
				.ThenInclude(c => c.User)
				.FirstOrDefault(p => p.Id == id);

			if (post == null)
			{
				return NotFound();
			}

			var viewModel = new PostViewModel
			{
				Id = post.Id,
				Title = post.Title,
				PostContent = post.PostContent,
				PublishTime = post.PublishTime,
				AuthorName = post.Member.Name,
				Comments = post.PostComments.Select(c => new PostCommentViewModel
				{
					Id = c.Id,
					Comment = c.Comment,
					UserName = c.User.Name,
					CommentTime = c.CommentTime,
					Disabled = c.Disabled
				}).ToList()
			};

			return View(viewModel);
		}

		// Toggle Delete Post
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleDelete(int id)
		{
			var post = _context.Posts.FirstOrDefault(p => p.Id == id);
			if (post == null)
			{
				return NotFound();
			}

			post.Disabled = !post.Disabled;
			_context.SaveChanges();

			return RedirectToAction("Index");
		}

		// Toggle Comment Disabled
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleCommentDisabled(int id)
		{
			var comment = _context.PostComments.FirstOrDefault(c => c.Id == id);
			if (comment == null)
			{
				return NotFound();
			}

			comment.Disabled = comment.Disabled == true ? (bool?)false : (bool?)true;
			_context.SaveChanges();

			return RedirectToAction("Edit", new { id = comment.PostId });
		}
	}

}
