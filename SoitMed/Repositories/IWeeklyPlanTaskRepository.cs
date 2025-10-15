using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanTaskRepository
    {
        Task<WeeklyPlanTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlanTask>> GetByWeeklyPlanIdAsync(int weeklyPlanId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> CreateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTask> UpdateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> BelongsToWeeklyPlanAsync(int taskId, int weeklyPlanId, CancellationToken cancellationToken = default);
    }
}




