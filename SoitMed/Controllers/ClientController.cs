using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController : BaseController
    {
        private readonly IClientService _clientService;
        private readonly IValidationService _validationService;
        private readonly ILogger<ClientController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ClientController(
            IClientService clientService, 
            IValidationService validationService,
            ILogger<ClientController> logger, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment) 
            : base(userManager)
        {
            _clientService = clientService;
            _validationService = validationService;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchClients(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? classification = null,
            [FromQuery] string? assignedSalesManId = null,
            [FromQuery] string? city = null,
            [FromQuery] int? governorateId = null,
            [FromQuery] List<string>? equipmentCategories = null,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Map query parameters to SearchClientDTO
                var searchDto = new SearchClientDTO
                {
                    Query = searchTerm ?? string.Empty,
                    Classification = classification,
                    AssignedSalesManId = assignedSalesManId,
                    City = city,
                    GovernorateId = governorateId,
                    EquipmentCategories = equipmentCategories,
                    Page = page,
                    PageSize = pageSize
                };

                // Debug logging
                _logger.LogInformation("SearchClients called with: Query={Query}, Classification={Classification}, AssignedSalesManId={AssignedSalesManId}, City={City}, GovernorateId={GovernorateId}, EquipmentCategories={EquipmentCategories}, Page={Page}, PageSize={PageSize}",
                    searchDto.Query, searchDto.Classification, searchDto.AssignedSalesManId, searchDto.City, searchDto.GovernorateId, 
                    searchDto.EquipmentCategories != null ? string.Join(",", searchDto.EquipmentCategories) : "null", searchDto.Page, searchDto.PageSize);

                // Validate search parameters
                var validationResult = _validationService.ValidateClientSearch(searchDto);
                if (!validationResult.IsValid)
                    return BadRequest(CreateErrorResponse(string.Join(", ", validationResult.Errors)));

                var result = await _clientService.SearchClientsAsync(searchDto, currentUser.Id);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clients");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في البحث عن العملاء"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDTO createDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Validate client creation data
                var validationResult = await _validationService.ValidateClientCreationAsync(createDto);
                if (!validationResult.IsValid)
                    return BadRequest(CreateErrorResponse(string.Join(", ", validationResult.Errors)));

                var result = await _clientService.CreateClientAsync(createDto, currentUser.Id);
                return CreatedAtAction(nameof(GetClient), new { id = result.Id }, 
                    CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في إنشاء العميل"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(long id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                var result = await _clientService.GetClientAsync(id);
                if (result == null)
                    return NotFound(CreateErrorResponse("العميل غير موجود"));

                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {ClientId}", id);
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب بيانات العميل"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(long id, [FromBody] UpdateClientDTO updateDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Validate client update data
                var validationResult = await _validationService.ValidateClientUpdateAsync(id, updateDto);
                if (!validationResult.IsValid)
                    return BadRequest(CreateErrorResponse(string.Join(", ", validationResult.Errors)));

                // Convert UpdateClientDTO to CreateClientDTO
                var createDto = new CreateClientDTO
                {
                    Name = updateDto.Name,
                    Type = updateDto.Type,
                    Specialization = updateDto.Specialization,
                    Location = updateDto.Location,
                    Phone = updateDto.Phone,
                    Email = updateDto.Email,
                    Website = updateDto.Website,
                    Address = updateDto.Address,
                    City = updateDto.City,
                    Governorate = updateDto.Governorate,
                    PostalCode = updateDto.PostalCode,
                    Notes = updateDto.Notes,
                    Status = updateDto.Status,
                    Priority = updateDto.Priority,
                    PotentialValue = updateDto.PotentialValue,
                    ContactPerson = updateDto.ContactPerson,
                    ContactPersonPhone = updateDto.ContactPersonPhone,
                    ContactPersonEmail = updateDto.ContactPersonEmail,
                    ContactPersonPosition = updateDto.ContactPersonPosition
                };

                var result = await _clientService.UpdateClientAsync(id, createDto, currentUser.Id);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {ClientId}", id);
                return StatusCode(500, CreateErrorResponse("حدث خطأ في تحديث العميل"));
            }
        }

        [HttpPost("find-or-create")]
        public async Task<IActionResult> FindOrCreateClient([FromBody] FindOrCreateClientDTO findDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Validate find or create parameters
                var validationResult = _validationService.ValidateClientFindOrCreate(findDto);
                if (!validationResult.IsValid)
                    return BadRequest(CreateErrorResponse(string.Join(", ", validationResult.Errors)));

                var result = await _clientService.FindOrCreateClientAsync(findDto, currentUser.Id);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding or creating client");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في البحث عن العميل أو إنشاؤه"));
            }
        }

        [HttpGet("my-clients")]
        public async Task<IActionResult> GetMyClients([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Validate pagination parameters
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(CreateErrorResponse("معاملات الصفحة غير صحيحة"));

                var result = await _clientService.GetMyClientsAsync(currentUser.Id, page, pageSize);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my clients");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب عملائي"));
            }
        }

        [HttpGet("follow-up-needed")]
        public async Task<IActionResult> GetClientsNeedingFollowUp()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                var result = await _clientService.GetClientsNeedingFollowUpAsync(currentUser.Id);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients needing follow-up");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب العملاء المحتاجين لمتابعة"));
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetClientStatistics()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                var result = await _clientService.GetClientStatisticsAsync(currentUser.Id);
                return Ok(CreateSuccessResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client statistics");
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب إحصائيات العملاء"));
            }
        }

        /// <summary>
        /// Get all clients with pagination (similar to soitmed_data_backend)
        /// </summary>
        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllClients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                _logger.LogInformation("GetAllClients called with pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);

                // Validate pagination parameters
                if (pageNumber < 1)
                {
                    _logger.LogWarning("Invalid pageNumber: {PageNumber}", pageNumber);
                    return BadRequest(CreateErrorResponse("PageNumber must be greater than 0"));
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    _logger.LogWarning("Invalid pageSize: {PageSize}", pageSize);
                    return BadRequest(CreateErrorResponse("PageSize must be between 1 and 100"));
                }

                var (clients, totalCount, _, _) = await _clientService.GetAllClientsAsync(pageNumber, pageSize);
                
                _logger.LogInformation("GetAllClients returning {Count} clients out of {TotalCount}", clients.Count(), totalCount);
                
                return Ok(CreateSuccessResponse(new
                {
                    Clients = clients,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all clients. PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب العملاء"));
            }
        }

        /// <summary>
        /// Get complete client profile with history (NEW - matches plan)
        /// </summary>
        [HttpGet("{id}/profile")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetClientProfile(long id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                var result = await _clientService.GetClientProfileAsync(id, currentUser.Id);
                if (result == null)
                    return NotFound(CreateErrorResponse("العميل غير موجود"));

                return Ok(CreateSuccessResponse(result, "Client profile retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to client profile");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client profile for client {ClientId}", id);
                return StatusCode(500, CreateErrorResponse("حدث خطأ في جلب ملف العميل"));
            }
        }

        /// <summary>
        /// Submit client legal documents (National ID and Tax Card)
        /// </summary>
        [HttpPost("{id}/submit-legal-documents")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Customer,Doctor,Technician")]
        public async Task<IActionResult> SubmitClientLegalDocuments(long id, [FromForm] SubmitClientLegalDocumentsDTO documentsDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(CreateErrorResponse("غير مصرح لك"));

                // Verify client exists
                var client = await _clientService.GetClientAsync(id);
                if (client == null)
                    return NotFound(CreateErrorResponse("العميل غير موجود"));

                // Validate files
                if (documentsDto.NationalIdFrontImage == null || documentsDto.NationalIdFrontImage.Length == 0)
                    return BadRequest(CreateErrorResponse("صورة الهوية الأمامية مطلوبة"));

                if (documentsDto.NationalIdBackImage == null || documentsDto.NationalIdBackImage.Length == 0)
                    return BadRequest(CreateErrorResponse("صورة الهوية الخلفية مطلوبة"));

                // Validate file types
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var frontExt = Path.GetExtension(documentsDto.NationalIdFrontImage.FileName).ToLowerInvariant();
                var backExt = Path.GetExtension(documentsDto.NationalIdBackImage.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(frontExt) || !allowedExtensions.Contains(backExt))
                    return BadRequest(CreateErrorResponse("نوع الملف غير مدعوم. يرجى استخدام JPG, PNG, أو GIF"));

                // Validate file sizes (max 5MB each)
                if (documentsDto.NationalIdFrontImage.Length > 5 * 1024 * 1024 || 
                    documentsDto.NationalIdBackImage.Length > 5 * 1024 * 1024)
                    return BadRequest(CreateErrorResponse("حجم الملف يتجاوز 5 ميجابايت"));

                // Validate tax card image if provided
                string? taxCardImagePath = null;
                if (documentsDto.TaxCardImage != null && documentsDto.TaxCardImage.Length > 0)
                {
                    var taxExt = Path.GetExtension(documentsDto.TaxCardImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(taxExt))
                        return BadRequest(CreateErrorResponse("نوع ملف البطاقة الضريبية غير مدعوم"));

                    if (documentsDto.TaxCardImage.Length > 5 * 1024 * 1024)
                        return BadRequest(CreateErrorResponse("حجم ملف البطاقة الضريبية يتجاوز 5 ميجابايت"));
                }

                // Create directory for client documents
                var clientDocumentsFolder = Path.Combine(_environment.WebRootPath, "client-documents", id.ToString());
                Directory.CreateDirectory(clientDocumentsFolder);

                // Upload National ID front image
                var frontFileName = $"national-id-front-{Guid.NewGuid()}{frontExt}";
                var frontFilePath = Path.Combine(clientDocumentsFolder, frontFileName);
                var frontRelativePath = Path.Combine("client-documents", id.ToString(), frontFileName).Replace('\\', '/');
                using (var stream = new FileStream(frontFilePath, FileMode.Create))
                {
                    await documentsDto.NationalIdFrontImage.CopyToAsync(stream);
                }

                // Upload National ID back image
                var backFileName = $"national-id-back-{Guid.NewGuid()}{backExt}";
                var backFilePath = Path.Combine(clientDocumentsFolder, backFileName);
                var backRelativePath = Path.Combine("client-documents", id.ToString(), backFileName).Replace('\\', '/');
                using (var stream = new FileStream(backFilePath, FileMode.Create))
                {
                    await documentsDto.NationalIdBackImage.CopyToAsync(stream);
                }

                // Upload Tax Card image if provided
                if (documentsDto.TaxCardImage != null && documentsDto.TaxCardImage.Length > 0)
                {
                    var taxExt = Path.GetExtension(documentsDto.TaxCardImage.FileName).ToLowerInvariant();
                    var taxFileName = $"tax-card-{Guid.NewGuid()}{taxExt}";
                    var taxFilePath = Path.Combine(clientDocumentsFolder, taxFileName);
                    taxCardImagePath = Path.Combine("client-documents", id.ToString(), taxFileName).Replace('\\', '/');
                    using (var stream = new FileStream(taxFilePath, FileMode.Create))
                    {
                        await documentsDto.TaxCardImage.CopyToAsync(stream);
                    }
                }

                // Save to database
                var result = await _clientService.SubmitClientLegalDocumentsAsync(
                    id,
                    documentsDto.NationalId,
                    frontRelativePath,
                    backRelativePath,
                    documentsDto.TaxCardNumber,
                    taxCardImagePath,
                    currentUser.Id
                );

                return Ok(CreateSuccessResponse(result, "تم إرسال المستندات القانونية بنجاح"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for submitting legal documents");
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting legal documents for client {ClientId}", id);
                return StatusCode(500, CreateErrorResponse("حدث خطأ في إرسال المستندات القانونية"));
            }
        }
    }
}
