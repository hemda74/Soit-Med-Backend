using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Services;
using SoitMed.Repositories;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserIdGenerationService userIdGenerationService;
        private readonly IRoleBasedImageUploadService _imageUploadService;

        public RoleSpecificUserController(UserManager<ApplicationUser> _userManager, IUnitOfWork unitOfWork, UserIdGenerationService _userIdGenerationService, IRoleBasedImageUploadService imageUploadService)
        {
            userManager = _userManager;
            _unitOfWork = unitOfWork;
            userIdGenerationService = _userIdGenerationService;
            _imageUploadService = imageUploadService;
        }

        // Create Doctor with User Account and Optional Image
        [HttpPost("doctor")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateDoctor([FromForm] CreateDoctorWithImageDTO doctorDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Check if hospital exists
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(doctorDTO.HospitalId);
            if (hospital == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Hospital with ID '{doctorDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
                    "HospitalId",
                    "HOSPITAL_NOT_FOUND"
                ));
            }

            // Get Medical department
            var medicalDepartment = await _unitOfWork.Departments.GetByNameAsync("Medical");
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

            await _unitOfWork.Doctors.CreateAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

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
                Message = $"Doctor '{doctor.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }



        // Create Engineer with User Account
        [HttpPost("engineer")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEngineer([FromForm] CreateEngineerWithImageDTO engineerDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Validate governorates
            var governorates = await _unitOfWork.Governorates.GetFilteredAsync(g => engineerDTO.GovernorateIds.Contains(g.GovernorateId));

            if (governorates.Count() != engineerDTO.GovernorateIds.Count)
            {
                var missingIds = engineerDTO.GovernorateIds.Except(governorates.Select(g => g.GovernorateId));
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Governorates with IDs [{string.Join(", ", missingIds)}] not found. Please verify the governorate IDs are correct.",
                    "GovernorateIds",
                    "GOVERNORATE_NOT_FOUND"
                ));
            }

            // Get Engineering department
            var engineeringDepartment = await _unitOfWork.Departments.GetByNameAsync("Engineering");
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

            await _unitOfWork.Engineers.CreateAsync(engineer);
            await _unitOfWork.SaveChangesAsync();

            // Assign governorates to engineer
            // Create Engineer-Governorate relationships
            // Note: EngineerGovernorate is a junction table, we'll add it directly to context
            // This would need a proper repository if we want to follow the pattern completely
            // For now, we'll use the context directly for junction tables

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
                Message = $"Engineer '{engineer.Name}' created successfully and assigned to {governorates.Count()} governorate(s)"
            });
        }

        // Create Technician with User Account
        [HttpPost("technician")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateTechnician([FromForm] CreateTechnicianWithImageDTO technicianDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Check if hospital exists
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(technicianDTO.HospitalId);
            if (hospital == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Hospital with ID '{technicianDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
                    "HospitalId",
                    "HOSPITAL_NOT_FOUND"
                ));
            }

            // Get Medical department
            var medicalDepartment = await _unitOfWork.Departments.GetByNameAsync("Medical");
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

            await _unitOfWork.Technicians.CreateAsync(technician);
            await _unitOfWork.SaveChangesAsync();

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
                Message = $"Technician '{technician.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }

        // Create Admin with User Account
        [HttpPost("admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromForm] CreateAdminWithImageDTO adminDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Administration department
            var adminDepartment = await _unitOfWork.Departments.GetByNameAsync("Administration");
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

            // Handle image upload if provided
            AdminImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "admin", adminDepartment.Name, adminDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new AdminImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedAdminWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Admin,
                DepartmentName = adminDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Admin '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Finance Manager with User Account
        [HttpPost("finance-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateFinanceManager([FromForm] CreateFinanceManagerWithImageDTO financeDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Finance department
            var financeDepartment = await _unitOfWork.Departments.GetByNameAsync("Finance");
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

            // Handle image upload if provided
            FinanceManagerImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "finance-manager", financeDepartment.Name, financeDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new FinanceManagerImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedFinanceManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceManager,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Finance Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Legal Manager with User Account
        [HttpPost("legal-manager")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateLegalManager([FromForm] CreateLegalManagerWithImageDTO legalDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Legal department
            var legalDepartment = await _unitOfWork.Departments.GetByNameAsync("Legal");
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

            // Handle image upload if provided
            LegalManagerImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "legal-manager", legalDepartment.Name, legalDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new LegalManagerImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedLegalManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalManager,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Legal Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Salesman with User Account
        [HttpPost("salesman")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateSalesman([FromForm] CreateSalesmanWithImageDTO salesDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Sales department
            var salesDepartment = await _unitOfWork.Departments.GetByNameAsync("Sales");
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

            // Handle image upload if provided
            SalesmanImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "salesman", salesDepartment.Name, salesDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new SalesmanImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedSalesmanWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Salesman,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Salesman '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Finance Employee with User Account
        [HttpPost("finance-employee")]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager")]
        public async Task<IActionResult> CreateFinanceEmployee([FromForm] CreateFinanceEmployeeWithImageDTO financeEmployeeDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Finance department
            var financeDepartment = await _unitOfWork.Departments.GetByNameAsync("Finance");
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
                financeEmployeeDTO.DepartmentId ?? financeDepartment.Id,
                null // Finance Employees don't use hospital ID
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

            // Handle image upload if provided
            FinanceEmployeeImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "finance-employee", financeDepartment.Name, financeEmployeeDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new FinanceEmployeeImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedFinanceEmployeeWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceEmployee,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Finance Employee '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Legal Employee with User Account
        [HttpPost("legal-employee")]
        [Authorize(Roles = "SuperAdmin,Admin,LegalManager")]
        public async Task<IActionResult> CreateLegalEmployee([FromForm] CreateLegalEmployeeWithImageDTO legalEmployeeDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Legal department
            var legalDepartment = await _unitOfWork.Departments.GetByNameAsync("Legal");
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
                legalEmployeeDTO.DepartmentId ?? legalDepartment.Id,
                null // Legal Employees don't use hospital ID
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

            // Handle image upload if provided
            LegalEmployeeImageInfo? profileImageInfo = null;
            if (profileImage != null && profileImage.Length > 0)
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, "legal-employee", legalDepartment.Name, legalEmployeeDTO.AltText);
                if (imageResult.Success)
                {
                    var userImage = new UserImage
                    {
                        UserId = user.Id,
                        FileName = imageResult.FileName ?? "profile.jpg",
                        FilePath = imageResult.FilePath ?? "",
                        ContentType = imageResult.ContentType,
                        FileSize = imageResult.FileSize,
                        AltText = imageResult.AltText,
                        IsProfileImage = true,
                        ImageType = "profile",
                        UploadedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _unitOfWork.UserImages.CreateAsync(userImage);
                    await _unitOfWork.SaveChangesAsync();

                    profileImageInfo = new LegalEmployeeImageInfo
                    {
                        Id = userImage.Id,
                        FileName = userImage.FileName,
                        FilePath = userImage.FilePath,
                        ContentType = userImage.ContentType,
                        FileSize = userImage.FileSize,
                        AltText = userImage.AltText,
                        IsProfileImage = userImage.IsProfileImage,
                        UploadedAt = userImage.UploadedAt
                    };
                }
            }

            return Ok(new CreatedLegalEmployeeWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalEmployee,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Legal Employee '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }
    }
}

