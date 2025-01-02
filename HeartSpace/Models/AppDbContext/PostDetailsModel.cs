using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Models
{
    public class PostDetailsModel
    {
        [Key] // 主鍵
        public int Id { get; set; }

        [Required(ErrorMessage = "標題是必填項")]
        [StringLength(100, ErrorMessage = "標題最多 100 字")]
        public string Title { get; set; } // 貼文標題

        [Required(ErrorMessage = "內文是必填項")]
        public string Content { get; set; } // 貼文內容

        public string ImagePath { get; set; } // 圖片路徑

        public DateTime CreatedAt { get; set; } = DateTime.Now; // 發文時間

        public string Author { get; set; } // 發文人名稱

        public virtual ICollection<CommentModel> Comments { get; set; } // 留言集合
    }
}
