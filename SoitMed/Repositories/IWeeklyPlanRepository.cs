using SoitMed.Models;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IWeeklyPlanRepository
    {
        Task<WeeklyPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<WeeklyPlan?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default);
        Task<WeeklyPlan?> GetByIdAndEmployeeIdAsync(long id, string employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlan>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WeeklyPlan>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<WeeklyPlan> Plans, int TotalCount)> GetPaginatedAsync(
            Expression<Func<WeeklyPlan, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
        Task<WeeklyPlan> CreateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default);
        Task<WeeklyPlan> UpdateAsync(WeeklyPlan weeklyPlan, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsForEmployeeAsync(long id, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> HasPlanForWeekAsync(string employeeId, DateOnly weekStartDate, long? excludePlanId = null, CancellationToken cancellationToken = default);
    }
}




