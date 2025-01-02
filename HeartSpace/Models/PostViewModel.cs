using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
	public class PostViewModel
	{
		public string CoverImage { get; set; }  // 貼文封面圖
		public string AuthorImage { get; set; } // 原PO頭像
		public string AuthorId { get; set; }    // 原PO ID
		public string Description { get; set; } // 貼文描述
		public List<string> Tags { get; set; }  // 標籤列表
		public string Timestamp { get; set; }   // 發佈時間
	}

}