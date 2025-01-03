using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HeartSpace.Models
{
	public class LoginViewModel
	{


		[Required(ErrorMessage = "請輸入帳號")]
		[StringLength(25, ErrorMessage = "帳號長度不得超過 25 個字元")]
		public string Account { get; set; }

		[Required(ErrorMessage = "請輸入密碼")]
		[StringLength(200, ErrorMessage = "密碼長度不得超過 200 個字元")]
		public string Password { get; set; }
	}
}