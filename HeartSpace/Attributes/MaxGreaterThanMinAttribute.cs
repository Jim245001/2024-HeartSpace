using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HeartSpace.Attributes
{	
	//揪團的最大人數必須大於或等於最小人數
	public class MaxGreaterThanMinAttribute : ValidationAttribute
	{
		private readonly string _comparisonProperty;

		public MaxGreaterThanMinAttribute(string comparisonProperty)
		{
			_comparisonProperty = comparisonProperty;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			// 取得當前屬性的值
			var currentValue = (int?)value;

			// 取得需要比較的屬性值
			var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
			if (property == null)
			{
				return new ValidationResult($"Unknown property: {_comparisonProperty}");
			}

			var comparisonValue = (int?)property.GetValue(validationContext.ObjectInstance);

			// 驗證邏輯：如果最大值小於最小值，返回錯誤
			if (currentValue.HasValue && comparisonValue.HasValue && currentValue < comparisonValue)
			{
				return new ValidationResult(ErrorMessage ?? $"最大人數必須大於或等於最小人數");
			}

			return ValidationResult.Success;
		}
	}
}