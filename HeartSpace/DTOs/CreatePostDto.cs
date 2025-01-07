using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.DTOs
{
    public class CreatePostDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = " 請輸入標題！")]
        [Display(Name = "分類")]
        public string Title { get; set; }

        [Required(ErrorMessage = " 請輸入內文！")]
        [Display(Name = "內文")]
        public string PostContent { get; set; }
        public string PostImg { get; set; } // 圖片的 Base64 編碼
        public bool Disabled { get; set; }


        [Required(ErrorMessage = " 請選擇分類！")]
        [Display(Name = "分類")]
        public int CategoryId { get; set; }
        public int MemberId { get; set; }
        public string CategoryName { get; set; }
        public string Comments { get; set; }


        public DateTime PublishTime { get; set; }

        public List<System.Web.Mvc.SelectListItem> CategoryList { get; set; }

    }
}