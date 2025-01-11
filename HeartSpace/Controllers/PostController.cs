using HeartSpace.Models;
using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (model.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "請選擇一個有效的分類！");
            }
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
		public ActionResult PostDetails(CreatePostDto model, int id)
		{
            var post = _postService.GetPostById(id);
            Debug.WriteLine($"CategoryName: {post.CategoryName}");



            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            if (post.Disabled && post.MemberId != GetCurrentUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "此貼文已關閉，您無權查看！");
            }

            using (var db = new AppDbContext())
            {
                var comments = db.PostComments
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CommentTime)
                    .ToList();

                ViewBag.CurrentUserId = GetCurrentUserId();

                var viewModel = new PostViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    PostContent = post.PostContent,
                    PostImg = post.PostImg,
                    CategoryName = post.CategoryName,
                    MemberNickName = db.Members.FirstOrDefault(m => m.Id == post.MemberId)?.NickName,
                    PublishTime = post.PublishTime,
                    MemberId = post.MemberId,
                    Disabled = post.Disabled,
                    Comments = comments.Select((c, index) => new CommentViewModel
                    {
                        PostId = c.PostId,
                        CommentId = c.Id,
                        UserId = c.UserId,
                        UserNickName = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.NickName,
                        UserImg = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.MemberImg,
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
            var post = _postService.GetPostById(id); // 取得貼文資訊
            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            // 檢查是否為該貼文的發文者
            int currentUserId = GetCurrentUserId(); // 假設有此方法取得目前登入者的 MemberId
            if (post.MemberId != currentUserId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "你沒有權限修改此貼文！");
            }

            post.CategoryList = _postService.GetCategories(); // 初始化類別清單
            post.OldPostImg = post.PostImg; // 初始化舊圖片路徑
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(CreatePostDto model, HttpPostedFileBase Image, bool? deleteOldImage)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryList = _postService.GetCategories();
                return View(model);
            }

            try
            {
                // 如果上傳了新圖片
                if (Image != null && Image.ContentLength > 0)
                {
                    // 生成新圖片名稱並保存
                    string fileName = GenerateFileName();
                    string savePath = Path.Combine(Server.MapPath("~/Images"), fileName);
                    Image.SaveAs(savePath);

                    // 更新資料庫圖片路徑
                    model.PostImg = $"/Images/{fileName}";

                    // 如果需要刪除舊圖片
                    if (!string.IsNullOrEmpty(model.OldPostImg))
                    {
                        var oldImagePath = Server.MapPath(model.OldPostImg);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }
                else if (deleteOldImage == true) // 如果勾選刪除舊圖片但未上傳新圖片
                {
                    // 檢查舊圖片路徑是否存在
                    if (!string.IsNullOrEmpty(model.OldPostImg))
                    {
                        var oldImagePath = Server.MapPath(model.OldPostImg);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath); // 刪除實體檔案
                            }
                            catch (Exception ex)
                            {
                                // Log 錯誤訊息以便調試
                                Console.WriteLine($"無法刪除檔案: {ex.Message}");
                            }
                        }
                    }
                    model.PostImg = null; // 將資料庫中的圖片路徑設為 null
                }

                // 更新資料庫
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
            var directoryPath = Server.MapPath("~/Images");

            // 檢查目錄是否存在，若不存在則建立
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 找出現有檔案中最大的數字
            var existingFiles = Directory.GetFiles(directoryPath)
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .Where(name => int.TryParse(name, out _))
                                         .Select(int.Parse)
                                         .OrderByDescending(x => x);

            int nextNumber = existingFiles.Any() ? existingFiles.First() + 1 : 1;

            // 回傳新檔案名稱
            return $"{nextNumber}.jpg";
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
                TempData["ErrorMessage"] = "找不到該貼文！";
                return RedirectToAction("PostDetails", new { id = postId });
            }

            if (post.MemberId != GetCurrentUserId())
            {
                TempData["ErrorMessage"] = "您無權限執行此操作！";
                return RedirectToAction("PostDetails", new { id = postId });
            }

            try
            {
                post.Disabled = true; // 永遠關閉
                _postService.UpdatePost(post); // 更新資料庫

                TempData["SuccessMessage"] = "貼文已成功關閉！";
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
                        CommentTime = DateTime.Now,
                         Disabled = false
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
                using (var db = new AppDbContext())
                {
                    var comment = db.PostComments.FirstOrDefault(c => c.Id == commentId);
                    if (comment == null)
                    {
                        TempData["ErrorMessage"] = "找不到該留言！";
                        return RedirectToAction("PostDetails", new { id = comment.PostId });
                    }

                    // 將 Disabled 狀態設為 1
                    comment.Disabled = true;
                    db.SaveChanges();
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
                return db.PostComments
                         .Where(c => c.Id == commentId)
                         .Select(c => c.PostId)
                         .FirstOrDefault();
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
