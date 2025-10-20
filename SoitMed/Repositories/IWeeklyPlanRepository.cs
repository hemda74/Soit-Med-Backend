using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanRepository : IBaseRepository<WeeklyPlan>
    {
        Task<IEnumerable<WeeklyPlan>> GetEmployeePlansAsync(string employeeId, int page = 1, int pageSize = 20);
        Task<WeeklyPlan?> GetCurrentWeekPlanAsync(string employeeId);
        Task<WeeklyPlan?> GetPlanByWeekAsync(string employeeId, DateTime weekStart);
        Task<IEnumerable<WeeklyPlan>> GetPendingApprovalPlansAsync(int page = 1, int pageSize = 20);
        Task<bool> HasPlanForWeekAsync(string employeeId, DateTime weekStart);
    }
}