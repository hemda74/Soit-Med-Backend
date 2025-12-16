using Microsoft.AspNetCore.Identity;
using SoitMed.DTO;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Collections.Generic;

namespace SoitMed.Services
{
    public class SparePartRequestService : ISparePartRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SparePartRequestService> _logger;

        public SparePartRequestService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<SparePartRequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<SparePartRequestResponseDTO> CreateSparePartRequestAsync(CreateSparePartRequestDTO dto, int maintenanceVisitId)
        {
            try
            {
                var visit = await _unitOfWork.MaintenanceVisits.GetByIdAsync(maintenanceVisitId);
                if (visit == null)
                    throw new ArgumentException("Maintenance visit not found", nameof(maintenanceVisitId));

                var sparePartRequest = new SparePartRequest
                {
                    MaintenanceRequestId = dto.MaintenanceRequestId,
                    MaintenanceVisitId = maintenanceVisitId,
                    PartName = dto.PartName,
                    PartNumber = dto.PartNumber,
                    Description = dto.Description,
                    Manufacturer = dto.Manufacturer,
                    Status = SparePartAvailabilityStatus.Checking
                };

                await _unitOfWork.SparePartRequests.CreateAsync(sparePartRequest);
                await _unitOfWork.SaveChangesAsync();

                // Link to visit
                visit.SparePartRequestId = sparePartRequest.Id;
                await _unitOfWork.MaintenanceVisits.UpdateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Update maintenance request status to WaitingForSparePart
                var maintenanceRequest = await _unitOfWork.MaintenanceRequests.GetByIdAsync(dto.MaintenanceRequestId);
                if (maintenanceRequest != null && maintenanceRequest.Status == MaintenanceRequestStatus.NeedsSparePart)
                {
                    maintenanceRequest.Status = MaintenanceRequestStatus.WaitingForSparePart;
                    await _unitOfWork.MaintenanceRequests.UpdateAsync(maintenanceRequest);
                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation("Spare part request created. RequestId: {RequestId}", sparePartRequest.Id);

                // Notify SparePartsCoordinator
                await _notificationService.SendNotificationToRoleGroupAsync(
                    "SparePartsCoordinator",
                    "New spare part request",
                    $"Part: {dto.PartName} for request #{dto.MaintenanceRequestId}",
                    new Dictionary<string, object> { { "SparePartRequestId", sparePartRequest.Id }, { "MaintenanceRequestId", dto.MaintenanceRequestId } }
                );

                return await MapToResponseDTO(sparePartRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating spare part request");
                throw;
            }
        }

        public async Task<SparePartRequestResponseDTO?> GetSparePartRequestAsync(int id)
        {
            var request = await _unitOfWork.SparePartRequests.GetWithDetailsAsync(id);
            if (request == null)
                return null;

            return await MapToResponseDTO(request);
        }

        public async Task<IEnumerable<SparePartRequestResponseDTO>> GetByMaintenanceRequestAsync(int maintenanceRequestId)
        {
            var requests = await _unitOfWork.SparePartRequests.GetByMaintenanceRequestIdAsync(maintenanceRequestId);
            var result = new List<SparePartRequestResponseDTO>();

            foreach (var request in requests)
            {
                result.Add(await MapToResponseDTO(request));
            }

            return result;
        }

        public async Task<SparePartRequestResponseDTO> CheckAvailabilityAsync(int sparePartRequestId, string coordinatorId, bool isLocalAvailable)
        {
            var request = await _unitOfWork.SparePartRequests.GetByIdAsync(sparePartRequestId);
            if (request == null)
                throw new ArgumentException("Spare part request not found", nameof(sparePartRequestId));

            request.AssignedToCoordinatorId = coordinatorId;
            request.CheckedAt = DateTime.UtcNow;

            if (isLocalAvailable)
            {
                request.Status = SparePartAvailabilityStatus.LocalAvailable;
                // Assign to InventoryManager
                // TODO: Auto-assign to available inventory manager
            }
            else
            {
                request.Status = SparePartAvailabilityStatus.GlobalRequired;
                // Notify MaintenanceManager to set price
                await _notificationService.SendNotificationToRoleGroupAsync(
                    "MaintenanceManager",
                    "Spare part requires global purchase",
                    $"Part: {request.PartName} - Please set price",
                    new Dictionary<string, object> { { "SparePartRequestId", sparePartRequestId } }
                );
            }

            await _unitOfWork.SparePartRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDTO(request);
        }

        public async Task<SparePartRequestResponseDTO> SetPriceAsync(int sparePartRequestId, UpdateSparePartPriceDTO dto, string managerId)
        {
            var request = await _unitOfWork.SparePartRequests.GetByIdAsync(sparePartRequestId);
            if (request == null)
                throw new ArgumentException("Spare part request not found", nameof(sparePartRequestId));

            request.CompanyPrice = dto.CompanyPrice;
            request.CustomerPrice = dto.CustomerPrice;
            request.PriceSetByManagerId = managerId;
            request.PriceSetAt = DateTime.UtcNow;
            request.Status = SparePartAvailabilityStatus.WaitingForCustomerApproval;

            await _unitOfWork.SparePartRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // Update maintenance request status and notify customer
            var maintenanceRequest = await _unitOfWork.MaintenanceRequests.GetByIdAsync(request.MaintenanceRequestId);
            if (maintenanceRequest != null)
            {
                if (maintenanceRequest.Status == MaintenanceRequestStatus.WaitingForSparePart)
                {
                    maintenanceRequest.Status = MaintenanceRequestStatus.WaitingForCustomerApproval;
                    await _unitOfWork.MaintenanceRequests.UpdateAsync(maintenanceRequest);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Notify customer
                await _notificationService.CreateNotificationAsync(
                    maintenanceRequest.CustomerId,
                    "Spare part price available",
                    $"Part: {request.PartName} - Price: {dto.CustomerPrice} EGP",
                    "SparePartRequest",
                    "High",
                    null,
                    null,
                    true,
                    new Dictionary<string, object> { { "SparePartRequestId", sparePartRequestId }, { "Price", dto.CustomerPrice } }
                );
            }

            return await MapToResponseDTO(request);
        }

        public async Task<SparePartRequestResponseDTO> CustomerDecisionAsync(int sparePartRequestId, CustomerSparePartDecisionDTO dto, string customerId)
        {
            var request = await _unitOfWork.SparePartRequests.GetByIdAsync(sparePartRequestId);
            if (request == null)
                throw new ArgumentException("Spare part request not found", nameof(sparePartRequestId));

            var maintenanceRequest = await _unitOfWork.MaintenanceRequests.GetByIdAsync(request.MaintenanceRequestId);
            if (maintenanceRequest?.CustomerId != customerId)
                throw new UnauthorizedAccessException("Customer does not own this request");

            request.CustomerApproved = dto.Approved;
            request.CustomerApprovedAt = dto.Approved ? DateTime.UtcNow : null;
            request.CustomerRejectionReason = dto.Approved ? null : dto.RejectionReason;

            if (dto.Approved)
            {
                request.Status = SparePartAvailabilityStatus.CustomerApproved;
                // Update maintenance request status back to InProgress
                if (maintenanceRequest != null)
                {
                    maintenanceRequest.Status = MaintenanceRequestStatus.InProgress;
                    await _unitOfWork.MaintenanceRequests.UpdateAsync(maintenanceRequest);
                    await _unitOfWork.SaveChangesAsync();
                }
                // Create payment
                // TODO: Integrate with PaymentService
            }
            else
            {
                request.Status = SparePartAvailabilityStatus.CustomerRejected;
                // Update maintenance request status - could be cancelled or back to NeedsSparePart
                if (maintenanceRequest != null)
                {
                    maintenanceRequest.Status = MaintenanceRequestStatus.NeedsSparePart;
                    await _unitOfWork.MaintenanceRequests.UpdateAsync(maintenanceRequest);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            await _unitOfWork.SparePartRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDTO(request);
        }

        public async Task<SparePartRequestResponseDTO> MarkAsReadyAsync(int sparePartRequestId, string inventoryManagerId)
        {
            var request = await _unitOfWork.SparePartRequests.GetByIdAsync(sparePartRequestId);
            if (request == null)
                throw new ArgumentException("Spare part request not found", nameof(sparePartRequestId));

            request.AssignedToInventoryManagerId = inventoryManagerId;
            request.Status = SparePartAvailabilityStatus.ReadyForEngineer;
            request.ReadyAt = DateTime.UtcNow;

            await _unitOfWork.SparePartRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            // Notify Engineer
            var maintenanceRequest = await _unitOfWork.MaintenanceRequests.GetByIdAsync(request.MaintenanceRequestId);
            if (maintenanceRequest?.AssignedToEngineerId != null)
            {
                await _notificationService.CreateNotificationAsync(
                    maintenanceRequest.AssignedToEngineerId,
                    "Spare part ready",
                    $"Part: {request.PartName} is ready for pickup",
                    "SparePartRequest",
                    "High",
                    null,
                    null,
                    true,
                    new Dictionary<string, object> { { "SparePartRequestId", sparePartRequestId } }
                );
            }

            return await MapToResponseDTO(request);
        }

        public async Task<SparePartRequestResponseDTO> MarkAsDeliveredToEngineerAsync(int sparePartRequestId)
        {
            var request = await _unitOfWork.SparePartRequests.GetByIdAsync(sparePartRequestId);
            if (request == null)
                throw new ArgumentException("Spare part request not found", nameof(sparePartRequestId));

            request.Status = SparePartAvailabilityStatus.DeliveredToEngineer;
            request.DeliveredToEngineerAt = DateTime.UtcNow;

            await _unitOfWork.SparePartRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return await MapToResponseDTO(request);
        }

        private async Task<SparePartRequestResponseDTO> MapToResponseDTO(SparePartRequest request)
        {
            var coordinator = request.AssignedToCoordinatorId != null
                ? await _userManager.FindByIdAsync(request.AssignedToCoordinatorId)
                : null;
            var inventoryManager = request.AssignedToInventoryManagerId != null
                ? await _userManager.FindByIdAsync(request.AssignedToInventoryManagerId)
                : null;
            var manager = request.PriceSetByManagerId != null
                ? await _userManager.FindByIdAsync(request.PriceSetByManagerId)
                : null;

            return new SparePartRequestResponseDTO
            {
                Id = request.Id,
                MaintenanceRequestId = request.MaintenanceRequestId,
                MaintenanceVisitId = request.MaintenanceVisitId,
                PartName = request.PartName,
                PartNumber = request.PartNumber,
                Description = request.Description,
                Manufacturer = request.Manufacturer,
                OriginalPrice = request.OriginalPrice,
                CompanyPrice = request.CompanyPrice,
                CustomerPrice = request.CustomerPrice,
                Status = request.Status,
                AssignedToCoordinatorId = request.AssignedToCoordinatorId,
                AssignedToCoordinatorName = coordinator?.UserName ?? "",
                AssignedToInventoryManagerId = request.AssignedToInventoryManagerId,
                AssignedToInventoryManagerName = inventoryManager?.UserName ?? "",
                PriceSetByManagerId = request.PriceSetByManagerId,
                PriceSetByManagerName = manager?.UserName ?? "",
                CustomerApproved = request.CustomerApproved,
                CustomerApprovedAt = request.CustomerApprovedAt,
                CustomerRejectionReason = request.CustomerRejectionReason,
                CreatedAt = request.CreatedAt,
                CheckedAt = request.CheckedAt,
                PriceSetAt = request.PriceSetAt,
                ReadyAt = request.ReadyAt,
                DeliveredToEngineerAt = request.DeliveredToEngineerAt
            };
        }
    }
}

