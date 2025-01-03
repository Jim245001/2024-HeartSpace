using HeartSpace.DAL;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
	//public class ProfileController : Controller
	//{
	//	private readonly ProfileService _profileService;

	//	public ProfileController(ProfileService profileService)
	//	{
	//		_profileService = profileService;
	//	}

	//	public ActionResult Index()
	//	{
	//		// 從 Session 獲取使用者 ID
	//		string userId = HttpContext.Session.GetString("UserId");

	//		// 將資料傳遞給 ViewModel
	//		var model = new ProfileViewModel
	//		{
	//			RegisteredActivities = _profileService.GetRegisteredActivities(userId),
	//			InitiatedActivities = _profileService.GetInitiatedActivities(userId),
	//			PublishedPosts = _profileService.GetPublishedPosts(userId),
	//			JoinedActivities = _profileService.GetJoinedActivities(userId)
	//		};

	//		return View(model);
	//	}
	//}

	//// Profile ViewModel
	//public class ProfileViewModel
	//{
	//	public IEnumerable<Activity> RegisteredActivities { get; set; }
	//	public IEnumerable<Activity> InitiatedActivities { get; set; }
	//	public IEnumerable<Post> PublishedPosts { get; set; }
	//	public IEnumerable<Activity> JoinedActivities { get; set; }
	//}
}