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
                NextContactDate = client.NextContactDate,
                Priority = client.Priority,
                Status = client.Status
            };
        }

        public ClientStatisticsDTO MapToClientStatisticsDTO(Client client, int totalClientsCount)
        {
            return new ClientStatisticsDTO
            {
                MyClientsCount = 1, // This would be calculated differently in practice
                TotalClientsCount = totalClientsCount,
                ClientsByType = new List<ClientTypeCount> { new() { Type = client.Type, Count = 1 } },
                ClientsByStatus = new List<ClientStatusCount> { new() { Status = client.Status, Count = 1 } },
                ClientsByPriority = new List<ClientPriorityCount> { new() { Priority = client.Priority, Count = 1 } }
            };
        }
    }
}

