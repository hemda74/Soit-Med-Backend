using FluentValidation;
using FluentValidation.Results;

namespace SoitMed.Common
{
    /// <summary>
    /// Helper class for common validation operations
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates a DTO and returns validation errors if any
        /// </summary>
        public static async Task<ValidationResult?> ValidateAsync<T>(T dto, IValidator<T> validator, CancellationToken cancellationToken = default)
        {
            var result = await validator.ValidateAsync(dto, cancellationToken);
            return result.IsValid ? null : result;
        }

        /// <summary>
        /// Creates a validation error response
        /// </summary>
        public static object CreateValidationErrorResponse(ValidationResult validationResult)
        {
            return new
            {
                success = false,
                message = "Validation failed. Please check the following fields:",
                errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                timestamp = DateTime.UtcNow
            };
        }
    }
}
