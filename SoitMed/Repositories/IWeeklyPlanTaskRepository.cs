using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for weekly plan task repository
    /// </summary>
    public interface IWeeklyPlanTaskRepository : IBaseRepository<WeeklyPlanTask>
    {
        Task<List<WeeklyPlanTask>> GetTasksByWeeklyPlanIdAsync(long weeklyPlanId);
        Task<List<WeeklyPlanTask>> GetTasksByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<WeeklyPlanTask>> GetOverdueTasksAsync(string employeeId);
        Task<List<WeeklyPlanTask>> GetTasksByClientIdAsync(long clientId);
        Task<List<WeeklyPlanTask>> GetTasksByClientClassificationAsync(string classification);
        Task<WeeklyPlanTask?> GetTaskWithDetailsAsync(long taskId);
        Task<List<WeeklyPlanTask>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<int> GetCompletedTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<WeeklyPlanTask>> GetTasksNeedingFollowUpAsync(string employeeId);
        Task<List<WeeklyPlanTask>> GetTasksByClientStatusAsync(string clientStatus);
        // REMOVED methods that used deleted fields:
        // - GetTasksByStatusAsync (Status moved to TaskProgress)
        // - GetTasksByPriorityAsync (Priority moved to TaskProgress)
        // - GetTasksByTaskTypeAsync (TaskType removed)
        // - GetTasksByPlaceTypeAsync (PlaceType removed)
    }
}



