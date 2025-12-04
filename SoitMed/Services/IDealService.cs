using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for deal management service
    /// </summary>
    public interface IDealService
    {
        #region Deal Management
        Task<DealResponseDTO> CreateDealAsync(CreateDealDTO createDealDto, string salesmanId);
        Task<DealResponseDTO?> GetDealAsync(long dealId);
        Task<DealResponseDTO?> GetDealAsync(long dealId, string userId, string userRole);
        Task<List<DealResponseDTO>> GetDealsAsync(string? status, string? salesmanId, string userId, string userRole);
        Task<List<DealResponseDTO>> GetDealsByClientAsync(long clientId);
        Task<List<DealResponseDTO>> GetDealsByClientAsync(long clientId, string userId, string userRole);
        Task<List<DealResponseDTO>> GetDealsBySalesmanAsync(string salesmanId);
        Task<List<DealResponseDTO>> GetDealsByStatusAsync(string status);
        Task<DealResponseDTO> UpdateDealAsync(long dealId, CreateDealDTO updateDealDto, string userId);
        #endregion

        #region Approval Workflow
        Task<DealResponseDTO> ManagerApprovalAsync(long dealId, ApproveDealDTO approvalDto, string managerId);
        Task<DealResponseDTO> SuperAdminApprovalAsync(long dealId, ApproveDealDTO approvalDto, string superAdminId);
        Task<List<DealResponseDTO>> GetPendingManagerApprovalsAsync();
        Task<List<DealResponseDTO>> GetPendingSuperAdminApprovalsAsync();
        Task<DealResponseDTO> MarkClientAccountCreatedAsync(long dealId, string adminId);
        Task<DealResponseDTO> SubmitSalesmanReportAsync(long dealId, string reportText, string? reportAttachments, string salesmanId);
        Task<List<DealResponseDTO>> GetDealsForLegalAsync();
        #endregion

        #region Deal Completion
        Task<DealResponseDTO> MarkDealAsCompletedAsync(long dealId, string completionNotes, string userId);
        Task<DealResponseDTO> MarkDealAsFailedAsync(long dealId, string failureNotes, string userId);
        #endregion

        #region Business Logic Methods
        Task<bool> ValidateDealAsync(CreateDealDTO dealDto);
        Task<bool> CanModifyDealAsync(long dealId, string userId);
        #endregion
    }
}


