using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HeartSpace.Helpers
{
	public static class PasswordHelper
	{
		public static string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashedBytes);
			}
		}

		public static bool VerifyPassword(string inputPassword, string hashedPassword)
		{
			return HashPassword(inputPassword) == hashedPassword;
		}
	}
}