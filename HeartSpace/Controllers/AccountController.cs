using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : BaseController
	{
		// GET: Account
		private readonly AppDbContext _db = new AppDbContext(); // 替換為您的 DbContext 類別


		/// <summary>
		/// 登入頁
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Login()
		{
			return View(new LoginViewModel());
		}


		/// <summary>
		/// 處理登入表單提交
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		public ActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var member = _db.Members
				.FirstOrDefault(m => m.Account == model.Account);

			if (member == null)
			{
				ModelState.AddModelError("", "帳號或密碼錯誤");
				return View(model);
			}

			string salt = GenerateSalt(member.Account);

			if (!VerifyPassword(model.Password, member.PasswordHash, salt))
			{
				ModelState.AddModelError("", "帳號或密碼錯誤");
				return View(model);
			}

			Session["UserId"] = member.Id;
			Session["UserName"] = member.NickName;
			Session["UserRole"] = member.Role;

			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
				1,
				member.NickName,
				DateTime.Now,
				DateTime.Now.AddMinutes(30),
				false,
				$"{member.Id}|{member.Role}"
			);

			string encryptedTicket = FormsAuthentication.Encrypt(ticket);
			var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
			Response.Cookies.Add(authCookie);

			string returnUrl = Request.QueryString["ReturnUrl"];
			if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction("Index", "Home");
		}


		/// <summary>
		/// 驗證密碼
		/// </summary>
		/// <param name="inputPassword"></param>
		/// <param name="storedHash"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		private bool VerifyPassword(string inputPassword, string storedHash, string salt)
		{
			using (var sha256 = System.Security.Cryptography.SHA256.Create())
			{
				string saltedPassword = inputPassword + salt; // 將密碼與鹽值結合
				byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
				string inputHash = Convert.ToBase64String(hashedBytes); // 將結果轉換為 Base64 字串
				return inputHash == storedHash; // 比對雜湊值是否一致
			}
		}


		/// <summary>
		/// 生成鹽值
		/// </summary>
		/// <param name="account"></param>
		/// <returns></returns>
		private string GenerateSalt(string account)
		{
			using (var sha256 = System.Security.Cryptography.SHA256.Create())
			{
				byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(account));
				return Convert.ToBase64String(hashBytes).Substring(0, 16); // 取前 16 個字元作為鹽值
			}
		}



		/// <summary>
		/// 註冊頁
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Register()
		{
			return View(new RegisterViewModel());
		}

		/// <summary>
		/// 處理註冊表單提交
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model); // 表單驗證失敗，返回原視圖
			}

			// 檢查帳號是否已存在
			if (_db.Members.Any(m => m.Account == model.Account))
			{
				ModelState.AddModelError("Account", "此帳號已被使用");
				return View(model);
			}

			// 檢查 Email 是否已存在
			if (_db.Members.Any(m => m.Email == model.Email))
			{
				ModelState.AddModelError("Email", "此 Email 已被使用");
				return View(model);
			}

			// 生成鹽值
			string salt = GenerateSalt(model.Account);

			// 雜湊密碼
			string passwordHash = HashPassword(model.Password, salt);

			// 建立新的會員資料
			var newMember = new Member
			{
				Account = model.Account,
				Name = model.Name,
				PasswordHash = passwordHash,
				NickName = model.NickName,
				Email = model.Email,
				Role = "User", // 預設角色
				IsConfirmed = false, // 註冊後需進行驗證
				Disabled = false,
				ConfirmCode = Guid.NewGuid().ToString() // 註冊驗證碼
			};

			// 儲存到資料庫
			_db.Members.Add(newMember);
			_db.SaveChanges();

			// 註冊完成，重導向登入頁
			TempData["SuccessMessage"] = "註冊成功！請使用您的帳號登入。";
			return RedirectToAction("Login");
		}


		/// <summary>
		/// 雜湊密碼
		/// </summary>
		/// <param name="password"></param>
		/// <param name="salt"></param>
		/// <returns></returns>
		private string HashPassword(string password, string salt)
		{
			using (var sha256 = SHA256.Create())
			{
				string saltedPassword = password + salt;
				byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
				return Convert.ToBase64String(hashBytes);
			}
		}

		/// <summary>
		/// 登出檢視
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Logout()
		{
			// 清空 Session
			Session.Clear();

			// 返回首頁
			return RedirectToAction("Index", "Home");
		}


		/// <summary>
		/// 個人頁檢視
		/// </summary>
		/// <returns></returns>
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


		/// <summary>
		/// 忘記密碼頁面
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult ForgotPassword()
		{
			return View();
		}

	}

}
