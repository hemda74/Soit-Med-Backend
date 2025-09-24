using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Linq.Expressions;

namespace SoitMed.Services
{
    public class SalesReportService : ISalesReportService
    {
        private readonly ISalesReportRepository _salesReportRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;

        public SalesReportService(
            ISalesReportRepository salesReportRepository,
            UserManager<ApplicationUser> userManager,
            Context context)
        {
            _salesReportRepository = salesReportRepository;
            _userManager = userManager;
            _context = context;
        }

        public async Task<SalesReportResponseDto?> CreateReportAsync(CreateSalesReportDto createDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify employee exists and has correct role
            var employee = await _userManager.FindByIdAsync(employeeId);
            if (employee == null)
                return null;

            var userRoles = await _userManager.GetRolesAsync(employee);
            if (!userRoles.Contains("Salesman"))
                return null;

            // Check for duplicate report (same employee, date, and type)
            var existingReport = await _context.SalesReports
                .FirstOrDefaultAsync(sr => sr.EmployeeId == employeeId 
                    && sr.ReportDate == createDto.ReportDate 
                    && sr.Type.ToLower() == createDto.Type.ToLower() 
                    && sr.IsActive, cancellationToken);

            if (existingReport != null)
                return null; // Duplicate report exists

            var salesReport = new SalesReport
            {
                Title = createDto.Title,
                Body = createDto.Body,
                Type = createDto.Type.ToLower(),
                ReportDate = createDto.ReportDate,
                EmployeeId = employeeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdReport = await _salesReportRepository.CreateAsync(salesReport, cancellationToken);
            return MapToResponseDto(createdReport);
        }

        public async Task<SalesReportResponseDto?> UpdateReportAsync(int id, UpdateSalesReportDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var existingReport = await _salesReportRepository.GetByIdAndEmployeeIdAsync(id, employeeId, cancellationToken);
            if (existingReport == null)
                return null;

            // Check for duplicate report (excluding current one)
            var duplicateReport = await _context.SalesReports
                .FirstOrDefaultAsync(sr => sr.EmployeeId == employeeId 
                    && sr.ReportDate == updateDto.ReportDate 
                    && sr.Type.ToLower() == updateDto.Type.ToLower() 
                    && sr.Id != id
                    && sr.IsActive, cancellationToken);

            if (duplicateReport != null)
                return null; // Duplicate report exists

            existingReport.Title = updateDto.Title;
            existingReport.Body = updateDto.Body;
            existingReport.Type = updateDto.Type.ToLower();
            existingReport.ReportDate = updateDto.ReportDate;
            existingReport.UpdatedAt = DateTime.UtcNow;

            var updatedReport = await _salesReportRepository.UpdateAsync(existingReport, cancellationToken);
            return MapToResponseDto(updatedReport);
        }

        public async Task<bool> DeleteReportAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            var exists = await _salesReportRepository.ExistsForEmployeeAsync(id, employeeId, cancellationToken);
            if (!exists)
                return false;

            return await _salesReportRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<SalesReportResponseDto?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var report = await _salesReportRepository.GetByIdAsync(id, cancellationToken);
            return report != null ? MapToResponseDto(report) : null;
        }

        public async Task<SalesReportResponseDto?> GetReportByIdForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            var report = await _salesReportRepository.GetByIdAndEmployeeIdAsync(id, employeeId, cancellationToken);
            return report != null ? MapToResponseDto(report) : null;
        }

        public async Task<PaginatedSalesReportsResponseDto> GetReportsAsync(FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default)
        {
            Expression<Func<SalesReport, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(filterDto.EmployeeId))
            {
                predicate = (Expression<Func<SalesReport, bool>>)(sr => sr.EmployeeId == filterDto.EmployeeId);
            }

            if (filterDto.StartDate.HasValue)
            {
                Expression<Func<SalesReport, bool>> startDatePredicate = sr => sr.ReportDate >= filterDto.StartDate.Value;
                predicate = predicate == null ? startDatePredicate : CombineExpressions(predicate, startDatePredicate);
            }

            if (filterDto.EndDate.HasValue)
            {
                Expression<Func<SalesReport, bool>> endDatePredicate = sr => sr.ReportDate <= filterDto.EndDate.Value;
                predicate = predicate == null ? endDatePredicate : CombineExpressions(predicate, endDatePredicate);
            }

            if (!string.IsNullOrEmpty(filterDto.Type))
            {
                Expression<Func<SalesReport, bool>> typePredicate = sr => sr.Type.ToLower() == filterDto.Type.ToLower();
                predicate = predicate == null ? typePredicate : CombineExpressions(predicate, typePredicate);
            }

            var (reports, totalCount) = await _salesReportRepository.GetPaginatedAsync(
                predicate, 
                filterDto.Page, 
                filterDto.PageSize, 
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize);

            return new PaginatedSalesReportsResponseDto
            {
                Data = reports.Select(MapToResponseDto).ToList(),
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = totalPages,
                HasNextPage = filterDto.Page < totalPages,
                HasPreviousPage = filterDto.Page > 1
            };
        }

        public async Task<PaginatedSalesReportsResponseDto> GetReportsForEmployeeAsync(string employeeId, FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default)
        {
            // Override employee filter to ensure employee can only see their own reports
            filterDto.EmployeeId = employeeId;
            return await GetReportsAsync(filterDto, cancellationToken);
        }

        public async Task<SalesReportResponseDto?> RateReportAsync(int id, RateSalesReportDto rateDto, CancellationToken cancellationToken = default)
        {
            var report = await _salesReportRepository.GetByIdAsync(id, cancellationToken);
            if (report == null)
                return null;

            // Only update if at least one field is provided
            if (rateDto.Rating.HasValue || !string.IsNullOrEmpty(rateDto.Comment))
            {
                if (rateDto.Rating.HasValue)
                    report.Rating = rateDto.Rating.Value;

                if (!string.IsNullOrEmpty(rateDto.Comment))
                    report.Comment = rateDto.Comment;

                report.UpdatedAt = DateTime.UtcNow;

                var updatedReport = await _salesReportRepository.UpdateAsync(report, cancellationToken);
                return MapToResponseDto(updatedReport);
            }

            return MapToResponseDto(report);
        }

        public async Task<bool> CanAccessReportAsync(int reportId, string userId, bool isManager, CancellationToken cancellationToken = default)
        {
            var report = await _salesReportRepository.GetByIdAsync(reportId, cancellationToken);
            if (report == null)
                return false;

            // Managers can access all reports, employees can only access their own
            return isManager || report.EmployeeId == userId;
        }

        private static SalesReportResponseDto MapToResponseDto(SalesReport report)
        {
            return new SalesReportResponseDto
            {
                Id = report.Id,
                Title = report.Title,
                Body = report.Body,
                Type = report.Type,
                ReportDate = report.ReportDate,
                EmployeeId = report.EmployeeId,
                EmployeeName = $"{report.Employee.FirstName} {report.Employee.LastName}".Trim(),
                Rating = report.Rating,
                Comment = report.Comment,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                IsActive = report.IsActive
            };
        }

        private static Expression<Func<SalesReport, bool>> CombineExpressions(
            Expression<Func<SalesReport, bool>> left,
            Expression<Func<SalesReport, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(SalesReport), "sr");
            var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
            var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
            var combinedBody = Expression.AndAlso(leftBody, rightBody);
            return Expression.Lambda<Func<SalesReport, bool>>(combinedBody, parameter);
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }
}
