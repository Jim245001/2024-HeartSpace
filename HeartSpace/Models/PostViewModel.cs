
using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HeartSpace.Models
{
<<<<<<< HEAD
	public class PostViewModel
	{
		public string PostImg { get; set; }  // 貼文封面圖
		public string MemberImg { get; set; } // 原PO頭像
		public string Description { get; set; } // 貼文描述

		[Key]
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

		public virtual ICollection<PostComment> Comments { get; set; } // 留言集合


		public string ImageUrl { get; set; } // UI 友好的圖片路徑

	}
=======
    public class PostViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string PostContent { get; set; }

        public string PostImg { get; set; } // 貼文封面圖 Base64 格式

        public DateTime PublishTime { get; set; }

        public int MemberId { get; set; }

        public string MemberName { get; set; } // 原PO名稱

        public string MemberImg { get; set; } // 原PO頭像 Base64 格式

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // 類別名稱
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>(); // 留言集合



    }
    public class CommentViewModel
    {
        public int CommentId { get; set; } // 留言 ID



        public string Content { get; set; } // 留言內容

        public string AuthorName { get; set; } // 留言者名稱

        public int AuthorId { get; set; } // 留言 ID

        public string AuthorImg { get; set; } // 留言者頭像 Base64 格式

        public DateTime CreatedAt { get; set; } // 留言時間
    }
>>>>>>> origin/K

}