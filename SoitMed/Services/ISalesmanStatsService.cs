using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface ISalesmanStatsService
    {
        Task<SalesmanStatsResponseDto> GetSalesmanStatisticsAsync(string salesmanId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<SalesmanStatsResponseDto> GetSalesmanCurrentWeekStatsAsync(string salesmanId, CancellationToken cancellationToken = default);
        Task<SalesmanStatsResponseDto> GetSalesmanCurrentMonthStatsAsync(string salesmanId, CancellationToken cancellationToken = default);
    }
}
