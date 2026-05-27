using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace back_cabs.CRM.Core.Validation
{
    public class EnumStringValueAttribute : ValidationAttribute
    {
        private readonly Type _enumType;

        public EnumStringValueAttribute(Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentException("El tipo proporcionado debe ser un enum.", nameof(enumType));
            }
            _enumType = enumType;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; // Dejar que [Required] se encargue de esto.
            }

            var stringValue = value.ToString();
            var enumValues = Enum.GetNames(_enumType);

            if (enumValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }

            var validValues = string.Join(", ", enumValues);
            return new ValidationResult($"El valor '{stringValue}' no es válido. Valores permitidos: {validValues}.");
        }
    }
}
