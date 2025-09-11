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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SoitMed.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly IConfiguration config;
		private readonly Context context;
		private readonly UserIdGenerationService userIdGenerationService;
		public AccountController(UserManager<ApplicationUser> _userManager, IConfiguration config, Context _context, UserIdGenerationService _userIdGenerationService)
		{
		userManager = _userManager;
	    this.config = config;
		context = _context;
		userIdGenerationService = _userIdGenerationService;
		}
		[HttpPost("register")]
		public async Task< IActionResult> Registe(RegisterUserDTO userDTO)
		{
			if(ModelState.IsValid)
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

				// Auto-assign department based on role if not provided or invalid
				int? departmentId = userDTO.DepartmentId;
				if (departmentId == null || departmentId <= 0)
				{
					var departmentName = UserRoles.GetDepartmentForRole(userDTO.Role);
					var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName);
					if (department == null)
					{
						return BadRequest($"Department '{departmentName}' for role '{userDTO.Role}' not found. Please ensure departments are seeded.");
					}
					departmentId = department.Id;
				}
				else
				{
					// Validate that the provided department exists
					var departmentExists = await context.Departments.AnyAsync(d => d.Id == departmentId);
					if (!departmentExists)
					{
						return BadRequest($"Department with ID {departmentId} does not exist.");
					}
				}

				// Generate custom user ID
				string customUserId = await userIdGenerationService.GenerateUserIdAsync(
					userDTO.FirstName ?? "Unknown",
					userDTO.LastName ?? "User", 
					userDTO.Role, 
					departmentId
				);

				ApplicationUser AppUser = new ApplicationUser()
				{
					Id = customUserId,
					UserName = userDTO.Email, // Use email as username
					Email = userDTO.Email,
					PasswordHash = userDTO.Password,
					FirstName = userDTO.FirstName,
					LastName = userDTO.LastName,
					DepartmentId = departmentId,
					CreatedAt = DateTime.UtcNow,
					IsActive = true
				};
			 IdentityResult Result=	await userManager.CreateAsync(AppUser,userDTO.Password);
				if (Result.Succeeded)
				{
					// Assign the specified role instead of hardcoded "Admin"
					await userManager.AddToRoleAsync(AppUser, userDTO.Role);
					return Ok($"Account Created with role: {userDTO.Role}");
				}
				return BadRequest(Result.Errors);
			}
			return BadRequest(ModelState);
		}

		[HttpPost("login")]
		public async Task< IActionResult> Login(LoginUserDTO userDTO)
		{
			if(ModelState.IsValid)
			{
				// Try to find user by username first, then by email
				ApplicationUser? UserFromDB = await userManager.FindByNameAsync(userDTO.UserName);
				
				// If not found by username, try by email
				if (UserFromDB == null)
				{
					UserFromDB = await userManager.FindByEmailAsync(userDTO.UserName);
				}

				if (UserFromDB != null)
				{
					bool found= await userManager.CheckPasswordAsync(UserFromDB,userDTO.Password);
					if (found)
					{
						//Create Token
						List<Claim> myclaims = new List<Claim>();
						myclaims.Add(new Claim(ClaimTypes.Name,UserFromDB.UserName ?? ""));
						myclaims.Add(new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id));
						myclaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

						var roles=await userManager.GetRolesAsync(UserFromDB);
						foreach (var role in roles)
						{
							myclaims.Add(new Claim(ClaimTypes.Role, role));
						}

						var SignKey = new SymmetricSecurityKey(
						   Encoding.UTF8.GetBytes(config["JWT:SecritKey"] ?? ""));

						SigningCredentials signingCredentials =
							new SigningCredentials(SignKey, SecurityAlgorithms.HmacSha256);

						JwtSecurityToken mytoken = new JwtSecurityToken(
						   issuer: config["JWT:ValidIss"],//provider create token
						   audience: config["JWT:ValidAud"],//cousumer url
						expires: DateTime.Now.AddYears(5), // Extended to 5 years
						   claims: myclaims,
						   signingCredentials: signingCredentials);
						return Ok(new
						{
							token = new JwtSecurityTokenHandler().WriteToken(mytoken),
							expired = mytoken.ValidTo
						});
					}
				}
				return BadRequest("Invalid Request");
			}
			return BadRequest(ModelState);
		}

		[HttpGet("departments")]
		public async Task<IActionResult> GetDepartments()
		{
			var departments = await context.Departments
				.Select(d => new { d.Id, d.Name, d.Description })
				.ToListAsync();
			
			return Ok(departments);
		}

		[HttpGet("roles")]
		public IActionResult GetRoles()
		{
			var roles = UserRoles.GetAllRoles();
			var rolesByDepartment = UserRoles.GetRolesByDepartment();
			
			return Ok(new
			{
				AllRoles = roles,
				RolesByDepartment = rolesByDepartment
			});
		}

		[HttpPost("change-password")]
		[Authorize] // User must be authenticated to change password
		public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				// Get current user from JWT token
				var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
				if (string.IsNullOrEmpty(userEmail))
				{
					return Unauthorized("User not found in token.");
				}

				// Find user by email (since email is now the username)
				var user = await userManager.FindByEmailAsync(userEmail);
				if (user == null)
				{
					return NotFound("User not found.");
				}

				// Verify current password
				var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, changePasswordDTO.CurrentPassword);
				if (!isCurrentPasswordValid)
				{
					return BadRequest(new
					{
						success = false,
						message = "Current password is incorrect."
					});
				}

				// Change password
				var result = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
				
				if (result.Succeeded)
				{
					return Ok(new
					{
						success = true,
						message = "Password changed successfully."
					});
				}
				else
				{
					return BadRequest(new
					{
						success = false,
						message = "Failed to change password.",
						errors = result.Errors.Select(e => e.Description).ToList()
					});
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "An error occurred while changing password.",
					error = ex.Message
				});
			}
		}

	}
}
