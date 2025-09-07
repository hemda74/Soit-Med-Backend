using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleSpecificUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly Context context;

        public RoleSpecificUserController(UserManager<ApplicationUser> _userManager, Context _context)
        {
            userManager = _userManager;
            context = _context;
        }

        // Create Doctor with User Account
        [HttpPost("doctor")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateDoctor(CreateDoctorDTO doctorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(doctorDTO.HospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {doctorDTO.HospitalId} not found");
            }

            // Get Medical department
            var medicalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Medical");
            if (medicalDepartment == null)
            {
                return BadRequest("Medical department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = doctorDTO.UserName,
                Email = doctorDTO.Email,
                FirstName = doctorDTO.FirstName,
                LastName = doctorDTO.LastName,
                DepartmentId = doctorDTO.DepartmentId ?? medicalDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, doctorDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign Doctor role
            await userManager.AddToRoleAsync(user, UserRoles.Doctor);

            // Create Doctor entity
            var doctor = new Doctor
            {
                Name = $"{doctorDTO.FirstName} {doctorDTO.LastName}".Trim(),
                Specialty = doctorDTO.Specialty,
                HospitalId = doctorDTO.HospitalId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Doctors.Add(doctor);
            await context.SaveChangesAsync();

            return Ok(new CreatedDoctorResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.Doctor,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                DoctorId = doctor.DoctorId,
                Specialty = doctor.Specialty,
                HospitalName = hospital.Name,
                Message = $"Doctor '{doctor.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }

        // Create Engineer with User Account
        [HttpPost("engineer")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEngineer(CreateEngineerDTO engineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate governorates
            var governorates = await context.Governorates
                .Where(g => engineerDTO.GovernorateIds.Contains(g.GovernorateId))
                .ToListAsync();

            if (governorates.Count != engineerDTO.GovernorateIds.Count)
            {
                var missingIds = engineerDTO.GovernorateIds.Except(governorates.Select(g => g.GovernorateId));
                return BadRequest($"Governorates with IDs [{string.Join(", ", missingIds)}] not found");
            }

            // Get Engineering department
            var engineeringDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
            if (engineeringDepartment == null)
            {
                return BadRequest("Engineering department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = engineerDTO.UserName,
                Email = engineerDTO.Email,
                FirstName = engineerDTO.FirstName,
                LastName = engineerDTO.LastName,
                DepartmentId = engineerDTO.DepartmentId ?? engineeringDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, engineerDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign Engineer role
            await userManager.AddToRoleAsync(user, UserRoles.Engineer);

            // Create Engineer entity
            var engineer = new Engineer
            {
                Name = $"{engineerDTO.FirstName} {engineerDTO.LastName}".Trim(),
                Specialty = engineerDTO.Specialty,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Engineers.Add(engineer);
            await context.SaveChangesAsync();

            // Assign governorates to engineer
            foreach (var governorate in governorates)
            {
                var engineerGovernorate = new EngineerGovernorate
                {
                    EngineerId = engineer.EngineerId,
                    GovernorateId = governorate.GovernorateId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };
                context.EngineerGovernorates.Add(engineerGovernorate);
            }
            await context.SaveChangesAsync();

            return Ok(new CreatedEngineerResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.Engineer,
                DepartmentName = engineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                EngineerId = engineer.EngineerId,
                Specialty = engineer.Specialty,
                AssignedGovernorates = governorates.Select(g => g.Name).ToList(),
                Message = $"Engineer '{engineer.Name}' created successfully and assigned to {governorates.Count} governorate(s)"
            });
        }

        // Create Technician with User Account
        [HttpPost("technician")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateTechnician(CreateTechnicianDTO technicianDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(technicianDTO.HospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {technicianDTO.HospitalId} not found");
            }

            // Get Medical department
            var medicalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Medical");
            if (medicalDepartment == null)
            {
                return BadRequest("Medical department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = technicianDTO.UserName,
                Email = technicianDTO.Email,
                FirstName = technicianDTO.FirstName,
                LastName = technicianDTO.LastName,
                DepartmentId = technicianDTO.DepartmentId ?? medicalDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, technicianDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign Technician role
            await userManager.AddToRoleAsync(user, UserRoles.Technician);

            // Create Technician entity
            var technician = new Technician
            {
                Name = $"{technicianDTO.FirstName} {technicianDTO.LastName}".Trim(),
                Department = technicianDTO.Department,
                HospitalId = technicianDTO.HospitalId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Technicians.Add(technician);
            await context.SaveChangesAsync();

            return Ok(new CreatedTechnicianResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.Technician,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                TechnicianId = technician.TechnicianId,
                Department = technician.Department,
                HospitalName = hospital.Name,
                Message = $"Technician '{technician.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }

        // Create Admin with User Account
        [HttpPost("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin(CreateAdminDTO adminDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get Administration department
            var adminDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Administration");
            if (adminDepartment == null)
            {
                return BadRequest("Administration department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = adminDTO.UserName,
                Email = adminDTO.Email,
                FirstName = adminDTO.FirstName,
                LastName = adminDTO.LastName,
                DepartmentId = adminDTO.DepartmentId ?? adminDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, adminDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign Admin role
            await userManager.AddToRoleAsync(user, UserRoles.Admin);

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.Admin,
                DepartmentName = adminDepartment.Name,
                CreatedAt = user.CreatedAt,
                Message = $"Admin '{user.UserName}' created successfully"
            });
        }

        // Create Finance Manager with User Account
        [HttpPost("finance-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateFinanceManager(CreateFinanceManagerDTO financeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get Finance department
            var financeDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Finance");
            if (financeDepartment == null)
            {
                return BadRequest("Finance department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = financeDTO.UserName,
                Email = financeDTO.Email,
                FirstName = financeDTO.FirstName,
                LastName = financeDTO.LastName,
                DepartmentId = financeDTO.DepartmentId ?? financeDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, financeDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign FinanceManager role
            await userManager.AddToRoleAsync(user, UserRoles.FinanceManager);

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.FinanceManager,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                Message = $"Finance Manager '{user.UserName}' created successfully"
            });
        }

        // Create Legal Manager with User Account
        [HttpPost("legal-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateLegalManager(CreateLegalManagerDTO legalDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get Legal department
            var legalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Legal");
            if (legalDepartment == null)
            {
                return BadRequest("Legal department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = legalDTO.UserName,
                Email = legalDTO.Email,
                FirstName = legalDTO.FirstName,
                LastName = legalDTO.LastName,
                DepartmentId = legalDTO.DepartmentId ?? legalDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, legalDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign LegalManager role
            await userManager.AddToRoleAsync(user, UserRoles.LegalManager);

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.LegalManager,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                Message = $"Legal Manager '{user.UserName}' created successfully"
            });
        }

        // Create Salesman with User Account
        [HttpPost("salesman")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateSalesman(CreateSalesmanDTO salesDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get Sales department
            var salesDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Sales");
            if (salesDepartment == null)
            {
                return BadRequest("Sales department not found. Please ensure departments are seeded.");
            }

            // Create user account
            var user = new ApplicationUser
            {
                UserName = salesDTO.UserName,
                Email = salesDTO.Email,
                FirstName = salesDTO.FirstName,
                LastName = salesDTO.LastName,
                DepartmentId = salesDTO.DepartmentId ?? salesDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, salesDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign Salesman role
            await userManager.AddToRoleAsync(user, UserRoles.Salesman);

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = UserRoles.Salesman,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                Message = $"Salesman '{user.UserName}' created successfully"
            });
        }
    }
}

