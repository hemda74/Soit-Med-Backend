using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing offer requests in the sales workflow
    /// </summary>
    public class OfferRequestService : IOfferRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OfferRequestService> _logger;

        public OfferRequestService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<OfferRequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        #region Offer Request Management

        public async Task<OfferRequestResponseDTO> CreateOfferRequestAsync(CreateOfferRequestDTO createDto, string userId)
        {
            try
            {
                // Validate client exists
                var client = await _unitOfWork.Clients.GetByIdAsync(createDto.ClientId);
                if (client == null)
                    throw new ArgumentException("Client not found", nameof(createDto.ClientId));

                // Validate task progress if provided
                if (createDto.TaskProgressId.HasValue)
                {
                    var taskProgress = await _unitOfWork.TaskProgresses.GetByIdAsync(createDto.TaskProgressId.Value);
                    if (taskProgress == null)
                        throw new ArgumentException("Task progress not found", nameof(createDto.TaskProgressId));
                }

                // Auto-assign to SalesSupport user(s)
                var salesSupportUsers = await _userManager.GetUsersInRoleAsync("SalesSupport");
                var salesSupportUserList = salesSupportUsers.Where(u => u.IsActive).ToList();
                
                string? assignedToSupportId = null;
                if (salesSupportUserList.Any())
                {
                    // If only one SalesSupport user, assign to that user
                    // If multiple, assign to the first one
                    assignedToSupportId = salesSupportUserList.First().Id;
                }

                var offerRequest = new OfferRequest
                {
                    RequestedBy = userId,
                    ClientId = createDto.ClientId,
                    TaskProgressId = createDto.TaskProgressId,
                    RequestedProducts = createDto.RequestedProducts,
                    SpecialNotes = createDto.SpecialNotes,
                    RequestDate = DateTime.UtcNow,
                    Status = assignedToSupportId != null ? "Assigned" : "Requested", // Auto-assigned if SalesSupport exists
                    AssignedTo = assignedToSupportId // Auto-assign to SalesSupport
                };

                await _unitOfWork.OfferRequests.CreateAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer request created successfully. RequestId: {RequestId}, ClientId: {ClientId}, AssignedTo: {AssignedTo}", 
                    offerRequest.Id, createDto.ClientId, assignedToSupportId ?? "None");

                // Send notification to assigned SalesSupport user ONLY (not all SalesSupport users)
                try
                {
                    _logger.LogInformation("Starting notification process for assigned SalesSupport user");
                    
                    var salesman = await _unitOfWork.Users.GetByIdAsync(userId);
                    var salesmanName = salesman != null ? $"{salesman.FirstName} {salesman.LastName}" : "Salesman";
                    
                    var notificationTitle = "New Offer Request";
                    var notificationMessage = $"{salesmanName} requested an offer for client: {client.Name}";
                    
                    // Prepare metadata with offer request details
                    var metadata = new Dictionary<string, object>
                    {
                        ["offerRequestId"] = offerRequest.Id,
                        ["clientId"] = client.Id,
                        ["clientName"] = client.Name,
                        ["salesmanId"] = userId,
                        ["salesmanName"] = salesmanName
                    };
                    
                    // Send notification ONLY to the assigned SalesSupport user (not all)
                    if (!string.IsNullOrEmpty(assignedToSupportId))
                    {
                        var assignedUser = salesSupportUserList.FirstOrDefault(u => u.Id == assignedToSupportId);
                        if (assignedUser != null)
                        {
                            _logger.LogInformation("Sending notification to assigned SalesSupport user: {SupportUserId} (Name: {Name}, Email: {Email}) for OfferRequest: {RequestId}", 
                                assignedUser.Id, assignedUser.FullName, assignedUser.Email, offerRequest.Id);
                            
                            try
                            {
                                var notification = await _notificationService.CreateNotificationAsync(
                                    assignedUser.Id,
                                    notificationTitle,
                                    notificationMessage,
                                    "OfferRequest",
                                    "High", // High priority for new offer requests
                                    null,
                                    null,
                                    true, // Mobile push notification
                                    metadata, // Pass metadata with offerRequestId
                                    CancellationToken.None
                                );
                                
                                _logger.LogInformation("✅ Notification successfully created and sent to assigned SalesSupport: {SupportUserId} (NotificationId: {NotificationId}) for OfferRequest: {RequestId}", 
                                    assignedUser.Id, notification.Id, offerRequest.Id);
                            }
                            catch (Exception notificationEx)
                            {
                                _logger.LogError(notificationEx, "❌ Failed to create notification for assigned SalesSupport user {SupportUserId} for OfferRequest: {RequestId}", 
                                    assignedUser.Id, offerRequest.Id);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No SalesSupport user was assigned. Offer request {RequestId} was created but no notification sent.", offerRequest.Id);
                    }

                    if (!salesSupportUserList.Any())
                    {
                        _logger.LogWarning("⚠️ No active SalesSupport users found in the system.");
                    }
                    else
                    {
                        _logger.LogInformation("Notification process completed. Attempted to notify {Count} SalesSupport user(s)", salesSupportUserList.Count);
                    }
                }
                catch (Exception notifEx)
                {
                    // Log but don't fail the offer request creation
                    _logger.LogError(notifEx, "❌ Critical error in notification process for offer request {RequestId}", offerRequest.Id);
                }

                return await MapToResponseDTO(offerRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer request");
                throw;
            }
        }

        public async Task<OfferRequestResponseDTO?> GetOfferRequestAsync(long requestId, string userId, string userRole)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (offerRequest == null)
                    return null;

                // Check authorization
                if (userRole != "SuperAdmin" && offerRequest.RequestedBy != userId && offerRequest.AssignedTo != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view this offer request");

                return await MapToResponseDTO(offerRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer request");
                throw;
            }
        }

        public async Task<List<OfferRequestResponseDTO>> GetOfferRequestsAsync(string? status, string? requestedBy, string userId, string userRole)
        {
            try
            {
                var query = _unitOfWork.OfferRequests.GetQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(or => or.Status == status);

                if (!string.IsNullOrEmpty(requestedBy))
                    query = query.Where(or => or.RequestedBy == requestedBy);

                // Apply authorization
                if (userRole == "SalesSupport")
                {
                    query = query.Where(or => or.AssignedTo == userId);
                }
                else if (userRole == "Salesman" || userRole == "Customer")
                {
                    query = query.Where(or => or.RequestedBy == userId);
                }
                // SuperAdmin and SalesManager can see all

                var offerRequests = await query.OrderByDescending(or => or.CreatedAt).ToListAsync();
                var result = new List<OfferRequestResponseDTO>();

                foreach (var offerRequest in offerRequests)
                {
                    result.Add(await MapToResponseDTO(offerRequest));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer requests");
                throw;
            }
        }

        public async Task<List<OfferRequestResponseDTO>> GetOfferRequestsBySalesmanAsync(string salesmanId, string? status)
        {
            try
            {
                var query = _unitOfWork.OfferRequests.GetQueryable().Where(or => or.RequestedBy == salesmanId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(or => or.Status == status);

                var offerRequests = await query.OrderByDescending(or => or.CreatedAt).ToListAsync();
                var result = new List<OfferRequestResponseDTO>();

                foreach (var offerRequest in offerRequests)
                {
                    result.Add(await MapToResponseDTO(offerRequest));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salesman offer requests");
                throw;
            }
        }

        public async Task<List<OfferRequestResponseDTO>> GetOfferRequestsAssignedToAsync(string supportId, string? status)
        {
            try
            {
                var query = _unitOfWork.OfferRequests.GetQueryable().Where(or => or.AssignedTo == supportId);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(or => or.Status == status);

                var offerRequests = await query.OrderByDescending(or => or.CreatedAt).ToListAsync();
                var result = new List<OfferRequestResponseDTO>();

                foreach (var offerRequest in offerRequests)
                {
                    result.Add(await MapToResponseDTO(offerRequest));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned offer requests");
                throw;
            }
        }

        public async Task<OfferRequestResponseDTO> AssignToSupportAsync(long requestId, string supportId, string userId)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (offerRequest == null)
                    throw new ArgumentException("Offer request not found", nameof(requestId));

                if (!await CanModifyOfferRequestAsync(requestId, userId))
                    throw new UnauthorizedAccessException("You don't have permission to assign this offer request");

                // Validate support user exists and has SalesSupport role
                var supportUser = await _unitOfWork.Users.GetByIdAsync(supportId);
                if (supportUser == null)
                    throw new ArgumentException("Support user not found", nameof(supportId));

                // Verify user has SalesSupport role
                var userRoles = await _userManager.GetRolesAsync(supportUser);
                if (!userRoles.Contains("SalesSupport"))
                    throw new ArgumentException("User must have SalesSupport role", nameof(supportId));

                offerRequest.AssignTo(supportId);

                await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer request assigned successfully. RequestId: {RequestId}, SupportId: {SupportId}", requestId, supportId);

                return await MapToResponseDTO(offerRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning offer request");
                throw;
            }
        }

        public async Task<OfferRequestResponseDTO> UpdateStatusAsync(long requestId, string status, string? notes, string userId)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (offerRequest == null)
                    throw new ArgumentException("Offer request not found", nameof(requestId));

                if (!await CanModifyOfferRequestAsync(requestId, userId))
                    throw new UnauthorizedAccessException("You don't have permission to update this offer request");

                if (!OfferRequestStatusConstants.IsValidStatus(status))
                    throw new ArgumentException("Invalid status", nameof(status));

                // Use appropriate methods for status transitions
                switch (status)
                {
                    case "Ready":
                        offerRequest.MarkAsCompleted(notes);
                        break;
                    case "Sent":
                        offerRequest.MarkAsSent();
                        if (!string.IsNullOrEmpty(notes))
                            offerRequest.CompletionNotes = notes;
                        break;
                    case "Cancelled":
                        offerRequest.Cancel(notes);
                        break;
                    default:
                        offerRequest.Status = status;
                        if (!string.IsNullOrEmpty(notes))
                            offerRequest.CompletionNotes = notes;
                        break;
                }

                await _unitOfWork.OfferRequests.UpdateAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer request status updated successfully. RequestId: {RequestId}, Status: {Status}", requestId, status);

                return await MapToResponseDTO(offerRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer request status");
                throw;
            }
        }

        public async Task<bool> DeleteOfferRequestAsync(long requestId, string userId)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (offerRequest == null)
                    return false;

                if (!await CanModifyOfferRequestAsync(requestId, userId))
                    throw new UnauthorizedAccessException("You don't have permission to delete this offer request");

                await _unitOfWork.OfferRequests.DeleteAsync(offerRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Offer request deleted successfully. RequestId: {RequestId}", requestId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting offer request");
                throw;
            }
        }

        #endregion

        #region Business Logic Methods

        public async Task<bool> ValidateOfferRequestAsync(CreateOfferRequestDTO requestDto)
        {
            try
            {
                // Check if client exists
                var client = await _unitOfWork.Clients.GetByIdAsync(requestDto.ClientId);
                if (client == null)
                    return false;

                // Check if task progress exists if provided
                if (requestDto.TaskProgressId.HasValue)
                {
                    var taskProgress = await _unitOfWork.TaskProgresses.GetByIdAsync(requestDto.TaskProgressId.Value);
                    if (taskProgress == null)
                        return false;
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(requestDto.RequestedProducts))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating offer request");
                return false;
            }
        }

        public async Task<bool> CanModifyOfferRequestAsync(long requestId, string userId)
        {
            try
            {
                var offerRequest = await _unitOfWork.OfferRequests.GetByIdAsync(requestId);
                if (offerRequest == null)
                    return false;

                // User can modify their own requests
                if (offerRequest.RequestedBy == userId)
                    return true;

                // Assigned support can modify
                if (offerRequest.AssignedTo == userId)
                    return true;

                // Managers can modify
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    var userRoles = new List<string> { "Salesman" }; // This should be replaced with actual role checking
                    if (userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin"))
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking offer request modification permission");
                return false;
            }
        }

        public async Task<List<OfferRequestResponseDTO>> GetPendingRequestsAsync()
        {
            try
            {
                var offerRequests = await _unitOfWork.OfferRequests.GetRequestsByStatusAsync("Requested");
                var result = new List<OfferRequestResponseDTO>();

                foreach (var offerRequest in offerRequests)
                {
                    result.Add(await MapToResponseDTO(offerRequest));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending offer requests");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<OfferRequestResponseDTO> MapToResponseDTO(OfferRequest offerRequest)
        {
            var requester = await _unitOfWork.Users.GetByIdAsync(offerRequest.RequestedBy);
            var client = await _unitOfWork.Clients.GetByIdAsync(offerRequest.ClientId);
            var assignedUser = offerRequest.AssignedTo != null ? await _unitOfWork.Users.GetByIdAsync(offerRequest.AssignedTo) : null;

            return new OfferRequestResponseDTO
            {
                Id = offerRequest.Id,
                RequestedBy = offerRequest.RequestedBy,
                RequestedByName = requester != null ? $"{requester.FirstName} {requester.LastName}" : "Unknown",
                ClientId = offerRequest.ClientId,
                ClientName = client?.Name ?? "Unknown",
                RequestedProducts = offerRequest.RequestedProducts,
                SpecialNotes = offerRequest.SpecialNotes,
                RequestDate = offerRequest.RequestDate,
                Status = offerRequest.Status,
                AssignedTo = offerRequest.AssignedTo,
                AssignedToName = assignedUser != null ? $"{assignedUser.FirstName} {assignedUser.LastName}" : null,
                CreatedOfferId = offerRequest.CreatedOfferId
            };
        }

        #endregion
    }
}


