using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.ViewModels;
using HeartSpace.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;

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

        // 分頁處理貼文資料
        var postsQuery = _context.Posts
            .Include(p => p.Member)
               .OrderBy(e => Guid.NewGuid())
            .Select(p => new PostCard
            {
                Id = p.Id,
                Title = p.Title,
                PostContent = p.PostContent,
                PublishTime = p.PublishTime,
                MemberNickName = p.Member != null ? p.Member.NickName : "未知作者",
                PostImg = p.PostImg,
                CategoryName = _context.Categories
                    .Where(c => c.Id == p.CategoryId)
                    .Select(c => c.CategoryName)
                    .FirstOrDefault() ?? "未分類"
            });

        var totalPostCount = postsQuery.Provider.Execute<int>(
            System.Linq.Expressions.Expression.Call(
                typeof(Queryable), "Count",
                new Type[] { postsQuery.ElementType },
                postsQuery.Expression
            )
        );
        var postItems = postsQuery.Skip((postPage - 1) * pageSize).Take(pageSize).AsEnumerable();
        var paginatedPosts = PaginatedList<PostCard>.Create(postItems, totalPostCount, postPage, pageSize);

        // 揪團活動資料分頁
        var eventsQuery = _context.Events
               .OrderBy(e => Guid.NewGuid())
            .Select(e => new EventViewModel
            {
                Id = e.Id,
				Title = e.EventName,
				EventContent = e.Description,
                EventTime = e.EventTime,
				MemberNickName = e.Member != null ? e.Member.NickName : "未知發起人",
				EventImg = e.EventImg,
				CategoryName = _context.Categories
					.Where(c => c.Id == c.Id)
					.Select(c => c.CategoryName)
					.FirstOrDefault() ?? "未分類"
			});

        var totalEventCount = eventsQuery.Provider.Execute<int>(
            System.Linq.Expressions.Expression.Call(
                typeof(Queryable), "Count",
                new Type[] { eventsQuery.ElementType },
                eventsQuery.Expression
            )
        );
        var eventItems = eventsQuery.Skip((eventPage - 1) * pageSize).Take(pageSize).AsEnumerable();
        var paginatedEvents = PaginatedList<EventCard>.Create(eventItems, totalEventCount, eventPage, pageSize);

        // 建立 ViewModel
        var viewModel = new HomePageViewModel
        {
            Posts = paginatedPosts,
            Events = paginatedEvents
        };

		

		return View(viewModel);


    }

}
