using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface ISalesManTargetRepository : IBaseRepository<SalesManTarget>
    {
        Task<List<SalesManTarget>> GetTargetsBySalesManAsync(string salesmanId, int year, int? quarter = null);
        Task<SalesManTarget?> GetTeamTargetAsync(int year, int? quarter = null);
        Task<List<SalesManTarget>> GetAllTargetsForPeriodAsync(int year, int? quarter = null);
        Task<SalesManTarget?> GetTargetBySalesManAndPeriodAsync(string? salesmanId, int year, int? quarter = null);
    }
}


