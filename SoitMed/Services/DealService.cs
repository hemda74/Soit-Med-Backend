using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing deals with multi-level approval workflow
    /// </summary>
    public class DealService : IDealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DealService> _logger;

        public DealService(IUnitOfWork unitOfWork, ILogger<DealService> logger)
        {
            _unitOfWork = unitOfWork;
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
                    Status = "PendingManagerApproval"
                };

                await _unitOfWork.SalesDeals.CreateAsync(deal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deal created successfully. DealId: {DealId}, OfferId: {OfferId}", deal.Id, createDealDto.OfferId);

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

                // Check authorization
                if (userRole != "SuperAdmin" && deal.SalesmanId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view this deal");

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
                    // Check authorization
                    if (userRole == "SuperAdmin" || deal.SalesmanId == userId)
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

                if (deal.Status != "PendingSuperAdminApproval")
                    throw new InvalidOperationException("Deal is not pending super admin approval");

                if (approvalDto.Approved)
                {
                    deal.ApproveBySuperAdmin(superAdminId, approvalDto.Comments);
                    // Automatically send to legal after super admin approval
                    deal.SendToLegal();
                }
                else
                {
                    if (string.IsNullOrEmpty(approvalDto.RejectionReason))
                        throw new ArgumentException("Rejection reason is required when rejecting a deal");

                    deal.RejectBySuperAdmin(superAdminId, approvalDto.RejectionReason, approvalDto.Comments);
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

            return new DealResponseDTO
            {
                Id = deal.Id,
                OfferId = deal.OfferId,
                ClientId = deal.ClientId,
                ClientName = client?.Name ?? "Unknown Client",
                SalesmanId = deal.SalesmanId,
                SalesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Unknown Salesman",
                DealValue = deal.DealValue,
                ClosedDate = deal.ClosedDate,
                Status = deal.Status,
                ManagerRejectionReason = deal.ManagerRejectionReason,
                ManagerComments = deal.ManagerComments,
                SuperAdminRejectionReason = deal.SuperAdminRejectionReason,
                SuperAdminComments = deal.SuperAdminComments,
                CreatedAt = deal.CreatedAt
            };
        }

        #endregion
    }
}


