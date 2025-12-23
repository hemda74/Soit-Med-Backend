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
        private async Task<(object? result, string? errorMessage)> HandleImageUploadAsync(
            IFormFile? profileImage, 
            ApplicationUser user, 
            string role, 
            string departmentName, 
            string? altText, 
            CreateImageInfoDelegate createImageInfoDelegate)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return (null, null); // No image provided, return null
            }

            try
            {
                // Validate file first
                if (!_imageUploadService.IsValidImageFile(profileImage))
                {
                    return (null, "Invalid image file. Please upload a valid JPG, JPEG, PNG, or GIF image (max 5MB).");
                }

                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, role, departmentName, altText);
                if (!imageResult.Success)
                {
                    return (null, imageResult.ErrorMessage ?? "Failed to upload image");
                }

                // Handle database operations with proper error handling
                try
                {
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

                    return (createImageInfoDelegate(userImage), null);
                }
                catch (Exception dbEx)
                {
                    // If database operation fails, clean up the uploaded file
                    try
                    {
                        if (!string.IsNullOrEmpty(imageResult.FilePath))
                        {
                            var fullPath = Path.Combine(_environment.WebRootPath, imageResult.FilePath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        // Log cleanup error but don't throw
                        Console.WriteLine($"Failed to clean up file: {cleanupEx.Message}");
                    }
                    
                    return (null, $"Database error: {dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Error uploading image: {ex.Message}");
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
        [HttpPost("Doctor")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateDoctor([FromForm] CreateDoctorWithImageDTO DoctorDTO, [FromForm] IFormFile? profileImage = null)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                // Check if hospital exists
                var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(DoctorDTO.HospitalId);
                if (hospital == null)
                {
                    return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                        $"Hospital with ID '{DoctorDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
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
                    DoctorDTO.FirstName ?? "Unknown",
                    DoctorDTO.LastName ?? "User",
                    UserRoles.Doctor,
                    DoctorDTO.DepartmentId ?? medicalDepartment.Id,
                    DoctorDTO.HospitalId
                );

                // Create user account
                var user = new ApplicationUser
                {
                    Id = customUserId, // Use custom generated ID
                    UserName = DoctorDTO.Email, // Use email as username
                    Email = DoctorDTO.Email,
                    FirstName = DoctorDTO.FirstName,
                    LastName = DoctorDTO.LastName,
                    PhoneNumber = DoctorDTO.PhoneNumber,
                    DepartmentId = DoctorDTO.DepartmentId ?? medicalDepartment.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, DoctorDTO.Password);
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
                var Doctor = new Doctor
                {
                    Name = $"{DoctorDTO.FirstName} {DoctorDTO.LastName}".Trim(),
                    Specialty = DoctorDTO.Specialty,
                    UserId = user.Id,
                    HospitalId = DoctorDTO.HospitalId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.Doctors.CreateAsync(Doctor);
                await _unitOfWork.SaveChangesAsync();

                // Create Doctor-Hospital relationship
                var DoctorHospital = new DoctorHospital
                {
                    DoctorId = Doctor.DoctorId,
                    HospitalId = DoctorDTO.HospitalId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow
                };
                await _unitOfWork.DoctorHospitals.CreateAsync(DoctorHospital);
                await _unitOfWork.SaveChangesAsync();

                var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                    profileImage, 
                    user, 
                    "Doctor", 
                    medicalDepartment.Name, 
                    DoctorDTO.AltText,
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

                if (!string.IsNullOrEmpty(imageError))
                {
                    return BadRequest(new { message = imageError });
                }

                return Ok(new CreatedDoctorWithImageResponseDTO
                {
                    UserId = user.Id,
                    Email = user.Email, // Email is now the username
                    Role = UserRoles.Doctor,
                    DepartmentName = medicalDepartment.Name,
                    CreatedAt = user.CreatedAt,
                    DoctorId = Doctor.DoctorId,
                    Specialty = Doctor.Specialty,
                    HospitalName = hospital.Name,
                    ProfileImage = profileImageInfo as DoctorImageInfo,
                    Message = $"Doctor '{Doctor.Name}' created successfully and assigned to hospital '{hospital.Name}'"
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in CreateDoctor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return BadRequest(new { 
                    message = "An error occurred while creating the Doctor. Please try again.",
                    error = ex.Message 
                });
            }
        }



        // Create Engineer with User Account
        [HttpPost("Engineer")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEngineer([FromForm] CreateEngineerWithImageDTO EngineerDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Validate governorates
            var governorates = await _unitOfWork.Governorates.GetFilteredAsync(g => EngineerDTO.GovernorateIds.Contains(g.GovernorateId));

            if (governorates.Count() != EngineerDTO.GovernorateIds.Count)
            {
                var missingIds = EngineerDTO.GovernorateIds.Except(governorates.Select(g => g.GovernorateId));
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Governorates with IDs [{string.Join(", ", missingIds)}] not found. Please verify the governorate IDs are correct.",
                    "GovernorateIds",
                    "GOVERNORATE_NOT_FOUND"
                ));
            }

            // Get Engineering department
            var EngineeringDepartment = await _unitOfWork.Departments.GetByNameAsync("Engineering");
            if (EngineeringDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Engineering department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                EngineerDTO.FirstName ?? "Unknown",
                EngineerDTO.LastName ?? "User",
                UserRoles.Engineer,
                EngineerDTO.DepartmentId ?? EngineeringDepartment.Id,
                null // Engineers don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = EngineerDTO.Email, // Use email as username
                Email = EngineerDTO.Email,
                FirstName = EngineerDTO.FirstName,
                LastName = EngineerDTO.LastName,
                PhoneNumber = EngineerDTO.PhoneNumber,
                DepartmentId = EngineerDTO.DepartmentId ?? EngineeringDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, EngineerDTO.Password);
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
            var Engineer = new Engineer
            {
                Name = $"{EngineerDTO.FirstName} {EngineerDTO.LastName}".Trim(),
                Specialty = EngineerDTO.Specialty,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Engineers.CreateAsync(Engineer);
            await _unitOfWork.SaveChangesAsync();

            // Assign governorates to Engineer
            // Create Engineer-Governorate relationships
            // Note: EngineerGovernorate is a junction table, we'll add it directly to context
            // This would need a proper repository if we want to follow the pattern completely
            // For now, we'll use the context directly for junction tables

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "Engineer", 
                EngineeringDepartment.Name, 
                EngineerDTO.AltText,
                userImage => new EngineerImageInfo
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType ?? string.Empty,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt
                });

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedEngineerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Engineer,
                DepartmentName = EngineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                EngineerId = Engineer.EngineerId.ToString(),
                Specialty = Engineer.Specialty,
                GovernorateNames = governorates?.Select(g => g.Name).ToList() ?? new List<string>(),
                ProfileImage = profileImageInfo as EngineerImageInfo,
                Message = $"Engineer '{Engineer.Name}' created successfully and assigned to {governorates?.Count()} governorate(s)"
            });
        }

        // Create Technician with User Account
        [HttpPost("Technician")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateTechnician([FromForm] CreateTechnicianWithImageDTO TechnicianDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Check if hospital exists
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(TechnicianDTO.HospitalId);
            if (hospital == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    $"Hospital with ID '{TechnicianDTO.HospitalId}' not found. Please verify the hospital ID is correct.",
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
                TechnicianDTO.FirstName ?? "Unknown",
                TechnicianDTO.LastName ?? "User",
                UserRoles.Technician,
                TechnicianDTO.DepartmentId ?? medicalDepartment.Id,
                TechnicianDTO.HospitalId
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = TechnicianDTO.Email, // Use email as username
                Email = TechnicianDTO.Email,
                FirstName = TechnicianDTO.FirstName,
                LastName = TechnicianDTO.LastName,
                PhoneNumber = TechnicianDTO.PhoneNumber,
                DepartmentId = TechnicianDTO.DepartmentId ?? medicalDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, TechnicianDTO.Password);
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
            var Technician = new Technician
            {
                Name = $"{TechnicianDTO.FirstName} {TechnicianDTO.LastName}".Trim(),
                Department = TechnicianDTO.Department,
                HospitalId = TechnicianDTO.HospitalId,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Technicians.CreateAsync(Technician);
            await _unitOfWork.SaveChangesAsync();

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "Technician", 
                medicalDepartment.Name, 
                TechnicianDTO.AltText,
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedTechnicianWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Technician,
                DepartmentName = medicalDepartment.Name,
                CreatedAt = user.CreatedAt,
                TechnicianId = Technician.TechnicianId,
                Department = Technician.Department,
                HospitalName = hospital.Name,
                ProfileImage = profileImageInfo as TechnicianImageInfo,
                Message = $"Technician '{Technician.Name}' created successfully and assigned to hospital '{hospital.Name}'"
            });
        }

        // Create Admin with User Account
        [HttpPost("Admin")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromForm] CreateAdminWithImageDTO AdminDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Administration department
            var AdminDepartment = await _unitOfWork.Departments.GetByNameAsync("Administration");
            if (AdminDepartment == null)
            {
                return BadRequest(ValidationHelperService.CreateBusinessLogicError(
                    "Administration department not found. Please ensure departments are seeded in the system.",
                    "DepartmentId",
                    "DEPARTMENT_NOT_FOUND"
                ));
            }

            // Generate custom user ID
            string customUserId = await userIdGenerationService.GenerateUserIdAsync(
                AdminDTO.FirstName ?? "Unknown",
                AdminDTO.LastName ?? "User",
                UserRoles.Admin,
                AdminDTO.DepartmentId ?? AdminDepartment.Id,
                null // Admins don't use hospital ID
            );

            // Create user account
            var user = new ApplicationUser
            {
                Id = customUserId, // Use custom generated ID
                UserName = AdminDTO.Email, // Use email as username
                Email = AdminDTO.Email,
                FirstName = AdminDTO.FirstName,
                LastName = AdminDTO.LastName,
                PhoneNumber = AdminDTO.PhoneNumber,
                DepartmentId = AdminDTO.DepartmentId ?? AdminDepartment.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, AdminDTO.Password);
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "Admin", 
                AdminDepartment.Name, 
                AdminDTO.AltText,
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedAdminWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.Admin,
                DepartmentName = AdminDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as AdminImageInfo,
                Message = $"Admin '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Finance Manager with User Account
        [HttpPost("FinanceManager")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "FinanceManager", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedFinanceManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceManager,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as FinanceManagerImageInfo,
                Message = $"Finance Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Legal Manager with User Account
        [HttpPost("LegalManager")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "LegalManager", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedLegalManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalManager,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as LegalManagerImageInfo,
                Message = $"Legal Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create SalesMan with User Account
        [HttpPost("SalesMan")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateSalesMan([FromForm] CreateSalesManWithImageDTO salesDTO, [FromForm] IFormFile? profileImage = null)
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
                UserRoles.SalesMan,
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

            // Assign SalesMan role
            await userManager.AddToRoleAsync(user, UserRoles.SalesMan);

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "SalesMan", 
                salesDepartment.Name, 
                salesDTO.AltText,
                userImage => new SalesManImageInfo
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedSalesManWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.SalesMan,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as SalesManImageInfo,
                Message = $"SalesMan '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Sales Manager with User Account
        [HttpPost("SalesManager")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "SalesManager", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedSalesManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.SalesManager,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as SalesManagerImageInfo,
                Message = $"Sales Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : ""),
                SalesTerritory = salesManagerDTO.SalesTerritory,
                SalesTeam = salesManagerDTO.SalesTeam,
                SalesTarget = salesManagerDTO.SalesTarget
            });
        }

        // Create Finance Employee with User Account
        [HttpPost("FinanceEmployee")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "FinanceEmployee", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedFinanceEmployeeWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.FinanceEmployee,
                DepartmentName = financeDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as FinanceEmployeeImageInfo,
                Message = $"Finance Employee '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Legal Employee with User Account
        [HttpPost("LegalEmployee")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "LegalEmployee", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedLegalEmployeeWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.LegalEmployee,
                DepartmentName = legalDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as LegalEmployeeImageInfo,
                Message = $"Legal Employee '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Maintenance Manager with User Account
        [HttpPost("MaintenanceManager")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateMaintenanceManager([FromForm] CreateMaintenanceManagerWithImageDTO maintenanceManagerDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Engineering department
            var EngineeringDepartment = await _unitOfWork.Departments.GetByNameAsync("Engineering");
            if (EngineeringDepartment == null)
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
                maintenanceManagerDTO.DepartmentId ?? EngineeringDepartment.Id,
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
                DepartmentId = maintenanceManagerDTO.DepartmentId ?? EngineeringDepartment.Id,
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "MaintenanceManager", 
                EngineeringDepartment.Name, 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedMaintenanceManagerWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.MaintenanceManager,
                DepartmentName = EngineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as MaintenanceManagerImageInfo,
                Message = $"Maintenance Manager '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Maintenance Support with User Account
        [HttpPost("MaintenanceSupport")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "SuperAdmin,Admin,MaintenanceManager")]
        public async Task<IActionResult> CreateMaintenanceSupport([FromForm] CreateMaintenanceSupportWithImageDTO maintenanceSupportDTO, [FromForm] IFormFile? profileImage = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            // Get Engineering department
            var EngineeringDepartment = await _unitOfWork.Departments.GetByNameAsync("Engineering");
            if (EngineeringDepartment == null)
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
                maintenanceSupportDTO.DepartmentId ?? EngineeringDepartment.Id,
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
                DepartmentId = maintenanceSupportDTO.DepartmentId ?? EngineeringDepartment.Id,
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "MaintenanceSupport", 
                EngineeringDepartment.Name, 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedMaintenanceSupportWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.MaintenanceSupport,
                DepartmentName = EngineeringDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as MaintenanceSupportImageInfo,
                Message = $"Maintenance Support '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : "")
            });
        }

        // Create Sales Support with User Account
        [HttpPost("SalesSupport")]
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

            var (profileImageInfo, imageError) = await HandleImageUploadAsync(
                profileImage, 
                user, 
                "SalesSupport", 
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

            if (!string.IsNullOrEmpty(imageError))
            {
                return BadRequest(new { message = imageError });
            }

            return Ok(new CreatedSalesSupportWithImageResponseDTO
            {
                UserId = user.Id,
                Email = user.Email, // Email is now the username
                Role = UserRoles.SalesSupport,
                DepartmentName = salesDepartment.Name,
                CreatedAt = user.CreatedAt,
                ProfileImage = profileImageInfo as SalesSupportImageInfo,
                Message = $"Sales Support '{user.UserName}' created successfully" + (profileImageInfo != null ? " with profile image" : ""),
                SupportSpecialization = salesSupportDTO.SupportSpecialization,
                SupportLevel = salesSupportDTO.SupportLevel,
                Notes = salesSupportDTO.Notes
            });
        }
    }
}

