using HeartSpace.Models;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
	public class PostController : Controller
	{
		private readonly PostService _postService;

		public PostController(PostService postService)
		{
			_postService = postService;
		}

        // 在控制器類別內，新增一個靜態屬性來存儲刪除的留言 ID
        private static readonly List<int> DeletedCommentIds = new List<int>();

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
                string fileName = GenerateFileName(); // 產生唯一檔案名稱
                string savePath = Path.Combine(Server.MapPath("~/Images"), fileName);
                Image.SaveAs(savePath); // 儲存圖片到 Images 資料夾
                model.PostImg = $"/Images/{fileName}"; // 儲存路徑到資料庫
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

            using (var db = new AppDbContext())
            {
                var comments = db.PostComments
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CommentTime)
                    .ToList();
                ViewBag.CurrentUserId = 1; //測試

                var viewModel = new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    PostContent = post.PostContent,
                    PostImg = post.PostImg,
                    CategoryName = _postService.GetCategoryNameById(post.CategoryId),
                    MemberNickName = db.Members.FirstOrDefault(m => m.Id == post.MemberId)?.NickName, // 改為 NickName
                    PublishTime = post.PublishTime,
                    MemberId = post.MemberId,
                    Comments = comments.Select((c, index) => new CommentViewModel
                    {
                        PostId = c.PostId,
                        CommentId = c.Id,
                        UserId = c.UserId,
                        UserNickName = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.NickName,
                        UserImg = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.MemberImg ?? null,
                        Comment = c.Comment,
                        CommentTime = c.CommentTime,
                        Disabled = c.Disabled ?? false,
                        FloorNumber = index + 1
                    }).ToList()
                };

                return View(viewModel);
            }
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
        public ActionResult EditPost(CreatePostDto model, HttpPostedFileBase Image, bool deleteOldImage)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryList = _postService.GetCategories();
                return View(model);
            }

            try
            {
                // 刪除舊圖片的邏輯
                if (deleteOldImage && !string.IsNullOrEmpty(model.OldPostImg))
                {
                    var oldImagePath = Server.MapPath(model.OldPostImg);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath); // 刪除舊圖片文件
                    }
                    model.PostImg = null; // 清空圖片路徑
                }

                // 如果上傳了新圖片
                if (Image != null && Image.ContentLength > 0)
                {
                    if (!deleteOldImage && !string.IsNullOrEmpty(model.OldPostImg))
                    {
                        // 如果沒勾選刪除舊圖片但上傳新圖片，詢問用戶確認
                        TempData["ConfirmationMessage"] = "您已上傳新圖片，是否要替換舊圖片？";
                        model.CategoryList = _postService.GetCategories();
                        return View(model);
                    }

                    // 保存新圖片
                    string fileName = GenerateFileName();
                    string savePath = Path.Combine(Server.MapPath("~/Images"), fileName);
                    Image.SaveAs(savePath);
                    model.PostImg = $"/Images/{fileName}";
                }

                // 更新貼文
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


        private string GenerateFileName()
        {
            using (var db = new AppDbContext())
            {
                int nextId = db.Posts.Max(p => (int?)p.Id) ?? 0 + 1; // 找出目前最大 ID 並加 1
                return $"{nextId}.jpg";
            }
        }

            [HttpPost]
		public ActionResult DeletePost(int id)
		{
            try
            {
                var post = _postService.GetPostById(id);
                if (post != null)
                {
                    // 刪除圖片檔案
                    var imagePath = Server.MapPath(post.PostImg);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    _postService.DeletePost(id);
                   
                }
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
}
