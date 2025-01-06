using HeartSpace.Models.DTOs;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.Services;
using System;
using System.Linq;
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

            model.PublishTime = DateTime.Now;
            var postId = _postService.AddPost(model);

        TempData["SuccessMessage"] = "貼文已成功儲存！";
        return RedirectToAction("PostDetails", new { id = postId });
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

                return member.Id;
            }
        }
    }
}
