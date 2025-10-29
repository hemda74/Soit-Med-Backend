using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class SalesmanTargetRepository : BaseRepository<SalesmanTarget>, ISalesmanTargetRepository
    {
        public SalesmanTargetRepository(Context context) : base(context)
        {
        }

        public async Task<List<SalesmanTarget>> GetTargetsBySalesmanAsync(string salesmanId, int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.SalesmanId == salesmanId && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.ToListAsync();
        }

        public async Task<SalesmanTarget?> GetTeamTargetAsync(int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.IsTeamTarget && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<SalesmanTarget>> GetAllTargetsForPeriodAsync(int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.ToListAsync();
        }

        public async Task<SalesmanTarget?> GetTargetBySalesmanAndPeriodAsync(string? salesmanId, int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.SalesmanId == salesmanId && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}

