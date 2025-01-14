using HeartSpaceAdmin.Models.EFModels;
using HeartSpaceAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartSpaceAdmin.Controllers
{
	public class EventController : Controller
	{
		private readonly AppDbContext _context;

		public EventController(AppDbContext context)
		{
			_context = context;
		}

		// Index Page
		public IActionResult Index()
		{
			var events = _context.Events
				.Include(e => e.Organizer)
				.Select(e => new EventViewModel
				{
					Id = e.Id,
					EventName = e.EventName,
					OrganizerName = e.Organizer.Name,
					EventTime = e.EventTime,
					DeadLine = e.DeadLine,
					Disabled = e.Disabled
				}).ToList();

			return View(events);
		}

		// View Details Page
		public IActionResult Details(int id)
		{
			var eventEntity = _context.Events
					.Include(e => e.EventComments) // 明確載入 EventComments
					.ThenInclude(c => c.Member)   // 載入 Comment 的 Member
					.Include(e => e.EventMembers) // 明確載入 EventMembers
					.ThenInclude(em => em.Member) // 載入 Member
					.FirstOrDefault(e => e.Id == id);

			if (eventEntity == null) return NotFound();

			var viewModel = new EventViewModel
			{
				Id = eventEntity.Id,
				EventName = eventEntity.EventName,
				OrganizerName = eventEntity.Organizer?.Name ?? "未知成員", // 確保 Organizer 為 null 時提供預設值
				Description = eventEntity.Description,
				EventTime = eventEntity.EventTime,
				DeadLine = eventEntity.DeadLine,
				IsOnline = eventEntity.IsOnline,
				Location = eventEntity.Location,
				EventImg = eventEntity.EventImg,
				ParticipantMax = eventEntity.ParticipantMax,
				ParticipantMin = eventEntity.ParticipantMin,
				Comments = eventEntity.EventComments?.Select(c => new EventCommentViewModel
				{
					Id = c.Id,
					MemberName = c.Member?.Name ?? "未知成員", // 確保 Member 為 null 時提供預設值
					CommentContent = c.EventCommentContent,
					CommentTime = c.CommentTime,
					Disabled = c.Disabled // null 視為未禁用
				}).ToList() ?? new List<EventCommentViewModel>(), // 確保 Comments 不為 null
				Participants = eventEntity.EventMembers?.Select(em => new EventMemberViewModel
				{
					Id = em.Id,
					MemberName = em.Member?.Name ?? "未知成員", // 確保 Member 為 null 時提供預設值
					IsAttend = em.IsAttend
				}).ToList() ?? new List<EventMemberViewModel>() // 確保 Participants 不為 null
			};

			return View(viewModel);

		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleDelete(int id)
		{
			// 嘗試從資料庫中取得指定 ID 的活動
			var eventEntity = _context.Events.FirstOrDefault(e => e.Id == id);
			if (eventEntity == null)
			{
				// 若無此活動，回傳 404 NotFound
				return NotFound();
			}

			// 切換 Disabled 狀態
			eventEntity.Disabled = !eventEntity.Disabled;

			// 儲存變更到資料庫
			_context.SaveChanges();

			// 返回 Index 頁面
			return RedirectToAction("Index");
		}



		//刪除或恢復留言
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleCommentDisabled(int id)
		{
			try
			{
				// 查找評論
				var comment = _context.EventComments.FirstOrDefault(c => c.Id == id);
				if (comment == null)
				{
					return NotFound(); // 如果找不到評論，返回 404
				}

				// 切換 Disabled 狀態
				comment.Disabled = string.IsNullOrWhiteSpace(comment.Disabled) ? "true" : null;

				// 保存變更
				_context.SaveChanges();

				// 重定向至詳細頁
				return RedirectToAction("Details", new { id = comment.EventId });
			}
			catch (Exception ex)
			{
				// 處理例外並返回錯誤
				return StatusCode(500, $"無法切換評論狀態：{ex.Message}");
			}
		}



	}

}
