using Microsoft.AspNetCore.Mvc;
using HeartSpaceAdmin.Models.EFModels;
using System.Linq;
using HeartSpaceAdmin.Models;
using Microsoft.AspNetCore.Identity;

public class MemberController : Controller
{
	private readonly AppDbContext _context;

	public MemberController(AppDbContext context)
	{
		_context = context;
	}

	// Index
	public IActionResult Index()
	{
		var members = _context.Members.Select(m => new MemberViewModel
		{
			Id = m.Id,
			Account = m.Account,
			Name = m.Name,
			NickName = m.NickName,
			Email = m.Email,
			Disabled = m.Disabled,
			Role = m.Role,
			AbsenceCount = m.AbsenceCount,
			MemberImg = m.MemberImg
		}).ToList();

		return View(members);
	}

	// Create (GET)
	public IActionResult Create()
	{
		return View(new MemberViewModel());
	}

	// Create (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(MemberViewModel model)
	{
		if (ModelState.IsValid)
		{
			if (_context.Members.Any(m => m.Account == model.Account))
			{
				ModelState.AddModelError("Account", "帳號已存在！");
				return View(model);
			}

			var member = new Member
			{
				Account = model.Account,
				Name = model.Name,
				NickName = model.NickName,
				Email = model.Email,
				Disabled = model.Disabled,
				Role = model.Role,
				AbsenceCount = model.AbsenceCount,
				PasswordHash = model.PasswordHash,
				ConfirmCode = model.ConfirmCode,
				IsConfirmed = model.IsConfirmed
			};

			try
			{   // 處理圖片上傳
				if (model.MemberImgFile != null && model.MemberImgFile.Length > 0)
				{
					// 確保目錄存在
					var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
					if (!Directory.Exists(uploadDir))
					{
						Directory.CreateDirectory(uploadDir);
					}

					// 檔案名稱與路徑
					var fileExtension = Path.GetExtension(model.MemberImgFile.FileName)?.ToLower();
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
					if (!allowedExtensions.Contains(fileExtension))
					{
						ModelState.AddModelError("MemberImgFile", "僅接受 JPG、JPEG、PNG格式圖片。");
						return View(model);
					}

					var fileName = $"Member_{Guid.NewGuid()}.jpg"; // 避免檔案重複
					var savePath = Path.Combine(uploadDir, fileName);

					using (var stream = new FileStream(savePath, FileMode.Create))
					{
						await model.MemberImgFile.CopyToAsync(stream); // 儲存檔案
					}

					member.MemberImg = $"/Images/{fileName}"; // 儲存相對路徑到資料庫
				}

				// 儲存會員資料
				_context.Members.Add(member);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "無法新增會員，請稍後再試！");
				Console.WriteLine($"圖片處理失敗：{ex.Message}");
			}
		}

		return View(model);
	}


	// Edit (GET)
	public IActionResult Edit(int id)
	{
		var member = _context.Members.Find(id);
		if (member == null)
		{
			return NotFound();
		}

		var model = new MemberViewModel
		{

			Id = member.Id,
			Account = member.Account,
			Name = member.Name,
			NickName = member.NickName,
			Email = member.Email,
			Disabled = member.Disabled,
			Role = member.Role,
			AbsenceCount = member.AbsenceCount,
			MemberImg = member.MemberImg,
			PasswordHash = member.PasswordHash,
			ConfirmCode = member.ConfirmCode,
			IsConfirmed = member.IsConfirmed
		};

		return View(model);
	}

	// Edit (POST)
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(MemberViewModel model)
	{
		if (ModelState.IsValid)
		{
			var member = _context.Members.Find(model.Id);
			if (member == null)
			{
				return NotFound();
			}

			member.Account = model.Account;
			member.Name = model.Name;
			member.NickName = model.NickName;
			member.Email = model.Email;
			member.Disabled = model.Disabled;
			member.Role = model.Role;
			member.AbsenceCount = model.AbsenceCount;
			member.PasswordHash = member.PasswordHash;
			member.ConfirmCode = member.ConfirmCode;
			member.IsConfirmed = member.IsConfirmed;

			try
			{
				// 處理圖片上傳
				// 檢查檔案格式
				var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
				var fileExtension = Path.GetExtension(model.MemberImgFile.FileName)?.ToLower();

				if (!allowedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("MemberImgFile", "僅接受 JPG、JPEG、PNG 格式圖片。");
					return View(model);
				}

				// 確保目錄存在
				var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
				if (!Directory.Exists(uploadDir))
				{
					Directory.CreateDirectory(uploadDir);
				}

				// 生成檔案名稱
				var fileName = $"Member_{Guid.NewGuid()}{fileExtension}"; // 保留原始格式
				var savePath = Path.Combine(uploadDir, fileName);

				// 儲存圖片
				using (var stream = new FileStream(savePath, FileMode.Create))
				{
					await model.MemberImgFile.CopyToAsync(stream);
				}

				// 刪除舊圖片（如果存在）
				if (!string.IsNullOrEmpty(member.MemberImg))
				{
					var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", member.MemberImg.TrimStart('/'));
					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}

				// 更新圖片路徑到資料庫
				member.MemberImg = $"/Images/{fileName}";


				// 儲存變更到資料庫
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
		}
			catch (Exception ex)
			{
				// 記錄錯誤
				ModelState.AddModelError("", "更新會員資料失敗，請稍後再試！");
				Console.WriteLine($"圖片處理失敗：{ex.Message}");
			}
		}
		return View(model);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public IActionResult ToggleDelete(int id)
	{
		// 確保從正確的表取數據
		var memberEntity = _context.Members.FirstOrDefault(m => m.Id == id);
		if (memberEntity == null)
		{
			return NotFound();
		}

		// 切換 Disabled 狀態
		memberEntity.Disabled = !memberEntity.Disabled;
		_context.SaveChanges();

		return RedirectToAction("Index");
	}
}
