using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class SalesManTargetRepository : BaseRepository<SalesManTarget>, ISalesManTargetRepository
    {
        public SalesManTargetRepository(Context context) : base(context)
        {
        }

        public async Task<List<SalesManTarget>> GetTargetsBySalesManAsync(string salesmanId, int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.SalesManId == salesmanId && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.ToListAsync();
        }

        public async Task<SalesManTarget?> GetTeamTargetAsync(int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.IsTeamTarget && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<SalesManTarget>> GetAllTargetsForPeriodAsync(int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.ToListAsync();
        }

        public async Task<SalesManTarget?> GetTargetBySalesManAndPeriodAsync(string? salesmanId, int year, int? quarter = null)
        {
            var query = _dbSet.Where(t => t.SalesManId == salesmanId && t.Year == year);

            if (quarter.HasValue)
            {
                query = query.Where(t => t.Quarter == quarter);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}

