using Lab1.DTO;
using Lab1.Models;
using Lab1.Models.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly Context context;

        public DepartmentController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await context.Departments
                .Select(d => new DepartmentResponseDTO
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CreatedAt = d.CreatedAt,
                    UserCount = d.Users.Count()
                })
                .ToListAsync();

            return Ok(departments);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            var response = new DepartmentResponseDTO
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CreatedAt = department.CreatedAt,
                UserCount = department.Users.Count()
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateDepartment(DepartmentDTO departmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if department already exists
            if (await context.Departments.AnyAsync(d => d.Name == departmentDTO.Name))
            {
                return BadRequest($"Department '{departmentDTO.Name}' already exists");
            }

            var department = new Department
            {
                Name = departmentDTO.Name,
                Description = departmentDTO.Description,
                CreatedAt = DateTime.UtcNow
            };

            context.Departments.Add(department);
            await context.SaveChangesAsync();

            return Ok($"Department '{department.Name}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentDTO departmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var department = await context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            // Check if new name conflicts with existing departments
            if (await context.Departments.AnyAsync(d => d.Name == departmentDTO.Name && d.Id != id))
            {
                return BadRequest($"Department '{departmentDTO.Name}' already exists");
            }

            department.Name = departmentDTO.Name;
            department.Description = departmentDTO.Description;

            await context.SaveChangesAsync();

            return Ok($"Department '{department.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            if (department.Users.Any())
            {
                return BadRequest($"Cannot delete department '{department.Name}' because it has {department.Users.Count()} users assigned to it");
            }

            context.Departments.Remove(department);
            await context.SaveChangesAsync();

            return Ok($"Department '{department.Name}' deleted successfully");
        }

        [HttpGet("roles")]
        public IActionResult GetDepartmentRoles()
        {
            var departmentRoles = UserRoles.GetRolesByDepartment();
            return Ok(departmentRoles);
        }

        [HttpGet("{id}/users")]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
        public async Task<IActionResult> GetDepartmentUsers(int id)
        {
            var department = await context.Departments
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            var users = department.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.FirstName,
                u.LastName,
                FullName = u.FullName,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt
            });

            return Ok(new
            {
                Department = department.Name,
                UserCount = users.Count(),
                Users = users
            });
        }
    }
}
