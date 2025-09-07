using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoleController : ControllerBase
	{
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly Context context;
		public RoleController(RoleManager<IdentityRole> _roleManager, Context _context)
		{
			roleManager = _roleManager;
			context = _context;
		}
		[HttpPost]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> CreateRole(string roleName)
		{
			bool roleExist = await roleManager.RoleExistsAsync(roleName);

			if (!roleExist)
			{
				IdentityResult result = await roleManager.CreateAsync(new IdentityRole { Name = roleName });
				if (result.Succeeded)
				{
					return Ok($"Role {roleName} created successfully");
				}
				else
				{
					return BadRequest(result.Errors);
				}

			}
			else
			{
				return BadRequest($"Role {roleName} already exists");
			}

		}
		[HttpGet]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public IActionResult GetRoles()
		{
			IQueryable<IdentityRole> roles = roleManager.Roles;
			return Ok(roles);
		}

		[HttpGet("available")]
		public IActionResult GetAvailableRoles()
		{
			var availableRoles = UserRoles.GetAllRoles();
			return Ok(new { roles = availableRoles });
		}

		// New dedicated role management endpoints for business roles
		[HttpGet("business")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> GetBusinessRoles()
		{
			var roles = await context.BusinessRoles
				.Select(r => new RoleResponseDTO
				{
					RoleId = r.RoleId,
					RoleName = r.RoleName,
					Description = r.Description,
					CreatedAt = r.CreatedAt,
					IsActive = r.IsActive
				})
				.ToListAsync();

			return Ok(roles);
		}

		[HttpGet("business/{id}")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public async Task<IActionResult> GetBusinessRole(int id)
		{
			var role = await context.BusinessRoles.FindAsync(id);
			if (role == null)
			{
				return NotFound($"Role with ID {id} not found");
			}

			var response = new RoleResponseDTO
			{
				RoleId = role.RoleId,
				RoleName = role.RoleName,
				Description = role.Description,
				CreatedAt = role.CreatedAt,
				IsActive = role.IsActive
			};

			return Ok(response);
		}

		// Required API endpoint: POST /roles
		[HttpPost("business")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> CreateBusinessRole(RoleDTO roleDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Check if role already exists
			if (await context.BusinessRoles.AnyAsync(r => r.RoleName == roleDTO.RoleName))
			{
				return BadRequest($"Role '{roleDTO.RoleName}' already exists");
			}

			var role = new SoitMed.Models.Core.Role
			{
				RoleName = roleDTO.RoleName,
				Description = roleDTO.Description,
				CreatedAt = DateTime.UtcNow,
				IsActive = true
			};

			context.BusinessRoles.Add(role);
			await context.SaveChangesAsync();

			return Ok($"Role '{role.RoleName}' created successfully");
		}

		// Required API endpoint: PUT /roles/{id}
		[HttpPut("business/{id}")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> UpdateBusinessRole(int id, RoleDTO roleDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var role = await context.BusinessRoles.FindAsync(id);
			if (role == null)
			{
				return NotFound($"Role with ID {id} not found");
			}

			// Check if new name conflicts with existing roles
			if (await context.BusinessRoles.AnyAsync(r => r.RoleName == roleDTO.RoleName && r.RoleId != id))
			{
				return BadRequest($"Role '{roleDTO.RoleName}' already exists");
			}

			role.RoleName = roleDTO.RoleName;
			role.Description = roleDTO.Description;

			await context.SaveChangesAsync();

			return Ok($"Role '{role.RoleName}' updated successfully");
		}

		[HttpDelete("business/{id}")]
		[Authorize(Roles = "SuperAdmin")]
		public async Task<IActionResult> DeleteBusinessRole(int id)
		{
			var role = await context.BusinessRoles.FindAsync(id);
			if (role == null)
			{
				return NotFound($"Role with ID {id} not found");
			}

			role.IsActive = false;

			await context.SaveChangesAsync();

			return Ok($"Role '{role.RoleName}' deactivated successfully");
		}
		[HttpDelete]
		[Authorize(Roles = "SuperAdmin")]
		public async Task< IActionResult> DeleteRole(string roleName)
		{
			var role= await roleManager.FindByNameAsync(roleName);
			if(role != null)
			{
				IdentityResult result= await roleManager.DeleteAsync(role);
				if(result.Succeeded)
				{
					return Ok($"Role {roleName} deleted successfully");
				}
				else
				{
					return BadRequest(result.Errors);
				}

			}
			else
			{
				return BadRequest($"Role {roleName} not found");
			}
		}
		[HttpPut]
		[Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateRole(string roleName, string newRoleName)
        {
            IdentityRole? role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {

                role.Name = newRoleName;
                IdentityResult result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return Ok($"Role {roleName} updated to {newRoleName} successfully");
                }
                else
                {
                    return BadRequest(result.Errors);
                }

            }
            else
            {
                return NotFound($"Role {roleName} not found");
            }
        }
	}
}
