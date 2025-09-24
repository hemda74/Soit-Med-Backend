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
			var userImage = await context.UserImages
				.Where(ui => ui.UserId == userId && ui.IsProfileImage && ui.IsActive)
				.OrderByDescending(ui => ui.UploadedAt)
				.FirstOrDefaultAsync();

			if (userImage == null)
				return null;

			return new UserImageInfoDTO
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
		[HttpDelete]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> DeleteUser(string Name)
		{
			ApplicationUser user = await userManager.FindByNameAsync(Name);
			if (user != null)
			{
				IdentityResult result = await userManager.DeleteAsync(user);
				if (result.Succeeded)
				{
					return Ok($"User {Name} deleted successfully");
				}
				else
				{
					return BadRequest(result.Errors);
				}
			}
			return NotFound($"User with Name {Name} not found");
		}
		[HttpPut]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> UpdateUser(string userName, RegisterUserDTO userDTO)
		{
			// Check if role is provided
			if (string.IsNullOrEmpty(userDTO.Role))
			{
				return BadRequest("Role field is required.");
			}

			// Validate the role
			if (!UserRoles.IsValidRole(userDTO.Role))
			{
				return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
			}

			ApplicationUser user = await userManager.FindByNameAsync(userName);
			if (user != null)
			{
				user.UserName = userDTO.Email; // Use email as username
				user.PasswordHash = userDTO.Password;
				user.Email = userDTO.Email;

				IdentityResult result = await userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					// Update user roles - remove all current roles and add the new one
					var currentRoles = await userManager.GetRolesAsync(user);
					if (currentRoles.Any())
					{
						await userManager.RemoveFromRolesAsync(user, currentRoles);
					}
					await userManager.AddToRoleAsync(user, userDTO.Role);

					return Ok($"User {userName} updated successfully with role: {userDTO.Role}");
				}
				else
				{
					return BadRequest(result.Errors);
				}
			}
			return NotFound($"User with Name {userName} not found");
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
				EmailConfirmed = user.EmailConfirmed,
				PhoneNumberConfirmed = user.PhoneNumberConfirmed,
				PhoneNumber = user.PhoneNumber
			};

			return Ok(userData);
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
				ProfileImage = profileImage
			};

			return Ok(userData);
		}

		// Get all users with detailed data (improved version with filtering and sorting)
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

				// Get all users with the specified role
				var usersInRole = await userManager.GetUsersInRoleAsync(filter.Role);
				var allUsersData = new List<UserDataDTO>();

				foreach (var user in usersInRole)
				{
					var userWithDepartment = await context.Users
						.Include(u => u.Department)
						.FirstOrDefaultAsync(u => u.Id == user.Id);

					if (userWithDepartment != null)
					{
						var roles = await userManager.GetRolesAsync(userWithDepartment);

						// Apply other filters
						if (!string.IsNullOrEmpty(filter.SearchTerm))
						{
							var searchTerm = filter.SearchTerm.ToLower();
							if (!((userWithDepartment.FirstName != null && userWithDepartment.FirstName.ToLower().Contains(searchTerm)) ||
								(userWithDepartment.LastName != null && userWithDepartment.LastName.ToLower().Contains(searchTerm)) ||
								(userWithDepartment.Email != null && userWithDepartment.Email.ToLower().Contains(searchTerm)) ||
								(userWithDepartment.UserName != null && userWithDepartment.UserName.ToLower().Contains(searchTerm))))
							{
								continue;
							}
						}

						if (filter.DepartmentId.HasValue && userWithDepartment.DepartmentId != filter.DepartmentId.Value)
						{
							continue;
						}

						if (filter.IsActive.HasValue && userWithDepartment.IsActive != filter.IsActive.Value)
						{
							continue;
						}

						if (filter.CreatedFrom.HasValue && userWithDepartment.CreatedAt < filter.CreatedFrom.Value)
						{
							continue;
						}

						if (filter.CreatedTo.HasValue && userWithDepartment.CreatedAt > filter.CreatedTo.Value)
						{
							continue;
						}

						var profileImage = await GetUserProfileImageAsync(userWithDepartment.Id);

						allUsersData.Add(new UserDataDTO
						{
							Id = userWithDepartment.Id,
							UserName = userWithDepartment.UserName ?? "",
							Email = userWithDepartment.Email ?? "",
							FirstName = userWithDepartment.FirstName,
							LastName = userWithDepartment.LastName,
							FullName = userWithDepartment.FullName,
							IsActive = userWithDepartment.IsActive,
							CreatedAt = userWithDepartment.CreatedAt,
							LastLoginAt = userWithDepartment.LastLoginAt,
							Roles = roles.ToList(),
							DepartmentId = userWithDepartment.DepartmentId,
							DepartmentName = userWithDepartment.Department?.Name,
							DepartmentDescription = userWithDepartment.Department?.Description,
							ProfileImage = profileImage
						});
					}
				}

				// Apply sorting
				allUsersData = filter.SortBy?.ToLower() switch
				{
					"firstname" => filter.SortOrder?.ToLower() == "asc" ? allUsersData.OrderBy(u => u.FirstName).ToList() : allUsersData.OrderByDescending(u => u.FirstName).ToList(),
					"lastname" => filter.SortOrder?.ToLower() == "asc" ? allUsersData.OrderBy(u => u.LastName).ToList() : allUsersData.OrderByDescending(u => u.LastName).ToList(),
					"email" => filter.SortOrder?.ToLower() == "asc" ? allUsersData.OrderBy(u => u.Email).ToList() : allUsersData.OrderByDescending(u => u.Email).ToList(),
					"isactive" => filter.SortOrder?.ToLower() == "asc" ? allUsersData.OrderBy(u => u.IsActive).ToList() : allUsersData.OrderByDescending(u => u.IsActive).ToList(),
					"createdat" or _ => filter.SortOrder?.ToLower() == "asc" ? allUsersData.OrderBy(u => u.CreatedAt).ToList() : allUsersData.OrderByDescending(u => u.CreatedAt).ToList()
				};

				// Apply pagination
				var roleFilteredTotalCount = allUsersData.Count;
				var roleFilteredUsersData = allUsersData
					.Skip((filter.PageNumber - 1) * filter.PageSize)
					.Take(filter.PageSize)
					.ToList();

				var roleFilteredTotalPages = (int)Math.Ceiling((double)roleFilteredTotalCount / filter.PageSize);

				var roleFilteredResponse = new PaginatedUserResponseDTO
				{
					Users = roleFilteredUsersData,
					TotalCount = roleFilteredTotalCount,
					PageNumber = filter.PageNumber,
					PageSize = filter.PageSize,
					TotalPages = roleFilteredTotalPages,
					HasPreviousPage = filter.PageNumber > 1,
					HasNextPage = filter.PageNumber < roleFilteredTotalPages,
					AppliedFilters = filter
				};

				return Ok(roleFilteredResponse);
			}

			// Original logic for when no role filter is specified
			var query = context.Users
				.Include(u => u.Department)
				.AsQueryable();

			// Apply filters
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

			var usersData = new List<UserDataDTO>();

			foreach (var user in users)
			{
				var roles = await userManager.GetRolesAsync(user);
				var profileImage = await GetUserProfileImageAsync(user.Id);

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
					Roles = roles.ToList(),
					DepartmentId = user.DepartmentId,
					DepartmentName = user.Department?.Name,
					DepartmentDescription = user.Department?.Description,
					ProfileImage = profileImage
				});
			}

			var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

			var response = new PaginatedUserResponseDTO
			{
				Users = usersData,
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

		// Get users by role
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
			var usersData = new List<UserDataDTO>();

			foreach (var user in usersInRole)
			{
				var userWithDepartment = await context.Users
					.Include(u => u.Department)
					.FirstOrDefaultAsync(u => u.Id == user.Id);

				if (userWithDepartment != null)
				{
					var roles = await userManager.GetRolesAsync(userWithDepartment);
					var profileImage = await GetUserProfileImageAsync(userWithDepartment.Id);

					usersData.Add(new UserDataDTO
					{
						Id = userWithDepartment.Id,
						UserName = userWithDepartment.UserName ?? "",
						Email = userWithDepartment.Email ?? "",
						FirstName = userWithDepartment.FirstName,
						LastName = userWithDepartment.LastName,
						FullName = userWithDepartment.FullName,
						IsActive = userWithDepartment.IsActive,
						CreatedAt = userWithDepartment.CreatedAt,
						LastLoginAt = userWithDepartment.LastLoginAt,
						Roles = roles.ToList(),
						DepartmentId = userWithDepartment.DepartmentId,
						DepartmentName = userWithDepartment.Department?.Name,
						DepartmentDescription = userWithDepartment.Department?.Description,
						ProfileImage = profileImage
					});
				}
			}

			return Ok(new { Role = role, UserCount = usersData.Count, Users = usersData });
		}

		// Get users by department
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

			var usersData = new List<UserDataDTO>();

			foreach (var user in users)
			{
				var roles = await userManager.GetRolesAsync(user);
				var profileImage = await GetUserProfileImageAsync(user.Id);

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
					Roles = roles.ToList(),
					DepartmentId = user.DepartmentId,
					DepartmentName = user.Department?.Name,
					DepartmentDescription = user.Department?.Description,
					ProfileImage = profileImage
				});
			}

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
