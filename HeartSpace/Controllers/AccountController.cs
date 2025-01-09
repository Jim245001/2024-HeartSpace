using HeartSpace.Helpers;
using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HeartSpace.Controllers.Account
{
	public class AccountController : Controller
	{
		private readonly AppDbContext _db = new AppDbContext();

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

			// 查找帳號
			var member = _db.Members.FirstOrDefault(m => m.Account == model.Account);

			// 驗證密碼
			if (member == null || !PasswordHelper.VerifyPassword(model.Password, member.PasswordHash))
			{
				ModelState.AddModelError("", "帳號或密碼錯誤");
				return View(model);
			}

			// 登入成功，建立 Session
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
			// 驗證模型
			if (!ModelState.IsValid)
			{
				return View(model); // 如果驗證失敗，返回原表單頁面並顯示錯誤訊息
			}

			// 檢查帳號和暱稱是否已存在
			if (_db.Members.Any(m => m.Account == model.Account))
			{
				ModelState.AddModelError("Account", "該帳號已被使用，請選擇其他帳號。");
				return View(model);
			}
			if (_db.Members.Any(m => m.NickName == model.NickName))
			{
				ModelState.AddModelError("NickName", "該暱稱已被使用，請選擇其他暱稱。");
				return View(model);
			}

			// 創建新會員並加密密碼
			var newMember = new Member
			{
				Account = model.Account,
				Name = model.Name, // 如果需要保存姓名，確保 RegisterViewModel 有 Name 屬性
				PasswordHash = PasswordHelper.HashPassword(model.Password), // 加密密碼
				Email = model.Email,
				NickName = model.NickName,
				Role = "User", // 設定默認角色
				IsConfirmed = false, // 初始為未驗證
				Disabled = false // 初始為未禁用
			};

			try
			{
				// 新增會員到資料庫
				_db.Members.Add(newMember);
				_db.SaveChanges();

				// 設定成功訊息並跳轉到登入頁面
				TempData["Message"] = "註冊成功！";
				return RedirectToAction("Login");
			}
			catch (System.Data.Entity.Validation.DbEntityValidationException ex)
			{
				// 捕捉並處理驗證例外
				foreach (var validationErrors in ex.EntityValidationErrors)
				{
					foreach (var validationError in validationErrors.ValidationErrors)
					{
						// 添加詳細錯誤訊息到 ModelState
						ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
					}
				}

				// 返回原表單頁面並顯示錯誤訊息
				return View(model);
			}
			catch (Exception ex)
			{
				// 捕捉其他例外（如資料庫連線錯誤）
				ModelState.AddModelError("", $"系統發生錯誤，請稍後再試！詳細錯誤：{ex.Message}");
				return View(model);
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
