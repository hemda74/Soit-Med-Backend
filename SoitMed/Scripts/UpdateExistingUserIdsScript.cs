using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Services;

namespace SoitMed.Scripts
{
    public class UpdateExistingUserIdsScript
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;
        private readonly UserIdGenerationService _userIdGenerationService;

        public UpdateExistingUserIdsScript(UserManager<ApplicationUser> userManager, Context context, UserIdGenerationService userIdGenerationService)
        {
            _userManager = userManager;
            _context = context;
            _userIdGenerationService = userIdGenerationService;
        }

        public async Task<string> UpdateAllUserIdsAsync()
        {
            try
            {
                var results = new List<string>();
                results.Add("=== UPDATING EXISTING USER IDs TO NEW FORMAT ===");

                // Get all users except SuperAdmin
                var superAdminEmail = "hemdan@hemdan.com";
                var allUsers = await _context.Users
                    .Where(u => u.Email != superAdminEmail)
                    .ToListAsync();

                results.Add($"Found {allUsers.Count} users to update (excluding SuperAdmin)");

                foreach (var user in allUsers)
                {
                    try
                    {
                        // Get user roles
                        var userRoles = await _userManager.GetRolesAsync(user);
                        var role = userRoles.FirstOrDefault() ?? "User";

                        // Get hospital ID if user is a doctor or technician
                        string? hospitalId = null;
                        if (role == "Doctor")
                        {
                            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                            hospitalId = doctor?.HospitalId;
                        }
                        else if (role == "Technician")
                        {
                            var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == user.Id);
                            hospitalId = technician?.HospitalId;
                        }

                        // Generate new ID
                        string newId = await _userIdGenerationService.GenerateUserIdAsync(
                            user.FirstName ?? "Unknown",
                            user.LastName ?? "User",
                            role,
                            user.DepartmentId,
                            hospitalId
                        );

                        string oldId = user.Id;
                        user.Id = newId;

                        // Update user in database
                        await _userManager.UpdateAsync(user);

                        // Update related entities
                        if (role == "Doctor")
                        {
                            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == oldId);
                            if (doctor != null)
                            {
                                doctor.UserId = newId;
                                _context.Doctors.Update(doctor);
                            }
                        }
                        else if (role == "Engineer")
                        {
                            var engineer = await _context.Engineers.FirstOrDefaultAsync(e => e.UserId == oldId);
                            if (engineer != null)
                            {
                                engineer.UserId = newId;
                                _context.Engineers.Update(engineer);
                            }
                        }
                        else if (role == "Technician")
                        {
                            var technician = await _context.Technicians.FirstOrDefaultAsync(t => t.UserId == oldId);
                            if (technician != null)
                            {
                                technician.UserId = newId;
                                _context.Technicians.Update(technician);
                            }
                        }

                        await _context.SaveChangesAsync();

                        results.Add($"Updated: {user.Email} - Old ID: {oldId} -> New ID: {newId}");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"Error updating user {user.Email}: {ex.Message}");
                    }
                }

                results.Add("\n=== USER ID UPDATE COMPLETED ===");
                return string.Join("\n", results);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

