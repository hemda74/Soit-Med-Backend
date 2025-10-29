using SoitMed.DTO;
using SoitMed.Models;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for mapping between entities and DTOs
    /// </summary>
    public interface IMappingService
    {
        /// <summary>
        /// Maps a Client entity to ClientResponseDTO
        /// </summary>
        ClientResponseDTO MapToClientResponseDTO(Client client);

        /// <summary>
        /// Maps a CreateClientDTO to Client entity
        /// </summary>
        Client MapToClient(CreateClientDTO createDto, string userId);

        /// <summary>
        /// Maps a Client entity to ClientFollowUpDTO
        /// </summary>
        ClientFollowUpDTO MapToClientFollowUpDTO(Client client);

        /// <summary>
        /// Maps a Client entity to ClientStatisticsDTO
        /// </summary>
        ClientStatisticsDTO MapToClientStatisticsDTO(Client client, int totalClientsCount);
    }
}

