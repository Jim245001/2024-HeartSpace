using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.ViewModels;
using HeartSpace.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HeartSpace.Controllers;

public class HomeController : BaseController
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
            .OrderByDescending(p => p.PublishTime)
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
                    .FirstOrDefault() ?? "未知主辦者",
                Img = e.EventImg
            });

        var totalEventCount = eventsQuery.Provider.Execute<int>(
            System.Linq.Expressions.Expression.Call(
                typeof(Queryable), "Count",
                new Type[] { eventsQuery.ElementType },
                eventsQuery.Expression
            )
        );
        var eventItems = eventsQuery.Skip((eventPage - 1) * pageSize).Take(pageSize).AsEnumerable();
        var paginatedEvents = PaginatedList<EventViewModel>.Create(eventItems, totalEventCount, eventPage, pageSize);

        // 建立 ViewModel
        var viewModel = new HomePageViewModel
        {
            Posts = paginatedPosts,
            Events = paginatedEvents
        };

        return View(viewModel);
    }

}
