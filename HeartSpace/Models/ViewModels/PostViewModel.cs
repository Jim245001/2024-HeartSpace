using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HeartSpace.Models.ViewModels
{
	public class PostViewModels
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "標題為必填項")]
		[StringLength(20, ErrorMessage = "標題最多 20 字")]
		public string Title { get; set; }

		[Required(ErrorMessage = "內文為必填項")]
		[StringLength(500, ErrorMessage = "內文最多 500 字")]
		public string PostContent { get; set; }

		public string PostImg { get; set; } // 貼文封面圖 Base64 格式

		public DateTime PublishTime { get; set; }
		public bool Disabled { get; set; }
		public int MemberId { get; set; }
		public string MemberNickName { get; set; } // 原PO暱稱

		public byte[] MemberImg { get; set; } // 原PO頭像 Base64 格式

		[Required(ErrorMessage = "請選擇分類")]
		[Display(Name = "分類")]
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public IEnumerable<SelectListItem> CategoryList { get; set; }

		public List<CommentViewModel> Comments { get; set; } // 留言集合
	}
}
