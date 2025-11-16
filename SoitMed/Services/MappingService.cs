using SoitMed.DTO;
using SoitMed.Models;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for mapping between entities and DTOs
    /// </summary>
    public class MappingService : IMappingService
    {
        public ClientResponseDTO MapToClientResponseDTO(Client client)
        {
            return new ClientResponseDTO
            {
                Id = client.Id,
                Name = client.Name,
                Phone = client.Phone,
                OrganizationName = client.OrganizationName,
                Classification = client.Classification,
                CreatedBy = client.CreatedBy,
                AssignedTo = client.AssignedTo,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
        }

        public Client MapToClient(CreateClientDTO createDto, string userId)
        {
            return new Client
            {
                Name = createDto.Name,
                Phone = createDto.Phone,
                OrganizationName = createDto.OrganizationName,
                Classification = createDto.Classification,
                CreatedBy = userId,
                AssignedTo = createDto.AssignedTo ?? userId
            };
        }

        public ClientFollowUpDTO MapToClientFollowUpDTO(Client client)
        {
            return new ClientFollowUpDTO
            {
                Id = client.Id,
                Name = client.Name,
                OrganizationName = client.OrganizationName,
                AssignedTo = client.AssignedTo
            };
        }

        public ClientStatisticsDTO MapToClientStatisticsDTO(Client client, int totalClientsCount)
        {
            return new ClientStatisticsDTO
            {
                TotalVisits = 0, // This would be calculated from task progresses
                TotalOffers = 0, // This would be calculated from offers
                SuccessfulDeals = 0, // This would be calculated from deals
                FailedDeals = 0, // This would be calculated from deals
                TotalRevenue = 0, // This would be calculated from successful deals
                AverageSatisfaction = null // This would be calculated from task progresses
            };
        }
    }
}
