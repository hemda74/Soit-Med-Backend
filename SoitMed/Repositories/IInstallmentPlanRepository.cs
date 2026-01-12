using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IInstallmentPlanRepository : IBaseRepository<InstallmentPlan>
    {
        Task<IEnumerable<InstallmentPlan>> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default);
    }
}



