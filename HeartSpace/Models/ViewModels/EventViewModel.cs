using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace HeartSpace.Models.ViewModels
{
	public class EventViewModel : IValidatableObject
	{
		// 基本屬性
		public int Id { get; set; }

		[Required(ErrorMessage = "{0}必填")]
		[StringLength(25, ErrorMessage = "活動名稱最多 25 字元")]
		[Display(Name = "活動名稱")]
		public string EventName { get; set; }

		[Display(Name = "分類 ID")]
		public int CategoryId { get; set; }

		public string CategoryName { get; set; } // 用於顯示分類名稱

		[Required(ErrorMessage = "{0}必填")]
		[StringLength(500, ErrorMessage = "描述最多 500 字元")]
		[Display(Name = "描述")]
		public string Description { get; set; }

		[Display(Name = "活動時間")]
		[DataType(DataType.DateTime)]
		public DateTime EventTime { get; set; }

		[StringLength(500, ErrorMessage = "地點最多 500 字元")]
		[Display(Name = "地點")]
		public string Location { get; set; }

		[Display(Name = "是否線上")]
		public bool IsOnline { get; set; }

		// 圖片相關
		[Display(Name = "照片")]
		public byte[] Img { get; set; } // 用於顯示圖片的二進制數據

		[Display(Name = "上傳照片")]
		public HttpPostedFileBase UploadedImg { get; set; } // 用於接收圖片上傳

		// 參與者相關
		[Required(ErrorMessage = "最小參加人數是必填欄位")]
		[Range(1, int.MaxValue, ErrorMessage = "最小參加人數必須大於或等於 1")]
		[Display(Name = "最小參加人數")]
		public int ParticipantMin { get; set; } = 1; // 預設值為 1

		[Range(1, int.MaxValue, ErrorMessage = "最大參加人數必須大於或等於 1")]
		[Display(Name = "最大參加人數")]
		public int? ParticipantMax { get; set; } // 允許為空（無上限）

		[Display(Name = "當前參加人數")]
		public int? ParticipantNow { get; set; }

		[StringLength(50, ErrorMessage = "限制最多 50 字元")]
		[Display(Name = "參加限制")]
		public string Limit { get; set; }

		[Display(Name = "報名截止日期")]
		[DataType(DataType.Date)]
		public DateTime DeadLine { get; set; }

		[Display(Name = "評論數量")]
		public int? CommentCount { get; set; }

		public bool IsRegistered { get; set; } // 是否已報名

		// 活動是否已關閉
		[Display(Name = "是否已關閉")]
		public bool Disabled { get; set; }

		// 發起人相關
		[Display(Name = "會員 ID")]
		public int MemberId { get; set; } // 發起人 ID

		public string MemberName { get; set; } // 顯示發起人名稱

		public byte[] MemberProfileImg { get; set; } // 顯示發起人大頭照

		public string Role { get; set; }   // 當前用戶的角色（例如：member 或 admin）

		public bool IsEventOwner { get; set; } // 是否為活動發起人

		public bool IsAdmin => Role?.ToLower() == "admin";  // 是否為管理員

		// 自定驗證
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ParticipantMax.HasValue && ParticipantMax < ParticipantMin)
			{
				yield return new ValidationResult(
					$"最大參加人數必須大於或等於最小參加人數（{ParticipantMin}）",
					new[] { nameof(ParticipantMax) });
			}
		}

		public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
		public int CurrentMemberId { get; set; } // 當前登入者的 ID
	}

	public class CommentViewModel
	{
		public int Id { get; set; }
		public int MemberId { get; set; }
		public string MemberName { get; set; }
		public byte[] MemberProfileImg { get; set; }
		public string EventCommentContent { get; set; }
		public DateTime CommentTime { get; set; }
	}
}
