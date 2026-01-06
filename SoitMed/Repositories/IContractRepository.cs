using SoitMed.Models.Contract;

namespace SoitMed.Repositories
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(long id);
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<Contract> CreateAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(long id);
        IQueryable<Contract> GetQueryable();
    }
}

