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
                Type = client.Type,
                Specialization = client.Specialization,
                Location = client.Location,
                Phone = client.Phone,
                Email = client.Email,
                Website = client.Website,
                Address = client.Address,
                City = client.City,
                Governorate = client.Governorate,
                PostalCode = client.PostalCode,
                Notes = client.Notes,
                Status = client.Status,
                Priority = client.Priority,
                Classification = client.Classification,
                Rating = client.SatisfactionRating,
                PotentialValue = client.PotentialValue,
                ContactPerson = client.ContactPerson,
                ContactPersonPhone = client.ContactPersonPhone,
                ContactPersonEmail = client.ContactPersonEmail,
                ContactPersonPosition = client.ContactPersonPosition,
                LastContactDate = client.LastContactDate,
                NextContactDate = client.NextContactDate,
                SatisfactionRating = client.SatisfactionRating,
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
                Type = createDto.Type,
                Specialization = createDto.Specialization,
                Location = createDto.Location,
                Phone = createDto.Phone,
                Email = createDto.Email,
                Website = createDto.Website,
                Address = createDto.Address,
                City = createDto.City,
                Governorate = createDto.Governorate,
                PostalCode = createDto.PostalCode,
                Notes = createDto.Notes,
                Status = createDto.Status,
                Priority = createDto.Priority,
                PotentialValue = createDto.PotentialValue,
                ContactPerson = createDto.ContactPerson,
                ContactPersonPhone = createDto.ContactPersonPhone,
                ContactPersonEmail = createDto.ContactPersonEmail,
                ContactPersonPosition = createDto.ContactPersonPosition,
                CreatedBy = userId,
                AssignedTo = createDto.AssignedTo
            };
        }

        public ClientFollowUpDTO MapToClientFollowUpDTO(Client client)
        {
            return new ClientFollowUpDTO
            {
                Id = client.Id,
                Name = client.Name,
                Type = client.Type,
                LastContactDate = client.LastContactDate,
                NextContactDate = client.NextContactDate,
                Priority = client.Priority,
                Status = client.Status,
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

