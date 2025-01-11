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
		private readonly IEventService _eventService;

		// 使用依賴注入
		public SearchController(IPostService postService, IEventService eventService)
		{
			_postService = postService ?? throw new ArgumentNullException(nameof(postService));
			_eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
		}

		public ActionResult SearchEvent(string keyword)
		{
			var events = _eventService.SearchEvents(keyword, 1, 10); // 搜尋符合條件的活動
																	
			var recommendedEvents = _eventService.GetRandomEvents(6);  // 隨機取得其他活動作為推薦
			var model = new SearchEventViewModel
			{
				Events = events,
				RecommendedEvents = recommendedEvents.ToList()
			};
			ViewBag.Keyword = keyword;
			return View(model);
		}

		public ActionResult SearchPost(string keyword, int pageIndex = 1, int pageSize = 10)
        {

            // 從服務層獲取搜尋結果與推薦貼文
            var posts = _postService.FindPostsByKeyword(keyword, pageIndex, pageSize)
                .Select(p => new PostCard
                {
                    Id = p.Id,
                    Title = p.Title,
                    PostContent = p.PostContent,
                    PublishTime = p.PublishTime,
                    PostImg = p.PostImg,
                    MemberNickName = p.MemberNickName,
                    MemberImg = p.MemberImg,
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
                    MemberImg = p.MemberImg,
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