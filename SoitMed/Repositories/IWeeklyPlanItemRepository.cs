using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanItemRepository : IBaseRepository<WeeklyPlanItem>
    {
        Task<IEnumerable<WeeklyPlanItem>> GetPlanItemsAsync(long weeklyPlanId);
        Task<IEnumerable<WeeklyPlanItem>> GetOverdueItemsAsync(string employeeId);
        Task<IEnumerable<WeeklyPlanItem>> GetUpcomingItemsAsync(string employeeId, int days = 7);
        Task<IEnumerable<WeeklyPlanItem>> GetItemsByStatusAsync(long weeklyPlanId, string status);
        Task<IEnumerable<WeeklyPlanItem>> GetItemsByPriorityAsync(long weeklyPlanId, string priority);
    }
}
