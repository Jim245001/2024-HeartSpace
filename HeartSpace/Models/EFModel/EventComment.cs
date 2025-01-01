namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EventComment
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int MemberId { get; set; }

        [Required]
        [StringLength(500)]
        public string EventCommentContent { get; set; }

        public DateTime CommentTime { get; set; }
    }
}
