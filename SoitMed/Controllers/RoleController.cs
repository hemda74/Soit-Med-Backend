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

		

		[HttpGet("fields/{role}")]
		[Authorize(Roles = "SuperAdmin,Admin")]
		public IActionResult GetRoleSpecificFields(string role)
		{
			if (!UserRoles.IsValidRole(role))
			{
				return BadRequest($"Invalid role. Valid roles are: {string.Join(", ", UserRoles.GetAllRoles())}");
			}

			var baseFields = new List<object>
			{
				new { name = "userName", type = "string", required = true, label = "Username" },
				new { name = "email", type = "email", required = true, label = "Email Address" },
				new { name = "password", type = "password", required = true, label = "Password" },
				new { name = "firstName", type = "string", required = false, label = "First Name" },
				new { name = "lastName", type = "string", required = false, label = "Last Name" },
				new { name = "departmentId", type = "number", required = false, label = "Department ID (Optional)" }
			};

			var roleSpecificFields = new List<object>();
			var requiredData = new List<object>();

			switch (role.ToLower())
			{
				case "doctor":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "specialty", type = "string", required = true, label = "Medical Specialty" },
						new { name = "hospitalId", type = "string", required = true, label = "Hospital ID" }
					});
					requiredData.Add(new { endpoint = "/api/Hospital", description = "Get list of hospitals for hospitalId selection" });
					break;

				case "engineer":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "specialty", type = "string", required = true, label = "Engineering Specialty", itemType = (string)null },
						new { name = "governorateIds", type = "array", required = true, label = "Assigned Governorates", itemType = "number" }
					});
					requiredData.Add(new { endpoint = "/api/Governorate", description = "Get list of governorates for governorateIds selection" });
					break;

				case "technician":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "department", type = "string", required = true, label = "Technical Department" },
						new { name = "hospitalId", type = "string", required = true, label = "Hospital ID" }
					});
					requiredData.Add(new { endpoint = "/api/Hospital", description = "Get list of hospitals for hospitalId selection" });
					break;

				case "admin":
					roleSpecificFields.Add(new { name = "accessLevel", type = "string", required = false, label = "Access Level (Optional)" });
					break;

				case "financemanager":
					roleSpecificFields.Add(new { name = "budgetAuthority", type = "string", required = false, label = "Budget Authority (Optional)" });
					break;

				case "legalmanager":
					roleSpecificFields.Add(new { name = "legalSpecialty", type = "string", required = false, label = "Legal Specialty (Optional)" });
					break;

				case "salesman":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "territory", type = "string", required = false, label = "Sales Territory (Optional)" },
						new { name = "salesTarget", type = "string", required = false, label = "Sales Target (Optional)" }
					});
					break;

				case "financeemployee":
				case "legalemployee":
				case "superadmin":
					// These roles only need base fields
					break;

				case "maintenancemanager":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "maintenanceSpecialty", type = "string", required = false, label = "Maintenance Specialty (Optional)" },
						new { name = "certification", type = "string", required = false, label = "Certification (Optional)" }
					});
					break;

				case "maintenancesupport":
					roleSpecificFields.AddRange(new[]
					{
						new { name = "jobTitle", type = "string", required = false, label = "Job Title (Optional)" },
						new { name = "technicalSkills", type = "string", required = false, label = "Technical Skills (Optional)" }
					});
					break;
			}

			return Ok(new
			{
				role = role,
				department = UserRoles.GetDepartmentForRole(role),
				baseFields = baseFields,
				roleSpecificFields = roleSpecificFields,
				requiredData = requiredData,
				createEndpoint = $"/api/RoleSpecificUser/{role.ToLower()}",
				message = $"Fields required to create a {role} user"
			});
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
