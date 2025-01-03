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
        public int CategoryId { get; set; }
        public int MemberId { get; set; }
        public DateTime PublishTime { get; set; }
    }
}