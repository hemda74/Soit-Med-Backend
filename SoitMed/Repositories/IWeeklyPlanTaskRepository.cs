using SoitMed.Models;

namespace SoitMed.Repositories
{
<<<<<<< HEAD
    public interface IWeeklyPlanTaskRepository
    {
        Task<WeeklyPlanTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlanTask>> GetByWeeklyPlanIdAsync(int weeklyPlanId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> CreateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> UpdateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> BelongsToWeeklyPlanAsync(int taskId, int weeklyPlanId, CancellationToken cancellationToken = default);
=======
    /// <summary>
    /// Interface for weekly plan task repository
    /// </summary>
    public interface IWeeklyPlanTaskRepository : IBaseRepository<WeeklyPlanTask>
    {
        Task<List<WeeklyPlanTask>> GetTasksByWeeklyPlanIdAsync(long weeklyPlanId);
        Task<List<WeeklyPlanTask>> GetTasksByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<WeeklyPlanTask>> GetOverdueTasksAsync(string employeeId);
        Task<List<WeeklyPlanTask>> GetTasksByStatusAsync(string status);
        Task<List<WeeklyPlanTask>> GetTasksByPriorityAsync(string priority);
        Task<List<WeeklyPlanTask>> GetTasksByClientIdAsync(long clientId);
        Task<List<WeeklyPlanTask>> GetTasksByTaskTypeAsync(string taskType);
        Task<List<WeeklyPlanTask>> GetTasksByClientClassificationAsync(string classification);
        Task<WeeklyPlanTask?> GetTaskWithDetailsAsync(long taskId);
        Task<List<WeeklyPlanTask>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<int> GetCompletedTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate);
        Task<List<WeeklyPlanTask>> GetTasksNeedingFollowUpAsync(string employeeId);
        Task<List<WeeklyPlanTask>> GetTasksByClientStatusAsync(string clientStatus);
        Task<List<WeeklyPlanTask>> GetTasksByPlaceTypeAsync(string placeType);
>>>>>>> dev
    }
}



<<<<<<< HEAD

=======
>>>>>>> dev
