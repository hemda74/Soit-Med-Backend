using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Legacy;
using SoitMed.Repositories;
using System.Text.Json;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for retrieving customer machines data from ITIWebApi44
    /// Replaces Media API endpoint that queries TBS database
    /// </summary>
    public class CustomerMachinesService : ICustomerMachinesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly ILegacyEmployeeService _legacyEmployeeService;
        private readonly ILogger<CustomerMachinesService> _logger;
        private readonly IConfiguration _configuration;

        public CustomerMachinesService(
            IUnitOfWork unitOfWork,
            Context context,
            ILegacyEmployeeService legacyEmployeeService,
            ILogger<CustomerMachinesService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _legacyEmployeeService = legacyEmployeeService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all machines for a customer by customer ID
        /// GENERALIZED APPROACH: Queries TBS directly (like MediaApi) to get ALL machines
        /// regardless of whether they exist in Equipment table
        /// </summary>
        public async Task<CustomerMachinesDto?> GetMachinesByCustomerIdAsync(long customerId)
        {
            try
            {
                // Step 1: Get customer (single query, AsNoTracking)
                // Try to match by Id (long) or LegacyCustomerId (int?)
                var customer = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == customerId || 
                                             (c.LegacyCustomerId.HasValue && 
                                              customerId <= int.MaxValue && 
                                              c.LegacyCustomerId.Value == (int)customerId));

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", customerId);
                    return null;
                }

                // Step 2: Get machines from TBS database (PRIMARY SOURCE - like MediaApi)
                // This ensures we get ALL machines, even if not in Equipment table
                List<TbsMachineInfo> tbsMachines = new List<TbsMachineInfo>();
                
                if (customer.LegacyCustomerId.HasValue)
                {
                    try
                    {
                        tbsMachines = await GetMachinesFromTBSAsync(customer.LegacyCustomerId.Value);
                        _logger.LogInformation("Found {Count} machines from TBS for customer {CustomerId} (LegacyCustomerId: {LegacyCustomerId})",
                            tbsMachines.Count, customerId, customer.LegacyCustomerId.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error querying TBS database for customer {CustomerId} (LegacyCustomerId: {LegacyCustomerId})",
                            customerId, customer.LegacyCustomerId);
                        // Continue - we'll still check Equipment table
                    }
                }

                // Step 3: Get machines from Equipment table (SECONDARY SOURCE)
                var equipmentIds = await GetAllEquipmentIdsForCustomerAsync(customer);
                var equipmentMachines = new List<TbsMachineInfo>();

                if (equipmentIds.Any())
                {
                    // Get equipment and map to TbsMachineInfo
                    var equipment = await _context.Equipment
                        .AsNoTracking()
                        .Where(e => equipmentIds.Contains(e.Id) && e.IsActive)
                        .ToListAsync();

                    equipmentMachines = equipment.Select(e =>
                    {
                        int machineId = e.Id;
                        if (e.LegacySourceId != null)
                        {
                            if (int.TryParse(e.LegacySourceId, out var ooiId))
                            {
                                machineId = ooiId;
                            }
                        }
                        return new TbsMachineInfo
                        {
                            MachineId = machineId,
                            SerialNumber = e.QRCode ?? e.LegacySourceId ?? string.Empty,
                            ModelName = e.Model,
                            ModelNameEn = e.Manufacturer,
                            ItemCode = e.LegacySourceId,
                            EquipmentId = e.Id // Track Equipment table ID for visit/media lookup
                        };
                    }).ToList();
                    _logger.LogInformation("Found {Count} machines from Equipment table for customer {CustomerId}",
                        equipmentMachines.Count, customerId);
                }

                // Step 4: Combine and deduplicate machines (by MachineId/OOI_ID)
                // TBS machines take priority (they have full details)
                var allMachines = tbsMachines
                    .Concat(equipmentMachines.Where(e => !tbsMachines.Any(t => t.MachineId == e.MachineId)))
                    .GroupBy(m => m.MachineId)
                    .Select(g => g.First())
                    .ToList();

                if (!allMachines.Any())
                {
                    _logger.LogInformation("No machines found for customer {CustomerId}", customerId);
                    return new CustomerMachinesDto
                    {
                        CustomerId = (int)customerId,
                        CustomerName = customer.Name,
                        CustomerAddress = customer.Address,
                        CustomerPhone = customer.Phone,
                        MachineCount = 0,
                        Machines = new List<MachineDto>()
                    };
                }

                // Step 5: Get visit counts (from both TBS and new database)
                var visitCounts = await GetVisitCountsForMachinesAsync(allMachines, customer.LegacyCustomerId);

                // Step 6: Get media files (from both TBS and new database)
                var mediaData = await GetMediaFilesForMachinesAsync(allMachines, customer.LegacyCustomerId);

                // Step 7: Map to DTOs
                return MapToCustomerMachinesDto(customer, allMachines, visitCounts, mediaData, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving machines for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Get all equipment IDs for a customer through multiple relationship paths
        /// </summary>
        private async Task<HashSet<int>> GetAllEquipmentIdsForCustomerAsync(Models.Client customer)
        {
            var equipmentIds = new HashSet<int>();

            // Path 1: Direct link via Equipment.CustomerId (if customer has RelatedUserId)
            if (!string.IsNullOrEmpty(customer.RelatedUserId))
            {
                var directEquipment = await _context.Equipment
                    .AsNoTracking()
                    .Where(e => e.CustomerId == customer.RelatedUserId && e.IsActive)
                    .Select(e => e.Id)
                    .ToListAsync();

                foreach (var id in directEquipment)
                {
                    equipmentIds.Add(id);
                }
            }

            // Path 2: Through MaintenanceVisits where CustomerId matches RelatedUserId
            if (!string.IsNullOrEmpty(customer.RelatedUserId))
            {
                var equipmentFromVisits = await _context.MaintenanceVisits
                    .AsNoTracking()
                    .Where(mv => mv.CustomerId == customer.RelatedUserId && mv.IsActive)
                    .Select(mv => mv.DeviceId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in equipmentFromVisits)
                {
                    equipmentIds.Add(id);
                }
            }

            // Path 3: Through MaintenanceRequests where CustomerId matches RelatedUserId
            if (!string.IsNullOrEmpty(customer.RelatedUserId))
            {
                var equipmentFromRequests = await _context.MaintenanceRequests
                    .AsNoTracking()
                    .Where(mr => mr.CustomerId == customer.RelatedUserId)
                    .Select(mr => mr.EquipmentId)
                    .Distinct()
                    .ToListAsync();

                foreach (var id in equipmentFromRequests)
                {
                    equipmentIds.Add(id);
                }
            }

            // Path 4: Through Hospital (if client's RelatedUserId is linked to Doctor/Technician with HospitalId)
            if (!string.IsNullOrEmpty(customer.RelatedUserId))
            {
                // Get hospital ID from Doctor relationship
                var doctorHospitalId = await (from d in _context.Doctors.AsNoTracking()
                                               where d.UserId == customer.RelatedUserId
                                               select d.HospitalId)
                                               .FirstOrDefaultAsync();

                // Get hospital ID from Technician relationship if not found in Doctor
                if (string.IsNullOrEmpty(doctorHospitalId))
                {
                    doctorHospitalId = await (from t in _context.Technicians.AsNoTracking()
                                              where t.UserId == customer.RelatedUserId
                                              select t.HospitalId)
                                              .FirstOrDefaultAsync();
                }

                if (!string.IsNullOrEmpty(doctorHospitalId))
                {
                    var equipmentFromHospital = await _context.Equipment
                        .AsNoTracking()
                        .Where(e => e.HospitalId == doctorHospitalId && e.IsActive)
                        .Select(e => e.Id)
                        .ToListAsync();

                    foreach (var id in equipmentFromHospital)
                    {
                        equipmentIds.Add(id);
                    }

                    _logger.LogInformation("Found {Count} equipment via Hospital for customer {CustomerId}",
                        equipmentFromHospital.Count, customer.Id);
                }

                // Path 5: Through RepairRequests (if client's RelatedUserId is linked to Doctor/Technician)
                // Get Doctor ID
                var doctorId = await (from d in _context.Doctors.AsNoTracking()
                                      where d.UserId == customer.RelatedUserId
                                      select (int?)d.DoctorId)
                                      .FirstOrDefaultAsync();

                // Get Technician ID if not found in Doctor
                int? technicianId = null;
                if (!doctorId.HasValue)
                {
                    technicianId = await (from t in _context.Technicians.AsNoTracking()
                                          where t.UserId == customer.RelatedUserId
                                          select (int?)t.TechnicianId)
                                          .FirstOrDefaultAsync();
                }

                // Get equipment from RepairRequests
                if (doctorId.HasValue || technicianId.HasValue)
                {
                    var equipmentFromRepairRequests = await _context.RepairRequests
                        .AsNoTracking()
                        .Where(rr => (doctorId.HasValue && rr.DoctorId == doctorId.Value) ||
                                     (technicianId.HasValue && rr.TechnicianId == technicianId.Value))
                        .Select(rr => rr.EquipmentId)
                        .Distinct()
                        .ToListAsync();

                    foreach (var id in equipmentFromRepairRequests)
                    {
                        equipmentIds.Add(id);
                    }

                    if (equipmentFromRepairRequests.Any())
                    {
                        _logger.LogInformation("Found {Count} equipment via RepairRequests for customer {CustomerId}",
                            equipmentFromRepairRequests.Count, customer.Id);
                    }
                }
            }


            // Note: Contracts don't directly link to Equipment
            // Equipment is referenced in offers but not as a foreign key relationship
            // If needed, equipment can be linked through other paths above

            return equipmentIds;
        }

        /// <summary>
        /// Get full machine details from TBS database for a legacy customer
        /// GENERALIZED APPROACH: Queries TBS directly (like MediaApi) to get ALL machines
        /// Returns machines with full details (serial, model, item code) even if not in Equipment table
        /// </summary>
        private async Task<List<TbsMachineInfo>> GetMachinesFromTBSAsync(int legacyCustomerId)
        {
            var allMachines = new List<TbsMachineInfo>();

            try
            {
                var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                if (string.IsNullOrEmpty(tbsConnectionString))
                {
                    _logger.LogWarning("TbsConnection string not configured, cannot query TBS database");
                    return allMachines;
                }

                var optionsBuilder = new DbContextOptionsBuilder<TbsDbContext>();
                optionsBuilder.UseSqlServer(tbsConnectionString);

                using var tbsContext = new TbsDbContext(optionsBuilder.Options);

                // Verify customer exists in TBS
                var legacyCustomer = await tbsContext.StkCustomers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CusId == legacyCustomerId);

                if (legacyCustomer == null)
                {
                    _logger.LogWarning("Legacy customer {LegacyCustomerId} not found in TBS database", legacyCustomerId);
                    return allMachines;
                }

                _logger.LogInformation("Querying TBS database for customer {LegacyCustomerId} ({CustomerName})", 
                    legacyCustomerId, legacyCustomer.CusName);

                // 1. Get machines through visits (where visit's CusId = customerId)
                var machinesFromVisits = await (from v in tbsContext.MntVisitings.AsNoTracking()
                                               where v.CusId == legacyCustomerId
                                               join vr in tbsContext.MntVisitingReports.AsNoTracking() on v.VisitingId equals vr.VisitingId
                                               join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on vr.OoiId equals ooi.OoiId
                                               join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                               select new TbsMachineInfo
                                               {
                                                   MachineId = ooi.OoiId,
                                                   SerialNumber = ooi.SerialNum ?? string.Empty,
                                                   ModelName = item.ItemNameAr,
                                                   ModelNameEn = item.ItemNameEn,
                                                   ItemCode = item.ItemCode ?? string.Empty
                                               })
                                               .ToListAsync();

                allMachines.AddRange(machinesFromVisits);
                _logger.LogInformation("Found {Count} machines via visits for legacy customer {LegacyCustomerId}", 
                    machinesFromVisits.Count, legacyCustomerId);

                // 2. Get machines through contract-linked visits (ALL visits for contracts of this customer)
                var machinesFromContractVisits = await (from contract in tbsContext.MntMaintenanceContracts.AsNoTracking()
                                                       where contract.CusId == legacyCustomerId
                                                       join v in tbsContext.MntVisitings.AsNoTracking() on contract.ContractId equals v.ContractId
                                                       join vr in tbsContext.MntVisitingReports.AsNoTracking() on v.VisitingId equals vr.VisitingId
                                                       join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on vr.OoiId equals ooi.OoiId
                                                       join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                       where !string.IsNullOrEmpty(ooi.SerialNum)
                                                       select new TbsMachineInfo
                                                       {
                                                           MachineId = ooi.OoiId,
                                                           SerialNumber = ooi.SerialNum ?? string.Empty,
                                                           ModelName = item.ItemNameAr,
                                                           ModelNameEn = item.ItemNameEn,
                                                           ItemCode = item.ItemCode ?? string.Empty
                                                       })
                                                       .ToListAsync();

                allMachines.AddRange(machinesFromContractVisits);
                _logger.LogInformation("Found {Count} machines via contract visits for legacy customer {LegacyCustomerId}", 
                    machinesFromContractVisits.Count, legacyCustomerId);

                // 3. Get machines through maintenance contract items (PRIMARY SOURCE)
                var machinesFromContractItems = await (from contract in tbsContext.MntMaintenanceContracts.AsNoTracking()
                                                      where contract.CusId == legacyCustomerId
                                                      join mci in tbsContext.MntMaintenanceContractItems.AsNoTracking() on contract.ContractId equals mci.ContractId
                                                      where mci.OoiId.HasValue
                                                      join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on mci.OoiId!.Value equals ooi.OoiId
                                                      join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                      select new TbsMachineInfo
                                                      {
                                                          MachineId = ooi.OoiId,
                                                          SerialNumber = ooi.SerialNum ?? string.Empty,
                                                          ModelName = item.ItemNameAr,
                                                          ModelNameEn = item.ItemNameEn,
                                                          ItemCode = item.ItemCode ?? string.Empty
                                                      })
                                                      .ToListAsync();

                allMachines.AddRange(machinesFromContractItems);
                _logger.LogInformation("Found {Count} machines via contract items for legacy customer {LegacyCustomerId}", 
                    machinesFromContractItems.Count, legacyCustomerId);

                // 4. Get machines through sales invoices (handle SC_ID column error gracefully)
                try
                {
                    var machinesFromSales = await (from si in tbsContext.StkSalesInvs.AsNoTracking()
                                                  where si.ScId.HasValue
                                                  join sc in tbsContext.StkSalesContracts.AsNoTracking() on si.ScId!.Value equals sc.ScId
                                                  where sc.CusId == legacyCustomerId
                                                  join oo in tbsContext.StkOrderOuts.AsNoTracking() on (long)si.SiId equals oo.SiId
                                                  where oo.SiId.HasValue
                                                  join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on oo.OoId equals ooi.OoiId
                                                  join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                  where !string.IsNullOrEmpty(ooi.SerialNum)
                                                  select new TbsMachineInfo
                                                  {
                                                      MachineId = ooi.OoiId,
                                                      SerialNumber = ooi.SerialNum ?? string.Empty,
                                                      ModelName = item.ItemNameAr,
                                                      ModelNameEn = item.ItemNameEn,
                                                      ItemCode = item.ItemCode ?? string.Empty
                                                  })
                                                  .ToListAsync();

                    allMachines.AddRange(machinesFromSales);
                    _logger.LogInformation("Found {Count} machines via sales invoices for legacy customer {LegacyCustomerId}", 
                        machinesFromSales.Count, legacyCustomerId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error querying sales invoices from TBS (SC_ID column may not exist) for customer {LegacyCustomerId}", 
                        legacyCustomerId);
                    // Continue - SC_ID column might not exist in all TBS databases
                }

                // 5. Get machines through order out (direct customer link)
                var machinesFromOrderOut = await (from oo in tbsContext.StkOrderOuts.AsNoTracking()
                                                 where oo.CusId == legacyCustomerId
                                                 join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on oo.OoId equals ooi.OoId
                                                 where !string.IsNullOrEmpty(ooi.SerialNum)
                                                 join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                 select new TbsMachineInfo
                                                 {
                                                     MachineId = ooi.OoiId,
                                                     SerialNumber = ooi.SerialNum ?? string.Empty,
                                                     ModelName = item.ItemNameAr,
                                                     ModelNameEn = item.ItemNameEn,
                                                     ItemCode = item.ItemCode ?? string.Empty
                                                 })
                                                 .ToListAsync();

                allMachines.AddRange(machinesFromOrderOut);

                _logger.LogInformation("Found {Count} machines via order out (على أمر توريد) for legacy customer {LegacyCustomerId}", 
                    machinesFromOrderOut.Count, legacyCustomerId);

                // Deduplicate by MachineId (OOI_ID)
                var uniqueMachines = allMachines
                    .GroupBy(m => m.MachineId)
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation("Total unique machines found in TBS for customer {LegacyCustomerId}: {Count} (from {TotalCount} total records)", 
                    legacyCustomerId, uniqueMachines.Count, allMachines.Count);

                return uniqueMachines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying TBS database for legacy customer {LegacyCustomerId}", legacyCustomerId);
                return allMachines; // Return what we found so far
            }
        }

        /// <summary>
        /// Get media files for all equipment in batch
        /// </summary>
        private async Task<Dictionary<int, List<MediaFileDto>>> GetMediaFilesForEquipmentBatchAsync(HashSet<int> equipmentIds)
        {
            var mediaData = new Dictionary<int, List<MediaFileDto>>();

            // Get all visit reports for equipment in one query
            var visitReports = await (from vr in _context.VisitReports.AsNoTracking()
                                     join mv in _context.MaintenanceVisits.AsNoTracking() on vr.VisitId equals mv.Id
                                     where equipmentIds.Contains(mv.DeviceId) && !string.IsNullOrEmpty(vr.MediaUrls)
                                     select new
                                     {
                                         mv.DeviceId,
                                         vr.MediaUrls
                                     })
                                     .ToListAsync();

            // Parse JSON and transform to media file DTOs
            foreach (var report in visitReports)
            {
                if (string.IsNullOrWhiteSpace(report.MediaUrls))
                    continue;

                try
                {
                    var mediaUrls = JsonSerializer.Deserialize<List<string>>(report.MediaUrls);
                    if (mediaUrls == null || !mediaUrls.Any())
                        continue;

                    if (!mediaData.ContainsKey(report.DeviceId))
                    {
                        mediaData[report.DeviceId] = new List<MediaFileDto>();
                    }

                    foreach (var fileName in mediaUrls)
                    {
                        if (string.IsNullOrWhiteSpace(fileName))
                            continue;

                        var mediaFile = new MediaFileDto
                        {
                            FileName = fileName,
                            FileUrl = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}",
                            FileType = GetFileType(fileName),
                            IsImage = IsImageFile(fileName),
                            IsPdf = IsPdfFile(fileName)
                        };

                        mediaData[report.DeviceId].Add(mediaFile);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Error parsing MediaUrls JSON for device {DeviceId}: {MediaUrls}", 
                        report.DeviceId, report.MediaUrls);
                }
            }

            return mediaData;
        }

        /// <summary>
        /// Get visit counts for machines (from both TBS and new database)
        /// </summary>
        private async Task<Dictionary<int, int>> GetVisitCountsForMachinesAsync(List<TbsMachineInfo> machines, int? legacyCustomerId)
        {
            var visitCounts = new Dictionary<int, int>();

            // Get visit counts from new database (if machines have EquipmentId)
            var equipmentIds = machines.Where(m => m.EquipmentId.HasValue).Select(m => m.EquipmentId!.Value).ToList();
            if (equipmentIds.Any())
            {
                var newDbVisitCounts = await _context.MaintenanceVisits
                    .AsNoTracking()
                    .Where(mv => equipmentIds.Contains(mv.DeviceId) && mv.IsActive)
                    .GroupBy(mv => mv.DeviceId)
                    .Select(g => new { DeviceId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.DeviceId, x => x.Count);

                foreach (var kvp in newDbVisitCounts)
                {
                    var machine = machines.FirstOrDefault(m => m.EquipmentId == kvp.Key);
                    if (machine != null)
                    {
                        visitCounts[machine.MachineId] = kvp.Value;
                    }
                }
            }

            // Get visit counts from TBS (for all machines by OOI_ID)
            if (legacyCustomerId.HasValue)
            {
                try
                {
                    var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                    if (!string.IsNullOrEmpty(tbsConnectionString))
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<TbsDbContext>();
                        optionsBuilder.UseSqlServer(tbsConnectionString);

                        using var tbsContext = new TbsDbContext(optionsBuilder.Options);

                        var machineIds = machines.Select(m => m.MachineId).ToList();
                        var tbsVisitCounts = await (from v in tbsContext.MntVisitings.AsNoTracking()
                                                   where v.CusId == legacyCustomerId.Value
                                                   join vr in tbsContext.MntVisitingReports.AsNoTracking() on v.VisitingId equals vr.VisitingId
                                                   where machineIds.Contains(vr.OoiId)
                                                   group vr by vr.OoiId into g
                                                   select new { MachineId = g.Key, Count = g.Count() })
                                                   .ToDictionaryAsync(x => x.MachineId, x => x.Count);

                        foreach (var kvp in tbsVisitCounts)
                        {
                            visitCounts[kvp.Key] = visitCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting visit counts from TBS for legacy customer {LegacyCustomerId}", legacyCustomerId);
                }
            }

            return visitCounts;
        }

        /// <summary>
        /// Get media files for machines (from both TBS and new database)
        /// </summary>
        private async Task<Dictionary<int, List<MediaFileDto>>> GetMediaFilesForMachinesAsync(List<TbsMachineInfo> machines, int? legacyCustomerId)
        {
            var mediaData = new Dictionary<int, List<MediaFileDto>>();

            // Get media files from new database (if machines have EquipmentId)
            var equipmentIds = machines.Where(m => m.EquipmentId.HasValue).Select(m => m.EquipmentId!.Value).ToList();
            if (equipmentIds.Any())
            {
                var visitReports = await (from vr in _context.VisitReports.AsNoTracking()
                                         join mv in _context.MaintenanceVisits.AsNoTracking() on vr.VisitId equals mv.Id
                                         where equipmentIds.Contains(mv.DeviceId) && !string.IsNullOrEmpty(vr.MediaUrls)
                                         select new
                                         {
                                             mv.DeviceId,
                                             vr.MediaUrls
                                         })
                                         .ToListAsync();

                foreach (var report in visitReports)
                {
                    var machine = machines.FirstOrDefault(m => m.EquipmentId == report.DeviceId);
                    if (machine == null) continue;

                    try
                    {
                        var mediaUrls = JsonSerializer.Deserialize<List<string>>(report.MediaUrls);
                        if (mediaUrls == null || !mediaUrls.Any()) continue;

                        if (!mediaData.ContainsKey(machine.MachineId))
                        {
                            mediaData[machine.MachineId] = new List<MediaFileDto>();
                        }

                        foreach (var fileName in mediaUrls)
                        {
                            if (string.IsNullOrWhiteSpace(fileName)) continue;

                            mediaData[machine.MachineId].Add(new MediaFileDto
                            {
                                FileName = fileName,
                                FileUrl = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}",
                                FileType = GetFileType(fileName),
                                IsImage = IsImageFile(fileName),
                                IsPdf = IsPdfFile(fileName)
                            });
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Error parsing MediaUrls JSON for device {DeviceId}", report.DeviceId);
                    }
                }
            }

            // TODO: Get media files from TBS if needed (MediaApi does this via GetMediaFilesForMachine)

            return mediaData;
        }

        /// <summary>
        /// Map data to CustomerMachinesDto
        /// </summary>
        private CustomerMachinesDto MapToCustomerMachinesDto(
            Models.Client customer,
            List<TbsMachineInfo> machines,
            Dictionary<int, int> visitCounts,
            Dictionary<int, List<MediaFileDto>> mediaData,
            long customerId)
        {
            var machineDtos = machines.Select(m => new MachineDto
            {
                MachineId = m.MachineId,
                SerialNumber = m.SerialNumber,
                ModelName = m.ModelName,
                ModelNameEn = m.ModelNameEn,
                ItemCode = m.ItemCode,
                VisitCount = visitCounts.GetValueOrDefault(m.MachineId, 0),
                MediaFiles = mediaData.GetValueOrDefault(m.MachineId, new List<MediaFileDto>())
            }).ToList();

            return new CustomerMachinesDto
            {
                CustomerId = (int)customerId,
                CustomerName = customer.Name,
                CustomerAddress = customer.Address,
                CustomerPhone = customer.Phone,
                MachineCount = machineDtos.Count,
                Machines = machineDtos
            };
        }

        /// <summary>
        /// Get file type from file name
        /// </summary>
        private string? GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension?.TrimStart('.');
        }

        /// <summary>
        /// Check if file is an image
        /// </summary>
        private bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
        }

        /// <summary>
        /// Check if file is a PDF
        /// </summary>
        private bool IsPdfFile(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension == ".pdf";
        }

        /// <summary>
        /// Helper class for TBS machine information
        /// </summary>
        private class TbsMachineInfo
        {
            public int MachineId { get; set; } // OOI_ID from TBS
            public string SerialNumber { get; set; } = string.Empty;
            public string? ModelName { get; set; }
            public string? ModelNameEn { get; set; }
            public string? ItemCode { get; set; }
            public int? EquipmentId { get; set; } // Equipment table ID (if exists in new database)
        }
    }
}
