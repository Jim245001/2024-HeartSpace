using System;
using System.Linq;
using System.Web.Mvc;
using HeartSpace.BLL;
using HeartSpace.Models.EFModels;
using HeartSpace.Models.ViewModels;

namespace HeartSpace.Controllers
{
	public class EventController : Controller
	{
		private readonly EventService _eventService;

		public EventController()
		{
			_eventService = new EventService();
		}

		[HttpGet]
		public ActionResult CreateEvent()
		{
			ViewBag.Categories = _eventService.GetCategories(); // 使用服務層取得分類
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateEvent(EventViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (model.ParticipantMax.HasValue && model.ParticipantMax < model.ParticipantMin)
				{
					ModelState.AddModelError(nameof(model.ParticipantMax), $"最大參加人數必須大於或等於最小參加人數（{model.ParticipantMin}）");
					return View(model);
				}

				byte[] imageData = null;
				if (model.UploadedImg != null && model.UploadedImg.ContentLength > 0)
				{
					using (var binaryReader = new System.IO.BinaryReader(model.UploadedImg.InputStream))
					{
						imageData = binaryReader.ReadBytes(model.UploadedImg.ContentLength);
					}
				}

				var newEvent = new Event
				{
					EventName = model.EventName,
					MemberId = GetCurrentMemberId(),
					EventImg = imageData,
					CategoryId = model.CategoryId,
					Description = model.Description,
					EventTime = model.EventTime,
					Location = model.Location,
					IsOnline = model.IsOnline,
					ParticipantMax = model.ParticipantMax ?? int.MaxValue,
					ParticipantMin = model.ParticipantMin,
					Limit = model.Limit,
					DeadLine = model.DeadLine,
					CommentCount = 0,
					ParticipantNow = 0
				};

				_eventService.AddEvent(newEvent);

				return RedirectToAction("EventDetail", new { id = newEvent.Id });
			}

			ViewBag.Categories = _eventService.GetCategories(); // 重新填充分類資料
			return View(model);
		}

		public ActionResult EventDetail(int id)
		{
			var model = _eventService.GetEventWithDetails(id);
			if (model == null)
			{
				return HttpNotFound();
			}

			// 獲取當前用戶的 ID
			int currentMemberId = GetCurrentMemberId(); // 假設有這個方法

			// 設置是否為活動發起人
			model.IsEventOwner = model.MemberId == currentMemberId;

			// 設置是否已報名此活動
			model.IsRegistered = _eventService.IsMemberRegistered(id, currentMemberId);


			// 加載評論
			model.Comments = _eventService.GetEventComments(id)
				.Select(c => new CommentViewModel
				{
					Id = c.Id,
					MemberId = c.MemberId,
					MemberName = c.Member?.Name ?? "未知用戶", // 預設名稱
					MemberNickName = c.Member?.NickName ?? "未知用戶", // 預設名稱
					MemberProfileImg = c.Member?.MemberImg,
					EventCommentContent = c.EventCommentContent,
					CommentTime = c.CommentTime
				})
				.ToList();
			// 加載當前參與人數
			model.ParticipantNow = _eventService.GetParticipantCount(id); 

			return View(model);
		}


		[HttpPost]
		public ActionResult ToggleRegistration(int eventId, string actionType)
		{
			int memberId = GetCurrentMemberId();

			if (actionType == "cancel")
			{
				_eventService.UnregisterMember(eventId, memberId);
				TempData["Message"] = "您已成功取消報名。";
			}
			else if (actionType == "register")
			{
				_eventService.RegisterMember(eventId, memberId);
				TempData["Message"] = "您已成功報名活動！";
			}

			return RedirectToAction("EventDetail", new { id = eventId });
		}

		private int GetCurrentMemberId()
		{
			return 3; // 測試用固定 MemberId
		}
	}
}
