using HeartSpace.Models;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static HeartSpace.Helpers.ImageHelper;

namespace HeartSpace.Controllers
{
<<<<<<< HEAD
	public class PostController : Controller
	{
	  private readonly PostService _postService;

	public PostController(PostService postService)
	{
		_postService = postService;
	}
=======
    public class PostController : Controller
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }
>>>>>>> origin/K

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
			model.MemberId = 1;
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

<<<<<<< HEAD
			model.PublishTime = DateTime.Now;
			var postId = _postService.AddPost(model);

		TempData["SuccessMessage"] = "貼文已成功儲存！";
		return RedirectToAction("PostDetails", new { id = postId });
	}
=======
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
>>>>>>> origin/K

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
				// 確保圖片為 Base64 格式，並處理 null 情況
				PostImg = !string.IsNullOrEmpty(post.PostImg) ? post.PostImg : null,
				CategoryName = _postService.GetCategoryNameById(post.CategoryId), // 獲取分類名稱
				MemberName = _postService.GetMemberNameById(post.MemberId),       // 獲取會員名稱
				PublishTime = post.PublishTime
			};

			return View(viewModel);
		}

		// 編輯貼文頁面
		[HttpGet]
		public ActionResult EditPost(int id)
		{
			var post = _postService.GetPostById(id);

			if (post == null)
			{
				return HttpNotFound("找不到該貼文！");
			}

			// 初始化分類選單
			post.CategoryList = _postService.GetCategories();
			return View(post);
		}

		// 編輯貼文動作
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditPost(CreatePostDto model, HttpPostedFileBase Image)
		{
			if (!ModelState.IsValid)
			{
				// 如果驗證失敗，重新填充分類選單
				model.CategoryList = _postService.GetCategories();
				return View(model);
			}

			try
			{
				// 處理圖片更新
				if (Image != null && Image.ContentLength > 0)
				{
					model.PostImg = Image.ToBase64String();
				}

			// 將 PostViewModel 轉換為 CreatePostDto
			var dto = new CreatePostDto
			{
				Id = model.Id,
				Title = model.Title,
				PostContent = model.PostContent,
				PostImg = model.PostImg,
				CategoryId = model.CategoryId,
				PublishTime = model.PublishTime,
				MemberId = model.MemberId,
			};

				// 呼叫 Service 更新貼文
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

		// 刪除貼文
		[HttpPost]
		public ActionResult DeletePost(int id)
		{
			try
			{
				_postService.DeletePost(id); // 呼叫 Service 刪除貼文
				TempData["SuccessMessage"] = "貼文已成功刪除！";
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "刪除失敗：" + ex.Message;
				return RedirectToAction("PostDetails", "Post", new { id = id });
			}
		}

		// 獲取當前用戶 ID（需根據你的系統調整）
		private int GetCurrentUserId()
		{
			var userName = User.Identity.Name; // 假設登入後的用戶名
			using (var db = new AppDbContext())
			{
				var member = db.Members.FirstOrDefault(m => m.Name == userName);

				if (member == null)
				{
					throw new Exception("找不到對應的會員，請檢查是否正確登入！");
				}

<<<<<<< HEAD
				return member.Id;
			}
		}
	}
=======
            // 將 PostViewModel 轉換為 CreatePostDto
            var dto = new CreatePostDto
            {
                Id = model.Id,
                Title = model.Title,
                PostContent = model.PostContent,
                PostImg = model.PostImg,
                CategoryId = model.CategoryId,
                PublishTime = model.PublishTime,
                MemberId = model.MemberId,
            };

            // 呼叫 Service 更新貼文
            _postService.UpdatePost(dto);

            TempData["SuccessMessage"] = "貼文已成功更新！";
            return RedirectToAction("PostDetails", "Post", new { id = model.Id });
        }

        // 刪除貼文
        [HttpPost]
        public ActionResult DeletePost(int id)
        {
            try
            {
                _postService.DeletePost(id); // 呼叫 Service 刪除貼文
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



        [HttpPost]
        public ActionResult AddCommentAndRefresh(int postId, string content)
        {
            try
            {
                // 確保 UserId 的存在（模擬登入或測試版本）
                int userId = ViewBag.CurrentUserId ?? 1; // 測試用 1 表示當前使用者 ID

                // 新增留言到資料庫
                using (var db = new AppDbContext())
                {
                    var newComment = new PostComment
                    {
                        PostId = postId,
                        UserId = userId,
                        Comment = content,
                        CommentTime = DateTime.Now
                    };
                    db.PostComments.Add(newComment);
                    db.SaveChanges();
                }

                TempData["SuccessMessage"] = "留言成功！";
                return RedirectToAction("PostDetails", new { id = postId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "留言失敗：" + ex.Message;
                return RedirectToAction("PostDetails", new { id = postId });
            }
        }

        [HttpPost]
        public ActionResult DeleteComment(int commentId)
        {
            try
            {
                if (!DeletedCommentIds.Contains(commentId))
                {
                    DeletedCommentIds.Add(commentId); // 將刪除的留言 ID 加入暫存列表
                }

                TempData["SuccessMessage"] = "留言已成功刪除！";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "刪除失敗：" + ex.Message;
            }

            // 返回貼文詳細頁
            return RedirectToAction("PostDetails", new { id = GetPostIdByCommentId(commentId) });
        }

        // 根據留言 ID 獲取貼文 ID 的方法
        private int GetPostIdByCommentId(int commentId)
        {
            using (var db = new AppDbContext())
            {
                return db.PostComments.Where(c => c.Id == commentId).Select(c => c.PostId).FirstOrDefault();
            }
        }


        // 獲取當前用戶 ID（需根據你的系統調整）
        private int GetCurrentUserId()
        {
            //var userName = User.Identity.Name; // 假設登入後的用戶名
            //using (var db = new AppDbContext())
            //{
            //    var member = db.Members.FirstOrDefault(m => m.Name == userName);

            //    if (member == null)
            //    {
            //        throw new Exception("找不到對應的會員，請檢查是否正確登入！");
            //    }

            //    return member.Id;
            //}

            return 1;
        }

    }
>>>>>>> origin/K
}
