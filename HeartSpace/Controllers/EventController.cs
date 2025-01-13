﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.Security;
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

			// 打印每條評論的 Disabled 值到 Console
			foreach (var comment in model.Comments)
			{
				Console.WriteLine($"Comment ID: {comment.Id}, Disabled: {comment.Disabled}");
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
			if (model.ParticipantMax < model.ParticipantMin)
			{
				ModelState.AddModelError(nameof(model.ParticipantMax), $"最大參加人數必須大於或等於最小參加人數（{model.ParticipantMin}）");
				ViewBag.Categories = _eventService.GetCategories();
				return View(model);
			}

			// 去除空白後再驗證
			if (string.IsNullOrWhiteSpace(model.Description))
			{
				ModelState.AddModelError("Description", "描述必填且不能只包含空白字元");
				ViewBag.Categories = _eventService.GetCategories();
				return View(model);
			}

			// 驗證照片
			if (model.UploadedEventImg != null)
			{
				// 驗證檔案副檔名
				var fileExtension = Path.GetExtension(model.UploadedEventImg.FileName).ToLower();
				var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
				if (!permittedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("UploadedEventImg", "只允許上傳圖片格式的檔案（.jpg, .jpeg, .png, .gif）。");
					return View(model);
				}

				// 驗證檔案大小
				const int maxFileSize = 5 * 1024 * 1024; // 5MB
				if (model.UploadedEventImg.ContentLength > maxFileSize)
				{
					ModelState.AddModelError("UploadedEventImg", "檔案大小不能超過 5MB。");
					return View(model);
				}
			}

			// 建立 Event 物件
			var newEvent = new Event
			{
				EventName = model.EventName,
				MemberId = GetCurrentMemberId(),
				CategoryId = model.CategoryId,
				Description = model.Description,
				EventTime = model.EventTime,
				Location = model.Location,
				IsOnline = model.IsOnline,
				ParticipantMax = model.ParticipantMax,
				ParticipantMin = model.ParticipantMin,
				Limit = model.Limit,
				DeadLine = model.DeadLine,
				CommentCount = 0,
				ParticipantNow = 0,
				Disabled = false,
			};

			try
			{
				// 儲存活動
				var newEventId = _eventService.AddEvent(newEvent);
				newEvent.Id = newEventId;
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "儲存活動時發生錯誤，請稍後再試。");
				ViewBag.Categories = _eventService.GetCategories();
				return View(model);
			}

			// 圖片處理
			if (model.UploadedEventImg != null)
			{
				var uploadsFolder = Path.Combine(Server.MapPath("~/Images"));
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder);
				}

				// 基於新生成的 EventId 命名圖片
				var sanitizedEventName = string.Concat(model.EventName.Split(Path.GetInvalidFileNameChars()));
				var fileExtension = Path.GetExtension(model.UploadedEventImg.FileName).ToLower();
				var uniqueFileName = $"Event{newEvent.Id}_{sanitizedEventName}{fileExtension}";
				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				try
				{
					// 儲存圖片
					model.UploadedEventImg.SaveAs(filePath);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "儲存圖片時發生錯誤，請稍後再試。");
					ViewBag.Categories = _eventService.GetCategories();
					return View(model);
				}

				// 更新圖片路徑到資料庫
				newEvent.EventImg = Path.Combine("Images", uniqueFileName).Replace("\\", "/");
				_eventService.UpdateEvent(newEvent);
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
			// 獲取分類清單
			var categories = _eventService.GetCategories();
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
				Categories = categories.Select(c => new SelectListItem
				{
					Value = c.Id.ToString(),
					Text = c.CategoryName
				}).ToList()

			};

			// 返回視圖，並將 ViewModel 傳遞給視圖
			return View(eventViewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditEvent(EventViewModel model, bool removePhoto = false)
		{
			if (!ModelState.IsValid)
			{
				try
				{
					model.Categories = _eventService.GetCategories()
						.Select(c => new SelectListItem
						{
							Value = c.Id.ToString(),
							Text = c.CategoryName
						}).ToList();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error initializing categories: {ex.Message}");
				}
				return View(model);
			}

			try
			{
				// 從資料庫獲取現有的活動資料
				var existingEvent = _eventService.GetEventById(model.Id);
				if (existingEvent == null)
				{
					return HttpNotFound("活動不存在");
				}

				// 更新活動的基本資料（不包括圖片）
				existingEvent.EventName = model.EventName;
				existingEvent.MemberId = model.MemberId;
				existingEvent.CategoryId = model.CategoryId;
				existingEvent.Description = model.Description;
				existingEvent.EventTime = model.EventTime;
				existingEvent.Location = model.Location;
				existingEvent.IsOnline = model.IsOnline;
				existingEvent.ParticipantMax = model.ParticipantMax;
				existingEvent.ParticipantMin = model.ParticipantMin;
				existingEvent.Limit = model.Limit;
				existingEvent.DeadLine = model.DeadLine;
				existingEvent.CommentCount = model.CommentCount;
				existingEvent.ParticipantNow = model.ParticipantNow;

				// 更新活動資料
				_eventService.UpdateEvent(existingEvent);

				// 照片更新邏輯
				string newImagePath = existingEvent.EventImg; // 預設保留現有圖片路徑

				// 移除照片邏輯
				if (removePhoto)
				{
					if (!string.IsNullOrEmpty(existingEvent.EventImg))
					{
						var oldFilePath = Path.Combine(Server.MapPath("~"), existingEvent.EventImg.Replace("/", "\\"));
						if (System.IO.File.Exists(oldFilePath))
						{
							System.IO.File.Delete(oldFilePath); // 刪除舊照片
						}
					}

					newImagePath = null; // 設為空，表示移除照片
					existingEvent.EventImg = null; // 同步更新模型的 EventImg
					//Console.WriteLine($"EventImg after delete logic: {existingEvent.EventImg}"); // 調試輸出
					_eventService.UpdateEvent(existingEvent); // 更新資料庫
				}
				Console.WriteLine($"removePhoto value: {removePhoto}");
				// 上傳新照片
				if (model.UploadedEventImg != null)
				{
					// 驗證檔案副檔名
					var fileExtension = Path.GetExtension(model.UploadedEventImg.FileName).ToLower();
					var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
					if (!permittedExtensions.Contains(fileExtension))
					{
						ModelState.AddModelError("UploadedEventImg", "只允許上傳圖片格式的檔案（.jpg, .jpeg, .png, .gif）。");
						ViewBag.Categories = _eventService.GetCategories();
						return View(model);
					}

					// 驗證檔案大小
					const int maxFileSize = 5 * 1024 * 1024; // 5MB
					if (model.UploadedEventImg.ContentLength > maxFileSize)
					{
						ModelState.AddModelError("UploadedEventImg", "檔案大小不能超過 5MB。");
						ViewBag.Categories = _eventService.GetCategories();
						return View(model);
					}

					// 確定圖片存放資料夾
					var uploadsFolder = Path.Combine(Server.MapPath("~/Images"));
					if (!Directory.Exists(uploadsFolder))
					{
						Directory.CreateDirectory(uploadsFolder);
					}

					// 基於活動 ID 命名新圖片
					var sanitizedFileName = Path.GetFileNameWithoutExtension(model.UploadedEventImg.FileName)
						.Replace(" ", "_")
						.Replace(":", "_")
						.Replace("/", "_"); // 替換掉不安全的字元
					var uniqueFileName = $"Event{model.Id}_{sanitizedFileName}{fileExtension}";
					var filePath = Path.Combine(uploadsFolder, uniqueFileName);

					// 儲存新圖片
					model.UploadedEventImg.SaveAs(filePath);
					newImagePath = Path.Combine("Images", uniqueFileName).Replace("\\", "/");
				}

				// 更新圖片路徑
				existingEvent.EventImg = newImagePath;
				_eventService.UpdateEvent(existingEvent); // 更新圖片路徑到資料庫

				//Console.WriteLine($"Final EventImg value before returning: {existingEvent.EventImg}"); // 最終確認
				// 更新成功後跳轉到活動詳情頁
				return RedirectToAction("EventDetail", new { id = model.Id });
			}
			catch (Exception ex)
			{
				
				ViewBag.Categories = _eventService.GetCategories();
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


		//刪除留言
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteComment(int commentId, int eventId)
		{
			try
			{
				int currentMemberId = GetCurrentMemberId(); // 獲取當前使用者的 MemberId
				_eventService.RemoveComment(commentId, currentMemberId); // 執行刪除邏輯

				// 重定向到活動詳細頁，附加時間戳避免快取
				return RedirectToAction("EventDetail", new { id = eventId, ts = DateTime.Now.Ticks });
			}
			catch (UnauthorizedAccessException)
			{
				return new HttpStatusCodeResult(403, "無權刪除此評論。");
			}
			catch (KeyNotFoundException)
			{
				return HttpNotFound("找不到該評論。");
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"刪除評論時發生錯誤：{ex.Message}");
			}
		}




		public ActionResult EditComment(int commentId)
		{
			// 獲取當前使用者 ID
			int currentMemberId = GetCurrentMemberId();

			// 從 Service 獲取單個評論
			var comment = _eventService.GetEventCommentById(commentId, currentMemberId);

			if (comment == null)
			{
				return HttpNotFound("找不到該留言。");
			}

			// 檢查是否為該評論的擁有者
			if (!comment.IsCommentOwner)
			{
				return new HttpStatusCodeResult(403, "無權編輯此留言。");
			}

			return View(comment); // 返回 View 並傳遞評論
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditComment(int commentId, string updatedContent, int eventId)
		{
			try
			{
				// 獲取當前使用者 ID
				int currentMemberId = GetCurrentMemberId();

				// 從 Service 獲取單個評論
				var comment = _eventService.GetEventCommentById(commentId, currentMemberId);

				if (comment == null)
				{
					return HttpNotFound("找不到該留言。");
				}

				// 確認是否為該評論的擁有者
				if (!comment.IsCommentOwner)
				{
					return new HttpStatusCodeResult(403, "無權編輯此留言。");
				}

				// 更新評論內容
				comment.EventCommentContent = updatedContent;
				_eventService.UpdateComment(comment);

				return RedirectToAction("EventDetail", new { id = eventId });
			}
			catch (Exception ex)
			{
				return new HttpStatusCodeResult(500, $"編輯留言時發生錯誤：{ex.Message}");
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

				// 獲取當前用戶 ID
				int currentMemberId = GetCurrentMemberId(); 

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
			if (!User.Identity.IsAuthenticated)
			{
				throw new UnauthorizedAccessException("使用者未登入。");
			}

			// 取得身份驗證 Cookie
			var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
			if (authCookie == null)
			{
				throw new UnauthorizedAccessException("未找到身份驗證 Cookie。");
			}

			// 解密票據
			var ticket = FormsAuthentication.Decrypt(authCookie.Value);
			if (ticket == null || string.IsNullOrEmpty(ticket.UserData))
			{
				throw new UnauthorizedAccessException("身份驗證票據無效。");
			}

			// 從 UserData 中取得 MemberId
			if (int.TryParse(ticket.UserData, out int memberId))
			{
				return memberId;
			}

			throw new Exception("票據中的用戶 ID 無效。");
		}

	

	}
}
