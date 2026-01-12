using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository for managing sales deals
    /// </summary>
    public class SalesDealRepository : BaseRepository<SalesDeal>, ISalesDealRepository
    {
        public SalesDealRepository(Context context) : base(context)
        {
        }

        // Override GetAllAsync to use AsNoTracking for better performance and to avoid relationship issues
        public override async Task<IEnumerable<SalesDeal>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SalesDeal>> GetDealsByClientIdAsync(long clientId)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.ClientId == clientId.ToString())
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsBySalesManAsync(string salesmanId)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.SalesManId == salesmanId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsByStatusAsync(string status)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetPendingApprovalsForManagerAsync()
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == "PendingManagerApproval")
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetPendingApprovalsForSuperAdminAsync()
        {
            // SuperAdmin can approve deals that are either:
            // 1. PendingSuperAdminApproval (new deals created after client response)
            // 2. PendingManagerApproval (legacy deals - SalesManager only approves offers, not deals)
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == "PendingSuperAdminApproval" || d.Status == "PendingManagerApproval")
                .OrderBy(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.ClosedDate >= startDate && d.ClosedDate <= endDate)
                .OrderByDescending(d => d.ClosedDate)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsByValueRangeAsync(decimal minValue, decimal maxValue)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.DealValue >= minValue && d.DealValue <= maxValue)
                .OrderByDescending(d => d.DealValue)
                .ToListAsync();
        }

        public async Task<SalesDeal?> GetDealWithDetailsAsync(long dealId)
        {
            return await _context.SalesDeals
                .Include(d => d.Client)
                .Include(d => d.SalesMan)
                .Include(d => d.Offer)
                .Include(d => d.ManagerApprover)
                .Include(d => d.SuperAdminApprover)
                .FirstOrDefaultAsync(d => d.Id == dealId.ToString());
        }

        public async Task<List<SalesDeal>> GetDealsByManagerAsync(string managerId)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.ManagerApprovedBy == managerId)
                .OrderByDescending(d => d.ManagerApprovedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsBySuperAdminAsync(string superAdminId)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.SuperAdminApprovedBy == superAdminId)
                .OrderByDescending(d => d.SuperAdminApprovedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsNeedingLegalReviewAsync()
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == "Approved" && !d.SentToLegalAt.HasValue)
                .OrderBy(d => d.SuperAdminApprovedAt)
                .ToListAsync();
        }

        public async Task<int> GetDealCountByStatusAsync(string status)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .CountAsync(d => d.Status == status);
        }

        public async Task<decimal> GetTotalDealValueByStatusAsync(string status)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == status)
                .SumAsync(d => d.DealValue);
        }

        public async Task<decimal> GetTotalDealValueBySalesManAsync(string salesmanId)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.SalesManId == salesmanId && d.Status == "Success")
                .SumAsync(d => d.DealValue);
        }

        public async Task<List<SalesDeal>> GetSuccessfulDealsAsync()
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == "Success")
                .OrderByDescending(d => d.CompletedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetFailedDealsAsync()
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.Status == "Failed")
                .OrderByDescending(d => d.CompletedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetDealsByRejectionReasonAsync(string rejectionReason)
        {
            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => d.ManagerRejectionReason == rejectionReason || d.SuperAdminRejectionReason == rejectionReason)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesDeal>> GetByIdsAsync(IEnumerable<long> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<SalesDeal>();

            return await _context.SalesDeals
                .AsNoTracking()
                .Where(d => idList.Contains(long.Parse(d.Id)))
                .ToListAsync();
        }
    }
}



