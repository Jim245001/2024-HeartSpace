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

            // 將當前用戶的 ID 傳到前端
            ViewBag.CurrentUserId = GetCurrentUserId();
            ViewBag.DeletedCommentIds = PostController.DeletedCommentIds;


            using (var db = new AppDbContext())
            {
                // 取得所有留言資料，並按時間排序
                var comments = db.PostComments
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CommentTime)
                    .ToList();

                // 構建 ViewModel，包含留言
                var viewModel = new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    PostContent = post.PostContent,
                    PostImg = post.PostImg,
                    CategoryName = _postService.GetCategoryNameById(post.CategoryId),
                    MemberName = db.Members.FirstOrDefault(m => m.Id == post.MemberId)?.Name,
                    MemberImg = db.Members.FirstOrDefault(m => m.Id == post.MemberId)?.MemberImg != null
            ? "data:image/png;base64," + Convert.ToBase64String(db.Members.FirstOrDefault(m => m.Id == post.MemberId).MemberImg)
            : "data:image/png;base64,預設Base64字串",
                    PublishTime = post.PublishTime,
                    MemberId = post.MemberId, // 發文者的 ID


                    // 留言集合
                    Comments = comments.Select((c, index) => new CommentViewModel
                    {
                        PostId = c.PostId,
                        CommentId = c.Id,
                        UserId = c.UserId,
                        UserName = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.Name,
                        UserImg = c.Member.MemberImg != null
                  ? "data:image/png;base64," + Convert.ToBase64String(c.Member.MemberImg)
                  : "data:image/png;base64,預設Base64字串",
                        Comment = c.Comment,
                        CommentTime = c.CommentTime,
                        Disabled = c.Disabled ?? false,
                        FloorNumber = index + 1 // 樓層編號
                    }).ToList()
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
}
