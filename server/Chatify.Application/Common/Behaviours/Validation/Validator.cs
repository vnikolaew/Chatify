using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Chatify.Application.Common.Behaviours.Validation;

public static class Validator
{
    public static List<ValidationResult> Validate<T>(T value)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var validationContext = new ValidationContext(value);

        List<ValidationResult> validationResults = new();
        foreach ( var property in properties )
        {
            var validationAttributes = property
                .GetCustomAttributes()
                .OfType<ValidationAttribute>()
                .ToArray();

            var propertyValue = property.GetValue(value);
            var validations = validationAttributes
                .Select(va => va.GetValidationResult(propertyValue, validationContext)!)
                .ToArray();

            validationResults.AddRange(validations);
        }

        var validationErrors = validationResults
            .Where(r => r != ValidationResult.Success)
            .ToList();

        return validationErrors;
    }
}