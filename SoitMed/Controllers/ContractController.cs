using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models.Contract;
using SoitMed.Models.Enums;
using SoitMed.Repositories;
using System.Text.Json;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Legal")]
    public class ContractController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ContractController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ContractController(IUnitOfWork unitOfWork, ILogger<ContractController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Get contracts with pagination, search, and filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetContracts([FromQuery] ContractFilterDTO filter)
        {
            try
            {
                // Handle ContractType conversion from query string (if sent as int)
                if (!filter.ContractType.HasValue)
                {
                    var contractTypeParam = Request.Query["contractType"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(contractTypeParam) && int.TryParse(contractTypeParam, out int contractTypeInt))
                    {
                        if (Enum.IsDefined(typeof(ContractTypeFilter), contractTypeInt))
                        {
                            filter.ContractType = (ContractTypeFilter)contractTypeInt;
                            _logger.LogInformation("ContractType filter applied: {ContractType}", filter.ContractType.Value);
                        }
                    }
                }
                
                _logger.LogInformation("Contract filter - ContractType: {ContractType}, Status: {Status}, IsLegacy: {IsLegacy}", 
                    filter.ContractType?.ToString() ?? "null", 
                    filter.Status?.ToString() ?? "null", 
                    filter.IsLegacy?.ToString() ?? "null");
                
                // Use JOIN approach similar to soitmed_data_backend for correct client mapping
                var context = _unitOfWork.GetContext();
                
                // Base query with JOIN to Clients (similar to soitmed_data_backend pattern)
                var query = from contract in context.Contracts.AsNoTracking()
                           join client in context.Clients.AsNoTracking() on contract.ClientId equals client.Id
                           select new
                           {
                               contract.Id,
                               contract.ContractNumber,
                               contract.Title,
                               contract.ContractContent,
                               contract.DocumentUrl,
                               contract.Status,
                               contract.DraftedAt,
                               contract.SentToCustomerAt,
                               contract.SignedAt,
                               contract.CancelledAt,
                               contract.CancellationReason,
                               contract.ClientId,
                               ClientName = client.Name,
                               ClientPhone = client.Phone,
                               ClientEmail = client.Email,
                               contract.DealId,
                               contract.CashAmount,
                               contract.InstallmentAmount,
                               contract.InterestRate,
                               contract.LatePenaltyRate,
                               contract.InstallmentDurationMonths,
                               contract.DraftedBy,
                               contract.CustomerSignedBy,
                               contract.LegacyContractId,
                               contract.CreatedAt,
                               contract.UpdatedAt
                           };

                // Search filter
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(c =>
                        c.ContractNumber.ToLower().Contains(searchTerm) ||
                        c.Title.ToLower().Contains(searchTerm) ||
                        c.ClientName.ToLower().Contains(searchTerm) ||
                        (c.LegacyContractId.HasValue && c.LegacyContractId.Value.ToString().Contains(searchTerm)));
                }

                // Status filter
                if (filter.Status.HasValue)
                {
                    query = query.Where(c => c.Status == filter.Status.Value);
                }

                // Client filter
                if (filter.ClientId.HasValue)
                {
                    query = query.Where(c => c.ClientId == filter.ClientId.Value);
                }

                // Deal filter
                if (filter.DealId.HasValue)
                {
                    query = query.Where(c => c.DealId == filter.DealId.Value);
                }

                // Start date range filter
                if (filter.StartDateFrom.HasValue)
                {
                    query = query.Where(c => c.DraftedAt >= filter.StartDateFrom.Value);
                }
                if (filter.StartDateTo.HasValue)
                {
                    query = query.Where(c => c.DraftedAt <= filter.StartDateTo.Value);
                }

                // End date range filter (using SignedAt or CancelledAt)
                if (filter.EndDateFrom.HasValue)
                {
                    query = query.Where(c =>
                        (c.SignedAt.HasValue && c.SignedAt >= filter.EndDateFrom.Value) ||
                        (c.CancelledAt.HasValue && c.CancelledAt >= filter.EndDateFrom.Value));
                }
                if (filter.EndDateTo.HasValue)
                {
                    query = query.Where(c =>
                        (c.SignedAt.HasValue && c.SignedAt <= filter.EndDateTo.Value) ||
                        (c.CancelledAt.HasValue && c.CancelledAt <= filter.EndDateTo.Value));
                }

                // Has installments filter - need to check InstallmentSchedules separately
                if (filter.HasInstallments.HasValue)
                {
                    var contractIdsWithInstallments = await context.InstallmentSchedules
                        .AsNoTracking()
                        .Select(s => s.ContractId)
                        .Distinct()
                        .ToListAsync();
                    
                    if (filter.HasInstallments.Value)
                    {
                        query = query.Where(c => contractIdsWithInstallments.Contains(c.Id));
                    }
                    else
                    {
                        query = query.Where(c => !contractIdsWithInstallments.Contains(c.Id));
                    }
                }

                // Legacy filter
                if (filter.IsLegacy.HasValue)
                {
                    if (filter.IsLegacy.Value)
                    {
                        query = query.Where(c => c.LegacyContractId != null);
                    }
                    else
                    {
                        query = query.Where(c => c.LegacyContractId == null);
                    }
                }

                // Contract Type filter (Main, Active, Expired, Cancelled)
                var now = DateTime.UtcNow; // Define once for use in filters and later in mapping
                if (filter.ContractType.HasValue && filter.ContractType.Value != ContractTypeFilter.All)
                {
                    switch (filter.ContractType.Value)
                    {
                        case ContractTypeFilter.MainContracts:
                            // For legacy contracts, we need to check MainContract = null in TBS
                            // We'll filter this after loading MainContract data from TBS
                            // For now, filter legacy contracts only (will be refined below)
                            var legacyContractIdsForMain = await query
                                .Where(c => c.LegacyContractId != null)
                                .Select(c => c.LegacyContractId!.Value)
                                .ToListAsync();
                            
                            if (legacyContractIdsForMain.Any())
                            {
                                try
                                {
                                    var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                                    if (!string.IsNullOrEmpty(tbsConnectionString))
                                    {
                                        var optionsBuilder = new DbContextOptionsBuilder<Models.Legacy.TbsDbContext>();
                                        optionsBuilder.UseSqlServer(tbsConnectionString);
                                        
                                        using var tbsContext = new Models.Legacy.TbsDbContext(optionsBuilder.Options);
                                        
                                        var mainContractIds = await tbsContext.MntMaintenanceContracts
                                            .AsNoTracking()
                                            .Where(tc => legacyContractIdsForMain.Contains(tc.ContractId) && tc.MainContract == null)
                                            .Select(tc => tc.ContractId)
                                            .ToListAsync();
                                        
                                        query = query.Where(c => c.LegacyContractId.HasValue && mainContractIds.Contains(c.LegacyContractId.Value));
                                    }
                                    else
                                    {
                                        // If TBS connection not available, return empty
                                        query = query.Where(c => false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to filter main contracts from TBS");
                                    query = query.Where(c => false);
                                }
                            }
                            else
                            {
                                query = query.Where(c => false);
                            }
                            break;
                            
                        case ContractTypeFilter.ActiveContracts:
                            // Signed contracts that are active (signed and not expired)
                            // For legacy contracts, we need to check EndDate from TBS
                            // For new contracts, check SignedAt and status
                            query = query.Where(c => 
                                c.Status == Models.Enums.ContractStatus.Signed &&
                                c.SignedAt.HasValue &&
                                c.SignedAt.Value <= now &&
                                c.Status != Models.Enums.ContractStatus.Expired);
                            break;
                            
                        case ContractTypeFilter.ExpiredContracts:
                            // Expired status or contracts that have passed their end date
                            query = query.Where(c => 
                                c.Status == Models.Enums.ContractStatus.Expired ||
                                (c.Status == Models.Enums.ContractStatus.Signed && 
                                 c.SignedAt.HasValue && 
                                 c.SignedAt.Value < now.AddMonths(-12))); // Contracts older than 12 months are likely expired
                            break;
                            
                        case ContractTypeFilter.CancelledContracts:
                            // Cancelled contracts
                            query = query.Where(c => c.Status == Models.Enums.ContractStatus.Cancelled);
                            break;
                    }
                }

                // Log query count before sorting
                var queryCountBeforeSort = await query.CountAsync();
                _logger.LogInformation("Query count before sorting: {Count}", queryCountBeforeSort);
                
                // Apply sorting
                var sortedQuery = filter.SortBy?.ToLower() switch
                {
                    "contractnumber" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(c => c.ContractNumber)
                        : query.OrderByDescending(c => c.ContractNumber),
                    "title" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(c => c.Title)
                        : query.OrderByDescending(c => c.Title),
                    "status" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(c => c.Status)
                        : query.OrderByDescending(c => c.Status),
                    "signedat" => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(c => c.SignedAt)
                        : query.OrderByDescending(c => c.SignedAt),
                    "createdat" or _ => filter.SortOrder?.ToLower() == "asc"
                        ? query.OrderBy(c => c.CreatedAt)
                        : query.OrderByDescending(c => c.CreatedAt)
                };

                // Get total count before pagination
                var totalCount = await sortedQuery.CountAsync();
                _logger.LogInformation("Total contracts after filtering: {Count}", totalCount);

                // Apply pagination and get contracts with client data from JOIN
                var contractsData = await sortedQuery
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Get contract IDs to load related data (InstallmentSchedules, Deal, etc.)
                var contractIds = contractsData.Select(c => c.Id).ToList();
                
                // Load InstallmentSchedules for installment counts
                var installmentSchedules = await context.InstallmentSchedules
                    .AsNoTracking()
                    .Where(s => contractIds.Contains(s.ContractId))
                    .ToListAsync();
                
                var installmentCounts = installmentSchedules
                    .GroupBy(s => s.ContractId)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Total = g.Count(),
                        Paid = g.Count(i => i.Status == Models.Enums.InstallmentStatus.Paid),
                        Overdue = g.Count(i => i.Status == Models.Enums.InstallmentStatus.Overdue)
                    });

                // Load Deal information if needed
                var dealIds = contractsData.Where(c => c.DealId.HasValue).Select(c => c.DealId!.Value).Distinct().ToList();
                var deals = dealIds.Any() 
                    ? await context.SalesDeals
                        .AsNoTracking()
                        .Where(d => dealIds.Contains(d.Id))
                        .ToDictionaryAsync(d => d.Id, d => d)
                    : new Dictionary<long, Models.SalesDeal>();

                // Load MainContract information from TBS for legacy contracts
                var legacyContractIds = contractsData
                    .Where(c => c.LegacyContractId.HasValue)
                    .Select(c => c.LegacyContractId!.Value)
                    .ToList();
                
                Dictionary<int, int?> mainContractMap = new();
                if (legacyContractIds.Any())
                {
                    try
                    {
                        var tbsConnectionString = _configuration.GetConnectionString("TbsConnection");
                        if (!string.IsNullOrEmpty(tbsConnectionString))
                        {
                            var optionsBuilder = new DbContextOptionsBuilder<Models.Legacy.TbsDbContext>();
                            optionsBuilder.UseSqlServer(tbsConnectionString);
                            
                            using var tbsContext = new Models.Legacy.TbsDbContext(optionsBuilder.Options);
                            
                            var tbsContracts = await tbsContext.MntMaintenanceContracts
                                .AsNoTracking()
                                .Where(tc => legacyContractIds.Contains(tc.ContractId))
                                .Select(tc => new { tc.ContractId, tc.MainContract })
                                .ToListAsync();
                            
                            mainContractMap = tbsContracts.ToDictionary(tc => tc.ContractId, tc => tc.MainContract);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load MainContract information from TBS");
                    }
                }

                // Map to DTOs using data from JOIN (ensures correct client mapping)
                // 'now' is already defined above for filter logic
                var contractDTOs = contractsData.Select(c =>
                {
                    var installments = installmentCounts.GetValueOrDefault(c.Id);
                    var deal = c.DealId.HasValue && deals.ContainsKey(c.DealId.Value) ? deals[c.DealId.Value] : null;
                    
                    // Get MainContract info for legacy contracts
                    int? mainContractId = null;
                    bool isMainContract = false;
                    if (c.LegacyContractId.HasValue && mainContractMap.ContainsKey(c.LegacyContractId.Value))
                    {
                        mainContractId = mainContractMap[c.LegacyContractId.Value];
                        isMainContract = !mainContractId.HasValue; // Main contract if MainContract is null
                    }
                    
                    // Calculate contract type display
                    string contractTypeDisplay = "Unknown";
                    if (c.Status == Models.Enums.ContractStatus.Cancelled)
                    {
                        contractTypeDisplay = "Cancelled"; // عقود الصيانة الملغاة
                    }
                    else if (c.Status == Models.Enums.ContractStatus.Expired || 
                             (c.Status == Models.Enums.ContractStatus.Signed && 
                              c.SignedAt.HasValue && 
                              c.SignedAt.Value < now.AddDays(-365)))
                    {
                        contractTypeDisplay = "Expired"; // عقود الصيانة المنتهية
                    }
                    else if (c.Status == Models.Enums.ContractStatus.Signed && 
                             c.SignedAt.HasValue && 
                             c.SignedAt.Value <= now)
                    {
                        contractTypeDisplay = "Active"; // عقود الصيانة السارية
                    }
                    else if (isMainContract)
                    {
                        contractTypeDisplay = "Main"; // عقود الصيانة الرئيسية
                    }
                    
                    // Calculate expiry info
                    var endDate = c.SignedAt ?? c.CancelledAt;
                    var isExpired = endDate.HasValue && endDate.Value < now;
                    var daysUntilExpiry = endDate.HasValue ? (int?)(endDate.Value - now).TotalDays : null;
                    
                    return new ContractResponseDTO
                    {
                        Id = c.Id,
                        ContractNumber = c.ContractNumber,
                        Title = c.Title,
                        ContractContent = c.ContractContent,
                        DocumentUrl = c.DocumentUrl,
                        Status = c.Status,
                        StatusDisplay = c.Status.ToString(),
                        DraftedAt = c.DraftedAt,
                        SentToCustomerAt = c.SentToCustomerAt,
                        SignedAt = c.SignedAt,
                        CancelledAt = c.CancelledAt,
                        CancellationReason = c.CancellationReason,
                        ClientId = c.ClientId,
                        ClientName = c.ClientName, // From JOIN - guaranteed correct
                        ClientPhone = c.ClientPhone,
                        ClientEmail = c.ClientEmail,
                        DealId = c.DealId,
                        DealTitle = deal != null ? $"Deal #{deal.Id} - {deal.DealValue:C}" : null,
                        CashAmount = c.CashAmount,
                        InstallmentAmount = c.InstallmentAmount,
                        InstallmentDurationMonths = c.InstallmentDurationMonths,
                        HasInstallments = installments != null && installments.Total > 0,
                        InstallmentCount = installments?.Total ?? 0,
                        PaidInstallmentCount = installments?.Paid ?? 0,
                        OverdueInstallmentCount = installments?.Overdue ?? 0,
                        DraftedBy = c.DraftedBy,
                        DrafterName = null, // Can be loaded separately if needed
                        CustomerSignedBy = c.CustomerSignedBy,
                        CustomerSignerName = null, // Can be loaded separately if needed
                        LegacyContractId = c.LegacyContractId,
                        IsLegacy = c.LegacyContractId.HasValue,
                        CreatedAt = c.CreatedAt,
                        IsMainContract = isMainContract,
                        MainContractId = mainContractId,
                        ContractTypeDisplay = contractTypeDisplay,
                        UpdatedAt = c.UpdatedAt,
                        IsExpired = c.Status == Models.Enums.ContractStatus.Expired,
                        DaysUntilExpiry = c.SignedAt.HasValue ? (int?)(c.SignedAt.Value - DateTime.UtcNow).TotalDays : null,
                        TotalAmount = c.CashAmount ?? c.InstallmentAmount
                    };
                }).ToList();

                var response = new PaginatedContractResponseDTO
                {
                    Contracts = contractDTOs,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };

                return Ok(new
                {
                    success = true,
                    message = "Contracts retrieved successfully",
                    data = new
                    {
                        contracts = response.Contracts,
                        totalCount = response.TotalCount,
                        page = response.Page,
                        pageSize = response.PageSize,
                        totalPages = response.TotalPages,
                        hasPreviousPage = response.HasPreviousPage,
                        hasNextPage = response.HasNextPage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contracts");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving contracts",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search for a contract by LegacyContractId
        /// </summary>
        [HttpGet("search/legacy/{legacyContractId}")]
        public async Task<IActionResult> GetContractByLegacyId(int legacyContractId)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetQueryable()
                    .FirstOrDefaultAsync(c => c.LegacyContractId == legacyContractId);

                if (contract == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Contract with LegacyContractId {legacyContractId} not found",
                        exists = false
                    });
                }

                var dto = MapToDTO(contract);
                return Ok(new
                {
                    success = true,
                    message = "Contract found",
                    exists = true,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for contract with LegacyContractId {LegacyContractId}", legacyContractId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching for the contract",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Search for a contract by ContractNumber
        /// </summary>
        [HttpGet("search/number/{contractNumber}")]
        public async Task<IActionResult> GetContractByNumber(string contractNumber)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetQueryable()
                    .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);

                if (contract == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"Contract with ContractNumber '{contractNumber}' not found",
                        exists = false
                    });
                }

                var dto = MapToDTO(contract);
                return Ok(new
                {
                    success = true,
                    message = "Contract found",
                    exists = true,
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for contract with ContractNumber {ContractNumber}", contractNumber);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while searching for the contract",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get contract by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContract(long id)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
                if (contract == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Contract with ID {id} not found"
                    });
                }

                var dto = MapToDTO(contract);
                return Ok(new
                {
                    success = true,
                    message = "Contract retrieved successfully",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract {ContractId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving the contract",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get contract with equipment and media files from legacy system
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetContractWithDetails(long id)
        {
            try
            {
                var contract = await _unitOfWork.Contracts.GetByIdAsync(id);
                if (contract == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Contract with ID {id} not found"
                    });
                }

                // Get basic contract info
                var dto = MapToDTO(contract);

                // Create detailed DTO
                var detailedDto = new DetailedContractResponseDTO
                {
                    Id = dto.Id,
                    ContractNumber = dto.ContractNumber,
                    Title = dto.Title,
                    ContractContent = dto.ContractContent,
                    DocumentUrl = dto.DocumentUrl,
                    Status = dto.Status,
                    StatusDisplay = dto.StatusDisplay,
                    DraftedAt = dto.DraftedAt,
                    SentToCustomerAt = dto.SentToCustomerAt,
                    SignedAt = dto.SignedAt,
                    CancelledAt = dto.CancelledAt,
                    CancellationReason = dto.CancellationReason,
                    ClientId = dto.ClientId,
                    ClientName = dto.ClientName,
                    ClientPhone = dto.ClientPhone,
                    ClientEmail = dto.ClientEmail,
                    DealId = dto.DealId,
                    DealTitle = dto.DealTitle,
                    CashAmount = dto.CashAmount,
                    InstallmentAmount = dto.InstallmentAmount,
                    InterestRate = dto.InterestRate,
                    LatePenaltyRate = dto.LatePenaltyRate,
                    InstallmentDurationMonths = dto.InstallmentDurationMonths,
                    HasInstallments = dto.HasInstallments,
                    InstallmentCount = dto.InstallmentCount,
                    PaidInstallmentCount = dto.PaidInstallmentCount,
                    OverdueInstallmentCount = dto.OverdueInstallmentCount,
                    DraftedBy = dto.DraftedBy,
                    DrafterName = dto.DrafterName,
                    CustomerSignedBy = dto.CustomerSignedBy,
                    CustomerSignerName = dto.CustomerSignerName,
                    LegacyContractId = dto.LegacyContractId,
                    IsLegacy = dto.IsLegacy,
                    CreatedAt = dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt,
                    IsExpired = dto.IsExpired,
                    DaysUntilExpiry = dto.DaysUntilExpiry,
                    TotalAmount = dto.TotalAmount
                };

                // If this is a legacy contract, fetch equipment and media from soitmed_data_backend API
                if (contract.LegacyContractId.HasValue)
                {
                    try
                    {
                        _logger.LogInformation("Loading equipment and media for legacy contract {ContractId} (LegacyId: {LegacyId})", 
                            id, contract.LegacyContractId.Value);

                        var (equipment, mediaFiles) = await GetLegacyContractDataAsync(contract.LegacyContractId.Value);
                        detailedDto.Equipment = equipment;
                        detailedDto.MediaFiles = mediaFiles;

                        _logger.LogInformation("Loaded {EquipmentCount} equipment items and {MediaCount} media files for contract {ContractId}", 
                            equipment.Count, mediaFiles.Count, contract.LegacyContractId.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load equipment/media for legacy contract {ContractId} (LegacyId: {LegacyId})", 
                            id, contract.LegacyContractId.Value);
                        // Continue without equipment/media if there's an error
                    }
                }
                else
                {
                    _logger.LogInformation("Contract {ContractId} is not a legacy contract, skipping equipment/media load", id);
                }

                return Ok(new
                {
                    success = true,
                    message = "Contract details retrieved successfully",
                    data = detailedDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contract details {ContractId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving contract details",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get equipment and media files from soitmed_data_backend API
        /// </summary>
        private async Task<(List<ContractEquipmentDTO> Equipment, List<ContractMediaFileDTO> MediaFiles)> GetLegacyContractDataAsync(int legacyContractId)
        {
            var legacyApiBaseUrl = _configuration["ConnectionSettings:LegacyMediaApiBaseUrl"];
            if (string.IsNullOrEmpty(legacyApiBaseUrl))
            {
                _logger.LogWarning("Legacy media API base URL not configured");
                return (new List<ContractEquipmentDTO>(), new List<ContractMediaFileDTO>());
            }

            try
            {
                // Use named client if available, otherwise create a new one
                var httpClient = _httpClientFactory.CreateClient("LegacyMediaApi");
                if (httpClient.BaseAddress == null)
                {
                    httpClient.BaseAddress = new Uri(legacyApiBaseUrl);
                }
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var response = await httpClient.GetAsync($"/api/Contracts/maintenance/{legacyContractId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch contract data from legacy API for contract {ContractId}: {StatusCode}", 
                        legacyContractId, response.StatusCode);
                    return (new List<ContractEquipmentDTO>(), new List<ContractMediaFileDTO>());
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Legacy API response for contract {ContractId}: {Response}", legacyContractId, content.Substring(0, Math.Min(500, content.Length)));
                
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);

                var equipment = new List<ContractEquipmentDTO>();
                var mediaFiles = new List<ContractMediaFileDTO>();

                // Extract machines/equipment - try both "machines" (camelCase) and "Machines" (PascalCase)
                JsonElement machinesElement;
                bool hasMachines = jsonResponse.TryGetProperty("machines", out machinesElement) || 
                                   jsonResponse.TryGetProperty("Machines", out machinesElement);
                
                if (hasMachines && machinesElement.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("Found {Count} machines in response", machinesElement.GetArrayLength());
                    
                    foreach (var machineElement in machinesElement.EnumerateArray())
                    {
                        // Try both camelCase and PascalCase property names
                        var machineId = GetInt32Property(machineElement, "machineId", "MachineId");
                        var itemId = GetInt32Property(machineElement, "itemId", "ItemId");
                        var serialNumber = GetStringProperty(machineElement, "serialNumber", "SerialNumber") ?? string.Empty;
                        var modelName = GetStringProperty(machineElement, "modelName", "ModelName");
                        var modelNameEn = GetStringProperty(machineElement, "modelNameEn", "ModelNameEn");
                        var devicePlace = GetStringProperty(machineElement, "devicePlace", "DevicePlace");
                        var itemCode = GetStringProperty(machineElement, "itemCode", "ItemCode");
                        var visitCount = GetInt32Property(machineElement, "visitCount", "VisitCount");
                        var expirationDate = GetDateTimeProperty(machineElement, "expirationDate", "ExpirationDate");
                        
                        var equipmentDto = new ContractEquipmentDTO
                        {
                            MachineId = machineId,
                            ItemId = itemId,
                            SerialNumber = serialNumber,
                            ModelName = modelName,
                            ModelNameEn = modelNameEn,
                            DevicePlace = devicePlace,
                            ItemCode = itemCode,
                            VisitCount = visitCount,
                            ExpirationDate = expirationDate
                        };
                        equipment.Add(equipmentDto);
                        
                        _logger.LogInformation("Mapped machine: MachineId={MachineId}, SerialNumber={SerialNumber}, ModelName={ModelName}", 
                            machineId, serialNumber, modelName);
                    }
                }
                else
                {
                    _logger.LogWarning("No 'machines' or 'Machines' property found in response for contract {ContractId}", legacyContractId);
                }

                // Extract media files ONLY from machines that belong to this contract
                // We don't include contract-level mediaFiles because they are customer-level files (unrelated)
                JsonElement machinesForMediaElement;
                bool hasMachinesForMedia = jsonResponse.TryGetProperty("machines", out machinesForMediaElement) || 
                                          jsonResponse.TryGetProperty("Machines", out machinesForMediaElement);
                
                if (hasMachinesForMedia && machinesForMediaElement.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("Extracting media files from {Count} machines for contract {ContractId}", 
                        machinesForMediaElement.GetArrayLength(), legacyContractId);
                    
                    foreach (var machineElement in machinesForMediaElement.EnumerateArray())
                    {
                        // Only get media files from machines that are actually linked to this contract
                        if (machineElement.TryGetProperty("mediaFiles", out var machineMediaElement) && machineMediaElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var fileElement in machineMediaElement.EnumerateArray())
                            {
                                var mediaFile = MapMediaFileFromJson(fileElement);
                                // Avoid duplicates by filename
                                if (!mediaFiles.Any(m => m.FileName == mediaFile.FileName))
                                {
                                    mediaFiles.Add(mediaFile);
                                    _logger.LogDebug("Added media file {FileName} from machine for contract {ContractId}", 
                                        mediaFile.FileName, legacyContractId);
                                }
                            }
                        }
                    }
                }
                
                // Filter out contract-level mediaFiles as they are customer-level (unrelated to specific contract)
                // Only include files that come from machines/visits related to this contract
                _logger.LogInformation("Filtered media files for contract {ContractId}: {Count} files from machines/visits only", 
                    legacyContractId, mediaFiles.Count);

                return (equipment, mediaFiles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contract data from legacy API for contract {ContractId}", legacyContractId);
                return (new List<ContractEquipmentDTO>(), new List<ContractMediaFileDTO>());
            }
        }

        /// <summary>
        /// Helper method to get string property from JSON (tries both camelCase and PascalCase)
        /// </summary>
        private string? GetStringProperty(JsonElement element, string camelCaseName, string pascalCaseName)
        {
            if (element.TryGetProperty(camelCaseName, out var camelElement))
                return camelElement.GetString();
            if (element.TryGetProperty(pascalCaseName, out var pascalElement))
                return pascalElement.GetString();
            return null;
        }

        /// <summary>
        /// Helper method to get int32 property from JSON (tries both camelCase and PascalCase)
        /// </summary>
        private int GetInt32Property(JsonElement element, string camelCaseName, string pascalCaseName)
        {
            if (element.TryGetProperty(camelCaseName, out var camelElement) && camelElement.ValueKind == JsonValueKind.Number)
                return camelElement.GetInt32();
            if (element.TryGetProperty(pascalCaseName, out var pascalElement) && pascalElement.ValueKind == JsonValueKind.Number)
                return pascalElement.GetInt32();
            return 0;
        }

        /// <summary>
        /// Helper method to get DateTime property from JSON (tries both camelCase and PascalCase)
        /// </summary>
        private DateTime? GetDateTimeProperty(JsonElement element, string camelCaseName, string pascalCaseName)
        {
            JsonElement dateElement;
            if (element.TryGetProperty(camelCaseName, out dateElement) || element.TryGetProperty(pascalCaseName, out dateElement))
            {
                if (dateElement.ValueKind == JsonValueKind.String && DateTime.TryParse(dateElement.GetString(), out var dateTime))
                    return dateTime;
                if (dateElement.ValueKind == JsonValueKind.Null)
                    return null;
            }
            return null;
        }

        /// <summary>
        /// Map media file from JSON element
        /// </summary>
        /// <summary>
        /// Helper method to map media file from JSON element
        /// Transforms file paths to API URLs for serving files from remote server (IP 104)
        /// </summary>
        private ContractMediaFileDTO MapMediaFileFromJson(JsonElement fileElement)
        {
            var fileName = fileElement.TryGetProperty("fileName", out var fileNameElement) ? fileNameElement.GetString() ?? string.Empty : string.Empty;
            var originalFilePath = fileElement.TryGetProperty("filePath", out var filePathElement) ? filePathElement.GetString() ?? string.Empty : string.Empty;
            
            // Transform file path to API URL
            // Files are stored on remote server (IP 104) in:
            // - D:\Soit-Med\legacy\SOIT\UploadFiles\Files
            // - D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports
            string apiFilePath = originalFilePath;
            
            // Extract fileName from filePath if not provided separately
            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(originalFilePath))
            {
                // Try to extract from API path like /api/Media/files/filename.ext
                if (originalFilePath.Contains("/api/Media/files/"))
                {
                    fileName = originalFilePath.Replace("/api/Media/files/", "").Split('?')[0]; // Remove query string if any
                }
                else if (originalFilePath.Contains("/api/LegacyMedia/files/"))
                {
                    fileName = originalFilePath.Replace("/api/LegacyMedia/files/", "").Split('?')[0];
                }
                else if (Path.IsPathRooted(originalFilePath))
                {
                    fileName = Path.GetFileName(originalFilePath);
                }
                else
                {
                    fileName = Path.GetFileName(originalFilePath);
                }
            }
            
            if (!string.IsNullOrEmpty(originalFilePath))
            {
                // If it's a soitmed_data_backend API path (/api/Media/files/...), convert to our API path
                if (originalFilePath.StartsWith("/api/Media/files/"))
                {
                    // Extract filename from the path
                    var extractedFileName = originalFilePath.Replace("/api/Media/files/", "").Split('?')[0];
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = extractedFileName;
                    }
                    // Convert to our LegacyMedia API endpoint
                    apiFilePath = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
                }
                // Check if it's a legacy Windows path
                else if (originalFilePath.Contains(@"D:\Soit-Med\legacy\SOIT") || 
                    originalFilePath.Contains(@"C:\Soit-Med\legacy\SOIT") ||
                    originalFilePath.Contains(@"D:\Soit-Med\legacy\SOIT\UploadFiles\Files") ||
                    originalFilePath.Contains(@"D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports"))
                {
                    // Extract just the filename if full path is provided
                    if (string.IsNullOrEmpty(fileName) && Path.IsPathRooted(originalFilePath))
                    {
                        fileName = Path.GetFileName(originalFilePath);
                    }
                    // Use API endpoint for serving files from remote server
                    apiFilePath = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
                }
                // If it's already our API path, keep it
                else if (originalFilePath.StartsWith("/api/LegacyMedia/files/"))
                {
                    apiFilePath = originalFilePath; // Already correct
                }
                // If it's just a filename or relative path, use API endpoint
                else if (!originalFilePath.StartsWith("/api/") && !originalFilePath.StartsWith("http"))
                {
                    // If it's just a filename, use API endpoint
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.GetFileName(originalFilePath);
                    }
                    apiFilePath = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
                }
            }
            else if (!string.IsNullOrEmpty(fileName))
            {
                // If only filename is provided, use API endpoint
                apiFilePath = $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
            }

            return new ContractMediaFileDTO
            {
                FileName = fileName,
                FilePath = apiFilePath, // Use API endpoint instead of direct file path
                FileType = fileElement.TryGetProperty("fileType", out var fileTypeElement) ? fileTypeElement.GetString() : null,
                MimeType = fileElement.TryGetProperty("mimeType", out var mimeElement) ? mimeElement.GetString() : null,
                FileSize = fileElement.TryGetProperty("fileSize", out var sizeElement) && sizeElement.ValueKind == JsonValueKind.Number ? sizeElement.GetInt64() : null,
                FileSizeFormatted = fileElement.TryGetProperty("fileSizeFormatted", out var sizeFormattedElement) ? sizeFormattedElement.GetString() : null,
                IsImage = fileElement.TryGetProperty("isImage", out var isImageElement) && isImageElement.GetBoolean(),
                IsPdf = fileElement.TryGetProperty("isPdf", out var isPdfElement) && isPdfElement.GetBoolean(),
                CanPreview = fileElement.TryGetProperty("canPreview", out var canPreviewElement) && canPreviewElement.GetBoolean(),
                IsAvailable = !fileElement.TryGetProperty("isAvailable", out var isAvailableElement) || isAvailableElement.GetBoolean(),
                AvailabilityMessage = fileElement.TryGetProperty("availabilityMessage", out var msgElement) ? msgElement.GetString() : null
            };
        }

        /// <summary>
        /// Map Contract entity to ContractResponseDTO
        /// </summary>
        private ContractResponseDTO MapToDTO(Contract contract)
        {
            var now = DateTime.UtcNow;
            var installmentSchedules = contract.InstallmentSchedules?.ToList() ?? new List<InstallmentSchedule>();
            
            // Calculate expiry info
            var endDate = contract.SignedAt ?? contract.CancelledAt;
            var isExpired = endDate.HasValue && endDate.Value < now;
            var daysUntilExpiry = endDate.HasValue ? (int?)(endDate.Value - now).TotalDays : null;

            return new ContractResponseDTO
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                Title = contract.Title,
                ContractContent = contract.ContractContent,
                DocumentUrl = contract.DocumentUrl,
                Status = contract.Status,
                StatusDisplay = contract.Status.ToString(),
                DraftedAt = contract.DraftedAt,
                SentToCustomerAt = contract.SentToCustomerAt,
                SignedAt = contract.SignedAt,
                CancelledAt = contract.CancelledAt,
                CancellationReason = contract.CancellationReason,
                ClientId = contract.ClientId,
                ClientName = contract.Client?.Name ?? "Unknown",
                ClientPhone = contract.Client?.Phone,
                ClientEmail = contract.Client?.Email,
                DealId = contract.DealId,
                DealTitle = contract.Deal != null ? $"Deal #{contract.Deal.Id} - {contract.Deal.DealValue:C}" : null,
                CashAmount = contract.CashAmount,
                InstallmentAmount = contract.InstallmentAmount,
                InterestRate = contract.InterestRate,
                LatePenaltyRate = contract.LatePenaltyRate,
                InstallmentDurationMonths = contract.InstallmentDurationMonths,
                HasInstallments = installmentSchedules.Any(),
                InstallmentCount = installmentSchedules.Count,
                PaidInstallmentCount = installmentSchedules.Count(i => i.Status == InstallmentStatus.Paid),
                OverdueInstallmentCount = installmentSchedules.Count(i => i.Status == InstallmentStatus.Overdue),
                DraftedBy = contract.DraftedBy,
                DrafterName = contract.Drafter != null ? $"{contract.Drafter.FirstName} {contract.Drafter.LastName}" : null,
                CustomerSignedBy = contract.CustomerSignedBy,
                CustomerSignerName = contract.CustomerSigner != null ? $"{contract.CustomerSigner.FirstName} {contract.CustomerSigner.LastName}" : null,
                LegacyContractId = contract.LegacyContractId,
                IsLegacy = contract.LegacyContractId != null,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                IsExpired = isExpired,
                DaysUntilExpiry = daysUntilExpiry,
                TotalAmount = contract.CashAmount ?? contract.InstallmentAmount
            };
        }
    }
}

