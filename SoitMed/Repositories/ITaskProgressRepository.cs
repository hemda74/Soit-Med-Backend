using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for task progress repository
    /// </summary>
    public interface ITaskProgressRepository : IBaseRepository<TaskProgress>
    {
        Task<List<TaskProgress>> GetProgressesByTaskIdAsync(int taskId);
        Task<List<TaskProgress>> GetProgressesByClientIdAsync(long clientId);
        Task<List<TaskProgress>> GetProgressesByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<TaskProgress>> GetAllProgressesAsync(DateTime? startDate, DateTime? endDate);
        Task<List<TaskProgress>> GetProgressesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<TaskProgress>> GetProgressesByProgressTypeAsync(string progressType);
        Task<List<TaskProgress>> GetProgressesByVisitResultAsync(string visitResult);
        Task<List<TaskProgress>> GetProgressesByNextStepAsync(string nextStep);
        Task<List<TaskProgress>> GetOverdueFollowUpsAsync(string employeeId, DateTime currentDate);
        Task<List<TaskProgress>> GetProgressesWithOfferRequestsAsync();
        Task<List<TaskProgress>> GetProgressesWithDealsAsync();
        Task<TaskProgress?> GetProgressWithDetailsAsync(long progressId);
        Task<int> GetProgressCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<int> GetProgressCountByClientAsync(long clientId);
      
        Task<List<TaskProgress>> GetRecentProgressesAsync(int count);
        Task<List<TaskProgress>> GetProgressesByTaskStatusAsync(string taskStatus);
    }
}



