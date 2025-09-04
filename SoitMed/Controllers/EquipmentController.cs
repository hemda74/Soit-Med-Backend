using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly Context context;

        public EquipmentController(Context _context)
        {
            context = _context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipment()
        {
            var equipment = await context.Equipment
                .Include(e => e.Hospital)
                .Select(e => new EquipmentResponseDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    QRCode = e.QRCode,
                    Description = e.Description,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    PurchaseDate = e.PurchaseDate,
                    WarrantyExpiry = e.WarrantyExpiry,
                    HospitalId = e.HospitalId,
                    HospitalName = e.Hospital.Name,
                    RepairVisitCount = e.RepairVisitCount,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            return Ok(equipment);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipment(int id)
        {
            var equipment = await context.Equipment
                .Include(e => e.Hospital)
                .Include(e => e.RepairRequests)
                .ThenInclude(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            var response = new EquipmentResponseDTO
            {
                Id = equipment.Id,
                Name = equipment.Name,
                QRCode = equipment.QRCode,
                Description = equipment.Description,
                Model = equipment.Model,
                Manufacturer = equipment.Manufacturer,
                PurchaseDate = equipment.PurchaseDate,
                WarrantyExpiry = equipment.WarrantyExpiry,
                HospitalId = equipment.HospitalId,
                HospitalName = equipment.Hospital.Name,
                RepairVisitCount = equipment.RepairVisitCount,
                Status = equipment.Status,
                CreatedAt = equipment.CreatedAt,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                IsActive = equipment.IsActive
            };

            return Ok(response);
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipmentByQR(string qrCode)
        {
            var equipment = await context.Equipment
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.QRCode == qrCode);

            if (equipment == null)
            {
                return NotFound($"Equipment with QR code {qrCode} not found");
            }

            var response = new EquipmentResponseDTO
            {
                Id = equipment.Id,
                Name = equipment.Name,
                QRCode = equipment.QRCode,
                Description = equipment.Description,
                Model = equipment.Model,
                Manufacturer = equipment.Manufacturer,
                PurchaseDate = equipment.PurchaseDate,
                WarrantyExpiry = equipment.WarrantyExpiry,
                HospitalId = equipment.HospitalId,
                HospitalName = equipment.Hospital.Name,
                RepairVisitCount = equipment.RepairVisitCount,
                Status = equipment.Status,
                CreatedAt = equipment.CreatedAt,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                IsActive = equipment.IsActive
            };

            return Ok(response);
        }

        [HttpGet("hospital/{hospitalId}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetHospitalEquipment(string hospitalId)
        {
            var hospital = await context.Hospitals.FindAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var equipment = await context.Equipment
                .Where(e => e.HospitalId == hospitalId && e.IsActive)
                .Select(e => new EquipmentResponseDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    QRCode = e.QRCode,
                    Description = e.Description,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    PurchaseDate = e.PurchaseDate,
                    WarrantyExpiry = e.WarrantyExpiry,
                    HospitalId = e.HospitalId,
                    HospitalName = hospital.Name,
                    RepairVisitCount = e.RepairVisitCount,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                Hospital = hospital.Name,
                EquipmentCount = equipment.Count,
                Equipment = equipment
            });
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> CreateEquipment(EquipmentDTO equipmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if QR code already exists
            if (await context.Equipment.AnyAsync(e => e.QRCode == equipmentDTO.QRCode))
            {
                return BadRequest($"Equipment with QR code '{equipmentDTO.QRCode}' already exists");
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(equipmentDTO.HospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {equipmentDTO.HospitalId} not found");
            }

            var equipment = new Equipment
            {
                Name = equipmentDTO.Name,
                QRCode = equipmentDTO.QRCode,
                Description = equipmentDTO.Description,
                Model = equipmentDTO.Model,
                Manufacturer = equipmentDTO.Manufacturer,
                PurchaseDate = equipmentDTO.PurchaseDate,
                WarrantyExpiry = equipmentDTO.WarrantyExpiry,
                HospitalId = equipmentDTO.HospitalId,
                Status = equipmentDTO.Status,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Equipment.Add(equipment);
            await context.SaveChangesAsync();

            return Ok($"Equipment '{equipment.Name}' with QR code '{equipment.QRCode}' created successfully");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateEquipment(int id, EquipmentDTO equipmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var equipment = await context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            // Check if new QR code conflicts with existing equipment
            if (await context.Equipment.AnyAsync(e => e.QRCode == equipmentDTO.QRCode && e.Id != id))
            {
                return BadRequest($"Equipment with QR code '{equipmentDTO.QRCode}' already exists");
            }

            // Check if hospital exists
            var hospital = await context.Hospitals.FindAsync(equipmentDTO.HospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {equipmentDTO.HospitalId} not found");
            }

            equipment.Name = equipmentDTO.Name;
            equipment.QRCode = equipmentDTO.QRCode;
            equipment.Description = equipmentDTO.Description;
            equipment.Model = equipmentDTO.Model;
            equipment.Manufacturer = equipmentDTO.Manufacturer;
            equipment.PurchaseDate = equipmentDTO.PurchaseDate;
            equipment.WarrantyExpiry = equipmentDTO.WarrantyExpiry;
            equipment.HospitalId = equipmentDTO.HospitalId;
            equipment.Status = equipmentDTO.Status;

            await context.SaveChangesAsync();

            return Ok($"Equipment '{equipment.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await context.Equipment
                .Include(e => e.RepairRequests)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            if (equipment.RepairRequests.Any(rr => rr.Status == RepairStatus.InProgress || rr.Status == RepairStatus.Assigned))
            {
                return BadRequest($"Cannot delete equipment '{equipment.Name}' because it has active repair requests");
            }

            equipment.IsActive = false;

            await context.SaveChangesAsync();

            return Ok($"Equipment '{equipment.Name}' deactivated successfully");
        }

        [HttpGet("{id}/repair-history")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipmentRepairHistory(int id)
        {
            var equipment = await context.Equipment
                .Include(e => e.RepairRequests)
                .ThenInclude(rr => rr.RequestingDoctor)
                .Include(e => e.RepairRequests)
                .ThenInclude(rr => rr.RequestingTechnician)
                .Include(e => e.RepairRequests)
                .ThenInclude(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            var repairHistory = equipment.RepairRequests
                .OrderByDescending(rr => rr.RequestedAt)
                .Select(rr => new RepairRequestResponseDTO
                {
                    Id = rr.Id,
                    EquipmentId = rr.EquipmentId,
                    EquipmentName = equipment.Name,
                    EquipmentQRCode = equipment.QRCode,
                    Description = rr.Description,
                    Symptoms = rr.Symptoms,
                    Priority = rr.Priority,
                    Status = rr.Status,
                    RequestorName = rr.RequestingDoctor?.Name ?? rr.RequestingTechnician?.Name,
                    RequestorType = rr.RequestingDoctor != null ? "Doctor" : "Technician",
                    AssignedEngineerId = rr.AssignedEngineerId,
                    AssignedEngineerName = rr.AssignedEngineer?.Name,
                    RequestedAt = rr.RequestedAt,
                    AssignedAt = rr.AssignedAt,
                    StartedAt = rr.StartedAt,
                    CompletedAt = rr.CompletedAt,
                    RepairNotes = rr.RepairNotes,
                    PartsUsed = rr.PartsUsed,
                    RepairCost = rr.RepairCost,
                    EstimatedHours = rr.EstimatedHours,
                    ActualHours = rr.ActualHours
                });

            return Ok(new
            {
                Equipment = equipment.Name,
                QRCode = equipment.QRCode,
                TotalRepairVisits = equipment.RepairVisitCount,
                RepairHistory = repairHistory
            });
        }
    }
}
