using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Services;
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
        private readonly UserIdGenerationService userIdGenerationService;
        private readonly IWebHostEnvironment _environment;

        public RoleSpecificUserController(UserManager<ApplicationUser> _userManager, Context _context, UserIdGenerationService _userIdGenerationService, IWebHostEnvironment environment)
        {
            userManager = _userManager;
            context = _context;
            userIdGenerationService = _userIdGenerationService;
            _environment = environment;
        }

        // Create Doctor with User Account
        [HttpPost("doctor")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateDoctor([FromForm] CreateDoctorDTO doctorDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(doctorDTO.HospitalId);
            if (hospital == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Hospital with ID '{doctorDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
                    "HospitalId",
                    "HOSPITAL_NOT_FOUND"
                ));
            }

            // Get Medical department
            var medicalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Medical");
            if (medicalDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Medical department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                doctorDTO.FirstName ?? "Unknown",
                doctorDTO.LastName ?? "User",
                UserRoles.Doctor,
                doctorDTO.DepartmentId ?? medicalDepartment.Id,
                doctorDTO.HospitalId
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = doctorDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
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

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (doctorDTO.ProfileImage != null && doctorDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, doctorDTO.ProfileImage, doctorDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedDoctorResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Doctor,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                DoctorId = doctor.DoctorId,
                Specialty = doctor.Specialty,
                HospitalName = hospital.Name,
                ProfileImage = profileImageDto,
                Message = $"Doctor '{doctor.Name}' created successfully and assigned to hospital '{hospital.Name}'" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Engineer with User Account
        [HttpPost("engineer")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEngineer([FromForm] CreateEngineerDTO engineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Validate governorates
            var governorates = await context.Governorates
                .Where(g => engineerDTO.GovernorateIds.Contains(g.GovernorateId))
                .ToListAsync();

            if (governorates.Count != engineerDTO.GovernorateIds.Count)
            {
                var missingIds = engineerDTO.GovernorateIds.Except(governorates.Select(g => g.GovernorateId));
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Governorates with IDs [{string.Join(", ", missingIds)}] not found. Please verify the governorate IDs are correct.",
                    "GovernorateIds",
                    "GOVERNORATE_NOT_FOUND"
                ));
            }

            // Get Engineering department
            var engineeringDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering");
            if (engineeringDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Engineering department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                engineerDTO.FirstName ?? "Unknown",
                engineerDTO.LastName ?? "User",
                UserRoles.Engineer,
                engineerDTO.DepartmentId ?? engineeringDepartment.Id,
                null // Engineers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = engineerDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
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

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (engineerDTO.ProfileImage != null && engineerDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, engineerDTO.ProfileImage, engineerDTO.ImageAltText);
            }

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
                Email = user.Email, // Email is now the username
                Role = UserRoles.Engineer,
                DepartmentName = engineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                EngineerId = engineer.EngineerId,
                Specialty = engineer.Specialty,
                AssignedGovernorates = governorates.Select(g => g.Name).ToList(),
                ProfileImage = profileImageDto,
                Message = $"Engineer '{engineer.Name}' created successfully and assigned to {governorates.Count} governorate(s)" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Technician with User Account
        [HttpPost("technician")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateTechnician([FromForm] CreateTechnicianDTO technicianDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(technicianDTO.HospitalId);
            if (hospital == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Hospital with ID '{technicianDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
                    "HospitalId",
                    "HOSPITAL_NOT_FOUND"
                ));
            }

            // Get Medical department
            var medicalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Medical");
            if (medicalDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Medical department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                technicianDTO.FirstName ?? "Unknown",
                technicianDTO.LastName ?? "User",
                UserRoles.Technician,
                technicianDTO.DepartmentId ?? medicalDepartment.Id,
                technicianDTO.HospitalId
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = technicianDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
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

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (technicianDTO.ProfileImage != null && technicianDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, technicianDTO.ProfileImage, technicianDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedTechnicianResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Technician,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                TechnicianId = technician.TechnicianId,
                Department = technician.Department,
                HospitalName = hospital.Name,
                ProfileImage = profileImageDto,
                Message = $"Technician '{technician.Name}' created successfully and assigned to hospital '{hospital.Name}'" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Admin with User Account
        [HttpPost("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromForm] CreateAdminDTO adminDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Administration department
            var adminDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Administration");
            if (adminDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Administration department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                adminDTO.FirstName ?? "Unknown",
                adminDTO.LastName ?? "User",
                UserRoles.Admin,
                adminDTO.DepartmentId ?? adminDepartment.Id,
                null // Admins don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = adminDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign Admin role
            await userManager.AddToRoleAsync(user, UserRoles.Admin);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (adminDTO.ProfileImage != null && adminDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, adminDTO.ProfileImage, adminDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Admin,
                DepartmentName = adminDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Admin '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Finance Manager with User Account
        [HttpPost("finance-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateFinanceManager([FromForm] CreateFinanceManagerDTO financeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Finance department
            var financeDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Finance");
            if (financeDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Finance department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                financeDTO.FirstName ?? "Unknown",
                financeDTO.LastName ?? "User",
                UserRoles.FinanceManager,
                financeDTO.DepartmentId ?? financeDepartment.Id,
                null // Finance Managers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = financeDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign FinanceManager role
            await userManager.AddToRoleAsync(user, UserRoles.FinanceManager);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (financeDTO.ProfileImage != null && financeDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, financeDTO.ProfileImage, financeDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceManager,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Finance Manager '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Legal Manager with User Account
        [HttpPost("legal-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateLegalManager([FromForm] CreateLegalManagerDTO legalDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Legal department
            var legalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Legal");
            if (legalDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Legal department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                legalDTO.FirstName ?? "Unknown",
                legalDTO.LastName ?? "User",
                UserRoles.LegalManager,
                legalDTO.DepartmentId ?? legalDepartment.Id,
                null // Legal Managers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = legalDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign LegalManager role
            await userManager.AddToRoleAsync(user, UserRoles.LegalManager);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (legalDTO.ProfileImage != null && legalDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, legalDTO.ProfileImage, legalDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalManager,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Legal Manager '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Salesman with User Account
        [HttpPost("salesman")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateSalesman([FromForm] CreateSalesmanDTO salesDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Sales department
            var salesDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Sales");
            if (salesDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Sales department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                salesDTO.FirstName ?? "Unknown",
                salesDTO.LastName ?? "User",
                UserRoles.Salesman,
                salesDTO.DepartmentId ?? salesDepartment.Id,
                null // Salesmen don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = salesDTO.Email, // Use email as username
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
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign Salesman role
            await userManager.AddToRoleAsync(user, UserRoles.Salesman);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (salesDTO.ProfileImage != null && salesDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, salesDTO.ProfileImage, salesDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Salesman,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Salesman '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Finance Employee with User Account
        [HttpPost("finance-employee")]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
        public async Task<IActionResult> CreateFinanceEmployee([FromForm] CreateFinanceEmployeeDTO financeEmployeeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Finance department
            var financeDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Finance");
            if (financeDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Finance department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                financeEmployeeDTO.FirstName ?? "Unknown",
                financeEmployeeDTO.LastName ?? "User",
                UserRoles.FinanceEmployee,
                financeEmployeeDTO.DepartmentId ?? financeDepartment.Id
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = financeEmployeeDTO.Email, // Use email as username
                Email = financeEmployeeDTO.Email,
                FirstName = financeEmployeeDTO.FirstName,
                LastName = financeEmployeeDTO.LastName,
                DepartmentId = financeEmployeeDTO.DepartmentId ?? financeDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, financeEmployeeDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign FinanceEmployee role
            await userManager.AddToRoleAsync(user, UserRoles.FinanceEmployee);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (financeEmployeeDTO.ProfileImage != null && financeEmployeeDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, financeEmployeeDTO.ProfileImage, financeEmployeeDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceEmployee,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Finance Employee '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        // Create Legal Employee with User Account
        [HttpPost("legal-employee")]
        [Authorize(Roles = "SuperAdmin,Admin,LegalManager")]
        public async Task<IActionResult> CreateLegalEmployee([FromForm] CreateLegalEmployeeDTO legalEmployeeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Legal department
            var legalDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Legal");
            if (legalDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Legal department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                legalEmployeeDTO.FirstName ?? "Unknown",
                legalEmployeeDTO.LastName ?? "User",
                UserRoles.LegalEmployee,
                legalEmployeeDTO.DepartmentId ?? legalDepartment.Id
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = legalEmployeeDTO.Email, // Use email as username
                Email = legalEmployeeDTO.Email,
                FirstName = legalEmployeeDTO.FirstName,
                LastName = legalEmployeeDTO.LastName,
                DepartmentId = legalEmployeeDTO.DepartmentId ?? legalDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, legalEmployeeDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign LegalEmployee role
            await userManager.AddToRoleAsync(user, UserRoles.LegalEmployee);

            // Handle profile image upload if provided
            UserImageDTO? profileImageDto = null;
            if (legalEmployeeDTO.ProfileImage != null && legalEmployeeDTO.ProfileImage.Length > 0)
            {
                profileImageDto = await UploadProfileImage(user, legalEmployeeDTO.ProfileImage, legalEmployeeDTO.ImageAltText);
            }

            await context.SaveChangesAsync();

            return Ok(new CreatedUserResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalEmployee,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageDto,
                Message = $"Legal Employee '{user.UserName}' created successfully" + (profileImageDto != null ? " with profile image" : "")
            });
        }

        private async Task<UserImageDTO?> UploadProfileImage(ApplicationUser user, IFormFile imageFile, string? altText)
        {
            try
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    return null; // Skip invalid files silently
                }

                // Validate file size (max 10MB)
                if (imageFile.Length > 10 * 1024 * 1024)
                {
                    return null; // Skip oversized files silently
                }

                // Create uploads directory
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "user-images", user.Id);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Create database record
                var userImage = new UserImage
                {
                    UserId = user.Id,
                    FileName = imageFile.FileName,
                    FilePath = Path.Combine("uploads", "user-images", user.Id, fileName).Replace("\\", "/"),
                    ContentType = imageFile.ContentType,
                    FileSize = imageFile.Length,
                    AltText = altText ?? $"{user.FullName} profile picture",
                    IsProfileImage = true,
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.UserImages.Add(userImage);

                return new UserImageDTO
                {
                    Id = userImage.Id,
                    UserId = userImage.UserId,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    UploadedAt = userImage.UploadedAt,
                    IsActive = userImage.IsActive,
                    IsProfileImage = userImage.IsProfileImage
                };
            }
            catch
            {
                return null; // Return null if image upload fails
            }
        }
    }
}

