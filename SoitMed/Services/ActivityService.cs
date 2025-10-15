using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActivityService> _logger;

        public ActivityService(IUnitOfWork unitOfWork, ILogger<ActivityService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ActivityResponseDto> CreateActivityAsync(int taskId, string userId, CreateActivityRequestDto request, CancellationToken cancellationToken = default)
        {
            // Use execution strategy to handle retries and transactions properly
            var strategy = _unitOfWork.GetContext().Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Validate the request
                    ValidateCreateActivityRequest(request);

                    // Verify the task exists and belongs to the user
                    var task = await _unitOfWork.WeeklyPlanTasks.GetByIdAsync(taskId, cancellationToken);
                    if (task == null)
                    {
                        throw new ArgumentException("Task not found", nameof(taskId));
                    }

                    // Verify the task belongs to the user's weekly plan
                    var weeklyPlan = await _unitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId, cancellationToken);
                    if (weeklyPlan == null || weeklyPlan.EmployeeId != userId.ToString())
                    {
                        throw new UnauthorizedAccessException("You don't have permission to add activities to this task");
                    }

                    // Create the activity log
                    var activityLog = new ActivityLog
                    {
                        PlanTaskId = taskId,
                        UserId = userId,
                        InteractionType = request.InteractionType,
                        ClientType = request.ClientType,
                        Result = request.Result,
                        Reason = request.Reason,
                        Comment = request.Comment,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ActivityLogs.CreateAsync(activityLog, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Create deal if provided
                    Deal? deal = null;
                    if (request.DealInfo != null)
                    {
                        deal = new Deal
                        {
                            ActivityLogId = activityLog.Id,
                            UserId = userId,
                            DealValue = request.DealInfo.DealValue,
                            Status = DealStatus.Pending,
                            ExpectedCloseDate = request.DealInfo.ExpectedCloseDate,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Deals.CreateAsync(deal, cancellationToken);
                    }

                    // Create offer if provided
                    Offer? offer = null;
                    if (request.OfferInfo != null)
                    {
                        offer = new Offer
                        {
                            ActivityLogId = activityLog.Id,
                            UserId = userId,
                            OfferDetails = request.OfferInfo.OfferDetails,
                            Status = OfferStatus.Draft,
                            DocumentUrl = request.OfferInfo.DocumentUrl,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Offers.CreateAsync(offer, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // Get user name for response
                    var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
                    var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

                    return MapToActivityResponseDto(activityLog, deal, offer, userName);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<DealResponseDto?> UpdateDealAsync(long dealId, string userId, UpdateDealDto updateDto, CancellationToken cancellationToken = default)
        {
            var deal = await _unitOfWork.Deals.GetByIdAsync(dealId, cancellationToken);
            if (deal == null || deal.UserId != userId)
            {
                return null;
            }

            if (updateDto.DealValue.HasValue)
                deal.DealValue = updateDto.DealValue.Value;

            if (updateDto.Status.HasValue)
                deal.Status = updateDto.Status.Value;

            if (updateDto.ExpectedCloseDate.HasValue)
                deal.ExpectedCloseDate = updateDto.ExpectedCloseDate.Value;

            deal.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Deals.UpdateAsync(deal, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

            return MapToDealResponseDto(deal, userName);
        }

        public async Task<OfferResponseDto?> UpdateOfferAsync(long offerId, string userId, UpdateOfferDto updateDto, CancellationToken cancellationToken = default)
        {
            var offer = await _unitOfWork.Offers.GetByIdAsync(offerId, cancellationToken);
            if (offer == null || offer.UserId != userId)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(updateDto.OfferDetails))
                offer.OfferDetails = updateDto.OfferDetails;

            if (updateDto.Status.HasValue)
                offer.Status = updateDto.Status.Value;

            if (updateDto.DocumentUrl != null)
                offer.DocumentUrl = updateDto.DocumentUrl;

            offer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Offers.UpdateAsync(offer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

            return MapToOfferResponseDto(offer, userName);
        }

        public async Task<IEnumerable<ActivityResponseDto>> GetActivitiesByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var activities = await _unitOfWork.ActivityLogs.GetWithDealsAndOffersAsync(
                userId, 
                startDate ?? DateTime.MinValue, 
                endDate ?? DateTime.MaxValue, 
                cancellationToken);

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

            return activities.Select(activity => MapToActivityResponseDto(activity, activity.Deal, activity.Offer, userName));
        }

        public async Task<ActivityResponseDto?> GetActivityByIdAsync(long activityId, string userId, CancellationToken cancellationToken = default)
        {
            var activity = await _unitOfWork.ActivityLogs.GetByIdAsync(activityId, cancellationToken);
            if (activity == null || activity.UserId != userId)
            {
                return null;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

            return MapToActivityResponseDto(activity, activity.Deal, activity.Offer, userName);
        }

        private void ValidateCreateActivityRequest(CreateActivityRequestDto request)
        {
            if (request.Result == ActivityResult.Interested)
            {
                if (request.DealInfo == null && request.OfferInfo == null)
                {
                    throw new ArgumentException("Either DealInfo or OfferInfo must be provided when Result is Interested");
                }

                if (request.DealInfo != null && request.OfferInfo != null)
                {
                    throw new ArgumentException("Cannot provide both DealInfo and OfferInfo for the same activity");
                }
            }

            if (request.Result == ActivityResult.NotInterested && request.Reason == null)
            {
                throw new ArgumentException("Reason must be provided when Result is NotInterested");
            }
        }

        private ActivityResponseDto MapToActivityResponseDto(ActivityLog activity, Deal? deal, Offer? offer, string userName)
        {
            return new ActivityResponseDto
            {
                Id = activity.Id,
                PlanTaskId = activity.PlanTaskId,
                UserId = activity.UserId,
                UserName = userName,
                InteractionType = activity.InteractionType,
                InteractionTypeName = activity.InteractionType.ToString(),
                ClientType = activity.ClientType,
                ClientTypeName = activity.ClientType.ToString(),
                Result = activity.Result,
                ResultName = activity.Result.ToString(),
                Reason = activity.Reason,
                ReasonName = activity.Reason?.ToString(),
                Comment = activity.Comment,
                CreatedAt = activity.CreatedAt,
                UpdatedAt = activity.UpdatedAt,
                Deal = deal != null ? MapToDealResponseDto(deal, userName) : null,
                Offer = offer != null ? MapToOfferResponseDto(offer, userName) : null
            };
        }

        private DealResponseDto MapToDealResponseDto(Deal deal, string userName)
        {
            return new DealResponseDto
            {
                Id = deal.Id,
                ActivityLogId = deal.ActivityLogId,
                UserId = deal.UserId,
                UserName = userName,
                DealValue = deal.DealValue,
                Status = deal.Status,
                StatusName = deal.Status.ToString(),
                ExpectedCloseDate = deal.ExpectedCloseDate,
                CreatedAt = deal.CreatedAt,
                UpdatedAt = deal.UpdatedAt
            };
        }

        private OfferResponseDto MapToOfferResponseDto(Offer offer, string userName)
        {
            return new OfferResponseDto
            {
                Id = offer.Id,
                ActivityLogId = offer.ActivityLogId,
                UserId = offer.UserId,
                UserName = userName,
                OfferDetails = offer.OfferDetails,
                Status = offer.Status,
                StatusName = offer.Status.ToString(),
                DocumentUrl = offer.DocumentUrl,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt
            };
        }
    }
}
