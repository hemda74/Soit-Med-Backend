using Microsoft.AspNetCore.Identity;
using SoitMed.Models.Identity;

namespace SoitMed.Common
{
    /// <summary>
    /// Helper class for common authorization logic
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Manager roles that can access all resources
        /// </summary>
        public static readonly string[] ManagerRoles = { "SalesManager", "SuperAdmin" };

        /// <summary>
        /// Checks if a user has manager-level access
        /// </summary>
        public static bool IsManager(IList<string> userRoles)
        {
            return ManagerRoles.Any(role => userRoles.Contains(role));
        }

        /// <summary>
        /// Checks if a user has manager-level access asynchronously
        /// </summary>
        public static async Task<bool> IsManagerAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            return IsManager(userRoles);
        }

        /// <summary>
        /// Checks if a user can access a resource based on ownership or manager role
        /// </summary>
        public static bool CanAccessResource(string userId, string resourceOwnerId, IList<string> userRoles)
        {
            return IsManager(userRoles) || userId == resourceOwnerId;
        }

        /// <summary>
        /// Checks if a user can access a resource asynchronously
        /// </summary>
        public static async Task<bool> CanAccessResourceAsync(string userId, string resourceOwnerId, UserManager<ApplicationUser> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var userRoles = await userManager.GetRolesAsync(user);
            return CanAccessResource(userId, resourceOwnerId, userRoles);
        }
    }
}
