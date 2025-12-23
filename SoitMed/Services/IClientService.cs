using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for client management service
    /// </summary>
    public interface IClientService
    {
        #region Client Management
        Task<ClientResponseDTO> CreateClientAsync(CreateClientDTO createClientDto, string userId);
        Task<ClientResponseDTO?> GetClientAsync(long clientId);
        Task<ClientProfileDTO?> GetClientProfileAsync(long clientId, string userId);
        Task<List<ClientResponseDTO>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<ClientResponseDTO>> GetClientsNeedingFollowUpAsync(string userId);
        Task<ClientResponseDTO> UpdateClientAsync(long clientId, CreateClientDTO updateClientDto, string userId);
        Task<bool> DeleteClientAsync(long clientId, string userId);
        #endregion

        #region Search and Filter
        Task<PaginatedClientsResponseDTO> SearchClientsAsync(SearchClientDTO searchDto, string userId);
        Task<List<ClientResponseDTO>> GetClientsByClassificationAsync(string classification, string userId);
        Task<ClientResponseDTO> FindOrCreateClientAsync(FindOrCreateClientDTO findDto, string userId);
        Task<ClientStatisticsDTO> GetClientStatisticsAsync(string userId);
        #endregion

        #region Legal Documents
        Task<ClientResponseDTO> SubmitClientLegalDocumentsAsync(long clientId, string nationalId, string nationalIdFrontImagePath, string nationalIdBackImagePath, string? taxCardNumber, string? taxCardImagePath, string userId);
        #endregion
    }
}

