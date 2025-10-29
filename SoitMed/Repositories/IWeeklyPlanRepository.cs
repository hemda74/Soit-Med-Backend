using SoitMed.Models;
<<<<<<< HEAD
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanRepository
    {
        Task<WeeklyPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<WeeklyPlan?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<WeeklyPlan?> GetByIdAndEmployeeIdAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlan>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlan>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<WeeklyPlan> Plans, int TotalCount)> GetPaginatedAsync(
            Expression<Func<WeeklyPlan, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
        Task<WeeklyPlan> CreateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default);
        Task<WeeklyPlan> UpdateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> HasPlanForWeekAsync(string employeeId, DateOnly weekStartDate, int? excludePlanId = null, CancellationToken cancellationToken = default);
    }
}




=======

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanRepository : IBaseRepository<WeeklyPlan>
    {
        Task<IEnumerable<WeeklyPlan>> GetEmployeePlansAsync(string employeeId, int page = 1, int pageSize = 20);
        Task<IEnumerable<WeeklyPlan>> GetAllPlansAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<WeeklyPlan>> GetAllPlansWithFiltersAsync(string? employeeId, DateTime? weekStartDate, DateTime? weekEndDate, bool? isViewed, int page = 1, int pageSize = 20);
        Task<int> CountAllPlansWithFiltersAsync(string? employeeId, DateTime? weekStartDate, DateTime? weekEndDate, bool? isViewed);
        Task<WeeklyPlan?> GetCurrentWeekPlanAsync(string employeeId);
        Task<WeeklyPlan?> GetPlanByWeekAsync(string employeeId, DateTime weekStart);
        Task<IEnumerable<WeeklyPlan>> GetPendingApprovalPlansAsync(int page = 1, int pageSize = 20);
        Task<bool> HasPlanForWeekAsync(string employeeId, DateTime weekStart);
        Task<WeeklyPlan?> GetPlanWithFullDetailsAsync(long id);
    }
}
>>>>>>> dev
