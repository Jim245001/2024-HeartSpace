using HeartSpace.DAL;
using HeartSpace.Helpers;
using HeartSpace.Models;
using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;

namespace HeartSpace.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        //[Authorize]
        public ActionResult EditProfile()
        {
            int memberId = 1;

            // 從資料庫取得會員資料
            var member = db.Members.Find(memberId);
            if (member == null)
            {
                return HttpNotFound();
            }

            // 填入 ViewModel
            var viewModel = new ProfileViewModel
            {
                Id = member.Id,
                Name = member.Name,
                Email = member.Email,
                Account = member.Account,
                MemberImg = member.MemberImg,
                NickName = member.NickName
            };

            return View(viewModel);
        }

        [HttpPost]
        //[Authorize]
        public ActionResult EditProfile(ProfileViewModel model)
        {
            // 檢查 ModelState 是否有效
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("<br/>", errors);
                return View(model);
            }

            // 檢查暱稱是否重複
            var isNickNameExists = db.Members.Any(m => m.NickName == model.NickName && m.Id != model.Id);
            if (isNickNameExists)
            {
                ModelState.AddModelError("NickName", "該暱稱已被使用，請選擇其他暱稱。");
                return View(model);
            }

            // 嘗試更新會員資料
            var member = db.Members.Find(model.Id);
            if (member != null)
            {
                // 更新會員名稱和暱稱
                member.Name = model.Name;
                member.NickName = model.NickName;

                // 處理頭像上傳
                if (model.MemberImgFile != null && model.MemberImgFile.ContentLength > 0)
                {
                    try
                    {
                        using (var binaryReader = new System.IO.BinaryReader(model.MemberImgFile.InputStream))
                        {
                            member.MemberImg = binaryReader.ReadBytes(model.MemberImgFile.ContentLength); // 將圖片轉為 byte[] 並儲存
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("MemberImgFile", "上傳圖片時發生錯誤：" + ex.Message);
                        return View(model); // 發生錯誤時，回傳表單
                    }
                }

                // 保存資料庫變更
                try
                {
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "資料已成功儲存！";
                    return RedirectToAction("EditProfile");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "儲存資料時發生錯誤：" + ex.Message);
                    return View(model);
                }
            }
            else
            {
                TempData["ErrorMessage"] = "找不到對應的會員資料！";
            }

            return View(model);
        }
    }
}