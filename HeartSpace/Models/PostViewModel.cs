
using HeartSpace.Models.EFModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HeartSpace.Models
{
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

        public int PostId { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImg { get; set; }
        public string Comment { get; set; }
        public DateTime CommentTime { get; set; }
        public int FloorNumber { get; set; } // 新增：樓層編號
    }

}