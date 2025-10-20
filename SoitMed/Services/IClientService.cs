using SoitMed.DTO;
using SoitMed.Models;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for client business logic operations
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Searches for clients based on query parameters
        /// </summary>
        Task<ServiceResult<ClientSearchResult>> SearchClientsAsync(SearchClientDTO searchDto, string userId);

        /// <summary>
        /// Creates a new client
        /// </summary>
        Task<ServiceResult<ClientResponseDTO>> CreateClientAsync(CreateClientDTO createDto, string userId);

        /// <summary>
        /// Gets a client by ID
        /// </summary>
        Task<ServiceResult<ClientResponseDTO>> GetClientAsync(long id, string userId);

        /// <summary>
        /// Updates an existing client
        /// </summary>
        Task<ServiceResult<ClientResponseDTO>> UpdateClientAsync(long id, UpdateClientDTO updateDto, string userId);

        /// <summary>
        /// Finds or creates a client
        /// </summary>
        Task<ServiceResult<ClientResponseDTO>> FindOrCreateClientAsync(FindOrCreateClientDTO findDto, string userId);

        /// <summary>
        /// Gets clients for a specific user
        /// </summary>
        Task<ServiceResult<ClientSearchResult>> GetMyClientsAsync(string userId, int page, int pageSize);

        /// <summary>
        /// Gets clients that need follow-up
        /// </summary>
        Task<ServiceResult<IEnumerable<ClientFollowUpDTO>>> GetClientsNeedingFollowUpAsync(string userId);

        /// <summary>
        /// Gets client statistics for a user
        /// </summary>
        Task<ServiceResult<ClientStatisticsDTO>> GetClientStatisticsAsync(string userId);

        /// <summary>
        /// Validates if a user can access a client
        /// </summary>
        Task<bool> CanAccessClientAsync(long clientId, string userId);
    }

    /// <summary>
    /// Service result wrapper for consistent error handling
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T> { IsSuccess = true, Data = data };
        }

        public static ServiceResult<T> Failure(string errorMessage, string? errorCode = null)
        {
            return new ServiceResult<T> { IsSuccess = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
        }
    }

    /// <summary>
    /// Client search result with pagination
    /// </summary>
    public class ClientSearchResult
    {
        public IEnumerable<ClientResponseDTO> Clients { get; set; } = new List<ClientResponseDTO>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }

    /// <summary>
    /// Pagination information
    /// </summary>
    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Client follow-up information
    /// </summary>
    public class ClientFollowUpDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? NextContactDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Client statistics information
    /// </summary>
    public class ClientStatisticsDTO
    {
        public int MyClientsCount { get; set; }
        public int TotalClientsCount { get; set; }
        public IEnumerable<ClientTypeCount> ClientsByType { get; set; } = new List<ClientTypeCount>();
        public IEnumerable<ClientStatusCount> ClientsByStatus { get; set; } = new List<ClientStatusCount>();
        public IEnumerable<ClientPriorityCount> ClientsByPriority { get; set; } = new List<ClientPriorityCount>();
    }

    public class ClientTypeCount
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ClientStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ClientPriorityCount
    {
        public string Priority { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}

