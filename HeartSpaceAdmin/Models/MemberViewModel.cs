using System.ComponentModel.DataAnnotations;

namespace HeartSpaceAdmin.Models
{
	public class MemberViewModel
	{
		public int Id { get; set; }

		[Required]
		public string Account { get; set; }

		public string Name { get; set; }

		public string NickName { get; set; }

		[Required]
		public string Email { get; set; }

		public bool Disabled { get; set; }

		public string Role { get; set; }

		public int? AbsenceCount { get; set; }

		public string MemberImg { get; set; }

		public string PasswordHash { get; set; }

		public string? ConfirmCode { get; set; } // 允許為 null

		public bool? IsConfirmed { get; set; }
	}
}
