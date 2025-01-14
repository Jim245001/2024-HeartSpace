using System.ComponentModel.DataAnnotations;

namespace HeartSpaceAdmin.Models
{
	public class MemberViewModel
	{
		public int Id { get; set; }

		[Required]
		public string Account { get; set; }
		[Required]
		public string Name { get; set; }
		[Required]
		public string NickName { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		public bool Disabled { get; set; }
		[Required]
		public string Role { get; set; }

		public int? AbsenceCount { get; set; }

		public string? MemberImg { get; set; }
		public IFormFile MemberImgFile { get; set; } // 用於上傳新圖片
		[Required]
		public string PasswordHash { get; set; }

		public string? ConfirmCode { get; set; } 

		public bool? IsConfirmed { get; set; }
	}
}
