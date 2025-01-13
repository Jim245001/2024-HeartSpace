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
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model); // 如果輸入驗證失敗，返回原表單
			}

			try
			{
				// 驗證帳號和密碼
				ValidateLogin(model.Account, model.Password);

				// 處理登入並生成 Cookie
				(string returnUrl, HttpCookie cookie) = ProcessLogin(model.Account);

				// 檢查 ReturnUrl 是否為本地 URL，防止 Open Redirect 攻擊
				if (!Url.IsLocalUrl(returnUrl))
				{
					returnUrl = Url.Action("Index", "Home"); // 設定為首頁
				}

				// 設置 Cookie
				Response.Cookies.Add(cookie);

				// 導向目標頁面
				return Redirect(returnUrl);
			}
			catch (Exception ex)
			{
				// 捕捉錯誤並顯示於表單
				ModelState.AddModelError("", ex.Message);
				return View(model);
			}
		}


		private (string returnUrl, HttpCookie cookie) ProcessLogin(string account)
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

		private void ValidateLogin(string account, string password)
		{
			using (var db = new AppDbContext())
			{
				var member = db.Members.FirstOrDefault(m => m.Account == account);

				if (member == null) throw new Exception("帳號錯誤");

				// 使用存儲的鹽值來驗證密碼
				if (!HashUtility.VarifySHA256(password, member.PasswordHash, member.ConfirmCode)) throw new Exception("密碼錯誤");

				if (member.IsConfirmed == false) throw new Exception("帳號未開通");
			}
		}






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
					IsConfirmed = false,
					Disabled = false,
					Role = "member",
					ConfirmCode = Guid.NewGuid().ToString() // 生成激活碼
				};

				db.Members.Add(member);
				db.SaveChanges();

			}
		}







		public ActionResult ActiveRegister(int memberId, string confirmCode)
		{

			ProcessActiveRegister(memberId, confirmCode);
			
			return View();
		
		}

		private void ProcessActiveRegister(int memberId, string confirmCode)
		{
			using (var db = new AppDbContext())
			{
				var member = db.Members.FirstOrDefault(m => m.Id == memberId && m.ConfirmCode == confirmCode && m.IsConfirmed == false);
				if (member == null) return;

				member.IsConfirmed = true;
				member.ConfirmCode = null;
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
