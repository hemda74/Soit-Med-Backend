using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IRequestWorkflowRepository : IBaseRepository<RequestWorkflow>
    {
        Task<IEnumerable<RequestWorkflow>> GetRequestsByRoleAsync(string role, RequestStatus? status = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<RequestWorkflow>> GetRequestsByUserIdAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<RequestWorkflow>> GetAssignedRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default);
        Task<RequestWorkflow?> GetRequestWithDetailsAsync(long id, CancellationToken cancellationToken = default);
    }
}
