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
            // 從服務層獲取搜尋結果與推薦貼文
            var posts = _postService.FindPostsByKeyword(keyword)
                .Select(p => new PostCard
                {
                    Id = p.Id,
                    Title = p.Title,
                    PostContent = p.PostContent,
                    PublishTime = p.PublishTime,
                    PostImg = p.PostImg,
                    MemberNickName = p.MemberNickName,
                    MemberImg = p.MemberImgBase64,
                    CategoryName = p.CategoryName
                })
                .ToList();

            var recommendedPosts = _postService.GetRandomPosts(6)
                .Select(p => new PostCard
                {
                    Id = p.Id,
                    Title = p.Title,
                    PostContent = p.PostContent,
                    PublishTime = p.PublishTime,
                    PostImg = p.PostImg,
                    MemberNickName = p.MemberNickName,
                    MemberImg = p.MemberImgBase64,
                    CategoryName = p.CategoryName
                })
                .ToList();

            // 建立 ViewModel
            var viewModel = new SearchPostViewModel
            {
                Posts = posts,
                RecommendedPosts = recommendedPosts
            };

            // 傳遞搜尋關鍵字給前端
            ViewBag.Keyword = keyword;

            return View(viewModel);
        }

    }
}