using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class WeeklyPlanTaskRepository : IWeeklyPlanTaskRepository
    {
        private readonly Context _context;

        public WeeklyPlanTaskRepository(Context context)
        {
            _context = context;
        }

        public async Task<WeeklyPlanTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlanTask>> GetByWeeklyPlanIdAsync(int weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.WeeklyPlanId == weeklyPlanId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<WeeklyPlanTask> CreateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            task.IsActive = true;

            _context.WeeklyPlanTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<WeeklyPlanTask> UpdateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyPlanTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var task = await _context.WeeklyPlanTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);

            if (task == null)
                return false;

            // Soft delete
            task.IsActive = false;
            task.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyPlanTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }

        public async Task<bool> BelongsToWeeklyPlanAsync(int taskId, int weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .AnyAsync(t => t.Id == taskId && t.WeeklyPlanId == weeklyPlanId && t.IsActive, cancellationToken);
        }
    }
}

