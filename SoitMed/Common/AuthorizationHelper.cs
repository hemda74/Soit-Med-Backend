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
        public static readonly string[] ManagerRoles = { "SalesManager", "SuperAdmin", "MaintenanceManager" };

        /// <summary>
        /// Sales-specific roles
        /// </summary>
        public static class SalesRoles
        {
            public const string Salesman = "Salesman";
            public const string SalesManager = "SalesManager";
            public const string SalesSupport = "SalesSupport";
            public const string SuperAdmin = "SuperAdmin";
        }

        /// <summary>
        /// Sales-specific permissions
        /// </summary>
        public static class SalesPermissions
        {
            // Client permissions
            public const string ViewOwnClients = "ViewOwnClients";
            public const string ViewAllClients = "ViewAllClients";
            public const string ViewClientHistory = "ViewClientHistory";
            public const string CreateClient = "CreateClient";
            public const string EditClient = "EditClient";
            
            // Weekly Plan permissions
            public const string CreateWeeklyPlan = "CreateWeeklyPlan";
            public const string ViewOwnWeeklyPlans = "ViewOwnWeeklyPlans";
            public const string ViewAllWeeklyPlans = "ViewAllWeeklyPlans";
            public const string ReviewWeeklyPlan = "ReviewWeeklyPlan";
            
            // Task Progress permissions
            public const string CreateTaskProgress = "CreateTaskProgress";
            public const string ViewOwnProgress = "ViewOwnProgress";
            public const string ViewAllProgress = "ViewAllProgress";
            
            // Offer permissions
            public const string RequestOffer = "RequestOffer";
            public const string CreateOffer = "CreateOffer";
            public const string ViewOwnOffers = "ViewOwnOffers";
            public const string ViewAllOffers = "ViewAllOffers";
            
            // Deal permissions
            public const string CreateDeal = "CreateDeal";
            public const string ViewOwnDeals = "ViewOwnDeals";
            public const string ViewAllDeals = "ViewAllDeals";
            public const string ApproveManagerDeal = "ApproveManagerDeal";
            public const string ApproveSuperAdminDeal = "ApproveSuperAdminDeal";
        }

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
