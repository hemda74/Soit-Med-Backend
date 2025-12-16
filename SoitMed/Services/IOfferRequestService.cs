using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for offer request service
    /// </summary>
    public interface IOfferRequestService
    {
        #region Offer Request Management

        Task<OfferRequestResponseDTO> CreateOfferRequestAsync(CreateOfferRequestDTO createDto, string userId);
        Task<OfferRequestResponseDTO?> GetOfferRequestAsync(long requestId, string userId, string userRole);
        Task<List<OfferRequestResponseDTO>> GetOfferRequestsAsync(string? status, string? requestedBy, string userId, string userRole);
        Task<List<OfferRequestResponseDTO>> GetOfferRequestsBySalesManAsync(string salesmanId, string? status);
        Task<List<OfferRequestResponseDTO>> GetOfferRequestsAssignedToAsync(string supportId, string? status);
        Task<OfferRequestResponseDTO> AssignToSupportAsync(long requestId, string supportId, string userId);
        Task<OfferRequestResponseDTO> UpdateStatusAsync(long requestId, string status, string? notes, string userId);
        Task<bool> DeleteOfferRequestAsync(long requestId, string userId);

        #endregion

        #region Business Logic Methods

        Task<bool> ValidateOfferRequestAsync(CreateOfferRequestDTO requestDto);
        Task<bool> CanModifyOfferRequestAsync(long requestId, string userId);
        Task<List<OfferRequestResponseDTO>> GetPendingRequestsAsync();

        #endregion
    }
}

