using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class RequestWorkflowService : IRequestWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RequestWorkflowService> _logger;

        public RequestWorkflowService(IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<RequestWorkflowService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<RequestWorkflowResponseDto> CreateRequestWorkflowAsync(string userId, CreateWorkflowRequestDto request, CancellationToken cancellationToken = default)
        {
            // Validate that the activity log exists and belongs to the user
            var activityLog = await _unitOfWork.ActivityLogs.GetByIdAsync(request.ActivityLogId, cancellationToken);
            if (activityLog == null || activityLog.UserId != userId)
            {
                throw new ArgumentException("Activity log not found or does not belong to the user");
            }

            // Determine the target role based on whether it's an offer or deal
            string targetRole;
            if (request.OfferId.HasValue)
            {
                targetRole = "SalesSupport";
            }
            else if (request.DealId.HasValue)
            {
                targetRole = "LegalManager";
            }
            else
            {
                throw new ArgumentException("Either OfferId or DealId must be provided");
            }

            // Get a user in the target role
            var targetUsers = await _unitOfWork.Users.GetUsersInRoleAsync(targetRole, cancellationToken);
            if (!targetUsers.Any())
            {
                throw new ArgumentException($"No users found with role {targetRole}");
            }

            var targetUser = targetUsers.First();

            var workflow = new RequestWorkflow
            {
                ActivityLogId = request.ActivityLogId,
                OfferId = request.OfferId,
                DealId = request.DealId,
                FromUserId = userId,
                ToUserId = request.ToUserId ?? targetUser.Id,
                Status = RequestStatus.Pending,
                Comment = request.Comment,
                DeliveryTermsId = request.DeliveryTermsId,
                PaymentTermsId = request.PaymentTermsId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RequestWorkflows.CreateAsync(workflow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification
            var message = request.OfferId.HasValue 
                ? "New offer request received" 
                : "New deal request received";
            
            await _notificationService.SendRequestNotificationAsync(
                workflow.FromUserId,
                "SalesSupport",
                request.OfferId.HasValue ? "Offer" : "Deal",
                request.ActivityLogId,
                request.OfferId,
                request.DealId,
                "Client Name", // You might want to get this from the activity log
                "Client Address", // You might want to get this from the activity log
                "Equipment Details", // You might want to get this from the activity log
                (int?)request.DeliveryTermsId,
                (int?)request.PaymentTermsId,
                cancellationToken);

            return new RequestWorkflowResponseDto
            {
                Id = workflow.Id,
                ActivityLogId = workflow.ActivityLogId,
                OfferId = workflow.OfferId,
                DealId = workflow.DealId,
                FromUserId = workflow.FromUserId,
                FromUserName = "Current User", // You might want to fetch the actual name
                ToUserId = workflow.ToUserId,
                ToUserName = targetUser.UserName ?? "Unknown",
                Status = workflow.Status,
                StatusName = workflow.Status.ToString(),
                Comment = workflow.Comment,
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt
            };
        }

        public async Task<IEnumerable<RequestWorkflowResponseDto>> GetSentRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            var requests = await _unitOfWork.RequestWorkflows.GetRequestsByUserIdAsync(userId, status, cancellationToken);
            
            return requests.Select(r => new RequestWorkflowResponseDto
            {
                Id = r.Id,
                ActivityLogId = r.ActivityLogId,
                OfferId = r.OfferId,
                DealId = r.DealId,
                FromUserId = r.FromUserId,
                FromUserName = r.FromUser?.UserName ?? "Unknown",
                ToUserId = r.ToUserId,
                ToUserName = r.ToUser?.UserName ?? "Unknown",
                Status = r.Status,
                StatusName = r.Status.ToString(),
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }

        public async Task<IEnumerable<RequestWorkflowResponseDto>> GetAssignedRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var requests = await _unitOfWork.RequestWorkflows.GetAssignedRequestsAsync(userId, status, cancellationToken);
                
                return requests.Select(r => new RequestWorkflowResponseDto
                {
                    Id = r.Id,
                    ActivityLogId = r.ActivityLogId,
                    OfferId = r.OfferId,
                    DealId = r.DealId,
                    FromUserId = r.FromUserId,
                    FromUserName = r.FromUser?.UserName ?? "Unknown",
                    ToUserId = r.ToUserId,
                    ToUserName = r.ToUser?.UserName ?? "Unknown",
                    Status = r.Status,
                    StatusName = r.Status.ToString(),
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                // Log the actual exception for debugging
                throw new Exception($"Error in GetAssignedRequestsAsync: {ex.Message}", ex);
            }
        }

        public async Task<RequestWorkflowResponseDto?> UpdateWorkflowStatusAsync(long workflowId, string userId, UpdateWorkflowRequestStatusDto updateDto, CancellationToken cancellationToken = default)
        {
            var workflow = await _unitOfWork.RequestWorkflows.GetByIdAsync(workflowId, cancellationToken);
            if (workflow == null || workflow.ToUserId != userId)
            {
                return null;
            }

            workflow.Status = updateDto.Status;
            workflow.Comment = updateDto.Comment;
            workflow.UpdatedAt = DateTime.UtcNow;

            if (updateDto.Status == RequestStatus.Completed)
            {
                workflow.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.RequestWorkflows.UpdateAsync(workflow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send notification to the original requester
            await _notificationService.SendStatusUpdateNotificationAsync(
                workflow.Id,
                updateDto.Status.ToString(),
                updateDto.Comment,
                cancellationToken);

            return new RequestWorkflowResponseDto
            {
                Id = workflow.Id,
                ActivityLogId = workflow.ActivityLogId,
                OfferId = workflow.OfferId,
                DealId = workflow.DealId,
                FromUserId = workflow.FromUserId,
                FromUserName = workflow.FromUser?.UserName ?? "Unknown",
                ToUserId = workflow.ToUserId,
                ToUserName = workflow.ToUser?.UserName ?? "Unknown",
                Status = workflow.Status,
                StatusName = workflow.Status.ToString(),
                Comment = workflow.Comment,
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt
            };
        }
    }
}
