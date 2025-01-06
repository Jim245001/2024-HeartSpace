using System;
using System.Linq;
using System.Web.Mvc;
using HeartSpace.Models;
using HeartSpace.Models.EFModel;
using HeartSpace.Models.ViewModels;
using HeartSpace.BLL;

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

			// 加載評論，並判斷是否為留言者本人
			model.Comments = _eventService.GetEventComments(id)
				.Select(c => new CommentViewModel
				{
					Id = c.Id,
					MemberId = c.MemberId,
					MemberName = c.Member?.Name ?? "未知用戶", // 預設名稱
					MemberNickName = c.Member?.NickName ?? "未知用戶", // 預設名稱
					MemberProfileImg = c.Member?.MemberImg,
					EventCommentContent = c.EventCommentContent,
					CommentTime = c.CommentTime,
					IsCommentOwner = _eventService.IsCommentOwner(c.Id, currentMemberId)  
				})
				.ToList();

			// 加載當前參與人數
			model.ParticipantNow = _eventService.GetParticipantCount(id);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteComment(int commentId, int eventId)
		{
			try
			{
				// 根據活動 ID 獲取留言
				var comment = _eventService.GetEventComments(eventId)
					.FirstOrDefault(c => c.Id == commentId);

				if (comment == null)
				{
					return HttpNotFound("找不到該留言。");
				}

				// 確認當前用戶是否有權刪除該留言
				int currentMemberId = GetCurrentMemberId(); // 假設有此方法
				if (!_eventService.IsCommentOwner(commentId, currentMemberId))
				{
					return new HttpStatusCodeResult(403, "無權刪除此留言。");
				}

				// 執行刪除操作
				_eventService.RemoveComment(comment);

				// 刪除成功後返回活動詳情頁
				return RedirectToAction("EventDetail", new { id = eventId });
			}
			catch (Exception ex)
			{
				// 返回錯誤訊息
				return new HttpStatusCodeResult(500, $"刪除留言時發生錯誤：{ex.Message}");
			}
		}

		public ActionResult EditComment(int commentId)
		{
			// 查詢留言
			var comment = _eventService.GetEventComments(commentId) // 請傳入正確的活動 ID
				.FirstOrDefault(c => c.Id == commentId);

			if (comment == null)
			{
				return HttpNotFound("找不到該留言。");
			}

			// 確認當前用戶是否為留言所有者
			int currentMemberId = GetCurrentMemberId(); // 假設有此方法
			if (!_eventService.IsCommentOwner(commentId, currentMemberId))
			{
				return new HttpStatusCodeResult(403, "無權編輯此留言。");
			}

			// 返回編輯視圖，傳遞現有資料
			return View(comment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditComment(int commentId, string updatedContent, int eventId)
		{
			try
			{
				// 使用傳入的活動 ID 獲取留言
				var comment = _eventService.GetEventComments(eventId)
							.FirstOrDefault(c => c.Id == commentId);

				if (comment == null)
				{
					return HttpNotFound("找不到該留言。");
				}

				// 確認當前用戶是否為留言所有者
				int currentMemberId = GetCurrentMemberId();
				if (!_eventService.IsCommentOwner(commentId, currentMemberId))
				{
					return new HttpStatusCodeResult(403, "無權編輯此留言。");
				}

				// 更新留言內容
				comment.EventCommentContent = updatedContent;
				_eventService.UpdateComment(comment); // 假設有 UpdateComment 方法

				// 返回活動詳情頁
				return RedirectToAction("EventDetail", new { id = comment.EventId });
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"編輯時發生錯誤：{ex.Message}");
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult AddComment(int eventId, string commentContent)
		{
			try
			{
				// 驗證留言內容是否有效
				if (string.IsNullOrWhiteSpace(commentContent))
				{
					return new HttpStatusCodeResult(400, "留言內容不得為空。");
				}

				// 獲取當前用戶 ID
				int currentMemberId = GetCurrentMemberId(); // 假設有這個方法

				// 建立新的留言物件
				var newComment = new EventComment
				{
					EventId = eventId,
					MemberId = currentMemberId,
					EventCommentContent = commentContent,
					CommentTime = DateTime.Now
				};

				// 呼叫服務層新增留言
				_eventService.AddComment(newComment);

				// 新增成功後重導回活動詳情頁
				return RedirectToAction("EventDetail", new { id = eventId });
			}
			catch (Exception ex)
			{
				// 返回錯誤訊息
				return new HttpStatusCodeResult(500, $"新增留言時發生錯誤：{ex.Message}");
			}
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
