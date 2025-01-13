using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using HeartSpace.Utilities;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : Controller
	{
		// GET: Account
		private readonly AppDbContext _db = new AppDbContext(); 

		[HttpGet]
		public ActionResult Login()
		{
			return View(new LoginViewModel());
		}

		[HttpPost]
		public ActionResult Login(LoginViewModel model)
		{
			if (ModelState.IsValid == false) return View(model);

			var result = ValidLogin(model);
			if (result.IsSuccess != true)
			{
				ModelState.AddModelError("", result.ErrorMessage);
				return View(model);
			}

			const bool rememberMe = false; // 是否記住登入成功的會員

			var processResult = ProcessLogin(model.Account, rememberMe);

			Response.Cookies.Add(processResult.cookie);

			return Redirect(processResult.returnUrl);
		}

		private (bool IsSuccess, string ErrorMessage) ValidLogin(LoginViewModel model)
		{
			var db = new AppDbContext();
			var member = db.Members.FirstOrDefault(m => m.Account == model.Account);

			if (member == null) return (false, "帳密有誤");

			if (member.IsConfirmed.HasValue == false || member.IsConfirmed.Value == false) return (false, "會員資格尚未確認");

			var salt = HashUtility.GetSalt();
			var hashPassword = HashUtility.ToSHA256(model.Password, salt);

			return string.CompareOrdinal(member.PasswordHash, hashPassword) == 0
				? (true, null)
				: (false, "帳密有誤");
		}

		private (string returnUrl, HttpCookie cookie) ProcessLogin(string account, bool rememberMe)
		{

			var roles = string.Empty;
			var ticket = 
				new FormsAuthenticationTicket(
					1,
					account, 
					DateTime.Now, 
					DateTime.Now.AddDays(2),
					false,
					roles,
					"/");

			string value = FormsAuthentication.Encrypt(ticket);
			var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, value);
			var returnUrl = FormsAuthentication.GetRedirectUrl(account, true);

			return (returnUrl, cookie);

		}

		//====================================================================================================

		public ActionResult ActiveRegister(int memberId, string confirmCode)
		{
			var result = Load(memberId, confirmCode);

			if (result.IsSuccess) ActiveMember(memberId);


			return View();
		}

		private void ActiveMember(int memberId)
		{
			var db = new AppDbContext();
			var memberInDb = db.Members.Find(memberId);
			if (memberInDb == null) return;

			memberInDb.IsConfirmed = true;
			memberInDb.ConfirmCode = string.Empty;
			db.SaveChanges();

		}

		private (bool IsSuccess, string ErrorMessage) Load(int memberId, string confirmCode)
		{
			var db = new AppDbContext();
			var memberInDb = db.Members.Find(memberId);
			if (memberInDb == null) return (false, "查無資料");

			return string.CompareOrdinal(confirmCode, memberInDb.ConfirmCode) != 0
				? (false, "驗證碼錯誤")
				: (true, string.Empty);
		}

		//====================================================================================================

		[HttpGet]
		public ActionResult Register()
		{
			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid) // 驗證失敗，返回表單
			{
				return View(model);
			}

			try
			{
				ProcessRegister(model); // 處理註冊邏輯
				TempData["SuccessMessage"] = "註冊成功！請使用您的帳號登入。";
				return RedirectToAction("Login");
			}
			catch (Exception ex)
			{
				// 添加伺服器端全局錯誤訊息
				ModelState.AddModelError("", ex.Message);
				return View(model); // 返回原表單並顯示錯誤
			}
		}


		private void ProcessRegister(RegisterViewModel model)
		{
			using (var db = new AppDbContext())
			{
				// 檢查帳號與電子郵件是否已存在
				if (db.Members.Any(m => m.Account == model.Account))
					throw new Exception("該帳號已存在");
				if (db.Members.Any(m => m.Email == model.Email))
					throw new Exception("該電子郵件已被使用");

				// 生成密碼鹽值與雜湊
				var salt = HashUtility.GetSalt();
				var hashPassword = HashUtility.ToSHA256(model.Password, salt);

				// 建立新會員
				var member = new Member
				{
					Account = model.Account,
					PasswordHash = hashPassword,
					Email = model.Email,
					Name = model.Name,
					NickName = model.NickName,
					IsConfirmed = true,
					Disabled = false,
					Role = "member",
					ConfirmCode = Guid.NewGuid().ToString() // 生成激活碼
				};

				db.Members.Add(member);
				db.SaveChanges();

			}
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
			FormsAuthentication.SignOut();

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
