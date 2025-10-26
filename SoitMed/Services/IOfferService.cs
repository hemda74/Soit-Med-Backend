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
        Task<OfferResponseDTO?> GetOfferAsync(long offerId);
        Task<List<OfferResponseDTO>> GetOffersByClientAsync(long clientId);
        Task<List<OfferResponseDTO>> GetOffersBySalesmanAsync(string salesmanId);
        Task<List<OfferResponseDTO>> GetOffersByStatusAsync(string status);
        Task<List<OfferResponseDTO>> GetOffersByCreatorAsync(string creatorId);
        Task<OfferResponseDTO> UpdateOfferAsync(long offerId, CreateOfferDTO updateOfferDto, string userId);
        Task<OfferResponseDTO> SendToSalesmanAsync(long offerId, string userId);
        Task<OfferResponseDTO> RecordClientResponseAsync(long offerId, string response, bool accepted, string userId);
        Task<bool> DeleteOfferAsync(long offerId, string userId);
        #endregion

        #region Business Logic Methods
        Task<bool> ValidateOfferAsync(CreateOfferDTO offerDto);
        Task<List<OfferResponseDTO>> GetExpiredOffersAsync();
        #endregion
    }
}
