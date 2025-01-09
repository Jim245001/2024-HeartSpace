using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
	public class ProfileViewModel
	{
		public int Id { get; set; } // 會員編號（隱藏欄位）

		[StringLength(25, ErrorMessage = "帳號長度不得超過25字元")]
		public string Account { get; set; } // 帳號（通常不可修改，視需求）

		[Required(ErrorMessage = "會員名稱是必填欄位")]
		[StringLength(25, ErrorMessage = "名稱長度不得超過25字元")]
		public string Name { get; set; } // 會員名稱

		[Required]
		public string Email { get; set; } // 電子郵件

		[Required(ErrorMessage = "會員暱稱是必填欄位")]
		[StringLength(10, ErrorMessage = "暱稱長度不得超過10字元")]
		public string NickName { get; set; } // 暱稱


	   


		[StringLength(50, ErrorMessage = "角色長度不得超過50字元")]
		public string Role { get; set; } // 角色（通常不可修改，視需求）

		public bool Disabled { get; set; } // 停用狀態

		public byte[] MemberImg { get; set; } // 會員頭像（已存在的圖片，作為顯示用）

		public HttpPostedFileBase MemberImgFile { get; set; } // 用於上傳新圖片
		// 用於前端顯示的 Base64 字串
		public string MemberImgBase64
		{
			get
			{
				return MemberImg != null
					? "data:image/png;base64," + Convert.ToBase64String(MemberImg)
					: null;
			}
		}
	}
}