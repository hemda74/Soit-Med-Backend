using SoitMed.DTO;
using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    public interface IRequestWorkflowService
    {
        Task<RequestWorkflowResponseDto> CreateRequestWorkflowAsync(string userId, CreateWorkflowRequestDto request, CancellationToken cancellationToken = default);
        Task<IEnumerable<RequestWorkflowResponseDto>> GetSentRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<RequestWorkflowResponseDto>> GetAssignedRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default);
        Task<RequestWorkflowResponseDto?> UpdateWorkflowStatusAsync(long workflowId, string userId, UpdateWorkflowRequestStatusDto updateDto, CancellationToken cancellationToken = default);
    }
}
