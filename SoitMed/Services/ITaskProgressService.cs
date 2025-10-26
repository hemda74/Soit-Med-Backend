using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for task progress management service
    /// </summary>
    public interface ITaskProgressService
    {
        #region Task Progress Management
        Task<TaskProgressResponseDTO> CreateProgressAsync(CreateTaskProgressDTO createDto, string userId);
        Task<TaskProgressResponseDTO> CreateProgressAndOfferRequestAsync(CreateTaskProgressWithOfferRequestDTO createDto, string userId);
        Task<TaskProgressResponseDTO?> GetProgressAsync(long progressId, string userId, string userRole);
        Task<List<TaskProgressResponseDTO>> GetProgressesByTaskAsync(long taskId, string userId, string userRole);
        Task<List<TaskProgressResponseDTO>> GetProgressesByClientAsync(long clientId, string userId, string userRole);
        Task<List<TaskProgressResponseDTO>> GetProgressesByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<TaskProgressResponseDTO>> GetAllProgressesAsync(DateTime? startDate, DateTime? endDate);
        Task<TaskProgressResponseDTO> UpdateProgressAsync(long progressId, CreateTaskProgressDTO updateDto, string userId);
        Task<bool> DeleteProgressAsync(long progressId, string userId);
        #endregion

        #region Business Logic Methods
        Task<bool> ValidateProgressAsync(CreateTaskProgressDTO progressDto);
        Task<bool> CanModifyProgressAsync(long progressId, string userId);
        Task<List<TaskProgressSummaryDTO>> GetProgressSummaryByClientAsync(long clientId);
        #endregion
    }
}

