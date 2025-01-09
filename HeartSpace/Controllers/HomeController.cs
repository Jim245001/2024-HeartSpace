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
                PostImg = p.PostImg,
                CategoryName = categoryDict.ContainsKey(p.CategoryId) ? categoryDict[p.CategoryId] : "未分類" // 從字典中查找分類名稱
            });

        var paginatedPosts = PaginatedList<PostCard>.Create(postsQuery, postPage, pageSize); // 使用 PostCard 作為類型

        // 揪團活動資料分頁
        var eventsQuery = _context.Events
    .OrderByDescending(e => e.EventTime)
    .Select(e => new EventViewModel
    {
        Id = e.Id,
        EventName = e.EventName,
        Description = e.Description,
        Location = e.Location,
        EventTime = e.EventTime,
        MemberName = _context.Members
            .Where(m => m.Id == e.MemberId)
            .Select(m => m.Name)
            .FirstOrDefault() ?? "未知主辦者", // 根據 MemberId 抓取 Name
        Img = e.EventImg,
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
