using HeartSpace.Models.Services;
using HeartSpace.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using HeartSpace.DTOs.Services.Interfaces;
using System.Linq;
using HeartSpace.Models.DTOs;
using System;
using HeartSpace.Models.ViewModels;




namespace HeartSpace.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPostService _postService;

        public SearchController(IPostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

    

        public ActionResult SearchPost(string keyword)
        {
            var viewModel = new SearchPostViewModel
            {
                CreatePostDtos = _postService.FindPostsByKeyword(keyword), // 搜尋結果
                PostCards = _postService.GetRandomPosts(5) // 隨機顯示的貼文卡片
            };

            ViewBag.Keyword = keyword; // 傳遞搜尋關鍵字
            return View(viewModel);
        }

    }
}