using HeartSpace.Helpers;
using HeartSpace.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.ViewModels
{
    public class SearchPostViewModel
    {
        public IEnumerable<CreatePostDto> CreatePostDtos { get; set; } // 搜尋結果
        public IEnumerable<PostCard> PostCards { get; set; } // 隨機顯示的 PostCard
        public PaginatedList<PostCard> Posts { get; set; }
    }
}