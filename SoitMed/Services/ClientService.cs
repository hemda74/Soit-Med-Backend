using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for client business logic operations
    /// </summary>
    public class ClientService : BaseService, IClientService
    {
        public ClientService(IUnitOfWork unitOfWork, ILogger<ClientService> logger) 
            : base(unitOfWork, logger)
        {
        }

        public async Task<ServiceResult<ClientSearchResult>> SearchClientsAsync(SearchClientDTO searchDto, string userId)
        {
            try
            {
                var clients = await UnitOfWork.Clients.SearchClientsAsync(searchDto.Query, searchDto.Page, searchDto.PageSize);
                var totalCount = await UnitOfWork.Clients.CountAsync();

                var clientDtos = clients.Select(MapToResponseDTO);
                var pagination = new PaginationInfo
                {
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                var result = new ClientSearchResult
                {
                    Clients = clientDtos,
                    Pagination = pagination
                };

                return ServiceResult<ClientSearchResult>.Success(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error searching clients for user {UserId}", userId);
                return ServiceResult<ClientSearchResult>.Failure("حدث خطأ في البحث عن العملاء", "SEARCH_ERROR");
            }
        }

        public async Task<ServiceResult<ClientResponseDTO>> CreateClientAsync(CreateClientDTO createDto, string userId)
        {
            try
            {
                // Check if client already exists
                var existingClient = await UnitOfWork.Clients.FindByNameAndTypeAsync(createDto.Name, createDto.Type);
                if (existingClient != null)
                {
                    return ServiceResult<ClientResponseDTO>.Failure("العميل موجود بالفعل", "CLIENT_EXISTS");
                }

                var client = new Client
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

                await UnitOfWork.Clients.CreateAsync(client);
                await UnitOfWork.SaveChangesAsync();

                var responseDto = MapToResponseDTO(client);
                return ServiceResult<ClientResponseDTO>.Success(responseDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating client for user {UserId}", userId);
                return ServiceResult<ClientResponseDTO>.Failure("حدث خطأ في إنشاء العميل", "CREATE_ERROR");
            }
        }

        public async Task<ServiceResult<ClientResponseDTO>> GetClientAsync(long id, string userId)
        {
            try
            {
                var client = await UnitOfWork.Clients.GetByIdAsync(id);
                if (client == null)
                {
                    return ServiceResult<ClientResponseDTO>.Failure("العميل غير موجود", "CLIENT_NOT_FOUND");
                }

                // Check if user can access this client
                if (!await CanAccessClientAsync(id, userId))
                {
                    return ServiceResult<ClientResponseDTO>.Failure("ليس لديك صلاحية للوصول إلى هذا العميل", "ACCESS_DENIED");
                }

                var responseDto = MapToResponseDTO(client);
                return ServiceResult<ClientResponseDTO>.Success(responseDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting client {ClientId} for user {UserId}", id, userId);
                return ServiceResult<ClientResponseDTO>.Failure("حدث خطأ في جلب بيانات العميل", "GET_ERROR");
            }
        }

        public async Task<ServiceResult<ClientResponseDTO>> UpdateClientAsync(long id, UpdateClientDTO updateDto, string userId)
        {
            try
            {
                var client = await UnitOfWork.Clients.GetByIdAsync(id);
                if (client == null)
                {
                    return ServiceResult<ClientResponseDTO>.Failure("العميل غير موجود", "CLIENT_NOT_FOUND");
                }

                // Check if user can access this client
                if (!await CanAccessClientAsync(id, userId))
                {
                    return ServiceResult<ClientResponseDTO>.Failure("ليس لديك صلاحية لتعديل هذا العميل", "ACCESS_DENIED");
                }

                // Update client properties using business logic methods
                if (!string.IsNullOrEmpty(updateDto.Name))
                    client.Name = updateDto.Name;
                if (!string.IsNullOrEmpty(updateDto.Type))
                    client.Type = updateDto.Type;
                if (updateDto.Specialization != null)
                    client.Specialization = updateDto.Specialization;
                if (updateDto.Location != null)
                    client.Location = updateDto.Location;

                // Use business logic methods for related updates
                client.UpdateContactInfo(updateDto.Phone, updateDto.Email, updateDto.Website);
                client.UpdateAddress(updateDto.Address, updateDto.City, updateDto.Governorate, updateDto.PostalCode);
                client.UpdateContactPerson(updateDto.ContactPerson, updateDto.ContactPersonPhone, 
                    updateDto.ContactPersonEmail, updateDto.ContactPersonPosition);

                if (!string.IsNullOrEmpty(updateDto.Status) || !string.IsNullOrEmpty(updateDto.Priority))
                {
                    client.UpdateStatus(updateDto.Status ?? client.Status, 
                        updateDto.Priority ?? client.Priority, 
                        updateDto.PotentialValue);
                }

                if (updateDto.AssignedTo != null)
                    client.AssignedTo = updateDto.AssignedTo;

                if (updateDto.Notes != null)
                    client.Notes = updateDto.Notes;

                await UnitOfWork.SaveChangesAsync();

                var responseDto = MapToResponseDTO(client);
                return ServiceResult<ClientResponseDTO>.Success(responseDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating client {ClientId} for user {UserId}", id, userId);
                return ServiceResult<ClientResponseDTO>.Failure("حدث خطأ في تحديث العميل", "UPDATE_ERROR");
            }
        }

        public async Task<ServiceResult<ClientResponseDTO>> FindOrCreateClientAsync(FindOrCreateClientDTO findDto, string userId)
        {
            try
            {
                var client = await UnitOfWork.Clients.FindOrCreateClientAsync(
                    findDto.Name, 
                    findDto.Type, 
                    findDto.Specialization, 
                    userId);

                var responseDto = MapToResponseDTO(client);
                return ServiceResult<ClientResponseDTO>.Success(responseDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error finding or creating client for user {UserId}", userId);
                return ServiceResult<ClientResponseDTO>.Failure("حدث خطأ في البحث عن العميل أو إنشاؤه", "FIND_CREATE_ERROR");
            }
        }

        public async Task<ServiceResult<ClientSearchResult>> GetMyClientsAsync(string userId, int page, int pageSize)
        {
            try
            {
                var clients = await UnitOfWork.Clients.GetMyClientsAsync(userId, page, pageSize);
                var totalCount = await UnitOfWork.Clients.CountAsync(c => c.CreatedBy == userId || c.AssignedTo == userId);

                var clientDtos = clients.Select(MapToResponseDTO);
                var pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                var result = new ClientSearchResult
                {
                    Clients = clientDtos,
                    Pagination = pagination
                };

                return ServiceResult<ClientSearchResult>.Success(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting my clients for user {UserId}", userId);
                return ServiceResult<ClientSearchResult>.Failure("حدث خطأ في جلب عملائي", "GET_MY_CLIENTS_ERROR");
            }
        }

        public async Task<ServiceResult<IEnumerable<ClientFollowUpDTO>>> GetClientsNeedingFollowUpAsync(string userId)
        {
            try
            {
                var clients = await UnitOfWork.Clients.GetClientsNeedingFollowUpAsync(userId);
                var followUpDtos = clients.Select(c => new ClientFollowUpDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    NextContactDate = c.NextContactDate,
                    Priority = c.Priority,
                    Status = c.Status
                });

                return ServiceResult<IEnumerable<ClientFollowUpDTO>>.Success(followUpDtos);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting clients needing follow-up for user {UserId}", userId);
                return ServiceResult<IEnumerable<ClientFollowUpDTO>>.Failure("حدث خطأ في جلب العملاء المحتاجين لمتابعة", "GET_FOLLOW_UP_ERROR");
            }
        }

        public async Task<ServiceResult<ClientStatisticsDTO>> GetClientStatisticsAsync(string userId)
        {
            try
            {
                var statistics = await UnitOfWork.Clients.GetClientStatisticsAsync(userId);
                
                // Map the anonymous object to our DTO
                var statisticsDto = new ClientStatisticsDTO
                {
                    MyClientsCount = (int)statistics.GetType().GetProperty("MyClientsCount")?.GetValue(statistics)!,
                    TotalClientsCount = (int)statistics.GetType().GetProperty("TotalClientsCount")?.GetValue(statistics)!,
                    ClientsByType = ((IEnumerable<dynamic>)statistics.GetType().GetProperty("ClientsByType")?.GetValue(statistics)!).Select(x => new ClientTypeCount { Type = x.Type, Count = x.Count }),
                    ClientsByStatus = ((IEnumerable<dynamic>)statistics.GetType().GetProperty("ClientsByStatus")?.GetValue(statistics)!).Select(x => new ClientStatusCount { Status = x.Status, Count = x.Count }),
                    ClientsByPriority = ((IEnumerable<dynamic>)statistics.GetType().GetProperty("ClientsByPriority")?.GetValue(statistics)!).Select(x => new ClientPriorityCount { Priority = x.Priority, Count = x.Count })
                };

                return ServiceResult<ClientStatisticsDTO>.Success(statisticsDto);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting client statistics for user {UserId}", userId);
                return ServiceResult<ClientStatisticsDTO>.Failure("حدث خطأ في جلب إحصائيات العملاء", "GET_STATISTICS_ERROR");
            }
        }

        public async Task<bool> CanAccessClientAsync(long clientId, string userId)
        {
            try
            {
                var client = await UnitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                    return false;

                return client.CreatedBy == userId || client.AssignedTo == userId;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking client access for user {UserId} and client {ClientId}", userId, clientId);
                return false;
            }
        }

        /// <summary>
        /// Maps a Client entity to ClientResponseDTO
        /// </summary>
        private static ClientResponseDTO MapToResponseDTO(Client client)
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
    }
}
