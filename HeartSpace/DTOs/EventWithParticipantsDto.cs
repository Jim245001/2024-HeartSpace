using HeartSpace.Models.EFModels;
using System;
using System.Collections.Generic;

namespace HeartSpace.DTOs
{
	public class EventWithParticipantsDto
	{
		public int EventId { get; set; } // 活動 ID
		public string EventName { get; set; } // 活動名稱

		// 發起人資訊
		public int MemberId { get; set; } // 發起人 ID
		public string MemberName { get; set; } // 發起人姓名
		public string NickName { get; set; } // 發起人暱稱
		public string Email { get; set; } // 發起人信箱
		public string MemberImg { get; set; } // 發起人頭像

		// 報名者清單
		public List<ParticipantDto> Participants { get; set; } // 參與者清單

		// 活動資訊
		public string CategoryName { get; set; } // 活動類別名稱
		public DateTime EventTime { get; set; } // 活動時間
		public bool IsOnline { get; set; } // 是否線上
		public string Location { get; set; } // 活動地點
		public int ParticipantMin { get; set; } // 最小參與人數
		public int ParticipantMax { get; set; } // 最大參與人數
		public string Description { get; set; } // 活動描述
		public DateTime DeadLine { get; set; } // 報名截止日期
		public int ParticipantNow { get; set; } // 目前報名人數
	}


	public class ParticipantDto
	{
		public int EventMemberId { get; set; } // 報名記錄 ID
		public int EventId { get; set; } // 活動 ID
		public int MemberId { get; set; } // 參與者 ID
		public string MemberName { get; set; } // 姓名
		public string NickName { get; set; } // 暱稱
		public string Email { get; set; } // 信箱
		public string MemberImg { get; set; } // 頭像
		public bool? IsAttend { get; set; } // 出席狀態
	}
}
