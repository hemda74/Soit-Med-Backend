using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineerController : ControllerBase
    {
        private readonly Context context;

        public EngineerController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEngineers()
        {
            var Engineers = await context.Engineers
                .Include(e => e.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Governorate)
                .Include(e => e.User)
                .Select(e => new EngineerResponseDTO
                {
                    EngineerId = e.EngineerId,
                    Name = e.Name,
                    Specialty = e.Specialty,
                    CreatedAt = e.CreatedAt,
                    IsActive = e.IsActive,
                    UserId = e.UserId,
                    Governorates = e.EngineerGovernorates
                        .Where(eg => eg.IsActive)
                        .Select(eg => new GovernorateSimpleDTO
                        {
                            GovernorateId = eg.Governorate.GovernorateId,
                            Name = eg.Governorate.Name
                        }).ToList()
                })
                .ToListAsync();

            return Ok(Engineers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEngineer(int id)
        {
            var Engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Governorate)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (Engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            var response = new EngineerResponseDTO
            {
                EngineerId = Engineer.EngineerId,
                Name = Engineer.Name,
                Specialty = Engineer.Specialty,
                CreatedAt = Engineer.CreatedAt,
                IsActive = Engineer.IsActive,
                UserId = Engineer.UserId,
                Governorates = Engineer.EngineerGovernorates
                    .Where(eg => eg.IsActive)
                    .Select(eg => new GovernorateSimpleDTO
                    {
                        GovernorateId = eg.Governorate.GovernorateId,
                        Name = eg.Governorate.Name
                    }).ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEngineer(EngineerDTO EngineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Engineer = new Engineer
            {
                Name = EngineerDTO.Name,
                Specialty = EngineerDTO.Specialty,
                UserId = EngineerDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Engineers.Add(Engineer);
            await context.SaveChangesAsync();

            // Assign to governorates if provided
            if (EngineerDTO.GovernorateIds != null && EngineerDTO.GovernorateIds.Any())
            {
                foreach (var governorateId in EngineerDTO.GovernorateIds)
                {
                    var governorate = await context.Governorates.FindAsync(governorateId);
                    if (governorate != null)
                    {
                        var assignment = new EngineerGovernorate
                        {
                            EngineerId = Engineer.EngineerId,
                            GovernorateId = governorateId,
                            AssignedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        context.EngineerGovernorates.Add(assignment);
                    }
                }

                await context.SaveChangesAsync();
            }

            return Ok($"Engineer '{Engineer.Name}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateEngineer(int id, EngineerDTO EngineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (Engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            Engineer.Name = EngineerDTO.Name;
            Engineer.Specialty = EngineerDTO.Specialty;
            Engineer.UserId = EngineerDTO.UserId;

            // Update governorate assignments if provided
            if (EngineerDTO.GovernorateIds != null)
            {
                // Deactivate all current assignments
                foreach (var assignment in Engineer.EngineerGovernorates.Where(eg => eg.IsActive))
                {
                    assignment.IsActive = false;
                }

                // Add new assignments
                foreach (var governorateId in EngineerDTO.GovernorateIds)
                {
                    var existingAssignment = Engineer.EngineerGovernorates
                        .FirstOrDefault(eg => eg.GovernorateId == governorateId);

                    if (existingAssignment != null)
                    {
                        existingAssignment.IsActive = true;
                        existingAssignment.AssignedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var governorate = await context.Governorates.FindAsync(governorateId);
                        if (governorate != null)
                        {
                            var assignment = new EngineerGovernorate
                            {
                                EngineerId = Engineer.EngineerId,
                                GovernorateId = governorateId,
                                AssignedAt = DateTime.UtcNow,
                                IsActive = true
                            };

                            context.EngineerGovernorates.Add(assignment);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();

            return Ok($"Engineer '{Engineer.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteEngineer(int id)
        {
            var Engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (Engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            // Deactivate all governorate assignments
            foreach (var assignment in Engineer.EngineerGovernorates.Where(eg => eg.IsActive))
            {
                assignment.IsActive = false;
            }

            Engineer.IsActive = false;

            await context.SaveChangesAsync();

            return Ok($"Engineer '{Engineer.Name}' deactivated successfully");
        }

        [HttpGet("{id}/governorates")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEngineerGovernorates(int id)
        {
            var Engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Governorate)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (Engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            var governorates = Engineer.EngineerGovernorates
                .Where(eg => eg.IsActive)
                .Select(eg => new
                {
                    eg.Governorate.GovernorateId,
                    eg.Governorate.Name,
                    eg.AssignedAt
                });

            return Ok(new
            {
                Engineer = Engineer.Name,
                GovernorateCount = governorates.Count(),
                Governorates = governorates
            });
        }
    }
}
