using System;
using System.ComponentModel.DataAnnotations;

namespace FCDBApp.CustomValidation
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public RequiredIfAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
            {
                return new ValidationResult($"Property with name {_comparisonProperty} not found.");
            }

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);
            if (comparisonValue != null && comparisonValue.ToString() == "__add_new__")
            {
                if (string.IsNullOrEmpty(value?.ToString()))
                {
                    return new ValidationResult($"The {validationContext.DisplayName} field is required.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
