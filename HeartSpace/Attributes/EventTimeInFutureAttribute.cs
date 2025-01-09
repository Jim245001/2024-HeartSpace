using System;
using System.ComponentModel.DataAnnotations;

namespace HeartSpace.Attributes
{
	public class EventTimeInFutureAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var eventTime = (DateTime?)value;

			if (eventTime.HasValue && eventTime <= DateTime.Now)
			{
				return new ValidationResult(ErrorMessage ?? "活動時間必須是未來時間");
			}

			return ValidationResult.Success;
		}
	}
}
