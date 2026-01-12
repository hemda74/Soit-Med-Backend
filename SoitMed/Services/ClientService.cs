using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Location;
using SoitMed.Repositories;
using System.Text.Json;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing clients with complete history tracking
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientService> _logger;
        private readonly IWebHostEnvironment _environment;

        public ClientService(IUnitOfWork unitOfWork, ILogger<ClientService> logger, IWebHostEnvironment environment) 
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
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
                // The controller's [Authorize(Roles="SalesMan,SalesManager,SuperAdmin")] handles role check
                // For data filtering, check if client is assigned to user when needed

                // Load sequentially to avoid DbContext concurrency issues (same DbContext instance)
                var taskProgresses = await _unitOfWork.TaskProgresses
                    .GetProgressesByClientIdAsync(clientId);
                var offers = await _unitOfWork.SalesOffers
                    .GetOffersByClientIdAsync(clientId.ToString());
                var deals = await _unitOfWork.SalesDeals
                    .GetDealsByClientIdAsync(clientId);

                // Calculate statistics
                // Count deals that are approved, sent to legal, or successful as successful deals
                // Include: Success (completed), SentToLegal, and Approved
                // Only count failed or rejected deals as failed
                var successfulDealStatuses = new[] { "Success", "SentToLegal", "Approved" };
                var failedDealStatuses = new[] { "Failed", "RejectedByManager", "RejectedBySuperAdmin" };
                
                // Calculate successful deals count - includes both SentToLegal AND Success (Completed)
                var successfulDealsCount = deals.Count(d => successfulDealStatuses.Contains(d.Status));
                
                // Calculate total revenue from successful deals
                // This includes revenue from BOTH SentToLegal deals AND Success (Completed) deals
                // DealValue is non-nullable decimal, so we can use it directly
                var totalRevenueValue = deals
                    .Where(d => successfulDealStatuses.Contains(d.Status))
                    .Sum(d => d.DealValue);
                
                var statistics = new ClientStatisticsDTO
                {
                    TotalVisits = taskProgresses.Count,
                    TotalOffers = offers.Count,
                    SuccessfulDeals = successfulDealsCount,
                    FailedDeals = deals.Count(d => failedDealStatuses.Contains(d.Status)),
                    TotalRevenue = totalRevenueValue,
                   
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

        public async Task<PaginatedClientsResponseDTO> SearchClientsAsync(SearchClientDTO searchDto, string userId)
        {
            try
            {
                // Get total count first using the same query logic as repository
                var context = _unitOfWork.GetContext();
                var baseQuery = context.Clients.AsQueryable();
                
                // Apply search filter if query is provided
                if (!string.IsNullOrWhiteSpace(searchDto.Query))
                {
                    var query = searchDto.Query;
                    var namePattern = "%" + query + "%";
                    baseQuery = baseQuery.Where(c => 
                        EF.Functions.Like(c.Name, namePattern) ||
                        (c.Specialization != null && EF.Functions.Like(c.Specialization, namePattern)) ||
                        (c.Location != null && EF.Functions.Like(c.Location, namePattern)) ||
                        (c.Phone != null && EF.Functions.Like(c.Phone, namePattern)) ||
                        (c.Email != null && EF.Functions.Like(c.Email, namePattern)));
                }

                // Filter by classification
                if (!string.IsNullOrWhiteSpace(searchDto.Classification))
                {
                    baseQuery = baseQuery.Where(c => c.Classification == searchDto.Classification);
                }

                // Filter by assigned salesman
                if (!string.IsNullOrWhiteSpace(searchDto.AssignedSalesManId))
                {
                    baseQuery = baseQuery.Where(c => c.AssignedTo == searchDto.AssignedSalesManId);
                }

                // Filter by city
                if (!string.IsNullOrWhiteSpace(searchDto.City))
                {
                    var cityPattern = "%" + searchDto.City + "%";
                    baseQuery = baseQuery.Where(c => c.City != null && EF.Functions.Like(c.City, cityPattern));
                }

                // Filter by governorate - need to look up governorate name from ID
                if (searchDto.GovernorateId.HasValue)
                {
                    try
                    {
                        // Look up the governorate name from the Governorates table first
                        var governorate = await context.Governorates
                            .AsNoTracking()
                            .FirstOrDefaultAsync(g => g.GovernorateId == searchDto.GovernorateId.Value.ToString());
                        
                        if (governorate != null && !string.IsNullOrWhiteSpace(governorate.Name))
                        {
                            var govPattern = "%" + governorate.Name + "%";
                            baseQuery = baseQuery.Where(c => c.Governorate != null && 
                                EF.Functions.Like(c.Governorate, govPattern));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to look up governorate {GovernorateId} for client search. Continuing without governorate filter.", 
                            searchDto.GovernorateId.Value);
                        // Continue without governorate filter rather than failing the entire search
                    }
                }

                // Filter by equipment categories
                if (searchDto.EquipmentCategories != null && searchDto.EquipmentCategories.Any())
                {
                    // InterestedEquipmentCategories is stored as JSON array string like ["Category1", "Category2"]
                    // Pre-process all search patterns outside LINQ to avoid string operations in query
                    var allPatterns = new List<string>();
                    foreach (var category in searchDto.EquipmentCategories)
                    {
                        // Add quoted versions (JSON format) - use string concatenation, not interpolation
                        allPatterns.Add("\"" + category + "\"");
                        allPatterns.Add("\"" + category.Replace("+", " ") + "\"");
                        allPatterns.Add("\"" + category.Replace(" ", "+") + "\"");
                        // Add unquoted versions (fallback)
                        allPatterns.Add(category);
                        allPatterns.Add(category.Replace("+", " "));
                        allPatterns.Add(category.Replace(" ", "+"));
                    }
                    
                    // Build a single OR condition that EF Core can translate
                    // Limit to reasonable number of patterns to avoid query complexity
                    var patternsToCheck = allPatterns.Take(18).ToList();
                    
                    if (patternsToCheck.Count > 0)
                    {
                        var p0 = patternsToCheck[0];
                        var p1 = patternsToCheck.Count > 1 ? patternsToCheck[1] : p0;
                        var p2 = patternsToCheck.Count > 2 ? patternsToCheck[2] : p0;
                        var p3 = patternsToCheck.Count > 3 ? patternsToCheck[3] : p0;
                        var p4 = patternsToCheck.Count > 4 ? patternsToCheck[4] : p0;
                        var p5 = patternsToCheck.Count > 5 ? patternsToCheck[5] : p0;
                        var p6 = patternsToCheck.Count > 6 ? patternsToCheck[6] : p0;
                        var p7 = patternsToCheck.Count > 7 ? patternsToCheck[7] : p0;
                        var p8 = patternsToCheck.Count > 8 ? patternsToCheck[8] : p0;
                        var p9 = patternsToCheck.Count > 9 ? patternsToCheck[9] : p0;
                        var p10 = patternsToCheck.Count > 10 ? patternsToCheck[10] : p0;
                        var p11 = patternsToCheck.Count > 11 ? patternsToCheck[11] : p0;
                        var p12 = patternsToCheck.Count > 12 ? patternsToCheck[12] : p0;
                        var p13 = patternsToCheck.Count > 13 ? patternsToCheck[13] : p0;
                        var p14 = patternsToCheck.Count > 14 ? patternsToCheck[14] : p0;
                        var p15 = patternsToCheck.Count > 15 ? patternsToCheck[15] : p0;
                        var p16 = patternsToCheck.Count > 16 ? patternsToCheck[16] : p0;
                        var p17 = patternsToCheck.Count > 17 ? patternsToCheck[17] : p0;
                        
                        baseQuery = baseQuery.Where(c => 
                            !string.IsNullOrEmpty(c.InterestedEquipmentCategories) &&
                            (c.InterestedEquipmentCategories.Contains(p0) ||
                             (patternsToCheck.Count > 1 && c.InterestedEquipmentCategories.Contains(p1)) ||
                             (patternsToCheck.Count > 2 && c.InterestedEquipmentCategories.Contains(p2)) ||
                             (patternsToCheck.Count > 3 && c.InterestedEquipmentCategories.Contains(p3)) ||
                             (patternsToCheck.Count > 4 && c.InterestedEquipmentCategories.Contains(p4)) ||
                             (patternsToCheck.Count > 5 && c.InterestedEquipmentCategories.Contains(p5)) ||
                             (patternsToCheck.Count > 6 && c.InterestedEquipmentCategories.Contains(p6)) ||
                             (patternsToCheck.Count > 7 && c.InterestedEquipmentCategories.Contains(p7)) ||
                             (patternsToCheck.Count > 8 && c.InterestedEquipmentCategories.Contains(p8)) ||
                             (patternsToCheck.Count > 9 && c.InterestedEquipmentCategories.Contains(p9)) ||
                             (patternsToCheck.Count > 10 && c.InterestedEquipmentCategories.Contains(p10)) ||
                             (patternsToCheck.Count > 11 && c.InterestedEquipmentCategories.Contains(p11)) ||
                             (patternsToCheck.Count > 12 && c.InterestedEquipmentCategories.Contains(p12)) ||
                             (patternsToCheck.Count > 13 && c.InterestedEquipmentCategories.Contains(p13)) ||
                             (patternsToCheck.Count > 14 && c.InterestedEquipmentCategories.Contains(p14)) ||
                             (patternsToCheck.Count > 15 && c.InterestedEquipmentCategories.Contains(p15)) ||
                             (patternsToCheck.Count > 16 && c.InterestedEquipmentCategories.Contains(p16)) ||
                             (patternsToCheck.Count > 17 && c.InterestedEquipmentCategories.Contains(p17)))
                        );
                    }
                }

                // Filter by client category (Potential vs Existing)
                if (!string.IsNullOrWhiteSpace(searchDto.ClientCategory))
                {
                    if (searchDto.ClientCategory.Equals("Existing", StringComparison.OrdinalIgnoreCase))
                    {
                        // Existing clients: have LegacyCustomerId OR have contracts OR have deals
                        var clientIdsWithContracts = context.Contracts.Select(c => c.ClientId).Distinct();
                        var clientIdsWithDeals = context.SalesDeals.Select(d => d.ClientId).Distinct();
                        
                        baseQuery = baseQuery.Where(c => 
                            c.LegacyCustomerId.HasValue ||
                            clientIdsWithContracts.Contains(c.Id) ||
                            clientIdsWithDeals.Contains(c.Id));
                    }
                    else if (searchDto.ClientCategory.Equals("Potential", StringComparison.OrdinalIgnoreCase))
                    {
                        // Potential clients: do NOT have LegacyCustomerId AND do NOT have contracts AND do NOT have deals
                        var clientIdsWithContracts = context.Contracts.Select(c => c.ClientId).Distinct();
                        var clientIdsWithDeals = context.SalesDeals.Select(d => d.ClientId).Distinct();
                        
                        baseQuery = baseQuery.Where(c => 
                            !c.LegacyCustomerId.HasValue &&
                            !clientIdsWithContracts.Contains(c.Id) &&
                            !clientIdsWithDeals.Contains(c.Id));
                    }
                }
                
                var totalCount = await baseQuery.CountAsync();
                
                // Get paginated clients with all filters
                var clients = await _unitOfWork.Clients.SearchClientsAsync(searchDto);
                var clientDTOs = clients.Select(MapToClientResponseDTO).ToList();
                
                return new PaginatedClientsResponseDTO
                {
                    Clients = clientDTOs,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };
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

        public async Task<(IEnumerable<ClientResponseDTO> Clients, int TotalCount, int PageNumber, int PageSize)> GetAllClientsAsync(int pageNumber = 1, int pageSize = 25, string? searchTerm = null)
        {
            try
            {
                var (clients, totalCount) = await _unitOfWork.Clients.GetAllClientsAsync(pageNumber, pageSize, searchTerm);
                var clientList = clients.ToList();
                
                // Get client IDs for batch queries
                var clientIds = clientList.Select(c => c.Id).ToList();
                
                // Batch query for contract counts per client
                var context = _unitOfWork.GetContext();
                var contractCounts = await context.Contracts
                    .AsNoTracking()
                    .Where(c => clientIds.Contains(c.ClientId))
                    .GroupBy(c => c.ClientId)
                    .Select(g => new { ClientId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ClientId, x => x.Count);
                
                // Batch query for deal counts per client
                var dealCounts = await context.SalesDeals
                    .AsNoTracking()
                    .Where(d => clientIds.Contains(d.ClientId))
                    .GroupBy(d => d.ClientId)
                    .Select(g => new { ClientId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ClientId, x => x.Count);
                
                // Map clients with counts
                var clientDtos = clientList.Select(client => 
                {
                    var contractCount = contractCounts.GetValueOrDefault(client.Id, 0);
                    var dealCount = dealCounts.GetValueOrDefault(client.Id, 0);
                    return MapToClientResponseDTOWithCounts(client, contractCount, dealCount);
                }).ToList();
                
                return (clientDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all clients. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}", pageNumber, pageSize, searchTerm ?? "null");
                throw;
            }
        }

        #endregion

        #region Mapping Methods

        private static ClientResponseDTO MapToClientResponseDTO(Client client)
        {
            // Parse InterestedEquipmentCategories from JSON string to List<string>
            List<string>? equipmentCategoriesList = null;
            if (!string.IsNullOrWhiteSpace(client.InterestedEquipmentCategories))
            {
                try
                {
                    // Try to parse as JSON array
                    equipmentCategoriesList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(client.InterestedEquipmentCategories);
                }
                catch
                {
                    // If parsing fails, try to parse as array of objects with Category property
                    try
                    {
                        var categoryObjects = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(client.InterestedEquipmentCategories);
                        if (categoryObjects != null)
                        {
                            equipmentCategoriesList = categoryObjects
                                .Where(obj => obj.ContainsKey("Category"))
                                .Select(obj => obj["Category"])
                                .ToList();
                        }
                    }
                    catch
                    {
                        // If all parsing fails, leave as null
                        equipmentCategoriesList = null;
                    }
                }
            }

            return new ClientResponseDTO
            {
                Id = long.Parse(client.Id),
                Name = client.Name,
                Type = client.Type,
                OrganizationName = client.OrganizationName,
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
                PotentialValue = client.PotentialValue,
                ContactPerson = client.ContactPerson,
                ContactPersonPhone = client.ContactPersonPhone,
                ContactPersonEmail = client.ContactPersonEmail,
                ContactPersonPosition = client.ContactPersonPosition,
                LastContactDate = client.LastContactDate,
                NextContactDate = client.NextContactDate,
                SatisfactionRating = client.SatisfactionRating,
                InterestedEquipmentCategories = equipmentCategoriesList, // Use parsed list instead of JSON string
                CreatedBy = client.CreatedBy,
                AssignedTo = client.AssignedTo,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt,
                NationalId = client.NationalId,
                NationalIdFrontImage = client.NationalIdFrontImage,
                NationalIdBackImage = client.NationalIdBackImage,
                TaxCardNumber = client.TaxCardNumber,
                TaxCardImage = client.TaxCardImage,
                LegalDocumentsSubmittedAt = client.LegalDocumentsSubmittedAt,
                HasAccount = client.RelatedUserId != null,
                // Default values - will be updated by MapToClientResponseDTOWithCounts
                HasEquipment = client.LegacyCustomerId.HasValue,
                ClientCategory = client.LegacyCustomerId.HasValue ? "Existing" : "Potential",
                ContractCount = 0,
                DealCount = 0,
                LegacyCustomerId = client.LegacyCustomerId
            };
        }

        /// <summary>
        /// Maps a client to ClientResponseDTO with contract and deal counts
        /// </summary>
        private static ClientResponseDTO MapToClientResponseDTOWithCounts(Client client, int contractCount, int dealCount)
        {
            var dto = MapToClientResponseDTO(client);
            dto.ContractCount = contractCount;
            dto.DealCount = dealCount;
            // Client is "Existing" if they have LegacyCustomerId OR contracts OR deals
            dto.HasEquipment = client.LegacyCustomerId.HasValue || contractCount > 0 || dealCount > 0;
            dto.ClientCategory = dto.HasEquipment ? "Existing" : "Potential";
            return dto;
        }

        private static TaskProgressSummaryDTO MapToTaskProgressSummaryDTO(TaskProgress taskProgress)
        {
            return new TaskProgressSummaryDTO
            {
                Id = long.Parse(taskProgress.Id),
                ProgressDate = taskProgress.ProgressDate,
                ProgressType = taskProgress.ProgressType,
                VisitResult = taskProgress.VisitResult,
                NextStep = taskProgress.NextStep,
            };
        }

        private static OfferSummaryDTO MapToOfferSummaryDTO(SalesOffer offer)
        {
            // Deserialize ValidUntil from JSON string to List<string>
            List<string>? validUntilList = null;
            if (!string.IsNullOrWhiteSpace(offer.ValidUntil))
            {
                try
                {
                    validUntilList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(offer.ValidUntil);
                }
                catch
                {
                    // Fallback: treat as single string value (backward compatibility)
                    validUntilList = new List<string> { offer.ValidUntil };
                }
            }
            
            return new OfferSummaryDTO
            {
                Id = offer.Id,
                CreatedAt = offer.CreatedAt,
                TotalAmount = offer.TotalAmount,
                Status = offer.Status,
                ValidUntil = validUntilList
            };
        }

        private static DealSummaryDTO MapToDealSummaryDTO(SalesDeal deal)
        {
            return new DealSummaryDTO
            {
                Id = long.Parse(deal.Id),
                ClosedDate = deal.ClosedDate,
                DealValue = deal.DealValue,
                Status = deal.Status
            };
        }

        #region Legal Documents

        public async Task<ClientResponseDTO> SubmitClientLegalDocumentsAsync(
            long clientId, 
            string nationalId, 
            string nationalIdFrontImagePath, 
            string nationalIdBackImagePath, 
            string? taxCardNumber, 
            string? taxCardImagePath, 
            string userId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    throw new ArgumentException("Client not found", nameof(clientId));

                // Update client with legal documents
                client.NationalId = nationalId;
                client.NationalIdFrontImage = nationalIdFrontImagePath;
                client.NationalIdBackImage = nationalIdBackImagePath;
                client.TaxCardNumber = taxCardNumber;
                client.TaxCardImage = taxCardImagePath;
                client.LegalDocumentsSubmittedAt = DateTime.UtcNow;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Legal documents submitted for Client: {ClientId} by User: {UserId}", clientId, userId);
                return MapToClientResponseDTO(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting legal documents for Client: {ClientId}", clientId);
                throw;
            }
        }

        #endregion

        #endregion
    }
}