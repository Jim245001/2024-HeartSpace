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
		private readonly EFModel1 db = new EFModel1();

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

				// 將 ViewModel 資料映射到資料庫模型
				var newEvent = new Event
				{
					EventName = model.EventName,
					MemberId = model.MemberId,
					img = imageData,
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

			
			int participantNow = GetParticipantNow(id);

			var model = new EventViewModel
			{
				Id = eventItem.Id,
				
				EventName = eventItem.EventName,
				MemberId = eventItem.MemberId,
				Description = eventItem.Description,
				Img = eventItem.img,
				EventTime = eventItem.EventTime,
				Location = eventItem.Location,
				IsOnline = eventItem.IsOnline,
				ParticipantMax = eventItem.ParticipantMax,
				ParticipantMin = eventItem.ParticipantMin,
				Limit = eventItem.Limit,
				DeadLine = eventItem.DeadLine,
				CommentCount = eventItem.CommentCount,
				ParticipantNow = participantNow,
				CategoryName = categoryName
			};

			return View(model);
		}


		// 計算參與人數
		public int GetParticipantNow(int eventId)
		{
			return db.EventMembers.Count(em => em.EventId == eventId);
		}
	}
}
