using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class DailyProgressRepository : IDailyProgressRepository
    {
        private readonly Context _context;

        public DailyProgressRepository(Context context)
        {
            _context = context;
        }

        public async Task<DailyProgress?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.DailyProgresses
                .FirstOrDefaultAsync(dp => dp.Id == id && dp.IsActive, cancellationToken);
        }

        public async Task<DailyProgress?> GetByWeeklyPlanAndDateAsync(long weeklyPlanId, DateOnly progressDate, CancellationToken cancellationToken = default)
        {
            return await _context.DailyProgresses
                .FirstOrDefaultAsync(dp => dp.WeeklyPlanId == weeklyPlanId 
                    && dp.ProgressDate == progressDate 
                    && dp.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<DailyProgress>> GetByWeeklyPlanIdAsync(long weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.DailyProgresses
                .Where(dp => dp.WeeklyPlanId == weeklyPlanId && dp.IsActive)
                .OrderBy(dp => dp.ProgressDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<DailyProgress> CreateAsync(DailyProgress dailyProgress, CancellationToken cancellationToken = default)
        {
            _context.DailyProgresses.Add(dailyProgress);
            await _context.SaveChangesAsync(cancellationToken);
            return dailyProgress;
        }

        public async Task<DailyProgress> UpdateAsync(DailyProgress dailyProgress, CancellationToken cancellationToken = default)
        {
            dailyProgress.UpdatedAt = DateTime.UtcNow;
            _context.DailyProgresses.Update(dailyProgress);
            await _context.SaveChangesAsync(cancellationToken);
            return dailyProgress;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var dailyProgress = await _context.DailyProgresses.FindAsync(new object[] { id }, cancellationToken);
            if (dailyProgress == null)
                return false;

            dailyProgress.IsActive = false;
            dailyProgress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.DailyProgresses
                .AnyAsync(dp => dp.Id == id && dp.IsActive, cancellationToken);
        }

        public async Task<bool> BelongsToWeeklyPlanAsync(int progressId, long weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.DailyProgresses
                .AnyAsync(dp => dp.Id == progressId && dp.WeeklyPlanId == weeklyPlanId && dp.IsActive, cancellationToken);
        }
    }
}










