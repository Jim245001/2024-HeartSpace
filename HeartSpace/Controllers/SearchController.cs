using HeartSpace.Models.Services;
using HeartSpace.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using HeartSpace.DTOs.Services.Interfaces;
using System.Linq;
using HeartSpace.Models.DTOs;
using System;
using HeartSpace.Models.ViewModels;
using System.Data.Entity;
using System.Diagnostics;
using HeartSpace.Models.EFModels;
using System.Drawing.Printing;
using HeartSpace.Helpers;




namespace HeartSpace.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPostService _postService;

        public SearchController(IPostService postService)
        {
            _postService = postService ?? throw new ArgumentNullException(nameof(postService));
          
        }
        


        public ActionResult SearchPost(string keyword, int pageIndex = 1, int pageSize = 10)
        {

            // 從服務層獲取 CreatePostDto
            var postDtos = _postService.FindPostsByKeyword(keyword, pageIndex, pageSize);

            // 將 CreatePostDto 轉換為 PostCard
            var posts = postDtos.Select(dto => new PostCard
            {
                Id = dto.Id,
                Title = dto.Title,
                PostContent = dto.PostContent,
                PublishTime = dto.PublishTime,
                PostImg = dto.PostImg,
                MemberNickName = dto.MemberNickName,
                MemberImg = dto.MemberImg,
                CategoryName = dto.CategoryName
            }).ToList();

            // 獲取推薦貼文
            var recommendedPosts = _postService.GetRandomPosts(6);

            // 建立 ViewModel
            var viewModel = new SearchPostViewModel
            {
                Posts = PaginatedList<PostCard>.Create(posts, pageIndex, pageSize),
                RecommendedPosts = recommendedPosts.ToList()
            };

            // 傳遞搜尋關鍵字給前端
            ViewBag.Keyword = keyword;

            return View(viewModel);
        }

    }
}