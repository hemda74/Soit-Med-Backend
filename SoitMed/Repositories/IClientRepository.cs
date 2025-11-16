using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IClientRepository : IBaseRepository<Client>
    {
        Task<IEnumerable<Client>> SearchClientsAsync(string query, string? classification = null, int page = 1, int pageSize = 20);
        Task<Client?> FindByNameAsync(string name);
        Task<Client?> FindByNameAndOrganizationAsync(string name, string? organizationName);
        Task<IEnumerable<Client>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20);
        Task<Client?> FindOrCreateClientAsync(string name, string? organizationName, string? phone, string createdBy);
        Task<object> GetClientStatisticsAsync(string userId);
        Task<List<Client>> GetByIdsAsync(IEnumerable<long> ids);
    }
}