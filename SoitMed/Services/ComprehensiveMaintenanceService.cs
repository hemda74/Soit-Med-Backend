using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoitMed.DTO;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Models.Core;
using SoitMed.Repositories;
using SoitMed.Models.Legacy;
using SoitMed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoitMed.Services
{
    public class ComprehensiveMaintenanceService : IComprehensiveMaintenanceService
    {
        private readonly Context _newDbContext;
        private readonly ILogger<ComprehensiveMaintenanceService> _logger;

        public ComprehensiveMaintenanceService(Context newDbContext, ILogger<ComprehensiveMaintenanceService> logger)
        {
            _newDbContext = newDbContext;
            _logger = logger;
        }

        #region Customer Management
        public async Task<CustomerEquipmentVisitsDTO> GetCustomerEquipmentVisitsAsync(string customerId, bool includeLegacy = true)
        {
            try
            {
                // Get customer from existing Clients table
                var customer = await _newDbContext.Clients
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    return null;

                // Get equipment from existing Equipment table (CustomerId references ApplicationUser)
                var equipment = await _newDbContext.Equipment
                    .Where(e => e.CustomerId == customerId)
                    .Select(e => new EquipmentDTO
                    {
                        Id = e.Id.ToString(),
                        SerialNumber = e.QRCode, // Use QRCode as serial number
                        Model = e.Model ?? string.Empty,
                        Manufacturer = e.Manufacturer ?? string.Empty,
                        CustomerId = e.CustomerId,
                        CustomerName = customer.Name,
                        InstallationDate = e.PurchaseDate,
                        Status = e.Status.ToString(),
                        Location = e.Hospital != null ? e.Hospital.Name : string.Empty, // Use hospital name as location
                        QRCode = e.QRCode,
                        HospitalId = e.HospitalId ?? string.Empty,
                        Name = e.Name,
                        PurchaseDate = e.PurchaseDate,
                        VisitCount = 0, // Will be calculated separately
                        Description = e.Description ?? string.Empty,
                        LastMaintenanceDate = e.LastMaintenanceDate,
                        WarrantyExpiry = e.WarrantyExpiry,
                        CreatedAt = e.CreatedAt,
                        IsActive = true,
                        MaintenanceVisitsCount = 0
                    })
                    .ToListAsync();

                // Get visits from existing MaintenanceVisits table
                var visits = await _newDbContext.MaintenanceVisits
                    .Include(v => v.Device)
                    .Where(v => v.CustomerId == customerId)
                    .Select(v => new MaintenanceVisitDTO
                    {
                        Id = v.Id.ToString(),
                        EquipmentId = v.DeviceId.ToString(),
                        EquipmentSerialNumber = v.QRCode ?? v.SerialCode,
                        VisitDate = v.ScheduledDate,
                        VisitType = v.Origin.ToString(),
                        Status = v.Status.ToString(),
                        EngineerId = v.EngineerId,
                        EngineerName = null,
                        Report = v.Report,
                        CompletionDate = v.CompletedAt,
                        CreatedAt = v.VisitDate
                    })
                    .ToListAsync();

                // Calculate visit counts for equipment
                var visitCounts = visits.GroupBy(v => v.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Count());

                foreach (var equip in equipment)
                {
                    if (visitCounts.ContainsKey(equip.Id))
                        equip.MaintenanceVisitsCount = visitCounts[equip.Id];
                }

                return new CustomerEquipmentVisitsDTO
                {
                    Customer = new CustomerDTO
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Phone = customer.Phone,
                        Email = customer.Email,
                        Address = customer.Address,
                        IsActive = true, // Default to active
                        CreatedAt = customer.CreatedAt
                    },
                    Equipment = equipment,
                    Visits = visits
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer equipment visits for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PagedResult<CustomerDTO>> SearchCustomersAsync(CustomerSearchCriteria criteria)
        {
            try
            {
                var query = _newDbContext.Clients.AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
                {
                    query = query.Where(c => c.Name.Contains(criteria.SearchTerm) ||
                                          (c.Phone != null && c.Phone.Contains(criteria.SearchTerm)) ||
                                          (c.Email != null && c.Email.Contains(criteria.SearchTerm)));
                }

                var totalCount = await query.CountAsync();

                var customers = await query
                    .Skip((criteria.Page - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .Select(c => new CustomerDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Phone = c.Phone,
                        Email = c.Email,
                        Address = c.Address,
                        IsActive = true, // Default to active
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return new PagedResult<CustomerDTO>
                {
                    Items = customers,
                    TotalCount = totalCount,
                    PageSize = criteria.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with criteria: {@Criteria}", criteria);
                throw;
            }
        }

        public async Task<CustomerDTO> CreateCustomerAsync(CreateCustomerRequest request)
        {
            try
            {
                var customer = new Client
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    Address = request.Address,
                    CreatedAt = DateTime.UtcNow
                };

                _newDbContext.Clients.Add(customer);
                await _newDbContext.SaveChangesAsync();

                return new CustomerDTO
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address,
                    IsActive = true,
                    CreatedAt = customer.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with request: {@Request}", request);
                throw;
            }
        }

        public async Task<CustomerDTO> UpdateCustomerAsync(string customerId, UpdateCustomerRequest request)
        {
            try
            {
                var customer = await _newDbContext.Clients.FindAsync(customerId);
                if (customer == null)
                    return null;

                if (!string.IsNullOrWhiteSpace(request.Name))
                    customer.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.Phone))
                    customer.Phone = request.Phone;
                if (!string.IsNullOrWhiteSpace(request.Email))
                    customer.Email = request.Email;
                if (!string.IsNullOrWhiteSpace(request.Address))
                    customer.Address = request.Address;

                customer.UpdatedAt = DateTime.UtcNow;
                await _newDbContext.SaveChangesAsync();

                return new CustomerDTO
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    Address = customer.Address,
                    IsActive = true,
                    CreatedAt = customer.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            try
            {
                var customer = await _newDbContext.Clients.FindAsync(customerId);
                if (customer == null)
                    return false;

                _newDbContext.Clients.Remove(customer);
                await _newDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerVisitStatsDTO> GetCustomerVisitStatisticsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _newDbContext.MaintenanceVisits
                    .Where(v => v.CustomerId == customerId);

                if (startDate.HasValue)
                    query = query.Where(v => v.ScheduledDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(v => v.ScheduledDate <= endDate.Value);

                var visits = await query.ToListAsync();

                return new CustomerVisitStats
                {
                    CustomerId = customerId,
                    TotalVisits = visits.Count,
                    CompletedVisits = visits.Count(v => v.Status == VisitStatus.Completed),
                    PendingVisits = visits.Count(v => v.Status == VisitStatus.PendingApproval),
                    EmergencyVisits = visits.Count(v => v.Origin == VisitOrigin.CustomerApp),
                    PreventiveVisits = visits.Count(v => v.Origin == VisitOrigin.AutoContract),
                    InstallationVisits = visits.Count(v => v.Origin == VisitOrigin.CallCenter),
                    LastVisitDate = visits.OrderByDescending(v => v.ScheduledDate).FirstOrDefault()?.ScheduledDate,
                    NextScheduledDate = visits.Where(v => v.ScheduledDate > DateTime.UtcNow && v.Status == VisitStatus.PendingApproval)
                                            .OrderBy(v => v.ScheduledDate).FirstOrDefault()?.ScheduledDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for customer {CustomerId}", customerId);
                throw;
            }
        }
        #endregion

        #region Equipment Management
        public async Task<List<EquipmentDTO>> GetEquipmentAsync(string customerId)
        {
            try
            {
                var equipment = await _newDbContext.Equipment
                    .Include(e => e.Customer)
                    .Where(e => e.CustomerId == customerId)
                    .ToListAsync();

                return equipment.Select(e => new EquipmentDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    QRCode = e.QRCode,
                    Description = e.Description,
                    Model = e.Model,
                    Manufacturer = e.Manufacturer,
                    CustomerId = e.CustomerId,
                    CustomerName = e.Customer?.FullName ?? string.Empty,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                    IsActive = e.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment for customer {CustomerId}", customerId);
                throw;
            }
        }
        #endregion

        public async Task<PagedResult<EquipmentDTO>> GetCustomerEquipmentAsync(string customerId, PagedRequest request)
        {
            try
            {
                var query = _newDbContext.Equipment
                    .Where(e => e.CustomerId == customerId);

                var totalCount = await query.CountAsync();

                var equipment = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(e => new EquipmentDTO
                    {
                        Id = e.Id.ToString(),
                        SerialNumber = e.QRCode,
                        Model = e.Model,
                        Manufacturer = e.Manufacturer,
                        CustomerId = e.CustomerId,
                        InstallationDate = e.PurchaseDate,
                        WarrantyExpiryDate = e.WarrantyExpiry,
                        Status = e.Status.ToString(),
                        Location = e.Hospital != null ? e.Hospital.Name : string.Empty,
                        CreatedAt = e.CreatedAt
                    })
                    .ToListAsync();

                return new PagedResult<EquipmentDTO>
                {
                    Items = equipment,
                    TotalCount = totalCount,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<EquipmentDTO> CreateEquipmentAsync(CreateEquipmentRequest request)
        {
            try
            {
                var equipment = new Models.Equipment.Equipment
                {
                    Name = request.Model, // Using model as name for now
                    QRCode = request.SerialNumber,
                    Model = request.Model,
                    Manufacturer = request.Manufacturer,
                    CustomerId = request.CustomerId,
                    PurchaseDate = request.InstallationDate,
                    WarrantyExpiry = request.WarrantyExpiryDate,
                    CreatedAt = DateTime.UtcNow
                };

                _newDbContext.Equipment.Add(equipment);
                await _newDbContext.SaveChangesAsync();

                return new EquipmentDTO
                {
                    Id = equipment.Id.ToString(),
                    SerialNumber = equipment.QRCode,
                    Model = equipment.Model,
                    Manufacturer = equipment.Manufacturer,
                    CustomerId = equipment.CustomerId,
                    InstallationDate = equipment.PurchaseDate,
                    WarrantyExpiryDate = equipment.WarrantyExpiry,
                    Status = equipment.Status.ToString(),
                    Location = equipment.Hospital?.Name ?? string.Empty,
                    CreatedAt = equipment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating equipment with request: {@Request}", request);
                throw;
            }
        }

        public async Task<EquipmentDTO> UpdateEquipmentAsync(string equipmentId, UpdateEquipmentRequest request)
        {
            try
            {
                var id = int.Parse(equipmentId);
                var equipment = await _newDbContext.Equipment.FindAsync(id);
                if (equipment == null)
                    return null;

                if (!string.IsNullOrWhiteSpace(request.ModelId))
                {
                    equipment.QRCode = request.ModelId;
                }
                if (!string.IsNullOrWhiteSpace(request.Model))
                    equipment.Model = request.Model;
                if (!string.IsNullOrWhiteSpace(request.Manufacturer))
                    equipment.Manufacturer = request.Manufacturer;
                if (request.InstallationDate.HasValue)
                {
                    equipment.PurchaseDate = request.InstallationDate.Value;
                }
                if (request.WarrantyExpiryDate.HasValue)
                    equipment.WarrantyExpiry = request.WarrantyExpiryDate.Value;
                if (!string.IsNullOrWhiteSpace(request.Status))
                    equipment.Status = Enum.Parse<Models.Equipment.EquipmentStatus>(request.Status);

                await _newDbContext.SaveChangesAsync();

                return new EquipmentDTO
                {
                    Id = equipment.Id.ToString(),
                    SerialNumber = equipment.QRCode,
                    Model = equipment.Model,
                    Manufacturer = equipment.Manufacturer,
                    CustomerId = equipment.CustomerId,
                    InstallationDate = equipment.PurchaseDate,
                    WarrantyExpiryDate = equipment.WarrantyExpiry,
                    Status = equipment.Status.ToString(),
                    Location = equipment.Hospital?.Name ?? string.Empty,
                    CreatedAt = equipment.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment {EquipmentId}", equipmentId);
                throw;
            }
        }

        public async Task<bool> DeleteEquipmentAsync(string equipmentId)
        {
            try
            {
                var id = int.Parse(equipmentId);
                var equipment = await _newDbContext.Equipment.FindAsync(id);
                if (equipment == null)
                    return false;

                _newDbContext.Equipment.Remove(equipment);
                await _newDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment {EquipmentId}", equipmentId);
                throw;
            }
        }

        #region Visit Management
        public async Task<MaintenanceVisitDTO> GetVisitAsync(string visitId)
        {
            try
            {
                var id = int.Parse(visitId);
                var visit = await _newDbContext.MaintenanceVisits
                    .Include(v => v.Device)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (visit == null)
                    return null;

                return new MaintenanceVisitDTO
                {
                    Id = visit.Id.ToString(),
                    EquipmentId = visit.DeviceId.ToString(),
                    EquipmentSerialNumber = visit.QRCode ?? visit.SerialCode,
                    VisitDate = visit.ScheduledDate,
                    VisitType = visit.Origin.ToString(),
                    Status = visit.Status.ToString(),
                    EngineerId = visit.EngineerId,
                    EngineerName = null,
                    Report = visit.Report,
                    CompletionDate = visit.CompletedAt,
                    CreatedAt = visit.VisitDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visit {VisitId}", visitId);
                throw;
            }
        }

        public async Task<PagedResult<MaintenanceVisitDTO>> GetEquipmentVisitsAsync(string equipmentId, VisitSearchCriteria criteria)
        {
            try
            {
                var query = _newDbContext.MaintenanceVisits
                    .Include(v => v.Device)
                    .Where(v => v.DeviceId == equipmentId);

                if (!string.IsNullOrEmpty(criteria.Status))
                    query = query.Where(v => v.Status == Enum.Parse<VisitStatus>(criteria.Status));

                if (!string.IsNullOrEmpty(criteria.VisitType))
                    query = query.Where(v => v.Origin == Enum.Parse<VisitOrigin>(criteria.VisitType));

                if (criteria.StartDate.HasValue)
                    query = query.Where(v => v.ScheduledDate >= criteria.StartDate.Value);

                if (criteria.EndDate.HasValue)
                    query = query.Where(v => v.ScheduledDate <= criteria.EndDate.Value);

                var totalCount = await query.CountAsync();

                var visits = await query
                    .Skip((criteria.Page - 1) * criteria.PageSize)
                    .Take(criteria.PageSize)
                    .Select(v => new MaintenanceVisitDTO
                    {
                        Id = v.Id.ToString(),
                        EquipmentId = v.DeviceId.ToString(),
                        EquipmentSerialNumber = v.QRCode ?? v.SerialCode,
                        VisitDate = v.ScheduledDate,
                        VisitType = v.Origin.ToString(),
                        Status = v.Status.ToString(),
                        EngineerId = v.EngineerId,
                        EngineerName = null,
                        Report = v.Report,
                        CompletionDate = v.CompletedAt,
                        CreatedAt = v.VisitDate
                    })
                    .ToListAsync();

                return new PagedResult<MaintenanceVisitDTO>
                {
                    Items = visits,
                    TotalCount = totalCount,
                    PageSize = criteria.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visits for equipment {EquipmentId}", equipmentId);
                throw;
            }
        }

        public async Task<MaintenanceVisitDTO> CreateVisitAsync(CreateVisitRequestDTO request)
        {
            try
            {
                var visit = new Models.Equipment.MaintenanceVisit
                {
                    DeviceId = request.EquipmentId,
                    CustomerId = null, // Will be set based on equipment
                    ScheduledDate = DateTime.Parse(request.VisitDate),
                    Origin = Enum.Parse<VisitOrigin>(request.VisitType?.ToString() ?? "Preventive"),
                    Status = VisitStatus.PendingApproval,
                    VisitDate = DateTime.UtcNow
                };

                // Set CustomerId based on equipment
                var equipment = await _newDbContext.Equipment.FindAsync(request.EquipmentId);
                if (equipment != null)
                {
                    visit.CustomerId = equipment.CustomerId;
                }

                _newDbContext.MaintenanceVisits.Add(visit);
                await _newDbContext.SaveChangesAsync();

                return new MaintenanceVisitDTO
                {
                    Id = visit.Id.ToString(),
                    EquipmentId = visit.DeviceId.ToString(),
                    EquipmentSerialNumber = equipment?.QRCode,
                    VisitDate = visit.ScheduledDate,
                    VisitType = visit.Origin.ToString(),
                    Status = visit.Status.ToString(),
                    EngineerId = null,
                    EngineerName = null,
                    Report = null,
                    CompletionDate = null,
                    CreatedAt = visit.VisitDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating visit with request: {@Request}", request);
                throw;
            }
        }

        public async Task<MaintenanceVisitDTO> UpdateVisitAsync(string visitId, UpdateVisitRequest request)
        {
            try
            {
                var id = int.Parse(visitId);
                var visit = await _newDbContext.MaintenanceVisits.FindAsync(id);
                if (visit == null)
                    return null;

                if (request.VisitDate.HasValue)
                    visit.ScheduledDate = request.VisitDate.Value;

                if (!string.IsNullOrEmpty(request.VisitType))
                    visit.Origin = Enum.Parse<VisitOrigin>(request.VisitType);

                if (!string.IsNullOrEmpty(request.Status))
                    visit.Status = Enum.Parse<VisitStatus>(request.Status);

                if (!string.IsNullOrEmpty(request.CompletionDate) && DateTime.TryParse(request.CompletionDate, out DateTime completionDate))
                    visit.CompletedAt = completionDate;

                await _newDbContext.SaveChangesAsync();

                var equipment = await _newDbContext.Equipment.FindAsync(visit.DeviceId);

                return new MaintenanceVisitDTO
                {
                    Id = visit.Id.ToString(),
                    EquipmentId = visit.DeviceId.ToString(),
                    EquipmentSerialNumber = equipment?.QRCode,
                    VisitDate = visit.ScheduledDate,
                    VisitType = visit.Origin.ToString(),
                    Status = visit.Status.ToString(),
                    EngineerId = visit.EngineerId,
                    EngineerName = null,
                    Report = visit.Report,
                    CompletionDate = visit.CompletedAt,
                    CreatedAt = visit.VisitDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visit {VisitId}", visitId);
                throw;
            }
        }

        public async Task<bool> DeleteVisitAsync(string visitId)
        {
            try
            {
                var id = int.Parse(visitId);
                var visit = await _newDbContext.MaintenanceVisits.FindAsync(id);
                if (visit == null)
                    return false;

                _newDbContext.MaintenanceVisits.Remove(visit);
                await _newDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visit {VisitId}", visitId);
                throw;
            }
        }

        public async Task<VisitCompletionResponse> CompleteVisitAsync(string visitId, CompleteVisitRequest request)
        {
            try
            {
                var id = int.Parse(visitId);
                var visit = await _newDbContext.MaintenanceVisits.FindAsync(id);
                if (visit == null)
                    return new VisitCompletionResponse { Success = false, Message = "Visit not found" };

                visit.Status = VisitStatus.Completed;
                visit.CompletedAt = DateTime.UtcNow;
                visit.Report = request.Report;

                await _newDbContext.SaveChangesAsync();

                return new VisitCompletionResponse
                {
                    Success = true,
                    Message = "Visit completed successfully",
                    CompletedAt = visit.CompletedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing visit {VisitId}", visitId);
                return new VisitCompletionResponse { Success = false, Message = "Error completing visit" };
            }
        }
        #endregion

        #region Contract Management
        public async Task<MaintenanceContractDTO> GetContractAsync(string contractId)
        {
            try
            {
                var contract = await _newDbContext.MaintenanceContracts
                    .Include(mc => mc.Client)
                    .FirstOrDefaultAsync(mc => mc.Id == contractId);

                if (contract == null)
                    return null;

                return new MaintenanceContractDTO
                {
                    Id = contract.Id,
                    ContractNumber = contract.ContractNumber,
                    ClientId = contract.ClientId,
                    ClientName = contract.Client?.FullName,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    ContractValue = contract.TotalAmount,
                    Status = contract.Status.ToString(),
                    ContractType = "Standard", // Default contract type
                    PaymentTerms = contract.PaymentTerms,
                    CreatedAt = contract.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", contractId);
                throw;
            }
        }

        public async Task<PagedResult<MaintenanceContractDTO>> GetCustomerContractsAsync(string customerId, PagedRequest request)
        {
            try
            {
                var query = _newDbContext.MaintenanceContracts
                    .Include(mc => mc.Client)
                    .Where(mc => mc.ClientId == customerId);

                var totalCount = await query.CountAsync();

                var contracts = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(mc => new MaintenanceContractDTO
                    {
                        Id = mc.Id,
                        ContractNumber = mc.ContractNumber,
                        ClientId = mc.ClientId,
                        ClientName = mc.Client.FullName,
                        StartDate = mc.StartDate,
                        EndDate = mc.EndDate,
                        ContractValue = mc.TotalAmount,
                        Status = mc.Status.ToString(),
                        ContractType = "Standard", // Default contract type
                        PaymentTerms = mc.PaymentTerms,
                        CreatedAt = mc.CreatedAt
                    })
                    .ToListAsync();

                return new PagedResult<MaintenanceContractDTO>
                {
                    Items = contracts,
                    TotalCount = totalCount,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<MaintenanceContractDTO> CreateContractAsync(CreateContractRequest request)
        {
            try
            {
                var contract = new Models.MaintenanceContract
                {
                    Id = Guid.NewGuid().ToString(),
                    ContractNumber = request.ContractNumber,
                    ClientId = request.ClientId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalAmount = request.ContractValue,
                    Status = ContractStatus.Draft,
                    PaymentTerms = request.PaymentTerms,
                    CreatedAt = DateTime.UtcNow
                };

                _newDbContext.MaintenanceContracts.Add(contract);
                await _newDbContext.SaveChangesAsync();

                return new MaintenanceContractDTO
                {
                    Id = contract.Id,
                    ContractNumber = contract.ContractNumber,
                    ClientId = contract.ClientId,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    ContractValue = contract.TotalAmount,
                    Status = contract.Status.ToString(),
                    ContractType = "Standard", // Default contract type
                    PaymentTerms = contract.PaymentTerms,
                    CreatedAt = contract.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract with request: {@Request}", request);
                throw;
            }
        }

        public async Task<MaintenanceContractDTO> UpdateContractAsync(string contractId, UpdateContractRequest request)
        {
            try
            {
                var contract = await _newDbContext.MaintenanceContracts.FindAsync(contractId);
                if (contract == null)
                    return null;

                if (!string.IsNullOrEmpty(request.ContractNumber))
                    contract.ContractNumber = request.ContractNumber;
                if (request.StartDate.HasValue)
                    contract.StartDate = request.StartDate.Value;
                if (request.EndDate.HasValue)
                    contract.EndDate = request.EndDate.Value;
                if (request.ContractValue.HasValue)
                    contract.TotalAmount = request.ContractValue.Value;
                if (!string.IsNullOrEmpty(request.Status))
                    contract.Status = Enum.Parse<ContractStatus>(request.Status);
                if (!string.IsNullOrEmpty(request.ContractType))
                    contract.Notes = request.ContractType; // Use Notes field for contract type
                if (!string.IsNullOrEmpty(request.PaymentTerms))
                    contract.PaymentTerms = request.PaymentTerms;

                contract.UpdatedAt = DateTime.UtcNow;
                await _newDbContext.SaveChangesAsync();

                return new MaintenanceContractDTO
                {
                    Id = contract.Id,
                    ContractNumber = contract.ContractNumber,
                    ClientId = contract.ClientId,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    ContractValue = contract.TotalAmount,
                    Status = contract.Status.ToString(),
                    ContractType = "Standard", // Default contract type
                    PaymentTerms = contract.PaymentTerms,
                    CreatedAt = contract.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", contractId);
                throw;
            }
        }

        public async Task<bool> DeleteContractAsync(string contractId)
        {
            try
            {
                var contract = await _newDbContext.MaintenanceContracts.FindAsync(contractId);
                if (contract == null)
                    return false;

                _newDbContext.MaintenanceContracts.Remove(contract);
                await _newDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", contractId);
                throw;
            }
        }
        #endregion

        #region Dashboard & Statistics
        public async Task<MaintenanceDashboardStatsDTO> GetMaintenanceDashboardStatsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var totalCustomers = await _newDbContext.Clients.CountAsync();
                var totalEquipment = await _newDbContext.Equipment.CountAsync();
                var totalVisits = await _newDbContext.MaintenanceVisits.CountAsync();
                var monthlyVisits = await _newDbContext.MaintenanceVisits
                    .Where(v => v.ScheduledDate >= startOfMonth && v.ScheduledDate <= endOfMonth)
                    .CountAsync();
                var pendingVisits = await _newDbContext.MaintenanceVisits
                    .Where(v => v.Status == VisitStatus.PendingApproval)
                    .CountAsync();
                var completedVisits = await _newDbContext.MaintenanceVisits
                    .Where(v => v.Status == VisitStatus.Completed)
                    .CountAsync();
                var activeContracts = await _newDbContext.MaintenanceContracts
                    .Where(mc => mc.Status == ContractStatus.Signed && mc.EndDate >= now)
                    .CountAsync();

                var visitCompletionRate = totalVisits > 0 ? (completedVisits * 100.0 / totalVisits) : 0;

                return new MaintenanceDashboardStatsDTO
                {
                    TotalCustomers = totalCustomers,
                    TotalEquipment = totalEquipment,
                    TotalVisits = totalVisits,
                    MonthlyVisits = monthlyVisits,
                    PendingVisits = pendingVisits,
                    CompletedVisits = completedVisits,
                    ActiveContracts = activeContracts,
                    VisitCompletionRate = Math.Round(visitCompletionRate, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }
        }
        #endregion

        #region Placeholder Methods (Not Implemented Yet)
        public Task<object> GetVisitReportsAsync(string visitId)
        {
            throw new NotImplementedException("Visit reports not implemented yet");
        }

        public Task<object> GetMediaFilesAsync(string visitId)
        {
            throw new NotImplementedException("Media files not implemented yet");
        }

        public Task<object> CreateMediaFileAsync(object request)
        {
            throw new NotImplementedException("Media file creation not implemented yet");
        }

        public Task<object> DeleteMediaFileAsync(string fileId)
        {
            throw new NotImplementedException("Media file deletion not implemented yet");
        }

        public Task<object> GetSparePartsAsync(PagedRequest request)
        {
            throw new NotImplementedException("Spare parts not implemented yet");
        }

        public Task<object> CreateSparePartAsync(object request)
        {
            throw new NotImplementedException("Spare part creation not implemented yet");
        }

        public Task<object> UpdateSparePartAsync(string partId, object request)
        {
            throw new NotImplementedException("Spare part update not implemented yet");
        }

        public Task<object> DeleteSparePartAsync(string partId)
        {
            throw new NotImplementedException("Spare part deletion not implemented yet");
        }

        public Task<object> GetUsedSparePartsAsync(string visitId)
        {
            throw new NotImplementedException("Used spare parts not implemented yet");
        }

        public Task<object> CreateInvoiceAsync(object request)
        {
            throw new NotImplementedException("Invoice creation not implemented yet");
        }

        public Task<object> GetInvoiceAsync(string invoiceId)
        {
            throw new NotImplementedException("Invoice retrieval not implemented yet");
        }

        public Task<object> UpdateInvoiceAsync(string invoiceId, object request)
        {
            throw new NotImplementedException("Invoice update not implemented yet");
        }

        public Task<object> DeleteInvoiceAsync(string invoiceId)
        {
            throw new NotImplementedException("Invoice deletion not implemented yet");
        }

        public Task<object> GetPaymentsAsync(string invoiceId)
        {
            throw new NotImplementedException("Payments retrieval not implemented yet");
        }

        public Task<object> CreatePaymentAsync(object request)
        {
            throw new NotImplementedException("Payment creation not implemented yet");
        }

        public Task<object> GetEngineersAsync(PagedRequest request)
        {
            throw new NotImplementedException("Engineers retrieval not implemented yet");
        }

        public Task<object> CreateEngineerAsync(object request)
        {
            throw new NotImplementedException("Engineer creation not implemented yet");
        }

        public Task<object> UpdateEngineerAsync(string engineerId, object request)
        {
            throw new NotImplementedException("Engineer update not implemented yet");
        }

        public Task<object> DeleteEngineerAsync(string engineerId)
        {
            throw new NotImplementedException("Engineer deletion not implemented yet");
        }

        public Task<object> MigrateLegacyDataAsync()
        {
            throw new NotImplementedException("Data migration not implemented yet");
        }

        public Task<object> SyncLegacyDataAsync()
        {
            throw new NotImplementedException("Data synchronization not implemented yet");
        }

        public Task<object> ValidateDataIntegrityAsync()
        {
            throw new NotImplementedException("Data validation not implemented yet");
        }

        public Task<object> GetAuditLogsAsync(PagedRequest request)
        {
            throw new NotImplementedException("Audit logs not implemented yet");
        }

        public Task<object> GetNotificationsAsync(string userId)
        {
            throw new NotImplementedException("Notifications not implemented yet");
        }

        public Task<object> CreateNotificationAsync(object request)
        {
            throw new NotImplementedException("Notification creation not implemented yet");
        }

        public Task<object> BackupDataAsync()
        {
            throw new NotImplementedException("Data backup not implemented yet");
        }

        public Task<object> RestoreDataAsync(object request)
        {
            throw new NotImplementedException("Data restore not implemented yet");
        }

        public Task<SystemHealthDTO> GetSystemHealthAsync()
        {
            throw new NotImplementedException("System health not implemented yet");
        }

        public Task<BackupStatusDTO> CreateBackupAsync()
        {
            throw new NotImplementedException("Backup creation not implemented yet");
        }

        public Task<bool> RestoreBackupAsync(string backupId)
        {
            throw new NotImplementedException("Backup restore not implemented yet");
        }

        // Missing interface implementations - stub methods
        public Task<VisitReportDTO> UpdateVisitReportAsync(string visitId, UpdateVisitReportDTO request)
        {
            throw new NotImplementedException("Update visit report not implemented yet");
        }

        public Task<List<MediaFileDTO>> GetVisitMediaFilesAsync(string visitId)
        {
            throw new NotImplementedException("Get visit media files not implemented yet");
        }

        public Task<MediaFileDTO> UploadVisitMediaAsync(string visitId, UploadMediaDTO media)
        {
            throw new NotImplementedException("Upload visit media not implemented yet");
        }

        public Task<bool> DeleteVisitMediaAsync(string visitId, string mediaId)
        {
            throw new NotImplementedException("Delete visit media not implemented yet");
        }

        public Task<CustomerVisitStatsDTO> GetCustomerVisitStatsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Get customer visit stats not implemented yet");
        }

        public Task<EquipmentVisitStatsDTO> GetEquipmentVisitStatsAsync(string equipmentId)
        {
            throw new NotImplementedException("Get equipment visit stats not implemented yet");
        }

        public Task<MaintenanceDashboardDTO> GetMaintenanceDashboardAsync()
        {
            throw new NotImplementedException("Get maintenance dashboard not implemented yet");
        }

        public Task<List<MonthlyVisitStatsDTO>> GetMonthlyVisitStatsAsync(int months = 12)
        {
            throw new NotImplementedException("Get monthly visit stats not implemented yet");
        }

        public Task<RevenueReportDTO> GetRevenueReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Get revenue report not implemented yet");
        }

        public Task<List<EngineerDTO>> GetAvailableEngineersAsync(DateTime visitDate, string specialization = null)
        {
            throw new NotImplementedException("Get available engineers not implemented yet");
        }

        public Task<EngineerWorkloadDTO> GetEngineerWorkloadAsync(string engineerId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException("Get engineer workload not implemented yet");
        }

        public Task<bool> AssignEngineerAsync(string visitId, string engineerId)
        {
            throw new NotImplementedException("Assign engineer not implemented yet");
        }

        public Task<List<EngineerPerformanceDTO>> GetEngineerPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Get engineer performance not implemented yet");
        }

        public Task<List<SparePartDTO>> GetSparePartsAsync(string equipmentId = null)
        {
            throw new NotImplementedException("Get spare parts not implemented yet");
        }

        public Task<SparePartDTO> CreateSparePartAsync(CreateSparePartDTO part)
        {
            throw new NotImplementedException("Create spare part not implemented yet");
        }

        public Task<SparePartDTO> UpdateSparePartAsync(string partId, UpdateSparePartDTO part)
        {
            throw new NotImplementedException("Update spare part not implemented yet");
        }

        public Task<bool> RecordPartsUsageAsync(string visitId, List<PartsUsageDTO> partsUsage)
        {
            throw new NotImplementedException("Record parts usage not implemented yet");
        }

        public Task<List<PartsUsageDTO>> GetVisitPartsUsageAsync(string visitId)
        {
            throw new NotImplementedException("Get visit parts usage not implemented yet");
        }

        public Task<InvoiceDTO> GenerateVisitInvoiceAsync(string visitId)
        {
            throw new NotImplementedException("Generate visit invoice not implemented yet");
        }

        public Task<InvoiceDTO> CreateInvoiceAsync(CreateInvoiceDTO invoice)
        {
            throw new NotImplementedException("Create invoice not implemented yet");
        }

        public Task<InvoiceDTO> UpdateInvoiceAsync(string invoiceId, UpdateInvoiceDTO invoice)
        {
            throw new NotImplementedException("Update invoice not implemented yet");
        }

        public Task<List<InvoiceDTO>> GetCustomerInvoicesAsync(string customerId)
        {
            throw new NotImplementedException("Get customer invoices not implemented yet");
        }

        public Task<bool> ProcessPaymentAsync(string invoiceId, ProcessPaymentDTO payment)
        {
            throw new NotImplementedException("Process payment not implemented yet");
        }

        public Task<MigrationStatusDTO> GetMigrationStatusAsync()
        {
            throw new NotImplementedException("Get migration status not implemented yet");
        }

        public Task<MigrationResultDTO> MigrateCustomersAsync(int batchSize = 100)
        {
            throw new NotImplementedException("Migrate customers not implemented yet");
        }

        public Task<MigrationResultDTO> MigrateEquipmentAsync(int batchSize = 100)
        {
            throw new NotImplementedException("Migrate equipment not implemented yet");
        }

        public Task<MigrationResultDTO> MigrateVisitsAsync(int batchSize = 100)
        {
            throw new NotImplementedException("Migrate visits not implemented yet");
        }

        public Task<MigrationResultDTO> MigrateContractsAsync(int batchSize = 100)
        {
            throw new NotImplementedException("Migrate contracts not implemented yet");
        }

        public Task<SyncResultDTO> SyncNewDataAsync()
        {
            throw new NotImplementedException("Sync new data not implemented yet");
        }

        public Task<DataConsistencyReportDTO> VerifyDataConsistencyAsync()
        {
            throw new NotImplementedException("Verify data consistency not implemented yet");
        }

        public Task<bool> ValidateBusinessRulesAsync(string entityType, object entity)
        {
            throw new NotImplementedException("Validate business rules not implemented yet");
        }

        public Task<List<AuditLogDTO>> GetAuditLogsAsync(string entityId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Get audit logs not implemented yet");
        }

        // Additional missing interface implementations
        public Task<EquipmentDTO> CreateEquipmentAsync(CreateEquipmentDTO request)
        {
            throw new NotImplementedException("Create equipment not implemented yet");
        }

        public Task<EquipmentDTO> UpdateEquipmentAsync(string equipmentId, UpdateEquipmentDTO request)
        {
            throw new NotImplementedException("Update equipment not implemented yet");
        }

        public Task<VisitCompletionResponseDTO> CompleteVisitAsync(CompleteVisitRequestDTO request)
        {
            throw new NotImplementedException("Complete visit not implemented yet");
        }

        public Task<MaintenanceVisitDTO> GetVisitByIdAsync(string visitId)
        {
            throw new NotImplementedException("Get visit by ID not implemented yet");
        }

        public Task<List<MaintenanceVisitDTO>> GetCustomerVisitsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("Get customer visits not implemented yet");
        }

        public Task<MaintenanceVisitDTO> ScheduleVisitAsync(ScheduleVisitRequestDTO request)
        {
            throw new NotImplementedException("Schedule visit not implemented yet");
        }

        public Task<MaintenanceVisitDTO> UpdateVisitAsync(string visitId, UpdateVisitRequestDTO request)
        {
            throw new NotImplementedException("Update visit not implemented yet");
        }

        public Task<bool> CancelVisitAsync(string visitId, string reason)
        {
            throw new NotImplementedException("Cancel visit not implemented yet");
        }

        public Task<MaintenanceContractDTO> GetContractByIdAsync(string contractId)
        {
            throw new NotImplementedException("Get contract by ID not implemented yet");
        }

        public Task<List<MaintenanceContractDTO>> GetCustomerContractsAsync(string customerId)
        {
            throw new NotImplementedException("Get customer contracts not implemented yet");
        }

        public Task<MaintenanceContractDTO> CreateContractAsync(CreateMaintenanceContractDTO request)
        {
            throw new NotImplementedException("Create contract not implemented yet");
        }

        public Task<MaintenanceContractDTO> UpdateContractAsync(string contractId, UpdateMaintenanceContractDTO request)
        {
            throw new NotImplementedException("Update contract not implemented yet");
        }

        public Task<List<MaintenanceContractDTO>> GetExpiringContractsAsync(int days = 30)
        {
            throw new NotImplementedException("Get expiring contracts not implemented yet");
        }

        public Task<bool> RenewContractAsync(string contractId, RenewContractRequestDTO request)
        {
            throw new NotImplementedException("Renew contract not implemented yet");
        }

        public Task<VisitReportDTO> GetVisitReportAsync(string visitId)
        {
            throw new NotImplementedException("Get visit report not implemented yet");
        }

        public Task<VisitReportDTO> CreateVisitReportAsync(CreateVisitReportDTO request)
        {
            throw new NotImplementedException("Create visit report not implemented yet");
        }

        public Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO request)
        {
            throw new NotImplementedException("Create customer not implemented yet");
        }

        public Task<CustomerDTO> UpdateCustomerAsync(string customerId, UpdateCustomerDTO request)
        {
            throw new NotImplementedException("Update customer not implemented yet");
        }

        public Task<EquipmentVisitsDTO> GetEquipmentVisitsAsync(string equipmentIdentifier, bool includeLegacy = true)
        {
            throw new NotImplementedException("Get equipment visits not implemented yet");
        }

        public Task<List<MaintenanceVisitDTO>> GetEquipmentVisitsAsync(string equipmentId)
        {
            throw new NotImplementedException("Get equipment visits not implemented yet");
        }

        // Additional missing interface implementations
        public Task<CustomerDTO> GetCustomerByIdAsync(string customerId)
        {
            throw new NotImplementedException("Get customer by ID not implemented yet");
        }

        public Task<EquipmentDTO> GetEquipmentByIdAsync(string equipmentId)
        {
            throw new NotImplementedException("Get equipment by ID not implemented yet");
        }

        public Task<List<EquipmentDTO>> GetCustomerEquipmentAsync(string customerId)
        {
            throw new NotImplementedException("Get customer equipment not implemented yet");
        }

        public Task<EquipmentDTO> UpdateEquipmentStatusAsync(string equipmentId, string status)
        {
            throw new NotImplementedException("Update equipment status not implemented yet");
        }
        #endregion
    }
}
