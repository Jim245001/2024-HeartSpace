using HeartSpace.Models.Services;
using HeartSpace.Models;
using System.Collections.Generic;
using System.Web.Mvc;



namespace HeartSpace.Controllers
{
    public class SearchController : Controller
    {

     

        // GET: Search
        public ActionResult SearchEvent()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SearchPosts(string keyword)
        {
            //if (string.IsNullOrWhiteSpace(keyword))
            //{
            //    ViewBag.Keyword = keyword;
            //    return View(new List<PostCard>());
            //}

            //// 呼叫 Service 搜尋貼文
            //var posts = _postService.SearchPosts(keyword);
            //ViewBag.Keyword = keyword;

            return View(); // 回傳結果至 View
        }



    }
}