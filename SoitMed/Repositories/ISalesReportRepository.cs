using SoitMed.Models;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface ISalesReportRepository
    {
        Task<SalesReport?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReport?> GetByIdAndEmployeeIdAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SalesReport>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<SalesReport>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SalesReport>> GetFilteredAsync(Expression<Func<SalesReport, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(IEnumerable<SalesReport> Reports, int TotalCount)> GetPaginatedAsync(
            Expression<Func<SalesReport, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
        Task<SalesReport> CreateAsync(SalesReport salesReport, CancellationToken cancellationToken = default);
        Task<SalesReport> UpdateAsync(SalesReport salesReport, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        IQueryable<SalesReport> GetQueryable();
    }
}

