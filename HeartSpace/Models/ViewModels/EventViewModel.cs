using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using HeartSpace.Attributes;

namespace HeartSpace.Models.ViewModels
{
	public class EventViewModel 
	{
		// 基本屬性
		public int Id { get; set; }

		//活動名稱
		[Required(ErrorMessage = "{0}必填")]
		[StringLength(25, ErrorMessage = "活動名稱最多 25 字元")]
		[Display(Name = "活動名稱")]
		public string EventName { get; set; }

		//類別
		[Required(ErrorMessage = "{0}必填")]
		[Display(Name = "分類 ID")]
		public int CategoryId { get; set; }

		public string CategoryName { get; set; } // 用於顯示分類名稱

		//活動資訊
		[Required(ErrorMessage = "{0}必填")]
		[RegularExpression(@"\S+", ErrorMessage = "描述不能為空白")]
		[StringLength(500, ErrorMessage = "描述最多 500 字元")]
		[Display(Name = "描述")]
		public string Description { get; set; }

		[Required(ErrorMessage = "{0}必填")]
		[Display(Name = "活動時間")]
		[DataType(DataType.DateTime)]
		[EventTimeInFuture(ErrorMessage = "活動時間必須是未來時間")]
		public DateTime EventTime { get; set; }

		[Required(ErrorMessage = "{0}必填")]
		[Display(Name = "報名截止日期")]
		[DataType(DataType.Date)]
		[DeadlineInFutureAndBeforeEvent("EventTime", ErrorMessage = "報名截止日期必須早於活動時間，且必須是未來時間")]
		public DateTime DeadLine { get; set; }

		[StringLength(500, ErrorMessage = "地點最多 500 字元")]
		[Display(Name = "地點")]
		public string Location { get; set; }

		[Required(ErrorMessage = "{0}必填")]
		[Display(Name = "是否線上")]
		public bool IsOnline { get; set; }

		[StringLength(50, ErrorMessage = "限制最多 50 字元")]
		[Display(Name = "參加限制")]
		public string Limit { get; set; }

		[Display(Name = "評論數量")]

		// 圖片相關
		[Column(TypeName = "image")]
		public byte[] EventImg { get; set; } // 用於顯示圖片的二進制數據

		[ValidateImage(5 * 1024 * 1024, "image/jpeg", "image/png", "image/gif", ErrorMessage = "圖片必須為 JPEG、PNG 或 GIF 格式，且大小不能超過 5MB")]
		public HttpPostedFileBase UploadedEventImg { get; set; } // 用於接收圖片上傳

		public string EventImgUrl => $"/Image/GetEventImage?eventId={Id}";
		public string MemberProfileImgUrl => $"/Image/GetMemberProfileImage?memberId={MemberId}";

		// 人數
		[Required(ErrorMessage = "{0}必填")]
		[Range(1, int.MaxValue, ErrorMessage = "人數下限必須至少為 1")]
		[Display(Name = "人數下限")]
		public int ParticipantMin { get; set; } = 1; // 預設值為 1

		[Required(ErrorMessage = "{0}必填")]
		[Range(1, int.MaxValue, ErrorMessage = "人數上限必須至少為 1")]
		[Display(Name = "人數上限")]
		[MaxGreaterThanMin("ParticipantMin", ErrorMessage = "最大人數必須大於或等於最小人數")]
		public int ParticipantMax { get; set; } 

		[Display(Name = "當前參加人數")]
		public int? ParticipantNow { get; set; }

		public int? CommentCount { get; set; }




		public bool IsRegistered { get; set; } // 是否已報名

		public bool IsFull { get; set; } //報名是否已滿

		// 活動是否已關閉
		[Display(Name = "是否已關閉")]
		public bool Disabled { get; set; }

		// 發起人相關
		[Display(Name = "會員 ID")]
		public int MemberId { get; set; } // 發起人 ID

		public string MemberName { get; set; } // 顯示發起人名稱

		public string MemberNickName { get; set; } // 顯示發起人暱稱

		public byte[] MemberProfileImg { get; set; } // 顯示發起人大頭照

		public string Role { get; set; }   // 當前用戶的角色（例如：member 或 admin）

		public bool IsEventOwner { get; set; } // 是否為活動發起人

		public bool IsAdmin => Role?.ToLower() == "admin";  // 是否為管理員



		public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
		public int CurrentMemberId { get; set; } // 當前登入者的 ID
	}

	public class CommentViewModel
	{
		public int Id { get; set; }
		public int MemberId { get; set; }
		public string MemberName { get; set; }
		public string MemberNickName { get; set; }
		public byte[] MemberProfileImg { get; set; }
		public string EventCommentContent { get; set; }
		public DateTime CommentTime { get; set; }
		public bool IsCommentOwner { get; set; }
	}





}
