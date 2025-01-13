using System;
using System.Security.Cryptography;
using System.Text;

namespace HeartSpace.Utilities
{
	public static class HashUtility
	{
		public static string GetSalt()
		{
			return System.Configuration.ConfigurationManager.AppSettings["Salt"];
		}

        public static string ToSHA256(string plainText, string salt)
		{
			// ref https://docs.microsoft.com/zh-tw/dotnet/api/system.security.cryptography.sha256?view=net-6.0
			using (var mySHA256 = SHA256.Create())
			{
				var passwordBytes = Encoding.UTF8.GetBytes(salt + plainText);
				var hash = mySHA256.ComputeHash(passwordBytes);
				var sb = new StringBuilder();
				foreach (var b in hash) sb.Append(b.ToString("X2"));

				return sb.ToString();
			}
		}

		public static bool VarifySHA256(string value, string hash, string salt)
		{
			var hashOfInput = ToSHA256(value, salt);
			return hash == hashOfInput;
		}
	}

}
