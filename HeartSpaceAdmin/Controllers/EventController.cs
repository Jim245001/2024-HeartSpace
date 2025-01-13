using HeartSpaceAdmin.Models.EFModels;
using HeartSpaceAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartSpaceAdmin.Controllers
{
	[Route("Event")]
	public class EventController : Controller
	{
		private readonly AppDbContext _context;

		public EventController(AppDbContext context)
		{
			_context = context;
		}

		// 活動管理首頁
		[HttpGet]
		public IActionResult Index()
		{
			var events = _context.Events
				.Include(e => e.Organizer) // 預加載 Organizer
				.Select(e => new EventViewModel
				{
					Id = e.Id,
					EventName = e.EventName,
					OrganizerName = e.Organizer.Name, // 假設 Organizer 一定存在
					Description = e.Description,
					EventTime = e.EventTime,
					DeadLine = e.DeadLine,
					Disabled = e.Disabled
				}).ToList();


			return View(events);
		}

		// 編輯活動
		[HttpGet("Edit/{id}")]
		public IActionResult Edit(int id)
		{
			var eventEntity = _context.Events
				.Include(e => e.EventComments)
				.Include(e => e.EventMembers)
				.FirstOrDefault(e => e.Id == id);

			if (eventEntity == null)
			{
				return NotFound();
			}

			var viewModel = new EventViewModel
			{
				Id = eventEntity.Id,
				EventName = eventEntity.EventName,
				OrganizerName = _context.Members.FirstOrDefault(m => m.Id == eventEntity.MemberId)?.Name,
				Description = eventEntity.Description,
				EventTime = eventEntity.EventTime,
				Location = eventEntity.Location,
				IsOnline = eventEntity.IsOnline,
				ParticipantMax = eventEntity.ParticipantMax,
				ParticipantMin = eventEntity.ParticipantMin,
				DeadLine = eventEntity.DeadLine,
				Comments = eventEntity.EventComments.Select(c => new EventCommentViewModel
				{
					Id = c.Id,
					EventId = c.EventId,
					MemberId = c.MemberId,
					MemberName = _context.Members.FirstOrDefault(m => m.Id == c.MemberId)?.Name,
					CommentContent = c.EventCommentContent,
					CommentTime = c.CommentTime,
					Disabled = c.Disabled == "True"
				}).ToList(),
				Participants = eventEntity.EventMembers.Select(em => new EventMemberViewModel
				{
					Id = em.Id,
					EventId = em.EventId,
					MemberId = em.MemberId,
					MemberName = _context.Members.FirstOrDefault(m => m.Id == em.MemberId)?.Name,
					IsAttend = em.IsAttend
				}).ToList()
			};

			return View(viewModel);
		}

		// 編輯保存
		[HttpPost("Edit/{id}")]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(EventViewModel model)
		{
			if (ModelState.IsValid)
			{
				var eventEntity = _context.Events.FirstOrDefault(e => e.Id == model.Id);
				if (eventEntity == null)
				{
					return NotFound();
				}

				eventEntity.EventName = model.EventName;
				eventEntity.Description = model.Description;
				eventEntity.EventTime = model.EventTime;
				eventEntity.Location = model.Location;
				eventEntity.IsOnline = model.IsOnline;
				eventEntity.ParticipantMax = model.ParticipantMax;
				eventEntity.ParticipantNow = model.ParticipantNow;
				eventEntity.DeadLine = model.DeadLine;

				_context.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(model);
		}


		//刪除
		[HttpPost]		
		[Route("Event/Delete/{id}")]
		public IActionResult Delete(int id)
		{
			var eventEntity = _context.Events.FirstOrDefault(e => e.Id == id);
			if (eventEntity == null)
			{
				return NotFound();
			}

			eventEntity.Disabled = true;
			_context.SaveChanges();

			return RedirectToAction("Index");
		}


		//還原刪除
		[HttpPost]
		[Route("Event/Restore/{id}")]
		public IActionResult Restore(int id)
		{
			var eventEntity = _context.Events.FirstOrDefault(e => e.Id == id);
			if (eventEntity == null)
			{
				return NotFound();
			}

			eventEntity.Disabled = false;
			_context.SaveChanges();

			return RedirectToAction("Index");
		}



	}

}
