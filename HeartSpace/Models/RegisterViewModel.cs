using System.ComponentModel.DataAnnotations;

namespace HeartSpace.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        [StringLength(25, ErrorMessage = "帳號長度不得超過 25 個字元")]
        public string Account { get; set; }

        [Required(ErrorMessage = "請輸入密碼")]
        [StringLength(200, ErrorMessage = "密碼長度不得超過 200 個字元")]
        public string Password { get; set; }

        [Required(ErrorMessage = "請再次輸入密碼")]
        [Compare("Password", ErrorMessage = "兩次密碼輸入不一致")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "請輸入電子郵件")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string Email { get; set; }

        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(50, ErrorMessage = "姓名長度不得超過 50 個字元")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入暱稱")]
        [StringLength(25, ErrorMessage = "暱稱長度不得超過 25 個字元")]
        public string NickName { get; set; }

       

    }
}