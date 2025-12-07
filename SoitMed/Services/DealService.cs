using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing deals with multi-level approval workflow
    /// </summary>
    public class DealService : IDealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DealService> _logger;

        public DealService(
            IUnitOfWork unitOfWork, 
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<DealService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Deal Management

        public async Task<DealResponseDTO> CreateDealAsync(CreateDealDTO createDealDto, string salesmanId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(createDealDto.OfferId);
                if (offer == null)
                    throw new ArgumentException("Offer not found", nameof(createDealDto.OfferId));

                if (offer.Status != "Accepted")
                    throw new InvalidOperationException("Only accepted offers can be converted to deals");

                var deal = new SalesDeal
                {
                    OfferId = createDealDto.OfferId,
                    ClientId = createDealDto.ClientId,
                    SalesmanId = salesmanId,
                    DealValue = createDealDto.DealValue,
                    ClosedDate = DateTime.UtcNow,
                    PaymentTerms = createDealDto.PaymentTerms,
                    DeliveryTerms = createDealDto.DeliveryTerms,
                    ExpectedDeliveryDate = createDealDto.ExpectedDeliveryDate,
                    Notes = createDealDto.Notes,
                    Status = "PendingSuperAdminApproval"
                };

                await _unitOfWork.SalesDeals.CreateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal created successfully. DealId: {DealId}, OfferId: {OfferId}", deal.Id, createDealDto.OfferId);

                // Send notification to SuperAdmin for approval (deals now go directly to SuperAdmin approval)
                await SendSuperAdminApprovalNotificationAsync(deal);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deal. OfferId: {OfferId}", createDealDto.OfferId);
                throw;
            }
        }

        public async Task<DealResponseDTO?> GetDealAsync(long dealId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    return null;

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deal. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO?> GetDealAsync(long dealId, string userId, string userRole)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    return null;

                // Check authorization:
                // - SuperAdmin and SalesManager can view any deal
                // - Salesman can only view their own deals
                if (userRole != "SuperAdmin" && userRole != "SalesManager" && deal.SalesmanId != userId)
                {
                    _logger.LogWarning("Unauthorized access attempt - UserId: {UserId}, UserRole: {UserRole}, DealId: {DealId}, DealSalesmanId: {DealSalesmanId}",
                        userId, userRole, dealId, deal.SalesmanId);
                    throw new UnauthorizedAccessException("You don't have permission to view this deal");
                }

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deal. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsAsync(string? status, string? salesmanId, string userId, string userRole)
        {
            try
            {
                var query = _unitOfWork.SalesDeals.GetQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(d => d.Status == status);

                if (!string.IsNullOrEmpty(salesmanId))
                    query = query.Where(d => d.SalesmanId == salesmanId);

                // Apply authorization
                if (userRole == "Salesman")
                {
                    query = query.Where(d => d.SalesmanId == userId);
                }
                // SalesManager and SuperAdmin can see all

                var deals = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals");
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsByClientAsync(long clientId, string userId, string userRole)
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsByClientIdAsync(clientId);
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    // Check authorization - SuperAdmin and SalesManager can see all deals, Salesman can only see their own
                    if (userRole == "SuperAdmin" || userRole == "SalesManager" || deal.SalesmanId == userId)
                    {
                        result.Add(await MapToDealResponseDTO(deal));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals by client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsByClientAsync(long clientId)
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsByClientIdAsync(clientId);
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals by client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsBySalesmanAsync(string salesmanId)
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsBySalesmanAsync(salesmanId);
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals by salesman. SalesmanId: {SalesmanId}", salesmanId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsByStatusAsync(string status)
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsByStatusAsync(status);
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals by status. Status: {Status}", status);
                throw;
            }
        }

        public async Task<DealResponseDTO> UpdateDealAsync(long dealId, CreateDealDTO updateDealDto, string userId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                // Only allow updates if deal is pending manager approval
                if (deal.Status != "PendingManagerApproval")
                    throw new InvalidOperationException("Only pending deals can be updated");

                deal.DealValue = updateDealDto.DealValue;
                deal.PaymentTerms = updateDealDto.PaymentTerms;
                deal.DeliveryTerms = updateDealDto.DeliveryTerms;
                deal.ExpectedDeliveryDate = updateDealDto.ExpectedDeliveryDate;
                deal.Notes = updateDealDto.Notes;
                deal.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal updated successfully. DealId: {DealId}", dealId);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deal. DealId: {DealId}", dealId);
                throw;
            }
        }

        #endregion

        #region Approval Workflow

        public async Task<DealResponseDTO> ManagerApprovalAsync(long dealId, ApproveDealDTO approvalDto, string managerId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "PendingManagerApproval")
                    throw new InvalidOperationException("Deal is not pending manager approval");

                if (approvalDto.Approved)
                {
                    deal.ApproveByManager(managerId, approvalDto.Comments);
                }
                else
                {
                    if (string.IsNullOrEmpty(approvalDto.RejectionReason))
                        throw new ArgumentException("Rejection reason is required when rejecting a deal");

                    deal.RejectByManager(managerId, approvalDto.RejectionReason, approvalDto.Comments);
                }

                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Manager approval processed for deal. DealId: {DealId}, Approved: {Approved}", 
                    dealId, approvalDto.Approved);

                // If approved, send notification to Super Admins for next level approval
                if (approvalDto.Approved && deal.Status == "PendingSuperAdminApproval")
                {
                    await SendSuperAdminApprovalNotificationAsync(deal);
                }

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing manager approval. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> SuperAdminApprovalAsync(long dealId, ApproveDealDTO approvalDto, string superAdminId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                // SuperAdmin can approve deals that are either:
                // 1. PendingSuperAdminApproval (new deals created after client response)
                // 2. PendingManagerApproval (legacy deals - SalesManager only approves offers, not deals)
                if (deal.Status != "PendingSuperAdminApproval" && deal.Status != "PendingManagerApproval")
                    throw new InvalidOperationException($"Deal is not pending approval. Current status: {deal.Status}");

                if (approvalDto.Approved)
                {
                    deal.ApproveBySuperAdmin(superAdminId, approvalDto.Comments);
                    // Status is now AwaitingClientAccountCreation - notify Admin users
                    await SendClientAccountCreationNotificationAsync(deal);
                    _logger.LogInformation("Deal approved by SuperAdmin. DealId: {DealId}, Status: {Status}", dealId, deal.Status);
                }
                else
                {
                    if (string.IsNullOrEmpty(approvalDto.RejectionReason))
                        throw new ArgumentException("Rejection reason is required when rejecting a deal");

                    deal.RejectBySuperAdmin(superAdminId, approvalDto.RejectionReason, approvalDto.Comments);
                    _logger.LogInformation("Deal rejected by SuperAdmin. DealId: {DealId}, Reason: {Reason}", 
                        dealId, approvalDto.RejectionReason);
                }

                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Super admin approval processed for deal. DealId: {DealId}, Approved: {Approved}", 
                    dealId, approvalDto.Approved);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing super admin approval. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetPendingManagerApprovalsAsync()
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetPendingApprovalsForManagerAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending manager approvals");
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetPendingSuperAdminApprovalsAsync()
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetPendingApprovalsForSuperAdminAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending super admin approvals");
                throw;
            }
        }

        #endregion

        #region Deal Completion

        public async Task<DealResponseDTO> MarkDealAsCompletedAsync(long dealId, string completionNotes, string userId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                deal.MarkAsCompleted(completionNotes);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal marked as completed. DealId: {DealId}", dealId);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking deal as completed. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> MarkDealAsFailedAsync(long dealId, string failureNotes, string userId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                deal.MarkAsFailed(failureNotes);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal marked as failed. DealId: {DealId}", dealId);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking deal as failed. DealId: {DealId}", dealId);
                throw;
            }
        }

        #endregion

        #region Business Logic Methods

        public async Task<bool> ValidateDealAsync(CreateDealDTO dealDto)
        {
            // Validate required fields
            if (dealDto.DealValue <= 0)
                return false;

            // Validate offer exists and is accepted
            var offer = await _unitOfWork.SalesOffers.GetByIdAsync(dealDto.OfferId);
            if (offer == null || offer.Status != "Accepted")
                return false;

            // Validate client exists
            var client = await _unitOfWork.Clients.GetByIdAsync(dealDto.ClientId);
            if (client == null)
                return false;

            return true;
        }

        public async Task<bool> CanModifyDealAsync(long dealId, string userId)
        {
            var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
            if (deal == null)
                return false;

            // Only allow modification if deal is pending manager approval
            return deal.Status == "PendingManagerApproval";
        }

        #endregion

        #region Mapping Methods

        private async Task<DealResponseDTO> MapToDealResponseDTO(SalesDeal deal)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
            var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesmanId);
            var managerApprover = !string.IsNullOrEmpty(deal.ManagerApprovedBy) 
                ? await _unitOfWork.Users.GetByIdAsync(deal.ManagerApprovedBy) : null;
            var superAdminApprover = !string.IsNullOrEmpty(deal.SuperAdminApprovedBy) 
                ? await _unitOfWork.Users.GetByIdAsync(deal.SuperAdminApprovedBy) : null;

            // Determine manager approval status
            bool? managerApproved = null;
            if (deal.ManagerApprovedAt.HasValue)
            {
                // If ManagerApprovedAt is set, check if it was approved or rejected
                managerApproved = string.IsNullOrEmpty(deal.ManagerRejectionReason);
            }

            // Determine super admin approval status
            bool? superAdminApproved = null;
            if (deal.SuperAdminApprovedAt.HasValue)
            {
                // If SuperAdminApprovedAt is set, check if it was approved or rejected
                superAdminApproved = string.IsNullOrEmpty(deal.SuperAdminRejectionReason);
            }

            return new DealResponseDTO
            {
                Id = deal.Id,
                OfferId = deal.OfferId,
                ClientId = deal.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                SalesmanId = deal.SalesmanId,
                SalesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown Salesman",
                DealValue = deal.DealValue,
                TotalValue = deal.DealValue, // Alias for mobile compatibility
                ClosedDate = deal.ClosedDate,
                Status = deal.Status,
                ManagerRejectionReason = deal.ManagerRejectionReason,
                ManagerComments = deal.ManagerComments,
                ManagerApproved = managerApproved,
                ManagerApprovedAt = deal.ManagerApprovedAt,
                ManagerApprovedByName = managerApprover != null ? $"{managerApprover.FirstName} {managerApprover.LastName}" : null,
                SuperAdminRejectionReason = deal.SuperAdminRejectionReason,
                SuperAdminComments = deal.SuperAdminComments,
                SuperAdminApproved = superAdminApproved,
                SuperAdminApprovedAt = deal.SuperAdminApprovedAt,
                SuperAdminApprovedByName = superAdminApprover != null ? $"{superAdminApprover.FirstName} {superAdminApprover.LastName}" : null,
                CreatedAt = deal.CreatedAt,
                ExpectedDeliveryDate = deal.ExpectedDeliveryDate,
                CompletionNotes = deal.CompletionNotes,
                FailureNotes = deal.Status == DealStatus.Failed ? deal.CompletionNotes : null,
                ReportText = deal.ReportText,
                ReportAttachments = deal.ReportAttachments,
                ReportSubmittedAt = deal.ReportSubmittedAt,
                SentToLegalAt = deal.SentToLegalAt
            };
        }

        #endregion

        #region Notification Helpers

        private async Task SendManagerApprovalNotificationAsync(SalesDeal deal)
        {
            try
            {
                // Get client info for notification
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                // Get salesman info
                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesmanId);
                var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}".Trim() : "Unknown Salesman";

                // Get all Sales Managers
                var managers = await _userManager.GetUsersInRoleAsync("SalesManager");
                var activeManagers = managers.Where(m => m.IsActive).ToList();

                if (!activeManagers.Any())
                {
                    _logger.LogWarning("No active Sales Managers found to notify for Deal: {DealId}", deal.Id);
                    return;
                }

                var notificationTitle = "Deal Pending Manager Approval";
                var notificationMessage = $"Deal #{deal.Id} from {salesmanName} for {clientName} (Value: {deal.DealValue:C}) requires your approval.";

                var metadata = new Dictionary<string, object>
                {
                    ["dealId"] = deal.Id,
                    ["clientName"] = clientName,
                    ["salesmanName"] = salesmanName,
                    ["dealValue"] = deal.DealValue,
                    ["status"] = deal.Status
                };

                // Send notification to all Sales Managers
                _logger.LogInformation("📢 Sending notifications to {Count} active Sales Managers for Deal: {DealId}", 
                    activeManagers.Count, deal.Id);

                // Also send a broadcast to the role group as backup
                try
                {
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        "SalesManager",
                        notificationTitle,
                        notificationMessage,
                        metadata,
                        CancellationToken.None
                    );
                    _logger.LogInformation("  Broadcast notification sent to Role_SalesManager group for Deal: {DealId}", deal.Id);
                }
                catch (Exception broadcastEx)
                {
                    _logger.LogWarning(broadcastEx, "⚠️ Failed to send broadcast to role group (continuing with individual notifications)");
                }

                foreach (var manager in activeManagers)
                {
                    try
                    {
                        _logger.LogInformation("📤 Creating notification for Sales Manager: {ManagerId} ({ManagerName}) for Deal: {DealId}", 
                            manager.Id, manager.UserName, deal.Id);

                        var notification = await _notificationService.CreateNotificationAsync(
                            manager.Id,
                            notificationTitle,
                            notificationMessage,
                            "Deal",
                            "High",
                            null,
                            null,
                            true, // Mobile push notification
                            metadata,
                            CancellationToken.None
                        );

                        _logger.LogInformation("  Notification created and sent to Sales Manager: {ManagerId} ({ManagerName}) for Deal: {DealId}. NotificationId: {NotificationId}", 
                            manager.Id, manager.UserName, deal.Id, notification.Id);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "❌ Failed to send notification to Sales Manager {ManagerId} ({ManagerName}) for Deal: {DealId}. Error: {Error}", 
                            manager.Id, manager.UserName, deal.Id, notifEx.Message);
                        // Continue with other managers even if one fails
                    }
                }

                _logger.LogInformation("📬 Completed sending notifications to Sales Managers for Deal: {DealId}", deal.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending manager approval notifications for Deal: {DealId}", deal.Id);
                // Don't fail the whole operation if notification fails
            }
        }

        private async Task SendSuperAdminApprovalNotificationAsync(SalesDeal deal)
        {
            try
            {
                // Get client info for notification
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                // Get salesman info
                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesmanId);
                var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}".Trim() : "Unknown Salesman";

                // Get manager who approved
                var manager = deal.ManagerApprovedBy != null 
                    ? await _unitOfWork.Users.GetByIdAsync(deal.ManagerApprovedBy) 
                    : null;
                var managerName = manager != null ? $"{manager.FirstName} {manager.LastName}".Trim() : "Manager";

                // Get all Super Admins
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                var activeSuperAdmins = superAdmins.Where(sa => sa.IsActive).ToList();

                if (!activeSuperAdmins.Any())
                {
                    _logger.LogWarning("No active Super Admins found to notify for Deal: {DealId}", deal.Id);
                    return;
                }

                var notificationTitle = "Deal Pending Super Admin Approval";
                // Check if deal was approved by manager first, or goes directly to SuperAdmin
                var notificationMessage = deal.ManagerApprovedBy != null
                    ? $"Deal #{deal.Id} from {salesmanName} for {clientName} (Value: {deal.DealValue:C}) approved by {managerName} and requires your final approval."
                    : $"Deal #{deal.Id} from {salesmanName} for {clientName} (Value: {deal.DealValue:C}) requires your approval.";

                var metadata = new Dictionary<string, object>
                {
                    ["dealId"] = deal.Id,
                    ["clientName"] = clientName,
                    ["salesmanName"] = salesmanName,
                    ["managerName"] = managerName,
                    ["dealValue"] = deal.DealValue,
                    ["status"] = deal.Status
                };

                // Send notification to all Super Admins
                _logger.LogInformation("📢 Sending notifications to {Count} active Super Admins for Deal: {DealId}", 
                    activeSuperAdmins.Count, deal.Id);

                // Also send a broadcast to the role group as backup
                try
                {
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        "SuperAdmin",
                        notificationTitle,
                        notificationMessage,
                        metadata,
                        CancellationToken.None
                    );
                    _logger.LogInformation("  Broadcast notification sent to Role_SuperAdmin group for Deal: {DealId}", deal.Id);
                }
                catch (Exception broadcastEx)
                {
                    _logger.LogWarning(broadcastEx, "⚠️ Failed to send broadcast to role group (continuing with individual notifications)");
                }

                foreach (var superAdmin in activeSuperAdmins)
                {
                    try
                    {
                        _logger.LogInformation("📤 Creating notification for Super Admin: {SuperAdminId} ({SuperAdminName}) for Deal: {DealId}", 
                            superAdmin.Id, superAdmin.UserName, deal.Id);

                        var notification = await _notificationService.CreateNotificationAsync(
                            superAdmin.Id,
                            notificationTitle,
                            notificationMessage,
                            "Deal",
                            "High",
                            null,
                            null,
                            true, // Mobile push notification
                            metadata,
                            CancellationToken.None
                        );

                        _logger.LogInformation("  Notification created and sent to Super Admin: {SuperAdminId} ({SuperAdminName}) for Deal: {DealId}. NotificationId: {NotificationId}", 
                            superAdmin.Id, superAdmin.UserName, deal.Id, notification.Id);
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "❌ Failed to send notification to Super Admin {SuperAdminId} ({SuperAdminName}) for Deal: {DealId}. Error: {Error}", 
                            superAdmin.Id, superAdmin.UserName, deal.Id, notifEx.Message);
                        // Continue with other super admins even if one fails
                    }
                }

                _logger.LogInformation("📬 Completed sending notifications to Super Admins for Deal: {DealId}", deal.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending super admin approval notifications for Deal: {DealId}", deal.Id);
                // Don't fail the whole operation if notification fails
            }
        }

        public async Task<DealResponseDTO> MarkClientAccountCreatedAsync(long dealId, string adminId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "AwaitingClientAccountCreation")
                    throw new InvalidOperationException($"Deal is not awaiting client account creation. Current status: {deal.Status}");

                deal.MarkClientAccountCreated();
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Notify salesman to submit report
                await SendSalesmanReportNotificationAsync(deal);

                _logger.LogInformation("Client account marked as created for Deal: {DealId} by Admin: {AdminId}", dealId, adminId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking client account as created. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> SubmitSalesmanReportAsync(long dealId, string reportText, string? reportAttachments, string salesmanId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "AwaitingSalesmanReport")
                    throw new InvalidOperationException($"Deal is not awaiting salesman report. Current status: {deal.Status}");

                if (deal.SalesmanId != salesmanId)
                    throw new UnauthorizedAccessException("Only the assigned salesman can submit the report");

                if (string.IsNullOrWhiteSpace(reportText))
                    throw new ArgumentException("Report text is required");

                deal.SubmitReportAndSendToLegal(reportText, reportAttachments);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Notify legal managers
                await SendLegalNotificationAsync(deal);

                _logger.LogInformation("Salesman report submitted for Deal: {DealId} by Salesman: {SalesmanId}", dealId, salesmanId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting salesman report. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsForLegalAsync()
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsByStatusAsync("SentToLegal");
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result.OrderByDescending(d => d.SentToLegalAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals for legal");
                throw;
            }
        }

        private async Task SendClientAccountCreationNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                var activeAdmins = admins.Where(a => a.IsActive).ToList();

                if (!activeAdmins.Any())
                {
                    _logger.LogWarning("No active Admins found to notify for Deal: {DealId}", deal.Id);
                    return;
                }

                var notificationTitle = "Client Account Creation Required";
                var notificationMessage = $"Deal #{deal.Id} for client {clientName} (Value: {deal.DealValue:C}) has been approved. Please create an account for this client.";

                var metadata = new Dictionary<string, object>
                {
                    ["dealId"] = deal.Id,
                    ["clientId"] = deal.ClientId,
                    ["clientName"] = clientName,
                    ["dealValue"] = deal.DealValue,
                    ["status"] = deal.Status
                };

                foreach (var admin in activeAdmins)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            admin.Id,
                            notificationTitle,
                            notificationMessage,
                            "Deal",
                            "High",
                            null,
                            null,
                            true,
                            metadata,
                            CancellationToken.None
                        );
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Failed to send notification to Admin {AdminId} for Deal: {DealId}", admin.Id, deal.Id);
                    }
                }

                // Also send to role group
                try
                {
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        "Admin",
                        notificationTitle,
                        notificationMessage,
                        metadata,
                        CancellationToken.None
                    );
                }
                catch (Exception broadcastEx)
                {
                    _logger.LogWarning(broadcastEx, "Failed to send broadcast to Admin role group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client account creation notification for Deal: {DealId}", deal.Id);
            }
        }

        private async Task SendSalesmanReportNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesmanId);
                if (salesman == null || !salesman.IsActive)
                {
                    _logger.LogWarning("Salesman {SalesmanId} not found or inactive for Deal: {DealId}", deal.SalesmanId, deal.Id);
                    return;
                }

                var notificationTitle = "Report Submission Required";
                var notificationMessage = $"Deal #{deal.Id} for client {clientName} is ready. Please submit your report with the offer.";

                var metadata = new Dictionary<string, object>
                {
                    ["dealId"] = deal.Id,
                    ["clientName"] = clientName,
                    ["dealValue"] = deal.DealValue,
                    ["status"] = deal.Status
                };

                await _notificationService.CreateNotificationAsync(
                    deal.SalesmanId,
                    notificationTitle,
                    notificationMessage,
                    "Deal",
                    "High",
                    null,
                    null,
                    true,
                    metadata,
                    CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending salesman report notification for Deal: {DealId}", deal.Id);
            }
        }

        private async Task SendLegalNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                var legalManagers = await _userManager.GetUsersInRoleAsync("LegalManager");
                var activeLegalManagers = legalManagers.Where(lm => lm.IsActive).ToList();

                if (!activeLegalManagers.Any())
                {
                    _logger.LogWarning("No active Legal Managers found to notify for Deal: {DealId}", deal.Id);
                    return;
                }

                var notificationTitle = "New Deal Received";
                var notificationMessage = $"Deal #{deal.Id} for client {clientName} (Value: {deal.DealValue:C}) with report has been sent to legal department.";

                var metadata = new Dictionary<string, object>
                {
                    ["dealId"] = deal.Id,
                    ["clientName"] = clientName,
                    ["dealValue"] = deal.DealValue,
                    ["status"] = deal.Status
                };

                foreach (var legalManager in activeLegalManagers)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            legalManager.Id,
                            notificationTitle,
                            notificationMessage,
                            "Deal",
                            "High",
                            null,
                            null,
                            true,
                            metadata,
                            CancellationToken.None
                        );
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Failed to send notification to Legal Manager {LegalManagerId} for Deal: {DealId}", legalManager.Id, deal.Id);
                    }
                }

                // Also send to role group
                try
                {
                    await _notificationService.SendNotificationToRoleGroupAsync(
                        "LegalManager",
                        notificationTitle,
                        notificationMessage,
                        metadata,
                        CancellationToken.None
                    );
                }
                catch (Exception broadcastEx)
                {
                    _logger.LogWarning(broadcastEx, "Failed to send broadcast to LegalManager role group");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending legal notification for Deal: {DealId}", deal.Id);
            }
        }

        #endregion
    }
}


