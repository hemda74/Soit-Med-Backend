using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class SalesmanStatsService : ISalesmanStatsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesmanStatsService> _logger;

        public SalesmanStatsService(IUnitOfWork unitOfWork, ILogger<SalesmanStatsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SalesmanStatsResponseDto> GetSalesmanStatisticsAsync(string salesmanId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            // Get user information
            var user = await _unitOfWork.Users.GetByIdAsync(salesmanId, cancellationToken);
            var userName = user?.FirstName + " " + user?.LastName ?? "Unknown User";

            // Get activities for the date range
            var activities = await _unitOfWork.ActivityLogs.GetWithDealsAndOffersAsync(
                salesmanId, 
                startDate, 
                endDate, 
                cancellationToken);

            // Calculate statistics
            var stats = CalculateSalesmanStatistics(activities, salesmanId, userName, startDate, endDate);

            return stats;
        }

        public async Task<SalesmanStatsResponseDto> GetSalesmanCurrentWeekStatsAsync(string salesmanId, CancellationToken cancellationToken = default)
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);

            return await GetSalesmanStatisticsAsync(salesmanId, startOfWeek, endOfWeek, cancellationToken);
        }

        public async Task<SalesmanStatsResponseDto> GetSalesmanCurrentMonthStatsAsync(string salesmanId, CancellationToken cancellationToken = default)
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            return await GetSalesmanStatisticsAsync(salesmanId, startOfMonth, endOfMonth, cancellationToken);
        }

        private SalesmanStatsResponseDto CalculateSalesmanStatistics(IEnumerable<ActivityLog> activities, string salesmanId, string userName, DateTime startDate, DateTime endDate)
        {
            var activitiesList = activities.ToList();

            // Basic activity counts
            var totalActivities = activitiesList.Count;
            var totalVisits = activitiesList.Count(a => a.InteractionType == InteractionType.Visit);
            var totalFollowUps = activitiesList.Count(a => a.InteractionType == InteractionType.FollowUp);
            var interestedActivities = activitiesList.Count(a => a.Result == ActivityResult.Interested);
            var notInterestedActivities = activitiesList.Count(a => a.Result == ActivityResult.NotInterested);

            // Deal statistics
            var deals = activitiesList.Where(a => a.Deal != null).Select(a => a.Deal!).ToList();
            var totalDeals = deals.Count;
            var wonDeals = deals.Count(d => d.Status == "Won" || d.Status == "Approved" || d.Status == "Completed");
            var lostDeals = deals.Count(d => d.Status == "Lost" || d.Status == "Rejected");
            var pendingDeals = deals.Count(d => d.Status == "Pending");

            // Offer statistics
            var offers = activitiesList.Where(a => a.Offer != null).Select(a => a.Offer!).ToList();
            var totalOffers = offers.Count;
            var acceptedOffers = offers.Count(o => o.Status == "Accepted");
            var rejectedOffers = offers.Count(o => o.Status == "Rejected");
            var draftOffers = offers.Count(o => o.Status == "Draft");
            var sentOffers = offers.Count(o => o.Status == "Sent");

            // Value calculations
            var totalDealValue = deals.Sum(d => d.Value);
            var wonDealValue = deals.Where(d => d.Status == "Won" || d.Status == "Approved" || d.Status == "Completed").Sum(d => d.Value);
            var averageDealValue = totalDeals > 0 ? totalDealValue / totalDeals : 0;

            // Performance metrics
            var conversionRate = totalActivities > 0 ? (decimal)totalDeals / totalActivities * 100 : 0;
            var offerAcceptanceRate = totalOffers > 0 ? (decimal)acceptedOffers / totalOffers * 100 : 0;
            var interestRate = totalActivities > 0 ? (decimal)interestedActivities / totalActivities * 100 : 0;

            // Client type statistics
            var clientTypeStats = activitiesList
                .GroupBy(a => a.ClientType)
                .Select(g => new ClientTypeStatsDto
                {
                    ClientType = g.Key,
                    ClientTypeName = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalActivities > 0 ? (decimal)g.Count() / totalActivities * 100 : 0,
                    TotalValue = g.Where(a => a.Deal != null).Sum(a => a.Deal!.Value)
                })
                .OrderBy(s => s.ClientType)
                .ToList();

            // Recent activities (last 10)
            var recentActivities = activitiesList
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new RecentActivityDto
                {
                    Id = a.Id,
                    UserName = userName,
                    InteractionType = a.InteractionType,
                    InteractionTypeName = a.InteractionType.ToString(),
                    ClientType = a.ClientType,
                    ClientTypeName = a.ClientType.ToString(),
                    Result = a.Result,
                    ResultName = a.Result.ToString(),
                    Comment = a.Comment,
                    CreatedAt = a.CreatedAt,
                    DealValue = a.Deal?.Value,
                    OfferDetails = a.Offer?.Description
                })
                .ToList();

            // Weekly trends (last 8 weeks)
            var weeklyTrends = CalculateWeeklyTrends(activitiesList, startDate, endDate);

            return new SalesmanStatsResponseDto
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = salesmanId,
                UserName = userName,
                TotalActivities = totalActivities,
                TotalVisits = totalVisits,
                TotalFollowUps = totalFollowUps,
                InterestedActivities = interestedActivities,
                NotInterestedActivities = notInterestedActivities,
                TotalDeals = totalDeals,
                WonDeals = wonDeals,
                LostDeals = lostDeals,
                PendingDeals = pendingDeals,
                TotalDealValue = totalDealValue,
                WonDealValue = wonDealValue,
                AverageDealValue = averageDealValue,
                TotalOffers = totalOffers,
                AcceptedOffers = acceptedOffers,
                RejectedOffers = rejectedOffers,
                DraftOffers = draftOffers,
                SentOffers = sentOffers,
                ConversionRate = conversionRate,
                OfferAcceptanceRate = offerAcceptanceRate,
                InterestRate = interestRate,
                ClientTypeStats = clientTypeStats,
                RecentActivities = recentActivities,
                WeeklyTrends = weeklyTrends
            };
        }

        private List<WeeklyTrendDto> CalculateWeeklyTrends(IEnumerable<ActivityLog> activities, DateTime startDate, DateTime endDate)
        {
            var trends = new List<WeeklyTrendDto>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var weekStart = currentDate;
                var weekEnd = currentDate.AddDays(6);
                if (weekEnd > endDate) weekEnd = endDate;

                var weekActivities = activities.Where(a => a.CreatedAt >= weekStart && a.CreatedAt <= weekEnd).ToList();
                var weekDeals = weekActivities.Where(a => a.Deal != null).Select(a => a.Deal!).ToList();
                var weekOffers = weekActivities.Where(a => a.Offer != null).Select(a => a.Offer!).ToList();

                trends.Add(new WeeklyTrendDto
                {
                    WeekStart = weekStart,
                    WeekEnd = weekEnd,
                    Activities = weekActivities.Count,
                    Visits = weekActivities.Count(a => a.InteractionType == InteractionType.Visit),
                    FollowUps = weekActivities.Count(a => a.InteractionType == InteractionType.FollowUp),
                    Deals = weekDeals.Count,
                    WonDeals = weekDeals.Count(d => d.Status == "Won" || d.Status == "Approved" || d.Status == "Completed"),
                    DealValue = weekDeals.Sum(d => d.Value),
                    Offers = weekOffers.Count,
                    AcceptedOffers = weekOffers.Count(o => o.Status == "Accepted")
                });

                currentDate = currentDate.AddDays(7);
            }

            return trends;
        }
    }
}
