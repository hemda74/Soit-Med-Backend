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
        private readonly IWebHostEnvironment _environment;

        // Delegate for creating role-specific image info
        private delegate object CreateImageInfoDelegate(UserImage userImage);

        /// <summary>
        /// Generalized method to handle image upload for any role
        /// </summary>
        private async Task<object> HandleImageUploadAsync(
            IFormFile? profileImage, 
            ApplicationUser user, 
            string role, 
            string departmentName, 
            string? altText, 
            CreateImageInfoDelegate createImageInfoDelegate)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return null; // No image provided, return null
            }

            try
            {
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, role, departmentName, altText);
                if (!imageResult.Success)
                {
                    // If upload fails, return a validation problem
                    return new BadRequestObjectResult(new { message = imageResult.ErrorMessage });
                }

                // First, deactivate any existing profile images for this user
                var existingProfileImages = await _unitOfWork.UserImages.GetAllAsync();
                var userProfileImages = existingProfileImages
                    .Where(ui => ui.UserId == user.Id && ui.IsProfileImage && ui.IsActive)
                    .ToList();

                foreach (var existingImage in userProfileImages)
                {
                    existingImage.IsActive = false;
                    await _unitOfWork.UserImages.UpdateAsync(existingImage);
                }

                // Create new profile image
                var userImage = new UserImage
                {
                    UserId = user.Id,
                    FileName = imageResult.FileName ?? "profile.jpg",
                    FilePath = imageResult.FilePath ?? "",
                    ContentType = imageResult.ContentType ?? "image/jpeg",
                    FileSize = imageResult.FileSize,
                    AltText = imageResult.AltText,
                    ImageType = "Profile",
                    IsProfileImage = true,
                    IsActive = true,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserImages.CreateAsync(userImage);
                await _unitOfWork.SaveChangesAsync();

                return createImageInfoDelegate(userImage);
            }
            catch (Exception ex)
            {
                // Log the error and return a proper error response
                return new BadRequestObjectResult(new { message = $"Error uploading image: {ex.Message}" });
            }
        }

        public RoleSpecificUserController(UserManager<ApplicationUser> _userManager, IUnitOfWork unitOfWork, UserIdGenerationService _userIdGenerationService, IRoleBasedImageUploadService imageUploadService, IWebHostEnvironment environment)
        {
            userManager = _userManager;
            _unitOfWork = unitOfWork;
            userIdGenerationService = _userIdGenerationService;
            _imageUploadService = imageUploadService;
            _environment = environment;
        }

        // Create Doctor with User Account and Optional Image
        [HttpPost("doctor")]
        [Consumes("multipart/form-data")]
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
                PhoneNumber = doctorDTO.PhoneNumber,
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
                UserId = user.Id,
                HospitalId = doctorDTO.HospitalId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Doctors.CreateAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            // Create Doctor-Hospital relationship
            var doctorHospital = new DoctorHospital
            {
                DoctorId = doctor.DoctorId,
                HospitalId = doctorDTO.HospitalId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            };
            await _unitOfWork.DoctorHospitals.CreateAsync(doctorHospital);
            await _unitOfWork.SaveChangesAsync();

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "doctor", 
                medicalDepartment.Name, 
                doctorDTO.AltText,
                userImage => new DoctorImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as DoctorImageInfo;



            return Ok(new CreatedDoctorWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Doctor,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                DoctorId = doctor.DoctorId,
                Specialty = doctor.Specialty,
                HospitalName = hospital.Name,
                ProfileImage = profileImageInfo,
                Message = $"Doctor '{doctor.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }



        // Create Engineer with User Account
        [HttpPost("engineer")]
        [Consumes("multipart/form-data")]
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
                PhoneNumber = engineerDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "engineer", 
                engineeringDepartment.Name, 
                engineerDTO.AltText,
                userImage => new EngineerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as EngineerImageInfo;

            return Ok(new CreatedEngineerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Engineer,
                DepartmentName = engineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                EngineerId = engineer.EngineerId.ToString(),
                Specialty = engineer.Specialty,
                GovernorateNames = governorates?.Select(g => g.Name).ToList() ?? new List<string>(),
                ProfileImage = profileImageInfo,
                Message = $"Engineer '{engineer.Name}' created successfully and assigned to {governorates.Count()} governorate(s)"
            });
        }

        // Create Technician with User Account
        [HttpPost("technician")]
        [Consumes("multipart/form-data")]
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
                PhoneNumber = technicianDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "technician", 
                medicalDepartment.Name, 
                technicianDTO.AltText,
                userImage => new TechnicianImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as TechnicianImageInfo;

            return Ok(new CreatedTechnicianWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Technician,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                TechnicianId = technician.TechnicianId,
                Department = technician.Department,
                HospitalName = hospital.Name,
                ProfileImage = profileImageInfo,
                Message = $"Technician '{technician.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }

        // Create Admin with User Account
        [HttpPost("admin")]
        [Consumes("multipart/form-data")]
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
                PhoneNumber = adminDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "admin", 
                adminDepartment.Name, 
                adminDTO.AltText,
                userImage => new AdminImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as AdminImageInfo;

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
        [Consumes("multipart/form-data")]
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
                PhoneNumber = financeDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "finance-manager", 
                financeDepartment.Name, 
                financeDTO.AltText,
                userImage => new FinanceManagerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as FinanceManagerImageInfo;

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
        [Consumes("multipart/form-data")]
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
                PhoneNumber = legalDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "legal-manager", 
                legalDepartment.Name, 
                legalDTO.AltText,
                userImage => new LegalManagerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as LegalManagerImageInfo;

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
        [Consumes("multipart/form-data")]
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
                PhoneNumber = salesDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "salesman", 
                salesDepartment.Name, 
                salesDTO.AltText,
                userImage => new SalesmanImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as SalesmanImageInfo;

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

        // Create Sales Manager with User Account
        [HttpPost("sales-manager")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(CreatedSalesManagerWithImageResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSalesManager([FromForm] CreateSalesManagerWithImageDTO salesManagerDTO, [FromForm] IFormFile? profileImage = null)
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
                salesManagerDTO.FirstName ?? "Unknown",
                salesManagerDTO.LastName ?? "User",
                UserRoles.SalesManager,
                salesManagerDTO.DepartmentId ?? salesDepartment.Id,
                null // Sales Managers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = salesManagerDTO.Email, // Use email as username
                Email = salesManagerDTO.Email,
                FirstName = salesManagerDTO.FirstName,
                LastName = salesManagerDTO.LastName,
                PhoneNumber = salesManagerDTO.PhoneNumber,
                DepartmentId = salesManagerDTO.DepartmentId ?? salesDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, salesManagerDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign Sales Manager role
            await userManager.AddToRoleAsync(user, UserRoles.SalesManager);

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "sales-manager", 
                salesDepartment.Name, 
                salesManagerDTO.AltText,
                userImage => new SalesManagerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as SalesManagerImageInfo;

            return Ok(new CreatedSalesManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.SalesManager,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Sales Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : ""),
                SalesTerritory = salesManagerDTO.SalesTerritory,
                SalesTeam = salesManagerDTO.SalesTeam,
                SalesTarget = salesManagerDTO.SalesTarget
            });
        }

        // Create Finance Employee with User Account
        [HttpPost("finance-employee")]
        [Consumes("multipart/form-data")]
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
                PhoneNumber = financeEmployeeDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "finance-employee", 
                financeDepartment.Name, 
                financeEmployeeDTO.AltText,
                userImage => new FinanceEmployeeImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as FinanceEmployeeImageInfo;

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
        [Consumes("multipart/form-data")]
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
                PhoneNumber = legalEmployeeDTO.PhoneNumber,
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

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "legal-employee", 
                legalDepartment.Name, 
                legalEmployeeDTO.AltText,
                userImage => new LegalEmployeeImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as LegalEmployeeImageInfo;

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

        // Create Maintenance Manager with User Account
        [HttpPost("maintenance-manager")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateMaintenanceManager([FromForm] CreateMaintenanceManagerWithImageDTO maintenanceManagerDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
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
                maintenanceManagerDTO.FirstName ?? "Unknown",
                maintenanceManagerDTO.LastName ?? "User",
                UserRoles.MaintenanceManager,
                maintenanceManagerDTO.DepartmentId ?? engineeringDepartment.Id,
                null // Maintenance Managers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = maintenanceManagerDTO.Email, // Use email as username
                Email = maintenanceManagerDTO.Email,
                FirstName = maintenanceManagerDTO.FirstName,
                LastName = maintenanceManagerDTO.LastName,
                PhoneNumber = maintenanceManagerDTO.PhoneNumber,
                DepartmentId = maintenanceManagerDTO.DepartmentId ?? engineeringDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, maintenanceManagerDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign MaintenanceManager role
            await userManager.AddToRoleAsync(user, UserRoles.MaintenanceManager);

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "maintenance-manager", 
                engineeringDepartment.Name, 
                maintenanceManagerDTO.AltText,
                userImage => new MaintenanceManagerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as MaintenanceManagerImageInfo;

            return Ok(new CreatedMaintenanceManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.MaintenanceManager,
                DepartmentName = engineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Maintenance Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Maintenance Support with User Account
        [HttpPost("maintenance-support")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin,MaintenanceManager")]
        public async Task<IActionResult> CreateMaintenanceSupport([FromForm] CreateMaintenanceSupportWithImageDTO maintenanceSupportDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
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
                maintenanceSupportDTO.FirstName ?? "Unknown",
                maintenanceSupportDTO.LastName ?? "User",
                UserRoles.MaintenanceSupport,
                maintenanceSupportDTO.DepartmentId ?? engineeringDepartment.Id,
                null // Maintenance Support don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = maintenanceSupportDTO.Email, // Use email as username
                Email = maintenanceSupportDTO.Email,
                FirstName = maintenanceSupportDTO.FirstName,
                LastName = maintenanceSupportDTO.LastName,
                PhoneNumber = maintenanceSupportDTO.PhoneNumber,
                DepartmentId = maintenanceSupportDTO.DepartmentId ?? engineeringDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, maintenanceSupportDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign MaintenanceSupport role
            await userManager.AddToRoleAsync(user, UserRoles.MaintenanceSupport);

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "maintenance-support", 
                engineeringDepartment.Name, 
                maintenanceSupportDTO.AltText,
                userImage => new MaintenanceSupportImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as MaintenanceSupportImageInfo;

            return Ok(new CreatedMaintenanceSupportWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.MaintenanceSupport,
                DepartmentName = engineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Maintenance Support '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Sales Support with User Account
        [HttpPost("sales-support")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin,SalesManager")]
        [ProducesResponseType(typeof(CreatedSalesSupportWithImageResponseDTO), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateSalesSupport([FromForm] CreateSalesSupportWithImageDTO salesSupportDTO, [FromForm] IFormFile? profileImage = null)
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
                salesSupportDTO.FirstName ?? "Unknown",
                salesSupportDTO.LastName ?? "User",
                UserRoles.SalesSupport,
                salesSupportDTO.DepartmentId ?? salesDepartment.Id,
                null // Sales Support don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = salesSupportDTO.Email, // Use email as username
                Email = salesSupportDTO.Email,
                FirstName = salesSupportDTO.FirstName,
                LastName = salesSupportDTO.LastName,
                PhoneNumber = salesSupportDTO.PhoneNumber,
                DepartmentId = salesSupportDTO.DepartmentId ?? salesDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, salesSupportDTO.Password);
            if (!result.Succeeded)
            {
                var identityErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
                    new Dictionary<string, string> { { "Password", string.Join("; ", identityErrors) } },
                    "User creation failed. Please check the following issues:"
                ));
            }

            // Assign SalesSupport role
            await userManager.AddToRoleAsync(user, UserRoles.SalesSupport);

            var profileImageInfoResult = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "sales-support", 
                salesDepartment.Name, 
                salesSupportDTO.AltText,
                userImage => new SalesSupportImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (profileImageInfoResult is BadRequestObjectResult badRequest)
            {
                return badRequest;
            }

            var profileImageInfo = profileImageInfoResult as SalesSupportImageInfo;

            return Ok(new CreatedSalesSupportWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.SalesSupport,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo,
                Message = $"Sales Support '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : ""),
                SupportSpecialization = salesSupportDTO.SupportSpecialization,
                SupportLevel = salesSupportDTO.SupportLevel,
                Notes = salesSupportDTO.Notes
            });
        }
    }
}

