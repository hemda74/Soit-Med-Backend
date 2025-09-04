using Lab1.DTO;
using Lab1.Models;
using Lab1.Models.Equipment;
using Lab1.Models.Hospital;
using Lab1.Models.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairRequestController : ControllerBase
    {
        private readonly Context context;

        public RepairRequestController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Engineer")]
        public async Task<IActionResult> GetRepairRequests()
        {
            var repairRequests = await context.RepairRequests
                .Include(rr => rr.Equipment)
                .ThenInclude(e => e.Hospital)
                .Include(rr => rr.RequestingDoctor)
                .Include(rr => rr.RequestingTechnician)
                .Include(rr => rr.AssignedEngineer)
                .Select(rr => new RepairRequestResponseDTO
                {
                    Id = rr.Id,
                    EquipmentId = rr.EquipmentId,
                    EquipmentName = rr.Equipment.Name,
                    EquipmentQRCode = rr.Equipment.QRCode,
                    HospitalName = rr.Equipment.Hospital.Name,
                    Description = rr.Description,
                    Symptoms = rr.Symptoms,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    RequestorName = rr.RequestingDoctor != null ? rr.RequestingDoctor.Name : rr.RequestingTechnician!.Name,
                    RequestorType = rr.RequestingDoctor != null ? "Doctor" : "Technician",
                    AssignedEngineerId = rr.AssignedEngineerId,
                    AssignedEngineerName = rr.AssignedEngineer != null ? rr.AssignedEngineer.Name : null,
                    RequestedAt = rr.RequestedAt,
                    AssignedAt = rr.AssignedAt,
                    StartedAt = rr.StartedAt,
                    CompletedAt = rr.CompletedAt,
                    RepairNotes = rr.RepairNotes,
                    PartsUsed = rr.PartsUsed,
                    RepairCost = rr.RepairCost,
                    EstimatedHours = rr.EstimatedHours,
                    ActualHours = rr.ActualHours
                })
                .ToListAsync();

            return Ok(repairRequests);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician,Engineer")]
        public async Task<IActionResult> GetRepairRequest(int id)
        {
            var repairRequest = await context.RepairRequests
                .Include(rr => rr.Equipment)
                .ThenInclude(e => e.Hospital)
                .Include(rr => rr.RequestingDoctor)
                .Include(rr => rr.RequestingTechnician)
                .Include(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(rr => rr.Id == id);

            if (repairRequest == null)
            {
                return NotFound($"Repair request with ID {id} not found");
            }

            var response = new RepairRequestResponseDTO
            {
                Id = repairRequest.Id,
                EquipmentId = repairRequest.EquipmentId,
                EquipmentName = repairRequest.Equipment.Name,
                EquipmentQRCode = repairRequest.Equipment.QRCode,
                HospitalName = repairRequest.Equipment.Hospital.Name,
                Description = repairRequest.Description,
                Symptoms = repairRequest.Symptoms,
                Priority = repairRequest.Priority,
                Status = repairRequest.Status,
                RequestorName = repairRequest.RequestingDoctor?.Name ?? repairRequest.RequestingTechnician?.Name,
                RequestorType = repairRequest.RequestingDoctor != null ? "Doctor" : "Technician",
                AssignedEngineerId = repairRequest.AssignedEngineerId,
                AssignedEngineerName = repairRequest.AssignedEngineer?.Name,
                RequestedAt = repairRequest.RequestedAt,
                AssignedAt = repairRequest.AssignedAt,
                StartedAt = repairRequest.StartedAt,
                CompletedAt = repairRequest.CompletedAt,
                RepairNotes = repairRequest.RepairNotes,
                PartsUsed = repairRequest.PartsUsed,
                RepairCost = repairRequest.RepairCost,
                EstimatedHours = repairRequest.EstimatedHours,
                ActualHours = repairRequest.ActualHours
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Technician")]
        public async Task<IActionResult> CreateRepairRequest(RepairRequestDTO repairRequestDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that either DoctorId or TechnicianId is provided (not both)
            if ((repairRequestDTO.DoctorId == null && repairRequestDTO.TechnicianId == null) ||
                (repairRequestDTO.DoctorId != null && repairRequestDTO.TechnicianId != null))
            {
                return BadRequest("Either DoctorId or TechnicianId must be provided (not both)");
            }

            // Check if equipment exists
            var equipment = await context.Equipment
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.Id == repairRequestDTO.EquipmentId);

            if (equipment == null)
            {
                return NotFound($"Equipment with ID {repairRequestDTO.EquipmentId} not found");
            }

            // Validate requestor exists
            if (repairRequestDTO.DoctorId.HasValue)
            {
                var doctor = await context.Doctors.FindAsync(repairRequestDTO.DoctorId.Value);
                if (doctor == null)
                {
                    return NotFound($"Doctor with ID {repairRequestDTO.DoctorId} not found");
                }
            }

            if (repairRequestDTO.TechnicianId.HasValue)
            {
                var technician = await context.Technicians.FindAsync(repairRequestDTO.TechnicianId.Value);
                if (technician == null)
                {
                    return NotFound($"Technician with ID {repairRequestDTO.TechnicianId} not found");
                }
            }

            var repairRequest = new RepairRequest
            {
                EquipmentId = repairRequestDTO.EquipmentId,
                Description = repairRequestDTO.Description,
                Symptoms = repairRequestDTO.Symptoms,
                Priority = repairRequestDTO.Priority,
                DoctorId = repairRequestDTO.DoctorId,
                TechnicianId = repairRequestDTO.TechnicianId,
                EstimatedHours = repairRequestDTO.EstimatedHours,
                Status = RepairStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.RepairRequests.Add(repairRequest);

            // Increment equipment repair visit count
            equipment.RepairVisitCount++;

            await context.SaveChangesAsync();

            // Auto-assign to engineers in the hospital's governorate
            await AutoAssignToEngineer(repairRequest.Id, equipment.Hospital.Location);

            return Ok($"Repair request for equipment '{equipment.Name}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Engineer")]
        public async Task<IActionResult> UpdateRepairRequest(int id, UpdateRepairRequestDTO updateDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var repairRequest = await context.RepairRequests.FindAsync(id);
            if (repairRequest == null)
            {
                return NotFound($"Repair request with ID {id} not found");
            }

            // Update fields if provided
            if (updateDTO.Status.HasValue)
            {
                repairRequest.Status = updateDTO.Status.Value;

                // Update timestamps based on status
                switch (updateDTO.Status.Value)
                {
                    case RepairStatus.Assigned:
                        if (!repairRequest.AssignedAt.HasValue)
                            repairRequest.AssignedAt = DateTime.UtcNow;
                        break;
                    case RepairStatus.InProgress:
                        if (!repairRequest.StartedAt.HasValue)
                            repairRequest.StartedAt = DateTime.UtcNow;
                        break;
                    case RepairStatus.Completed:
                        if (!repairRequest.CompletedAt.HasValue)
                            repairRequest.CompletedAt = DateTime.UtcNow;
                        break;
                }
            }

            if (updateDTO.AssignedEngineerId.HasValue)
            {
                var engineer = await context.Engineers.FindAsync(updateDTO.AssignedEngineerId.Value);
                if (engineer == null)
                {
                    return NotFound($"Engineer with ID {updateDTO.AssignedEngineerId} not found");
                }
                repairRequest.AssignedEngineerId = updateDTO.AssignedEngineerId.Value;
                repairRequest.AssignedAt = DateTime.UtcNow;
                repairRequest.Status = RepairStatus.Assigned;
            }

            if (!string.IsNullOrEmpty(updateDTO.RepairNotes))
                repairRequest.RepairNotes = updateDTO.RepairNotes;

            if (!string.IsNullOrEmpty(updateDTO.PartsUsed))
                repairRequest.PartsUsed = updateDTO.PartsUsed;

            if (updateDTO.RepairCost.HasValue)
                repairRequest.RepairCost = updateDTO.RepairCost.Value;

            if (updateDTO.ActualHours.HasValue)
                repairRequest.ActualHours = updateDTO.ActualHours.Value;

            await context.SaveChangesAsync();

            return Ok($"Repair request updated successfully");
        }

        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin,Admin,Engineer")]
        public async Task<IActionResult> GetPendingRepairRequests()
        {
            var pendingRequests = await context.RepairRequests
                .Where(rr => rr.Status == RepairStatus.Pending && rr.IsActive)
                .Include(rr => rr.Equipment)
                .ThenInclude(e => e.Hospital)
                .Include(rr => rr.RequestingDoctor)
                .Include(rr => rr.RequestingTechnician)
                .Select(rr => new RepairRequestResponseDTO
                {
                    Id = rr.Id,
                    EquipmentId = rr.EquipmentId,
                    EquipmentName = rr.Equipment.Name,
                    EquipmentQRCode = rr.Equipment.QRCode,
                    HospitalName = rr.Equipment.Hospital.Name,
                    Description = rr.Description,
                    Symptoms = rr.Symptoms,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    RequestorName = rr.RequestingDoctor != null ? rr.RequestingDoctor.Name : rr.RequestingTechnician!.Name,
                    RequestorType = rr.RequestingDoctor != null ? "Doctor" : "Technician",
                    RequestedAt = rr.RequestedAt,
                    EstimatedHours = rr.EstimatedHours
                })
                .OrderByDescending(rr => rr.Priority)
                .ThenBy(rr => rr.RequestedAt)
                .ToListAsync();

            return Ok(pendingRequests);
        }

        [HttpGet("engineer/{engineerId}")]
        [Authorize(Roles = "SuperAdmin,Admin,Engineer")]
        public async Task<IActionResult> GetEngineerRepairRequests(int engineerId)
        {
            var engineer = await context.Engineers.FindAsync(engineerId);
            if (engineer == null)
            {
                return NotFound($"Engineer with ID {engineerId} not found");
            }

            var repairRequests = await context.RepairRequests
                .Where(rr => rr.AssignedEngineerId == engineerId && rr.IsActive)
                .Include(rr => rr.Equipment)
                .ThenInclude(e => e.Hospital)
                .Include(rr => rr.RequestingDoctor)
                .Include(rr => rr.RequestingTechnician)
                .Select(rr => new RepairRequestResponseDTO
                {
                    Id = rr.Id,
                    EquipmentId = rr.EquipmentId,
                    EquipmentName = rr.Equipment.Name,
                    EquipmentQRCode = rr.Equipment.QRCode,
                    HospitalName = rr.Equipment.Hospital.Name,
                    Description = rr.Description,
                    Symptoms = rr.Symptoms,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    RequestorName = rr.RequestingDoctor != null ? rr.RequestingDoctor.Name : rr.RequestingTechnician!.Name,
                    RequestorType = rr.RequestingDoctor != null ? "Doctor" : "Technician",
                    RequestedAt = rr.RequestedAt,
                    AssignedAt = rr.AssignedAt,
                    StartedAt = rr.StartedAt,
                    CompletedAt = rr.CompletedAt,
                    RepairNotes = rr.RepairNotes,
                    PartsUsed = rr.PartsUsed,
                    RepairCost = rr.RepairCost,
                    EstimatedHours = rr.EstimatedHours,
                    ActualHours = rr.ActualHours
                })
                .OrderByDescending(rr => rr.Priority)
                .ThenBy(rr => rr.RequestedAt)
                .ToListAsync();

            return Ok(new
            {
                Engineer = engineer.Name,
                RequestCount = repairRequests.Count,
                Requests = repairRequests
            });
        }

        private async Task AutoAssignToEngineer(int repairRequestId, string hospitalLocation)
        {
            // Find engineers in governorates that match the hospital location
            // This is a simplified matching - in reality, you'd have a more sophisticated location mapping
            var availableEngineers = await context.Engineers
                .Include(e => e.EngineerGovernorates)
                .ThenInclude(eg => eg.Governorate)
                .Where(e => e.IsActive && 
                           e.EngineerGovernorates.Any(eg => eg.IsActive && 
                                                           eg.Governorate.Name.Contains(hospitalLocation)))
                .ToListAsync();

            if (availableEngineers.Any())
            {
                // Simple round-robin assignment - assign to engineer with least active requests
                var engineerWorkloads = await context.RepairRequests
                    .Where(rr => rr.AssignedEngineerId.HasValue && 
                                rr.Status != RepairStatus.Completed && 
                                rr.Status != RepairStatus.Cancelled)
                    .GroupBy(rr => rr.AssignedEngineerId)
                    .Select(g => new { EngineerId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var selectedEngineer = availableEngineers
                    .OrderBy(e => engineerWorkloads.FirstOrDefault(w => w.EngineerId == e.EngineerId)?.Count ?? 0)
                    .First();

                var repairRequest = await context.RepairRequests.FindAsync(repairRequestId);
                if (repairRequest != null)
                {
                    repairRequest.AssignedEngineerId = selectedEngineer.EngineerId;
                    repairRequest.AssignedAt = DateTime.UtcNow;
                    repairRequest.Status = RepairStatus.Assigned;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
