using HeartSpace.Models;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Services;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static HeartSpace.Helpers.ImageHelper;

namespace HeartSpace.Controllers
{
	public class PostController : Controller
	{
		private readonly PostService _postService;

		public PostController(PostService postService)
		{
			_postService = postService;
		}

		[HttpGet]
		public ActionResult CreatePost()
		{
			var model = new CreatePostDto();
			ViewBag.Categories = _postService.GetCategories();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreatePost(CreatePostDto model, HttpPostedFileBase Image)
		{
			model.MemberId = GetCurrentUserId();
			if (!ModelState.IsValid)
			{
				ViewBag.Categories = _postService.GetCategories();
				return View(model);
			}

			// 處理圖片上傳
			if (Image != null && Image.ContentLength > 0)
			{
				model.PostImg = Image.ToBase64String();
			}

			model.PublishTime = DateTime.Now;

			try
			{
				var postId = _postService.AddPost(model);
				TempData["SuccessMessage"] = "貼文已成功儲存！";
				return RedirectToAction("PostDetails", new { id = postId });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "儲存貼文時發生錯誤，請稍後再試");
				ViewBag.Categories = _postService.GetCategories();
				return View(model);
			}
		}

		[HttpGet]
		public ActionResult PostDetails(int id)
		{
			var post = _postService.GetPostById(id);

			if (post == null)
			{
				return HttpNotFound("找不到該貼文！");
			}

			var viewModel = new Models.PostViewModel
			{
				Id = post.Id,
				Title = post.Title,
				PostContent = post.PostContent,
				PostImg = post.PostImg ?? null, // 確保圖片為 Base64 格式，並處理 null 情況
				CategoryName = _postService.GetCategoryNameById(post.CategoryId),
				MemberName = _postService.GetMemberNameById(post.MemberId),
				PublishTime = post.PublishTime
			};

			return View(viewModel);
		}

		[HttpGet]
		public ActionResult EditPost(int id)
		{
			var post = _postService.GetPostById(id);

			if (post == null)
			{
				return HttpNotFound("找不到該貼文！");
			}

			post.CategoryList = _postService.GetCategories();
			return View(post);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditPost(CreatePostDto model, HttpPostedFileBase Image)
		{
			if (!ModelState.IsValid)
			{
				model.CategoryList = _postService.GetCategories();
				return View(model);
			}

			try
			{
				if (Image != null && Image.ContentLength > 0)
				{
					model.PostImg = Image.ToBase64String();
				}

				_postService.UpdatePost(model);

				TempData["SuccessMessage"] = "貼文已成功更新！";
				return RedirectToAction("PostDetails", "Post", new { id = model.Id });
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "更新失敗：" + ex.Message);
				model.CategoryList = _postService.GetCategories();
				return View(model);
			}
		}

		[HttpPost]
		public ActionResult DeletePost(int id)
		{
			try
			{
				_postService.DeletePost(id);
				TempData["SuccessMessage"] = "貼文已成功刪除！";
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "刪除失敗：" + ex.Message;
				return RedirectToAction("PostDetails", "Post", new { id = id });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult TogglePostStatus(int postId)
		{
			var post = _postService.GetPostById(postId);
			if (post == null)
			{
				return HttpNotFound("找不到該貼文！");
			}

			if (post.MemberId != GetCurrentUserId())
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "您無權限執行此操作");
			}

			try
			{
				post.Disabled = !post.Disabled;
				_postService.UpdatePost(post);
				TempData["SuccessMessage"] = post.Disabled ? "貼文已關閉！" : "貼文已重新開啟！";
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "操作失敗：" + ex.Message;
			}

			return RedirectToAction("PostDetails", new { id = postId });
		}

		private int GetCurrentUserId()
		{
			// TODO: 根據您的認證系統，返回當前用戶 ID
			return 1; // 測試時直接返回用戶 ID 1
		}
	}
}
