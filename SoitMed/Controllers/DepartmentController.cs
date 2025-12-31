using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Core;
using SoitMed.Repositories;
using SoitMed.Services;
using SoitMed.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public DepartmentController(IUnitOfWork unitOfWork, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
        public async Task<IActionResult> GetDepartments()
        {
            var response = await _cacheService.GetOrCreateAsync(
                CacheKeys.Reference.Departments,
                async () =>
                {
                    var departments = await _unitOfWork.Departments.GetDepartmentsWithUsersAsync();
                    
                    return departments.Select(d => new DepartmentResponseDTO
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        CreatedAt = d.CreatedAt,
                        UserCount = d.Users.Count()
                    }).ToList();
                },
                TimeSpan.FromHours(24)
            );

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,FinanceManager,LegalManager")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var response = await _cacheService.GetOrCreateAsync(
                $"Departments:{id}",
                async () =>
                {
                    var department = await _unitOfWork.Departments.GetDepartmentWithUsersAsync(id);

                    if (department == null)
                    {
                        return null;
                    }

                    return new DepartmentResponseDTO
                    {
                        Id = department.Id,
                        Name = department.Name,
                        Description = department.Description,
                        CreatedAt = department.CreatedAt,
                        UserCount = department.Users.Count()
                    };
                },
                TimeSpan.FromHours(12)
            );

            if (response == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

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
            if (await _unitOfWork.Departments.ExistsByNameAsync(departmentDTO.Name))
            {
                return BadRequest($"Department '{departmentDTO.Name}' already exists");
            }

            var department = new Department
            {
                Name = departmentDTO.Name,
                Description = departmentDTO.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Departments.CreateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync(CacheKeys.Reference.Departments);

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

            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            // Check if new name conflicts with existing departments
            if (await _unitOfWork.Departments.ExistsByNameExcludingIdAsync(departmentDTO.Name, id))
            {
                return BadRequest($"Department '{departmentDTO.Name}' already exists");
            }

            department.Name = departmentDTO.Name;
            department.Description = departmentDTO.Description;

            await _unitOfWork.Departments.UpdateAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync(CacheKeys.Reference.Departments);
            await _cacheService.RemoveAsync($"Departments:{id}");

            return Ok($"Department '{department.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _unitOfWork.Departments.GetDepartmentWithUsersAsync(id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found");
            }

            if (department.Users.Any())
            {
                return BadRequest($"Cannot delete department '{department.Name}' because it has {department.Users.Count()} users assigned to it");
            }

            await _unitOfWork.Departments.DeleteAsync(department);
            await _unitOfWork.SaveChangesAsync();

            // Invalidate cache
            await _cacheService.RemoveAsync(CacheKeys.Reference.Departments);
            await _cacheService.RemoveAsync($"Departments:{id}");

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
            var department = await _unitOfWork.Departments.GetDepartmentWithUsersAsync(id);

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
