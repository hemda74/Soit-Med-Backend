using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public class WeeklyPlanRepository : IWeeklyPlanRepository
    {
        private readonly Context _context;

        public WeeklyPlanRepository(Context context)
        {
            _context = context;
        }

        public async Task<WeeklyPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.IsActive, cancellationToken);
        }

        public async Task<WeeklyPlan?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .Include(wp => wp.Tasks.Where(t => t.IsActive))
                .Include(wp => wp.DailyProgresses.Where(dp => dp.IsActive))
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.IsActive, cancellationToken);
        }

        public async Task<WeeklyPlan?> GetByIdAndEmployeeIdAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .Include(wp => wp.Tasks.Where(t => t.IsActive))
                .Include(wp => wp.DailyProgresses.Where(dp => dp.IsActive))
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.EmployeeId == employeeId && wp.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlan>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .Include(wp => wp.Tasks.Where(t => t.IsActive))
                .Include(wp => wp.DailyProgresses.Where(dp => dp.IsActive))
                .Where(wp => wp.IsActive)
                .OrderByDescending(wp => wp.WeekStartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlan>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .Include(wp => wp.Tasks.Where(t => t.IsActive))
                .Include(wp => wp.DailyProgresses.Where(dp => dp.IsActive))
                .Where(wp => wp.EmployeeId == employeeId && wp.IsActive)
                .OrderByDescending(wp => wp.WeekStartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<WeeklyPlan> Plans, int TotalCount)> GetPaginatedAsync(
            Expression<Func<WeeklyPlan, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.WeeklyPlans
                .Include(wp => wp.Employee)
                .Include(wp => wp.Tasks.Where(t => t.IsActive))
                .Include(wp => wp.DailyProgresses.Where(dp => dp.IsActive))
                .Where(wp => wp.IsActive);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var plans = await query
                .OrderByDescending(wp => wp.WeekStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (plans, totalCount);
        }

        public async Task<WeeklyPlan> CreateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default)
        {
            _context.WeeklyPlans.Add(weeklyPlan);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Reload with navigation properties
            return (await GetByIdWithDetailsAsync(weeklyPlan.Id, cancellationToken))!;
        }

        public async Task<WeeklyPlan> UpdateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default)
        {
            weeklyPlan.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyPlans.Update(weeklyPlan);
            await _context.SaveChangesAsync(cancellationToken);
            return weeklyPlan;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var weeklyPlan = await _context.WeeklyPlans.FindAsync(new object[] { id }, cancellationToken);
            if (weeklyPlan == null)
                return false;

            weeklyPlan.IsActive = false;
            weeklyPlan.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .AnyAsync(wp => wp.Id == id && wp.IsActive, cancellationToken);
        }

        public async Task<bool> ExistsForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .AnyAsync(wp => wp.Id == id && wp.EmployeeId == employeeId && wp.IsActive, cancellationToken);
        }

        public async Task<bool> HasPlanForWeekAsync(string employeeId, DateOnly weekStartDate, int? excludePlanId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.WeeklyPlans
                .Where(wp => wp.EmployeeId == employeeId 
                    && wp.WeekStartDate == weekStartDate 
                    && wp.IsActive);

            if (excludePlanId.HasValue)
                query = query.Where(wp => wp.Id != excludePlanId.Value);

            return await query.AnyAsync(cancellationToken);
        }
    }
}




