using HeartSpace.Models.EFModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class CommentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; } // 所屬貼文 ID

        [Required(ErrorMessage = "留言內容是必填項")]
        public string Content { get; set; } // 留言內容

        public string AuthorName { get; set; } // 留言作者名稱

        public DateTime CreatedAt { get; set; } = DateTime.Now; // 留言時間

        public virtual PostModel Post { get; set; } // 所屬貼文
    }
}
