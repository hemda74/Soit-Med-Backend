using Microsoft.AspNetCore.Identity;
using SoitMed.Models.Identity;

namespace SoitMed.Common
{
    /// <summary>
    /// Helper class for common service operations
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// Validates that a user exists and has the correct role
        /// </summary>
        public static async Task<bool> ValidateUserRoleAsync(string userId, string requiredRole, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await userManager.GetRolesAsync(user);
            return userRoles.Contains(requiredRole);
        }

        /// <summary>
        /// Validates that a user exists and has any of the specified roles
        /// </summary>
        public static async Task<bool> ValidateUserRolesAsync(string userId, string[] requiredRoles, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await userManager.GetRolesAsync(user);
            return requiredRoles.Any(role => userRoles.Contains(role));
        }
    }
}
