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
			ViewBag.Categories = db.Categories.ToList(); // 抓取 Category 資料表的資料
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult CreateEvent(EventViewModel model)
		{
			if (ModelState.IsValid)
			{
				byte[] imageData = null;

				// 照片上傳處理
				if (model.Img != null && model.Img.ContentLength > 0)
				{
					using (var binaryReader = new System.IO.BinaryReader(model.Img.InputStream))
					{
						imageData = binaryReader.ReadBytes(model.Img.ContentLength);
					}
				}

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
					ParticipantMax = model.ParticipantMax,
					ParticipantMin = model.ParticipantMin,
					Limit = model.Limit,
					DeadLine = model.DeadLine,
					CommentCount = 0, // 初始評論數量為 0
					ParticipantNow = 0 // 初始參與人數為 0
				};

				db.Events.Add(newEvent);
				db.SaveChanges();

				return RedirectToAction("EventDetail", new { id = newEvent.Id });
			}

			return View(model);
		}

		public ActionResult EventDetail(int id)
		{
			var eventItem = db.Events.FirstOrDefault(e => e.Id == id);
			if (eventItem == null)
			{
				return HttpNotFound();
			}

			var model = new EventViewModel
			{
				Id = eventItem.Id,
				EventName = eventItem.EventName,
				MemberId = eventItem.MemberId,
				Description = eventItem.Description,
				EventTime = eventItem.EventTime,
				Location = eventItem.Location,
				IsOnline = eventItem.IsOnline,
				ParticipantMax = eventItem.ParticipantMax,
				ParticipantMin = eventItem.ParticipantMin,
				Limit = eventItem.Limit,
				DeadLine = eventItem.DeadLine,
				CommentCount = eventItem.CommentCount,
				ParticipantNow = eventItem.ParticipantNow
			};

			return View(model);
		}
	}
}
