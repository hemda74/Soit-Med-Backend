using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class SalesmanStatisticsService : ISalesmanStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesmanStatisticsService> _logger;

        public SalesmanStatisticsService(IUnitOfWork unitOfWork, ILogger<SalesmanStatisticsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Statistics

        public async Task<SalesmanStatisticsDTO> GetStatisticsAsync(string salesmanId, int year, int? quarter = null)
        {
            var salesman = await _unitOfWork.Users.GetByIdAsync(salesmanId);
            if (salesman == null)
                throw new ArgumentException("Salesman not found", nameof(salesmanId));

            var (startDate, endDate) = GetDateRangeForPeriod(year, quarter);
            
            // Get visit statistics - Filter in database, not memory
            var salesmanVisits = await _unitOfWork.TaskProgresses.GetProgressesByEmployeeAsync(
                salesmanId, startDate, endDate);

            var totalVisits = salesmanVisits.Count(v => !string.IsNullOrEmpty(v.VisitResult));
            var successfulVisits = salesmanVisits.Count(v => v.VisitResult == "Interested");
            var failedVisits = salesmanVisits.Count(v => v.VisitResult == "NotInterested");
            var successRate = totalVisits > 0 ? (decimal)successfulVisits / totalVisits * 100 : 0;

            // Get offer statistics - Filter in database
            var context = _unitOfWork.GetContext();
            var salesmanOffers = await context.SalesOffers
                .AsNoTracking()
                .Where(o => o.AssignedTo == salesmanId
                    && o.CreatedAt >= startDate
                    && o.CreatedAt < endDate)
                .ToListAsync();

            var totalOffers = salesmanOffers.Count;
            var acceptedOffers = salesmanOffers.Count(o => o.Status == "Accepted");
            var rejectedOffers = salesmanOffers.Count(o => o.Status == "Rejected");
            var acceptanceRate = totalOffers > 0 ? (decimal)acceptedOffers / totalOffers * 100 : 0;

            // Get deal statistics - Filter in database
            var salesmanDeals = await context.SalesDeals
                .AsNoTracking()
                .Where(d => d.SalesmanId == salesmanId
                    && d.CreatedAt >= startDate
                    && d.CreatedAt < endDate)
                .ToListAsync();

            var totalDeals = salesmanDeals.Count;
            var totalDealValue = salesmanDeals.Sum(d => d.DealValue);

            return new SalesmanStatisticsDTO
            {
                SalesmanId = salesmanId,
                SalesmanName = $"{salesman.FirstName} {salesman.LastName}",
                Year = year,
                Quarter = quarter,
                TotalVisits = totalVisits,
                SuccessfulVisits = successfulVisits,
                FailedVisits = failedVisits,
                SuccessRate = successRate,
                TotalOffers = totalOffers,
                AcceptedOffers = acceptedOffers,
                RejectedOffers = rejectedOffers,
                OfferAcceptanceRate = acceptanceRate,
                TotalDeals = totalDeals,
                TotalDealValue = totalDealValue
            };
        }

        public async Task<List<SalesmanStatisticsDTO>> GetAllSalesmenStatisticsAsync(int year, int? quarter = null)
        {
            var salesmen = await _unitOfWork.Users.GetUsersInRoleAsync("Salesman");
            var (startDate, endDate) = GetDateRangeForPeriod(year, quarter);

            // Use bulk query instead of per-salesman queries for better performance
            var context = _unitOfWork.GetContext();
            var salesmanIds = salesmen.Select(s => s.Id).ToList();

            var statistics = new List<SalesmanStatisticsDTO>();

            foreach (var salesman in salesmen)
            {
                // Parallel database queries for each salesman
                var visitsTask = context.TaskProgresses
                    .AsNoTracking()
                    .Where(tp => tp.EmployeeId == salesman.Id
                        && tp.ProgressDate >= startDate
                        && tp.ProgressDate < endDate
                        && !string.IsNullOrEmpty(tp.VisitResult))
                    .ToListAsync();

                var offersTask = context.SalesOffers
                    .AsNoTracking()
                    .Where(o => o.AssignedTo == salesman.Id
                        && o.CreatedAt >= startDate
                        && o.CreatedAt < endDate)
                    .ToListAsync();

                var dealsTask = context.SalesDeals
                    .AsNoTracking()
                    .Where(d => d.SalesmanId == salesman.Id
                        && d.CreatedAt >= startDate
                        && d.CreatedAt < endDate)
                    .ToListAsync();

                await Task.WhenAll(visitsTask, offersTask, dealsTask);

                var visits = await visitsTask;
                var offers = await offersTask;
                var deals = await dealsTask;

                var totalVisits = visits.Count;
                var successfulVisits = visits.Count(v => v.VisitResult == "Interested");
                var failedVisits = visits.Count(v => v.VisitResult == "NotInterested");
                var successRate = totalVisits > 0 ? (decimal)successfulVisits / totalVisits * 100 : 0;

                var totalOffers = offers.Count;
                var acceptedOffers = offers.Count(o => o.Status == "Accepted");
                var rejectedOffers = offers.Count(o => o.Status == "Rejected");
                var acceptanceRate = totalOffers > 0 ? (decimal)acceptedOffers / totalOffers * 100 : 0;

                var totalDeals = deals.Count;
                var totalDealValue = deals.Sum(d => d.DealValue);

                statistics.Add(new SalesmanStatisticsDTO
                {
                    SalesmanId = salesman.Id,
                    SalesmanName = $"{salesman.FirstName} {salesman.LastName}",
                    Year = year,
                    Quarter = quarter,
                    TotalVisits = totalVisits,
                    SuccessfulVisits = successfulVisits,
                    FailedVisits = failedVisits,
                    SuccessRate = successRate,
                    TotalOffers = totalOffers,
                    AcceptedOffers = acceptedOffers,
                    RejectedOffers = rejectedOffers,
                    OfferAcceptanceRate = acceptanceRate,
                    TotalDeals = totalDeals,
                    TotalDealValue = totalDealValue
                });
            }

            return statistics;
        }

        #endregion

        #region Targets

        public async Task<SalesmanTargetDTO> CreateTargetAsync(CreateSalesmanTargetDTO dto, string managerId)
        {
            // Validate salesman exists if individual target
            if (!dto.IsTeamTarget && !string.IsNullOrEmpty(dto.SalesmanId))
            {
                var salesman = await _unitOfWork.Users.GetByIdAsync(dto.SalesmanId);
                if (salesman == null)
                    throw new ArgumentException("Salesman not found", nameof(dto.SalesmanId));
            }

            // Check if target already exists for this period
            var existingTarget = await _unitOfWork.SalesmanTargets.GetTargetBySalesmanAndPeriodAsync(
                dto.SalesmanId, dto.Year, dto.Quarter);

            if (existingTarget != null)
                throw new InvalidOperationException("Target already exists for this period");

            var target = new SalesmanTarget
            {
                SalesmanId = dto.SalesmanId,
                CreatedByManagerId = managerId,
                Year = dto.Year,
                Quarter = dto.Quarter,
                TargetVisits = dto.TargetVisits,
                TargetSuccessfulVisits = dto.TargetSuccessfulVisits,
                TargetOffers = dto.TargetOffers,
                TargetDeals = dto.TargetDeals,
                TargetOfferAcceptanceRate = dto.TargetOfferAcceptanceRate,
                IsTeamTarget = dto.IsTeamTarget,
                Notes = dto.Notes
            };

            await _unitOfWork.SalesmanTargets.CreateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            var manager = await _unitOfWork.Users.GetByIdAsync(managerId);

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = manager != null 
                    ? $"{manager.FirstName} {manager.LastName}" 
                    : "Unknown"
            };
        }

        public async Task<SalesmanTargetDTO> UpdateTargetAsync(long targetId, CreateSalesmanTargetDTO dto)
        {
            var target = await _unitOfWork.SalesmanTargets.GetByIdAsync(targetId);
            if (target == null)
                throw new ArgumentException("Target not found", nameof(targetId));

            target.TargetVisits = dto.TargetVisits;
            target.TargetSuccessfulVisits = dto.TargetSuccessfulVisits;
            target.TargetOffers = dto.TargetOffers;
            target.TargetDeals = dto.TargetDeals;
            target.TargetOfferAcceptanceRate = dto.TargetOfferAcceptanceRate;
            target.Notes = dto.Notes;

            await _unitOfWork.SalesmanTargets.UpdateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            var manager = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = manager != null 
                    ? $"{manager.FirstName} {manager.LastName}" 
                    : "Unknown"
            };
        }

        public async Task<bool> DeleteTargetAsync(long targetId)
        {
            var target = await _unitOfWork.SalesmanTargets.GetByIdAsync(targetId);
            if (target == null)
                return false;

            await _unitOfWork.SalesmanTargets.DeleteAsync(target);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<SalesmanTargetDTO?> GetTargetAsync(long targetId)
        {
            var target = await _unitOfWork.SalesmanTargets.GetByIdAsync(targetId);
            if (target == null)
                return null;

            var manager = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = manager != null 
                    ? $"{manager.FirstName} {manager.LastName}" 
                    : "Unknown"
            };
        }

        public async Task<List<SalesmanTargetDTO>> GetTargetsForSalesmanAsync(string salesmanId, int year)
        {
            var targets = await _unitOfWork.SalesmanTargets.GetTargetsBySalesmanAsync(salesmanId, year);
            var result = new List<SalesmanTargetDTO>();

            foreach (var target in targets)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                result.Add(new SalesmanTargetDTO
                {
                    Id = target.Id,
                    SalesmanId = target.SalesmanId,
                    SalesmanName = target.SalesmanId != null 
                        ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                        : "Team",
                    Year = target.Year,
                    Quarter = target.Quarter,
                    TargetVisits = target.TargetVisits,
                    TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                    TargetOffers = target.TargetOffers,
                    TargetDeals = target.TargetDeals,
                    TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                    IsTeamTarget = target.IsTeamTarget,
                    Notes = target.Notes,
                    CreatedAt = target.CreatedAt,
                    CreatedByManagerName = manager != null 
                        ? $"{manager.FirstName} {manager.LastName}" 
                        : "Unknown"
                });
            }

            return result;
        }

        public async Task<SalesmanTargetDTO?> GetTeamTargetAsync(int year, int? quarter = null)
        {
            var target = await _unitOfWork.SalesmanTargets.GetTeamTargetAsync(year, quarter);
            if (target == null)
                return null;

            var manager = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = manager != null 
                    ? $"{manager.FirstName} {manager.LastName}" 
                    : "Unknown"
            };
        }

        #endregion

        #region Progress

        public async Task<SalesmanProgressDTO> GetSalesmanProgressAsync(string salesmanId, int year, int? quarter = null)
        {
            var statistics = await GetStatisticsAsync(salesmanId, year, quarter);
            var individualTargets = await _unitOfWork.SalesmanTargets.GetTargetsBySalesmanAsync(salesmanId, year, quarter);
            var teamTarget = await _unitOfWork.SalesmanTargets.GetTeamTargetAsync(year, quarter);

            var individualTarget = individualTargets.FirstOrDefault();
            
            // Convert to DTOs
            SalesmanTargetDTO? individualTargetDto = null;
            if (individualTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(individualTarget.CreatedByManagerId);
                individualTargetDto = new SalesmanTargetDTO
                {
                    Id = individualTarget.Id,
                    SalesmanId = individualTarget.SalesmanId,
                    SalesmanName = "Individual",
                    Year = individualTarget.Year,
                    Quarter = individualTarget.Quarter,
                    TargetVisits = individualTarget.TargetVisits,
                    TargetSuccessfulVisits = individualTarget.TargetSuccessfulVisits,
                    TargetOffers = individualTarget.TargetOffers,
                    TargetDeals = individualTarget.TargetDeals,
                    TargetOfferAcceptanceRate = individualTarget.TargetOfferAcceptanceRate,
                    IsTeamTarget = individualTarget.IsTeamTarget,
                    Notes = individualTarget.Notes,
                    CreatedAt = individualTarget.CreatedAt,
                    CreatedByManagerName = manager != null 
                        ? $"{manager.FirstName} {manager.LastName}" 
                        : "Unknown"
                };
            }

            SalesmanTargetDTO? teamTargetDto = null;
            if (teamTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(teamTarget.CreatedByManagerId);
                teamTargetDto = new SalesmanTargetDTO
                {
                    Id = teamTarget.Id,
                    SalesmanId = teamTarget.SalesmanId,
                    SalesmanName = "Team",
                    Year = teamTarget.Year,
                    Quarter = teamTarget.Quarter,
                    TargetVisits = teamTarget.TargetVisits,
                    TargetSuccessfulVisits = teamTarget.TargetSuccessfulVisits,
                    TargetOffers = teamTarget.TargetOffers,
                    TargetDeals = teamTarget.TargetDeals,
                    TargetOfferAcceptanceRate = teamTarget.TargetOfferAcceptanceRate,
                    IsTeamTarget = teamTarget.IsTeamTarget,
                    Notes = teamTarget.Notes,
                    CreatedAt = teamTarget.CreatedAt,
                    CreatedByManagerName = manager != null 
                        ? $"{manager.FirstName} {manager.LastName}" 
                        : "Unknown"
                };
            }

            // Calculate progress based on individual target if available, otherwise team target
            var targetToUse = individualTargetDto ?? teamTargetDto;
            
            var visitsProgress = targetToUse != null && targetToUse.TargetVisits > 0 
                ? (decimal)statistics.TotalVisits / targetToUse.TargetVisits * 100 
                : 0;

            var successfulVisitsProgress = targetToUse != null && targetToUse.TargetSuccessfulVisits > 0 
                ? (decimal)statistics.SuccessfulVisits / targetToUse.TargetSuccessfulVisits * 100 
                : 0;

            var offersProgress = targetToUse != null && targetToUse.TargetOffers > 0 
                ? (decimal)statistics.TotalOffers / targetToUse.TargetOffers * 100 
                : 0;

            var dealsProgress = targetToUse != null && targetToUse.TargetDeals > 0 
                ? (decimal)statistics.TotalDeals / targetToUse.TargetDeals * 100 
                : 0;

            var acceptanceRateProgress = targetToUse != null && targetToUse.TargetOfferAcceptanceRate != null && targetToUse.TargetOfferAcceptanceRate > 0
                ? statistics.OfferAcceptanceRate / targetToUse.TargetOfferAcceptanceRate * 100
                : null;

            return new SalesmanProgressDTO
            {
                CurrentStatistics = statistics,
                IndividualTarget = individualTargetDto,
                TeamTarget = teamTargetDto,
                VisitsProgress = Math.Min(visitsProgress, 100),
                SuccessfulVisitsProgress = Math.Min(successfulVisitsProgress, 100),
                OffersProgress = Math.Min(offersProgress, 100),
                DealsProgress = Math.Min(dealsProgress, 100),
                OfferAcceptanceRateProgress = acceptanceRateProgress.HasValue ? Math.Min(acceptanceRateProgress.Value, 100) : null
            };
        }

        #endregion

        #region Private Helpers

        private (DateTime startDate, DateTime endDate) GetDateRangeForPeriod(int year, int? quarter)
        {
            DateTime startDate, endDate;

            if (quarter.HasValue)
            {
                // Quarterly date ranges
                switch (quarter.Value)
                {
                    case 1:
                        startDate = new DateTime(year, 1, 1);
                        endDate = new DateTime(year, 4, 1);
                        break;
                    case 2:
                        startDate = new DateTime(year, 4, 1);
                        endDate = new DateTime(year, 7, 1);
                        break;
                    case 3:
                        startDate = new DateTime(year, 7, 1);
                        endDate = new DateTime(year, 10, 1);
                        break;
                    case 4:
                        startDate = new DateTime(year, 10, 1);
                        endDate = new DateTime(year + 1, 1, 1);
                        break;
                    default:
                        throw new ArgumentException("Invalid quarter", nameof(quarter));
                }
            }
            else
            {
                // Yearly date range
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year + 1, 1, 1);
            }

            return (startDate, endDate);
        }

        #endregion
    }
}

