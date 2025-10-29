using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface ISalesmanTargetRepository : IBaseRepository<SalesmanTarget>
    {
        Task<List<SalesmanTarget>> GetTargetsBySalesmanAsync(string salesmanId, int year, int? quarter = null);
        Task<SalesmanTarget?> GetTeamTargetAsync(int year, int? quarter = null);
        Task<List<SalesmanTarget>> GetAllTargetsForPeriodAsync(int year, int? quarter = null);
        Task<SalesmanTarget?> GetTargetBySalesmanAndPeriodAsync(string? salesmanId, int year, int? quarter = null);
    }
}


