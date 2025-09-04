using Lab1.DTO;
using Lab1.Models;
using Lab1.Models.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GovernorateController : ControllerBase
    {
        private readonly Context context;

        public GovernorateController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetGovernorates()
        {
            var governorates = await context.Governorates
                .Select(g => new GovernorateResponseDTO
                {
                    GovernorateId = g.GovernorateId,
                    Name = g.Name,
                    CreatedAt = g.CreatedAt,
                    IsActive = g.IsActive,
                    EngineerCount = g.EngineerGovernorates.Count(eg => eg.IsActive)
                })
                .ToListAsync();

            return Ok(governorates);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetGovernorate(int id)
        {
            var governorate = await context.Governorates
                .Include(g => g.EngineerGovernorates)
                .ThenInclude(eg => eg.Engineer)
                .FirstOrDefaultAsync(g => g.GovernorateId == id);

            if (governorate == null)
            {
                return NotFound($"Governorate with ID {id} not found");
            }

            var response = new GovernorateResponseDTO
            {
                GovernorateId = governorate.GovernorateId,
                Name = governorate.Name,
                CreatedAt = governorate.CreatedAt,
                IsActive = governorate.IsActive,
                EngineerCount = governorate.EngineerGovernorates.Count(eg => eg.IsActive)
            };

            return Ok(response);
        }

        // Required API endpoint: GET /governorates/{id}/engineers
        [HttpGet("{id}/engineers")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetGovernorateEngineers(int id)
        {
            var governorate = await context.Governorates
                .Include(g => g.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Engineer)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(g => g.GovernorateId == id);

            if (governorate == null)
            {
                return NotFound($"Governorate with ID {id} not found");
            }

            var engineers = governorate.EngineerGovernorates
                .Where(eg => eg.IsActive && eg.Engineer.IsActive)
                .Select(eg => new
                {
                    eg.Engineer.EngineerId,
                    eg.Engineer.Name,
                    eg.Engineer.Specialty,
                    eg.Engineer.CreatedAt,
                    eg.AssignedAt,
                    User = eg.Engineer.User != null ? new { eg.Engineer.User.UserName, eg.Engineer.User.Email } : null
                });

            return Ok(new
            {
                Governorate = governorate.Name,
                EngineerCount = engineers.Count(),
                Engineers = engineers
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateGovernorate(GovernorateDTO governorateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if governorate already exists
            if (await context.Governorates.AnyAsync(g => g.Name == governorateDTO.Name))
            {
                return BadRequest($"Governorate '{governorateDTO.Name}' already exists");
            }

            var governorate = new Governorate
            {
                Name = governorateDTO.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Governorates.Add(governorate);
            await context.SaveChangesAsync();

            return Ok($"Governorate '{governorate.Name}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateGovernorate(int id, GovernorateDTO governorateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var governorate = await context.Governorates.FindAsync(id);
            if (governorate == null)
            {
                return NotFound($"Governorate with ID {id} not found");
            }

            // Check if new name conflicts with existing governorates
            if (await context.Governorates.AnyAsync(g => g.Name == governorateDTO.Name && g.GovernorateId != id))
            {
                return BadRequest($"Governorate '{governorateDTO.Name}' already exists");
            }

            governorate.Name = governorateDTO.Name;

            await context.SaveChangesAsync();

            return Ok($"Governorate '{governorate.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteGovernorate(int id)
        {
            var governorate = await context.Governorates
                .Include(g => g.EngineerGovernorates)
                .FirstOrDefaultAsync(g => g.GovernorateId == id);

            if (governorate == null)
            {
                return NotFound($"Governorate with ID {id} not found");
            }

            if (governorate.EngineerGovernorates.Any(eg => eg.IsActive))
            {
                return BadRequest($"Cannot delete governorate '{governorate.Name}' because it has active engineers assigned to it");
            }

            context.Governorates.Remove(governorate);
            await context.SaveChangesAsync();

            return Ok($"Governorate '{governorate.Name}' deleted successfully");
        }

        // Engineer assignment endpoints
        [HttpPost("{governorateId}/engineers/{engineerId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AssignEngineerToGovernorate(int governorateId, int engineerId)
        {
            var governorate = await context.Governorates.FindAsync(governorateId);
            if (governorate == null)
            {
                return NotFound($"Governorate with ID {governorateId} not found");
            }

            var engineer = await context.Engineers.FindAsync(engineerId);
            if (engineer == null)
            {
                return NotFound($"Engineer with ID {engineerId} not found");
            }

            // Check if assignment already exists
            var existingAssignment = await context.EngineerGovernorates
                .FirstOrDefaultAsync(eg => eg.EngineerId == engineerId && eg.GovernorateId == governorateId);

            if (existingAssignment != null)
            {
                if (existingAssignment.IsActive)
                {
                    return BadRequest($"Engineer '{engineer.Name}' is already assigned to governorate '{governorate.Name}'");
                }
                else
                {
                    // Reactivate the assignment
                    existingAssignment.IsActive = true;
                    existingAssignment.AssignedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Create new assignment
                var assignment = new EngineerGovernorate
                {
                    EngineerId = engineerId,
                    GovernorateId = governorateId,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.EngineerGovernorates.Add(assignment);
            }

            await context.SaveChangesAsync();

            return Ok($"Engineer '{engineer.Name}' assigned to governorate '{governorate.Name}' successfully");
        }

        [HttpDelete("{governorateId}/engineers/{engineerId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> RemoveEngineerFromGovernorate(int governorateId, int engineerId)
        {
            var assignment = await context.EngineerGovernorates
                .Include(eg => eg.Engineer)
                .Include(eg => eg.Governorate)
                .FirstOrDefaultAsync(eg => eg.EngineerId == engineerId && eg.GovernorateId == governorateId && eg.IsActive);

            if (assignment == null)
            {
                return NotFound($"Active assignment between engineer ID {engineerId} and governorate ID {governorateId} not found");
            }

            assignment.IsActive = false;

            await context.SaveChangesAsync();

            return Ok($"Engineer '{assignment.Engineer.Name}' removed from governorate '{assignment.Governorate.Name}' successfully");
        }
    }
}
