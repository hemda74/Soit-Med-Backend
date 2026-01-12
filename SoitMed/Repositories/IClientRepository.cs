using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<IEnumerable<Client>> SearchClientsAsync(SoitMed.DTO.SearchClientDTO searchDto);
        Task<Client?> FindByNameAsync(string name);
        Task<Client?> FindByNameAndTypeAsync(string name, string type);
        Task<IEnumerable<Client>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Client>> GetClientsNeedingFollowUpAsync(string userId);
        Task<Client?> FindOrCreateClientAsync(string name, string type, string? specialization, string createdBy);
        Task<object> GetClientStatisticsAsync(string userId);
        Task<List<Client>> GetByIdsAsync(IEnumerable<string> ids);
        Task<(IEnumerable<Client> Clients, int TotalCount)> GetAllClientsAsync(int pageNumber = 1, int pageSize = 25, string? searchTerm = null);
    }
}