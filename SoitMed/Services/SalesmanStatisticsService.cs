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

            // Use bulk queries for better performance - fetch all data at once
            var context = _unitOfWork.GetContext();
            var salesmanIds = salesmen.Select(s => s.Id).ToList();

            // Fetch all visits, offers, and deals for all salesmen in one query each
            var allVisits = await context.TaskProgresses
                .AsNoTracking()
                .Where(tp => salesmanIds.Contains(tp.EmployeeId)
                    && tp.ProgressDate >= startDate
                    && tp.ProgressDate < endDate
                    && !string.IsNullOrEmpty(tp.VisitResult))
                .ToListAsync();

            var allOffers = await context.SalesOffers
                .AsNoTracking()
                .Where(o => salesmanIds.Contains(o.AssignedTo)
                    && o.CreatedAt >= startDate
                    && o.CreatedAt < endDate)
                .ToListAsync();

            var allDeals = await context.SalesDeals
                .AsNoTracking()
                .Where(d => salesmanIds.Contains(d.SalesmanId)
                    && d.CreatedAt >= startDate
                    && d.CreatedAt < endDate)
                .ToListAsync();

            // Process statistics for each salesman from in-memory data
            var statistics = new List<SalesmanStatisticsDTO>();

            foreach (var salesman in salesmen)
            {
                var visits = allVisits.Where(v => v.EmployeeId == salesman.Id).ToList();
                var offers = allOffers.Where(o => o.AssignedTo == salesman.Id).ToList();
                var deals = allDeals.Where(d => d.SalesmanId == salesman.Id).ToList();

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

        public async Task<SalesmanTargetDTO> CreateTargetAsync(CreateSalesmanTargetDTO dto, string? managerId, string? salesmanId = null)
        {
            // Validate target type and permissions
            if (dto.TargetType == TargetType.Money && string.IsNullOrEmpty(managerId))
                throw new UnauthorizedAccessException("Money targets can only be set by managers");
            
            if (dto.TargetType == TargetType.Activity && string.IsNullOrEmpty(salesmanId))
                throw new UnauthorizedAccessException("Activity targets must be set by the salesman");

            // Validate salesman exists if individual target
            var targetSalesmanId = dto.TargetType == TargetType.Activity ? salesmanId : dto.SalesmanId;
            if (!dto.IsTeamTarget && !string.IsNullOrEmpty(targetSalesmanId))
            {
                var salesman = await _unitOfWork.Users.GetByIdAsync(targetSalesmanId);
                if (salesman == null)
                    throw new ArgumentException("Salesman not found", nameof(targetSalesmanId));
            }

            // Check if target already exists for this period and type
            var existingTargets = await _unitOfWork.SalesmanTargets.GetTargetsBySalesmanAsync(
                targetSalesmanId, dto.Year, dto.Quarter);
            
            var existingTarget = existingTargets.FirstOrDefault(t => t.TargetType == dto.TargetType);
            if (existingTarget != null)
                throw new InvalidOperationException($"Target of type {dto.TargetType} already exists for this period");

            var target = new SalesmanTarget
            {
                SalesmanId = targetSalesmanId,
                TargetType = dto.TargetType,
                CreatedByManagerId = dto.TargetType == TargetType.Money ? managerId : null,
                Year = dto.Year,
                Quarter = dto.Quarter,
                TargetVisits = dto.TargetVisits ?? 0,
                TargetSuccessfulVisits = dto.TargetSuccessfulVisits ?? 0,
                TargetOffers = dto.TargetOffers ?? 0,
                TargetDeals = dto.TargetDeals ?? 0,
                TargetOfferAcceptanceRate = dto.TargetOfferAcceptanceRate,
                TargetRevenue = dto.TargetRevenue,
                IsTeamTarget = dto.IsTeamTarget,
                Notes = dto.Notes
            };

            await _unitOfWork.SalesmanTargets.CreateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (dto.TargetType == TargetType.Money && !string.IsNullOrEmpty(managerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(managerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }
            else if (dto.TargetType == TargetType.Activity && !string.IsNullOrEmpty(salesmanId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(salesmanId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetType = target.TargetType,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                TargetRevenue = target.TargetRevenue,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = dto.TargetType == TargetType.Money ? createdByName : null,
                CreatedBySalesmanName = dto.TargetType == TargetType.Activity ? createdByName : null
            };
        }

        public async Task<SalesmanTargetDTO> UpdateTargetAsync(long targetId, CreateSalesmanTargetDTO dto, string? currentUserId = null)
        {
            var target = await _unitOfWork.SalesmanTargets.GetByIdAsync(targetId);
            if (target == null)
                throw new ArgumentException("Target not found", nameof(targetId));

            // Validate permissions
            if (target.TargetType == TargetType.Money)
            {
                if (string.IsNullOrEmpty(currentUserId))
                    throw new UnauthorizedAccessException("Money targets can only be updated by managers");
                
                // Check if user is a manager (additional validation could be added here)
            }
            
            if (target.TargetType == TargetType.Activity)
            {
                if (string.IsNullOrEmpty(currentUserId))
                    throw new UnauthorizedAccessException("You must be logged in to update activity targets");
                
                if (string.IsNullOrEmpty(target.SalesmanId))
                    throw new UnauthorizedAccessException("This activity target is not associated with a salesman");
                
                if (target.SalesmanId != currentUserId)
                    throw new UnauthorizedAccessException("Activity targets can only be updated by the salesman who created them");
            }

            // Don't allow changing target type (only check if TargetType is explicitly set)
            if (dto.TargetType != 0 && target.TargetType != dto.TargetType)
                throw new InvalidOperationException("Cannot change target type");

            // Only update fields that are provided (for PATCH support)
            // Nullable int fields: null means field not provided, value means update
            if (dto.TargetVisits.HasValue)
                target.TargetVisits = dto.TargetVisits.Value;
            if (dto.TargetSuccessfulVisits.HasValue)
                target.TargetSuccessfulVisits = dto.TargetSuccessfulVisits.Value;
            if (dto.TargetOffers.HasValue)
                target.TargetOffers = dto.TargetOffers.Value;
            if (dto.TargetDeals.HasValue)
                target.TargetDeals = dto.TargetDeals.Value;
            
            // For nullable fields, only update if provided
            if (dto.TargetOfferAcceptanceRate.HasValue)
                target.TargetOfferAcceptanceRate = dto.TargetOfferAcceptanceRate;
            if (dto.TargetRevenue.HasValue)
                target.TargetRevenue = dto.TargetRevenue;
            if (dto.Notes != null)
                target.Notes = dto.Notes;

            await _unitOfWork.SalesmanTargets.UpdateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }
            else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesmanId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.SalesmanId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetType = target.TargetType,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                TargetRevenue = target.TargetRevenue,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = target.TargetType == TargetType.Money ? createdByName : null,
                CreatedBySalesmanName = target.TargetType == TargetType.Activity ? createdByName : null
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

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }
            else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesmanId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.SalesmanId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = target.SalesmanId != null 
                    ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                    : "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetType = target.TargetType,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                TargetRevenue = target.TargetRevenue,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = target.TargetType == TargetType.Money ? createdByName : null,
                CreatedBySalesmanName = target.TargetType == TargetType.Activity ? createdByName : null
            };
        }

        public async Task<List<SalesmanTargetDTO>> GetTargetsForSalesmanAsync(string salesmanId, int year)
        {
            var targets = await _unitOfWork.SalesmanTargets.GetTargetsBySalesmanAsync(salesmanId, year);
            var result = new List<SalesmanTargetDTO>();

            foreach (var target in targets)
            {
                ApplicationUser? creator = null;
                string? createdByName = null;
                
                if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
                {
                    creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                    createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
                }
                else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesmanId))
                {
                    creator = await _unitOfWork.Users.GetByIdAsync(target.SalesmanId);
                    createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
                }

                result.Add(new SalesmanTargetDTO
                {
                    Id = target.Id,
                    SalesmanId = target.SalesmanId,
                    SalesmanName = target.SalesmanId != null 
                        ? $"{target.Salesman?.FirstName} {target.Salesman?.LastName}" 
                        : "Team",
                    Year = target.Year,
                    Quarter = target.Quarter,
                    TargetType = target.TargetType,
                    TargetVisits = target.TargetVisits,
                    TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                    TargetOffers = target.TargetOffers,
                    TargetDeals = target.TargetDeals,
                    TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                    TargetRevenue = target.TargetRevenue,
                    IsTeamTarget = target.IsTeamTarget,
                    Notes = target.Notes,
                    CreatedAt = target.CreatedAt,
                    CreatedByManagerName = target.TargetType == TargetType.Money ? createdByName : null,
                    CreatedBySalesmanName = target.TargetType == TargetType.Activity ? createdByName : null
                });
            }

            return result;
        }

        public async Task<SalesmanTargetDTO?> GetTeamTargetAsync(int year, int? quarter = null)
        {
            var target = await _unitOfWork.SalesmanTargets.GetTeamTargetAsync(year, quarter);
            if (target == null)
                return null;

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesmanTargetDTO
            {
                Id = target.Id,
                SalesmanId = target.SalesmanId,
                SalesmanName = "Team",
                Year = target.Year,
                Quarter = target.Quarter,
                TargetType = target.TargetType,
                TargetVisits = target.TargetVisits,
                TargetSuccessfulVisits = target.TargetSuccessfulVisits,
                TargetOffers = target.TargetOffers,
                TargetDeals = target.TargetDeals,
                TargetOfferAcceptanceRate = target.TargetOfferAcceptanceRate,
                TargetRevenue = target.TargetRevenue,
                IsTeamTarget = target.IsTeamTarget,
                Notes = target.Notes,
                CreatedAt = target.CreatedAt,
                CreatedByManagerName = target.TargetType == TargetType.Money ? createdByName : null,
                CreatedBySalesmanName = null
            };
        }

        #endregion

        #region Progress

        public async Task<SalesmanProgressDTO> GetSalesmanProgressAsync(string salesmanId, int year, int? quarter = null)
        {
            var statistics = await GetStatisticsAsync(salesmanId, year, quarter);
            var individualTargets = await _unitOfWork.SalesmanTargets.GetTargetsBySalesmanAsync(salesmanId, year, quarter);
            var allTargets = await _unitOfWork.SalesmanTargets.GetAllTargetsForPeriodAsync(year, quarter);
            var teamTargets = allTargets.Where(t => t.IsTeamTarget).ToList();

            var individualMoneyTarget = individualTargets.FirstOrDefault(t => t.TargetType == TargetType.Money);
            var individualActivityTarget = individualTargets.FirstOrDefault(t => t.TargetType == TargetType.Activity);
            var teamMoneyTarget = teamTargets.FirstOrDefault(t => t.TargetType == TargetType.Money);
            var teamActivityTarget = teamTargets.FirstOrDefault(t => t.TargetType == TargetType.Activity);
            
            // Convert to DTOs
            SalesmanTargetDTO? individualMoneyTargetDto = null;
            if (individualMoneyTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(individualMoneyTarget.CreatedByManagerId);
                individualMoneyTargetDto = new SalesmanTargetDTO
                {
                    Id = individualMoneyTarget.Id,
                    SalesmanId = individualMoneyTarget.SalesmanId,
                    SalesmanName = "Individual",
                    Year = individualMoneyTarget.Year,
                    Quarter = individualMoneyTarget.Quarter,
                    TargetType = individualMoneyTarget.TargetType,
                    TargetVisits = individualMoneyTarget.TargetVisits,
                    TargetSuccessfulVisits = individualMoneyTarget.TargetSuccessfulVisits,
                    TargetOffers = individualMoneyTarget.TargetOffers,
                    TargetDeals = individualMoneyTarget.TargetDeals,
                    TargetOfferAcceptanceRate = individualMoneyTarget.TargetOfferAcceptanceRate,
                    TargetRevenue = individualMoneyTarget.TargetRevenue,
                    IsTeamTarget = individualMoneyTarget.IsTeamTarget,
                    Notes = individualMoneyTarget.Notes,
                    CreatedAt = individualMoneyTarget.CreatedAt,
                    CreatedByManagerName = manager != null 
                        ? $"{manager.FirstName} {manager.LastName}" 
                        : "Unknown"
                };
            }

            SalesmanTargetDTO? individualActivityTargetDto = null;
            if (individualActivityTarget != null)
            {
                var salesman = await _unitOfWork.Users.GetByIdAsync(individualActivityTarget.SalesmanId);
                individualActivityTargetDto = new SalesmanTargetDTO
                {
                    Id = individualActivityTarget.Id,
                    SalesmanId = individualActivityTarget.SalesmanId,
                    SalesmanName = "Individual",
                    Year = individualActivityTarget.Year,
                    Quarter = individualActivityTarget.Quarter,
                    TargetType = individualActivityTarget.TargetType,
                    TargetVisits = individualActivityTarget.TargetVisits,
                    TargetSuccessfulVisits = individualActivityTarget.TargetSuccessfulVisits,
                    TargetOffers = individualActivityTarget.TargetOffers,
                    TargetDeals = individualActivityTarget.TargetDeals,
                    TargetOfferAcceptanceRate = individualActivityTarget.TargetOfferAcceptanceRate,
                    TargetRevenue = individualActivityTarget.TargetRevenue,
                    IsTeamTarget = individualActivityTarget.IsTeamTarget,
                    Notes = individualActivityTarget.Notes,
                    CreatedAt = individualActivityTarget.CreatedAt,
                    CreatedBySalesmanName = salesman != null 
                        ? $"{salesman.FirstName} {salesman.LastName}" 
                        : "Unknown"
                };
            }

            SalesmanTargetDTO? teamMoneyTargetDto = null;
            if (teamMoneyTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(teamMoneyTarget.CreatedByManagerId);
                teamMoneyTargetDto = new SalesmanTargetDTO
                {
                    Id = teamMoneyTarget.Id,
                    SalesmanId = teamMoneyTarget.SalesmanId,
                    SalesmanName = "Team",
                    Year = teamMoneyTarget.Year,
                    Quarter = teamMoneyTarget.Quarter,
                    TargetType = teamMoneyTarget.TargetType,
                    TargetVisits = teamMoneyTarget.TargetVisits,
                    TargetSuccessfulVisits = teamMoneyTarget.TargetSuccessfulVisits,
                    TargetOffers = teamMoneyTarget.TargetOffers,
                    TargetDeals = teamMoneyTarget.TargetDeals,
                    TargetOfferAcceptanceRate = teamMoneyTarget.TargetOfferAcceptanceRate,
                    TargetRevenue = teamMoneyTarget.TargetRevenue,
                    IsTeamTarget = teamMoneyTarget.IsTeamTarget,
                    Notes = teamMoneyTarget.Notes,
                    CreatedAt = teamMoneyTarget.CreatedAt,
                    CreatedByManagerName = manager != null 
                        ? $"{manager.FirstName} {manager.LastName}" 
                        : "Unknown"
                };
            }

            SalesmanTargetDTO? teamActivityTargetDto = null;
            if (teamActivityTarget != null)
            {
                teamActivityTargetDto = new SalesmanTargetDTO
                {
                    Id = teamActivityTarget.Id,
                    SalesmanId = teamActivityTarget.SalesmanId,
                    SalesmanName = "Team",
                    Year = teamActivityTarget.Year,
                    Quarter = teamActivityTarget.Quarter,
                    TargetType = teamActivityTarget.TargetType,
                    TargetVisits = teamActivityTarget.TargetVisits,
                    TargetSuccessfulVisits = teamActivityTarget.TargetSuccessfulVisits,
                    TargetOffers = teamActivityTarget.TargetOffers,
                    TargetDeals = teamActivityTarget.TargetDeals,
                    TargetOfferAcceptanceRate = teamActivityTarget.TargetOfferAcceptanceRate,
                    TargetRevenue = teamActivityTarget.TargetRevenue,
                    IsTeamTarget = teamActivityTarget.IsTeamTarget,
                    Notes = teamActivityTarget.Notes,
                    CreatedAt = teamActivityTarget.CreatedAt,
                    CreatedBySalesmanName = null
                };
            }

            // Calculate progress based on individual activity target if available, otherwise team activity target
            var activityTargetToUse = individualActivityTargetDto ?? teamActivityTargetDto;
            var moneyTargetToUse = individualMoneyTargetDto ?? teamMoneyTargetDto;
            
            var visitsProgress = activityTargetToUse != null && activityTargetToUse.TargetVisits > 0 
                ? (decimal)statistics.TotalVisits / activityTargetToUse.TargetVisits * 100 
                : 0;

            var successfulVisitsProgress = activityTargetToUse != null && activityTargetToUse.TargetSuccessfulVisits > 0 
                ? (decimal)statistics.SuccessfulVisits / activityTargetToUse.TargetSuccessfulVisits * 100 
                : 0;

            var offersProgress = activityTargetToUse != null && activityTargetToUse.TargetOffers > 0 
                ? (decimal)statistics.TotalOffers / activityTargetToUse.TargetOffers * 100 
                : 0;

            var dealsProgress = activityTargetToUse != null && activityTargetToUse.TargetDeals > 0 
                ? (decimal)statistics.TotalDeals / activityTargetToUse.TargetDeals * 100 
                : 0;

            var acceptanceRateProgress = activityTargetToUse != null && activityTargetToUse.TargetOfferAcceptanceRate != null && activityTargetToUse.TargetOfferAcceptanceRate > 0
                ? statistics.OfferAcceptanceRate / activityTargetToUse.TargetOfferAcceptanceRate * 100
                : null;

            decimal? revenueProgress = moneyTargetToUse != null && moneyTargetToUse.TargetRevenue.HasValue && moneyTargetToUse.TargetRevenue.Value > 0
                ? (decimal?)(statistics.TotalDealValue / moneyTargetToUse.TargetRevenue.Value * 100)
                : null;

            return new SalesmanProgressDTO
            {
                CurrentStatistics = statistics,
                IndividualMoneyTarget = individualMoneyTargetDto,
                IndividualActivityTarget = individualActivityTargetDto,
                TeamMoneyTarget = teamMoneyTargetDto,
                TeamActivityTarget = teamActivityTargetDto,
                VisitsProgress = Math.Min(visitsProgress, 100),
                SuccessfulVisitsProgress = Math.Min(successfulVisitsProgress, 100),
                OffersProgress = Math.Min(offersProgress, 100),
                DealsProgress = Math.Min(dealsProgress, 100),
                OfferAcceptanceRateProgress = acceptanceRateProgress.HasValue ? Math.Min(acceptanceRateProgress.Value, 100) : null,
                RevenueProgress = revenueProgress.HasValue ? Math.Min(revenueProgress.Value, 100) : null
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

