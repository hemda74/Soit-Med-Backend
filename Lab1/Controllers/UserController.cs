using Lab1.DTO;
using Lab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lab1.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> userManager;
		public UserController(UserManager<ApplicationUser> _userManager)
		{
			userManager = _userManager;
		}
		[HttpPost]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> CreateUser(RegisterUserDTO userDTO)
		{
			if(ModelState.IsValid)
			{
				// Validate the role
				if (!UserRoles.IsValidRole(userDTO.Role))
				{
					return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
				}

				ApplicationUser user = new ApplicationUser
				{
					UserName = userDTO.UserName,
					Email = userDTO.Email,
					PasswordHash=userDTO.Password,
				};
				
				IdentityResult result=	await userManager.CreateAsync(user,userDTO.Password);
				if(result.Succeeded)
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
		public async Task<IActionResult>DeleteUser(string Name)
		{
		 ApplicationUser user=	await userManager.FindByNameAsync(Name);
			if(user!=null)
			{
				IdentityResult result = await userManager.DeleteAsync(user);
				if(result.Succeeded)
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
		public async Task<IActionResult>UpdateUser(string userName, RegisterUserDTO userDTO)
		{
			// Validate the role
			if (!UserRoles.IsValidRole(userDTO.Role))
			{
				return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
			}

			ApplicationUser user=await userManager.FindByNameAsync(userName);
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


	}
}
