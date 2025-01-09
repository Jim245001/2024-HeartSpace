using System.ComponentModel.DataAnnotations;

namespace HeartSpace.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string Email { get; set; }
    }
}