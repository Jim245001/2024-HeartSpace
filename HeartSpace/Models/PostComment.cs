namespace HeartSpace.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PostComment
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Comment { get; set; }

        public DateTime CommentTime { get; set; }

        public bool? Disabled { get; set; }

        public virtual Member Member { get; set; }

        public virtual Post Post { get; set; }
    }
}
