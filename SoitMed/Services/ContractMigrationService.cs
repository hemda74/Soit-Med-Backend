using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Contract;
using SoitMed.Models.Enums;
using SoitMed.Models.Legacy;
using SoitMed.Repositories;
using ContractEntity = SoitMed.Models.Contract.Contract;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for migrating contracts from legacy TBS database to new ITIWebApi44 database
    /// Implements idempotency checks and media path transformation
    /// </summary>
    public class ContractMigrationService : IContractMigrationService
    {
        private readonly Context _context;
        private readonly TbsDbContext _tbsContext;
        private readonly MediaPathTransformer _mediaPathTransformer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContractMigrationService> _logger;

        public ContractMigrationService(
            Context context,
            TbsDbContext tbsContext,
            MediaPathTransformer mediaPathTransformer,
            IUnitOfWork unitOfWork,
            ILogger<ContractMigrationService> logger)
        {
            _context = context;
            _tbsContext = tbsContext;
            _mediaPathTransformer = mediaPathTransformer;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MigrationResult> MigrateAllContractsAsync(string adminUserId)
        {
            var result = new MigrationResult();

            try
            {
                _logger.LogInformation("Starting migration of all contracts from TBS to ITIWebApi44");

                // Get all legacy contracts from TBS
                var legacyContracts = await _tbsContext.MntMaintenanceContracts
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Found {Count} legacy contracts to migrate", legacyContracts.Count);

                foreach (var legacyContract in legacyContracts)
                {
                    try
                    {
                        // Check idempotency: Skip if already migrated
                        var existingContract = await _context.Contracts
                            .FirstOrDefaultAsync(c => c.LegacyContractId == legacyContract.ContractId);

                        if (existingContract != null)
                        {
                            _logger.LogInformation("Contract {ContractId} already migrated, skipping", legacyContract.ContractId);
                            continue;
                        }

                        // Migrate single contract
                        var contractResult = await MigrateSingleContractAsync(legacyContract, adminUserId);
                        
                        if (contractResult.Success)
                        {
                            result.ContractsMigrated++;
                            result.InstallmentsMigrated += contractResult.InstallmentsMigrated;
                            result.NegotiationsCreated += contractResult.NegotiationsCreated;
                        }
                        else
                        {
                            result.Errors++;
                            result.ErrorMessages.AddRange(contractResult.ErrorMessages);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors++;
                        result.ErrorMessages.Add($"Error migrating contract {legacyContract.ContractId}: {ex.Message}");
                        _logger.LogError(ex, "Error migrating contract {ContractId}", legacyContract.ContractId);
                    }
                }

                result.Success = result.Errors == 0;
                result.Message = $"Migration completed: {result.ContractsMigrated} contracts migrated, {result.Errors} errors";

                _logger.LogInformation("Migration completed: {ContractsMigrated} contracts, {Errors} errors", 
                    result.ContractsMigrated, result.Errors);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during contract migration");
                result.Success = false;
                result.ErrorMessages.Add($"Fatal error: {ex.Message}");
                return result;
            }
        }

        public async Task<MigrationResult> MigrateContractAsync(int legacyContractId, string adminUserId)
        {
            var result = new MigrationResult();

            try
            {
                // Check idempotency
                var existingContract = await _context.Contracts
                    .FirstOrDefaultAsync(c => c.LegacyContractId == legacyContractId);

                if (existingContract != null)
                {
                    result.Success = false;
                    result.ErrorMessages.Add($"Contract with LegacyContractId {legacyContractId} already exists (Id: {existingContract.Id})");
                    return result;
                }

                // Get legacy contract from TBS
                var legacyContract = await _tbsContext.MntMaintenanceContracts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ContractId == legacyContractId);

                if (legacyContract == null)
                {
                    result.Success = false;
                    result.ErrorMessages.Add($"Legacy contract {legacyContractId} not found in TBS database");
                    return result;
                }

                result = await MigrateSingleContractAsync(legacyContract, adminUserId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating contract {ContractId}", legacyContractId);
                result.Success = false;
                result.ErrorMessages.Add($"Error: {ex.Message}");
                return result;
            }
        }

        private async Task<MigrationResult> MigrateSingleContractAsync(
            TbsMaintenanceContract legacyContract, 
            string adminUserId)
        {
            var result = new MigrationResult();

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Find or create Client from legacy CusId
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.LegacyCustomerId == legacyContract.CusId);

                if (client == null)
                {
                    // Try to find by name from TBS
                    var tbsCustomer = await _tbsContext.StkCustomers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CusId == legacyContract.CusId);

                    if (tbsCustomer != null)
                    {
                        client = await _context.Clients
                            .FirstOrDefaultAsync(c => c.Name == tbsCustomer.CusName);
                    }
                }

                if (client == null)
                {
                    result.Success = false;
                    result.ErrorMessages.Add($"Client not found for CusId {legacyContract.CusId}");
                    await _unitOfWork.RollbackTransactionAsync();
                    return result;
                }

                // Find or create Deal (if SoId exists)
                long? dealId = null;
                if (legacyContract.SoId.HasValue)
                {
                    var deal = await _context.SalesDeals
                        .FirstOrDefaultAsync(d => d.Id == legacyContract.SoId.Value);

                    if (deal == null)
                    {
                        // Create a placeholder deal if needed (or skip if not required)
                        _logger.LogWarning("Deal not found for SoId {SoId}, creating contract without deal", legacyContract.SoId);
                    }
                    else
                    {
                        dealId = deal.Id;
                    }
                }

                // If no deal, contract can still be created (DealId is nullable)
                // This allows migration of contracts that don't have associated sales deals

                // Determine contract status based on dates
                var contractStatus = DetermineContractStatus(legacyContract);

                // Transform media paths
                string? documentUrl = null;
                if (!string.IsNullOrWhiteSpace(legacyContract.ScFile))
                {
                    documentUrl = _mediaPathTransformer.TransformPath(legacyContract.ScFile);
                }

                // Create new Contract
                var contract = new ContractEntity
                {
                    DealId = dealId,
                    ContractNumber = legacyContract.ContractCode?.ToString() ?? $"LEG-{legacyContract.ContractId}",
                    Title = $"Maintenance Contract {legacyContract.ContractCode ?? legacyContract.ContractId}",
                    ContractContent = BuildContractContent(legacyContract),
                    DocumentUrl = documentUrl,
                    Status = contractStatus,
                    DraftedAt = legacyContract.StartDate,
                    SignedAt = contractStatus == ContractStatus.Signed ? legacyContract.StartDate : null,
                    ClientId = client.Id,
                    DraftedBy = adminUserId,
                    CashAmount = legacyContract.InstallmentAmount == null ? legacyContract.ContractTotalValue : null,
                    InstallmentAmount = legacyContract.InstallmentAmount,
                    InstallmentDurationMonths = legacyContract.InstallmentMonths,
                    LegacyContractId = legacyContract.ContractId,
                    CreatedAt = legacyContract.StartDate,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Contracts.AddAsync(contract);
                await _context.SaveChangesAsync();

                // Create system note in ContractNegotiations
                var negotiation = new ContractNegotiation
                {
                    ContractId = contract.Id,
                    ActionType = "System",
                    Notes = "Migrated from Legacy TBS",
                    SubmittedBy = adminUserId,
                    SubmitterRole = "System",
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.ContractNegotiations.AddAsync(negotiation);
                result.NegotiationsCreated = 1;

                // Migrate installments if payment plan exists
                if (legacyContract.InstallmentMonths.HasValue && legacyContract.InstallmentAmount.HasValue)
                {
                    var installments = await CreateInstallmentSchedulesAsync(
                        contract.Id, 
                        legacyContract.InstallmentAmount.Value,
                        legacyContract.InstallmentMonths.Value,
                        legacyContract.StartDate);

                    await _context.InstallmentSchedules.AddRangeAsync(installments);
                    result.InstallmentsMigrated = installments.Count;
                }

                await _context.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;
                result.ContractsMigrated = 1;
                result.Message = $"Contract {legacyContract.ContractId} migrated successfully";

                _logger.LogInformation("Successfully migrated contract {LegacyContractId} to new contract {ContractId}", 
                    legacyContract.ContractId, contract.Id);

                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error migrating contract {ContractId}", legacyContract.ContractId);
                result.Success = false;
                result.ErrorMessages.Add($"Error: {ex.Message}");
                return result;
            }
        }

        private ContractStatus DetermineContractStatus(TbsMaintenanceContract legacyContract)
        {
            var now = DateTime.UtcNow;

            // If end date has passed, mark as expired
            if (legacyContract.EndDate < now)
            {
                return ContractStatus.Expired;
            }

            // If contract has start date and is within valid period, mark as signed
            if (legacyContract.StartDate <= now && legacyContract.EndDate >= now)
            {
                return ContractStatus.Signed;
            }

            // Default to signed for completed legacy contracts
            return ContractStatus.Signed;
        }

        private string BuildContractContent(TbsMaintenanceContract legacyContract)
        {
            var content = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(legacyContract.NotesTech))
            {
                content.AppendLine($"Technical Notes: {legacyContract.NotesTech}");
            }

            if (!string.IsNullOrWhiteSpace(legacyContract.NotesFinance))
            {
                content.AppendLine($"Financial Notes: {legacyContract.NotesFinance}");
            }

            if (!string.IsNullOrWhiteSpace(legacyContract.NotesAdmin))
            {
                content.AppendLine($"Administrative Notes: {legacyContract.NotesAdmin}");
            }

            content.AppendLine($"Contract Code: {legacyContract.ContractCode}");
            content.AppendLine($"Classer Number: {legacyContract.ClasserNumber}");
            content.AppendLine($"Total Value: {legacyContract.ContractTotalValue}");

            return content.ToString();
        }

        private async Task<List<InstallmentSchedule>> CreateInstallmentSchedulesAsync(
            long contractId,
            decimal totalAmount,
            int installmentMonths,
            DateTime startDate)
        {
            var installments = new List<InstallmentSchedule>();
            var installmentAmount = totalAmount / installmentMonths;

            for (int i = 1; i <= installmentMonths; i++)
            {
                var dueDate = startDate.AddMonths(i);
                var installment = new InstallmentSchedule
                {
                    ContractId = contractId,
                    InstallmentNumber = i,
                    Amount = installmentAmount,
                    DueDate = dueDate,
                    Status = dueDate < DateTime.UtcNow ? InstallmentStatus.Overdue : InstallmentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                installments.Add(installment);
            }

            return installments;
        }

        public async Task<MigrationStatistics> GetMigrationStatisticsAsync()
        {
            var stats = new MigrationStatistics();

            try
            {
                // Count total legacy contracts in TBS
                stats.TotalLegacyContracts = await _tbsContext.MntMaintenanceContracts
                    .AsNoTracking()
                    .CountAsync();

                // Count migrated contracts
                stats.MigratedContracts = await _context.Contracts
                    .Where(c => c.LegacyContractId != null)
                    .CountAsync();

                stats.PendingContracts = stats.TotalLegacyContracts - stats.MigratedContracts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration statistics");
            }

            return stats;
        }
    }
}

