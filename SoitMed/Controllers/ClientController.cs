using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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

        public ClientController(
            IClientService clientService, 
            IValidationService validationService,
            ILogger<ClientController> logger, 
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _clientService = clientService;
            _validationService = validationService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchClients(
            [FromQuery] string? searchTerm = null,
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
                    Page = page,
                    PageSize = pageSize
                };

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
        /// Get complete client profile with history (NEW - matches plan)
        /// </summary>
        [HttpGet("{id}/profile")]
        [Authorize(Roles = "Salesman,SalesManager,SuperAdmin")]
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
    }
}
