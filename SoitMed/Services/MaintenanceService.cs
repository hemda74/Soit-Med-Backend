using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.Common.DomainEvents;
using SoitMed.Common.Exceptions;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Models.Core;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing maintenance visit lifecycle
    /// Implements state machine pattern, audit logging, and domain events
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVisitStateService _visitStateService;
        private readonly IAuditService _auditService;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IVisitStateService visitStateService,
            IAuditService auditService,
            IDomainEventDispatcher domainEventDispatcher,
            INotificationService notificationService,
            ICacheService cacheService,
            ILogger<MaintenanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _visitStateService = visitStateService;
            _auditService = auditService;
            _domainEventDispatcher = domainEventDispatcher;
            _notificationService = notificationService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new maintenance visit with proper state management
        /// </summary>
        public async Task<MaintenanceVisit> CreateVisitAsync(CreateVisitDTO dto, string userId)
        {
            using var transaction = new TransactionScope(TransactionScopeOption.Required, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, 
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // Get user and role
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("User not found", nameof(userId));

                var userRoles = await _userManager.GetRolesAsync(user);
                var userRole = userRoles.FirstOrDefault() ?? string.Empty;

                // Get maintenance request
                var request = await _unitOfWork.MaintenanceRequests.GetByIdAsync(dto.MaintenanceRequestId);
                if (request == null)
                    throw new ArgumentException("Maintenance request not found", nameof(dto.MaintenanceRequestId));

                // Get equipment/device
                var device = await _unitOfWork.Equipment.GetByIdAsync(dto.DeviceId);
                if (device == null)
                    throw new ArgumentException("Device not found", nameof(dto.DeviceId));

                // Determine initial status based on user role
                var initialStatus = _visitStateService.GetInitialState(userRole);

                // Generate ticket number
                var ticketNumber = await GenerateTicketNumberAsync();

                // Create visit
                var visit = new MaintenanceVisit
                {
                    TicketNumber = ticketNumber,
                    MaintenanceRequestId = dto.MaintenanceRequestId,
                    CustomerId = request.CustomerId,
                    DeviceId = dto.DeviceId,
                    ScheduledDate = dto.ScheduledDate,
                    Origin = dto.Origin,
                    Status = initialStatus,
                    EngineerId = dto.EngineerId ?? string.Empty, // Will be updated when engineers are assigned
                    IsPaidVisit = dto.IsPaidVisit,
                    Cost = dto.Cost,
                    VisitDate = DateTime.UtcNow,
                    Outcome = MaintenanceVisitOutcome.NeedsSecondVisit // Default, will be updated
                };

                await _unitOfWork.MaintenanceVisits.CreateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Assign engineers if provided
                if (dto.EngineerIds != null && dto.EngineerIds.Any())
                {
                    await AssignEngineersInternalAsync(visit.Id, dto.EngineerIds, userId);
                    // Set primary engineer
                    visit.EngineerId = dto.EngineerIds.First();
                }

                await _unitOfWork.MaintenanceVisits.UpdateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Log creation
                await _auditService.LogEntityChangeAsync<MaintenanceVisit>(null, visit, userId, "Created", 
                    $"Visit {ticketNumber} created by {userRole}");

                // Fire domain event if scheduled
                if (initialStatus == VisitStatus.Scheduled)
                {
                    await FireVisitScheduledEventAsync(visit);
                }

                transaction.Complete();

                _logger.LogInformation("Maintenance visit created: {TicketNumber}, Status: {Status}, User: {UserId}",
                    ticketNumber, initialStatus, userId);

                return visit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance visit. UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Approves a pending visit (transitions to Scheduled)
        /// </summary>
        public async Task<MaintenanceVisit> ApproveVisitAsync(int visitId, string approverId)
        {
            using var transaction = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var visit = await _unitOfWork.MaintenanceVisits.GetByIdAsync(visitId);
                if (visit == null)
                    throw new ArgumentException("Visit not found", nameof(visitId));

                var oldStatus = visit.Status;

                // Validate transition
                _visitStateService.ValidateTransition(oldStatus, VisitStatus.Scheduled);

                // Update status
                visit.Status = VisitStatus.Scheduled;
                await _unitOfWork.MaintenanceVisits.UpdateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Log status change
                await _auditService.LogStatusChangeAsync(visitId, oldStatus, VisitStatus.Scheduled, approverId,
                    "Visit approved by manager");

                // Fire domain event
                await FireVisitScheduledEventAsync(visit);

                transaction.Complete();

                _logger.LogInformation("Visit {VisitId} approved. Status: {OldStatus} -> {NewStatus}",
                    visitId, oldStatus, VisitStatus.Scheduled);

                return visit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving visit {VisitId}", visitId);
                throw;
            }
        }

        /// <summary>
        /// Assigns engineers to a visit
        /// </summary>
        public async Task<MaintenanceVisit> AssignEngineersAsync(int visitId, List<string> engineerIds, string assignedById)
        {
            using var transaction = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                await AssignEngineersInternalAsync(visitId, engineerIds, assignedById);

                var visit = await _unitOfWork.MaintenanceVisits.GetByIdAsync(visitId);
                if (visit == null)
                    throw new ArgumentException("Visit not found", nameof(visitId));

                // Update primary engineer if not set
                if (string.IsNullOrEmpty(visit.EngineerId) && engineerIds.Any())
                {
                    visit.EngineerId = engineerIds.First();
                    await _unitOfWork.MaintenanceVisits.UpdateAsync(visit);
                }

                await _unitOfWork.SaveChangesAsync();

                // Log assignment change
                await _auditService.LogAssignmentChangeAsync(visitId, null, 
                    string.Join(",", engineerIds), assignedById, 
                    $"Assigned {engineerIds.Count} engineer(s)");

                transaction.Complete();

                _logger.LogInformation("Engineers assigned to visit {VisitId}: {EngineerIds}",
                    visitId, string.Join(", ", engineerIds));

                return visit!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning engineers to visit {VisitId}", visitId);
                throw;
            }
        }

        private async Task AssignEngineersInternalAsync(int visitId, List<string> engineerIds, string assignedById)
        {
            var context = _unitOfWork.GetContext();

            // Remove existing assignments
            var existingAssignments = await context.VisitAssignees
                .Where(va => va.VisitId == visitId)
                .ToListAsync();

            context.VisitAssignees.RemoveRange(existingAssignments);

            // Add new assignments
            foreach (var engineerId in engineerIds)
            {
                var engineer = await _userManager.FindByIdAsync(engineerId);
                if (engineer == null)
                {
                    _logger.LogWarning("Engineer {EngineerId} not found, skipping assignment", engineerId);
                    continue;
                }

                var assignment = new VisitAssignees
                {
                    VisitId = visitId,
                    EngineerId = engineerId,
                    AssignedById = assignedById,
                    AssignedAt = DateTime.UtcNow
                };

                await context.VisitAssignees.AddAsync(assignment);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifies machine QR code and starts visit (transitions to InProgress)
        /// </summary>
        public async Task<MaintenanceVisit> VerifyMachineAndStartVisitAsync(int visitId, string scannedQrCode, string engineerId)
        {
            using var transaction = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                var visit = await _unitOfWork.MaintenanceVisits.GetWithDetailsAsync(visitId);
                if (visit == null)
                    throw new ArgumentException("Visit not found", nameof(visitId));

                // Verify engineer is assigned
                var isAssigned = visit.EngineerId == engineerId || 
                                visit.Assignees.Any(a => a.EngineerId == engineerId);
                if (!isAssigned)
                    throw new SecurityException("VerifyMachineAndStartVisit", visitId.ToString(),
                        "Engineer is not assigned to this visit");

                // Get device (with caching)
                var device = await _cacheService.GetOrCreateAsync(
                    $"Device:Id:{visit.DeviceId}",
                    async () => visit.Device ?? await _unitOfWork.Equipment.GetByIdAsync(visit.DeviceId),
                    TimeSpan.FromHours(1)
                );

                if (device == null)
                    throw new ArgumentException("Device not found");

                // Also cache by QR code for faster lookup
                if (!string.IsNullOrEmpty(device.QRCode))
                {
                    await _cacheService.SetAsync(
                        $"Device:QRCode:{device.QRCode}",
                        device,
                        TimeSpan.FromHours(1)
                    );
                }

                // Verify QR code match
                if (!string.Equals(device.QRCode, scannedQrCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException("VerifyMachineAndStartVisit", visitId.ToString(),
                        $"QR code mismatch. Expected: {device.QRCode}, Scanned: {scannedQrCode}");
                }

                var oldStatus = visit.Status;

                // Validate transition
                _visitStateService.ValidateTransition(oldStatus, VisitStatus.InProgress);

                // Update status and start time
                visit.Status = VisitStatus.InProgress;
                visit.StartedAt = DateTime.UtcNow;
                await _unitOfWork.MaintenanceVisits.UpdateAsync(visit);
                await _unitOfWork.SaveChangesAsync();

                // Log status change
                await _auditService.LogStatusChangeAsync(visitId, oldStatus, VisitStatus.InProgress, engineerId,
                    "Visit started after QR code verification");

                transaction.Complete();

                _logger.LogInformation("Visit {VisitId} started. QR code verified. Status: {OldStatus} -> {NewStatus}",
                    visitId, oldStatus, VisitStatus.InProgress);

                return visit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying machine and starting visit {VisitId}", visitId);
                throw;
            }
        }

        /// <summary>
        /// Generates a unique ticket number
        /// </summary>
        private async Task<string> GenerateTicketNumberAsync()
        {
            var context = _unitOfWork.GetContext();
            string ticketNumber;
            bool isUnique = false;
            int attempt = 0;
            const int maxAttempts = 10;

            do
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
                var random = new Random().Next(1000, 9999);
                ticketNumber = $"VISIT-{timestamp}-{random}";

                isUnique = !await context.MaintenanceVisits
                    .AnyAsync(v => v.TicketNumber == ticketNumber);

                attempt++;
            } while (!isUnique && attempt < maxAttempts);

            if (!isUnique)
                throw new InvalidOperationException("Failed to generate unique ticket number");

            return ticketNumber;
        }

        /// <summary>
        /// Fires VisitScheduledEvent domain event
        /// </summary>
        private async Task FireVisitScheduledEventAsync(MaintenanceVisit visit)
        {
            try
            {
                var customer = await _userManager.FindByIdAsync(visit.CustomerId);
                var device = visit.Device ?? await _unitOfWork.Equipment.GetByIdAsync(visit.DeviceId);
                var engineerIds = visit.Assignees.Select(a => a.EngineerId).ToList();
                if (!string.IsNullOrEmpty(visit.EngineerId) && !engineerIds.Contains(visit.EngineerId))
                    engineerIds.Add(visit.EngineerId);

                var domainEvent = new VisitScheduledEvent(
                    visit.Id,
                    visit.TicketNumber,
                    visit.CustomerId,
                    customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : "Unknown",
                    visit.DeviceId,
                    device?.Name ?? "Unknown Device",
                    visit.ScheduledDate,
                    visit.Origin,
                    engineerIds
                );

                await _domainEventDispatcher.DispatchAsync(domainEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error firing VisitScheduledEvent for visit {VisitId}", visit.Id);
                // Don't throw - event dispatching should not break the main operation
            }
        }
    }

    /// <summary>
    /// DTO for creating a visit
    /// </summary>
    public class CreateVisitDTO
    {
        public int MaintenanceRequestId { get; set; }
        public int DeviceId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public VisitOrigin Origin { get; set; }
        public string? EngineerId { get; set; }
        public List<string>? EngineerIds { get; set; }
        public bool IsPaidVisit { get; set; }
        public decimal? Cost { get; set; }
    }
}

