using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.ViewModels;
using HeartSpace.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class HomeController : Controller
{
	private readonly AppDbContext _context;

	public HomeController() : this(new AppDbContext()) { }

	public HomeController(AppDbContext context)
	{
		_context = context;
	}

	public ActionResult Index(int postPage = 1, int eventPage = 1)
	{
		int pageSize = 6; // 每頁顯示 6 筆資料
        var categoryDict = _context.Categories
    .ToDictionary(c => c.Id, c => c.CategoryName);
        // 分頁處理貼文資料
        var postsQuery = _context.Posts
            .Include(p => p.Member)

            .OrderByDescending(p => p.PublishTime)
            .Select(p => new PostCard // 將 PostViewModel 直接轉換成 PostCard
            {
                Id = p.Id,
                Title = p.Title,
                PostContent = p.PostContent,
                PublishTime = p.PublishTime,
                MemberNickName = p.Member != null ? p.Member.NickName : "未知作者",
                PostImg = p.PostImg != null ? Convert.ToBase64String(p.PostImg) : null, // 如果有圖片，轉換為 Base64
                CategoryName = categoryDict.ContainsKey(p.CategoryId) ? categoryDict[p.CategoryId] : "未分類" // 從字典中查找分類名稱
            });

        var paginatedPosts = PaginatedList<PostCard>.Create(postsQuery, postPage, pageSize); // 使用 PostCard 作為類型

        // 揪團活動資料分頁
        var eventsQuery = _context.Events
			.Include(e => e.Member)
			.OrderByDescending(e => e.EventTime)
			.Select(e => new EventViewModel
			{
				Id = e.Id, // 確保選取了 Id
				EventName = e.EventName,
				Description = e.Description,
				Location = e.Location,
				EventTime = e.EventTime,
				MemberName = e.Member != null ? e.Member.Name : "未知主辦者",
				Img = e.EventImg
			});

		var paginatedEvents = PaginatedList<EventViewModel>.Create(eventsQuery, eventPage, pageSize);

		// 建立 ViewModel
		var viewModel = new HomePageViewModel
		{
            Posts = paginatedPosts,

            Events = paginatedEvents
		};

		return View(viewModel);
	}
}
