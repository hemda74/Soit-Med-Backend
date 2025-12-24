using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanTaskRepository
    {
        Task<WeeklyPlanTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlanTask>> GetByWeeklyPlanIdAsync(long weeklyPlanId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> CreateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlanTask>> CreateRangeAsync(IEnumerable<WeeklyPlanTask> tasks, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> UpdateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> BelongsToWeeklyPlanAsync(int taskId, long weeklyPlanId, CancellationToken cancellationToken = default);
    }
}




