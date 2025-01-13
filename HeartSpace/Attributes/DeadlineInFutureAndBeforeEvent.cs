using System;
using System.ComponentModel.DataAnnotations;

namespace HeartSpace.Attributes
{
	public class DeadlineInFutureAndBeforeEventAttribute : ValidationAttribute
	{
		private readonly string _comparisonProperty;

		public DeadlineInFutureAndBeforeEventAttribute(string comparisonProperty)
		{
			_comparisonProperty = comparisonProperty;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var deadline = (DateTime?)value;
			var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

			if (property == null)
			{
				return new ValidationResult($"Unknown property: {_comparisonProperty}");
			}

			var eventTime = (DateTime?)property.GetValue(validationContext.ObjectInstance);

			// 驗證報名截止日期是否是未來時間
			if (deadline.HasValue && deadline <= DateTime.Now)
			{
				return new ValidationResult(ErrorMessage ?? "報名截止日期必須是未來時間");
			}

			// 驗證報名截止日期是否早於活動時間
			if (deadline.HasValue && eventTime.HasValue && deadline >= eventTime)
			{
				return new ValidationResult(ErrorMessage ?? "報名截止日期必須早於活動時間");
			}

			return ValidationResult.Success;
		}
	}
}
