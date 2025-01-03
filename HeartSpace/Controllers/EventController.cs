using System;
using System.Linq;
using System.Web.Mvc;
using HeartSpace.Models;
using HeartSpace.Models.EFModel;
using HeartSpace.Models.ViewModels;

namespace HeartSpace.Controllers
{
	public class EventController : Controller
	{
		private readonly AppDbContext db = new AppDbContext();

		[HttpGet]
		public ActionResult CreateEvent()
		{
			ViewBag.Categories = db.Categories.OrderBy(c => c.DisplayOrder).ToList(); // 抓取 Category 資料表的資料
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateEvent(EventViewModel model)
		{
			// 檢查 ModelState 是否有效
			if (ModelState.IsValid)
			{
				// 驗證邏輯：最大參加人數必須大於或等於最小參加人數
				if (model.ParticipantMax.HasValue && model.ParticipantMax < model.ParticipantMin)
				{
					ModelState.AddModelError(nameof(model.ParticipantMax), $"最大參加人數必須大於或等於最小參加人數（{model.ParticipantMin}）");
					return View(model);
				}

				byte[] imageData = null;

				// 照片上傳處理
				if (model.UploadedImg != null && model.UploadedImg.ContentLength > 0)
				{
					using (var binaryReader = new System.IO.BinaryReader(model.UploadedImg.InputStream))
					{
						imageData = binaryReader.ReadBytes(model.UploadedImg.ContentLength);
					}
				}

				// 如果最大參加人數為 null，表示無上限
				var participantMax = model.ParticipantMax.HasValue ? model.ParticipantMax.Value : int.MaxValue;

				// 取得目前登入者的 MemberId**
				int currentMemberId = GetCurrentMemberId(); // 假設這是一個可以取得登入者 ID 的方法

				// 將 ViewModel 資料映射到資料庫模型
				var newEvent = new Event
				{
					EventName = model.EventName,
					MemberId = currentMemberId,
					EventImg = imageData,
					CategoryId = model.CategoryId,
					Description = model.Description,
					EventTime = model.EventTime,
					Location = model.Location,
					IsOnline = model.IsOnline,
					ParticipantMax = participantMax, // 處理「無上限」的情況
					ParticipantMin = model.ParticipantMin,
					Limit = model.Limit,
					DeadLine = model.DeadLine,
					CommentCount = 0, // 初始評論數量為 0
					ParticipantNow = 0 // 初始參與人數為 0
				};

				db.Events.Add(newEvent);
				db.SaveChanges();

				// 重定向到活動詳情頁面
				return RedirectToAction("EventDetail", new { id = newEvent.Id });
			}

			// 如果驗證失敗，返回表單視圖並顯示錯誤訊息
			ViewBag.Categories = db.Categories.OrderBy(c => c.DisplayOrder).ToList(); // 重新填充分類資料
			return View(model);
		}


		public ActionResult EventDetail(int id)
		{
			var eventItem = db.Events.FirstOrDefault(e => e.Id == id);
			if (eventItem == null)
			{
				return HttpNotFound();
			}

			// 根據 CategoryId 從 Categories 表中取得類別名稱
			var categoryName = db.Categories
				.Where(c => c.Id == eventItem.CategoryId)
				.Select(c => c.CategoryName)
				.FirstOrDefault();

			// 根據 MemberId 從 Members 表中取得發起人姓名和大頭照
			var member = db.Members
				.Where(m => m.Id == eventItem.MemberId)
				.Select(m => new
				{
					MemberName = m.Name,
					MemberProfileImg = m.MemberImg
				})
				.FirstOrDefault();

			int participantNow = GetParticipantNow(id);

			int memberId = GetCurrentMemberId(); // 假設有一個方法可以取得當前用戶的 MemberId

			int currentMemberId = GetCurrentMemberId(); // 取得目前登入者的 MemberId

			var comments = db.EventComments
							   .Where(c => c.EventId == id)
							   .Select(c => new CommentViewModel
							   {
								   Id = c.Id,
								   MemberId = c.MemberId,
								   MemberName = c.Member.Name,
								   MemberProfileImg = c.Member.MemberImg,
								   EventCommentContent = c.EventCommentContent,
								   CommentTime = c.CommentTime
							   }).ToList();

			// 檢查用戶是否已報名
			bool isRegistered = db.EventMembers.Any(em => em.EventId == id && em.MemberId == memberId);

			// 判斷當前用戶是否為活動發起人
			bool isEventOwner = eventItem.MemberId == memberId;

			// 取得當前用戶的角色
			var currentUserRole = db.Members
				.Where(m => m.Id == memberId)
				.Select(m => m.Role)
				.FirstOrDefault();

			var model = new EventViewModel
			{
				Id = eventItem.Id,
				EventName = eventItem.EventName,
				MemberId = eventItem.MemberId,
				Description = eventItem.Description,
				Img = eventItem.EventImg,
				EventTime = eventItem.EventTime,
				Location = eventItem.Location,
				IsOnline = eventItem.IsOnline,
				ParticipantMax = eventItem.ParticipantMax,
				ParticipantMin = eventItem.ParticipantMin,
				Limit = eventItem.Limit,
				DeadLine = eventItem.DeadLine,
				CommentCount = eventItem.CommentCount,
				ParticipantNow = participantNow,
				CategoryName = categoryName,
				MemberName = member?.MemberName,
				MemberProfileImg = member?.MemberProfileImg,
				IsRegistered = isRegistered,
				IsEventOwner = isEventOwner,
				Role = currentUserRole,
				Comments = comments,
				CurrentMemberId = currentMemberId
			};

			return View(model);
		}



		// 計算參與人數
		public int GetParticipantNow(int eventId)
		{
			return db.EventMembers.Count(em => em.EventId == eventId) + 1; // +1是加上發起人
		}

		//處理報名和取消報名
		[HttpPost]
		public ActionResult ToggleRegistration(int eventId, string actionType)
		{
			int memberId = GetCurrentMemberId(); // 假設有一個方法可以取得當前用戶的 MemberId

			if (actionType == "cancel")
			{
				// 取消報名邏輯
				var existingRegistration = db.EventMembers
					.FirstOrDefault(em => em.EventId == eventId && em.MemberId == memberId);

				if (existingRegistration != null)
				{
					db.EventMembers.Remove(existingRegistration);
					db.SaveChanges();
					TempData["Message"] = "您已成功取消報名。";
				}
			}
			else if (actionType == "register")
			{
				// 報名活動邏輯
				var newRegistration = new EventMember
				{
					EventId = eventId,
					MemberId = memberId
				};

				db.EventMembers.Add(newRegistration);
				db.SaveChanges();
				TempData["Message"] = "您已成功報名活動！";
			}

			// 返回活動詳情頁面
			return RedirectToAction("EventDetail", new { id = eventId });
		}


		//簡單測試 pending
		private int GetCurrentMemberId()
		{
			return 3; // 測試用固定 MemberId
		}


		//關閉活動
		[HttpPost]
		public ActionResult CloseEvent(int id)
		{
			var eventItem = db.Events.FirstOrDefault(e => e.Id == id);
			if (eventItem == null)
			{
				return HttpNotFound();
			}

			// 設置活動為關閉狀態
			eventItem.Disabled = true;
			db.SaveChanges();

			TempData["Message"] = "活動已成功關閉。";
			return RedirectToAction("Index", "Home");
		}

		//修改活動
		[HttpGet]
		public ActionResult EditEvent(int id)
		{
			// 從資料庫取得活動資料
			var eventItem = db.Events.FirstOrDefault(e => e.Id == id);
			if (eventItem == null)
			{
				return HttpNotFound();
			}

			// 檢查目前登入者是否為活動發起人
			int currentUserId = GetCurrentMemberId(); // 假設有一個方法取得當前登入者的 MemberId
			if (eventItem.MemberId != currentUserId)
			{
				TempData["Error"] = "您沒有權限編輯此活動！";
				return RedirectToAction("EventDetail", new { id });
			}

			// 取得活動分類
			ViewBag.Categories = db.Categories
				.OrderBy(c => c.DisplayOrder)
				.ToList();

			// 建立 ViewModel
			var model = new EventViewModel
			{
				Id = eventItem.Id,
				EventName = eventItem.EventName,
				Description = eventItem.Description,
				CategoryId = eventItem.CategoryId,
				EventTime = eventItem.EventTime,
				DeadLine = eventItem.DeadLine,
				Location = eventItem.Location,
				IsOnline = eventItem.IsOnline,
				ParticipantMin = eventItem.ParticipantMin,
				ParticipantMax = eventItem.ParticipantMax,
				Limit = eventItem.Limit,
				Img = eventItem.EventImg // 將目前圖片帶入
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditEvent(EventViewModel model)
		{

			// 從資料庫取得對應的活動
			var eventItem = db.Events.FirstOrDefault(e => e.Id == model.Id);
			if (eventItem == null)
			{
				return HttpNotFound();
			}

			// 檢查目前登入者是否為活動發起人
			int currentUserId = GetCurrentMemberId(); // 假設有一個方法取得當前登入者的 MemberId
			if (eventItem.MemberId != currentUserId)
			{
				TempData["Error"] = "您沒有權限編輯此活動！";
				return RedirectToAction("EventDetail", new { id = model.Id });
			}

			if (!ModelState.IsValid)
			{
				// 如果驗證失敗，重新取得分類資料並回到表單
				ViewBag.Categories = db.Categories
					.OrderBy(c => c.DisplayOrder)
					.ToList();
				return View(model);
			}


			// 更新活動資料
			eventItem.EventName = model.EventName;
			eventItem.Description = model.Description;
			eventItem.CategoryId = model.CategoryId;
			eventItem.EventTime = model.EventTime;
			eventItem.DeadLine = model.DeadLine;
			eventItem.Location = model.Location;
			eventItem.IsOnline = model.IsOnline;
			eventItem.ParticipantMin = model.ParticipantMin;
			eventItem.ParticipantMax = model.ParticipantMax;
			eventItem.Limit = model.Limit;

			// 更新圖片
			if (model.UploadedImg != null && model.UploadedImg.ContentLength > 0)
			{
				using (var binaryReader = new System.IO.BinaryReader(model.UploadedImg.InputStream))
				{
					eventItem.EventImg = binaryReader.ReadBytes(model.UploadedImg.ContentLength);
				}
			}

			db.SaveChanges(); // 儲存變更

			TempData["Message"] = "活動已成功更新！";
			return RedirectToAction("EventDetail", new { id = eventItem.Id });
		}

		//新增留言
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult AddComment(int eventId, string commentContent)
		{
			int memberId = GetCurrentMemberId(); // 取得目前登入者的 MemberId

			var newComment = new EventComment
			{
				EventId = eventId,
				MemberId = memberId,
				EventCommentContent = commentContent,
				CommentTime = DateTime.Now
			};

			db.EventComments.Add(newComment);
			db.SaveChanges();

			return RedirectToAction("EventDetail", new { id = eventId });

			

		}

		//刪除留言
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteComment(int commentId)
		{
			var comment = db.EventComments.FirstOrDefault(c => c.Id == commentId);
			if (comment == null)
			{
				return HttpNotFound();
			}

			int memberId = GetCurrentMemberId(); // 取得目前登入者的 MemberId
			if (comment.MemberId != memberId)
			{
				TempData["Error"] = "您沒有權限刪除此留言！";
				return RedirectToAction("EventDetail", new { id = comment.EventId });
			}

			db.EventComments.Remove(comment);
			db.SaveChanges();

			return RedirectToAction("EventDetail", new { id = comment.EventId });
		}
	}
}
