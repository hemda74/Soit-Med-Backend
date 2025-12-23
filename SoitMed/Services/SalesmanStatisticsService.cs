using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    public class SalesManStatisticsService : ISalesManStatisticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SalesManStatisticsService> _logger;

        public SalesManStatisticsService(IUnitOfWork unitOfWork, ILogger<SalesManStatisticsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Statistics

        public async Task<SalesManStatisticsDTO> GetStatisticsAsync(string salesmanId, int year, int? quarter = null)
        {
            var salesman = await _unitOfWork.Users.GetByIdAsync(salesmanId);
            if (salesman == null)
                throw new ArgumentException("SalesMan not found", nameof(salesmanId));

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
                .Where(d => d.SalesManId == salesmanId
                    && d.CreatedAt >= startDate
                    && d.CreatedAt < endDate)
                .ToListAsync();

            var totalDeals = salesmanDeals.Count;
            var totalDealValue = salesmanDeals.Sum(d => d.DealValue);

            return new SalesManStatisticsDTO
            {
                SalesManId = salesmanId,
                SalesManName = $"{salesman.FirstName} {salesman.LastName}",
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

        public async Task<List<SalesManStatisticsDTO>> GetAllSalesmenStatisticsAsync(int year, int? quarter = null)
        {
            var salesmen = await _unitOfWork.Users.GetUsersInRoleAsync("SalesMan");
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
                .Where(d => salesmanIds.Contains(d.SalesManId)
                    && d.CreatedAt >= startDate
                    && d.CreatedAt < endDate)
                .ToListAsync();

            // Process statistics for each salesman from in-memory data
            var statistics = new List<SalesManStatisticsDTO>();

            foreach (var salesman in salesmen)
            {
                var visits = allVisits.Where(v => v.EmployeeId == salesman.Id).ToList();
                var offers = allOffers.Where(o => o.AssignedTo == salesman.Id).ToList();
                var deals = allDeals.Where(d => d.SalesManId == salesman.Id).ToList();

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

                statistics.Add(new SalesManStatisticsDTO
                {
                    SalesManId = salesman.Id,
                    SalesManName = $"{salesman.FirstName} {salesman.LastName}",
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

        public async Task<SalesManTargetDTO> CreateTargetAsync(CreateSalesManTargetDTO dto, string? managerId, string? salesmanId = null)
        {
            // Validate target type and permissions
            if (dto.TargetType == TargetType.Money && string.IsNullOrEmpty(managerId))
                throw new UnauthorizedAccessException("Money targets can only be set by managers");
            
            if (dto.TargetType == TargetType.Activity && string.IsNullOrEmpty(salesmanId))
                throw new UnauthorizedAccessException("Activity targets must be set by the salesman");

            // Validate salesman exists if individual target
            var targetSalesManId = dto.TargetType == TargetType.Activity ? salesmanId : dto.SalesManId;
            if (!dto.IsTeamTarget && !string.IsNullOrEmpty(targetSalesManId))
            {
                var salesman = await _unitOfWork.Users.GetByIdAsync(targetSalesManId);
                if (salesman == null)
                    throw new ArgumentException("SalesMan not found", nameof(targetSalesManId));
            }

            // Check if target already exists for this period and type
            var existingTargets = await _unitOfWork.SalesManTargets.GetTargetsBySalesManAsync(
                targetSalesManId, dto.Year, dto.Quarter);
            
            var existingTarget = existingTargets.FirstOrDefault(t => t.TargetType == dto.TargetType);
            if (existingTarget != null)
                throw new InvalidOperationException($"Target of type {dto.TargetType} already exists for this period");

            var target = new SalesManTarget
            {
                SalesManId = targetSalesManId,
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

            await _unitOfWork.SalesManTargets.CreateAsync(target);
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

            return new SalesManTargetDTO
            {
                Id = target.Id,
                SalesManId = target.SalesManId,
                SalesManName = target.SalesManId != null 
                    ? $"{target.SalesMan?.FirstName} {target.SalesMan?.LastName}" 
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
                CreatedBySalesManName = dto.TargetType == TargetType.Activity ? createdByName : null
            };
        }

        public async Task<SalesManTargetDTO> UpdateTargetAsync(long targetId, CreateSalesManTargetDTO dto, string? currentUserId = null)
        {
            var target = await _unitOfWork.SalesManTargets.GetByIdAsync(targetId);
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
                
                if (string.IsNullOrEmpty(target.SalesManId))
                    throw new UnauthorizedAccessException("This activity target is not associated with a salesman");
                
                if (target.SalesManId != currentUserId)
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

            await _unitOfWork.SalesManTargets.UpdateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }
            else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesManId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.SalesManId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesManTargetDTO
            {
                Id = target.Id,
                SalesManId = target.SalesManId,
                SalesManName = target.SalesManId != null 
                    ? $"{target.SalesMan?.FirstName} {target.SalesMan?.LastName}" 
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
                CreatedBySalesManName = target.TargetType == TargetType.Activity ? createdByName : null
            };
        }

        public async Task<bool> DeleteTargetAsync(long targetId)
        {
            var target = await _unitOfWork.SalesManTargets.GetByIdAsync(targetId);
            if (target == null)
                return false;

            await _unitOfWork.SalesManTargets.DeleteAsync(target);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<SalesManTargetDTO?> GetTargetAsync(long targetId)
        {
            var target = await _unitOfWork.SalesManTargets.GetByIdAsync(targetId);
            if (target == null)
                return null;

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }
            else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesManId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.SalesManId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesManTargetDTO
            {
                Id = target.Id,
                SalesManId = target.SalesManId,
                SalesManName = target.SalesManId != null 
                    ? $"{target.SalesMan?.FirstName} {target.SalesMan?.LastName}" 
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
                CreatedBySalesManName = target.TargetType == TargetType.Activity ? createdByName : null
            };
        }

        public async Task<List<SalesManTargetDTO>> GetTargetsForSalesManAsync(string salesmanId, int year)
        {
            var targets = await _unitOfWork.SalesManTargets.GetTargetsBySalesManAsync(salesmanId, year);
            var result = new List<SalesManTargetDTO>();

            foreach (var target in targets)
            {
                ApplicationUser? creator = null;
                string? createdByName = null;
                
                if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
                {
                    creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                    createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
                }
                else if (target.TargetType == TargetType.Activity && !string.IsNullOrEmpty(target.SalesManId))
                {
                    creator = await _unitOfWork.Users.GetByIdAsync(target.SalesManId);
                    createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
                }

                result.Add(new SalesManTargetDTO
                {
                    Id = target.Id,
                    SalesManId = target.SalesManId,
                    SalesManName = target.SalesManId != null 
                        ? $"{target.SalesMan?.FirstName} {target.SalesMan?.LastName}" 
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
                    CreatedBySalesManName = target.TargetType == TargetType.Activity ? createdByName : null
                });
            }

            return result;
        }

        public async Task<SalesManTargetDTO?> GetTeamTargetAsync(int year, int? quarter = null)
        {
            var target = await _unitOfWork.SalesManTargets.GetTeamTargetAsync(year, quarter);
            if (target == null)
                return null;

            ApplicationUser? creator = null;
            string? createdByName = null;
            
            if (target.TargetType == TargetType.Money && !string.IsNullOrEmpty(target.CreatedByManagerId))
            {
                creator = await _unitOfWork.Users.GetByIdAsync(target.CreatedByManagerId);
                createdByName = creator != null ? $"{creator.FirstName} {creator.LastName}" : "Unknown";
            }

            return new SalesManTargetDTO
            {
                Id = target.Id,
                SalesManId = target.SalesManId,
                SalesManName = "Team",
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
                CreatedBySalesManName = null
            };
        }

        #endregion

        #region Progress

        public async Task<SalesManProgressDTO> GetSalesManProgressAsync(string salesmanId, int year, int? quarter = null)
        {
            var statistics = await GetStatisticsAsync(salesmanId, year, quarter);
            var individualTargets = await _unitOfWork.SalesManTargets.GetTargetsBySalesManAsync(salesmanId, year, quarter);
            var allTargets = await _unitOfWork.SalesManTargets.GetAllTargetsForPeriodAsync(year, quarter);
            var teamTargets = allTargets.Where(t => t.IsTeamTarget).ToList();

            var individualMoneyTarget = individualTargets.FirstOrDefault(t => t.TargetType == TargetType.Money);
            var individualActivityTarget = individualTargets.FirstOrDefault(t => t.TargetType == TargetType.Activity);
            var teamMoneyTarget = teamTargets.FirstOrDefault(t => t.TargetType == TargetType.Money);
            var teamActivityTarget = teamTargets.FirstOrDefault(t => t.TargetType == TargetType.Activity);
            
            // Convert to DTOs
            SalesManTargetDTO? individualMoneyTargetDto = null;
            if (individualMoneyTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(individualMoneyTarget.CreatedByManagerId);
                individualMoneyTargetDto = new SalesManTargetDTO
                {
                    Id = individualMoneyTarget.Id,
                    SalesManId = individualMoneyTarget.SalesManId,
                    SalesManName = "Individual",
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

            SalesManTargetDTO? individualActivityTargetDto = null;
            if (individualActivityTarget != null)
            {
                var salesman = await _unitOfWork.Users.GetByIdAsync(individualActivityTarget.SalesManId);
                individualActivityTargetDto = new SalesManTargetDTO
                {
                    Id = individualActivityTarget.Id,
                    SalesManId = individualActivityTarget.SalesManId,
                    SalesManName = "Individual",
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
                    CreatedBySalesManName = salesman != null 
                        ? $"{salesman.FirstName} {salesman.LastName}" 
                        : "Unknown"
                };
            }

            SalesManTargetDTO? teamMoneyTargetDto = null;
            if (teamMoneyTarget != null)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(teamMoneyTarget.CreatedByManagerId);
                teamMoneyTargetDto = new SalesManTargetDTO
                {
                    Id = teamMoneyTarget.Id,
                    SalesManId = teamMoneyTarget.SalesManId,
                    SalesManName = "Team",
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

            SalesManTargetDTO? teamActivityTargetDto = null;
            if (teamActivityTarget != null)
            {
                teamActivityTargetDto = new SalesManTargetDTO
                {
                    Id = teamActivityTarget.Id,
                    SalesManId = teamActivityTarget.SalesManId,
                    SalesManName = "Team",
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
                    CreatedBySalesManName = null
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

            return new SalesManProgressDTO
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

