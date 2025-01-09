using HeartSpace.DAL;
using HeartSpace.Helpers;
using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using HeartSpace.Models.ViewModels;

namespace HeartSpace.Controllers
{
	public class ProfileController : Controller
	{
		private readonly AppDbContext db = new AppDbContext();

		[HttpGet]
		public ActionResult Profile(int? id)
		{
			if (id == null)
			{
				TempData["ErrorMessage"] = "會員編號未提供！";
				return RedirectToAction("ProfileList");
			}

			var member = db.Members.Find(id.Value);
			if (member == null)
			{
				TempData["ErrorMessage"] = "找不到對應的會員資料！";
				return RedirectToAction("ProfileList");
			}

			// 查詢報名的活動
			var registeredEvents = db.EventMembers
				.Where(em => em.MemberId == id)
				.Select(em => new EventRecord
				{
					EventId = em.Event.Id,
					EventName = em.Event.EventName,
					EventTime = em.Event.EventTime,
					Location = em.Event.Location,
					IsAttend = em.IsAttend
				}).ToList();

			// 查詢已參加的活動
			var attendedEvents = db.EventMembers
				.Where(em => em.MemberId == id && em.IsAttend == true)
				.Select(em => new EventRecord
				{
					EventId = em.Event.Id,
					EventName = em.Event.EventName,
					EventTime = em.Event.EventTime,
					Location = em.Event.Location,
					IsAttend = em.IsAttend
				}).ToList();

			// 查詢發起的活動
			var createdEvents = db.Events
				.Where(e => e.MemberId == id)
				.Select(e => new EventRecord
				{
					EventId = e.Id,
					EventName = e.EventName,
					EventTime = e.EventTime,
					Location = e.Location
				}).ToList();

			// 查詢發布的貼文
			var userPosts = db.Posts
				.Where(p => p.MemberId == id)
				.Select(p => new PostRecord
				{
					PostId = p.Id,
					Title = p.Title,
					PublishTime = p.PublishTime,
					Content = p.PostContent
				}).ToList();

			// 計算缺席次數
			int absentEventCount = GetAbsentEventCount(id.Value);

			// 建立 ViewModel
			var model = new ProfileViewModel
			{
				Id = member.Id,
				Name = member.Name,
				NickName = member.NickName,
				RegisteredEvents = registeredEvents,
				AttendedEvents = attendedEvents,
				CreatedEvents = createdEvents,
				UserPosts = userPosts,
				AbsentEventCount = absentEventCount
			};


			var events = db.EventMembers
				.Where(em => em.MemberId == id)
				.Select(em => new EventViewModel
				{
					Id = em.Event.Id,
					EventName = em.Event.EventName,
					EventTime = em.Event.EventTime,
					Location = em.Event.Location
				}).ToList();

			ViewBag.Events = events;


			return View(model);
		}

		private int GetAbsentEventCount(int memberId)
		{
			// 從資料庫中計算指定使用者的缺席次數
			return db.EventMembers
				.Count(em => em.MemberId == memberId && em.IsAttend == false);
		}

		[HttpGet]
		public ActionResult EditProfile(int? id)
		{
			if (id == null)
			{
				TempData["ErrorMessage"] = "會員編號未提供！";
				return RedirectToAction("ProfileList");
			}

			var member = db.Members.Find(id.Value);
			if (member == null)
			{
				TempData["ErrorMessage"] = "找不到對應的會員資料！";
				return RedirectToAction("ProfileList");
			}

			// 計算缺席次數
			int absentEventCount = GetAbsentEventCount(id.Value);

			// 建立 ViewModel
			var model = new ProfileViewModel
			{
				Id = member.Id,
				Name = member.Name,
				NickName = member.NickName,
				MemberImg = member.MemberImg,
				AbsentEventCount = absentEventCount
			};

			return View(model);
		}

		[HttpPost]
		public ActionResult EditProfile(ProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				model.AbsentEventCount = GetAbsentEventCount(model.Id);
				return View(model);
			}

			// 更新會員邏輯
			var member = db.Members.Find(model.Id);
			if (member != null)
			{
				member.Name = model.Name;
				member.NickName = model.NickName;

				if (model.MemberImgFile != null && model.MemberImgFile.ContentLength > 0)
				{
					using (var binaryReader = new BinaryReader(model.MemberImgFile.InputStream))
					{
						member.MemberImg = binaryReader.ReadBytes(model.MemberImgFile.ContentLength);
					}
				}

				db.SaveChanges();
				TempData["SuccessMessage"] = "會員資料已更新！";
			}
			else
			{
				TempData["ErrorMessage"] = "找不到對應的會員資料！";
			}

			return RedirectToAction("EditProfile", new { id = model.Id });
		}
	}
}
