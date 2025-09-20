using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserImageController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public UserImageController(
            Context context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        /// <summary>
        /// Get all images for the current user
        /// </summary>
        [HttpGet("my-images")]
        public async Task<IActionResult> GetMyImages()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var images = await _context.UserImages
                .Where(ui => ui.UserId == userId && ui.IsActive)
                .OrderByDescending(ui => ui.UploadedAt)
                .Select(ui => new UserImageDTO
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    FileName = ui.FileName,
                    FilePath = ui.FilePath,
                    ContentType = ui.ContentType,
                    FileSize = ui.FileSize,
                    AltText = ui.AltText,
                    UploadedAt = ui.UploadedAt,
                    IsActive = ui.IsActive,
                    IsProfileImage = ui.IsProfileImage
                })
                .ToListAsync();

            return Ok(new
            {
                data = images,
                message = $"Found {images.Count} image(s)",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get profile image for the current user
        /// </summary>
        [HttpGet("my-profile-image")]
        public async Task<IActionResult> GetMyProfileImage()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var profileImage = await _context.UserImages
                .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                .Select(ui => new UserImageDTO
                {
                    Id = ui.Id,
                    UserId = ui.UserId,
                    FileName = ui.FileName,
                    FilePath = ui.FilePath,
                    ContentType = ui.ContentType,
                    FileSize = ui.FileSize,
                    AltText = ui.AltText,
                    UploadedAt = ui.UploadedAt,
                    IsActive = ui.IsActive,
                    IsProfileImage = ui.IsProfileImage
                })
                .FirstOrDefaultAsync();

            if (profileImage == null)
                return NotFound(new
                {
                    message = "No profile image found",
                    timestamp = DateTime.UtcNow
                });

            return Ok(new
            {
                data = profileImage,
                message = "Profile image retrieved successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Upload a new image for the current user
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] UploadUserImageDTO model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
            }

            var file = model.File;
            if (file == null || file.Length == 0)
                return BadRequest(ValidationHelperService.CreateBusinessLogicError("No file provided", "File"));

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(ValidationHelperService.CreateBusinessLogicError("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed", "File"));

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(ValidationHelperService.CreateBusinessLogicError("File size cannot exceed 10MB", "File"));

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "user-images", userId);
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Always unset any existing profile images when uploading a new image
                var existingProfileImages = await _context.UserImages
                    .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                    .ToListAsync();

                foreach (var existingImage in existingProfileImages)
                {
                    existingImage.IsProfileImage = false;
                }

                // Create database record - always set as profile image
                var userImage = new UserImage
                {
                    UserId = userId,
                    FileName = file.FileName,
                    FilePath = Path.Combine("uploads", "user-images", userId, fileName).Replace("\\", "/"),
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    AltText = model.AltText,
                    IsProfileImage = true, // Always set as profile image
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.UserImages.Add(userImage);
                await _context.SaveChangesAsync();

                var response = new UserImageUploadResponseDTO
                {
                    Id = userImage.Id,
                    UserId = userImage.UserId,
                    FileName = userImage.FileName,
                    FilePath = userImage.FilePath,
                    ContentType = userImage.ContentType,
                    FileSize = userImage.FileSize,
                    AltText = userImage.AltText,
                    UploadedAt = userImage.UploadedAt,
                    IsProfileImage = true, // Always true for uploaded images
                    Message = "Image uploaded and set as profile image successfully"
                };

                return Ok(new
                {
                    data = response,
                    message = "Image uploaded and set as profile image successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to upload image",
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Set an existing image as profile image
        /// </summary>
        [HttpPut("{imageId}/set-profile")]
        public async Task<IActionResult> SetAsProfileImage(int imageId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == imageId && ui.UserId == userId && ui.IsActive);

            if (image == null)
                return NotFound(new
                {
                    error = "Image not found",
                    timestamp = DateTime.UtcNow
                });

            try
            {
                // Unset any existing profile image
                var existingProfileImages = await _context.UserImages
                    .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                    .ToListAsync();

                foreach (var existingImage in existingProfileImages)
                {
                    existingImage.IsProfileImage = false;
                }

                // Set this image as profile image
                image.IsProfileImage = true;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Profile image updated successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to update profile image",
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Update image metadata
        /// </summary>
        [HttpPut("{imageId}")]
        public async Task<IActionResult> UpdateImage(int imageId, [FromBody] UpdateUserImageDTO updateDto)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == imageId && ui.UserId == userId && ui.IsActive);

            if (image == null)
                return NotFound(new
                {
                    error = "Image not found",
                    timestamp = DateTime.UtcNow
                });

            try
            {
                if (updateDto.AltText != null)
                    image.AltText = updateDto.AltText;

                if (updateDto.IsProfileImage.HasValue && updateDto.IsProfileImage.Value)
                {
                    // Unset any existing profile image
                    var existingProfileImages = await _context.UserImages
                        .Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
                        .ToListAsync();

                    foreach (var existingImage in existingProfileImages)
                    {
                        existingImage.IsProfileImage = false;
                    }

                    image.IsProfileImage = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Image updated successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to update image",
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [HttpDelete("{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == imageId && ui.UserId == userId && ui.IsActive);

            if (image == null)
                return NotFound(new
                {
                    error = "Image not found",
                    timestamp = DateTime.UtcNow
                });

            try
            {
                // Delete physical file
                var fullPath = Path.Combine(_environment.WebRootPath, image.FilePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                // Soft delete from database
                image.IsActive = false;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Image deleted successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to delete image",
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Serve image file
        /// </summary>
        [HttpGet("serve/{imageId}")]
        public async Task<IActionResult> ServeImage(int imageId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var image = await _context.UserImages
                .FirstOrDefaultAsync(ui => ui.Id == imageId && ui.UserId == userId && ui.IsActive);

            if (image == null)
                return NotFound();

            var fullPath = Path.Combine(_environment.WebRootPath, image.FilePath);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, image.ContentType ?? "application/octet-stream", image.FileName);
        }
    }
}
