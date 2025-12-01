using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

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
            var now = DateTime.UtcNow.Date;
            
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
