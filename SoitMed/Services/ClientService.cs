using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing clients with complete history tracking
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientService> _logger;

        public ClientService(IUnitOfWork unitOfWork, ILogger<ClientService> logger) 
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Client Management

        public async Task<ClientResponseDTO> CreateClientAsync(CreateClientDTO createClientDto, string userId)
        {
            try
            {
                var client = new Client
                {
                    Name = createClientDto.Name,
                    Type = createClientDto.Type, // Can be null - optional field
                    Specialization = createClientDto.Specialization,
                    Location = createClientDto.Location,
                    Phone = createClientDto.Phone,
                    Email = createClientDto.Email,
                    Address = createClientDto.Address,
                    City = createClientDto.City,
                    Governorate = createClientDto.Governorate,
                    ContactPerson = createClientDto.ContactPerson,
                    ContactPersonPhone = createClientDto.ContactPersonPhone,
                    ContactPersonEmail = createClientDto.ContactPersonEmail,
                    ContactPersonPosition = createClientDto.ContactPersonPosition,
                    Notes = createClientDto.Notes,
                    Priority = createClientDto.Priority,
                    Classification = createClientDto.Classification,
                    Status = "Active",
                    CreatedBy = userId,
                    AssignedTo = userId
                };

                await _unitOfWork.Clients.CreateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client created successfully. ClientId: {ClientId}, Name: {Name}", client.Id, client.Name);

                return MapToClientResponseDTO(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client. Name: {Name}", createClientDto.Name);
                throw;
            }
        }

        public async Task<ClientResponseDTO?> GetClientAsync(long clientId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    return null;

                return MapToClientResponseDTO(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<ClientProfileDTO?> GetClientProfileAsync(long clientId, string userId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    return null;

                // Authorization is already checked at controller level
                // Here we just ensure the client exists and return the profile
                // The controller's [Authorize(Roles="Salesman,SalesManager,SuperAdmin")] handles role check
                // For data filtering, check if client is assigned to user when needed

                // Use parallel queries for better performance
                var taskProgressesTask = _unitOfWork.TaskProgresses
                    .GetProgressesByClientIdAsync(clientId);
                var offersTask = _unitOfWork.SalesOffers
                    .GetOffersByClientIdAsync(clientId);
                var dealsTask = _unitOfWork.SalesDeals
                    .GetDealsByClientIdAsync(clientId);

                // Wait for all queries to complete in parallel
                await Task.WhenAll(taskProgressesTask, offersTask, dealsTask);

                var taskProgresses = await taskProgressesTask;
                var offers = await offersTask;
                var deals = await dealsTask;

                // Calculate statistics
                var statistics = new ClientStatisticsDTO
                {
                    TotalVisits = taskProgresses.Count,
                    TotalOffers = offers.Count,
                    SuccessfulDeals = deals.Count(d => d.Status == "Success"),
                    FailedDeals = deals.Count(d => d.Status == "Failed"),
                    TotalRevenue = deals.Where(d => d.Status == "Success").Sum(d => d.DealValue),
                    AverageSatisfaction = taskProgresses.Where(tp => tp.SatisfactionRating.HasValue)
                        .Any() ? taskProgresses.Where(tp => tp.SatisfactionRating.HasValue)
                        .Average(tp => tp.SatisfactionRating.Value) : null
                };

                return new ClientProfileDTO
                {
                    ClientInfo = MapToClientResponseDTO(client),
                    AllVisits = taskProgresses.Select(MapToTaskProgressSummaryDTO).ToList(),
                    AllOffers = offers.Select(MapToOfferSummaryDTO).ToList(),
                    AllDeals = deals.Select(MapToDealSummaryDTO).ToList(),
                    Statistics = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client profile. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<List<ClientResponseDTO>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var clients = await _unitOfWork.Clients.GetMyClientsAsync(userId, page, pageSize);
                return clients.Select(MapToClientResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my clients. UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<ClientResponseDTO>> GetClientsNeedingFollowUpAsync(string userId)
        {
            try
            {
                var clients = await _unitOfWork.Clients.GetClientsNeedingFollowUpAsync(userId);
                return clients.Select(MapToClientResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients needing follow-up. UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<ClientResponseDTO> UpdateClientAsync(long clientId, CreateClientDTO updateClientDto, string userId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    throw new ArgumentException("Client not found", nameof(clientId));

                // Update client properties
                client.Name = updateClientDto.Name;
                client.Type = updateClientDto.Type;
                client.Specialization = updateClientDto.Specialization;
                client.Location = updateClientDto.Location;
                client.Phone = updateClientDto.Phone;
                client.Email = updateClientDto.Email;
                client.Address = updateClientDto.Address;
                client.City = updateClientDto.City;
                client.Governorate = updateClientDto.Governorate;
                client.ContactPerson = updateClientDto.ContactPerson;
                client.ContactPersonPhone = updateClientDto.ContactPersonPhone;
                client.ContactPersonEmail = updateClientDto.ContactPersonEmail;
                client.ContactPersonPosition = updateClientDto.ContactPersonPosition;
                client.Notes = updateClientDto.Notes;
                client.Priority = updateClientDto.Priority;
                client.Classification = updateClientDto.Classification;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client updated successfully. ClientId: {ClientId}", clientId);

                return MapToClientResponseDTO(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        public async Task<bool> DeleteClientAsync(long clientId, string userId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    return false;

                await _unitOfWork.Clients.DeleteAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client deleted successfully. ClientId: {ClientId}", clientId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client. ClientId: {ClientId}", clientId);
                throw;
            }
        }

        #endregion

        #region Search and Filter

        public async Task<List<ClientResponseDTO>> SearchClientsAsync(SearchClientDTO searchDto, string userId)
        {
            try
            {
                // Repository now handles empty search terms and returns all clients
                var clients = await _unitOfWork.Clients.SearchClientsAsync(searchDto.Query ?? string.Empty, searchDto.Page, searchDto.PageSize);
                return clients.Select(MapToClientResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clients. Query: {Query}, UserId: {UserId}", searchDto.Query, userId);
                throw;
            }
        }

        public async Task<List<ClientResponseDTO>> GetClientsByClassificationAsync(string classification, string userId)
        {
            try
            {
                // Get all clients and filter by classification
                var allClients = await _unitOfWork.Clients.GetMyClientsAsync(userId, 1, 20);
                var clients = allClients.Where(c => c.Classification == classification);
                return clients.Select(MapToClientResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients by classification. Classification: {Classification}, UserId: {UserId}", classification, userId);
                throw;
            }
        }

        public async Task<ClientResponseDTO> FindOrCreateClientAsync(FindOrCreateClientDTO findDto, string userId)
        {
            try
            {
                // Try to find existing client
                var existingClient = await _unitOfWork.Clients.FindByNameAndTypeAsync(findDto.Name, findDto.Type);
                if (existingClient != null)
                {
                    return MapToClientResponseDTO(existingClient);
                }

                // Create new client if not found
                var createDto = new CreateClientDTO
                {
                    Name = findDto.Name,
                    Type = findDto.Type,
                    Specialization = findDto.Specialization
                };

                return await CreateClientAsync(createDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding or creating client. Name: {Name}, Type: {Type}, UserId: {UserId}", 
                    findDto.Name, findDto.Type, userId);
                throw;
            }
        }

        public async Task<ClientStatisticsDTO> GetClientStatisticsAsync(string userId)
        {
            try
            {
                // Get all clients for the user
                var clients = await _unitOfWork.Clients.GetMyClientsAsync(userId, 1, 20);
                
                // Calculate statistics
                var statistics = new ClientStatisticsDTO
                {
                    TotalVisits = 0, // This would be calculated from task progresses
                    TotalOffers = 0, // This would be calculated from offers
                    SuccessfulDeals = 0, // This would be calculated from deals
                    FailedDeals = 0, // This would be calculated from deals
                    TotalRevenue = 0, // This would be calculated from successful deals
                    AverageSatisfaction = null // This would be calculated from task progresses
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client statistics. UserId: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Mapping Methods

        private static ClientResponseDTO MapToClientResponseDTO(Client client)
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
                Status = client.Status,
                Priority = client.Priority,
                Classification = client.Classification,
                LastContactDate = client.LastContactDate,
                NextContactDate = client.NextContactDate,
                SatisfactionRating = client.SatisfactionRating,
                CreatedAt = client.CreatedAt
            };
        }

        private static TaskProgressSummaryDTO MapToTaskProgressSummaryDTO(TaskProgress taskProgress)
        {
            return new TaskProgressSummaryDTO
            {
                Id = taskProgress.Id,
                ProgressDate = taskProgress.ProgressDate,
                ProgressType = taskProgress.ProgressType,
                VisitResult = taskProgress.VisitResult,
                NextStep = taskProgress.NextStep,
                SatisfactionRating = taskProgress.SatisfactionRating
            };
        }

        private static OfferSummaryDTO MapToOfferSummaryDTO(SalesOffer offer)
        {
            return new OfferSummaryDTO
            {
                Id = offer.Id,
                CreatedAt = offer.CreatedAt,
                TotalAmount = offer.TotalAmount,
                Status = offer.Status,
                ValidUntil = offer.ValidUntil
            };
        }

        private static DealSummaryDTO MapToDealSummaryDTO(SalesDeal deal)
        {
            return new DealSummaryDTO
            {
                Id = deal.Id,
                ClosedDate = deal.ClosedDate,
                DealValue = deal.DealValue,
                Status = deal.Status
            };
        }

        #endregion
    }
}