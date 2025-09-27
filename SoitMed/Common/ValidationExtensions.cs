using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SoitMed.Common
{
    /// <summary>
    /// Extension methods for validation operations
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates a DTO and returns a BadRequest result if validation fails
        /// </summary>
        public static async Task<IActionResult?> ValidateAsync<T>(this T dto, IValidator<T> validator, CancellationToken cancellationToken = default)
        {
            var result = await validator.ValidateAsync(dto, cancellationToken);
            if (!result.IsValid)
            {
                var errors = result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                
                return new BadRequestObjectResult(ResponseHelper.CreateValidationErrorResponse(errors));
            }
            return null;
        }
    }
}
