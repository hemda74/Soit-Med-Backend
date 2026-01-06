using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Contract;

namespace SoitMed.Repositories
{
    public class ContractRepository : IContractRepository
    {
        private readonly Context _context;

        public ContractRepository(Context context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(long id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Deal)
                .Include(c => c.Drafter)
                .Include(c => c.CustomerSigner)
                .Include(c => c.InstallmentSchedules)
                .Include(c => c.Negotiations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Deal)
                .ToListAsync();
        }

        public async Task<Contract> CreateAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            return await Task.FromResult(contract);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return false;

            _context.Contracts.Remove(contract);
            return true;
        }

        public IQueryable<Contract> GetQueryable()
        {
            return _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Deal)
                .Include(c => c.Drafter)
                .Include(c => c.InstallmentSchedules)
                .AsQueryable();
        }
    }
}

