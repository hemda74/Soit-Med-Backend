using Microsoft.AspNetCore.Identity;
using SoitMed.DTO;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Models.Core;
using SoitMed.Models.Location;

namespace SoitMed.Services
{
    public class MaintenanceRequestService : IMaintenanceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MaintenanceRequestService> _logger;

        public MaintenanceRequestService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<MaintenanceRequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<MaintenanceRequestResponseDTO> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDTO dto, string customerId)
        {
            try
            {
                var customer = await _userManager.FindByIdAsync(customerId);
                if (customer == null)
                    throw new ArgumentException("Customer not found", nameof(customerId));

                var equipment = await _unitOfWork.Equipment.GetByIdAsync(dto.EquipmentId);
                if (equipment == null)
                    throw new ArgumentException("Equipment not found", nameof(dto.EquipmentId));

                // Determine customer type from roles
                var customerRoles = await _userManager.GetRolesAsync(customer);
                var customerType = customerRoles.Contains(UserRoles.Doctor) ? "Doctor" :
                                  customerRoles.Contains(UserRoles.Technician) ? "Technician" :
                                  "Customer";

                // Get hospital ID if customer is linked to hospital
                string? hospitalId = null;
                if (equipment.HospitalId != null)
                {
                    hospitalId = equipment.HospitalId;
                }
                else if (customerRoles.Contains(UserRoles.Doctor))
                {
                    // Try to get hospital from doctor's hospitals
                    var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(customerId);
                    if (doctor != null)
                    {
                        var doctorHospital = await _unitOfWork.DoctorHospitals.GetFirstOrDefaultAsync(dh => dh.DoctorId == doctor.DoctorId);
                        hospitalId = doctorHospital?.HospitalId;
                    }
                }

                var request = new MaintenanceRequest
                {
                    CustomerId = customerId,
                    CustomerType = customerType,
                    HospitalId = hospitalId,
                    EquipmentId = dto.EquipmentId,
                    Description = dto.Description,
                    Symptoms = dto.Symptoms,
                    Status = MaintenanceRequestStatus.Pending,
                    PaymentStatus = PaymentStatus.NotRequired
                };

                await _unitOfWork.MaintenanceRequests.CreateAsync(request);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Maintenance request created. RequestId: {RequestId}, CustomerId: {CustomerId}", 
                    request.Id, customerId);

                // Try auto-assignment to engineer
                await TryAutoAssignToEngineerAsync(request.Id, equipment);

                // Send notification to Maintenance Support
                await _notificationService.SendNotificationToRoleGroupAsync(
                    UserRoles.MaintenanceSupport,
                    $"New maintenance request from {customer.UserName}",
                    $"Equipment: {equipment.Name}",
                    new Dictionary<string, object> { { "MaintenanceRequestId", request.Id } }
                );

                return await MapToResponseDTO(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request. CustomerId: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<MaintenanceRequestResponseDTO?> GetMaintenanceRequestAsync(int id)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetWithDetailsAsync(id);
            if (request == null)
                return null;

            return await MapToResponseDTO(request);
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetCustomerRequestsAsync(string customerId)
        {
            var requests = await _unitOfWork.MaintenanceRequests.GetByCustomerIdAsync(customerId);
            var result = new List<MaintenanceRequestResponseDTO>();

            foreach (var request in requests)
            {
                result.Add(await MapToResponseDTO(request));
            }

            return result;
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetEngineerRequestsAsync(string engineerId)
        {
            var requests = await _unitOfWork.MaintenanceRequests.GetByEngineerIdAsync(engineerId);
            var result = new List<MaintenanceRequestResponseDTO>();

            foreach (var request in requests)
            {
                result.Add(await MapToResponseDTO(request));
            }

            return result;
        }

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetPendingRequestsAsync()
        {
            var requests = await _unitOfWork.MaintenanceRequests.GetPendingRequestsAsync();
            var result = new List<MaintenanceRequestResponseDTO>();

            foreach (var request in requests)
            {
                result.Add(await MapToResponseDTO(request));
            }

            return result;
        }

        public async Task<MaintenanceRequestResponseDTO> AssignToEngineerAsync(int requestId, AssignMaintenanceRequestDTO dto, string maintenanceSupportId)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("Maintenance request not found", nameof(requestId));

            var engineer = await _userManager.FindByIdAsync(dto.EngineerId);
            if (engineer == null)
                throw new ArgumentException("Engineer not found", nameof(dto.EngineerId));

            request.AssignedToEngineerId = dto.EngineerId;
            request.AssignedByMaintenanceSupportId = maintenanceSupportId;
            request.AssignedAt = DateTime.UtcNow;
            request.Status = MaintenanceRequestStatus.Assigned;

            await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Maintenance request assigned. RequestId: {RequestId}, EngineerId: {EngineerId}", 
                requestId, dto.EngineerId);

            // Send notification to engineer
            await _notificationService.CreateNotificationAsync(
                dto.EngineerId,
                "New maintenance request assigned",
                $"Request #{requestId}: {request.Description}",
                "MaintenanceRequest",
                "High",
                null,
                null,
                true,
                new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
            );

            return await MapToResponseDTO(request);
        }

        public async Task<MaintenanceRequestResponseDTO> UpdateStatusAsync(int requestId, MaintenanceRequestStatus status, string? notes = null)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("Maintenance request not found", nameof(requestId));

            request.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                request.Notes = notes;
            }

            if (status == MaintenanceRequestStatus.Completed)
            {
                request.CompletedAt = DateTime.UtcNow;
            }
            else if (status == MaintenanceRequestStatus.InProgress)
            {
                request.StartedAt = DateTime.UtcNow;
            }

            await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDTO(request);
        }

        private async Task<MaintenanceRequestResponseDTO> MapToResponseDTO(MaintenanceRequest request)
        {
            var customer = await _userManager.FindByIdAsync(request.CustomerId);
            var engineer = request.AssignedToEngineerId != null 
                ? await _userManager.FindByIdAsync(request.AssignedToEngineerId) 
                : null;
            var equipment = await _unitOfWork.Equipment.GetByIdAsync(request.EquipmentId);
            var hospital = request.HospitalId != null 
                ? await _unitOfWork.Hospitals.GetByIdAsync(request.HospitalId) 
                : null;

            var attachments = request.Attachments?.Select(a => new MaintenanceRequestAttachmentResponseDTO
            {
                Id = a.Id,
                MaintenanceRequestId = a.MaintenanceRequestId,
                FilePath = a.FilePath,
                FileName = a.FileName,
                FileType = a.FileType,
                FileSize = a.FileSize,
                AttachmentType = a.AttachmentType,
                Description = a.Description,
                UploadedAt = a.UploadedAt
            }).ToList() ?? new List<MaintenanceRequestAttachmentResponseDTO>();

            var visits = request.Visits?.Select(v => new MaintenanceVisitResponseDTO
            {
                Id = v.Id,
                MaintenanceRequestId = v.MaintenanceRequestId,
                EngineerId = v.EngineerId,
                EngineerName = engineer?.UserName ?? "",
                QRCode = v.QRCode,
                SerialCode = v.SerialCode,
                Report = v.Report,
                ActionsTaken = v.ActionsTaken,
                PartsUsed = v.PartsUsed,
                ServiceFee = v.ServiceFee,
                Outcome = v.Outcome,
                SparePartRequestId = v.SparePartRequestId,
                VisitDate = v.VisitDate,
                StartedAt = v.StartedAt,
                CompletedAt = v.CompletedAt,
                Notes = v.Notes
            }).ToList() ?? new List<MaintenanceVisitResponseDTO>();

            var payments = request.Payments?.Select(p => new PaymentResponseDTO
            {
                Id = p.Id,
                MaintenanceRequestId = p.MaintenanceRequestId,
                CustomerId = p.CustomerId,
                CustomerName = customer?.UserName ?? "",
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                PaymentMethodName = p.PaymentMethod.ToString(),
                Status = p.Status,
                StatusName = p.Status.ToString(),
                TransactionId = p.TransactionId,
                PaymentReference = p.PaymentReference,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt,
                ConfirmedAt = p.ConfirmedAt
            }).ToList() ?? new List<PaymentResponseDTO>();

            return new MaintenanceRequestResponseDTO
            {
                Id = request.Id,
                CustomerId = request.CustomerId,
                CustomerName = customer?.UserName ?? "",
                CustomerType = request.CustomerType,
                HospitalId = request.HospitalId,
                HospitalName = hospital?.Name ?? "",
                EquipmentId = request.EquipmentId,
                EquipmentName = equipment?.Name ?? "",
                EquipmentQRCode = equipment?.QRCode ?? "",
                Description = request.Description,
                Symptoms = request.Symptoms,
                Status = request.Status,
                AssignedToEngineerId = request.AssignedToEngineerId,
                AssignedToEngineerName = engineer?.UserName ?? "",
                PaymentStatus = request.PaymentStatus,
                TotalAmount = request.TotalAmount,
                PaidAmount = request.PaidAmount,
                RemainingAmount = request.RemainingAmount,
                CreatedAt = request.CreatedAt,
                StartedAt = request.StartedAt,
                CompletedAt = request.CompletedAt,
                Attachments = attachments,
                Visits = visits,
                Payments = payments
            };
        }

        private async Task TryAutoAssignToEngineerAsync(int requestId, Models.Equipment.Equipment equipment)
        {
            try
            {
                string? location = null;

                // Get location from hospital
                if (equipment.HospitalId != null)
                {
                    var hospital = await _unitOfWork.Hospitals.GetByIdAsync(equipment.HospitalId);
                    if (hospital != null)
                    {
                        location = hospital.Location;
                    }
                }
                else if (equipment.CustomerId != null)
                {
                    // For customer-linked equipment, skip auto-assignment
                    _logger.LogInformation("Equipment linked to customer directly, skipping auto-assignment");
                    return;
                }

                if (string.IsNullOrEmpty(location))
                {
                    _logger.LogInformation("No location found for equipment, skipping auto-assignment");
                    return;
                }

                // Get all active engineers with their governorates
                var allEngineers = await _unitOfWork.Engineers.GetActiveEngineersAsync();
                var availableEngineers = new List<Engineer>();

                foreach (var engineer in allEngineers)
                {
                    var engineerWithGovs = await _unitOfWork.Engineers.GetEngineerWithGovernoratesAsync(engineer.EngineerId);
                    if (engineerWithGovs?.EngineerGovernorates != null)
                    {
                        var matches = engineerWithGovs.EngineerGovernorates
                            .Where(eg => eg.IsActive && 
                                eg.Governorate != null && 
                                eg.Governorate.Name.Contains(location, StringComparison.OrdinalIgnoreCase));
                        
                        if (matches.Any())
                        {
                            availableEngineers.Add(engineer);
                        }
                    }
                }

                if (!availableEngineers.Any())
                {
                    _logger.LogInformation("No available engineers found for location {Location}", location);
                    return;
                }

                // Get engineer workloads (active maintenance requests)
                var allRequests = await _unitOfWork.MaintenanceRequests
                    .GetFilteredAsync(mr => mr.AssignedToEngineerId != null && 
                        mr.Status != MaintenanceRequestStatus.Completed && 
                        mr.Status != MaintenanceRequestStatus.Cancelled &&
                        mr.IsActive);

                var workloadDict = allRequests
                    .GroupBy(mr => mr.AssignedToEngineerId)
                    .ToDictionary(g => g.Key!, g => g.Count());

                // Select engineer with least workload
                Engineer? selectedEngineer = null;
                int minWorkload = int.MaxValue;

                foreach (var engineer in availableEngineers)
                {
                    if (string.IsNullOrEmpty(engineer.UserId))
                        continue;

                    var workload = workloadDict.GetValueOrDefault(engineer.UserId, 0);
                    if (workload < minWorkload)
                    {
                        minWorkload = workload;
                        selectedEngineer = engineer;
                    }
                }

                if (selectedEngineer != null && !string.IsNullOrEmpty(selectedEngineer.UserId))
                {
                    var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
                    if (request != null)
                    {
                        request.AssignedToEngineerId = selectedEngineer.UserId;
                        request.AssignedAt = DateTime.UtcNow;
                        request.Status = MaintenanceRequestStatus.Assigned;

                        await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Auto-assigned request {RequestId} to engineer {EngineerId}", 
                            requestId, selectedEngineer.UserId);

                        // Notify engineer
                        await _notificationService.CreateNotificationAsync(
                            selectedEngineer.UserId,
                            "New maintenance request assigned",
                            $"Request #{requestId}: {request.Description}",
                            "MaintenanceRequest",
                            "High",
                            null,
                            null,
                            true,
                            new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in auto-assignment for request {RequestId}", requestId);
                // Don't throw - auto-assignment is optional
            }
        }
    }
}

