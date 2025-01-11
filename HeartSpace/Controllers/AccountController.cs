using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : BaseController
	{
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
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var member = _db.Members.FirstOrDefault(m => m.Account == model.Account);

			if (member == null)
			{
				ModelState.AddModelError("", "帳號或密碼錯誤");
				return View(model);
			}

			if (!VerifyPassword(model.Password, member.PasswordHash))
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

			string returnUrl = Request.QueryString["/Home/Index"];
			if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}

			return RedirectToAction("Index", "Home");
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
				return View(model);
			}

			if (_db.Members.Any(m => m.Account == model.Account))
			{
				ModelState.AddModelError("Account", "此帳號已被使用");
				return View(model);
			}

			if (_db.Members.Any(m => m.Email == model.Email))
			{
				ModelState.AddModelError("Email", "此 Email 已被使用");
				return View(model);
			}

			string passwordHash = HashPassword(model.Password);

			var newMember = new Member
			{
				Account = model.Account,
				Name = model.Name,
				PasswordHash = passwordHash,
				NickName = model.NickName,
				Email = model.Email,
				Role = "User",
				IsConfirmed = false,
				Disabled = false,
				ConfirmCode = Guid.NewGuid().ToString()
			};

			_db.Members.Add(newMember);
			_db.SaveChanges();

			TempData["SuccessMessage"] = "註冊成功！請使用您的帳號登入。";
			return RedirectToAction("Login");
		}

		/// <summary>
		/// 雜湊密碼
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashedBytes);
			}
		}

		/// <summary>
		/// 驗證密碼
		/// </summary>
		/// <param name="inputPassword"></param>
		/// <param name="storedHash"></param>
		/// <returns></returns>
		private bool VerifyPassword(string inputPassword, string storedHash)
		{
			string inputHash = HashPassword(inputPassword);
			return inputHash == storedHash;
		}

		/// <summary>
		/// 登出檢視
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Logout()
		{
			Session.Clear();
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

			int userId = (int)Session["UserId"];
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
