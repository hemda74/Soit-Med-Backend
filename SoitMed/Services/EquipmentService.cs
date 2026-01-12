using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Hospital;
using SoitMed.Models.Legacy;
using SoitMed.Repositories;
using SoitMed.Services;

namespace SoitMed.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Context _context;
        private readonly ILogger<EquipmentService> _logger;
        private readonly IConfiguration _configuration;

        public EquipmentService(
            IUnitOfWork unitOfWork,
            Context context,
            ILogger<EquipmentService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get all equipment for a client using comprehensive relationship checking
        /// Similar to soitmed_data_backend's GetMachinesByCustomerIdAsync approach
        /// </summary>
        public async Task<IEnumerable<EquipmentResponseDTO>> GetEquipmentByClientIdAsync(long clientId)
        {
            try
            {
                // Get client info
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null)
                {
                    _logger.LogWarning("Client with ID {ClientId} not found", clientId);
                    return Enumerable.Empty<EquipmentResponseDTO>();
                }

                var equipmentIds = new HashSet<string>();

                // 1. Equipment directly linked via Client's RelatedUserId -> ApplicationUser -> Equipment.CustomerId
                if (!string.IsNullOrEmpty(client.RelatedUserId))
                {
                    var equipmentFromRelatedUser = await _context.Equipment
                        .AsNoTracking()
                        .Where(e => e.CustomerId == client.RelatedUserId && e.IsActive)
                        .Select(e => e.Id)
                        .ToListAsync();
                    
                    foreach (var id in equipmentFromRelatedUser)
                    {
                        equipmentIds.Add(id);
                    }
                    
                    _logger.LogInformation("Found {Count} equipment via RelatedUserId for client {ClientId}", 
                        equipmentFromRelatedUser.Count, clientId);
                }

                // 2. Equipment through MaintenanceRequests where CustomerId matches RelatedUserId
                if (!string.IsNullOrEmpty(client.RelatedUserId))
                {
                    var equipmentFromMaintenanceRequests = await (from mr in _context.MaintenanceRequests.AsNoTracking()
                                                                 where mr.CustomerId == client.RelatedUserId
                                                                 select mr.EquipmentId)
                                                                 .Distinct()
                                                                 .ToListAsync();
                    
                    foreach (var id in equipmentFromMaintenanceRequests)
                    {
                        equipmentIds.Add(id);
                    }
                    
                    _logger.LogInformation("Found {Count} equipment via MaintenanceRequests for client {ClientId}", 
                        equipmentFromMaintenanceRequests.Count, clientId);
                }

                // 3. Equipment through MaintenanceVisits where CustomerId matches RelatedUserId
                if (!string.IsNullOrEmpty(client.RelatedUserId))
                {
                    var equipmentFromMaintenanceVisits = await (from mv in _context.MaintenanceVisits.AsNoTracking()
                                                                where mv.CustomerId == client.RelatedUserId
                                                                select mv.DeviceId)
                                                                .Distinct()
                                                                .ToListAsync();
                    
                    foreach (var id in equipmentFromMaintenanceVisits)
                    {
                        equipmentIds.Add(id);
                    }
                    
                    _logger.LogInformation("Found {Count} equipment via MaintenanceVisits for client {ClientId}", 
                        equipmentFromMaintenanceVisits.Count, clientId);
                }

                // 4. Equipment through Hospital (if client is linked to hospital via RelatedUserId)
                // Check through Doctor or Technician relationships
                if (!string.IsNullOrEmpty(client.RelatedUserId))
                {
                    // Get hospital ID from Doctor relationship
                    var doctorHospitalId = await (from d in _context.Doctors.AsNoTracking()
                                                   where d.UserId == client.RelatedUserId
                                                   select d.HospitalId)
                                                   .FirstOrDefaultAsync();
                    
                    // Get hospital ID from Technician relationship if not found in Doctor
                    if (string.IsNullOrEmpty(doctorHospitalId))
                    {
                        doctorHospitalId = await (from t in _context.Technicians.AsNoTracking()
                                                  where t.UserId == client.RelatedUserId
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
                        
                        _logger.LogInformation("Found {Count} equipment via Hospital for client {ClientId}", 
                            equipmentFromHospital.Count, clientId);
                    }
                }

                // 5. Equipment through LegacySourceId matching (if client has LegacyCustomerId)
                // Query legacy TBS database following soitmed_data_backend approach
                int? legacyCustomerId = client.LegacyCustomerId;
                
                // If LegacyCustomerId is not set, try to find it by matching name/phone in legacy database
                if (!legacyCustomerId.HasValue)
                {
                    try
                    {
                        legacyCustomerId = await FindLegacyCustomerIdAsync(client.Name, client.Phone);
                        if (legacyCustomerId.HasValue)
                        {
                            _logger.LogInformation("Found legacy customer ID {LegacyCustomerId} for client {ClientId} by name/phone match", 
                                legacyCustomerId.Value, clientId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error searching for legacy customer for client {ClientId}", clientId);
                    }
                }

                // Query legacy database if we have a legacy customer ID
                if (legacyCustomerId.HasValue)
                {
                    try
                    {
                        var legacyEquipment = await GetLegacyEquipmentByCustomerIdAsync(legacyCustomerId.Value);
                        _logger.LogInformation("Found {Count} equipment from legacy database for client {ClientId} (LegacyCustomerId: {LegacyCustomerId})", 
                            legacyEquipment.Count, clientId, legacyCustomerId.Value);
                        
                        // If we found equipment in legacy database, return it
                        if (legacyEquipment.Any())
                        {
                            return legacyEquipment;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error querying legacy database for client {ClientId} (LegacyCustomerId: {LegacyCustomerId})", 
                            clientId, legacyCustomerId.Value);
                        // Continue with current database results
                    }
                }

                // Load all unique equipment from current database
                var equipmentList = await _context.Equipment
                    .AsNoTracking()
                    .Where(e => equipmentIds.Contains(e.Id))
                    .Include(e => e.Hospital)
                    .Include(e => e.Customer)
                    .ToListAsync();

                _logger.LogInformation("Total unique equipment found for client {ClientId}: {Count}", 
                    clientId, equipmentList.Count);

                // Map to DTOs
                var equipmentDTOs = equipmentList.Select(e => new EquipmentResponseDTO
                {
                    Id = e.Id.ToString(),
                    Name = e.Name,
                    QRCode = e.QRCode,
                    QRCodeImageData = e.QRCodeImageData,
                    QRCodePdfPath = e.QRCodePdfPath,
                    Description = e.Description,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    PurchaseDate = e.PurchaseDate,
                    WarrantyExpiry = e.WarrantyExpiry,
                    HospitalId = e.HospitalId,
                    HospitalName = e.Hospital?.Name ?? string.Empty,
                    RepairVisitCount = e.RepairVisitCount,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    IsActive = e.IsActive,
                    QrToken = e.QrToken != Guid.Empty ? e.QrToken.ToString() : null,
                    IsQrPrinted = e.IsQrPrinted
                }).ToList();

                return equipmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment for client {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get all equipment for a customer (ApplicationUser) using comprehensive relationship checking
        /// </summary>
        public async Task<IEnumerable<EquipmentResponseDTO>> GetEquipmentByCustomerIdAsync(string customerId)
        {
            try
            {
                var equipmentIds = new HashSet<string>();

                // 1. Equipment directly linked via CustomerId
                var equipmentFromDirectLink = await _context.Equipment
                    .AsNoTracking()
                    .Where(e => e.CustomerId == customerId && e.IsActive)
                    .Select(e => e.Id)
                    .ToListAsync();
                
                foreach (var id in equipmentFromDirectLink)
                {
                    equipmentIds.Add(id);
                }

                // 2. Equipment through MaintenanceRequests
                var equipmentFromMaintenanceRequests = await (from mr in _context.MaintenanceRequests.AsNoTracking()
                                                             where mr.CustomerId == customerId
                                                             select mr.EquipmentId)
                                                             .Distinct()
                                                             .ToListAsync();
                
                foreach (var id in equipmentFromMaintenanceRequests)
                {
                    equipmentIds.Add(id);
                }

                // 3. Equipment through MaintenanceVisits
                var equipmentFromMaintenanceVisits = await (from mv in _context.MaintenanceVisits.AsNoTracking()
                                                            where mv.CustomerId == customerId
                                                            select mv.DeviceId)
                                                            .Distinct()
                                                            .ToListAsync();
                
                foreach (var id in equipmentFromMaintenanceVisits)
                {
                    equipmentIds.Add(id);
                }

                // 4. Equipment through Hospital (if user is linked to hospital via Doctor or Technician)
                // Get hospital ID from Doctor relationship
                var userHospitalId = await (from d in _context.Doctors.AsNoTracking()
                                           where d.UserId == customerId
                                           select d.HospitalId)
                                           .FirstOrDefaultAsync();
                
                // Get hospital ID from Technician relationship if not found in Doctor
                if (string.IsNullOrEmpty(userHospitalId))
                {
                    userHospitalId = await (from t in _context.Technicians.AsNoTracking()
                                          where t.UserId == customerId
                                          select t.HospitalId)
                                          .FirstOrDefaultAsync();
                }
                
                if (!string.IsNullOrEmpty(userHospitalId))
                {
                    var equipmentFromHospital = await _context.Equipment
                        .AsNoTracking()
                        .Where(e => e.HospitalId == userHospitalId && e.IsActive)
                        .Select(e => e.Id)
                        .ToListAsync();
                    
                    foreach (var id in equipmentFromHospital)
                    {
                        equipmentIds.Add(id);
                    }
                }

                // Load all unique equipment
                var equipmentList = await _context.Equipment
                    .AsNoTracking()
                    .Where(e => equipmentIds.Contains(e.Id))
                    .Include(e => e.Hospital)
                    .Include(e => e.Customer)
                    .ToListAsync();

                // Map to DTOs
                var equipmentDTOs = equipmentList.Select(e => new EquipmentResponseDTO
                {
                    Id = e.Id.ToString(),
                    Name = e.Name,
                    QRCode = e.QRCode,
                    QRCodeImageData = e.QRCodeImageData,
                    QRCodePdfPath = e.QRCodePdfPath,
                    Description = e.Description,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    PurchaseDate = e.PurchaseDate,
                    WarrantyExpiry = e.WarrantyExpiry,
                    HospitalId = e.HospitalId,
                    HospitalName = e.Hospital?.Name ?? string.Empty,
                    RepairVisitCount = e.RepairVisitCount,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    IsActive = e.IsActive,
                    QrToken = e.QrToken != Guid.Empty ? e.QrToken.ToString() : null,
                    IsQrPrinted = e.IsQrPrinted
                }).ToList();

                return equipmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment for customer {CustomerId}", customerId);
                throw;
            }
        }

        /// <summary>
        /// Get equipment from legacy TBS database following soitmed_data_backend comprehensive approach
        /// </summary>
        private async Task<List<EquipmentResponseDTO>> GetLegacyEquipmentByCustomerIdAsync(int legacyCustomerId)
        {
            var result = new List<EquipmentResponseDTO>();
            var machineIds = new HashSet<int>();

            try
            {
                var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                if (string.IsNullOrEmpty(tbsConnectionString))
                {
                    _logger.LogWarning("TbsConnection string not configured, cannot query legacy database");
                    return result;
                }

                var optionsBuilder = new DbContextOptionsBuilder<TbsDbContext>();
                optionsBuilder.UseSqlServer(tbsConnectionString);

                using var tbsContext = new TbsDbContext(optionsBuilder.Options);

                // Verify customer exists in legacy database
                var legacyCustomer = await tbsContext.StkCustomers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CusId == legacyCustomerId);

                if (legacyCustomer == null)
                {
                    _logger.LogWarning("Legacy customer {LegacyCustomerId} not found in TBS database", legacyCustomerId);
                    return result;
                }

                _logger.LogInformation("Querying legacy TBS database for customer {LegacyCustomerId} ({CustomerName})", 
                    legacyCustomerId, legacyCustomer.CusName);

                // 1. Get machines through visits (where visit's CusId = customerId)
                var machinesFromVisits = await (from v in tbsContext.MntVisitings.AsNoTracking()
                                               where v.CusId == legacyCustomerId
                                               join vr in tbsContext.MntVisitingReports.AsNoTracking() on v.VisitingId equals vr.VisitingId
                                               join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on vr.OoiId equals ooi.OoiId
                                               join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                               select new
                                               {
                                                   MachineId = ooi.OoiId,
                                                   SerialNumber = ooi.SerialNum ?? string.Empty,
                                                   ModelName = item.ItemNameAr ?? string.Empty,
                                                   ModelNameEn = item.ItemNameEn,
                                                   ItemCode = item.ItemCode ?? string.Empty
                                               })
                                               .ToListAsync();

                foreach (var m in machinesFromVisits)
                {
                    machineIds.Add(m.MachineId);
                }

                _logger.LogInformation("Found {Count} machines via visits for legacy customer {LegacyCustomerId}", 
                    machinesFromVisits.Count, legacyCustomerId);

                // 2. Get machines through contract-linked visits
                var machinesFromContractVisits = await (from contract in tbsContext.MntMaintenanceContracts.AsNoTracking()
                                                       where contract.CusId == legacyCustomerId
                                                       join v in tbsContext.MntVisitings.AsNoTracking() on contract.ContractId equals v.ContractId
                                                       join vr in tbsContext.MntVisitingReports.AsNoTracking() on v.VisitingId equals vr.VisitingId
                                                       join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on vr.OoiId equals ooi.OoiId
                                                       join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                       where !string.IsNullOrEmpty(ooi.SerialNum)
                                                       select new
                                                       {
                                                           MachineId = ooi.OoiId,
                                                           SerialNumber = ooi.SerialNum ?? string.Empty,
                                                           ModelName = item.ItemNameAr ?? string.Empty,
                                                           ModelNameEn = item.ItemNameEn,
                                                           ItemCode = item.ItemCode ?? string.Empty
                                                       })
                                                       .ToListAsync();

                foreach (var m in machinesFromContractVisits)
                {
                    machineIds.Add(m.MachineId);
                }

                _logger.LogInformation("Found {Count} machines via contract visits for legacy customer {LegacyCustomerId}", 
                    machinesFromContractVisits.Count, legacyCustomerId);

                // 3. Get machines through maintenance contract items
                var machinesFromContractItems = await (from contract in tbsContext.MntMaintenanceContracts.AsNoTracking()
                                                      where contract.CusId == legacyCustomerId
                                                      join mci in tbsContext.MntMaintenanceContractItems.AsNoTracking() on contract.ContractId equals mci.ContractId
                                                      where mci.OoiId.HasValue
                                                      join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on mci.OoiId.Value equals ooi.OoiId
                                                      join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                                      select new
                                                      {
                                                          MachineId = ooi.OoiId,
                                                          SerialNumber = ooi.SerialNum ?? string.Empty,
                                                          ModelName = item.ItemNameAr ?? string.Empty,
                                                          ModelNameEn = item.ItemNameEn,
                                                          ItemCode = item.ItemCode ?? string.Empty
                                                      })
                                                      .ToListAsync();

                foreach (var m in machinesFromContractItems)
                {
                    machineIds.Add(m.MachineId);
                }

                _logger.LogInformation("Found {Count} machines via contract items for legacy customer {LegacyCustomerId}", 
                    machinesFromContractItems.Count, legacyCustomerId);

                // 4. Get machines through sales invoices
                // TbsSalesInvoice doesn't have CusId directly, need to join with TbsSalesContract
                var machinesFromSales = await (from si in tbsContext.StkSalesInvs.AsNoTracking()
                                              join sc in tbsContext.StkSalesContracts.AsNoTracking() on si.ScId equals sc.ScId
                                              where sc.CusId == legacyCustomerId
                                              join oo in tbsContext.StkOrderOuts.AsNoTracking() on si.SiId equals oo.SiId
                                              join ooi in tbsContext.StkOrderOutItems.AsNoTracking() on oo.OoId equals ooi.OoId
                                              join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                              where !string.IsNullOrEmpty(ooi.SerialNum)
                                              select new
                                              {
                                                  MachineId = ooi.OoiId,
                                                  SerialNumber = ooi.SerialNum ?? string.Empty,
                                                  ModelName = item.ItemNameAr ?? string.Empty,
                                                  ModelNameEn = item.ItemNameEn,
                                                  ItemCode = item.ItemCode ?? string.Empty
                                              })
                                              .ToListAsync();

                foreach (var m in machinesFromSales)
                {
                    machineIds.Add(m.MachineId);
                }

                _logger.LogInformation("Found {Count} machines via sales invoices for legacy customer {LegacyCustomerId}", 
                    machinesFromSales.Count, legacyCustomerId);

                // 5. Get machines through contract TempOoiId
                var machinesFromTempOoi = await (from contract in tbsContext.MntMaintenanceContracts.AsNoTracking()
                                               where contract.CusId == legacyCustomerId
                                               // Note: TempOoiId might be in a different column, adjust if needed
                                               select new { ContractId = contract.ContractId })
                                               .ToListAsync();

                // Get all unique machines with their details
                var allMachines = await (from ooi in tbsContext.StkOrderOutItems.AsNoTracking()
                                        where machineIds.Contains(ooi.OoiId)
                                        join item in tbsContext.StkItems.AsNoTracking() on ooi.ItemId equals item.ItemId
                                        select new
                                        {
                                            MachineId = ooi.OoiId,
                                            SerialNumber = ooi.SerialNum ?? string.Empty,
                                            ModelName = item.ItemNameAr ?? string.Empty,
                                            ModelNameEn = item.ItemNameEn,
                                            ItemCode = item.ItemCode ?? string.Empty,
                                            DevicePlace = ooi.DevicePlace,
                                            ExpirationDate = ooi.ItemDateExpire
                                        })
                                        .Distinct()
                                        .ToListAsync();

                // Get visit counts for each machine
                var visitCounts = await (from vr in tbsContext.MntVisitingReports.AsNoTracking()
                                       where machineIds.Contains(vr.OoiId)
                                       group vr by vr.OoiId into g
                                       select new { MachineId = g.Key, VisitCount = g.Count() })
                                       .ToDictionaryAsync(x => x.MachineId, x => x.VisitCount);

                // Map to EquipmentResponseDTO
                foreach (var machine in allMachines)
                {
                    result.Add(new EquipmentResponseDTO
                    {
                        Id = machine.MachineId.ToString(), // Use legacy machine ID
                        Name = !string.IsNullOrEmpty(machine.ModelName) ? machine.ModelName : machine.ModelNameEn ?? "Unknown",
                        QRCode = machine.SerialNumber, // Use serial number as QR code identifier
                        Description = machine.ModelNameEn,
                        Model = machine.ModelName,
                        Manufacturer = null,
                        PurchaseDate = null,
                        WarrantyExpiry = machine.ExpirationDate,
                        HospitalId = string.Empty,
                        HospitalName = string.Empty,
                        RepairVisitCount = visitCounts.GetValueOrDefault(machine.MachineId, 0),
                        Status = EquipmentStatus.Operational.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        LastMaintenanceDate = null,
                        IsActive = true,
                        QrToken = Guid.NewGuid().ToString(),
                        IsQrPrinted = false
                    });
                }

                _logger.LogInformation("Returning {Count} unique machines from legacy database for customer {LegacyCustomerId}", 
                    result.Count, legacyCustomerId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying legacy database for customer {LegacyCustomerId}", legacyCustomerId);
                return result; // Return empty list on error
            }
        }

        /// <summary>
        /// Find legacy customer ID by matching name and/or phone in TBS database
        /// </summary>
        private async Task<int?> FindLegacyCustomerIdAsync(string? clientName, string? clientPhone)
        {
            try
            {
                var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                if (string.IsNullOrEmpty(tbsConnectionString))
                {
                    return null;
                }

                var optionsBuilder = new DbContextOptionsBuilder<TbsDbContext>();
                optionsBuilder.UseSqlServer(tbsConnectionString);

                using var tbsContext = new TbsDbContext(optionsBuilder.Options);

                // Try to match by name first (most reliable)
                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    // Use EF.Functions.Like for better SQL translation (supports Arabic)
                    var searchPattern = $"%{clientName.Trim()}%";
                    var customerByName = await tbsContext.StkCustomers
                        .AsNoTracking()
                        .Where(c => c.CusName != null && EF.Functions.Like(c.CusName, searchPattern))
                        .FirstOrDefaultAsync();

                    if (customerByName != null)
                    {
                        _logger.LogInformation("Found legacy customer by name: {LegacyCustomerId} ({Name})", 
                            customerByName.CusId, customerByName.CusName);
                        return customerByName.CusId;
                    }
                }

                // If name match failed, try phone match (exact match or contains)
                if (!string.IsNullOrWhiteSpace(clientPhone))
                {
                    var phonePattern = $"%{clientPhone.Trim()}%";
                    
                    // Try exact match first
                    var customerByPhone = await tbsContext.StkCustomers
                        .AsNoTracking()
                        .Where(c => (c.CusTel != null && c.CusTel == clientPhone.Trim()) ||
                                   (c.CusMobile != null && c.CusMobile == clientPhone.Trim()))
                        .FirstOrDefaultAsync();

                    // If exact match failed, try pattern match
                    if (customerByPhone == null)
                    {
                        customerByPhone = await tbsContext.StkCustomers
                            .AsNoTracking()
                            .Where(c => (c.CusTel != null && EF.Functions.Like(c.CusTel, phonePattern)) ||
                                       (c.CusMobile != null && EF.Functions.Like(c.CusMobile, phonePattern)))
                            .FirstOrDefaultAsync();
                    }

                    if (customerByPhone != null)
                    {
                        _logger.LogInformation("Found legacy customer by phone: {LegacyCustomerId} ({Name})", 
                            customerByPhone.CusId, customerByPhone.CusName);
                        return customerByPhone.CusId;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error finding legacy customer ID for name: {ClientName}, phone: {ClientPhone}", 
                    clientName, clientPhone);
                return null;
            }
        }
    }
}

