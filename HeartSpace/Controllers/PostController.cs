using HeartSpace.Models;
using HeartSpace.Models.EFModel;
using HeartSpace.Models.EFModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace HeartSpace.Controllers
{
    public class PostController : Controller
    {
        // GET: Post
        public ActionResult CreatePost()
        {
            var model = new PostModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult CreatePost(PostModel model, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                // 處理圖片上傳
                if (Image != null)
                {
                    string path = Server.MapPath("~/Uploads/");
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    string fileName = System.IO.Path.GetFileName(Image.FileName);
                    string fullPath = System.IO.Path.Combine(path, fileName);
                    Image.SaveAs(fullPath);

                    model.ImagePath = "/Uploads/" + fileName; // 存儲圖片路徑
                }

                // TODO: 儲存到資料庫

                return RedirectToAction("Index");
            }

            return View(model);
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

            return View(post); // 傳遞貼文數據到檢視
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int id, Post model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // 若資料無效，返回編輯頁面
            }

            var post = db.Posts.FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            // 更新貼文數據
            post.Title = model.Title;
            post.PostContent = model.PostContent;
            post.PostImg = model.PostImg; // 假設圖片以 byte[] 儲存
            post.PublishTime = DateTime.Now; // 更新發佈時間（可選）

            db.SaveChanges(); // 保存變更

            return RedirectToAction("PostDetails", new { id = post.Id }); // 返回貼文詳情頁面
        }
    }
}