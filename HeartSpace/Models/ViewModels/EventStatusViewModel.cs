using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.ViewModels
{
	public class EventStatusViewModel
	{
		public int Id { get; set; }
		public string EventName { get; set; } // 活動名稱
		public ParticipantViewModel EventOwner { get; set; } // 活動發起人
		public List<ParticipantViewModel> Participants { get; set; } // 報名的參與者

		public string CategoryName { get; set; } // 類別
		public DateTime EventTime { get; set; } // 活動時間
		public bool IsOnline { get; set; } // 是否線上
		public string Location { get; set; } // 活動地點
		public int ParticipantMin { get; set; } // 最小參與人數
		public int ParticipantMax { get; set; } // 最大參與人數
		public string Description { get; set; } // 活動描述
		public DateTime DeadLine { get; set; } // 報名截止日期
		public int ParticipantNow { get; set; } // 目前報名人數
	}

	public class ParticipantViewModel
	{
		public int MemberId { get; set; } // 成員ID
		public string NickName { get; set; } // 暱稱
		public string FullName { get; set; } // 姓名
		public string Email { get; set; } // 信箱
		public string ProfileImage { get; set; } // 頭像
		public bool? IsAttend { get; set; } // 出席狀態 (true: 已出席, false: 未出席, null: 未選擇)
	}

	public class AttendanceUpdateViewModel
	{
		public int MemberId { get; set; }
		public int EventId { get; set; }
		public bool? IsAttend { get; set; }
	}
}

