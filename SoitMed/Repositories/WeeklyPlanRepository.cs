using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
<<<<<<< HEAD
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




=======

namespace SoitMed.Repositories
{
    public class WeeklyPlanRepository : BaseRepository<WeeklyPlan>, IWeeklyPlanRepository
    {
        public WeeklyPlanRepository(Context context) : base(context)
        {
        }

        public override async Task<WeeklyPlan?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == (long)id, cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlan>> GetAllPlansAsync(int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Client)
                .Include(p => p.Employee)  // Include Employee user data
                .OrderByDescending(p => p.WeekStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<WeeklyPlan>> GetAllPlansWithFiltersAsync(string? employeeId, DateTime? weekStartDate, DateTime? weekEndDate, bool? isViewed, int page = 1, int pageSize = 20)
        {
            var query = _context.WeeklyPlans
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Client)
                .Include(p => p.Employee)
                .AsQueryable();

            // Filter by employee (salesman)
            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(p => p.EmployeeId == employeeId);
            }

            // Filter by week start date
            if (weekStartDate.HasValue)
            {
                query = query.Where(p => p.WeekStartDate >= weekStartDate.Value);
            }

            // Filter by week end date
            if (weekEndDate.HasValue)
            {
                query = query.Where(p => p.WeekEndDate <= weekEndDate.Value);
            }

            // Filter by viewed status
            if (isViewed.HasValue)
            {
                if (isViewed.Value)
                {
                    query = query.Where(p => p.ManagerViewedAt.HasValue);
                }
                else
                {
                    query = query.Where(p => !p.ManagerViewedAt.HasValue);
                }
            }

            return await query
                .OrderByDescending(p => p.WeekStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAllPlansWithFiltersAsync(string? employeeId, DateTime? weekStartDate, DateTime? weekEndDate, bool? isViewed)
        {
            var query = _context.WeeklyPlans.AsQueryable();

            // Filter by employee (salesman)
            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(p => p.EmployeeId == employeeId);
            }

            // Filter by week start date
            if (weekStartDate.HasValue)
            {
                query = query.Where(p => p.WeekStartDate >= weekStartDate.Value);
            }

            // Filter by week end date
            if (weekEndDate.HasValue)
            {
                query = query.Where(p => p.WeekEndDate <= weekEndDate.Value);
            }

            // Filter by viewed status
            if (isViewed.HasValue)
            {
                if (isViewed.Value)
                {
                    query = query.Where(p => p.ManagerViewedAt.HasValue);
                }
                else
                {
                    query = query.Where(p => !p.ManagerViewedAt.HasValue);
                }
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<WeeklyPlan>> GetEmployeePlansAsync(string employeeId, int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Client)
                .OrderByDescending(p => p.WeekStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WeeklyPlan?> GetCurrentWeekPlanAsync(string employeeId)
        {
            var now = DateTime.UtcNow;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStart)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync();
        }

        public async Task<WeeklyPlan?> GetPlanByWeekAsync(string employeeId, DateTime weekStart)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStart)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WeeklyPlan>> GetPendingApprovalPlansAsync(int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Where(p => p.IsActive && p.ManagerReviewedAt == null)
                .Include(p => p.Tasks)
                .OrderBy(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> HasPlanForWeekAsync(string employeeId, DateTime weekStart)
        {
            return await _context.WeeklyPlans
                .AnyAsync(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStart);
        }

        public async Task<WeeklyPlan?> GetPlanWithFullDetailsAsync(long id)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Client)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
>>>>>>> dev
