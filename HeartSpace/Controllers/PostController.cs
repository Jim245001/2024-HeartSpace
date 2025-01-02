using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

            if (!id.HasValue)
            {
                return HttpNotFound("貼文 ID 未提供！");
            }

            // 不使用 Include，直接查詢貼文
            var post = db.Posts.FirstOrDefault(p => p.Id == id.Value);

            if (post == null)
            {
                return HttpNotFound("找不到該貼文！");
            }

            return View(post); // 傳遞貼文到檢視
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
            post.Img = model.Img; // 假設圖片以 byte[] 儲存
            post.PublishTime = DateTime.Now; // 更新發佈時間（可選）

            db.SaveChanges(); // 保存變更

            return RedirectToAction("PostDetails", new { id = post.Id }); // 返回貼文詳情頁面
        }
    }
}