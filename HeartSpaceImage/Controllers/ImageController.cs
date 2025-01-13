using HeartSpaceImage.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeartSpaceImage.Controllers
{
    public class ImageController : Controller
    {
        // GET: Index
        public ActionResult Index()
        {
            // 取得 Images 資料夾的路徑
            var imagePath = Server.MapPath("~/Images");

            // 搜尋所有圖片檔案
            var images = Directory.EnumerateFiles(imagePath)
                                  .Select(Path.GetFileName)
                                  .ToList();

            // 根據檔名開頭分類
            var memberImages = images.Where(img => img.StartsWith("Member_")).ToList();
            var eventImages = images.Where(img => img.StartsWith("Event_")).ToList();
            var postImages = images.Where(img => img.StartsWith("Post_")).ToList();

            // 傳遞到 View
            var model = new ImagesViewModel
            {
                MemberImages = memberImages,
                EventImages = eventImages,
                PostImages = postImages
            };

            return View(model);
        }
    }
}