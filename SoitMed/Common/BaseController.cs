using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Models.Identity;
using FluentValidation;

namespace SoitMed.Common
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers
    /// </summary>
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly UserManager<ApplicationUser> UserManager;

        protected BaseController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        /// <summary>
        /// Gets the current user ID from the JWT token
        /// </summary>
        protected string? GetCurrentUserId()
        {
            return UserManager.GetUserId(User);
        }

        /// <summary>
        /// Gets the current user role from the JWT token
        /// </summary>
        protected async Task<string> GetCurrentUserRoleAsync()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return string.Empty;

            var roles = await UserManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Gets the current user role synchronously from claims
        /// </summary>
        protected string GetCurrentUserRole()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            return role?.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the current user asynchronously
        /// </summary>
        protected async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return null;

            return await UserManager.FindByIdAsync(userId);
        }

        /// <summary>
        /// Checks if the current user has any of the specified roles
        /// </summary>
        protected async Task<bool> HasAnyRoleAsync(params string[] roles)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
                return false;

            var userRoles = await UserManager.GetRolesAsync(user);
            return roles.Any(role => userRoles.Contains(role));
        }

        /// <summary>
        /// Validates a DTO using FluentValidation
        /// </summary>
        protected async Task<IActionResult?> ValidateDtoAsync<T>(T dto, IValidator<T> validator, CancellationToken cancellationToken = default)
        {
            return await dto.ValidateAsync(validator, cancellationToken);
        }

        /// <summary>
        /// Creates a standardized success response
        /// </summary>
        protected IActionResult SuccessResponse(object? data = null, string message = "Operation completed successfully")
        {
            return Ok(CreateSuccessResponse(data, message));
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        protected IActionResult ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            var response = CreateErrorResponse(message, errors);
            return statusCode switch
            {
                400 => BadRequest(response),
                401 => Unauthorized(response),
                403 => Forbid(),
                404 => NotFound(response),
                409 => Conflict(response),
                _ => StatusCode(statusCode, response)
            };
        }

        /// <summary>
        /// Creates a standardized success response object
        /// </summary>
        protected static object CreateSuccessResponse(object? data = null, string message = "Operation completed successfully")
        {
            return ResponseHelper.CreateSuccessResponse(data, message);
        }

        /// <summary>
        /// Creates a standardized error response object
        /// </summary>
        protected static object CreateErrorResponse(string message, object? errors = null)
        {
            return ResponseHelper.CreateErrorResponse(message, errors);
        }

        /// <summary>
        /// Ensures the current user is authenticated
        /// </summary>
        protected IActionResult? EnsureAuthenticated()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            return null;
        }
    }
}
