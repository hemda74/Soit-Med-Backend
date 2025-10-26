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

        public async Task<IEnumerable<WeeklyPlan>> GetEmployeePlansAsync(string employeeId, int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId)
                .Include(p => p.Tasks)
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
    }
}
