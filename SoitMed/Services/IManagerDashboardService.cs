using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IManagerDashboardService
    {
        Task<DashboardStatsResponseDto> GetDashboardStatisticsAsync(string managerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetSubordinateUserIdsAsync(string managerId, CancellationToken cancellationToken = default);
    }
}
