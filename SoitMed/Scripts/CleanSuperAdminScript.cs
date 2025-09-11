using Microsoft.AspNetCore.Identity;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models;

namespace SoitMed.Scripts
{
    public class CleanSuperAdminScript
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;

        public CleanSuperAdminScript(UserManager<ApplicationUser> userManager, Context context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> CleanSuperAdminAsync()
        {
            try
            {
                // Find the target user
                var targetUser = await _userManager.FindByEmailAsync("hemdan@hemdan.com");
                if (targetUser == null)
                {
                    return "User hemdan@hemdan.com not found";
                }

                // Remove ALL roles from the target user first
                var currentRoles = await _userManager.GetRolesAsync(targetUser);
                foreach (var role in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(targetUser, role);
                }

                // Remove SuperAdmin role from ALL users
                var allUsers = _userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(UserRoles.SuperAdmin))
                    {
                        await _userManager.RemoveFromRoleAsync(user, UserRoles.SuperAdmin);
                    }
                }

                // Add ONLY SuperAdmin role to the target user
                await _userManager.AddToRoleAsync(targetUser, UserRoles.SuperAdmin);

                // Delete any other SuperAdmin accounts
                var otherSuperAdmins = allUsers.Where(u => u.Email != "hemdan@hemdan.com" && 
                    _userManager.GetRolesAsync(u).Result.Contains(UserRoles.SuperAdmin)).ToList();
                
                foreach (var user in otherSuperAdmins)
                {
                    await _userManager.DeleteAsync(user);
                }

                return $"Successfully cleaned up roles. {targetUser.Email} is now the only SuperAdmin with only SuperAdmin role.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

