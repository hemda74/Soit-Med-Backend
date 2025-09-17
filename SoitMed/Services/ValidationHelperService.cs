using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;

namespace SoitMed.Services
{
    public class ValidationHelperService
    {
        /// <summary>
        /// Formats ModelState validation errors into a user-friendly response
        /// </summary>
        /// <param name="modelState">The ModelState containing validation errors</param>
        /// <returns>Formatted error response with field-specific messages</returns>
        public static object FormatValidationErrors(ModelStateDictionary modelState)
        {
            var errors = new Dictionary<string, List<string>>();
            var generalErrors = new List<string>();

            foreach (var key in modelState.Keys)
            {
                var fieldErrors = modelState[key]?.Errors
                    .Where(e => !string.IsNullOrEmpty(e.ErrorMessage))
                    .Select(e => e.ErrorMessage)
                    .ToList() ?? new List<string>();

                if (fieldErrors.Any())
                {
                    if (string.IsNullOrEmpty(key) || key == "model")
                    {
                        generalErrors.AddRange(fieldErrors);
                    }
                    else
                    {
                        errors[key] = fieldErrors;
                    }
                }
            }

            var response = new
            {
                success = false,
                message = "Validation failed. Please check the following fields:",
                errors = errors,
                generalErrors = generalErrors,
                timestamp = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Creates a standardized error response for business logic validation failures
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="field">The field that caused the error (optional)</param>
        /// <param name="code">Error code (optional)</param>
        /// <returns>Formatted error response</returns>
        public static object CreateBusinessLogicError(string message, string? field = null, string? code = null)
        {
            var response = new
            {
                success = false,
                message = message,
                field = field,
                code = code,
                timestamp = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Creates a standardized error response for multiple business logic validation failures
        /// </summary>
        /// <param name="errors">Dictionary of field names and their error messages</param>
        /// <param name="generalMessage">General error message</param>
        /// <returns>Formatted error response</returns>
        public static object CreateMultipleBusinessLogicErrors(Dictionary<string, string> errors, string generalMessage = "Validation failed. Please check the following issues:")
        {
            var response = new
            {
                success = false,
                message = generalMessage,
                errors = errors,
                timestamp = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Validates email format and provides specific error message
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email is required";

            if (email.Length > 256)
                return "Email cannot exceed 256 characters";

            if (!email.Contains("@") || !email.Contains("."))
                return "Please provide a valid email address format";

            return null;
        }

        /// <summary>
        /// Validates password strength and provides specific error message
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required";

            if (password.Length < 6)
                return "Password must be at least 6 characters long";

            if (password.Length > 100)
                return "Password cannot exceed 100 characters";

            return null;
        }

        /// <summary>
        /// Validates that a list of governorate IDs is not empty
        /// </summary>
        /// <param name="governorateIds">List of governorate IDs</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidateGovernorateIds(List<int> governorateIds)
        {
            if (governorateIds == null || !governorateIds.Any())
                return "At least one governorate must be assigned";

            if (governorateIds.Any(id => id <= 0))
                return "All governorate IDs must be positive numbers";

            return null;
        }

        /// <summary>
        /// Validates hospital ID format
        /// </summary>
        /// <param name="hospitalId">Hospital ID to validate</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string? ValidateHospitalId(string hospitalId)
        {
            if (string.IsNullOrWhiteSpace(hospitalId))
                return "Hospital ID is required";

            if (hospitalId.Length > 50)
                return "Hospital ID cannot exceed 50 characters";

            return null;
        }
    }
}
