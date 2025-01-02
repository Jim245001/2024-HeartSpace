using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace HeartSpace.Models.ViewModels
{
	public class EventViewModel
	{
		public int Id { get; set; }

		[Required]
		[StringLength(25, ErrorMessage = "活動名稱最多 25 字元")]
		[Display(Name = "活動名稱")]
		public string EventName { get; set; }

		[Display(Name = "會員 ID")]
		public int MemberId { get; set; }

		[Display(Name = "照片")]
		public HttpPostedFileBase Img { get; set; } // 用於接收上傳的檔案

		[Display(Name = "分類 ID")]
		public int CategoryId { get; set; }

		[Required]
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

		[Display(Name = "最大參加人數")]
		public int? ParticipantMax { get; set; }

		[Display(Name = "最小參加人數")]
		public int ParticipantMin { get; set; }

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
	}
}
