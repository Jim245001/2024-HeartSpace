using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
	public class ImageController : Controller
	{
		private readonly AppDbContext _context;

		public ImageController()
		{
			_context = new AppDbContext();
		}

		public ActionResult GetEventImage(int eventId)
		{
			var eventImg = _context.Events
				.Where(e => e.Id == eventId)
				.Select(e => e.EventImg)
				.FirstOrDefault();

			if (eventImg == null)
			{
				return HttpNotFound();
			}

			return File(eventImg, "image/jpeg"); // 假設圖片為 JPEG 格式
		}

		public ActionResult GetMemberProfileImage(int memberId)
		{
			var profileImg = _context.Members
				.Where(m => m.Id == memberId)
				.Select(m => m.MemberImg)
				.FirstOrDefault();

			if (profileImg == null)
			{
				return HttpNotFound();
			}

			return File(profileImg, "image/jpeg"); // 假設圖片為 JPEG 格式
		}
	}

}