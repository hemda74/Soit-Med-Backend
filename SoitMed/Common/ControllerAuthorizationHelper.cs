using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Models.Identity;

namespace SoitMed.Common
{
    /// <summary>
    /// Helper class for controller authorization operations
    /// </summary>
    public static class ControllerAuthorizationHelper
    {
        /// <summary>
        /// Gets the current user and validates authentication
        /// </summary>
        public static async Task<(ApplicationUser? user, IActionResult? error)> GetCurrentUserAsync(
            string? userId, 
            UserManager<ApplicationUser> userManager)
        {
            if (string.IsNullOrEmpty(userId))
                return (null, new UnauthorizedObjectResult(ResponseHelper.CreateUnauthorizedResponse()));

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return (null, new UnauthorizedObjectResult(ResponseHelper.CreateUnauthorizedResponse()));

            return (user, null);
        }

        /// <summary>
        /// Checks if user has manager-level access
        /// </summary>
        public static async Task<bool> IsManagerAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            return await AuthorizationHelper.IsManagerAsync(user, userManager);
        }

        /// <summary>
        /// Checks if user can access a resource
        /// </summary>
        public static async Task<bool> CanAccessResourceAsync(string userId, string resourceOwnerId, UserManager<ApplicationUser> userManager)
        {
            return await AuthorizationHelper.CanAccessResourceAsync(userId, resourceOwnerId, userManager);
        }
    }
}
