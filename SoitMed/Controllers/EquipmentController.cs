using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Hospital;
using SoitMed.Models.Location;
using SoitMed.Services;
using SoitMed.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQRCodeService _qrCodeService;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(
            IUnitOfWork unitOfWork, 
            IQRCodeService qrCodeService,
            ILogger<EquipmentController> logger)
        {
            _unitOfWork = unitOfWork;
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipment()
        {
            var equipment = await _unitOfWork.Equipment.GetActiveEquipmentAsync();
            
            var response = equipment.Select(e => new EquipmentResponseDTO
            {
                Id = e.Id,
                Name = e.Name,
                QRCode = e.QRCode,
                QRCodeImageData = e.QRCodeImageData,
                QRCodePdfPath = e.QRCodePdfPath,
                Description = e.Description,
                Model = e.Model,
                Manufacturer = e.Manufacturer,
                PurchaseDate = e.PurchaseDate,
                WarrantyExpiry = e.WarrantyExpiry,
                HospitalId = e.HospitalId,
                HospitalName = e.Hospital?.Name ?? string.Empty,
                RepairVisitCount = e.RepairVisitCount,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                LastMaintenanceDate = e.LastMaintenanceDate,
                IsActive = e.IsActive,
                QrToken = e.QrToken,
                IsQrPrinted = e.IsQrPrinted
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipment(int id)
        {
            var equipment = await _unitOfWork.Equipment.GetEquipmentWithAllDetailsAsync(id);

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
                HospitalName = equipment.Hospital?.Name ?? string.Empty,
                RepairVisitCount = equipment.RepairVisitCount,
                Status = equipment.Status,
                CreatedAt = equipment.CreatedAt,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                IsActive = equipment.IsActive,
                QrToken = equipment.QrToken,
                IsQrPrinted = equipment.IsQrPrinted
            };

            return Ok(response);
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipmentByQR(string qrCode)
        {
            var equipment = await _unitOfWork.Equipment.GetByQRCodeAsync(qrCode);

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
                HospitalName = equipment.Hospital?.Name ?? string.Empty,
                RepairVisitCount = equipment.RepairVisitCount,
                Status = equipment.Status,
                CreatedAt = equipment.CreatedAt,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                IsActive = equipment.IsActive,
                QrToken = equipment.QrToken,
                IsQrPrinted = equipment.IsQrPrinted
            };

            return Ok(response);
        }

        [HttpGet("hospital/{hospitalId}")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetHospitalEquipment(string hospitalId)
        {
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(hospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {hospitalId} not found");
            }

            var equipment = await _unitOfWork.Equipment.GetByHospitalIdAsync(hospitalId);
            
            var response = equipment.Where(e => e.IsActive).Select(e => new EquipmentResponseDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    QRCode = e.QRCode,
                    QRCodeImageData = e.QRCodeImageData,
                    QRCodePdfPath = e.QRCodePdfPath,
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
                    IsActive = e.IsActive,
                    QrToken = e.QrToken,
                    IsQrPrinted = e.IsQrPrinted
                });

            return Ok(new
            {
                Hospital = hospital.Name,
                EquipmentCount = response.Count(),
                Equipment = response
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

            // Check if hospital exists
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(equipmentDTO.HospitalId);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {equipmentDTO.HospitalId} not found");
            }

            // Check if custom QR code already exists (if provided)
            if (!string.IsNullOrEmpty(equipmentDTO.QRCode) && 
                await _unitOfWork.Equipment.ExistsByQRCodeAsync(equipmentDTO.QRCode))
            {
                return BadRequest($"Equipment with QR code '{equipmentDTO.QRCode}' already exists");
            }

            var equipment = new Equipment
            {
                Name = equipmentDTO.Name,
                QRCode = equipmentDTO.QRCode ?? string.Empty, // Will be generated by service
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

            // Generate QR code and save as PDF
            var qrResult = await _qrCodeService.GenerateQRCodeAsync(equipment);
            if (!qrResult.Success)
            {
                return BadRequest($"Failed to generate QR code: {qrResult.ErrorMessage}");
            }

            // Update equipment with QR code data
            equipment.QRCode = qrResult.QRCode;
            equipment.QRCodeImageData = qrResult.QRCodeImageData;
            equipment.QRCodePdfPath = qrResult.QRCodePdfPath;

            await _unitOfWork.Equipment.CreateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new
            {
                Message = $"Equipment '{equipment.Name}' created successfully",
                QRCode = equipment.QRCode,
                QRCodePdfPath = equipment.QRCodePdfPath,
                Equipment = new EquipmentResponseDTO
                {
                    Id = equipment.Id,
                    Name = equipment.Name,
                    QRCode = equipment.QRCode,
                    QRCodeImageData = equipment.QRCodeImageData,
                    QRCodePdfPath = equipment.QRCodePdfPath,
                    Description = equipment.Description,
                    Model = equipment.Model,
                    Manufacturer = equipment.Manufacturer,
                    PurchaseDate = equipment.PurchaseDate,
                    WarrantyExpiry = equipment.WarrantyExpiry,
                    HospitalId = equipment.HospitalId,
                    HospitalName = hospital.Name,
                    RepairVisitCount = equipment.RepairVisitCount,
                    Status = equipment.Status,
                    CreatedAt = equipment.CreatedAt,
                    LastMaintenanceDate = equipment.LastMaintenanceDate,
                    IsActive = equipment.IsActive,
                    QrToken = equipment.QrToken,
                    IsQrPrinted = equipment.IsQrPrinted
                }
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateEquipment(int id, EquipmentDTO equipmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            // Check if new QR code conflicts with existing equipment
            if (await _unitOfWork.Equipment.ExistsByQRCodeExcludingIdAsync(equipmentDTO.QRCode, id))
            {
                return BadRequest($"Equipment with QR code '{equipmentDTO.QRCode}' already exists");
            }

            // Check if hospital exists
            var hospital = await _unitOfWork.Hospitals.GetByHospitalIdAsync(equipmentDTO.HospitalId);
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

            await _unitOfWork.Equipment.UpdateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Equipment '{equipment.Name}' updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var equipment = await _unitOfWork.Equipment.GetEquipmentWithRepairRequestsAsync(id);

            if (equipment == null)
            {
                return NotFound($"Equipment with ID {id} not found");
            }

            if (equipment.RepairRequests.Any(rr => rr.Status == RepairStatus.InProgress || rr.Status == RepairStatus.Assigned))
            {
                return BadRequest($"Cannot delete equipment '{equipment.Name}' because it has active repair requests");
            }

            equipment.IsActive = false;

            await _unitOfWork.Equipment.UpdateAsync(equipment);
            await _unitOfWork.SaveChangesAsync();

            return Ok($"Equipment '{equipment.Name}' deactivated successfully");
        }

        [HttpGet("{id}/repair-history")]
        [Authorize(Roles = "SuperAdmin,Admin,Doctor,Technician")]
        public async Task<IActionResult> GetEquipmentRepairHistory(int id)
        {
            var equipment = await _unitOfWork.Equipment.GetEquipmentWithAllDetailsAsync(id);

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

        /// <summary>
        /// Mark QR code as printed for an equipment
        /// POST /api/Equipment/Qr/MarkPrinted
        /// </summary>
        [HttpPost("Qr/MarkPrinted")]
        [Authorize(Roles = "SuperAdmin,Admin,Support")]
        public async Task<IActionResult> MarkQrPrinted([FromBody] DTO.MarkQrPrintedDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var equipment = await _unitOfWork.Equipment.GetByIdAsync(dto.EquipmentId);
                if (equipment == null)
                {
                    return NotFound(new { message = $"Equipment with ID {dto.EquipmentId} not found" });
                }

                // Mark as printed
                equipment.IsQrPrinted = true;
                equipment.QrLastPrintedDate = DateTime.UtcNow;

                await _unitOfWork.Equipment.UpdateAsync(equipment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("QR code marked as printed for Equipment {EquipmentId} by {CurrentUser}", 
                    dto.EquipmentId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = $"QR code for equipment '{equipment.Name}' marked as printed",
                    data = new
                    {
                        equipmentId = equipment.Id,
                        equipmentName = equipment.Name,
                        qrToken = equipment.QrToken,
                        isQrPrinted = equipment.IsQrPrinted,
                        qrLastPrintedDate = equipment.QrLastPrintedDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking QR as printed for equipment {EquipmentId}", dto.EquipmentId);
                return StatusCode(500, new { message = "An error occurred while marking QR as printed", error = ex.Message });
            }
        }
    }
}
