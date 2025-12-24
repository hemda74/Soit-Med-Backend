using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class ManagerDashboardService : IManagerDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ManagerDashboardService> _logger;

        public ManagerDashboardService(IUnitOfWork unitOfWork, ILogger<ManagerDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DashboardStatsResponseDto> GetDashboardStatisticsAsync(string managerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // Get subordinate user IDs
            var subordinateUserIds = await GetSubordinateUserIdsAsync(managerId, cancellationToken);
            
            if (!subordinateUserIds.Any())
            {
                return new DashboardStatsResponseDto
                {
                    StartDate = startDate,
                    EndDate = endDate
                };
            }

            // Get activities for the date range
            var activities = await _unitOfWork.ActivityLogs.GetWithDealsAndOffersAsync(
                subordinateUserIds.First(), // This method needs to be updated to accept multiple user IDs
                startDate, 
                endDate, 
                cancellationToken);

            // For now, we'll get activities for each user separately
            // In a real implementation, you'd want to update the repository method
            var allActivities = new List<ActivityLog>();
            foreach (var userId in subordinateUserIds)
            {
                var userActivities = await _unitOfWork.ActivityLogs.GetWithDealsAndOffersAsync(userId, startDate, endDate, cancellationToken);
                allActivities.AddRange(userActivities);
            }

            // Calculate statistics
            var stats = CalculateStatistics(allActivities, subordinateUserIds, startDate, endDate);

            return stats;
        }

        public async Task<IEnumerable<string>> GetSubordinateUserIdsAsync(string managerId, CancellationToken cancellationToken = default)
        {
            // This is a simplified implementation
            // In a real system, you'd have a proper hierarchy table or manager-employee relationships
            // For now, we'll return all users except the manager
            var allUsers = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            return allUsers
                .Where(u => u.Id != managerId)
                .Select(u => u.Id)
                .ToList();
        }

        private DashboardStatsResponseDto CalculateStatistics(IEnumerable<ActivityLog> activities, IEnumerable<string> userIds, DateTime startDate, DateTime endDate)
        {
            var activitiesList = activities.ToList();

            // Basic counts
            var totalActivities = activitiesList.Count;
            var totalVisits = activitiesList.Count(a => a.InteractionType == InteractionType.Visit);
            var totalFollowUps = activitiesList.Count(a => a.InteractionType == InteractionType.FollowUp);

            // Deal statistics
            var deals = activitiesList.Where(a => a.Deal != null).Select(a => a.Deal!).ToList();
            var totalDeals = deals.Count;
            var wonDeals = deals.Count(d => d.Status == Models.Enums.DealStatus.Completed || d.Status == Models.Enums.DealStatus.Closed);
            var lostDeals = deals.Count(d => d.Status == Models.Enums.DealStatus.Lost);
            var pendingDeals = deals.Count(d => d.Status == Models.Enums.DealStatus.Pending);

            // Offer statistics
            var offers = activitiesList.Where(a => a.Offer != null).Select(a => a.Offer!).ToList();
            var totalOffers = offers.Count;
            var acceptedOffers = offers.Count(o => o.Status == Models.Enums.OfferStatus.Accepted);
            var rejectedOffers = offers.Count(o => o.Status == Models.Enums.OfferStatus.Rejected);
            var draftOffers = offers.Count(o => o.Status == Models.Enums.OfferStatus.Pending);
            var sentOffers = offers.Count(o => o.Status == Models.Enums.OfferStatus.Sent);

            // Value calculations
            var totalDealValue = deals.Sum(d => d.DealValue);
            var wonDealValue = deals.Where(d => d.Status == Models.Enums.DealStatus.Completed || d.Status == Models.Enums.DealStatus.Closed).Sum(d => d.DealValue);
            var averageDealValue = totalDeals > 0 ? totalDealValue / totalDeals : 0;

            // Conversion rates
            var conversionRate = totalActivities > 0 ? (decimal)totalDeals / totalActivities * 100 : 0;
            var offerAcceptanceRate = totalOffers > 0 ? (decimal)acceptedOffers / totalOffers * 100 : 0;

            // Client type statistics
            var clientTypeStats = activitiesList
                .GroupBy(a => a.ClientType)
                .Select(g => new ClientTypeStatsDto
                {
                    ClientType = g.Key,
                    ClientTypeName = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalActivities > 0 ? (decimal)g.Count() / totalActivities * 100 : 0,
                    TotalValue = g.Where(a => a.Deal != null).Sum(a => a.Deal!.DealValue)
                })
                .OrderBy(s => s.ClientType)
                .ToList();

            // Salesperson statistics
            var salespersonStats = new List<SalespersonStatsDto>();
            foreach (var userId in userIds)
            {
                var userActivities = activitiesList.Where(a => a.UserId == userId).ToList();
                var userDeals = userActivities.Where(a => a.Deal != null).Select(a => a.Deal!).ToList();
                
                var user = _unitOfWork.Users.GetByIdAsync(userId, CancellationToken.None).Result;
                var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

                salespersonStats.Add(new SalespersonStatsDto
                {
                    UserId = userId,
                    UserName = userName,
                    TotalActivities = userActivities.Count,
                    TotalDeals = userDeals.Count,
                    WonDeals = userDeals.Count(d => d.Status == Models.Enums.DealStatus.Completed || d.Status == Models.Enums.DealStatus.Closed),
                    TotalValue = userDeals.Sum(d => d.DealValue),
                    WonValue = userDeals.Where(d => d.Status == Models.Enums.DealStatus.Completed || d.Status == Models.Enums.DealStatus.Closed).Sum(d => d.DealValue),
                    ConversionRate = userActivities.Count > 0 ? (decimal)userDeals.Count / userActivities.Count * 100 : 0
                });
            }

            // Recent activities (last 10)
            var recentActivities = activitiesList
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new RecentActivityDto
                {
                    Id = a.Id,
                    UserName = _unitOfWork.Users.GetByIdAsync(a.UserId, CancellationToken.None).Result?.FirstName + " " + 
                              _unitOfWork.Users.GetByIdAsync(a.UserId, CancellationToken.None).Result?.LastName ?? "Unknown User",
                    InteractionType = a.InteractionType,
                    InteractionTypeName = a.InteractionType.ToString(),
                    ClientType = a.ClientType,
                    ClientTypeName = a.ClientType.ToString(),
                    Result = a.Result,
                    ResultName = a.Result.ToString(),
                    Comment = a.Comment,
                    CreatedAt = a.CreatedAt,
                    DealValue = a.Deal?.DealValue,
                    OfferDetails = a.Offer?.OfferDetails
                })
                .ToList();

            return new DashboardStatsResponseDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalActivities = totalActivities,
                TotalVisits = totalVisits,
                TotalFollowUps = totalFollowUps,
                TotalDeals = totalDeals,
                TotalOffers = totalOffers,
                WonDeals = wonDeals,
                LostDeals = lostDeals,
                PendingDeals = pendingDeals,
                AcceptedOffers = acceptedOffers,
                RejectedOffers = rejectedOffers,
                DraftOffers = draftOffers,
                SentOffers = sentOffers,
                TotalDealValue = totalDealValue,
                WonDealValue = wonDealValue,
                AverageDealValue = averageDealValue,
                ConversionRate = conversionRate,
                OfferAcceptanceRate = offerAcceptanceRate,
                ClientTypeStats = clientTypeStats,
                SalespersonStats = salespersonStats,
                RecentActivities = recentActivities
            };
        }
    }
}
