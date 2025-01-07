using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System.Linq;
using System.Web.Mvc;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : Controller
	{
		// GET: Account
		private readonly AppDbContext _db = new AppDbContext(); // 替換為您的 DbContext 類別

		[HttpGet]
		public ActionResult Login()
		{
			return View(new LoginViewModel());
		}

		[HttpPost]
		public ActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// 比對帳號與密碼 (假設 PasswordHash 已加密)
			var member = _db.Members
				.FirstOrDefault(m => m.Account == model.Account && m.PasswordHash == model.Password);

			if (member == null)
			{
				ModelState.AddModelError("", "帳號或密碼錯誤");
				return View(model);
			}

			// 登入成功邏輯，例如建立會話或票證
			Session["UserId"] = member.Id;
			Session["UserName"] = member.NickName;
			Session["UserRole"] = member.Role;

			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public ActionResult Register()
		{
			return View(new RegisterViewModel());
		}

		[HttpPost]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// 處理註冊邏輯
			var newMember = new Member
			{
				Account = model.Account,
				PasswordHash = model.Password, // 請加密密碼
				NickName = model.NickName,
				Email = model.Email,
				IsConfirmed = false
			};

			_db.Members.Add(newMember);
			_db.SaveChanges();

			TempData["Message"] = "註冊成功！";
			return RedirectToAction("Login");
		}

		[HttpGet]
		public ActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public ActionResult ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			// 模擬發送重設密碼的信件
			TempData["Message"] = "重設密碼的信件已發送到您的信箱。";
			return RedirectToAction("Login");
		}

		[HttpGet]
		public ActionResult Logout()
		{
			// 清空 Session
			Session.Clear();

			// 返回首頁
			return RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public ActionResult Profile()
		{
			if (Session["UserId"] == null)
			{
				return RedirectToAction("Login", "Account");
			}

			// 加載用戶的個人資料
			var userId = (int)Session["UserId"];
			var member = _db.Members.FirstOrDefault(m => m.Id == userId);

			if (member == null)
			{
				return RedirectToAction("Login", "Account");
			}

			return View(member);
		}
	}
}
