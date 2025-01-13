using System.Collections.Generic;
using System.Web.Mvc;

namespace HeartSpaceImage.Models
{
    public class ImagesViewModel 
    {
        public List<string> MemberImages { get; set; }
        public List<string> EventImages { get; set; }
        public List<string> PostImages { get; set; }
    }
}