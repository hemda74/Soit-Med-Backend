using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using System.Linq.Expressions;

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

        // Explicit implementation for long parameter
        public async Task<WeeklyPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<WeeklyPlan?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Client)
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<WeeklyPlan?> GetByIdAndEmployeeIdAsync(long id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.EmployeeId == employeeId, cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlan>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId)
                .Include(p => p.Tasks)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<WeeklyPlan> Plans, int TotalCount)> GetPaginatedAsync(
            Expression<Func<WeeklyPlan, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.WeeklyPlans.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var plans = await query
                .Include(p => p.Tasks)
                .OrderByDescending(p => p.WeekStartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (plans, totalCount);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var plan = await _context.WeeklyPlans.FindAsync(new object[] { id }, cancellationToken);
            if (plan == null)
                return false;

            _context.WeeklyPlans.Remove(plan);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans.AnyAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsForEmployeeAsync(long id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlans.AnyAsync(p => p.Id == id && p.EmployeeId == employeeId, cancellationToken);
        }

        public async Task<bool> HasPlanForWeekAsync(string employeeId, DateOnly weekStartDate, long? excludePlanId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStartDate);

            if (excludePlanId.HasValue)
            {
                query = query.Where(p => p.Id != excludePlanId.Value);
            }

            return await query.AnyAsync(cancellationToken);
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
                var startDateOnly = DateOnly.FromDateTime(weekStartDate.Value);
                query = query.Where(p => p.WeekStartDate >= startDateOnly);
            }

            // Filter by week end date
            if (weekEndDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(weekEndDate.Value);
                query = query.Where(p => p.WeekEndDate <= endDateOnly);
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
                var startDateOnly = DateOnly.FromDateTime(weekStartDate.Value);
                query = query.Where(p => p.WeekStartDate >= startDateOnly);
            }

            // Filter by week end date
            if (weekEndDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(weekEndDate.Value);
                query = query.Where(p => p.WeekEndDate <= endDateOnly);
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
            var now = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            
            // OPTIMIZATION: Eagerly load Tasks and Progresses to avoid N+1 queries
            // Use AsNoTracking for read-only operations to improve performance
            // Check if current date falls within the week range (inclusive)
            return await _context.WeeklyPlans
                .AsNoTracking()
                .Where(p => p.EmployeeId == employeeId 
                    && p.WeekStartDate <= now 
                    && p.WeekEndDate >= now
                    && p.IsActive)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Progresses)
                .FirstOrDefaultAsync();
        }

        public async Task<WeeklyPlan?> GetPlanByWeekAsync(string employeeId, DateTime weekStart)
        {
            var weekStartDateOnly = DateOnly.FromDateTime(weekStart);
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStartDateOnly)
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
            var weekStartDateOnly = DateOnly.FromDateTime(weekStart);
            return await _context.WeeklyPlans
                .AnyAsync(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStartDateOnly);
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
