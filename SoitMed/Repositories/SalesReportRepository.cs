using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public class SalesReportRepository : ISalesReportRepository
    {
        private readonly Context _context;

        public SalesReportRepository(Context context)
        {
            _context = context;
        }

        public async Task<SalesReport?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .Include(sr => sr.Employee)
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.IsActive, cancellationToken);
        }

        public async Task<SalesReport?> GetByIdAndEmployeeIdAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .Include(sr => sr.Employee)
                .FirstOrDefaultAsync(sr => sr.Id == id && sr.EmployeeId == employeeId && sr.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<SalesReport>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .Include(sr => sr.Employee)
                .Where(sr => sr.IsActive)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SalesReport>> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .Include(sr => sr.Employee)
                .Where(sr => sr.EmployeeId == employeeId && sr.IsActive)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SalesReport>> GetFilteredAsync(Expression<Func<SalesReport, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .Include(sr => sr.Employee)
                .Where(sr => sr.IsActive)
                .Where(predicate)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<SalesReport> Reports, int TotalCount)> GetPaginatedAsync(
            Expression<Func<SalesReport, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SalesReports
                .Include(sr => sr.Employee)
                .Where(sr => sr.IsActive);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var reports = await query
                .OrderByDescending(sr => sr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (reports, totalCount);
        }

        public async Task<SalesReport> CreateAsync(SalesReport salesReport, CancellationToken cancellationToken = default)
        {
            _context.SalesReports.Add(salesReport);
            await _context.SaveChangesAsync(cancellationToken);
            return salesReport;
        }

        public async Task<SalesReport> UpdateAsync(SalesReport salesReport, CancellationToken cancellationToken = default)
        {
            salesReport.UpdatedAt = DateTime.UtcNow;
            _context.SalesReports.Update(salesReport);
            await _context.SaveChangesAsync(cancellationToken);
            return salesReport;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var salesReport = await _context.SalesReports.FindAsync(new object[] { id }, cancellationToken);
            if (salesReport == null)
                return false;

            salesReport.IsActive = false;
            salesReport.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .AnyAsync(sr => sr.Id == id && sr.IsActive, cancellationToken);
        }

        public async Task<bool> ExistsForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _context.SalesReports
                .AnyAsync(sr => sr.Id == id && sr.EmployeeId == employeeId && sr.IsActive, cancellationToken);
        }

        public IQueryable<SalesReport> GetQueryable()
        {
            return _context.SalesReports
                .Include(sr => sr.Employee)
                .Where(sr => sr.IsActive);
        }
    }
}

