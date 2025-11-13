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
        Task<OfferResponseDTO?> GetOfferAsync(long offerId);
        Task<List<OfferResponseDTO>> GetOffersByClientAsync(long clientId);
        Task<List<OfferResponseDTO>> GetOffersBySalesmanAsync(string salesmanId);
        Task<List<OfferResponseDTO>> GetOffersByStatusAsync(string status);
        Task<List<OfferResponseDTO>> GetOffersByCreatorAsync(string creatorId);
        Task<PaginatedOffersResponseDTO> GetAllOffersWithFiltersAsync(string? status, string? salesmanId, int page, int pageSize, DateTime? startDate, DateTime? endDate);
        Task<OfferResponseDTO> UpdateOfferAsync(long offerId, CreateOfferDTO updateOfferDto, string userId);
        Task<OfferResponseDTO> SendToSalesmanAsync(long offerId, string userId);
        Task<OfferResponseDTO> RecordClientResponseAsync(long offerId, string response, bool accepted, string userId);
        Task<OfferResponseDTO> CompleteOfferAsync(long offerId, string? completionNotes, string userId);
        Task<OfferResponseDTO> MarkAsNeedsModificationAsync(long offerId, string? reason, string userId);
        Task<OfferResponseDTO> MarkAsUnderReviewAsync(long offerId, string userId);
        Task<int> UpdateExpiredOffersAsync();
        Task<List<OfferActivityDTO>> GetRecentActivityAsync(int limit = 20);
        Task<bool> DeleteOfferAsync(long offerId, string userId);
        Task<OfferResponseDTO> AssignOfferToSalesmanAsync(long offerId, string salesmanId, string userId);
        #endregion

        #region Enhanced Offer Features - Equipment Management
        Task<OfferEquipmentDTO> AddEquipmentAsync(long offerId, CreateOfferEquipmentDTO dto);
        Task<List<OfferEquipmentDTO>> GetEquipmentListAsync(long offerId);
        Task<bool> DeleteEquipmentAsync(long offerId, long equipmentId);
        Task<OfferEquipmentDTO> UpdateEquipmentImagePathAsync(long offerId, long equipmentId, string imagePath);
        Task<string?> GetEquipmentImagePathAsync(long offerId, long equipmentId);
        #endregion

        #region Enhanced Offer Features - Terms Management
        Task<OfferTermsDTO> AddOrUpdateTermsAsync(long offerId, CreateOfferTermsDTO dto);
        #endregion

        #region Enhanced Offer Features - Installment Plans
        Task<List<InstallmentPlanDTO>> CreateInstallmentPlanAsync(long offerId, CreateInstallmentPlanDTO dto);
        #endregion

        Task<object?> GetOfferRequestDetailsAsync(long requestId);

        #region Enhanced Offer - Complete Details
        Task<EnhancedOfferResponseDTO> GetEnhancedOfferAsync(long offerId);
        #endregion

        #region Business Logic Methods
        Task<bool> ValidateOfferAsync(CreateOfferDTO offerDto);
        Task<List<OfferResponseDTO>> GetExpiredOffersAsync();
        #endregion
    }
}
