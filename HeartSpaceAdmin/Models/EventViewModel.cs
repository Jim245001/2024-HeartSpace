namespace HeartSpaceAdmin.Models
{
	public class EventViewModel
	{
		public int Id { get; set; }
		public string EventName { get; set; }
		public int MemberId { get; set; }
		public string OrganizerName { get; set; } // 從 Members 表取得
		public string Category { get; set; } // 從 CategoryId 對應
		public string Description { get; set; }
		public DateTime EventTime { get; set; }
		public string Location { get; set; }
		public bool IsOnline { get; set; }
		public bool Disabled { get; set; }
		public int ParticipantMax { get; set; }
		public int ParticipantMin { get; set; }
		public int ParticipantNow { get; set; }
		public DateOnly DeadLine { get; set; }
		public List<EventCommentViewModel> Comments { get; set; } = new List<EventCommentViewModel>(); //  活動留言
		public List<EventMemberViewModel> Participants { get; set; } // 活動成員
	}

	public class EventCommentViewModel
	{
		public int Id { get; set; }
		public int EventId { get; set; }
		public int MemberId { get; set; }
		public string MemberName { get; set; } // 從 Members 表取得
		public string CommentContent { get; set; }
		public DateTime CommentTime { get; set; }
		public bool Disabled { get; set; }
	}

	public class EventMemberViewModel
	{
		public int Id { get; set; }
		public int EventId { get; set; }
		public int MemberId { get; set; }
		public string MemberName { get; set; } // 從 Members 表取得
		public bool? IsAttend { get; set; }
	}

}
