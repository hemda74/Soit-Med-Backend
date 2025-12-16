using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for sales deal repository
    /// </summary>
    public interface ISalesDealRepository : IBaseRepository<SalesDeal>
    {
        Task<List<SalesDeal>> GetDealsByClientIdAsync(long clientId);
        Task<List<SalesDeal>> GetDealsBySalesManAsync(string salesmanId);
        Task<List<SalesDeal>> GetDealsByStatusAsync(string status);
        Task<List<SalesDeal>> GetPendingApprovalsForManagerAsync();
        Task<List<SalesDeal>> GetPendingApprovalsForSuperAdminAsync();
        Task<List<SalesDeal>> GetDealsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesDeal>> GetDealsByValueRangeAsync(decimal minValue, decimal maxValue);
        Task<SalesDeal?> GetDealWithDetailsAsync(long dealId);
        Task<List<SalesDeal>> GetDealsByManagerAsync(string managerId);
        Task<List<SalesDeal>> GetDealsBySuperAdminAsync(string superAdminId);
        Task<List<SalesDeal>> GetDealsNeedingLegalReviewAsync();
        Task<int> GetDealCountByStatusAsync(string status);
        Task<decimal> GetTotalDealValueByStatusAsync(string status);
        Task<decimal> GetTotalDealValueBySalesManAsync(string salesmanId);
        Task<List<SalesDeal>> GetSuccessfulDealsAsync();
        Task<List<SalesDeal>> GetFailedDealsAsync();
        Task<List<SalesDeal>> GetDealsByRejectionReasonAsync(string rejectionReason);
        Task<List<SalesDeal>> GetByIdsAsync(IEnumerable<long> ids);
    }
}



