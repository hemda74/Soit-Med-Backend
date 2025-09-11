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
    public class CleanAndCreateTestUsersScript
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;
        private readonly UserIdGenerationService _userIdGenerationService;

        public CleanAndCreateTestUsersScript(UserManager<ApplicationUser> userManager, Context context, UserIdGenerationService userIdGenerationService)
        {
            _userManager = userManager;
            _context = context;
            _userIdGenerationService = userIdGenerationService;
        }

        public async Task<string> CleanAndCreateTestUsersAsync()
        {
            try
            {
                var results = new List<string>();

                // Step 1: Clean up old users (except SuperAdmin)
                results.Add("=== CLEANING UP OLD USERS ===");
                
                var superAdminEmail = "hemdan@hemdan.com";
                var allUsers = _userManager.Users.ToList();
                var usersToDelete = allUsers.Where(u => u.Email != superAdminEmail).ToList();
                
                foreach (var user in usersToDelete)
                {
                    // Remove from roles first
                    var roles = await _userManager.GetRolesAsync(user);
                    foreach (var role in roles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, role);
                    }
                    
                    // Delete user
                    await _userManager.DeleteAsync(user);
                    results.Add($"Deleted user: {user.Email}");
                }

                // Step 2: Create test users for each role
                results.Add("\n=== CREATING TEST USERS ===");

                // Get departments and hospitals
                var medicalDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Medical");
                var engineeringDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
                var adminDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Administration");
                var financeDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Finance");
                var legalDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Legal");
                var salesDept = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Sales");

                var hospitals = await _context.Hospitals.Take(3).ToListAsync();
                var governorates = await _context.Governorates.Take(5).ToListAsync();

                // Create Doctors (3-5 users)
                results.Add("\n--- Creating Doctors ---");
                for (int i = 1; i <= 4; i++)
                {
                    var hospital = hospitals[(i - 1) % hospitals.Count];
                    var doctor = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Doctor{i}", "Test", UserRoles.Doctor, medicalDept?.Id, hospital.HospitalId),
                        UserName = $"doctor{i}@hospital.com",
                        Email = $"doctor{i}@hospital.com",
                        FirstName = $"Doctor{i}",
                        LastName = "Test",
                        DepartmentId = medicalDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(doctor, "Doctor123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(doctor, UserRoles.Doctor);
                        
                        // Create Doctor entity
                        var doctorEntity = new Doctor
                        {
                            Name = $"Doctor{i} Test",
                            Specialty = i == 1 ? "Cardiology" : i == 2 ? "Neurology" : i == 3 ? "Orthopedics" : "General Medicine",
                            HospitalId = hospital.HospitalId,
                            UserId = doctor.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        _context.Doctors.Add(doctorEntity);
                        await _context.SaveChangesAsync();
                        
                        results.Add($"Created Doctor: {doctor.Id} - {doctor.Email}");
                    }
                }

                // Create Engineers (3-5 users)
                results.Add("\n--- Creating Engineers ---");
                for (int i = 1; i <= 4; i++)
                {
                    var engineer = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Engineer{i}", "Test", UserRoles.Engineer, engineeringDept?.Id, null),
                        UserName = $"engineer{i}@company.com",
                        Email = $"engineer{i}@company.com",
                        FirstName = $"Engineer{i}",
                        LastName = "Test",
                        DepartmentId = engineeringDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(engineer, "Engineer123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(engineer, UserRoles.Engineer);
                        
                        // Create Engineer entity
                        var engineerEntity = new Engineer
                        {
                            Name = $"Engineer{i} Test",
                            Specialty = i == 1 ? "Biomedical" : i == 2 ? "Software" : i == 3 ? "Hardware" : "Systems",
                            UserId = engineer.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        _context.Engineers.Add(engineerEntity);
                        await _context.SaveChangesAsync();
                        
                        // Assign governorates
                        var assignedGovs = governorates.Take(i).ToList();
                        foreach (var gov in assignedGovs)
                        {
                            var engineerGov = new EngineerGovernorate
                            {
                                EngineerId = engineerEntity.EngineerId,
                                GovernorateId = gov.GovernorateId,
                                AssignedAt = DateTime.UtcNow,
                                IsActive = true
                            };
                            _context.EngineerGovernorates.Add(engineerGov);
                        }
                        await _context.SaveChangesAsync();
                        
                        results.Add($"Created Engineer: {engineer.Id} - {engineer.Email}");
                    }
                }

                // Create Technicians (3-5 users)
                results.Add("\n--- Creating Technicians ---");
                for (int i = 1; i <= 4; i++)
                {
                    var hospital = hospitals[(i - 1) % hospitals.Count];
                    var technician = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Technician{i}", "Test", UserRoles.Technician, medicalDept?.Id, hospital.HospitalId),
                        UserName = $"technician{i}@hospital.com",
                        Email = $"technician{i}@hospital.com",
                        FirstName = $"Technician{i}",
                        LastName = "Test",
                        DepartmentId = medicalDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(technician, "Tech123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(technician, UserRoles.Technician);
                        
                        // Create Technician entity
                        var technicianEntity = new Technician
                        {
                            Name = $"Technician{i} Test",
                            Department = i == 1 ? "Radiology" : i == 2 ? "Laboratory" : i == 3 ? "Cardiology" : "Emergency",
                            HospitalId = hospital.HospitalId,
                            UserId = technician.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        _context.Technicians.Add(technicianEntity);
                        await _context.SaveChangesAsync();
                        
                        results.Add($"Created Technician: {technician.Id} - {technician.Email}");
                    }
                }

                // Create Admins (3-5 users)
                results.Add("\n--- Creating Admins ---");
                for (int i = 1; i <= 3; i++)
                {
                    var admin = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Admin{i}", "Test", UserRoles.Admin, adminDept?.Id, null),
                        UserName = $"admin{i}@company.com",
                        Email = $"admin{i}@company.com",
                        FirstName = $"Admin{i}",
                        LastName = "Test",
                        DepartmentId = adminDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(admin, "Admin123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(admin, UserRoles.Admin);
                        results.Add($"Created Admin: {admin.Id} - {admin.Email}");
                    }
                }

                // Create Finance Managers (3-5 users)
                results.Add("\n--- Creating Finance Managers ---");
                for (int i = 1; i <= 3; i++)
                {
                    var finance = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Finance{i}", "Test", UserRoles.FinanceManager, financeDept?.Id, null),
                        UserName = $"finance{i}@company.com",
                        Email = $"finance{i}@company.com",
                        FirstName = $"Finance{i}",
                        LastName = "Test",
                        DepartmentId = financeDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(finance, "Finance123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(finance, UserRoles.FinanceManager);
                        results.Add($"Created Finance Manager: {finance.Id} - {finance.Email}");
                    }
                }

                // Create Legal Managers (3-5 users)
                results.Add("\n--- Creating Legal Managers ---");
                for (int i = 1; i <= 3; i++)
                {
                    var legal = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Legal{i}", "Test", UserRoles.LegalManager, legalDept?.Id, null),
                        UserName = $"legal{i}@company.com",
                        Email = $"legal{i}@company.com",
                        FirstName = $"Legal{i}",
                        LastName = "Test",
                        DepartmentId = legalDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(legal, "Legal123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(legal, UserRoles.LegalManager);
                        results.Add($"Created Legal Manager: {legal.Id} - {legal.Email}");
                    }
                }

                // Create Salesmen (3-5 users)
                results.Add("\n--- Creating Salesmen ---");
                for (int i = 1; i <= 4; i++)
                {
                    var salesman = new ApplicationUser
                    {
                        Id = await _userIdGenerationService.GenerateUserIdAsync(
                            $"Sales{i}", "Test", UserRoles.Salesman, salesDept?.Id, null),
                        UserName = $"sales{i}@company.com",
                        Email = $"sales{i}@company.com",
                        FirstName = $"Sales{i}",
                        LastName = "Test",
                        DepartmentId = salesDept?.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(salesman, "Sales123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(salesman, UserRoles.Salesman);
                        results.Add($"Created Salesman: {salesman.Id} - {salesman.Email}");
                    }
                }

                results.Add("\n=== OPERATION COMPLETED SUCCESSFULLY ===");
                return string.Join("\n", results);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
