using HeartSpace.Models;
using HeartSpace.Models.EFModel;
using HeartSpace.Models.EFModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using HeartSpace.Models.Services;

namespace HeartSpace.Controllers
{
    public class PostController : Controller
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        public ActionResult CreatePost()
        {

            //// 調用 Service 取得需要的分類清單
            //var categories = _postService.GetCategories();

            //// 建立 ViewModel 並將分類清單存入 ViewBag 或直接加入 ViewModel
            //var model = new PostViewModel();
            //ViewBag.Categories = new SelectList(categories, "Id", "CategoryName");

            //return View(model); // 返回 View 與初始化資料

            var model = new PostViewModel();
            ViewBag.Categories = _postService.GetCategories();
            return View(model);
        }
        [HttpPost]
        public ActionResult CreatePost(PostModel model, HttpPostedFileBase Image)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "CategoryName");
                return View(model);
            }

            // 處理圖片上傳
            byte[] postImage = null;
            if (Image != null && Image.ContentLength > 0)
            {
                using (var binaryReader = new System.IO.BinaryReader(Image.InputStream))
                {
                    postImage = binaryReader.ReadBytes(Image.ContentLength);
                }
            }

            // 新增貼文資料
            var post = new Post
            {
                Title = model.Title,
                PostContent = model.PostContent,
                PublishTime = DateTime.Now,
                CategoryId = model.CategoryId,
                MemberId = 1, // 替換為目前登入的會員 ID
                PostImg = postImage
            };

            try
            {
                db.Posts.Add(post);
                db.SaveChanges();

                // 儲存成功訊息
                TempData["SuccessMessage"] = "貼文已成功儲存！";

                // 跳轉到 PostDetails 頁面
                return RedirectToAction("PostDetails", "Post", new { id = post.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "儲存失敗：" + ex.Message);
                ViewBag.Categories = new SelectList(db.Categories, "Id", "CategoryName");
                return View(model);
            }
        }

        private int GetCurrentUserId()
        {

            var userName = User.Identity.Name; // 假設你儲存了用戶名
            var member = db.Members.FirstOrDefault(m => m.Name == userName);
            return member?.Id ?? throw new Exception("用戶尚未登入");

        }

        private AppDbContext db = new AppDbContext();

        public ActionResult PostDetails(int? id)
        {
            var post = db.Posts
               .Include(p => p.Member)
               .Include(p => p.PostComments.Select(c => c.Member))
               .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            // 轉換為 ViewModel
            var viewModel = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                PostContent = post.PostContent,
                PostImg = post.PostImg != null ? Convert.ToBase64String(post.PostImg) : null,
                PublishTime = post.PublishTime,
                MemberId = post.MemberId,
                MemberName = post.Member?.Name, // 如果 Member 存在
                MemberImg = post.Member?.MemberImg != null ? Convert.ToBase64String(post.Member.MemberImg) : null,
                Comments = post.PostComments.Select(c => new CommentViewModel
                {
                    CommentId = c.Id,
                    Content = c.Comment,
                    CreatedAt = c.CommentTime,
                    // 如果沒有 Member 關聯，移除以下兩行
                    AuthorName = c.Member?.Name,
                    AuthorImg = c.Member?.MemberImg != null ? Convert.ToBase64String(c.Member.MemberImg) : null,
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult EditPost(int? id)
        {
            if (!id.HasValue)
            {
                return HttpNotFound("貼文 ID 未提供！");
            }

            var post = db.Posts.FirstOrDefault(p => p.Id == id.Value);

            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            // 將 Post 轉換為 PostViewModel
            var model = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                PostContent = post.PostContent,
                PostImg = post.PostImg != null ? Convert.ToBase64String(post.PostImg) : null,
                PublishTime = post.PublishTime,
                MemberId = post.MemberId,
                CategoryId = post.CategoryId,
                CategoryList = db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }).ToList()

            };

            return View(model); // 傳遞 ViewModel 到檢視
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int id, PostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 如果驗證失敗，需要重新填充類別清單，否則下拉清單會是空的
                model.CategoryList = db.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                }).ToList();

                return View(model);
            }

            var post = db.Posts.FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            // 更新貼文數據
            post.Title = model.Title;
            post.PostContent = model.PostContent;
            post.CategoryId = model.CategoryId; // 保存選擇的類別 ID

            if (!string.IsNullOrEmpty(model.PostImg))
            {
                post.PostImg = Convert.FromBase64String(model.PostImg); // 將圖片更新
            }

            db.SaveChanges(); // 保存更改
            return RedirectToAction("PostDetails", new { id = post.Id });
        }
    }
}