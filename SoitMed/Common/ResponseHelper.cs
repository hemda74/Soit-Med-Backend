namespace SoitMed.Common
{
    /// <summary>
    /// Helper class for creating standardized API responses
    /// </summary>
    public static class ResponseHelper
    {
        /// <summary>
        /// Creates a standardized success response
        /// </summary>
        public static object CreateSuccessResponse(object? data = null, string message = "Operation completed successfully")
        {
            return new
            {
                success = true,
                data,
                message,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        public static object CreateErrorResponse(string message, object? errors = null)
        {
            return new
            {
                success = false,
                message,
                errors,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a validation error response
        /// </summary>
        public static object CreateValidationErrorResponse(Dictionary<string, string[]> errors)
        {
            return new
            {
                success = false,
                message = "Validation failed. Please check the following fields:",
                errors,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a not found response
        /// </summary>
        public static object CreateNotFoundResponse(string message = "Resource not found")
        {
            return new
            {
                success = false,
                message,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a conflict response
        /// </summary>
        public static object CreateConflictResponse(string message = "Resource already exists")
        {
            return new
            {
                success = false,
                message,
                timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates an unauthorized response
        /// </summary>
        public static object CreateUnauthorizedResponse(string message = "Unauthorized access")
        {
            return new
            {
                success = false,
                message,
                timestamp = DateTime.UtcNow
            };
        }
    }
}
