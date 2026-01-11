using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoitMed.DTO;
using SoitMed.Models.Equipment;
using SoitMed.Models.Identity;
using SoitMed.Models.Legacy;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Repositories;
using SoitMed.Common;

namespace SoitMed.Services
{
    /// <summary>
    /// Enhanced Maintenance Service with Customer → Equipment → Visits Workflow
    /// Integrates legacy TBS database with new itiwebapi44 database
    /// Implements complete visit history and completion tracking
    /// </summary>
    public class EnhancedMaintenanceService : IEnhancedMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TbsDbContext _tbsDbContext; // Legacy database context
        private readonly Context _newDbContext;   // New database context
        private readonly ILogger<EnhancedMaintenanceService> _logger;
        private readonly IConfiguration _configuration;

        public EnhancedMaintenanceService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            TbsDbContext tbsDbContext,
            Context newDbContext,
            ILogger<EnhancedMaintenanceService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tbsDbContext = tbsDbContext;
            _newDbContext = newDbContext;
            _logger = logger;
            _configuration = configuration;
        }

        #region Customer Management

        /// <summary>
        /// Get customer with their equipment and visit history
        /// Merges data from both legacy and new databases
        /// </summary>
        public async Task<CustomerEquipmentVisitsDTO> GetCustomerEquipmentVisitsAsync(string customerId, bool includeLegacy = true)
        {
            try
            {
                var result = new CustomerEquipmentVisitsDTO();

                // Get customer from new database
                var newCustomer = await _newDbContext.Clients
                    .FirstOrDefaultAsync(c => c.Id.ToString() == customerId || c.Phone == customerId || c.Email == customerId);

                if (newCustomer != null)
                {
                    result.Customer = new CustomerDTO
                    {
                        Id = newCustomer.Id.ToString(),
                        Name = newCustomer.Name,
                        Phone = newCustomer.Phone,
                        Email = newCustomer.Email,
                        Address = newCustomer.Address,
                        Source = "New"
                    };

                    // Get equipment from new database
                    result.Equipment = await GetCustomerEquipmentFromNewDB((int)newCustomer.Id);
                    
                    // Get visits from new database
                    result.Visits = await GetCustomerVisitsFromNewDB((int)newCustomer.Id);
                }

                // Include legacy data if requested
                if (includeLegacy)
                {
                    await IncludeLegacyCustomerData(result, customerId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer equipment visits for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Search customers across both databases
        /// </summary>
        public async Task<SoitMed.DTO.PagedResult<CustomerDTO>> SearchCustomersAsync(CustomerSearchCriteria criteria)
        {
            try
            {
                var result = new SoitMed.DTO.PagedResult<CustomerDTO>();

                // Search in new database
                var newCustomersQuery = _newDbContext.Clients.AsQueryable();

                if (!string.IsNullOrEmpty(criteria.SearchTerm))
                {
                    newCustomersQuery = newCustomersQuery.Where(c => 
                        c.Name.Contains(criteria.SearchTerm) ||
                        c.Phone.Contains(criteria.SearchTerm) ||
                        c.Email.Contains(criteria.SearchTerm));
                }

                var newCustomers = await newCustomersQuery
                    .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .Select(c => new CustomerDTO
                    {
                        Id = c.Id.ToString(),
                        Name = c.Name,
                        Phone = c.Phone,
                        Email = c.Email,
                        Address = c.Address,
                        Source = "New"
                    })
                    .ToListAsync();

                result.Items.AddRange(newCustomers);

                // Search in legacy database if requested
                if (criteria.IncludeLegacy)
                {
                    var legacyCustomersQuery = _tbsDbContext.StkCustomers.AsQueryable();

                    if (!string.IsNullOrEmpty(criteria.SearchTerm))
                    {
                        legacyCustomersQuery = legacyCustomersQuery.Where(c =>
                            c.CusName.Contains(criteria.SearchTerm) ||
                            c.CusMobile.Contains(criteria.SearchTerm) ||
                            c.CusEmail.Contains(criteria.SearchTerm));
                    }

                    var legacyCustomers = await legacyCustomersQuery
                        .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                        .Take(criteria.PageSize)
                        .Select(c => new CustomerDTO
                        {
                            Id = c.CusId.ToString(),
                            Name = c.CusName,
                            Phone = c.CusMobile,
                            Email = c.CusEmail,
                            Address = c.CusAddress,
                            Source = "Legacy"
                        })
                        .ToListAsync();

                    result.Items.AddRange(legacyCustomers);
                }

                result.TotalCount = result.Items.Count;
                result.PageNumber = criteria.PageNumber;
                result.PageSize = criteria.PageSize;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with criteria {@Criteria}", criteria);
                throw;
            }
        }

        #endregion

        #region Equipment Management

        /// <summary>
        /// Get equipment details with complete visit history
        /// </summary>
        public async Task<EquipmentVisitsDTO> GetEquipmentVisitsAsync(string equipmentIdentifier, bool includeLegacy = true)
        {
            try
            {
                var result = new EquipmentVisitsDTO();

                // Try to find equipment in new database by ID or QR code
                var newEquipment = await _newDbContext.Equipment
                    .FirstOrDefaultAsync(e => e.Id.ToString() == equipmentIdentifier || e.QRCode == equipmentIdentifier);

                if (newEquipment != null)
                {
                    result.Equipment = new EnhancedEquipmentDTO
                    {
                        Id = newEquipment.Id.ToString(),
                        Model = newEquipment.Model ?? "Unknown",
                        SerialNumber = newEquipment.QRCode ?? "N/A",
                        Status = newEquipment.Status.ToString(),
                        Location = "Unknown", // Equipment model doesn't have Location property
                        CustomerId = newEquipment.CustomerId ?? "Unknown",
                        Source = "New"
                    };

                    // Get visits for this equipment from new database
                    result.Visits = await GetEquipmentVisitsFromNewDB(newEquipment.Id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment visits for equipment {EquipmentId}", equipmentIdentifier);
                throw;
            }
        }

        /// <summary>
        /// Get customer's equipment from new database
        /// </summary>
        private async Task<List<EnhancedEquipmentDTO>> GetCustomerEquipmentFromNewDB(int customerId)
        {
            return await _newDbContext.Equipment
                .Where(e => e.CustomerId != null && e.CustomerId == customerId.ToString())
                .Select(e => new EnhancedEquipmentDTO
                {
                    Id = e.Id.ToString(),
                    Model = e.Model ?? "Unknown",
                    SerialNumber = e.QRCode ?? "N/A",
                    Status = e.Status.ToString(),
                    Location = "Unknown", // Equipment model doesn't have Location property
                    CustomerId = e.CustomerId ?? "Unknown",
                    Source = "New"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get customer's visits from new database
        /// </summary>
        private async Task<List<VisitDTO>> GetCustomerVisitsFromNewDB(int customerId)
        {
            return await _newDbContext.MaintenanceVisits
                .Join(_newDbContext.MaintenanceRequests, v => v.MaintenanceRequestId, mr => mr.Id, (v, mr) => new { v, mr })
                .Where(x => x.mr.CustomerId == customerId.ToString())
                .Select(x => new VisitDTO
                {
                    Id = x.v.Id.ToString(),
                    VisitDate = x.v.VisitDate,
                    ScheduledDate = x.v.ScheduledDate,
                    Status = x.v.Status.ToString(),
                    EngineerName = x.v.EngineerId, // TODO: Get engineer name
                    Report = x.v.Report,
                    ActionsTaken = x.v.ActionsTaken,
                    PartsUsed = x.v.PartsUsed,
                    ServiceFee = x.v.ServiceFee,
                    Outcome = x.v.Outcome.ToString(),
                    Source = "New"
                })
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get equipment visits from new database
        /// </summary>
        private async Task<List<VisitDTO>> GetEquipmentVisitsFromNewDB(int equipmentId)
        {
            return await _newDbContext.MaintenanceVisits
                .Where(v => v.DeviceId == equipmentId)
                .Select(v => new VisitDTO
                {
                    Id = v.Id.ToString(),
                    VisitDate = v.VisitDate,
                    ScheduledDate = v.ScheduledDate,
                    Status = v.Status.ToString(),
                    EngineerName = v.EngineerId, // TODO: Get engineer name
                    Report = v.Report,
                    ActionsTaken = v.ActionsTaken,
                    PartsUsed = v.PartsUsed,
                    ServiceFee = v.ServiceFee,
                    Outcome = v.Outcome.ToString(),
                    Source = "New"
                })
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        #endregion

        #region Legacy Data Integration

        /// <summary>
        /// Include legacy customer data from TBS database
        /// </summary>
        private async Task IncludeLegacyCustomerData(CustomerEquipmentVisitsDTO result, string customerId)
        {
            try
            {
                // Try to find customer in legacy database
                var legacyCustomer = await _tbsDbContext.StkCustomers
                    .FirstOrDefaultAsync(c => c.CusId.ToString() == customerId || 
                                             c.CusMobile == customerId || 
                                             c.CusEmail == customerId);

                if (legacyCustomer != null)
                {
                    // If no customer found in new DB, use legacy customer
                    if (result.Customer == null)
                    {
                        result.Customer = new CustomerDTO
                        {
                            Id = legacyCustomer.CusId.ToString(),
                            Name = legacyCustomer.CusName,
                            Phone = legacyCustomer.CusMobile,
                            Email = legacyCustomer.CusEmail,
                            Address = legacyCustomer.CusAddress,
                            Source = "Legacy"
                        };
                    }

                    // Get legacy equipment for this customer
                    var legacyEquipment = await GetCustomerEquipmentFromLegacyDB(legacyCustomer.CusId);
                    result.Equipment.AddRange(legacyEquipment);

                    // Get legacy visits for this customer
                    var legacyVisits = await GetCustomerVisitsFromLegacyDB(legacyCustomer.CusId);
                    result.Visits.AddRange(legacyVisits);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error including legacy customer data for customer {CustomerId}", customerId);
            }
        }

        /// <summary>
        /// Include legacy equipment data from TBS database
        /// </summary>
        private async Task IncludeLegacyEquipmentData(EquipmentVisitsDTO result, string equipmentIdentifier)
        {
            try
            {
                // For now, skip legacy equipment integration to avoid DTO conflicts
                // This can be implemented later with proper DTO mapping
                _logger.LogInformation("Legacy equipment integration skipped for equipment {EquipmentId}", equipmentIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error including legacy equipment data for equipment {EquipmentId}", equipmentIdentifier);
            }
        }

        /// <summary>
        /// Get customer's equipment from legacy TBS database
        /// </summary>
        private async Task<List<EnhancedEquipmentDTO>> GetCustomerEquipmentFromLegacyDB(int customerId)
        {
            return await _tbsDbContext.StkOrderOutItems
                .Join(_tbsDbContext.StkItems, ooi => ooi.ItemId, item => item.ItemId, (ooi, item) => new { ooi, item })
                .Join(_tbsDbContext.MntVisitingReports, x => x.ooi.OoiId, vr => vr.OoiId, (x, vr) => new { x.ooi, x.item, vr })
                .Join(_tbsDbContext.MntVisitings, x => x.vr.VisitingId, v => v.VisitingId, (x, v) => new { x.ooi, x.item, x.vr, v })
                .Where(x => x.v.CusId == customerId)
                .Select(x => new EnhancedEquipmentDTO
                {
                    Id = x.ooi.OoiId.ToString(),
                    Model = x.item.ItemNameAr,
                    SerialNumber = x.ooi.SerialNum ?? "N/A",
                    Status = "Active",
                    Location = x.ooi.DevicePlace ?? "Unknown",
                    CustomerId = customerId.ToString(),
                    Source = "Legacy"
                })
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Get customer's visits from legacy TBS database
        /// </summary>
        private async Task<List<VisitDTO>> GetCustomerVisitsFromLegacyDB(int customerId)
        {
            return await _tbsDbContext.MntVisitings
                .Where(v => v.CusId == customerId)
                .Join(_tbsDbContext.MntVisitingReports, v => v.VisitingId, vr => vr.VisitingId, (v, vr) => new { v, vr })
                .Join(_tbsDbContext.StkOrderOutItems, x => x.vr.OoiId, ooi => ooi.OoiId, (x, ooi) => new { x.v, x.vr, ooi })
                .Join(_tbsDbContext.StkItems, x => x.ooi.ItemId, item => item.ItemId, (x, item) => new { x.v, x.vr, x.ooi, item })
                .Select(x => new VisitDTO
                {
                    Id = x.v.VisitingId.ToString(),
                    VisitDate = x.v.VisitingDate ?? DateTime.MinValue,
                    ScheduledDate = x.v.VisitingDateDefault ?? DateTime.MinValue,
                    Status = GetLegacyVisitStatus(x.v, x.vr),
                    EngineerName = x.v.EmpCode != null ? x.v.EmpCode.ToString() : "Unknown",
                    Report = x.vr.ReportDecription,
                    ActionsTaken = x.v.Notes,
                    PartsUsed = "", // Legacy doesn't track parts used separately
                    ServiceFee = x.v.VisitingValue,
                    Outcome = GetLegacyVisitOutcome(x.vr),
                    Source = "Legacy"
                })
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get equipment visits from legacy TBS database
        /// </summary>
        private async Task<List<VisitDTO>> GetEquipmentVisitsFromLegacyDB(int equipmentId)
        {
            return await _tbsDbContext.MntVisitingReports
                .Where(vr => vr.OoiId == equipmentId)
                .Join(_tbsDbContext.MntVisitings, vr => vr.VisitingId, v => v.VisitingId, (vr, v) => new { vr, v })
                .Join(_tbsDbContext.StkOrderOutItems, x => x.vr.OoiId, ooi => ooi.OoiId, (x, ooi) => new { x.vr, x.v, ooi })
                .Join(_tbsDbContext.StkItems, x => x.ooi.ItemId, item => item.ItemId, (x, item) => new { x.vr, x.v, x.ooi, item })
                .Select(x => new VisitDTO
                {
                    Id = x.v.VisitingId.ToString(),
                    VisitDate = x.v.VisitingDate ?? DateTime.MinValue,
                    ScheduledDate = x.v.VisitingDateDefault ?? DateTime.MinValue,
                    Status = GetLegacyVisitStatus(x.v, x.vr),
                    EngineerName = x.v.EmpCode != null ? x.v.EmpCode.ToString() : "Unknown",
                    Report = x.vr.ReportDecription,
                    ActionsTaken = x.v.Notes,
                    PartsUsed = "", // Legacy doesn't track parts used separately
                    ServiceFee = x.v.VisitingValue,
                    Outcome = GetLegacyVisitOutcome(x.vr),
                    Source = "Legacy"
                })
                .OrderByDescending(v => v.VisitDate)
                .ToListAsync();
        }

        #endregion

        #region Visit Completion Logic

        /// <summary>
        /// Complete a maintenance visit with comprehensive tracking
        /// Updates both legacy and new databases as needed
        /// </summary>
        public async Task<VisitCompletionDTO> CompleteVisitAsync(CompleteVisitDTO dto)
        {
            try
            {
                var result = new VisitCompletionDTO();

                // Handle new database visit completion
                if (dto.Source == "New" || string.IsNullOrEmpty(dto.Source))
                {
                    result = await CompleteNewVisitAsync(dto);
                }
                else if (dto.Source == "Legacy")
                {
                    result = await CompleteLegacyVisitAsync(dto);
                }

                // Sync completion status across databases if needed
                await SyncVisitCompletionAsync(dto);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing visit {@VisitDto}", dto);
                throw;
            }
        }

        /// <summary>
        /// Get visit completion statistics for a customer
        /// </summary>
        public async Task<CustomerVisitStatsDTO> GetCustomerVisitStatsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var result = new CustomerVisitStatsDTO
                {
                    CustomerId = customerId,
                    StartDate = startDate ?? DateTime.MinValue,
                    EndDate = endDate ?? DateTime.MaxValue
                };

                // Get stats from new database
                var newStats = await GetCustomerVisitStatsFromNewDB(customerId, startDate, endDate);
                result.TotalVisits += newStats.TotalVisits;
                result.CompletedVisits += newStats.CompletedVisits;
                result.PendingVisits += newStats.PendingVisits;
                result.CancelledVisits += newStats.CancelledVisits;
                result.TotalRevenue += newStats.TotalRevenue;

                // Get stats from legacy database
                var legacyStats = await GetCustomerVisitStatsFromLegacyDB(customerId, startDate, endDate);
                result.TotalVisits += legacyStats.TotalVisits;
                result.CompletedVisits += legacyStats.CompletedVisits;
                result.PendingVisits += legacyStats.PendingVisits;
                result.CancelledVisits += legacyStats.CancelledVisits;
                result.TotalRevenue += legacyStats.TotalRevenue;

                result.CompletionRate = result.TotalVisits > 0 ? 
                    (decimal)result.CompletedVisits / result.TotalVisits * 100 : 0;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer visit stats for customer {CustomerId}", customerId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private string GetLegacyVisitStatus(TbsVisiting visit, TbsVisitingReport report)
        {
            if (visit.IsCancelled == true)
                return "Cancelled";
            
            if (report?.RepVisitStatusId.HasValue == true)
            {
                return report.RepVisitStatusId.Value switch
                {
                    1 => "Completed",
                    2 => "Pending",
                    3 => "InProgress",
                    4 => "Cancelled",
                    _ => "Unknown"
                };
            }

            return visit.VisitingDate.HasValue && visit.VisitingDate <= DateTime.Now ? "Completed" : "Pending";
        }

        private string GetLegacyVisitOutcome(TbsVisitingReport report)
        {
            if (report?.VisitingResultId.HasValue == true)
            {
                return report.VisitingResultId.Value switch
                {
                    1 => "Completed",
                    2 => "NeedsSecondVisit",
                    3 => "NeedsSparePart",
                    4 => "CannotComplete",
                    _ => "Unknown"
                };
            }

            return "Unknown";
        }

        private async Task<VisitCompletionDTO> CompleteNewVisitAsync(CompleteVisitDTO dto)
        {
            var visit = await _newDbContext.MaintenanceVisits
                .FirstOrDefaultAsync(v => v.Id.ToString() == dto.VisitId);

            if (visit == null)
                throw new ArgumentException("Visit not found", nameof(dto.VisitId));

            // Update visit details
            visit.Report = dto.Report;
            visit.ActionsTaken = dto.ActionsTaken;
            visit.PartsUsed = dto.PartsUsed;
            visit.ServiceFee = dto.ServiceFee;
            visit.Outcome = Enum.Parse<MaintenanceVisitOutcome>(dto.Outcome);
            visit.Status = VisitStatus.Completed;
            visit.CompletedAt = DateTime.UtcNow;

            await _newDbContext.SaveChangesAsync();

            return new VisitCompletionDTO
            {
                Success = true,
                VisitId = visit.Id.ToString(),
                CompletionDate = visit.CompletedAt.Value,
                Status = "Completed",
                Message = "Visit completed successfully"
            };
        }

        private async Task<VisitCompletionDTO> CompleteLegacyVisitAsync(CompleteVisitDTO dto)
        {
            // Note: Legacy database is typically read-only
            // This would require special permissions or separate update logic
            _logger.LogWarning("Legacy visit completion attempted for visit {VisitId}", dto.VisitId);

            return new VisitCompletionDTO
            {
                Success = false,
                VisitId = dto.VisitId,
                Message = "Legacy database is read-only. Visit completion not supported."
            };
        }

        private async Task SyncVisitCompletionAsync(CompleteVisitDTO dto)
        {
            // Logic to sync completion status between databases
            // This would be implemented based on business requirements
            _logger.LogInformation("Syncing visit completion for visit {VisitId}", dto.VisitId);
        }

        private async Task<CustomerVisitStatsDTO> GetCustomerVisitStatsFromNewDB(string customerId, DateTime? startDate, DateTime? endDate)
        {
            var query = _newDbContext.MaintenanceVisits
                .Join(_newDbContext.MaintenanceRequests, v => v.MaintenanceRequestId, mr => mr.Id, (v, mr) => new { v, mr })
                .Where(x => x.mr.CustomerId == customerId);

            if (startDate.HasValue)
                query = query.Where(x => x.v.VisitDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.v.VisitDate <= endDate.Value);

            var visits = await query.ToListAsync();

            return new CustomerVisitStatsDTO
            {
                CustomerId = customerId,
                StartDate = startDate ?? DateTime.MinValue,
                EndDate = endDate ?? DateTime.MaxValue,
                TotalVisits = visits.Count,
                CompletedVisits = visits.Count(x => x.v.Status == VisitStatus.Completed),
                PendingVisits = visits.Count(x => x.v.Status == VisitStatus.Scheduled),
                CancelledVisits = visits.Count(x => x.v.Status == VisitStatus.Cancelled),
                TotalRevenue = visits.Where(x => x.v.ServiceFee.HasValue).Sum(x => x.v.ServiceFee.Value)
            };
        }

        private async Task<CustomerVisitStatsDTO> GetCustomerVisitStatsFromLegacyDB(string customerId, DateTime? startDate, DateTime? endDate)
        {
            var query = _tbsDbContext.MntVisitings.Where(v => v.CusId.ToString() == customerId);

            if (startDate.HasValue)
                query = query.Where(v => v.VisitingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(v => v.VisitingDate <= endDate.Value);

            var visits = await query.ToListAsync();

            return new CustomerVisitStatsDTO
            {
                TotalVisits = visits.Count,
                CompletedVisits = visits.Count(v => v.VisitingDate.HasValue && v.VisitingDate <= DateTime.Now && v.IsCancelled != true),
                PendingVisits = visits.Count(v => !v.VisitingDate.HasValue || v.VisitingDate > DateTime.Now),
                CancelledVisits = visits.Count(v => v.IsCancelled == true),
                TotalRevenue = visits.Where(v => v.VisitingValue.HasValue).Sum(v => v.VisitingValue.Value)
            };
        }

        #endregion
    }
}
