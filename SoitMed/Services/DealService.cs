using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Text.Json;

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
                    SalesManId = salesmanId,
                    // Set second salesman from offer if different
                    SecondSalesManId = offer.AssignedTo != salesmanId ? offer.AssignedTo : null,
                    DealValue = createDealDto.DealValue,
                    ClosedDate = DateTime.UtcNow,
                    PaymentTerms = createDealDto.PaymentTerms,
                    DeliveryTerms = createDealDto.DeliveryTerms,
                    ExpectedDeliveryDate = createDealDto.ExpectedDeliveryDate,
                    Notes = createDealDto.Notes,
                    Status = "PendingManagerApproval"
                };

                await _unitOfWork.SalesDeals.CreateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal created successfully. DealId: {DealId}, OfferId: {OfferId}", deal.Id, createDealDto.OfferId);

                // Send notification to SalesManager for approval
                await SendManagerApprovalNotificationAsync(deal);

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
                // - SalesMan can only view their own deals
                if (userRole != "SuperAdmin" && userRole != "SalesManager" && deal.SalesManId != userId)
                {
                    _logger.LogWarning("Unauthorized access attempt - UserId: {UserId}, UserRole: {UserRole}, DealId: {DealId}, DealSalesManId: {DealSalesManId}",
                        userId, userRole, dealId, deal.SalesManId);
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
                    query = query.Where(d => d.SalesManId == salesmanId);

                // Apply authorization
                if (userRole == "SalesMan")
                {
                    query = query.Where(d => d.SalesManId == userId);
                }
                // SalesManager and SuperAdmin can see all

                var deals = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    try
                    {
                        result.Add(await MapToDealResponseDTO(deal));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping deal {DealId} to DTO. Skipping this deal.", deal?.Id ?? 0);
                        // Continue processing other deals instead of failing the entire request
                    }
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
                    // Check authorization - SuperAdmin and SalesManager can see all deals, SalesMan can only see their own
                    if (userRole == "SuperAdmin" || userRole == "SalesManager" || deal.SalesManId == userId)
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

        public async Task<List<DealResponseDTO>> GetDealsBySalesManAsync(string salesmanId)
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsBySalesManAsync(salesmanId);
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals by salesman. SalesManId: {SalesManId}", salesmanId);
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

                // If approved, send notification to salesmen to add reviews and admin to set credentials
                if (approvalDto.Approved && deal.Status == "AwaitingSalesmenReviewsAndAccountSetup")
                {
                    await SendSalesmenReviewsNotificationAsync(deal);
                    await SendClientAccountSetupNotificationAsync(deal);
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

                _logger.LogInformation("Super Admin approval processed for deal. DealId: {DealId}, Approved: {Approved}", 
                    dealId, approvalDto.Approved);

                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing super Admin approval. DealId: {DealId}", dealId);
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
                    try
                    {
                        result.Add(await MapToDealResponseDTO(deal));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error mapping deal {DealId} to DTO. Skipping this deal.", deal?.Id ?? 0);
                        // Continue processing other deals instead of failing the entire request
                    }
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
                // Get all deals first to debug
                var context = _unitOfWork.GetContext();
                var allDealsCount = await context.SalesDeals.AsNoTracking().CountAsync();
                _logger.LogInformation("Total deals in database: {Count}", allDealsCount);
                
                var deals = await _unitOfWork.SalesDeals.GetPendingApprovalsForSuperAdminAsync();
                
                // Log for debugging
                _logger.LogInformation("GetPendingSuperAdminApprovalsAsync: Found {Count} deals with pending status", deals.Count);
                
                if (deals.Count == 0 && allDealsCount > 0)
                {
                    // Log all deal statuses for debugging
                    var allDeals = await context.SalesDeals
                        .AsNoTracking()
                        .Select(d => new { d.Id, d.Status, d.CreatedAt })
                        .OrderByDescending(d => d.CreatedAt)
                        .Take(50)
                        .ToListAsync();
                    
                    var statusCounts = allDeals
                        .GroupBy(d => d.Status ?? "NULL")
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToList();
                    
                    _logger.LogWarning("No pending deals found. Total deals: {Total}, Status breakdown: {Statuses}",
                        allDealsCount,
                        string.Join(", ", statusCounts.Select(s => $"{s.Status}: {s.Count}")));
                    
                    // Log individual deal statuses for debugging
                    foreach (var deal in allDeals.Take(20))
                    {
                        _logger.LogInformation("Deal - Id: {DealId}, Status: '{Status}', CreatedAt: {CreatedAt}", 
                            deal.Id, deal.Status ?? "NULL", deal.CreatedAt);
                    }
                }
                
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                _logger.LogInformation("Returning {Count} deals to SuperAdmin", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending super Admin approvals");
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
            if (deal == null)
                throw new ArgumentNullException(nameof(deal));

            Client? client = null;
            ApplicationUser? salesman = null;
            ApplicationUser? secondSalesman = null;
            ApplicationUser? managerApprover = null;
            ApplicationUser? superAdminApprover = null;
            ApplicationUser? credentialsSetBy = null;

            try
            {
                if (deal.ClientId > 0)
                    client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load client {ClientId} for deal {DealId}", deal.ClientId, deal.Id);
            }

            try
            {
                if (!string.IsNullOrEmpty(deal.SalesManId))
                    salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesManId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load salesman {SalesManId} for deal {DealId}", deal.SalesManId, deal.Id);
            }

            try
            {
                if (!string.IsNullOrEmpty(deal.SecondSalesManId))
                    secondSalesman = await _unitOfWork.Users.GetByIdAsync(deal.SecondSalesManId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load second salesman {SecondSalesManId} for deal {DealId}", deal.SecondSalesManId, deal.Id);
            }

            try
            {
                if (!string.IsNullOrEmpty(deal.ManagerApprovedBy))
                    managerApprover = await _unitOfWork.Users.GetByIdAsync(deal.ManagerApprovedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load manager approver {ManagerApprovedBy} for deal {DealId}", deal.ManagerApprovedBy, deal.Id);
            }

            try
            {
                if (!string.IsNullOrEmpty(deal.SuperAdminApprovedBy))
                    superAdminApprover = await _unitOfWork.Users.GetByIdAsync(deal.SuperAdminApprovedBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load super admin approver {SuperAdminApprovedBy} for deal {DealId}", deal.SuperAdminApprovedBy, deal.Id);
            }

            try
            {
                if (!string.IsNullOrEmpty(deal.ClientCredentialsSetBy))
                    credentialsSetBy = await _unitOfWork.Users.GetByIdAsync(deal.ClientCredentialsSetBy);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load credentials setter {ClientCredentialsSetBy} for deal {DealId}", deal.ClientCredentialsSetBy, deal.Id);
            }

            // Determine manager approval status
            bool? managerApproved = null;
            if (deal.ManagerApprovedAt.HasValue)
            {
                // If ManagerApprovedAt is set, check if it was approved or rejected
                managerApproved = string.IsNullOrEmpty(deal.ManagerRejectionReason);
            }

            // Determine super Admin approval status
            bool? superAdminApproved = null;
            if (deal.SuperAdminApprovedAt.HasValue)
            {
                // If SuperAdminApprovedAt is set, check if it was approved or rejected
                superAdminApproved = string.IsNullOrEmpty(deal.SuperAdminRejectionReason);
            }

            var dealDto = new DealResponseDTO
            {
                Id = deal.Id,
                OfferId = deal.OfferId,
                ClientId = deal.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                SalesManId = deal.SalesManId,
                SalesManName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown SalesMan",
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
                SentToLegalAt = deal.SentToLegalAt,
                // Salesmen reviews
                SecondSalesManId = deal.SecondSalesManId,
                SecondSalesManName = secondSalesman != null ? $"{secondSalesman.FirstName} {secondSalesman.LastName}" : null,
                FirstSalesManReview = deal.FirstSalesManReview,
                FirstSalesManReviewSubmittedAt = deal.FirstSalesManReviewSubmittedAt,
                SecondSalesManReview = deal.SecondSalesManReview,
                SecondSalesManReviewSubmittedAt = deal.SecondSalesManReviewSubmittedAt,
                // Client credentials
                ClientUsername = deal.ClientUsername,
                ClientCredentialsSetAt = deal.ClientCredentialsSetAt,
                ClientCredentialsSetByName = credentialsSetBy != null ? $"{credentialsSetBy.FirstName} {credentialsSetBy.LastName}" : null
            };

            // Include offer data when deal is sent to legal
            if (deal.Status == DealStatus.SentToLegal && deal.OfferId.HasValue)
            {
                try
                {
                    dealDto.Offer = await MapOfferToEnhancedDTOAsync(deal.OfferId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load offer data for deal {DealId}, offer {OfferId}. Continuing without offer data.", 
                        deal.Id, deal.OfferId.Value);
                    // Continue without offer data rather than failing the whole operation
                }
            }

            return dealDto;
        }

        private async Task<EnhancedOfferResponseDTO?> MapOfferToEnhancedDTOAsync(long offerId)
        {
            try
            {
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                    return null;

                var offerClient = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                var creator = await _unitOfWork.Users.GetByIdAsync(offer.CreatedBy);
                var salesman = await _unitOfWork.Users.GetByIdAsync(offer.AssignedTo);

                // Get OfferRequest information if linked
                string? requesterId = null;
                string? requesterName = null;
                if (offer.OfferRequestId.HasValue)
                {
                    var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(offer.OfferRequestId.Value);
                    if (offerRequest != null)
                    {
                        requesterId = offerRequest.RequestedBy;
                        var requester = await _unitOfWork.Users.GetByIdAsync(offerRequest.RequestedBy);
                        if (requester != null)
                        {
                            requesterName = $"{requester.FirstName} {requester.LastName}".Trim();
                        }
                    }
                }

                // Load equipment
                var equipment = await _unitOfWork.OfferEquipment.GetByOfferIdAsync(offerId);
                var equipmentDtos = equipment.Select(e => new OfferEquipmentDTO
                {
                    Id = e.Id,
                    OfferId = e.OfferId,
                    Name = e.Name,
                    Model = e.Model,
                    Provider = e.Provider,
                    Country = e.Country,
                    Year = e.Year,
                    ImagePath = e.ImagePath,
                    ProviderImagePath = e.ProviderImagePath,
                    Price = e.Price,
                    Description = e.Description,
                    InStock = e.InStock
                }).ToList();

                // Load terms
                var terms = await _unitOfWork.OfferTerms.GetByOfferIdAsync(offerId);
                OfferTermsDTO? termsDto = null;
                if (terms != null)
                {
                    termsDto = new OfferTermsDTO
                    {
                        Id = terms.Id,
                        OfferId = terms.OfferId,
                        WarrantyPeriod = terms.WarrantyPeriod,
                        DeliveryTime = terms.DeliveryTime,
                        MaintenanceTerms = terms.MaintenanceTerms,
                        OtherTerms = terms.OtherTerms
                    };
                }

                // Load installments
                var installments = await _unitOfWork.InstallmentPlans.GetByOfferIdAsync(offerId);
                var installmentDtos = installments.Select(i => new InstallmentPlanDTO
                {
                    Id = i.Id,
                    OfferId = i.OfferId,
                    InstallmentNumber = i.InstallmentNumber,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status,
                    Notes = i.Notes
                }).ToList();

                // Calculate discount amount
                decimal? discountAmount = null;
                if (offer.FinalPrice.HasValue && offer.FinalPrice.Value < offer.TotalAmount)
                {
                    discountAmount = offer.TotalAmount - offer.FinalPrice.Value;
                }

                return new EnhancedOfferResponseDTO
                {
                    Id = offer.Id,
                    OfferRequestId = offer.OfferRequestId,
                    OfferRequestRequesterId = requesterId,
                    OfferRequestRequesterName = requesterName,
                    ClientId = offer.ClientId,
                    ClientName = offerClient?.Name ?? "Unknown Client",
                    CreatedBy = offer.CreatedBy,
                    CreatedByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown Creator",
                    AssignedTo = offer.AssignedTo,
                    AssignedToName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown SalesMan",
                    Products = offer.Products,
                    TotalAmount = offer.TotalAmount,
                    PaymentTerms = DeserializeStringList(offer.PaymentTerms),
                    DeliveryTerms = DeserializeStringList(offer.DeliveryTerms),
                    WarrantyTerms = DeserializeStringList(offer.WarrantyTerms),
                    ValidUntil = DeserializeStringList(offer.ValidUntil),
                    Status = offer.Status,
                    SentToClientAt = offer.SentToClientAt,
                    ClientResponse = offer.ClientResponse,
                    SalesManagerApprovedBy = offer.SalesManagerApprovedBy,
                    SalesManagerApprovedAt = offer.SalesManagerApprovedAt,
                    SalesManagerComments = offer.SalesManagerComments,
                    SalesManagerRejectionReason = offer.SalesManagerRejectionReason,
                    CreatedAt = offer.CreatedAt,
                    UpdatedAt = offer.UpdatedAt,
                    IsSalesManagerApproved = offer.Status == "Sent" && !string.IsNullOrEmpty(offer.SalesManagerApprovedBy),
                    CanSendToSalesMan = offer.Status == "Sent",
                    PdfPath = offer.PdfPath,
                    PdfGeneratedAt = offer.PdfGeneratedAt,
                    PaymentType = offer.PaymentType,
                    FinalPrice = offer.FinalPrice,
                    DiscountAmount = discountAmount,
                    OfferDuration = offer.OfferDuration,
                    Notes = offer.Notes,
                    Equipment = equipmentDtos,
                    Terms = termsDto,
                    Installments = installmentDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping offer to enhanced DTO. OfferId: {OfferId}", offerId);
                throw;
            }
        }

        private List<string>? DeserializeStringList(string? jsonOrString)
        {
            if (string.IsNullOrWhiteSpace(jsonOrString))
                return null;

            // Try to deserialize as JSON array first
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(jsonOrString);
                if (list != null && list.Count > 0)
                    return list;
            }
            catch
            {
                // Not JSON, try as single string (backward compatibility)
            }

            // Fallback: treat as single string value (backward compatibility)
            return new List<string> { jsonOrString };
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
                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesManId);
                var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}".Trim() : "Unknown SalesMan";

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
                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesManId);
                var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}".Trim() : "Unknown SalesMan";

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
                        // Continue with other super Admins even if one fails
                    }
                }

                _logger.LogInformation("📬 Completed sending notifications to Super Admins for Deal: {DealId}", deal.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending super Admin approval notifications for Deal: {DealId}", deal.Id);
                // Don't fail the whole operation if notification fails
            }
        }

        public async Task<DealResponseDTO> MarkClientAccountCreatedAsync(long dealId, string AdminId)
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
                await SendSalesManReportNotificationAsync(deal);

                _logger.LogInformation("Client account marked as created for Deal: {DealId} by Admin: {AdminId}", dealId, AdminId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking client account as created. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> SubmitSalesManReportAsync(long dealId, string reportText, string? reportAttachments, string salesmanId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                // Use case-insensitive comparison to handle any status string variations
                if (!string.Equals(deal.Status, DealStatus.AwaitingSalesManReport, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Deal is not awaiting salesman report. Current status: {deal.Status}");

                if (deal.SalesManId != salesmanId)
                    throw new UnauthorizedAccessException("Only the assigned salesman can submit the report");

                if (string.IsNullOrWhiteSpace(reportText))
                    throw new ArgumentException("Report text is required");

                deal.SubmitReportAndSendToLegal(reportText, reportAttachments);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Notify legal managers
                await SendLegalNotificationAsync(deal);

                _logger.LogInformation("SalesMan report submitted for Deal: {DealId} by SalesMan: {SalesManId}", dealId, salesmanId);
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
                // Get all deals that are in legal workflow (SentToLegal, LegalReviewed)
                var query = _unitOfWork.SalesDeals.GetQueryable()
                    .Where(d => 
                        d.Status == DealStatus.SentToLegal || 
                        d.Status == DealStatus.LegalReviewed
                    );
                
                var legalDeals = await query.ToListAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in legalDeals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result.OrderByDescending(d => d.SentToLegalAt ?? d.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals for legal");
                throw;
            }
        }

        public async Task<DealResponseDTO> MarkDealAsLegalReviewedAsync(long dealId, string legalUserId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != DealStatus.SentToLegal)
                    throw new InvalidOperationException($"Deal must be in SentToLegal status to mark as reviewed. Current status: {deal.Status}");

                deal.MarkAsLegalReviewed();
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal {DealId} marked as reviewed by legal user {LegalUserId}", dealId, legalUserId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking deal as legal reviewed. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<int> GetTotalDealsCountAsync()
        {
            try
            {
                // Use GetQueryable to get count without loading all deals into memory
                var count = await _unitOfWork.SalesDeals.GetQueryable().CountAsync();
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total deals count");
                throw;
            }
        }

        public async Task<DealStatisticsDTO> GetDealStatisticsAsync()
        {
            try
            {
                var allDeals = await _unitOfWork.SalesDeals.GetQueryable().ToListAsync();
                
                var totalDeals = allDeals.Count;
                var totalDealValue = allDeals.Sum(d => d.DealValue);
                var averageDealValue = totalDeals > 0 ? totalDealValue / totalDeals : 0;

                // Get all unique statuses
                var allStatuses = allDeals.Select(d => d.Status).Distinct().ToList();
                
                var dealsByStatus = new Dictionary<string, int>();
                var dealValueByStatus = new Dictionary<string, decimal>();

                foreach (var status in allStatuses)
                {
                    dealsByStatus[status] = await _unitOfWork.SalesDeals.GetDealCountByStatusAsync(status);
                    dealValueByStatus[status] = await _unitOfWork.SalesDeals.GetTotalDealValueByStatusAsync(status);
                }

                return new DealStatisticsDTO
                {
                    TotalDeals = totalDeals,
                    TotalDealValue = totalDealValue,
                    AverageDealValue = averageDealValue,
                    DealsByStatus = dealsByStatus,
                    DealValueByStatus = dealValueByStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deal statistics");
                throw;
            }
        }

        public async Task<DealResponseDTO> SubmitFirstSalesManReviewAsync(long dealId, string reviewText, string salesmanId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "AwaitingSalesmenReviewsAndAccountSetup")
                    throw new InvalidOperationException($"Deal is not awaiting salesmen reviews. Current status: {deal.Status}");

                if (deal.SalesManId != salesmanId)
                    throw new UnauthorizedAccessException("Only the primary assigned salesman can submit the first review");

                if (string.IsNullOrWhiteSpace(reviewText))
                    throw new ArgumentException("Review text is required");

                deal.SubmitFirstSalesManReview(reviewText);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Check if all conditions are met and send to legal
                if (deal.CheckAndSendToLegalIfReady())
                {
                    await _unitOfWork.SalesDeals.UpdateAsync(deal);
                    await _unitOfWork.SaveChangesAsync();
                    await SendLegalNotificationAsync(deal);
                    _logger.LogInformation("Deal automatically sent to legal after all reviews and credentials are ready. DealId: {DealId}", dealId);
                }

                _logger.LogInformation("First salesman review submitted for Deal: {DealId} by SalesMan: {SalesManId}", dealId, salesmanId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting first salesman review. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> SubmitSecondSalesManReviewAsync(long dealId, string reviewText, string salesmanId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "AwaitingSalesmenReviewsAndAccountSetup")
                    throw new InvalidOperationException($"Deal is not awaiting salesmen reviews. Current status: {deal.Status}");

                if (string.IsNullOrWhiteSpace(deal.SecondSalesManId))
                    throw new InvalidOperationException("This deal does not have a second salesman assigned");

                if (deal.SecondSalesManId != salesmanId)
                    throw new UnauthorizedAccessException("Only the second assigned salesman can submit the second review");

                if (string.IsNullOrWhiteSpace(reviewText))
                    throw new ArgumentException("Review text is required");

                deal.SubmitSecondSalesManReview(reviewText);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Check if all conditions are met and send to legal
                if (deal.CheckAndSendToLegalIfReady())
                {
                    await _unitOfWork.SalesDeals.UpdateAsync(deal);
                    await _unitOfWork.SaveChangesAsync();
                    await SendLegalNotificationAsync(deal);
                    _logger.LogInformation("Deal automatically sent to legal after all reviews and credentials are ready. DealId: {DealId}", dealId);
                }

                _logger.LogInformation("Second salesman review submitted for Deal: {DealId} by SalesMan: {SalesManId}", dealId, salesmanId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting second salesman review. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<DealResponseDTO> SetClientCredentialsAsync(long dealId, string username, string password, string adminId)
        {
            try
            {
                var deal = await _unitOfWork.SalesDeals.GetByIdAsync(dealId);
                if (deal == null)
                    throw new ArgumentException("Deal not found", nameof(dealId));

                if (deal.Status != "AwaitingSalesmenReviewsAndAccountSetup")
                    throw new InvalidOperationException($"Deal is not awaiting account setup. Current status: {deal.Status}");

                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Username is required");

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password is required");

                // Encrypt password (using a simple approach - in production, use proper encryption)
                // For now, we'll store it as-is, but in production you should use proper encryption
                var encryptedPassword = password; // TODO: Implement proper password encryption

                deal.SetClientCredentials(username, encryptedPassword, adminId);
                await _unitOfWork.SalesDeals.UpdateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                // Check if all conditions are met and send to legal
                if (deal.CheckAndSendToLegalIfReady())
                {
                    await _unitOfWork.SalesDeals.UpdateAsync(deal);
                    await _unitOfWork.SaveChangesAsync();
                    await SendLegalNotificationAsync(deal);
                    _logger.LogInformation("Deal automatically sent to legal after all reviews and credentials are ready. DealId: {DealId}", dealId);
                }

                _logger.LogInformation("Client credentials set for Deal: {DealId} by Admin: {AdminId}", dealId, adminId);
                return await MapToDealResponseDTO(deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting client credentials. DealId: {DealId}", dealId);
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsAwaitingReviewsAndAccountSetupAsync()
        {
            try
            {
                var deals = await _unitOfWork.SalesDeals.GetDealsByStatusAsync("AwaitingSalesmenReviewsAndAccountSetup");
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result.OrderByDescending(d => d.ManagerApprovedAt).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals awaiting reviews and account setup");
                throw;
            }
        }

        public async Task<List<DealResponseDTO>> GetDealsAwaitingReportAsync(string userId, string userRole)
        {
            try
            {
                var query = _unitOfWork.SalesDeals.GetQueryable()
                    .Where(d => d.Status == DealStatus.AwaitingSalesManReport);

                // Apply authorization: SalesMan can only see their own deals
                if (userRole == "SalesMan")
                {
                    query = query.Where(d => d.SalesManId == userId);
                }
                // SalesManager and SuperAdmin can see all deals awaiting report

                var deals = await query.OrderByDescending(d => d.SuperAdminApprovedAt ?? d.ManagerApprovedAt ?? d.CreatedAt).ToListAsync();
                var result = new List<DealResponseDTO>();

                foreach (var deal in deals)
                {
                    result.Add(await MapToDealResponseDTO(deal));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting deals awaiting report");
                throw;
            }
        }

        private async Task SendClientAccountCreationNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                var Admins = await _userManager.GetUsersInRoleAsync("Admin");
                var activeAdmins = Admins.Where(a => a.IsActive).ToList();

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

                foreach (var Admin in activeAdmins)
                {
                    try
                    {
                        await _notificationService.CreateNotificationAsync(
                            Admin.Id,
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
                        _logger.LogError(notifEx, "Failed to send notification to Admin {AdminId} for Deal: {DealId}", Admin.Id, deal.Id);
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

        private async Task SendSalesManReportNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                var salesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesManId);
                if (salesman == null || !salesman.IsActive)
                {
                    _logger.LogWarning("SalesMan {SalesManId} not found or inactive for Deal: {DealId}", deal.SalesManId, deal.Id);
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
                    deal.SalesManId,
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

        private async Task SendSalesmenReviewsNotificationAsync(SalesDeal deal)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(deal.ClientId);
                var clientName = client?.Name ?? "Unknown Client";

                // Notify first salesman
                var firstSalesman = await _unitOfWork.Users.GetByIdAsync(deal.SalesManId);
                if (firstSalesman != null && firstSalesman.IsActive)
                {
                    var notificationTitle = "Review Submission Required";
                    var notificationMessage = $"Deal #{deal.Id} for client {clientName} has been approved by manager. Please submit your review for the legal team.";

                    var metadata = new Dictionary<string, object>
                    {
                        ["dealId"] = deal.Id,
                        ["clientName"] = clientName,
                        ["dealValue"] = deal.DealValue,
                        ["status"] = deal.Status
                    };

                    await _notificationService.CreateNotificationAsync(
                        deal.SalesManId,
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

                // Notify second salesman if exists
                if (!string.IsNullOrWhiteSpace(deal.SecondSalesManId))
                {
                    var secondSalesman = await _unitOfWork.Users.GetByIdAsync(deal.SecondSalesManId);
                    if (secondSalesman != null && secondSalesman.IsActive)
                    {
                        var notificationTitle = "Review Submission Required";
                        var notificationMessage = $"Deal #{deal.Id} for client {clientName} has been approved by manager. Please submit your review for the legal team.";

                        var metadata = new Dictionary<string, object>
                        {
                            ["dealId"] = deal.Id,
                            ["clientName"] = clientName,
                            ["dealValue"] = deal.DealValue,
                            ["status"] = deal.Status
                        };

                        await _notificationService.CreateNotificationAsync(
                            deal.SecondSalesManId,
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending salesmen reviews notification for Deal: {DealId}", deal.Id);
            }
        }

        private async Task SendClientAccountSetupNotificationAsync(SalesDeal deal)
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

                var notificationTitle = "Client Account Setup Required";
                var notificationMessage = $"Deal #{deal.Id} for client {clientName} (Value: {deal.DealValue:C}) has been approved. Please set the username and password for this client.";

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
                _logger.LogError(ex, "Error sending client account setup notification for Deal: {DealId}", deal.Id);
            }
        }

        #endregion
    }
}


