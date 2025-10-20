using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class WeeklyPlanRepository : BaseRepository<WeeklyPlan>, IWeeklyPlanRepository
    {
        public WeeklyPlanRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<WeeklyPlan>> GetEmployeePlansAsync(string employeeId, int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId)
                .Include(p => p.PlanItems)
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
                .Include(p => p.PlanItems)
                .FirstOrDefaultAsync();
        }

        public async Task<WeeklyPlan?> GetPlanByWeekAsync(string employeeId, DateTime weekStart)
        {
            return await _context.WeeklyPlans
                .Where(p => p.EmployeeId == employeeId && p.WeekStartDate == weekStart)
                .Include(p => p.PlanItems)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<WeeklyPlan>> GetPendingApprovalPlansAsync(int page = 1, int pageSize = 20)
        {
            return await _context.WeeklyPlans
                .Where(p => p.Status == "Submitted")
                .Include(p => p.PlanItems)
                .OrderBy(p => p.SubmittedAt)
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
