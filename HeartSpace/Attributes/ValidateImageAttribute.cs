using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Attributes
{
	public class ValidateImageAttribute : ValidationAttribute
	{
		private readonly int _maxFileSize; // 最大檔案大小（以字節為單位）
		private readonly string[] _allowedFileTypes; // 允許的檔案類型

		public ValidateImageAttribute(int maxFileSize, params string[] allowedFileTypes)
		{
			_maxFileSize = maxFileSize;
			_allowedFileTypes = allowedFileTypes;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var file = value as HttpPostedFileBase;

			// 如果沒有上傳檔案，允許通過（根據需求可以改為不允許）
			if (file == null)
			{
				return ValidationResult.Success;
			}

			// 驗證檔案類型
			if (!_allowedFileTypes.Contains(file.ContentType))
			{
				return new ValidationResult($"只允許上傳 {string.Join(", ", _allowedFileTypes)} 格式的圖片。");
			}

			// 驗證檔案大小
			if (file.ContentLength > _maxFileSize)
			{
				return new ValidationResult($"圖片大小不能超過 {_maxFileSize / (1024 * 1024)} MB。");
			}

			return ValidationResult.Success;
		}
	}
}