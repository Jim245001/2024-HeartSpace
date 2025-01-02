using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class PostDetailsModel
    {
        [ [Key]
        public int Id { get; set; }

        public int MemberId { get; set; }

        [Required(ErrorMessage = "標題是必填項")]
        [StringLength(100, ErrorMessage = "標題最多 100 字")]
        public string Title { get; set; }

        [Required(ErrorMessage = "內文是必填項")]
        public string PostContent { get; set; }

        public string img { get; set; }

        public DateTime PublishTime { get; set; } = DateTime.Now;

        public int CategoryId { get; set; }

        public virtual ICollection<CommentModel> Comments { get; set; } // 留言集合
    }
}
