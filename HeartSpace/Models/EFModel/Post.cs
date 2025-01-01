namespace HeartSpace.Models.EFModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Post
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        [Required]
        [StringLength(20)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string PostContent { get; set; }

        [Column(TypeName = "image")]
        public byte[] img { get; set; }

        public DateTime PublishTime { get; set; }

        public int CategoryId { get; set; }

        public int? CommentCount { get; set; }
    }
}
