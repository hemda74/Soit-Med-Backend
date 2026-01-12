using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for offer management service
    /// </summary>
    public interface IOfferService
    {
        #region Offer Management
        Task<OfferResponseDTO> CreateOfferFromRequestAsync(CreateOfferDTO createOfferDto, string userId);
        Task<OfferResponseDTO> CreateOfferAsync(CreateOfferDTO createOfferDto, string userId);
        Task<OfferResponseDTO> CreateOfferWithItemsAsync(CreateOfferWithItemsDTO createOfferDto, string userId);
        Task<OfferResponseDTO?> GetOfferAsync(string offerId);
        Task<List<OfferResponseDTO>> GetOffersByClientAsync(long clientId);
        Task<List<OfferResponseDTO>> GetOffersBySalesManAsync(string salesmanId);
        Task<List<OfferResponseDTO>> GetOffersByStatusAsync(string status);
        Task<List<OfferResponseDTO>> GetOffersByCreatorAsync(string creatorId);
        Task<PaginatedOffersResponseDTO> GetAllOffersWithFiltersAsync(string? status, string? salesmanId, int page, int pageSize, DateTime? startDate, DateTime? endDate);
        Task<OfferResponseDTO> UpdateOfferAsync(string offerId, CreateOfferDTO updateOfferDto, string userId);
        Task<OfferResponseDTO> UpdateOfferBySalesManagerAsync(string offerId, UpdateOfferDTO updateDto, string userId);
        Task<OfferResponseDTO> SendToSalesManAsync(string offerId, string userId);
        Task<OfferResponseDTO> RecordClientResponseAsync(string offerId, string response, bool accepted, string userId);
        Task<OfferResponseDTO> CompleteOfferAsync(string offerId, string? completionNotes, string userId);
        Task<OfferResponseDTO> MarkAsNeedsModificationAsync(string offerId, string? reason, string userId);
        Task<OfferResponseDTO> MarkAsUnderReviewAsync(string offerId, string userId);
        Task<OfferResponseDTO> ResumeFromUnderReviewAsync(string offerId, string userId);
        Task<int> UpdateExpiredOffersAsync();
        Task<List<OfferActivityDTO>> GetRecentActivityAsync(int limit = 20);
        Task<bool> DeleteOfferAsync(string offerId, string userId);
        Task<OfferResponseDTO> AssignOfferToSalesManAsync(string offerId, string salesmanId, string userId);
        Task<OfferResponseDTO> SalesManagerApprovalAsync(string offerId, ApproveOfferDTO approvalDto, string salesManagerId);
        Task<List<OfferResponseDTO>> GetPendingSalesManagerApprovalsAsync();
        #endregion

        #region Enhanced Offer Features - Equipment Management
        Task<OfferEquipmentDTO> AddEquipmentAsync(string offerId, CreateOfferEquipmentDTO dto);
        Task<List<OfferEquipmentDTO>> GetEquipmentListAsync(string offerId);
        Task<bool> DeleteEquipmentAsync(string offerId, long equipmentId);
        Task<OfferEquipmentDTO> UpdateEquipmentImagePathAsync(string offerId, long equipmentId, string imagePath);
        Task<string?> GetEquipmentImagePathAsync(string offerId, long equipmentId);
        #endregion

        #region Enhanced Offer Features - Terms Management
        Task<OfferTermsDTO> AddOrUpdateTermsAsync(string offerId, CreateOfferTermsDTO dto);
        #endregion

        #region Enhanced Offer Features - Installment Plans
        Task<List<InstallmentPlanDTO>> CreateInstallmentPlanAsync(string offerId, CreateInstallmentPlanDTO dto);
        #endregion

        Task<object?> GetOfferRequestDetailsAsync(long requestId);

        #region Enhanced Offer - Complete Details
        Task<EnhancedOfferResponseDTO> GetEnhancedOfferAsync(string offerId);
        #endregion

        #region Business Logic Methods
        Task<bool> ValidateOfferAsync(CreateOfferDTO offerDto);
        Task<List<OfferResponseDTO>> GetExpiredOffersAsync();
        #endregion
    }
}
