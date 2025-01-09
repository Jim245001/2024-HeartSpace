using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
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

		//檢視揪團
		public ActionResult EventDetail(int id)
		{
			var model = _eventService.GetEventDetailsWithExtras(id, GetCurrentMemberId());

			if (model == null)
			{
				return HttpNotFound();
			}

			return View(model);
		}

		//建立揪團
		[HttpGet]
		public ActionResult CreateEvent()
		{
			ViewBag.Categories = _eventService.GetCategories(); 
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateEvent(EventViewModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Categories = _eventService.GetCategories();
				return View(model);
			}
			// 驗證人數
			if ( model.ParticipantMax < model.ParticipantMin)
			{
				ModelState.AddModelError(nameof(model.ParticipantMax), $"最大參加人數必須大於或等於最小參加人數（{model.ParticipantMin}）");
				ViewBag.Categories = _eventService.GetCategories(); // 重新填充分類資料
				return View(model);
			}

			// 去除空白後再驗證
			if (string.IsNullOrWhiteSpace(model.Description))
			{
				ModelState.AddModelError("Description", "描述必填且不能只包含空白字元");
				ViewBag.Categories = _eventService.GetCategories();
				return View(model);
			}

			// 圖片處理
			byte[] imageData = null;
			//不為 null 且檔案大小大於 0 時，進行圖片處理
			if (model.UploadedEventImg != null && model.UploadedEventImg.ContentLength > 0)
			{
				var allowedFileTypes = new[] { "image/jpeg", "image/png", "image/gif" };
				if (!allowedFileTypes.Contains(model.UploadedEventImg.ContentType))
				{
					ModelState.AddModelError("UploadedImg", "只允許上傳 JPEG、PNG 或 GIF 格式的圖片。");
					ViewBag.Categories = _eventService.GetCategories();
					return View(model);
				}

				if (model.UploadedEventImg.ContentLength > 5 * 1024 * 1024) // 限制大小為 5MB
				{
					ModelState.AddModelError("UploadedEventImg", "圖片大小不能超過 5MB。");
					ViewBag.Categories = _eventService.GetCategories();
					return View(model);
				}

				try //轉換成 byte[]
				{
					using (var binaryReader = new System.IO.BinaryReader(model.UploadedEventImg.InputStream))
					{
						imageData = binaryReader.ReadBytes(model.UploadedEventImg.ContentLength);
					}
				}
				catch (Exception)
				{
					ModelState.AddModelError("UploadedEventImg", "圖片處理失敗，請重試或選擇其他圖片。");
					ViewBag.Categories = _eventService.GetCategories();
					return View(model);
				}
			}


			// 建立 Event 物件
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
				ParticipantMax = model.ParticipantMax, // 若為 null，設定為 int.MaxValue
				ParticipantMin = model.ParticipantMin,
				Limit = model.Limit,
				DeadLine = model.DeadLine,
				CommentCount = 0, // 預設評論數量為 0
				ParticipantNow = 0, // 預設參與人數為 0
				Disabled = false,
			};

			try
			{
				// 儲存活動
				//_eventService.AddEvent(newEvent);
				var newEventId = _eventService.AddEvent(newEvent);
				newEvent.Id = newEventId; // 將返回的 Id 賦值給 newEvent.Id
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "儲存活動時發生錯誤，請稍後再試。");
				ViewBag.Categories = _eventService.GetCategories(); // 重新填充分類資料
				return View(model);
			}

			// 導向到活動詳細頁
			return RedirectToAction("EventDetail", new { id = newEvent.Id });
		}

		//關閉揪團
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CloseEvent(int id)
		{
			try
			{
				_eventService.DeleteEvent(id);

				// 重定向到活動列表頁或其他頁面
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"關閉活動時發生錯誤：{ex.Message}");
			}
		}

		//修改揪團
		[HttpGet]
		public ActionResult EditEvent(int id)
		{
			// 從 Service 層獲取活動資料
			var eventEntity = _eventService.GetEventById(id);
			if (eventEntity == null)
			{
				return HttpNotFound("找不到該活動。");
			}
			// 獲取所有分類
			var categories = _eventService.GetCategories();
			ViewBag.Categories = categories.Select(c => new SelectListItem
			{
				Value = c.Id.ToString(),
				Text = c.CategoryName
			});
			

			// 將 Event 轉換為 EventViewModel
			var eventViewModel = new EventViewModel
			{
				Id = eventEntity.Id,
				EventName = eventEntity.EventName,
				MemberId = eventEntity.MemberId,
				EventImg = eventEntity.EventImg,
				CategoryId = eventEntity.CategoryId,
				Description = eventEntity.Description,
				EventTime = eventEntity.EventTime,
				Location = eventEntity.Location,
				IsOnline = eventEntity.IsOnline,
				ParticipantMax = eventEntity.ParticipantMax,
				ParticipantMin = eventEntity.ParticipantMin,
				Limit = eventEntity.Limit,
				DeadLine = eventEntity.DeadLine,
				CommentCount = eventEntity.CommentCount,
				ParticipantNow = eventEntity.ParticipantNow,
				
			};

			// 返回視圖，並將 ViewModel 傳遞給視圖
			return View(eventViewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditEvent(EventViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model); // 如果資料驗證失敗，返回編輯頁
			}

			try
			{
				// 將 EventViewModel 轉換為 Event
				var updatedEvent = new Event
				{
					Id = model.Id,
					EventName = model.EventName,
					MemberId = model.MemberId,
					EventImg = model.EventImg,
					CategoryId = model.CategoryId,
					Description = model.Description,
					EventTime = model.EventTime,
					Location = model.Location,
					IsOnline = model.IsOnline,
					ParticipantMax = model.ParticipantMax,
					ParticipantMin = model.ParticipantMin,
					Limit = model.Limit,
					DeadLine = model.DeadLine,
					CommentCount = model.CommentCount,
					ParticipantNow = model.ParticipantNow
				};

				// 更新活動
				_eventService.UpdateEvent(updatedEvent);

				// 更新成功後跳轉到活動詳情頁
				return RedirectToAction("EventDetail", new { id = model.Id });
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"修改活動時發生錯誤：{ex.Message}");
			}
		}

		//報名狀況
		[HttpGet]
		public ActionResult EventStatus(int id)
		{
			// 獲取活動詳細資料
			var model = _eventService.GetEventStatus(id);

			if (model == null)
			{
				return HttpNotFound(); 
			}

			return View(model);
		}

		//出席狀況
		[HttpPost]
		public JsonResult UpdateAttendanceBatch(List<AttendanceUpdateViewModel> updates)
		{
			try
			{
				// 調用 Service 更新出席狀態
				foreach (var update in updates)
				{
					_eventService.UpdateAttendance(update.MemberId, update.EventId, update.IsAttend);
				}

				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}



		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteComment(int commentId, int eventId)
		{
			try
			{
				var comment = _eventService.GetEventComments(eventId)
					.FirstOrDefault(c => c.Id == commentId);

				if (comment == null)
				{
					return HttpNotFound("找不到該留言。");
				}

				int currentMemberId = GetCurrentMemberId(); // 假設有此方法
				if (!_eventService.IsCommentOwner(commentId, currentMemberId))
				{
					return new HttpStatusCodeResult(403, "無權刪除此留言。");
				}

				_eventService.RemoveComment(comment);

				return RedirectToAction("EventDetail", new { id = eventId });
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"刪除留言時發生錯誤：{ex.Message}");
			}
		}

		public ActionResult EditComment(int commentId)
		{
			var comment = _eventService.GetEventComments(commentId)
				.FirstOrDefault(c => c.Id == commentId);

			if (comment == null)
			{
				return HttpNotFound("找不到該留言。");
			}

			int currentMemberId = GetCurrentMemberId(); // 假設有此方法
			if (!_eventService.IsCommentOwner(commentId, currentMemberId))
			{
				return new HttpStatusCodeResult(403, "無權編輯此留言。");
			}

			return View(comment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditComment(int commentId, string updatedContent, int eventId)
		{
			try
			{
				var comment = _eventService.GetEventComments(eventId)
					.FirstOrDefault(c => c.Id == commentId);

				if (comment == null)
				{
					return HttpNotFound("找不到該留言。");
				}

				int currentMemberId = GetCurrentMemberId();
				if (!_eventService.IsCommentOwner(commentId, currentMemberId))
				{
					return new HttpStatusCodeResult(403, "無權編輯此留言。");
				}

				comment.EventCommentContent = updatedContent;
				_eventService.UpdateComment(comment);

				return RedirectToAction("EventDetail", new { id = eventId });
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
				if (string.IsNullOrWhiteSpace(commentContent))
				{
					return new HttpStatusCodeResult(400, "留言內容不得為空。");
				}

				int currentMemberId = GetCurrentMemberId(); // 假設有此方法

				var newComment = new EventComment
				{
					EventId = eventId,
					MemberId = currentMemberId,
					EventCommentContent = commentContent,
					CommentTime = DateTime.Now
				};

				_eventService.AddComment(newComment);

				return RedirectToAction("EventDetail", new { id = eventId });
			}
			catch (Exception ex)
			{
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
