using HeartSpace.Models.EFModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

public partial class Event
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
	public Event()
	{
		EventComments = new HashSet<EventComment>();
		EventMembers = new HashSet<EventMember>();
	}

	public int Id { get; set; }

	[Required]
	[StringLength(25)]
	public string EventName { get; set; }

	public int MemberId { get; set; }

	[Column(TypeName = "image")]
	public byte[] EventImg { get; set; }

	public int CategoryId { get; set; }

	[Required]
	[StringLength(500)]
	public string Description { get; set; }

	public DateTime EventTime { get; set; }

	[StringLength(500)]
	public string Location { get; set; }

	public bool IsOnline { get; set; }

	public int? ParticipantMax { get; set; }

	public int ParticipantMin { get; set; }

	[StringLength(50)]
	public string Limit { get; set; }

	[Column(TypeName = "date")]
	public DateTime DeadLine { get; set; }

	public int? CommentCount { get; set; }

	public int? ParticipantNow { get; set; }

	public bool Disabled { get; set; }

	public virtual Category Category { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
	public virtual ICollection<EventComment> EventComments { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
	public virtual ICollection<EventMember> EventMembers { get; set; }

	public virtual Member Member { get; set; } // 新增導航屬性
}
