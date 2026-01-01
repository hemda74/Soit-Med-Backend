using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoitMed.Models;
using SoitMed.Models.Enums;
using SoitMed.Models.Equipment;
using SoitMed.Models.Legacy;
using SoitMed.Models.Identity;
using SoitMed.Common;
using System.Text;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for importing legacy data from old SOIT system to SoitMed
    /// </summary>
    public class LegacyImporterService : ILegacyImporterService
    {
        private readonly Context _context;
        private readonly ILogger<LegacyImporterService> _logger;
        private readonly string _logDirectory;
        private readonly ConnectionSettings _connectionSettings;

        public LegacyImporterService(
            Context context,
            ILogger<LegacyImporterService> logger,
            IOptions<ConnectionSettings> connectionSettings)
        {
            _context = context;
            _logger = logger;
            _connectionSettings = connectionSettings.Value;
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LegacyImport");
            
            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _logger.LogInformation("LegacyImporterService initialized. Mode: {Mode}, MediaPath: {MediaPath}", 
                _connectionSettings.Mode, _connectionSettings.GetActiveMediaPath());
        }

        /// <summary>
        /// Import all legacy data
        /// </summary>
        public async Task<ImportResult> ImportAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new ImportResult();
            var logFileName = $"LegacyImport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";
            result.LogFilePath = Path.Combine(_logDirectory, logFileName);

            _logger.LogInformation("Starting legacy data import. Log file: {LogFile}", result.LogFilePath);

            using var logWriter = new StreamWriter(result.LogFilePath, append: false, Encoding.UTF8);
            await logWriter.WriteLineAsync($"Legacy Data Import Started: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            await logWriter.WriteLineAsync("=".PadRight(80, '='));
            await logWriter.FlushAsync();

            try
            {
                // Step 1: Import Clients
                _logger.LogInformation("Step 1: Importing Clients...");
                await logWriter.WriteLineAsync("\n[STEP 1] Importing Clients");
                await logWriter.FlushAsync();
                var clientsResult = await ImportClientsAsync(cancellationToken, logWriter);
                result.SuccessCount += clientsResult.SuccessCount;
                result.FailureCount += clientsResult.FailureCount;
                result.SkippedCount += clientsResult.SkippedCount;
                result.Errors.AddRange(clientsResult.Errors);

                // Step 2: Import Equipment
                _logger.LogInformation("Step 2: Importing Equipment...");
                await logWriter.WriteLineAsync("\n[STEP 2] Importing Equipment");
                await logWriter.FlushAsync();
                var equipmentResult = await ImportEquipmentAsync(cancellationToken, logWriter);
                result.SuccessCount += equipmentResult.SuccessCount;
                result.FailureCount += equipmentResult.FailureCount;
                result.SkippedCount += equipmentResult.SkippedCount;
                result.Errors.AddRange(equipmentResult.Errors);

                // Step 3: Import Maintenance Visits
                _logger.LogInformation("Step 3: Importing Maintenance Visits...");
                await logWriter.WriteLineAsync("\n[STEP 3] Importing Maintenance Visits");
                await logWriter.FlushAsync();
                var visitsResult = await ImportMaintenanceVisitsAsync(cancellationToken, logWriter);
                result.SuccessCount += visitsResult.SuccessCount;
                result.FailureCount += visitsResult.FailureCount;
                result.SkippedCount += visitsResult.SkippedCount;
                result.Errors.AddRange(visitsResult.Errors);

                await logWriter.WriteLineAsync("\n" + "=".PadRight(80, '='));
                await logWriter.WriteLineAsync($"Import Completed: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                await logWriter.WriteLineAsync($"Total Success: {result.SuccessCount}");
                await logWriter.WriteLineAsync($"Total Failures: {result.FailureCount}");
                await logWriter.WriteLineAsync($"Total Skipped: {result.SkippedCount}");
                await logWriter.FlushAsync();

                _logger.LogInformation("Legacy data import completed. Success: {Success}, Failures: {Failures}, Skipped: {Skipped}",
                    result.SuccessCount, result.FailureCount, result.SkippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during legacy data import");
                await logWriter.WriteLineAsync($"\nFATAL ERROR: {ex.Message}");
                await logWriter.WriteLineAsync($"Stack Trace: {ex.StackTrace}");
                await logWriter.FlushAsync();
                result.Errors.Add($"Fatal error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Import clients from legacy Stk_Customers table
        /// </summary>
        public async Task<ImportResult> ImportClientsAsync(CancellationToken cancellationToken = default, StreamWriter? logWriter = null)
        {
            var result = new ImportResult();
            var log = logWriter ?? new StreamWriter(Path.Combine(_logDirectory, $"ClientsImport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log"), append: false, Encoding.UTF8);

            try
            {
                // Get all legacy customers using AsNoTracking for read-only access
                var legacyCustomers = await _context.LegacyCustomers
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} legacy customers to import", legacyCustomers.Count);
                await log.WriteLineAsync($"Found {legacyCustomers.Count} legacy customers to process");

                foreach (var legacyCustomer in legacyCustomers)
                {
                    try
                    {
                        // Check if client already exists (by LegacyCustomerId)
                        var existingClient = await _context.Clients
                            .FirstOrDefaultAsync(c => c.LegacyCustomerId == legacyCustomer.CusId, cancellationToken);

                        if (existingClient != null)
                        {
                            result.SkippedCount++;
                            await log.WriteLineAsync($"SKIPPED: Client with LegacyCustomerId={legacyCustomer.CusId} already exists (Id: {existingClient.Id})");
                            continue;
                        }

                        // Create new client
                        var client = new Client
                        {
                            Name = legacyCustomer.CusName ?? "Unknown",
                            Phone = legacyCustomer.CusMobile ?? legacyCustomer.CusPhone,
                            Email = legacyCustomer.CusEmail,
                            Address = legacyCustomer.CusAddress,
                            City = legacyCustomer.CusCity,
                            Governorate = legacyCustomer.CusGovernorate,
                            Notes = legacyCustomer.CusNotes,
                            LegacyCustomerId = legacyCustomer.CusId,
                            RelatedUserId = null, // Manual linking later
                            Status = ClientStatus.Active,
                            Priority = ClientPriority.Medium,
                            CreatedBy = "LegacyImport", // System user
                            CreatedAt = legacyCustomer.CusCreatedDate ?? DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Clients.Add(client);
                        await _context.SaveChangesAsync(cancellationToken);

                        result.SuccessCount++;
                        await log.WriteLineAsync($"SUCCESS: Imported client CusId={legacyCustomer.CusId} -> ClientId={client.Id} (Name: {client.Name})");
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        var errorMsg = $"FAILED: Client CusId={legacyCustomer.CusId} - {ex.Message}";
                        result.Errors.Add(errorMsg);
                        await log.WriteLineAsync(errorMsg);
                        _logger.LogError(ex, "Error importing client CusId={CusId}", legacyCustomer.CusId);
                    }
                }

                await log.FlushAsync();
                _logger.LogInformation("Clients import completed. Success: {Success}, Failures: {Failures}, Skipped: {Skipped}",
                    result.SuccessCount, result.FailureCount, result.SkippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clients import");
                await log.WriteLineAsync($"FATAL ERROR: {ex.Message}");
                result.Errors.Add($"Fatal error: {ex.Message}");
            }
            finally
            {
                if (logWriter == null)
                {
                    log.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Import equipment from legacy Stk_Order_Out_Items table
        /// </summary>
        public async Task<ImportResult> ImportEquipmentAsync(CancellationToken cancellationToken = default, StreamWriter? logWriter = null)
        {
            var result = new ImportResult();
            var log = logWriter ?? new StreamWriter(Path.Combine(_logDirectory, $"EquipmentImport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log"), append: false, Encoding.UTF8);

            try
            {
                // Get all legacy order out items using AsNoTracking
                var legacyItems = await _context.LegacyOrderOutItems
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} legacy equipment items to import", legacyItems.Count);
                await log.WriteLineAsync($"Found {legacyItems.Count} legacy equipment items to process");

                foreach (var legacyItem in legacyItems)
                {
                    try
                    {
                        // Check if equipment already exists (by LegacySourceId)
                        var existingEquipment = await _context.Equipment
                            .FirstOrDefaultAsync(e => e.LegacySourceId == legacyItem.OoiId.ToString(), cancellationToken);

                        if (existingEquipment != null)
                        {
                            result.SkippedCount++;
                            await log.WriteLineAsync($"SKIPPED: Equipment with LegacySourceId={legacyItem.OoiId} already exists (Id: {existingEquipment.Id})");
                            continue;
                        }

                        // Find the client using LegacyCustomerId
                        Client? client = null;
                        if (legacyItem.CusId.HasValue)
                        {
                            client = await _context.Clients
                                .FirstOrDefaultAsync(c => c.LegacyCustomerId == legacyItem.CusId.Value, cancellationToken);

                            if (client == null)
                            {
                                result.SkippedCount++;
                                var errorMsg = $"SKIPPED: Equipment OoiId={legacyItem.OoiId} - Client with LegacyCustomerId={legacyItem.CusId} not found";
                                result.Errors.Add(errorMsg);
                                await log.WriteLineAsync(errorMsg);
                                continue;
                            }
                        }

                        // Generate QR Token
                        var qrToken = Guid.NewGuid();

                        // Create new equipment
                        var equipment = new Equipment
                        {
                            Name = legacyItem.ItemName ?? "Unknown Equipment",
                            QRCode = legacyItem.SerialNum ?? qrToken.ToString(), // Use SerialNum as QRCode if available
                            Description = legacyItem.ItemDescription,
                            Model = legacyItem.ItemModel,
                            Manufacturer = legacyItem.ItemManufacturer,
                            PurchaseDate = legacyItem.PurchaseDate,
                            WarrantyExpiry = legacyItem.WarrantyExpiry,
                            CustomerId = client?.Id.ToString(), // Link to client if found
                            QrToken = qrToken,
                            IsQrPrinted = false,
                            QrLastPrintedDate = null,
                            LegacySourceId = legacyItem.OoiId.ToString(), // Store OOI_ID
                            Status = EquipmentStatus.Operational,
                            CreatedAt = legacyItem.CreatedDate ?? DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Equipment.Add(equipment);
                        await _context.SaveChangesAsync(cancellationToken);

                        result.SuccessCount++;
                        await log.WriteLineAsync($"SUCCESS: Imported equipment OoiId={legacyItem.OoiId} -> EquipmentId={equipment.Id} (Name: {equipment.Name}, ClientId: {equipment.CustomerId})");
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        var errorMsg = $"FAILED: Equipment OoiId={legacyItem.OoiId} - {ex.Message}";
                        result.Errors.Add(errorMsg);
                        await log.WriteLineAsync(errorMsg);
                        _logger.LogError(ex, "Error importing equipment OoiId={OoiId}", legacyItem.OoiId);
                    }
                }

                await log.FlushAsync();
                _logger.LogInformation("Equipment import completed. Success: {Success}, Failures: {Failures}, Skipped: {Skipped}",
                    result.SuccessCount, result.FailureCount, result.SkippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during equipment import");
                await log.WriteLineAsync($"FATAL ERROR: {ex.Message}");
                result.Errors.Add($"Fatal error: {ex.Message}");
            }
            finally
            {
                if (logWriter == null)
                {
                    log.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Import maintenance visits from legacy MNT_Visiting table
        /// </summary>
        public async Task<ImportResult> ImportMaintenanceVisitsAsync(CancellationToken cancellationToken = default, StreamWriter? logWriter = null)
        {
            var result = new ImportResult();
            var log = logWriter ?? new StreamWriter(Path.Combine(_logDirectory, $"MaintenanceVisitsImport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.log"), append: false, Encoding.UTF8);

            try
            {
                // Get all legacy maintenance visits using AsNoTracking
                var legacyVisits = await _context.LegacyMaintenanceVisits
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} legacy maintenance visits to import", legacyVisits.Count);
                await log.WriteLineAsync($"Found {legacyVisits.Count} legacy maintenance visits to process");

                foreach (var legacyVisit in legacyVisits)
                {
                    try
                    {
                        // Check if visit already exists (by LegacyVisitId)
                        var existingVisit = await _context.MaintenanceVisits
                            .FirstOrDefaultAsync(v => v.LegacyVisitId == legacyVisit.VisitingId, cancellationToken);

                        if (existingVisit != null)
                        {
                            result.SkippedCount++;
                            await log.WriteLineAsync($"SKIPPED: Visit with LegacyVisitId={legacyVisit.VisitingId} already exists (Id: {existingVisit.Id})");
                            continue;
                        }

                        // Find equipment using LegacySourceId
                        Equipment? equipment = null;
                        if (legacyVisit.OoiId.HasValue)
                        {
                            equipment = await _context.Equipment
                                .FirstOrDefaultAsync(e => e.LegacySourceId == legacyVisit.OoiId.Value.ToString(), cancellationToken);

                            if (equipment == null)
                            {
                                result.SkippedCount++;
                                var errorMsg = $"SKIPPED: Visit VisitingId={legacyVisit.VisitingId} - Equipment with LegacySourceId={legacyVisit.OoiId} not found";
                                result.Errors.Add(errorMsg);
                                await log.WriteLineAsync(errorMsg);
                                continue;
                            }
                        }
                        else
                        {
                            result.SkippedCount++;
                            var errorMsg = $"SKIPPED: Visit VisitingId={legacyVisit.VisitingId} - OoiId is null";
                            result.Errors.Add(errorMsg);
                            await log.WriteLineAsync(errorMsg);
                            continue;
                        }

                        // Find customer (ApplicationUser) - try to find via Client first
                        ApplicationUser? customer = null;
                        if (equipment.CustomerId != null)
                        {
                            customer = await _context.Users
                                .FirstOrDefaultAsync(u => u.Id == equipment.CustomerId, cancellationToken);
                        }

                        // If no customer found via Equipment, try to find via legacy CusId
                        if (customer == null && legacyVisit.CusId.HasValue)
                        {
                            var client = await _context.Clients
                                .FirstOrDefaultAsync(c => c.LegacyCustomerId == legacyVisit.CusId.Value, cancellationToken);
                            
                            if (client?.RelatedUserId != null)
                            {
                                customer = await _context.Users
                                    .FirstOrDefaultAsync(u => u.Id == client.RelatedUserId, cancellationToken);
                            }
                        }

                        // If still no customer, create a placeholder or skip
                        if (customer == null)
                        {
                            result.SkippedCount++;
                            var errorMsg = $"SKIPPED: Visit VisitingId={legacyVisit.VisitingId} - Customer not found for EquipmentId={equipment.Id}";
                            result.Errors.Add(errorMsg);
                            await log.WriteLineAsync(errorMsg);
                            continue;
                        }

                        // Map legacy status to VisitStatus enum
                        var visitStatus = MapLegacyStatusToVisitStatus(legacyVisit.VisitingStatus);

                        // Create maintenance request if needed (or find existing)
                        var maintenanceRequest = await _context.MaintenanceRequests
                            .FirstOrDefaultAsync(mr => mr.EquipmentId == equipment.Id && 
                                                      mr.CustomerId == customer.Id, cancellationToken);

                        if (maintenanceRequest == null)
                        {
                            maintenanceRequest = new MaintenanceRequest
                            {
                                CustomerId = customer.Id,
                                EquipmentId = equipment.Id,
                                Description = $"Legacy import from VisitingId={legacyVisit.VisitingId}",
                                Status = MaintenanceRequestStatus.Completed,
                                CreatedAt = legacyVisit.CreatedDate ?? DateTime.UtcNow,
                                IsActive = true
                            };
                            _context.MaintenanceRequests.Add(maintenanceRequest);
                            await _context.SaveChangesAsync(cancellationToken);
                        }

                        // Generate ticket number
                        var ticketNumber = $"LEGACY-{legacyVisit.VisitingId:D6}";

                        // Find engineer by name (if possible) or use a default
                        var engineer = await _context.Users
                            .Where(u => u.UserName.Contains(legacyVisit.EngineerName ?? "") || 
                                       u.Email.Contains(legacyVisit.EngineerName ?? ""))
                            .FirstOrDefaultAsync(cancellationToken);

                        if (engineer == null)
                        {
                            // Get first engineer user as fallback
                            engineer = await _context.Users
                                .FirstOrDefaultAsync(cancellationToken);
                        }

                        if (engineer == null)
                        {
                            result.SkippedCount++;
                            var errorMsg = $"SKIPPED: Visit VisitingId={legacyVisit.VisitingId} - No engineer found";
                            result.Errors.Add(errorMsg);
                            await log.WriteLineAsync(errorMsg);
                            continue;
                        }

                        // Create maintenance visit
                        var visit = new MaintenanceVisit
                        {
                            TicketNumber = ticketNumber,
                            MaintenanceRequestId = maintenanceRequest.Id,
                            CustomerId = customer.Id,
                            DeviceId = equipment.Id,
                            ScheduledDate = legacyVisit.VisitingDate ?? DateTime.UtcNow,
                            VisitDate = legacyVisit.VisitingDate ?? DateTime.UtcNow,
                            Origin = VisitOrigin.ManualSales, // Legacy data
                            Status = visitStatus,
                            EngineerId = engineer.Id,
                            Report = legacyVisit.VisitingNotes,
                            ActionsTaken = legacyVisit.ActionsTaken,
                            PartsUsed = legacyVisit.PartsUsed,
                            ServiceFee = legacyVisit.ServiceFee,
                            LegacyVisitId = legacyVisit.VisitingId,
                            Outcome = MapLegacyStatusToOutcome(legacyVisit.VisitingStatus),
                            IsActive = true
                        };

                        // Set completion dates if status is Completed
                        if (visitStatus == VisitStatus.Completed)
                        {
                            visit.CompletedAt = legacyVisit.VisitingDate ?? DateTime.UtcNow;
                            visit.StartedAt = legacyVisit.VisitingDate?.AddHours(-1) ?? DateTime.UtcNow;
                        }

                        _context.MaintenanceVisits.Add(visit);
                        await _context.SaveChangesAsync(cancellationToken);

                        result.SuccessCount++;
                        await log.WriteLineAsync($"SUCCESS: Imported visit VisitingId={legacyVisit.VisitingId} -> VisitId={visit.Id} (EquipmentId: {equipment.Id})");
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        var errorMsg = $"FAILED: Visit VisitingId={legacyVisit.VisitingId} - {ex.Message}";
                        result.Errors.Add(errorMsg);
                        await log.WriteLineAsync(errorMsg);
                        _logger.LogError(ex, "Error importing visit VisitingId={VisitingId}", legacyVisit.VisitingId);
                    }
                }

                await log.FlushAsync();
                _logger.LogInformation("Maintenance visits import completed. Success: {Success}, Failures: {Failures}, Skipped: {Skipped}",
                    result.SuccessCount, result.FailureCount, result.SkippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during maintenance visits import");
                await log.WriteLineAsync($"FATAL ERROR: {ex.Message}");
                result.Errors.Add($"Fatal error: {ex.Message}");
            }
            finally
            {
                if (logWriter == null)
                {
                    log.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Map legacy status string to VisitStatus enum
        /// </summary>
        private VisitStatus MapLegacyStatusToVisitStatus(string? legacyStatus)
        {
            if (string.IsNullOrWhiteSpace(legacyStatus))
                return VisitStatus.Scheduled;

            var status = legacyStatus.Trim().ToLowerInvariant();

            return status switch
            {
                "completed" or "done" or "finished" => VisitStatus.Completed,
                "in progress" or "inprogress" or "ongoing" => VisitStatus.InProgress,
                "pending" or "waiting" => VisitStatus.PendingApproval,
                "scheduled" or "scheduled" => VisitStatus.Scheduled,
                "needs parts" or "needsparts" or "parts needed" => VisitStatus.NeedsSpareParts,
                "rescheduled" or "postponed" => VisitStatus.Rescheduled,
                "cancelled" or "canceled" => VisitStatus.Cancelled,
                _ => VisitStatus.Scheduled // Default
            };
        }

        /// <summary>
        /// Map legacy status to MaintenanceVisitOutcome enum
        /// </summary>
        private MaintenanceVisitOutcome MapLegacyStatusToOutcome(string? legacyStatus)
        {
            if (string.IsNullOrWhiteSpace(legacyStatus))
                return MaintenanceVisitOutcome.Completed;

            var status = legacyStatus.Trim().ToLowerInvariant();

            return status switch
            {
                "completed" or "done" or "finished" => MaintenanceVisitOutcome.Completed,
                "needs parts" or "needsparts" or "parts needed" => MaintenanceVisitOutcome.NeedsSparePart,
                "needs second visit" or "needssecondvisit" => MaintenanceVisitOutcome.NeedsSecondVisit,
                "cannot complete" or "cannotcomplete" => MaintenanceVisitOutcome.CannotComplete,
                _ => MaintenanceVisitOutcome.Completed // Default
            };
        }
    }
}

