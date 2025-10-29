using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SoitMed.Hubs;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<Notification> CreateNotificationAsync(string userId, string title, string message, string type, string? priority = null, long? requestWorkflowId = null, long? activityLogId = null, bool isMobilePush = false, CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Priority = priority ?? "Medium",
                RequestWorkflowId = requestWorkflowId,
                ActivityLogId = activityLogId,
                IsMobilePush = isMobilePush,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notification via SignalR
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Priority = notification.Priority,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RequestWorkflowId = notification.RequestWorkflowId,
                ActivityLogId = notification.ActivityLogId
            });

            _logger.LogInformation($"Notification sent to user {userId}: {title}");

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.GetContext().Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkNotificationAsReadAsync(long notificationId, string userId, CancellationToken cancellationToken = default)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken);
            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _unitOfWork.GetContext().Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            if (notifications.Any())
            {
                await _unitOfWork.Notifications.UpdateRangeAsync(notifications, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.GetContext().Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
        }

        public async Task SendRequestNotificationAsync(string fromUserId, string toRole, string requestType, long activityLogId, long? offerId, long? dealId, string clientName, string clientAddress, string equipmentDetails, int? deliveryTermsId, int? paymentTermsId, CancellationToken cancellationToken = default)
        {
            // Create request workflow record
            var requestWorkflow = new RequestWorkflow
            {
                ActivityLogId = activityLogId,
                OfferId = offerId,
                DealId = dealId,
                RequestType = requestType,
                FromRole = "Salesman",
                ToRole = toRole,
                FromUserId = fromUserId,
                Status = RequestStatus.Pending,
                ClientName = clientName,
                ClientAddress = clientAddress,
                EquipmentDetails = equipmentDetails,
                DeliveryTermsId = deliveryTermsId,
                PaymentTermsId = paymentTermsId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RequestWorkflows.CreateAsync(requestWorkflow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get users with the target role
            var targetUsers = await _unitOfWork.Users.GetUsersInRoleAsync(toRole, cancellationToken);

            // Send notifications to all users with the target role
            foreach (var user in targetUsers)
            {
                var title = $"New {requestType} Request";
                var message = $"New {requestType} request from {clientName} at {clientAddress}";
                
                await CreateNotificationAsync(
                    user.Id, 
                    title, 
                    message, 
                    "Request", 
                    "High", 
                    requestWorkflow.Id, 
                    activityLogId, 
                    true, // Mobile push for important requests
                    cancellationToken);
            }

            // Also send to role-based group
            await _hubContext.Clients.Group($"Role_{toRole}").SendAsync("NewRequest", new
            {
                RequestId = requestWorkflow.Id,
                RequestType = requestType,
                ClientName = clientName,
                ClientAddress = clientAddress,
                EquipmentDetails = equipmentDetails,
                CreatedAt = requestWorkflow.CreatedAt
            });
        }

        public async Task SendAssignmentNotificationAsync(string fromUserId, string toUserId, long requestWorkflowId, CancellationToken cancellationToken = default)
        {
            var requestWorkflow = await _unitOfWork.RequestWorkflows.GetByIdAsync(requestWorkflowId, cancellationToken);
            if (requestWorkflow != null)
            {
                requestWorkflow.ToUserId = toUserId;
                requestWorkflow.Status = RequestStatus.Assigned;
                requestWorkflow.AssignedAt = DateTime.UtcNow;
                requestWorkflow.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.RequestWorkflows.UpdateAsync(requestWorkflow, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var title = "Request Assigned to You";
                var message = $"A {requestWorkflow.RequestType} request has been assigned to you for {requestWorkflow.ClientName}";

                await CreateNotificationAsync(
                    toUserId,
                    title,
                    message,
                    "Assignment",
                    "High",
                    requestWorkflowId,
                    requestWorkflow.ActivityLogId,
                    true,
                    cancellationToken);
            }
        }

        public async Task SendStatusUpdateNotificationAsync(long requestWorkflowId, string status, string? comments = null, CancellationToken cancellationToken = default)
        {
            var requestWorkflow = await _unitOfWork.RequestWorkflows.GetByIdAsync(requestWorkflowId, cancellationToken);
            if (requestWorkflow != null)
            {
                requestWorkflow.Status = Enum.Parse<RequestStatus>(status);
                requestWorkflow.Comment = comments;
                requestWorkflow.UpdatedAt = DateTime.UtcNow;

                if (requestWorkflow.Status == RequestStatus.Completed)
                {
                    requestWorkflow.CompletedAt = DateTime.UtcNow;
                }

                await _unitOfWork.RequestWorkflows.UpdateAsync(requestWorkflow, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Notify the original requester
                var title = $"Request Status Updated";
                var message = $"Your {requestWorkflow.RequestType} request status has been updated to {status}";

                if (!string.IsNullOrEmpty(comments))
                {
                    message += $": {comments}";
                }

                await CreateNotificationAsync(
                    requestWorkflow.FromUserId,
                    title,
                    message,
                    "Update",
                    "Medium",
                    requestWorkflowId,
                    requestWorkflow.ActivityLogId,
                    false,
                    cancellationToken);
            }
        }
    }
}
