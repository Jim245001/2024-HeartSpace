using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
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

}