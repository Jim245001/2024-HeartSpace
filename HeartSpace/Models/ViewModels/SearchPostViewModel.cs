using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeartSpace.Models
{
    public class SearchPostViewModel
    {
        public List<PostCard> Posts { get; set; }
        public List<PostCard> RecommendedPosts { get; set; }
    }
}