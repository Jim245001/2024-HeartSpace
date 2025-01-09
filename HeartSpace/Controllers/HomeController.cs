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

	public ActionResult Index(int? categoryId = null)
	{
		// 取得分類資料
		var categories = _context.Categories
			.OrderBy(c => c.DisplayOrder)
			.ToList();

		// 建立 ViewModel
		var viewModel = new HomePageViewModel
		{
			Categories = categories,
			SelectedCategoryId = categoryId
		};

		return View(viewModel);
	}

	public ActionResult GetPosts(int postPage = 1, int? categoryId = null)
	{
		int pageSize = 6; // 每頁顯示 6 筆資料

		// 分頁處理貼文資料，並根據 CategoryId 進行篩選
		var postsQuery = _context.Posts
			.Include(p => p.Member) // 載入會員資料
			.Where(p => !categoryId.HasValue || p.CategoryId == categoryId) // 如果 categoryId 有值，進行篩選
			.Select(p => new PostViewModel
			{
				Id = p.Id,
				Title = p.Title,
				PostContent = p.PostContent,
				PublishTime = p.PublishTime,
				MemberNickName = p.Member != null ? p.Member.Name : "未知作者",
				CategoryId = p.CategoryId // 加入 CategoryId 用於前端顯示
			});

		var paginatedPosts = PaginatedList<PostViewModel>.Create(
			postsQuery,
			postPage,
			pageSize,
			query => query.OrderByDescending(p => p.PublishTime) // 傳入排序邏輯
		);

		return PartialView("_PostsPartial", paginatedPosts);
	}

	public ActionResult GetEvents(int eventPage = 1, int? categoryId = null)
	{
		int pageSize = 6; // 每頁顯示 6 筆資料

		// 根據 categoryId 進行篩選
		var eventsQuery = _context.Events
			.Include(e => e.Member)
			.Where(e => !categoryId.HasValue || e.CategoryId == categoryId) // 如果 categoryId 有值，進行篩選
			.Select(e => new EventViewModel
			{
				Id = e.Id,
				EventName = e.EventName,
				Description = e.Description,
				Location = e.Location,
				EventTime = e.EventTime,
				MemberName = e.Member != null ? e.Member.Name : "未知主辦者",
				Img = e.EventImg
			});

		var paginatedEvents = PaginatedList<EventViewModel>.Create(
			eventsQuery,
			eventPage,
			pageSize,
			query => query.OrderByDescending(e => e.EventTime) // 傳入排序邏輯
		);

		return PartialView("_EventsPartial", paginatedEvents);
	}

}
