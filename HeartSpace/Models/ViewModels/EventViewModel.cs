using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace HeartSpace.Models.ViewModels
{
	public class EventViewModel : IValidatableObject
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "{0}必填")]
		[StringLength(25, ErrorMessage = "活動名稱最多 25 字元")]
		[Display(Name = "活動名稱")]
		public string EventName { get; set; }

		[Display(Name = "會員 ID")]
		public int MemberId { get; set; }

		[Display(Name = "照片")]
		public HttpPostedFileBase Img { get; set; } // 用於接收上傳的檔案

		[Display(Name = "分類 ID")]
		public int CategoryId { get; set; }

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

		[Required(ErrorMessage = "最小參加人數是必填欄位")]
		[Range(1, int.MaxValue, ErrorMessage = "最小參加人數必須大於或等於 1")]
		public int ParticipantMin { get; set; } = 1; // 預設值為 1

		[Range(1, int.MaxValue, ErrorMessage = "最大參加人數必須大於或等於 1")]
		public int? ParticipantMax { get; set; } // 允許為空（無上限）

		[StringLength(50, ErrorMessage = "限制最多 50 字元")]
		[Display(Name = "參加限制")]
		public string Limit { get; set; }

		[Display(Name = "報名截止日期")]
		[DataType(DataType.Date)]
		public DateTime DeadLine { get; set; }

		[Display(Name = "評論數量")]
		public int? CommentCount { get; set; }

		[Display(Name = "當前參加人數")]
		public int? ParticipantNow { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (ParticipantMax.HasValue && ParticipantMax < ParticipantMin)
			{
				yield return new ValidationResult(
					$"最大參加人數必須大於或等於最小參加人數（{ParticipantMin}）",
					new[] { nameof(ParticipantMax) });
			}
		}
	}

}
