using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface ISalesReportService
    {
        Task<SalesReportResponseDto?> CreateReportAsync(CreateSalesReportDto createDto, string employeeId, CancellationToken cancellationToken = default);
        Task<SalesReportResponseDto?> UpdateReportAsync(int id, UpdateSalesReportDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteReportAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<SalesReportResponseDto?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<SalesReportResponseDto?> GetReportByIdForEmployeeAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<PaginatedSalesReportsResponseDto> GetReportsAsync(FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default);
        Task<PaginatedSalesReportsResponseDto> GetReportsForEmployeeAsync(string employeeId, FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default);
        Task<SalesReportResponseDto?> RateReportAsync(int id, RateSalesReportDto rateDto, CancellationToken cancellationToken = default);
        Task<bool> CanAccessReportAsync(int reportId, string userId, bool isManager, CancellationToken cancellationToken = default);
    }
}

