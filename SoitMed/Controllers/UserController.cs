using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
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
		public UserController(UserManager<ApplicationUser> _userManager, Context _context)
		{
			userManager = _userManager;
			context = _context;
		}
		[HttpPost]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> CreateUser(RegisterUserDTO userDTO)
		{
			if (ModelState.IsValid)
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

				ApplicationUser user = new ApplicationUser
				{
					UserName = userDTO.UserName,
					Email = userDTO.Email,
					PasswordHash = userDTO.Password,
				};

				IdentityResult result = await userManager.CreateAsync(user, userDTO.Password);
				if (result.Succeeded)
				{
					// Assign the specified role
					await userManager.AddToRoleAsync(user, userDTO.Role);
					return Ok($"User {user.UserName} created successfully with role: {userDTO.Role}");
				}
				else
				{
					return BadRequest(result.Errors);
				}
			}
			else
			{
				return BadRequest(ModelState);
			}
		}
		[HttpGet]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public IActionResult GetUsers()
		{
			var users = userManager.Users.ToList();
			return Ok(users);
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
				user.UserName = userDTO.UserName;
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
				EmailConfirmed = user.EmailConfirmed,
				PhoneNumberConfirmed = user.PhoneNumberConfirmed,
				PhoneNumber = user.PhoneNumber
			};

			return Ok(userData);
		}

		// Get user data by ID
		[HttpGet("{id}")]
		[Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
		public async Task<IActionResult> GetUserById(string id)
		{
			var user = await context.Users
				.Include(u => u.Department)
				.FirstOrDefaultAsync(u => u.Id == id);

			if (user == null)
			{
				return NotFound($"User with ID {id} not found");
			}

			var roles = await userManager.GetRolesAsync(user);

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
				DepartmentDescription = user.Department?.Description
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
				DepartmentDescription = user.Department?.Description
			};

			return Ok(userData);
		}

		// Get all users with detailed data (improved version of existing GetUsers)
		[HttpGet("all")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> GetAllUsersData()
		{
			var users = await context.Users
				.Include(u => u.Department)
				.ToListAsync();

			var usersData = new List<UserDataDTO>();

			foreach (var user in users)
			{
				var roles = await userManager.GetRolesAsync(user);

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
					DepartmentDescription = user.Department?.Description
				});
			}

			return Ok(usersData);
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
						DepartmentDescription = userWithDepartment.Department?.Description
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
					DepartmentDescription = user.Department?.Description
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

	}
}
