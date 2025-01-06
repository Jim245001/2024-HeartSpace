using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.DTOs
{
    public class CreatePostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PostContent { get; set; }
        public string PostImg { get; set; } // 圖片的 Base64 編碼
        public int CategoryId { get; set; }
        public int MemberId { get; set; }
        public string CategoryName { get; set; }
        public string Comments { get; set; }


        public DateTime PublishTime { get; set; }

        public List<System.Web.Mvc.SelectListItem> CategoryList { get; set; }

    }
}