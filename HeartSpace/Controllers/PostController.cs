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
using System.Data.Entity;
using System.Web.Security;

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
			model.MemberId = GetCurrentMemberId();

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
    try
    {
        // 確保資料夾存在
        HeartSpaceImage.ImageHelper.EnsureDirectoryExists();
        var uploadDir = HeartSpaceImage.ImageHelper.RootPath;

        // 取得副檔名
        var fileExtension = Path.GetExtension(Image.FileName).ToLower();

        // 檢查格式
        if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png" && fileExtension != ".webp")
        {
            ModelState.AddModelError("Image", "僅接受 JPG、JPEG、PNG、WEBP 格式圖片。");
            return View(model);
        }

                    // 搜尋現有檔案，找出最大編號
                    var existingFiles = Directory.GetFiles(uploadDir, "Post_*.jpg");
                    int nextNumber = existingFiles
                        .Select(file => Path.GetFileNameWithoutExtension(file))
                        .Where(name => name.StartsWith("Post_"))
                        .Select(name => int.TryParse(name.Replace("Post_", ""), out int num) ? num : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    // 生成新檔案名稱
                    string fileName = $"Post_{nextNumber}.jpg";
                    string savePath = Path.Combine(uploadDir, fileName);

                    // 儲存圖片
                    Image.SaveAs(savePath);

                    if (fileExtension == ".webp")
        {
            // 處理 .webp 圖片轉換
            using (var webpStream = Image.InputStream)
            {
                using (var bitmap = new System.Drawing.Bitmap(webpStream))
                {
                    bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    System.Diagnostics.Debug.WriteLine($"WEBP 已轉換為 JPG 並儲存到：{savePath}");
                }
            }
        }
        else
        {
            // 直接儲存非 .webp 圖片
            Image.SaveAs(savePath);
            System.Diagnostics.Debug.WriteLine($"圖片已儲存到路徑：{savePath}");
        }

        // 更新資料庫的圖片路徑
        model.PostImg = $"https://localhost:44378/Images/{fileName}";
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("Image", $"圖片上傳失敗：{ex.Message}");
        return View(model);
    }
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

			if (post.Disabled && post.MemberId != GetCurrentMemberId())
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "此貼文已關閉，您無權查看！");
			}

            using (var db = new AppDbContext())
            {
                var comments = db.PostComments
                        .Include(c => c.Member) // 預加載 Member 資料
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CommentTime)
                    .ToList();

				ViewBag.CurrentUserId = GetCurrentMemberId();

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
                    MemberImg = db.Members.FirstOrDefault(m => m.Id == post.MemberId)?.MemberImg, // 新增這行
                    Disabled = post.Disabled,
                    Comments = comments.Select((c, index) => new CommentViewModel
                    {
                        PostId = c.PostId,
                        CommentId = c.Id,
                        UserId = c.UserId,
                        UserNickName = db.Members.FirstOrDefault(m => m.Id == c.UserId)?.NickName,
                        UserImg = c.Member != null ? c.Member.MemberImg : null, // 正確加載留言者頭像
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
			int currentUserId = GetCurrentMemberId(); // 假設有此方法取得目前登入者的 MemberId
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
                    // 確保資料夾存在
                    HeartSpaceImage.ImageHelper.EnsureDirectoryExists();
                    var uploadDir = HeartSpaceImage.ImageHelper.RootPath;

                    // 取得副檔名
                    var fileExtension = Path.GetExtension(Image.FileName).ToLower();

                    // 支援的檔案格式檢查
                    if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png" && fileExtension != ".webp")
                    {
                        ModelState.AddModelError("Image", "僅接受 JPG、JPEG、PNG、WEBP 格式圖片。");
                        return View(model);
                    }

                    // 搜尋現有檔案，找出最大編號
                    var existingFiles = Directory.GetFiles(uploadDir, "Post_*.jpg");
                    int nextNumber = existingFiles
                        .Select(file => Path.GetFileNameWithoutExtension(file))
                        .Where(name => name.StartsWith("Post_"))
                        .Select(name => int.TryParse(name.Replace("Post_", ""), out int num) ? num : 0)
                        .DefaultIfEmpty(0)
                        .Max() + 1;

                    // 生成新檔案名稱
                    string fileName = $"Post_{nextNumber}.jpg";
                    string savePath = Path.Combine(uploadDir, fileName);

                    // 儲存圖片
                    Image.SaveAs(savePath);

                    // 更新資料庫圖片路徑
                    model.PostImg = $"https://localhost:44378/Images/{fileName}";

                    // 如果需要刪除舊圖片
                    if (!string.IsNullOrEmpty(model.OldPostImg))
                    {
                        var oldImagePath = Path.Combine(uploadDir, Path.GetFileName(model.OldPostImg));
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
                        var uploadDir = HeartSpaceImage.ImageHelper.RootPath;
                        var oldImagePath = Path.Combine(uploadDir, Path.GetFileName(model.OldPostImg));
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

            // 找出現有檔案中以 PostImg_ 開頭的檔案，並提取數字部分
            var existingFiles = Directory.GetFiles(directoryPath)
                                         .Select(Path.GetFileNameWithoutExtension)
                                         .Where(name => name.StartsWith("PostImg_") && int.TryParse(name.Substring("PostImg_".Length), out _))
                                         .Select(name => int.Parse(name.Substring("PostImg_".Length)))
                                         .OrderByDescending(x => x);

			int nextNumber = existingFiles.Any() ? existingFiles.First() + 1 : 1;

            // 回傳新檔案名稱（加上 PostImg_ 前綴）
            return $"PostImg_{nextNumber}.jpg";

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

			if (post.MemberId != GetCurrentMemberId())
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
		private int GetCurrentMemberId()
		{
			if (!User.Identity.IsAuthenticated)
			{
				throw new UnauthorizedAccessException("使用者未登入。");
			}

			var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie == null)
			{
				throw new UnauthorizedAccessException("未找到身份驗證 Cookie。");
			}

			var ticket = FormsAuthentication.Decrypt(authCookie.Value);
			if (ticket == null || string.IsNullOrEmpty(ticket.UserData))
			{
				throw new UnauthorizedAccessException("身份驗證票據無效。");
			}

			// 解析 UserData 格式 "MemberId|Role"
			var parts = ticket.UserData.Split('|');
			if (int.TryParse(parts[0], out int memberId))
			{
				return memberId; // 返回 MemberId
			}

			throw new Exception("票據中的用戶 ID 無效。");
		}


	}
}
