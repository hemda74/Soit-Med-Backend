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
                    // Try to get hospital from Doctor's hospitals
                    var Doctor = await _unitOfWork.Doctors.GetByUserIdAsync(customerId);
                    if (Doctor != null)
                    {
                        var DoctorHospital = await _unitOfWork.DoctorHospitals.GetFirstOrDefaultAsync(dh => dh.DoctorId == Doctor.DoctorId);
                        hospitalId = DoctorHospital?.HospitalId;
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

                // Try auto-assignment to Engineer
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

        public async Task<IEnumerable<MaintenanceRequestResponseDTO>> GetEngineerRequestsAsync(string EngineerId)
        {
            var requests = await _unitOfWork.MaintenanceRequests.GetByEngineerIdAsync(EngineerId);
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

            var Engineer = await _userManager.FindByIdAsync(dto.EngineerId);
            if (Engineer == null)
                throw new ArgumentException("Engineer not found", nameof(dto.EngineerId));

            request.AssignedToEngineerId = dto.EngineerId;
            request.AssignedByMaintenanceSupportId = maintenanceSupportId;
            request.AssignedAt = DateTime.UtcNow;
            request.Status = MaintenanceRequestStatus.Assigned;

            await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Maintenance request assigned. RequestId: {RequestId}, EngineerId: {EngineerId}", 
                requestId, dto.EngineerId);

            // Send notification to Engineer
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

            // Validate status transition
            if (!IsValidStatusTransition(request.Status, status))
            {
                throw new InvalidOperationException($"Cannot transition from {request.Status} to {status}");
            }

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
                if (request.StartedAt == null)
                {
                    request.StartedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Maintenance request status updated. RequestId: {RequestId}, Status: {Status}", 
                requestId, status);

            // Notify customer about status change
            await _notificationService.CreateNotificationAsync(
                request.CustomerId,
                "Maintenance request status updated",
                $"Request #{requestId} status changed to {status}",
                "MaintenanceRequest",
                "Medium",
                null,
                null,
                true,
                new Dictionary<string, object> { { "MaintenanceRequestId", requestId }, { "Status", status.ToString() } }
            );

            return await MapToResponseDTO(request);
        }

        public async Task<MaintenanceRequestResponseDTO> CancelRequestAsync(int requestId, string userId, string? reason = null)
        {
            var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(requestId);
            if (request == null)
                throw new ArgumentException("Maintenance request not found", nameof(requestId));

            // Check if request can be cancelled
            if (request.Status == MaintenanceRequestStatus.Completed)
            {
                throw new InvalidOperationException("Cannot cancel a completed request");
            }

            // Check if there's an active visit
            var activeVisits = await _unitOfWork.MaintenanceVisits.GetByMaintenanceRequestIdAsync(requestId);
            var hasActiveVisit = activeVisits.Any(v => v.IsActive && v.CompletedAt == null);
            if (hasActiveVisit)
            {
                throw new InvalidOperationException("Cannot cancel request with active visit in progress");
            }

            // Check permissions
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found", nameof(userId));

            var userRoles = await _userManager.GetRolesAsync(user);
            var isCustomer = request.CustomerId == userId;
            var isMaintenanceStaff = userRoles.Contains(UserRoles.MaintenanceSupport) || 
                                    userRoles.Contains(UserRoles.MaintenanceManager) ||
                                    userRoles.Contains(UserRoles.SuperAdmin);

            if (!isCustomer && !isMaintenanceStaff)
            {
                throw new UnauthorizedAccessException("You don't have permission to cancel this request");
            }

            request.Status = MaintenanceRequestStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
            {
                request.Notes = $"Cancelled: {reason}";
            }

            await _unitOfWork.MaintenanceRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Maintenance request cancelled. RequestId: {RequestId}, UserId: {UserId}", 
                requestId, userId);

            // Notify all stakeholders
            var notifications = new List<Task>();
            
            // Notify customer (if not the one cancelling)
            if (request.CustomerId != userId)
            {
                notifications.Add(_notificationService.CreateNotificationAsync(
                    request.CustomerId,
                    "Maintenance request cancelled",
                    $"Request #{requestId} has been cancelled",
                    "MaintenanceRequest",
                    "High",
                    null,
                    null,
                    true,
                    new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
                ));
            }

            // Notify Engineer if assigned
            if (request.AssignedToEngineerId != null && request.AssignedToEngineerId != userId)
            {
                notifications.Add(_notificationService.CreateNotificationAsync(
                    request.AssignedToEngineerId,
                    "Maintenance request cancelled",
                    $"Request #{requestId} has been cancelled",
                    "MaintenanceRequest",
                    "High",
                    null,
                    null,
                    true,
                    new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
                ));
            }

            // Notify maintenance support
            await _notificationService.SendNotificationToRoleGroupAsync(
                UserRoles.MaintenanceSupport,
                "Maintenance request cancelled",
                $"Request #{requestId} cancelled by {user.UserName}",
                new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
            );

            await Task.WhenAll(notifications);

            return await MapToResponseDTO(request);
        }

        private bool IsValidStatusTransition(MaintenanceRequestStatus currentStatus, MaintenanceRequestStatus newStatus)
        {
            // Define valid transitions
            var validTransitions = new Dictionary<MaintenanceRequestStatus, List<MaintenanceRequestStatus>>
            {
                { MaintenanceRequestStatus.Pending, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.Assigned, MaintenanceRequestStatus.Cancelled, MaintenanceRequestStatus.OnHold } },
                { MaintenanceRequestStatus.Assigned, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.InProgress, MaintenanceRequestStatus.Cancelled, MaintenanceRequestStatus.OnHold } },
                { MaintenanceRequestStatus.InProgress, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.Completed, MaintenanceRequestStatus.NeedsSecondVisit, MaintenanceRequestStatus.NeedsSparePart, MaintenanceRequestStatus.OnHold } },
                { MaintenanceRequestStatus.NeedsSecondVisit, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.Assigned, MaintenanceRequestStatus.InProgress, MaintenanceRequestStatus.Cancelled } },
                { MaintenanceRequestStatus.NeedsSparePart, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.WaitingForSparePart, MaintenanceRequestStatus.Cancelled } },
                { MaintenanceRequestStatus.WaitingForSparePart, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.WaitingForCustomerApproval, MaintenanceRequestStatus.InProgress, MaintenanceRequestStatus.Cancelled } },
                { MaintenanceRequestStatus.WaitingForCustomerApproval, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.InProgress, MaintenanceRequestStatus.Cancelled } },
                { MaintenanceRequestStatus.OnHold, new List<MaintenanceRequestStatus> { MaintenanceRequestStatus.Assigned, MaintenanceRequestStatus.InProgress, MaintenanceRequestStatus.Cancelled } },
            };

            // Completed and Cancelled are terminal states
            if (currentStatus == MaintenanceRequestStatus.Completed || currentStatus == MaintenanceRequestStatus.Cancelled)
            {
                return false;
            }

            if (validTransitions.TryGetValue(currentStatus, out var allowed))
            {
                return allowed.Contains(newStatus);
            }

            return false;
        }

        private async Task<MaintenanceRequestResponseDTO> MapToResponseDTO(MaintenanceRequest request)
        {
            var customer = await _userManager.FindByIdAsync(request.CustomerId);
            var Engineer = request.AssignedToEngineerId != null 
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
                EngineerName = Engineer?.UserName ?? "",
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
                AssignedToEngineerName = Engineer?.UserName ?? "",
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
                    // Notify maintenance support that manual assignment is needed
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        UserRoles.MaintenanceSupport,
                        "Manual assignment required",
                        $"Request #{requestId} requires manual assignment (no location found)",
                        new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
                    );
                    return;
                }

                // Get all active Engineers with their governorates
                var allEngineers = await _unitOfWork.Engineers.GetActiveEngineersAsync();
                var availableEngineers = new List<Engineer>();

                foreach (var Engineer in allEngineers)
                {
                    var EngineerWithGovs = await _unitOfWork.Engineers.GetEngineerWithGovernoratesAsync(Engineer.EngineerId);
                    if (EngineerWithGovs?.EngineerGovernorates != null)
                    {
                        var matches = EngineerWithGovs.EngineerGovernorates
                            .Where(eg => eg.IsActive && 
                                eg.Governorate != null && 
                                eg.Governorate.Name.Contains(location, StringComparison.OrdinalIgnoreCase));
                        
                        if (matches.Any())
                        {
                            availableEngineers.Add(Engineer);
                        }
                    }
                }

                if (!availableEngineers.Any())
                {
                    _logger.LogInformation("No available Engineers found for location {Location}", location);
                    // Notify maintenance support that manual assignment is needed
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        UserRoles.MaintenanceSupport,
                        "Manual assignment required",
                        $"Request #{requestId} requires manual assignment (no Engineers available for location: {location})",
                        new Dictionary<string, object> { { "MaintenanceRequestId", requestId } }
                    );
                    return;
                }

                // Get Engineer workloads (active maintenance requests)
                var allRequests = await _unitOfWork.MaintenanceRequests
                    .GetFilteredAsync(mr => mr.AssignedToEngineerId != null && 
                        mr.Status != MaintenanceRequestStatus.Completed && 
                        mr.Status != MaintenanceRequestStatus.Cancelled &&
                        mr.IsActive);

                var workloadDict = allRequests
                    .GroupBy(mr => mr.AssignedToEngineerId)
                    .ToDictionary(g => g.Key!, g => g.Count());

                // Select Engineer with least workload
                Engineer? selectedEngineer = null;
                int minWorkload = int.MaxValue;

                foreach (var Engineer in availableEngineers)
                {
                    if (string.IsNullOrEmpty(Engineer.UserId))
                        continue;

                    var workload = workloadDict.GetValueOrDefault(Engineer.UserId, 0);
                    if (workload < minWorkload)
                    {
                        minWorkload = workload;
                        selectedEngineer = Engineer;
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

                        _logger.LogInformation("Auto-assigned request {RequestId} to Engineer {EngineerId}", 
                            requestId, selectedEngineer.UserId);

                        // Notify Engineer
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

