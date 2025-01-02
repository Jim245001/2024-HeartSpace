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
    }
}