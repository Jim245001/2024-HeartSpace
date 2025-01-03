using System;
using System.ComponentModel.DataAnnotations;

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
        public string PostContent { get; set; }

        public string ImagePath { get; set; }
        public byte[] Img { get; set; }

        public DateTime PublishTime { get; set; } = DateTime.Now;

        public int MemberId { get; set; }

    }
}
