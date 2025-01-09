using HeartSpace.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		public List<EventViewModel> Events { get; set; } = new List<EventViewModel>();

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

		// 報名的活動
		public List<EventRecord> RegisteredEvents { get; set; } = new List<EventRecord>();

		// 已參加的活動
		public List<EventRecord> AttendedEvents { get; set; } = new List<EventRecord>();

		// 發起的活動
		public List<EventRecord> CreatedEvents { get; set; } = new List<EventRecord>();

		// 發布的貼文
		public List<PostRecord> UserPosts { get; set; } = new List<PostRecord>();

		// 缺席活動次數
		public int AbsentEventCount { get; set; }
	}

	// 活動記錄的內部類別
	public class EventRecord
	{
		public int EventId { get; set; } // 活動 ID
		public string EventName { get; set; } // 活動名稱
		public DateTime? EventTime { get; set; } // 活動時間
		public string Location { get; set; } // 活動地點
		public bool? IsAttend { get; set; } // 是否出席
	}

	// 貼文記錄的內部類別
	public class PostRecord
	{
		public int PostId { get; set; } // 貼文 ID
		public string Title { get; set; } // 貼文標題
		public DateTime PublishTime { get; set; } // 發布時間
		public string Content { get; set; } // 貼文內容
	}
}
