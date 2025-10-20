using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class WeeklyPlanItemRepository : BaseRepository<WeeklyPlanItem>, IWeeklyPlanItemRepository
    {
        public WeeklyPlanItemRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<WeeklyPlanItem>> GetPlanItemsAsync(long weeklyPlanId)
        {
            return await _context.WeeklyPlanItems
                .Where(i => i.WeeklyPlanId == weeklyPlanId)
                .OrderBy(i => i.PlannedVisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WeeklyPlanItem>> GetOverdueItemsAsync(string employeeId)
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.WeeklyPlanItems
                .Where(i => i.WeeklyPlan.EmployeeId == employeeId && 
                           i.PlannedVisitDate.Date < today && 
                           i.Status == "Planned")
                .OrderBy(i => i.PlannedVisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WeeklyPlanItem>> GetUpcomingItemsAsync(string employeeId, int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);
            
            return await _context.WeeklyPlanItems
                .Where(i => i.WeeklyPlan.EmployeeId == employeeId && 
                           i.PlannedVisitDate.Date >= today && 
                           i.PlannedVisitDate.Date <= futureDate &&
                           i.Status == "Planned")
                .OrderBy(i => i.PlannedVisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WeeklyPlanItem>> GetItemsByStatusAsync(long weeklyPlanId, string status)
        {
            return await _context.WeeklyPlanItems
                .Where(i => i.WeeklyPlanId == weeklyPlanId && i.Status == status)
                .OrderBy(i => i.PlannedVisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<WeeklyPlanItem>> GetItemsByPriorityAsync(long weeklyPlanId, string priority)
        {
            return await _context.WeeklyPlanItems
                .Where(i => i.WeeklyPlanId == weeklyPlanId && i.Priority == priority)
                .OrderBy(i => i.PlannedVisitDate)
                .ToListAsync();
        }
    }
}
