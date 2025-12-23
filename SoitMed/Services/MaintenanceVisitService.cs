using Microsoft.AspNetCore.Identity;
using SoitMed.DTO;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Collections.Generic;

namespace SoitMed.Services
{
    public class MaintenanceVisitService : IMaintenanceVisitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MaintenanceVisitService> _logger;

        public MaintenanceVisitService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<MaintenanceVisitService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<MaintenanceVisitResponseDTO> CreateVisitAsync(CreateMaintenanceVisitDTO dto, string EngineerId)
        {
            try
            {
                var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(dto.MaintenanceRequestId);
                if (request == null)
                    throw new ArgumentException("Maintenance request not found", nameof(dto.MaintenanceRequestId));

                var visit = new MaintenanceVisit
                {
                    MaintenanceRequestId = dto.MaintenanceRequestId,
                    EngineerId = EngineerId,
                    QRCode = dto.QRCode,
                    SerialCode = dto.SerialCode,
                    Report = dto.Report,
                    ActionsTaken = dto.ActionsTaken,
                    PartsUsed = dto.PartsUsed,
                    ServiceFee = dto.ServiceFee,
                    Outcome = dto.Outcome,
                    VisitDate = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Notes = dto.Notes
                };

                await _unitOfWork.MaintenanceVisits.CreateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Set request to InProgress when visit starts (if not already)
                if (request.Status == MaintenanceRequestStatus.Assigned || request.Status == MaintenanceRequestStatus.Pending)
                {
                    request.Status = MaintenanceRequestStatus.InProgress;
                    if (request.StartedAt == null)
                    {
                        request.StartedAt = DateTime.UtcNow;
                    }
                }

                // Update request status based on outcome
                if (dto.Outcome == MaintenanceVisitOutcome.Completed)
                {
                    request.Status = MaintenanceRequestStatus.Completed;
                    request.CompletedAt = DateTime.UtcNow;
                }
                else if (dto.Outcome == MaintenanceVisitOutcome.NeedsSecondVisit)
                {
                    request.Status = MaintenanceRequestStatus.NeedsSecondVisit;
                }
                else if (dto.Outcome == MaintenanceVisitOutcome.NeedsSparePart)
                {
                    request.Status = MaintenanceRequestStatus.NeedsSparePart;
                }
                else if (dto.Outcome == MaintenanceVisitOutcome.CannotComplete)
                {
                    // If cannot complete, set to OnHold for review
                    request.Status = MaintenanceRequestStatus.OnHold;
                    if (!string.IsNullOrEmpty(dto.Notes))
                    {
                        request.Notes = $"Cannot complete: {dto.Notes}";
                    }
                }

                await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Maintenance visit created. VisitId: {VisitId}, RequestId: {RequestId}", 
                    visit.Id, dto.MaintenanceRequestId);

                // Notify maintenance support
                await _notificationService.SendNotificationToRoleGroupAsync(
                    "MaintenanceSupport",
                    "Maintenance visit completed",
                    $"Visit #{visit.Id} for request #{dto.MaintenanceRequestId}",
                    new Dictionary<string, object> { { "MaintenanceRequestId", dto.MaintenanceRequestId }, { "VisitId", visit.Id } }
                );

                return await MapToResponseDTO(visit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance visit");
                throw;
            }
        }

        public async Task<MaintenanceVisitResponseDTO?> GetVisitAsync(int id)
        {
            var visit = await _unitOfWork.MaintenanceVisits.GetWithDetailsAsync(id);
            if (visit == null)
                return null;

            return await MapToResponseDTO(visit);
        }

        public async Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByRequestAsync(int maintenanceRequestId)
        {
            var visits = await _unitOfWork.MaintenanceVisits.GetByMaintenanceRequestIdAsync(maintenanceRequestId);
            var result = new List<MaintenanceVisitResponseDTO>();

            foreach (var visit in visits)
            {
                result.Add(await MapToResponseDTO(visit));
            }

            return result;
        }

        public async Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByEngineerAsync(string EngineerId)
        {
            var visits = await _unitOfWork.MaintenanceVisits.GetByEngineerIdAsync(EngineerId);
            var result = new List<MaintenanceVisitResponseDTO>();

            foreach (var visit in visits)
            {
                result.Add(await MapToResponseDTO(visit));
            }

            return result;
        }

        public async Task<EquipmentDTO?> GetEquipmentByQRCodeAsync(string qrCode)
        {
            var equipment = await _unitOfWork.Equipment.GetByQRCodeAsync(qrCode);
            if (equipment == null)
                return null;

            return new EquipmentDTO
            {
                Name = equipment.Name,
                QRCode = equipment.QRCode,
                Description = equipment.Description,
                Model = equipment.Model,
                Manufacturer = equipment.Manufacturer,
                PurchaseDate = equipment.PurchaseDate,
                WarrantyExpiry = equipment.WarrantyExpiry,
                HospitalId = equipment.HospitalId ?? "",
                Status = equipment.Status
            };
        }

        public async Task<EquipmentDTO?> GetEquipmentBySerialCodeAsync(string serialCode)
        {
            // Note: Serial code search would need to be added to Equipment model/repository
            // For now, this is a placeholder
            var equipment = await _unitOfWork.Equipment.GetFirstOrDefaultAsync(
                e => e.QRCode == serialCode || e.Name.Contains(serialCode));
            
            if (equipment == null)
                return null;

            return new EquipmentDTO
            {
                Name = equipment.Name,
                QRCode = equipment.QRCode,
                Description = equipment.Description,
                Model = equipment.Model,
                Manufacturer = equipment.Manufacturer,
                PurchaseDate = equipment.PurchaseDate,
                WarrantyExpiry = equipment.WarrantyExpiry,
                HospitalId = equipment.HospitalId ?? "",
                Status = equipment.Status
            };
        }

        private async Task<MaintenanceVisitResponseDTO> MapToResponseDTO(MaintenanceVisit visit)
        {
            var Engineer = await _userManager.FindByIdAsync(visit.EngineerId);

            return new MaintenanceVisitResponseDTO
            {
                Id = visit.Id,
                MaintenanceRequestId = visit.MaintenanceRequestId,
                EngineerId = visit.EngineerId,
                EngineerName = Engineer?.UserName ?? "",
                QRCode = visit.QRCode,
                SerialCode = visit.SerialCode,
                Report = visit.Report,
                ActionsTaken = visit.ActionsTaken,
                PartsUsed = visit.PartsUsed,
                ServiceFee = visit.ServiceFee,
                Outcome = visit.Outcome,
                SparePartRequestId = visit.SparePartRequestId,
                VisitDate = visit.VisitDate,
                StartedAt = visit.StartedAt,
                CompletedAt = visit.CompletedAt,
                Notes = visit.Notes
            };
        }
    }
}

