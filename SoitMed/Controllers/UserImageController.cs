using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserImageController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;
        private readonly IRoleBasedImageUploadService _imageUploadService;
        private readonly ILogger<UserImageController> _logger;

        public UserImageController(
            UserManager<ApplicationUser> userManager,
            Context context,
            IRoleBasedImageUploadService imageUploadService,
            ILogger<UserImageController> logger)
        {
            _userManager = userManager;
            _context = context;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        // Upload user profile image (POST)
        [HttpPost("image")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateUserImageResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile profileImage, [FromForm] string? altText = null)
        {
            try
            {
                // Validate input
                if (profileImage == null || profileImage.Length == 0)
                {
                    return BadRequest(new { error = "Profile image is required", code = "IMAGE_REQUIRED" });
                }

                if (profileImage.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest(new { error = "Image file size cannot exceed 5MB", code = "IMAGE_TOO_LARGE" });
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(profileImage.ContentType?.ToLower()))
                {
                    return BadRequest(new { error = "Only JPEG, PNG, and GIF images are allowed", code = "INVALID_IMAGE_TYPE" });
                }

                // Get user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "User ID not found in token", code = "USER_ID_NOT_FOUND" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", code = "USER_NOT_FOUND" });
                }

                // Get user roles and department
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "user";
                var departmentName = user.Department?.Name ?? "default";

                // Upload image file
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, role, departmentName, altText);
                if (!imageResult.Success)
                {
                    return BadRequest(new { error = $"Failed to upload image: {imageResult.ErrorMessage}", code = "UPLOAD_FAILED" });
                }

                // Find existing profile image (regardless of IsActive status)
                var existingUserImage = await _context.UserImages
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsProfileImage);

                UserImage userImage;
                if (existingUserImage != null)
                {
                    // Update existing record
                    existingUserImage.FileName = imageResult.FileName ?? "profile.jpg";
                    existingUserImage.FilePath = imageResult.FilePath ?? "";
                    existingUserImage.ContentType = imageResult.ContentType ?? "image/jpeg";
                    existingUserImage.FileSize = imageResult.FileSize;
                    existingUserImage.AltText = imageResult.AltText;
                    existingUserImage.IsActive = true; // Ensure it's active
                    existingUserImage.UploadedAt = DateTime.UtcNow;
                    
                    _context.UserImages.Update(existingUserImage);
                    userImage = existingUserImage;
                }
                else
                {
                    // Create new user image record
                    userImage = new UserImage
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

                    _context.UserImages.Add(userImage);
                }

                await _context.SaveChangesAsync();

                // Return success response
                var profileImageInfo = new UserImageInfoDTO
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt,
                    IsActive = userImage.IsActive
                };

                return Ok(new UpdateUserImageResponseDTO
                {
                    UserId = user.Id,
                    Message = "Profile image uploaded successfully",
                    ProfileImage = profileImageInfo,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UploadProfileImage: {Message}", ex.Message);
                
                return StatusCode(500, new { error = "An unexpected error occurred while uploading the image. Please try again.", code = "UPLOAD_ERROR" });
            }
        }

        // Get user profile image
        [HttpGet("image")]
        [Authorize]
        [ProducesResponseType(typeof(UserImageInfoDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfileImage()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "User ID not found in token", code = "USER_ID_NOT_FOUND" });
                }

                var userImage = await _context.UserImages
                    .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                    .OrderByDescending(ui => ui.UploadedAt)
                    .FirstOrDefaultAsync();

                if (userImage == null)
                {
                    return NotFound(new { error = "No profile image found", code = "NO_IMAGE_FOUND" });
                }

                var profileImageInfo = new UserImageInfoDTO
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt,
                    IsActive = userImage.IsActive
                };

                return Ok(profileImageInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProfileImage: {Message}", ex.Message);
                return StatusCode(500, new { error = "An unexpected error occurred while retrieving the image.", code = "GET_ERROR" });
            }
        }

        // Update user profile image (PUT)
        [HttpPut("image")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateUserImageResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfileImage([FromForm] IFormFile profileImage, [FromForm] string? altText = null)
        {
            try
            {
                // Validate input
                if (profileImage == null || profileImage.Length == 0)
                {
                    return BadRequest(new { error = "Profile image is required", code = "IMAGE_REQUIRED" });
                }

                if (profileImage.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return BadRequest(new { error = "Image file size cannot exceed 5MB", code = "IMAGE_TOO_LARGE" });
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(profileImage.ContentType?.ToLower()))
                {
                    return BadRequest(new { error = "Only JPEG, PNG, and GIF images are allowed", code = "INVALID_IMAGE_TYPE" });
                }

                // Get user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "User ID not found in token", code = "USER_ID_NOT_FOUND" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", code = "USER_NOT_FOUND" });
                }

                // Get user roles and department
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "user";
                var departmentName = user.Department?.Name ?? "default";

                // Upload image file
                var imageResult = await _imageUploadService.UploadUserImageAsync(profileImage, user, role, departmentName, altText);
                if (!imageResult.Success)
                {
                    return BadRequest(new { error = $"Failed to upload image: {imageResult.ErrorMessage}", code = "UPLOAD_FAILED" });
                }

                // Find existing profile image (regardless of IsActive status)
                var existingUserImage = await _context.UserImages
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IsProfileImage);

                UserImage userImage;
                if (existingUserImage != null)
                {
                    // Update existing record
                    existingUserImage.FileName = imageResult.FileName ?? "profile.jpg";
                    existingUserImage.FilePath = imageResult.FilePath ?? "";
                    existingUserImage.ContentType = imageResult.ContentType ?? "image/jpeg";
                    existingUserImage.FileSize = imageResult.FileSize;
                    existingUserImage.AltText = imageResult.AltText;
                    existingUserImage.IsActive = true; // Ensure it's active
                    existingUserImage.UploadedAt = DateTime.UtcNow;
                    
                    _context.UserImages.Update(existingUserImage);
                    userImage = existingUserImage;
                }
                else
                {
                    // Create new user image record
                    userImage = new UserImage
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

                    _context.UserImages.Add(userImage);
                }

                await _context.SaveChangesAsync();

                // Return success response
                var profileImageInfo = new UserImageInfoDTO
                {
                    Id = userImage.Id,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    IsProfileImage = userImage.IsProfileImage,
                    UploadedAt = userImage.UploadedAt,
                    IsActive = userImage.IsActive
                };

                return Ok(new UpdateUserImageResponseDTO
                {
                    UserId = user.Id,
                    Message = "Profile image updated successfully",
                    ProfileImage = profileImageInfo,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateProfileImage: {Message}", ex.Message);
                
                return StatusCode(500, new { error = "An unexpected error occurred while updating the image. Please try again.", code = "UPDATE_ERROR" });
            }
        }

        // Delete user profile image
        [HttpDelete("image")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteUserImageResponseDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProfileImage()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "User ID not found in token", code = "USER_ID_NOT_FOUND" });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { error = "User not found", code = "USER_NOT_FOUND" });
                }

                var existingImages = await _context.UserImages
                    .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                    .ToListAsync();

                if (!existingImages.Any())
                {
                    return NotFound(new { error = "No profile image found to delete", code = "NO_IMAGE_FOUND" });
                }

                // Deactivate all profile images
                foreach (var existingImage in existingImages)
                {
                    existingImage.IsActive = false;
                    _context.UserImages.Update(existingImage);
                }

                await _context.SaveChangesAsync();

                return Ok(new DeleteUserImageResponseDTO
                {
                    UserId = user.Id,
                    Message = "Profile image deleted successfully",
                    DeletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while deleting the profile image",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}