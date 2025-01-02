namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(25)]
        public string EventName { get; set; }

        public int MemberId { get; set; }

        [Column(TypeName = "image")]
        public byte[] img { get; set; }

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

		//public virtual Category Categories { get; set; }
	}
}
