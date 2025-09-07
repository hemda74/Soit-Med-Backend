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
            var engineers = await context.Engineers
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

            return Ok(engineers);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEngineer(int id)
        {
            var engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Governorate)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            var response = new EngineerResponseDTO
            {
                EngineerId = engineer.EngineerId,
                Name = engineer.Name,
                Specialty = engineer.Specialty,
                CreatedAt = engineer.CreatedAt,
                IsActive = engineer.IsActive,
                UserId = engineer.UserId,
                Governorates = engineer.EngineerGovernorates
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
        public async Task<IActionResult> CreateEngineer(EngineerDTO engineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var engineer = new Engineer
            {
                Name = engineerDTO.Name,
                Specialty = engineerDTO.Specialty,
                UserId = engineerDTO.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Engineers.Add(engineer);
            await context.SaveChangesAsync();

            // Assign to governorates if provided
            if (engineerDTO.GovernorateIds != null && engineerDTO.GovernorateIds.Any())
            {
                foreach (var governorateId in engineerDTO.GovernorateIds)
                {
                    var governorate = await context.Governorates.FindAsync(governorateId);
                    if (governorate != null)
                    {
                        var assignment = new EngineerGovernorate
                        {
                            EngineerId = engineer.EngineerId,
                            GovernorateId = governorateId,
                            AssignedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        context.EngineerGovernorates.Add(assignment);
                    }
                }

                await context.SaveChangesAsync();
            }

            return Ok($"Engineer '{engineer.Name}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateEngineer(int id, EngineerDTO engineerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            engineer.Name = engineerDTO.Name;
            engineer.Specialty = engineerDTO.Specialty;
            engineer.UserId = engineerDTO.UserId;

            // Update governorate assignments if provided
            if (engineerDTO.GovernorateIds != null)
            {
                // Deactivate all current assignments
                foreach (var assignment in engineer.EngineerGovernorates.Where(eg => eg.IsActive))
                {
                    assignment.IsActive = false;
                }

                // Add new assignments
                foreach (var governorateId in engineerDTO.GovernorateIds)
                {
                    var existingAssignment = engineer.EngineerGovernorates
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
                                EngineerId = engineer.EngineerId,
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

            return Ok($"Engineer '{engineer.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteEngineer(int id)
        {
            var engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            // Deactivate all governorate assignments
            foreach (var assignment in engineer.EngineerGovernorates.Where(eg => eg.IsActive))
            {
                assignment.IsActive = false;
            }

            engineer.IsActive = false;

            await context.SaveChangesAsync();

            return Ok($"Engineer '{engineer.Name}' deactivated successfully");
        }

        [HttpGet("{id}/governorates")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEngineerGovernorates(int id)
        {
            var engineer = await context.Engineers
                .Include(e => e.EngineerGovernorates.Where(eg => eg.IsActive))
                .ThenInclude(eg => eg.Governorate)
                .FirstOrDefaultAsync(e => e.EngineerId == id);

            if (engineer == null)
            {
                return NotFound($"Engineer with ID {id} not found");
            }

            var governorates = engineer.EngineerGovernorates
                .Where(eg => eg.IsActive)
                .Select(eg => new
                {
                    eg.Governorate.GovernorateId,
                    eg.Governorate.Name,
                    eg.AssignedAt
                });

            return Ok(new
            {
                Engineer = engineer.Name,
                GovernorateCount = governorates.Count(),
                Governorates = governorates
            });
        }
    }
}
