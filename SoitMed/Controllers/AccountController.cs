using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
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
		public AccountController(UserManager<ApplicationUser> _userManager, IConfiguration config, Context _context)
		{
		userManager = _userManager;
	    this.config = config;
		context = _context;
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

				// Auto-assign department based on role if not provided
				int? departmentId = userDTO.DepartmentId;
				if (departmentId == null)
				{
					var departmentName = UserRoles.GetDepartmentForRole(userDTO.Role);
					var department = await context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName);
					departmentId = department?.Id;
				}

				ApplicationUser AppUser = new ApplicationUser()
				{
					UserName = userDTO.UserName,
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
				ApplicationUser? UserFromDB= await userManager.FindByNameAsync(userDTO.UserName);
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

	}
}
