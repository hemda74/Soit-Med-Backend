using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SoitMed.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly Context context;
		private readonly UserIdGenerationService userIdGenerationService;
		private readonly IRoleBasedImageUploadService _imageUploadService;
		
		public UserController(UserManager<ApplicationUser> _userManager, Context _context, UserIdGenerationService _userIdGenerationService, IRoleBasedImageUploadService imageUploadService)
		{
			userManager = _userManager;
			context = _context;
			userIdGenerationService = _userIdGenerationService;
			_imageUploadService = imageUploadService;
		}

		// Helper method to get user profile image
		private async Task<UserImageInfoDTO?> GetUserProfileImageAsync(string userId)
		{
			var profileImage = await context.UserImages
				.Where(ui => ui.UserId == userId && ui.IsActive && ui.IsProfileImage)
				.OrderByDescending(ui => ui.UploadedAt)
				.FirstOrDefaultAsync();

			if (profileImage == null)
				return null;

			return new UserImageInfoDTO
			{
				Id = profileImage.Id,
				FileName = profileImage.FileName,
				FilePath = profileImage.FilePath,
				ContentType = profileImage.ContentType,
				FileSize = profileImage.FileSize,
				AltText = profileImage.AltText,
				IsProfileImage = profileImage.IsProfileImage,
				UploadedAt = profileImage.UploadedAt,
				IsActive = profileImage.IsActive
			};
		}

		// OPTIMIZED: Bulk load users data with roles and profile images
		private async Task<List<UserDataDTO>> GetOptimizedUsersDataAsync(List<ApplicationUser> users)
		{
			if (!users.Any())
				return new List<UserDataDTO>();

			var userIds = users.Select(u => u.Id).ToList();

			// OPTIMIZED: Load all roles for all users in a single query
			var userRoles = await context.UserRoles
				.Where(ur => userIds.Contains(ur.UserId))
				.Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
				.ToListAsync();

			// OPTIMIZED: Load all profile images for all users in a single query
			var profileImages = await context.UserImages
				.Where(ui => userIds.Contains(ui.UserId) && ui.IsActive && ui.IsProfileImage)
				.GroupBy(ui => ui.UserId)
				.Select(g => g.OrderByDescending(ui => ui.UploadedAt).First())
				.ToListAsync();

			// Group roles by user ID for efficient lookup
			var rolesByUserId = userRoles
				.GroupBy(ur => ur.UserId)
				.ToDictionary(g => g.Key, g => g.Select(ur => ur.Name).ToList());

			// Group profile images by user ID for efficient lookup
			var imagesByUserId = profileImages
				.ToDictionary(ui => ui.UserId, ui => new UserImageInfoDTO
				{
					Id = ui.Id,
					FileName = ui.FileName,
					FilePath = ui.FilePath,
					ContentType = ui.ContentType,
					FileSize = ui.FileSize,
					AltText = ui.AltText,
					IsProfileImage = ui.IsProfileImage,
					UploadedAt = ui.UploadedAt,
					IsActive = ui.IsActive
				});

			// Build the result efficiently
			var usersData = new List<UserDataDTO>();
			foreach (var user in users)
			{
				var roles = rolesByUserId.GetValueOrDefault(user.Id, new List<string>());
				var profileImage = imagesByUserId.GetValueOrDefault(user.Id);

				usersData.Add(new UserDataDTO
				{
					Id = user.Id,
					UserName = user.UserName ?? "",
					Email = user.Email ?? "",
					FirstName = user.FirstName,
					LastName = user.LastName,
					FullName = user.FullName,
					IsActive = user.IsActive,
					CreatedAt = user.CreatedAt,
					LastLoginAt = user.LastLoginAt,
					Roles = roles,
					DepartmentId = user.DepartmentId,
					DepartmentName = user.Department?.Name,
					DepartmentDescription = user.Department?.Description,
					ProfileImage = profileImage,
					PhoneNumber = user.PhoneNumber,
					PhoneNumberConfirmed = user.PhoneNumberConfirmed
				});
			}

			return usersData;
		}

		// Helper method to safely upsert user profile image
		private async Task<UserImage> UpsertUserProfileImageAsync(string userId, string fileName, string filePath, string contentType, long fileSize, string? altText)
		{
			using var transaction = await context.Database.BeginTransactionAsync();
			try
			{
				// First, deactivate any existing profile images for this user
				var existingImages = await context.UserImages
					.Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
					.ToListAsync();

				foreach (var existingImage in existingImages)
				{
					existingImage.IsActive = false;
					context.UserImages.Update(existingImage);
				}

				// Create new user image record
				var userImage = new UserImage
				{
					UserId = userId,
					FileName = fileName,
					FilePath = filePath,
					ContentType = contentType,
					FileSize = fileSize,
					AltText = altText,
					ImageType = "Profile",
					IsProfileImage = true,
					IsActive = true,
					UploadedAt = DateTime.UtcNow
				};

				context.UserImages.Add(userImage);
				await context.SaveChangesAsync();
				await transaction.CommitAsync();

				return userImage;
			}
			catch (Exception)
			{
				await transaction.RollbackAsync();
				throw;
			}
		}
		[HttpDelete("{userId}")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> DeleteUser(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest(new
				{
					success = false,
					message = "User ID is required",
					timestamp = DateTime.UtcNow
				});
			}

			// Find user by ID
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new
				{
					success = false,
					message = $"User with ID '{userId}' not found",
					timestamp = DateTime.UtcNow
				});
			}

			// Check if user is SuperAdmin (prevent deleting SuperAdmin)
			var roles = await userManager.GetRolesAsync(user);
			if (roles.Contains("SuperAdmin"))
			{
				return BadRequest(new
				{
					success = false,
					message = "Cannot delete SuperAdmin user",
					timestamp = DateTime.UtcNow
				});
			}

			// Check if user is trying to delete themselves
			var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (currentUserId == userId)
			{
				return BadRequest(new
				{
					success = false,
					message = "Cannot delete your own account",
					timestamp = DateTime.UtcNow
				});
			}

			// Delete user
			IdentityResult result = await userManager.DeleteAsync(user);
			if (result.Succeeded)
			{
				return Ok(new
				{
					success = true,
					message = $"User '{user.UserName}' deleted successfully",
					deletedUserId = userId,
					deletedUserName = user.UserName,
					timestamp = DateTime.UtcNow
				});
			}
			else
			{
				return BadRequest(new
				{
					success = false,
					message = "Failed to delete user",
					errors = result.Errors.Select(e => e.Description).ToList(),
					timestamp = DateTime.UtcNow
				});
			}
		}
		[HttpPatch("{userId}")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> UpdateUser(string userId, UpdateUserDTO userDTO)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest(new
				{
					success = false,
					message = "User ID is required",
					timestamp = DateTime.UtcNow
				});
			}

			// Check if role is provided
			if (string.IsNullOrEmpty(userDTO.Role))
			{
				return BadRequest(new
				{
					success = false,
					message = "Role field is required",
					timestamp = DateTime.UtcNow
				});
			}

			// Validate the role
			if (!UserRoles.IsValidRole(userDTO.Role))
			{
				return BadRequest(new
				{
					success = false,
					message = $"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}",
					timestamp = DateTime.UtcNow
				});
			}

			// Find user by ID
			ApplicationUser? user = await userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new
				{
					success = false,
					message = $"User with ID '{userId}' not found",
					timestamp = DateTime.UtcNow
				});
			}

			// Check if user is SuperAdmin (prevent updating SuperAdmin)
			var currentRoles = await userManager.GetRolesAsync(user);
			if (currentRoles.Contains("SuperAdmin"))
			{
				return BadRequest(new
				{
					success = false,
					message = "Cannot update SuperAdmin user",
					timestamp = DateTime.UtcNow
				});
			}

			// Update user properties
			user.UserName = userDTO.Email; // Use email as username
			user.Email = userDTO.Email;
			user.FirstName = userDTO.FirstName;
			user.LastName = userDTO.LastName;
			user.PhoneNumber = userDTO.PhoneNumber;

			// Update user
			IdentityResult result = await userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				// Update user roles - remove all current roles and add the new one
				if (currentRoles.Any())
				{
					await userManager.RemoveFromRolesAsync(user, currentRoles);
				}
				await userManager.AddToRoleAsync(user, userDTO.Role);

				return Ok(new
				{
					success = true,
					message = $"User '{user.UserName}' updated successfully",
					updatedUserId = userId,
					updatedUserName = user.UserName,
					newRole = userDTO.Role,
					timestamp = DateTime.UtcNow
				});
			}
			else
			{
				return BadRequest(new
				{
					success = false,
					message = "Failed to update user",
					errors = result.Errors.Select(e => e.Description).ToList(),
					timestamp = DateTime.UtcNow
				});
			}
		}

		// Get current logged-in user's data
		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetCurrentUserData()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID not found in token");
			}

			var user = await context.Users
				.Include(u => u.Department)
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
			{
				return NotFound("User not found");
			}

			var roles = await userManager.GetRolesAsync(user);
			var profileImage = await GetUserProfileImageAsync(user.Id);

		var userData = new CurrentUserDataDTO
		{
			Id = user.Id,
			UserName = user.UserName ?? "",
			Email = user.Email ?? "",
			FirstName = user.FirstName,
			LastName = user.LastName,
			FullName = user.FullName,
			IsActive = user.IsActive,
			CreatedAt = user.CreatedAt,
			LastLoginAt = user.LastLoginAt,
			Roles = roles.ToList(),
			DepartmentId = user.DepartmentId,
			DepartmentName = user.Department?.Name,
			DepartmentDescription = user.Department?.Description,
			ProfileImage = profileImage,
			PhoneNumber = user.PhoneNumber,
			PersonalMail = user.PersonalMail,
			DateOfBirth = user.DateOfBirth
		};

		return Ok(userData);
	}

	// Get current user's profile completion status
	[HttpGet("profile-completion")]
	[Authorize]
	public async Task<IActionResult> GetProfileCompletion()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			return BadRequest("User ID not found in token");
		}

		var user = await context.Users
			.Include(u => u.Department)
			.FirstOrDefaultAsync(u => u.Id == userId);

		if (user == null)
		{
			return NotFound("User not found");
		}

		// Get user profile image
		var profileImage = await GetUserProfileImageAsync(user.Id);

		// Define fields to check (5 fields for all user types)
		var checks = new List<(string FieldName, bool IsComplete)>
		{
			("Profile Image", profileImage != null),
			("First Name", !string.IsNullOrWhiteSpace(user.FirstName)),
			("Last Name", !string.IsNullOrWhiteSpace(user.LastName)),
			("Phone Number", !string.IsNullOrWhiteSpace(user.PhoneNumber)),
			("Date of Birth", user.DateOfBirth.HasValue)
		};

		// Calculate completion stats
		var completedFields = checks.Where(c => c.IsComplete).Select(c => c.FieldName).ToList();
		var missingFields = checks.Where(c => !c.IsComplete).Select(c => c.FieldName).ToList();
		var completedSteps = completedFields.Count;
		var totalSteps = checks.Count;
		var remainingSteps = totalSteps - completedSteps;
		var progress = totalSteps > 0 ? (int)Math.Round((double)completedSteps / totalSteps * 100) : 0;

		var result = new ProfileCompletionDTO
		{
			Progress = progress,
			CompletedSteps = completedSteps,
			TotalSteps = totalSteps,
			RemainingSteps = remainingSteps,
			CompletedFields = completedFields,
			MissingFields = missingFields
		};

		return Ok(result);
	}

	// Update current user's own profile
	[HttpPatch("me")]
	[Authorize]
	public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileDTO updateDTO)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId))
		{
			return BadRequest("User ID not found in token");
		}

		var user = await userManager.FindByIdAsync(userId);
		if (user == null)
		{
			return NotFound("User not found");
		}

		// Update fields if provided
		if (!string.IsNullOrWhiteSpace(updateDTO.FirstName))
			user.FirstName = updateDTO.FirstName;

		if (!string.IsNullOrWhiteSpace(updateDTO.LastName))
			user.LastName = updateDTO.LastName;

		if (!string.IsNullOrWhiteSpace(updateDTO.PhoneNumber))
			user.PhoneNumber = updateDTO.PhoneNumber;

		if (!string.IsNullOrWhiteSpace(updateDTO.PersonalMail))
			user.PersonalMail = updateDTO.PersonalMail;

		if (updateDTO.DateOfBirth.HasValue)
			user.DateOfBirth = updateDTO.DateOfBirth;

		var result = await userManager.UpdateAsync(user);

		if (result.Succeeded)
		{
			return Ok(new
			{
				success = true,
				message = "Profile updated successfully",
				timestamp = DateTime.UtcNow
			});
		}

		return BadRequest(new
		{
			success = false,
			message = "Failed to update profile",
			errors = result.Errors.Select(e => e.Description).ToList(),
			timestamp = DateTime.UtcNow
		});
	}


	// Get user data by username
	[HttpGet("username/{username}")]
	[Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
	public async Task<IActionResult> GetUserByUsername(string username)
		{
			var user = await context.Users
				.Include(u => u.Department)
				.FirstOrDefaultAsync(u => u.UserName == username);

			if (user == null)
			{
				return NotFound($"User with username {username} not found");
			}

			var roles = await userManager.GetRolesAsync(user);
			var profileImage = await GetUserProfileImageAsync(user.Id);

			var userData = new UserDataDTO
			{
				Id = user.Id,
				UserName = user.UserName ?? "",
				Email = user.Email ?? "",
				FirstName = user.FirstName,
				LastName = user.LastName,
				FullName = user.FullName,
				IsActive = user.IsActive,
				CreatedAt = user.CreatedAt,
				LastLoginAt = user.LastLoginAt,
				Roles = roles.ToList(),
				DepartmentId = user.DepartmentId,
				DepartmentName = user.Department?.Name,
				DepartmentDescription = user.Department?.Description,
				ProfileImage = profileImage,
				PhoneNumber = user.PhoneNumber,
				PhoneNumberConfirmed = user.PhoneNumberConfirmed
			};

			return Ok(userData);
		}

		// Get all users with detailed data (optimized version with eager loading)
		[HttpGet("all")]
		[Authorize(Roles = "SuperAdmin,Admin,Doctor")]
		public async Task<IActionResult> GetAllUsersData([FromQuery] UserFilterDTO filter)
		{
			// If role filter is specified, use the role-specific approach
			if (!string.IsNullOrEmpty(filter.Role))
			{
				// Validate role
				if (!UserRoles.IsValidRole(filter.Role))
				{
					return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
				}

				// Get all users with the specified role using optimized query
				var usersInRole = await userManager.GetUsersInRoleAsync(filter.Role);
				var userIds = usersInRole.Select(u => u.Id).ToList();

				// Build optimized query with eager loading
				var query = context.Users
					.Include(u => u.Department)
					.Where(u => userIds.Contains(u.Id))
					.AsQueryable();

				// Apply other filters
				if (!string.IsNullOrEmpty(filter.SearchTerm))
				{
					var searchTerm = filter.SearchTerm.ToLower();
					query = query.Where(u => 
						(u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
						(u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
						(u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
						(u.UserName != null && u.UserName.ToLower().Contains(searchTerm))
					);
				}

				if (filter.DepartmentId.HasValue)
				{
					query = query.Where(u => u.DepartmentId == filter.DepartmentId.Value);
				}

				if (filter.IsActive.HasValue)
				{
					query = query.Where(u => u.IsActive == filter.IsActive.Value);
				}

				if (filter.CreatedFrom.HasValue)
				{
					query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
				}

				if (filter.CreatedTo.HasValue)
				{
					query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
				}

				// Apply sorting
				query = filter.SortBy?.ToLower() switch
				{
					"firstname" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
					"lastname" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
					"email" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
					"isactive" => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.IsActive) : query.OrderByDescending(u => u.IsActive),
					"createdat" or _ => filter.SortOrder?.ToLower() == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt)
				};

				// Get total count before pagination
				var totalCount = await query.CountAsync();

				// Apply pagination
				var users = await query
					.Skip((filter.PageNumber - 1) * filter.PageSize)
					.Take(filter.PageSize)
					.ToListAsync();

				// OPTIMIZED: Load all roles and profile images in bulk
				var allUsersData = await GetOptimizedUsersDataAsync(users);

				var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

				var response = new PaginatedUserResponseDTO
				{
					Users = allUsersData,
					TotalCount = totalCount,
					PageNumber = filter.PageNumber,
					PageSize = filter.PageSize,
					TotalPages = totalPages,
					HasPreviousPage = filter.PageNumber > 1,
					HasNextPage = filter.PageNumber < totalPages,
					AppliedFilters = filter
				};

				return Ok(response);
			}

			// Original logic for when no role filter is specified - OPTIMIZED
			var baseQuery = context.Users
				.Include(u => u.Department)
				.AsQueryable();

			// Apply filters
			if (!string.IsNullOrEmpty(filter.SearchTerm))
			{
				var searchTerm = filter.SearchTerm.ToLower();
				baseQuery = baseQuery.Where(u => 
					(u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
					(u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
					(u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
					(u.UserName != null && u.UserName.ToLower().Contains(searchTerm))
				);
			}

			if (filter.DepartmentId.HasValue)
			{
				baseQuery = baseQuery.Where(u => u.DepartmentId == filter.DepartmentId.Value);
			}

			if (filter.IsActive.HasValue)
			{
				baseQuery = baseQuery.Where(u => u.IsActive == filter.IsActive.Value);
			}

			if (filter.CreatedFrom.HasValue)
			{
				baseQuery = baseQuery.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
			}

			if (filter.CreatedTo.HasValue)
			{
				baseQuery = baseQuery.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
			}

			// Apply sorting
			baseQuery = filter.SortBy?.ToLower() switch
			{
				"firstname" => filter.SortOrder?.ToLower() == "asc" ? baseQuery.OrderBy(u => u.FirstName) : baseQuery.OrderByDescending(u => u.FirstName),
				"lastname" => filter.SortOrder?.ToLower() == "asc" ? baseQuery.OrderBy(u => u.LastName) : baseQuery.OrderByDescending(u => u.LastName),
				"email" => filter.SortOrder?.ToLower() == "asc" ? baseQuery.OrderBy(u => u.Email) : baseQuery.OrderByDescending(u => u.Email),
				"isactive" => filter.SortOrder?.ToLower() == "asc" ? baseQuery.OrderBy(u => u.IsActive) : baseQuery.OrderByDescending(u => u.IsActive),
				"createdat" or _ => filter.SortOrder?.ToLower() == "asc" ? baseQuery.OrderBy(u => u.CreatedAt) : baseQuery.OrderByDescending(u => u.CreatedAt)
			};

			// Get total count before pagination
			var baseTotalCount = await baseQuery.CountAsync();

			// Apply pagination
			var baseUsers = await baseQuery
				.Skip((filter.PageNumber - 1) * filter.PageSize)
				.Take(filter.PageSize)
				.ToListAsync();

			// OPTIMIZED: Load all roles and profile images in bulk
			var baseUsersData = await GetOptimizedUsersDataAsync(baseUsers);

			var baseTotalPages = (int)Math.Ceiling((double)baseTotalCount / filter.PageSize);

			var baseResponse = new PaginatedUserResponseDTO
			{
				Users = baseUsersData,
				TotalCount = baseTotalCount,
				PageNumber = filter.PageNumber,
				PageSize = filter.PageSize,
				TotalPages = baseTotalPages,
				HasPreviousPage = filter.PageNumber > 1,
				HasNextPage = filter.PageNumber < baseTotalPages,
				AppliedFilters = filter
			};

			return Ok(baseResponse);
		}

		// Get users by role (OPTIMIZED)
		[HttpGet("role/{role}")]
		[Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
		public async Task<IActionResult> GetUsersByRole(string role)
		{
			// Validate role
			if (!UserRoles.IsValidRole(role))
			{
				return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
			}

			var usersInRole = await userManager.GetUsersInRoleAsync(role);
			var userIds = usersInRole.Select(u => u.Id).ToList();

			// OPTIMIZED: Load all users with departments in a single query
			var users = await context.Users
				.Include(u => u.Department)
				.Where(u => userIds.Contains(u.Id))
				.ToListAsync();

			// OPTIMIZED: Load all roles and profile images in bulk
			var usersData = await GetOptimizedUsersDataAsync(users);

			return Ok(new { Role = role, UserCount = usersData.Count, Users = usersData });
		}

		// Get users by department (OPTIMIZED)
		[HttpGet("department/{departmentId}")]
		[Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
		public async Task<IActionResult> GetUsersByDepartment(int departmentId)
		{
			var department = await context.Departments.FindAsync(departmentId);
			if (department == null)
			{
				return NotFound($"Department with ID {departmentId} not found");
			}

			var users = await context.Users
				.Include(u => u.Department)
				.Where(u => u.DepartmentId == departmentId)
				.ToListAsync();

			// OPTIMIZED: Load all roles and profile images in bulk
			var usersData = await GetOptimizedUsersDataAsync(users);

			return Ok(new
			{
				Department = department.Name,
				DepartmentId = departmentId,
				UserCount = usersData.Count,
				Users = usersData
			});
		}


		// SuperAdmin: Activate or Deactivate User
		[HttpPut("activate-deactivate")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> ActivateDeactivateUser([FromBody] UserActivationDTO activationDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
			}

			// Validate action
			if (activationDTO.Action.ToLower() != "activate" && activationDTO.Action.ToLower() != "deactivate")
			{
				return BadRequest(ValidationHelperService.CreateBusinessLogicError(
					"Action must be either 'activate' or 'deactivate'",
					"Action",
					"INVALID_ACTION"
				));
			}

			// Find user
			var user = await userManager.FindByIdAsync(activationDTO.UserId);
			if (user == null)
			{
				return BadRequest(ValidationHelperService.CreateBusinessLogicError(
					$"User with ID '{activationDTO.UserId}' not found",
					"UserId",
					"USER_NOT_FOUND"
				));
			}

			// Check if user is SuperAdmin (prevent deactivating SuperAdmin)
			var roles = await userManager.GetRolesAsync(user);
			if (roles.Contains("SuperAdmin"))
			{
				return BadRequest(ValidationHelperService.CreateBusinessLogicError(
					"Cannot deactivate SuperAdmin user",
					"UserId",
					"SUPERADMIN_PROTECTION"
				));
			}

			// Update user status
			user.IsActive = activationDTO.Action.ToLower() == "activate";

			var result = await userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var identityErrors = result.Errors.Select(e => e.Description).ToList();
				return BadRequest(ValidationHelperService.CreateMultipleBusinessLogicErrors(
					new Dictionary<string, string> { { "User", string.Join("; ", identityErrors) } },
					"Failed to update user status"
				));
			}

			var response = new UserActivationResponseDTO
			{
				UserId = user.Id,
				UserName = user.UserName ?? "",
				Email = user.Email ?? "",
				IsActive = user.IsActive,
				Action = activationDTO.Action,
				Reason = activationDTO.Reason,
				ActionDate = DateTime.UtcNow,
				Message = $"User '{user.UserName}' has been {activationDTO.Action}d successfully"
			};

			return Ok(response);
		}

		// SuperAdmin: Get User Statistics
		[HttpGet("statistics")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> GetUserStatistics()
		{
			try
			{
				var allUsers = await context.Users.ToListAsync();
				var departments = await context.Departments.ToListAsync();

				// Basic counts
				var totalUsers = allUsers.Count;
				var activeUsers = allUsers.Count(u => u.IsActive);
				var inactiveUsers = totalUsers - activeUsers;

				// Users by role breakdown
				var usersByRole = new Dictionary<string, int>();
				foreach (var role in UserRoles.GetAllRoles())
				{
					var usersInRole = await userManager.GetUsersInRoleAsync(role);
					usersByRole[role] = usersInRole.Count;
				}

				// Users by department breakdown
				var usersByDepartment = new Dictionary<string, int>();
				foreach (var department in departments)
				{
					var count = allUsers.Count(u => u.DepartmentId == department.Id);
					if (count > 0)
					{
						usersByDepartment[department.Name] = count;
					}
				}

				var statistics = new UserStatisticsDTO
				{
					TotalUsers = totalUsers,
					ActiveUsers = activeUsers,
					InactiveUsers = inactiveUsers,
					UsersByRole = usersByRole.Values.Sum(),
					GeneratedAt = DateTime.UtcNow,
					UsersByRoleBreakdown = usersByRole,
					UsersByDepartment = usersByDepartment
				};

				return Ok(statistics);
			}
			catch (Exception ex)
			{
				return BadRequest(ValidationHelperService.CreateBusinessLogicError(
					$"Error generating user statistics: {ex.Message}",
					"Statistics",
					"STATISTICS_ERROR"
				));
			}
		}

		// SuperAdmin: Get User Counts (Simple version)
		[HttpGet("counts")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> GetUserCounts()
		{
			try
			{
				var allUsers = await context.Users.ToListAsync();
				var totalUsers = allUsers.Count;
				var activeUsers = allUsers.Count(u => u.IsActive);

				return Ok(new
				{
					TotalUsers = totalUsers,
					ActiveUsers = activeUsers,
					InactiveUsers = totalUsers - activeUsers,
					GeneratedAt = DateTime.UtcNow
				});
			}
			catch (Exception ex)
			{
				return BadRequest(ValidationHelperService.CreateBusinessLogicError(
					$"Error getting user counts: {ex.Message}",
					"Counts",
					"COUNTS_ERROR"
				));
			}
		}


	}
}
