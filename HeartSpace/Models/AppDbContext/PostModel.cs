using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Models.EFModel
{
    public class PostModel
    {
        [Key] 
        public int Id { get; set; }

        [Required(ErrorMessage = "標題是必填項")]
        [StringLength(100, ErrorMessage = "標題最多 100 字")]
        public string Title { get; set; } 

        [Required(ErrorMessage = "內文是必填項")]
        public string Content { get; set; } 

        public string ImagePath { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public string Author { get; set; } 

        public List<string> Tags { get; set; } 
    }
}