using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IDailyProgressRepository
    {
        Task<DailyProgress?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DailyProgress?> GetByWeeklyPlanAndDateAsync(long weeklyPlanId, DateOnly progressDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<DailyProgress>> GetByWeeklyPlanIdAsync(long weeklyPlanId, CancellationToken cancellationToken = default);
        Task<DailyProgress> CreateAsync(DailyProgress dailyProgress, CancellationToken cancellationToken = default);
        Task<DailyProgress> UpdateAsync(DailyProgress dailyProgress, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> BelongsToWeeklyPlanAsync(int progressId, long weeklyPlanId, CancellationToken cancellationToken = default);
    }
}










