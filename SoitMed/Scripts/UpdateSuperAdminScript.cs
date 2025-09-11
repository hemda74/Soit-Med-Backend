using Microsoft.AspNetCore.Identity;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models;

namespace SoitMed.Scripts
{
    public class UpdateSuperAdminScript
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;

        public UpdateSuperAdminScript(UserManager<ApplicationUser> userManager, Context context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> UpdateSuperAdminAsync()
        {
            try
            {
                // Find the target user
                var targetUser = await _userManager.FindByEmailAsync("hemdan@hemdan.com");
                if (targetUser == null)
                {
                    return "User hemdan@hemdan.com not found";
                }

                // Update password from "sharf" to "shraf"
                var token = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
                var passwordResult = await _userManager.ResetPasswordAsync(targetUser, token, "356120Ahmed@shraf");
                
                if (!passwordResult.Succeeded)
                {
                    return $"Failed to update password: {string.Join(", ", passwordResult.Errors.Select(e => e.Description))}";
                }

                // Remove SuperAdmin role from all users
                var allUsers = _userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains(UserRoles.SuperAdmin))
                    {
                        await _userManager.RemoveFromRoleAsync(user, UserRoles.SuperAdmin);
                    }
                }

                // Add SuperAdmin role to the target user
                await _userManager.AddToRoleAsync(targetUser, UserRoles.SuperAdmin);

                // Delete the other SuperAdmin account we created
                var otherSuperAdmin = await _userManager.FindByEmailAsync("superadmin@company.com");
                if (otherSuperAdmin != null)
                {
                    await _userManager.DeleteAsync(otherSuperAdmin);
                }

                return $"Successfully updated {targetUser.Email} to be the only SuperAdmin with password '356120Ahmed@shraf'";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
