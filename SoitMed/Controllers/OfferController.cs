using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using SoitMed.Services;
using System.IO;
using System.Security.Claims;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing sales offers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OfferController : BaseController
    {
        private readonly IOfferService _offerService;
        private readonly IOfferEquipmentImageService _equipmentImageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OfferController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IOfferPdfService _pdfService;

        public OfferController(
            IOfferService offerService,
            IOfferEquipmentImageService equipmentImageService,
            IUnitOfWork unitOfWork,
            ILogger<OfferController> logger,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            IOfferPdfService pdfService) 
            : base(userManager)
        {
            _offerService = offerService;
            _equipmentImageService = equipmentImageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Get all salesmen (for SalesSupport to assign offers)
        /// </summary>
        [HttpGet("salesmen")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetAllSalesmen([FromQuery] string? q = null)
        {
            try
            {
                var salesmen = await UserManager.GetUsersInRoleAsync("SalesMan");

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim().ToLower();
                    salesmen = salesmen
                        .Where(u =>
                            (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(term)) ||
                            (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(term)) ||
                            (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(term)))
                        .ToList();
                }

                var salesmenList = salesmen.Select(u => new
                {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber,
                    userName = u.UserName,
                    isActive = u.IsActive
                }).ToList();

                return Ok(ResponseHelper.CreateSuccessResponse(salesmenList));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all salesmen for assignment");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving salesmen"));
            }
        }

        /// <summary>
        /// Get all offers
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetOffers([FromQuery] string? status = null, [FromQuery] string? clientId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                List<OfferResponseDTO> result;
                
                if (!string.IsNullOrEmpty(clientId) && long.TryParse(clientId, out var clientIdLong))
                {
                    result = await _offerService.GetOffersByClientAsync(clientIdLong);
                }
                else if (!string.IsNullOrEmpty(status))
                {
                    result = await _offerService.GetOffersByStatusAsync(status);
                }
                else
                {
                    // Return all offers
                    result = await _offerService.GetOffersByStatusAsync("");
                }

                // Apply date filter if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    result = result.Where(o =>
                    {
                        var offerDate = o.CreatedAt.Date;
                        bool matchesStart = !startDate.HasValue || offerDate >= startDate.Value.Date;
                        bool matchesEnd = !endDate.HasValue || offerDate <= endDate.Value.Date;
                        return matchesStart && matchesEnd;
                    }).ToList();
                }

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offers");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offers"));
            }
        }

        /// <summary>
        /// Get offers created by sales support
        /// </summary>
        [HttpGet("my-offers")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetMyOffers([FromQuery] string? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var result = await _offerService.GetOffersByCreatorAsync(userId);

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    result = result.Where(o => o.Status == status).ToList();
                }

                // Apply date filter if provided
                if (startDate.HasValue || endDate.HasValue)
                {
                    result = result.Where(o =>
                    {
                        var offerDate = o.CreatedAt.Date;
                        bool matchesStart = !startDate.HasValue || offerDate >= startDate.Value.Date;
                        bool matchesEnd = !endDate.HasValue || offerDate <= endDate.Value.Date;
                        return matchesStart && matchesEnd;
                    }).ToList();
                }

                return Ok(ResponseHelper.CreateSuccessResponse(result, "My offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my offers");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offers"));
            }
        }

        /// <summary>
        /// Get offers for the current customer (customer-facing endpoint)
        /// </summary>
        [HttpGet("customer-offers")]
        [Authorize(Roles = "Customer,Doctor,Technician")]
        public async Task<IActionResult> GetCustomerOffers([FromQuery] string? status = null)
        {
            try
            {
                // Get user ID directly from JWT token claim (same as other controllers)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("User ID not found"));
                }

                // Get current user to find their client record
                var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                if (currentUser == null)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("User not found"));
                }

                long clientId = 0;
                Client? client = null;

                // Try to find client by customer's email (case-insensitive)
                var context = _unitOfWork.GetContext();
                if (!string.IsNullOrEmpty(currentUser.Email))
                {
                    var userEmailLower = currentUser.Email.ToLower();
                    _logger.LogInformation("Searching for client record for customer user {UserId} with email {Email}", 
                        userId, currentUser.Email);
                    
                    // Try to find by Client.Email first
                    client = await context.Clients
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.Email) && 
                                                  c.Email.ToLower() == userEmailLower);

                    // If not found, try ContactPersonEmail
                    if (client == null)
                    {
                        _logger.LogInformation("Client not found by Email, trying ContactPersonEmail for {Email}", currentUser.Email);
                        client = await context.Clients
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.ContactPersonEmail) && 
                                                      c.ContactPersonEmail.ToLower() == userEmailLower);
                    }
                }

                if (client != null)
                {
                    clientId = long.Parse(client.Id);
                    _logger.LogInformation("Found client record for customer user {UserId} by email {Email}. ClientId: {ClientId}, ClientName: {ClientName}", 
                        userId, currentUser.Email, clientId, client.Name);
                }
                else
                {
                    // Try to find by user's full name
                    var userFullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                    if (!string.IsNullOrEmpty(userFullName))
                    {
                        _logger.LogInformation("Client not found by email, trying to find by name: {Name}", userFullName);
                        var namePattern = $"%{userFullName}%";
                        client = await context.Clients
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, namePattern) ||
                                                      (c.ContactPerson != null && EF.Functions.Like(c.ContactPerson, namePattern)));

                        if (client != null)
                        {
                            clientId = long.Parse(client.Id);
                            _logger.LogInformation("Found client record for customer user {UserId} by name {Name}. ClientId: {ClientId}, ClientName: {ClientName}", 
                                userId, userFullName, clientId, client.Name);
                        }
                    }
                }

                // If still not found, return empty list with a message
                if (client == null || clientId == 0)
                {
                    _logger.LogWarning("Client record not found for customer user {UserId}. Email: {Email}, Name: {Name}", 
                        userId, currentUser.Email, $"{currentUser.FirstName} {currentUser.LastName}");
                    return Ok(ResponseHelper.CreateSuccessResponse(new List<OfferResponseDTO>(), "No client record found. Please contact support to link your account to a client record."));
                }

                var result = await _offerService.GetOffersByClientAsync(clientId);

                // Filter by status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    result = result.Where(o => o.Status == status).ToList();
                }

                // Only return offers that have been sent to the client (status = "Sent" or later)
                // Include all statuses that are visible to customers after the offer has been sent
                result = result.Where(o => 
                    o.Status == "Sent" || 
                    o.Status == "UnderReview" ||
                    o.Status == "Accepted" || 
                    o.Status == "Rejected" || 
                    o.Status == "Completed").ToList();

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Customer offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer offers");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offers"));
            }
        }

        /// <summary>
        /// Get pending SalesManager approvals
        /// </summary>
        [HttpGet("pending-salesmanager-approvals")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetPendingSalesManagerApprovals()
        {
            try
            {
                var result = await _offerService.GetPendingSalesManagerApprovalsAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Pending SalesManager approvals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending SalesManager approvals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving pending SalesManager approvals"));
            }
        }

        /// <summary>
        /// Approve or reject an offer by SalesManager
        /// </summary>
        [HttpPost("{id}/salesmanager-approval")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> SalesManagerApproval(string id, [FromBody] ApproveOfferDTO approvalDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.SalesManagerApprovalAsync(id, approvalDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, approvalDto.Approved 
                    ? "Offer approved successfully" 
                    : "Offer rejected successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for SalesManager approval. OfferId: {OfferId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for SalesManager approval. OfferId: {OfferId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SalesManager approval. OfferId: {OfferId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while processing SalesManager approval"));
            }
        }

        /// <summary>
        /// Get offer by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin,SalesMan,Customer,Doctor,Technician")]
        public async Task<IActionResult> GetOffer(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("User ID not found"));
                }

                var userRole = GetCurrentUserRole();
                
                // For customer roles, validate they can access this offer
                if (!string.IsNullOrEmpty(userRole) && 
                    (userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase)))
                {
                    // Get current user to find their client record
                    var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (currentUser == null)
                    {
                        return BadRequest(ResponseHelper.CreateErrorResponse("User not found"));
                    }

                    long clientId = 0;
                    Client? client = null;

                    // Try to find client by customer's email (case-insensitive)
                    var context = _unitOfWork.GetContext();
                    if (!string.IsNullOrEmpty(currentUser.Email))
                    {
                        var userEmailLower = currentUser.Email.ToLower();
                        _logger.LogInformation("Searching for client record for customer user {UserId} with email {Email}", 
                            userId, currentUser.Email);
                        
                        // Try to find by Client.Email first
                        client = await context.Clients
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.Email) && 
                                                      c.Email.ToLower() == userEmailLower);

                        // If not found, try ContactPersonEmail
                        if (client == null)
                        {
                            _logger.LogInformation("Client not found by Email, trying ContactPersonEmail for {Email}", currentUser.Email);
                            client = await context.Clients
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.ContactPersonEmail) && 
                                                          c.ContactPersonEmail.ToLower() == userEmailLower);
                        }
                    }

                    if (client != null)
                    {
                        clientId = long.Parse(client.Id);
                        _logger.LogInformation("Found client record for customer user {UserId} by email {Email}. ClientId: {ClientId}, ClientName: {ClientName}", 
                            userId, currentUser.Email, clientId, client.Name);
                    }
                    else
                    {
                        // Try to find by user's full name
                        var userFullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                        if (!string.IsNullOrEmpty(userFullName))
                        {
                            _logger.LogInformation("Client not found by email, trying to find by name: {Name}", userFullName);
                            var namePattern = $"%{userFullName}%";
                            client = await context.Clients
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, namePattern) ||
                                                          (c.ContactPerson != null && EF.Functions.Like(c.ContactPerson, namePattern)));

                            if (client != null)
                            {
                                clientId = long.Parse(client.Id);
                                _logger.LogInformation("Found client record for customer user {UserId} by name {Name}. ClientId: {ClientId}, ClientName: {ClientName}", 
                                    userId, userFullName, clientId, client.Name);
                            }
                        }
                    }

                    if (clientId == 0)
                    {
                        _logger.LogWarning("Client record not found for customer user {UserId}. Email: {Email}, Name: {Name}", 
                            userId, currentUser.Email, $"{currentUser.FirstName} {currentUser.LastName}");
                        return BadRequest(ResponseHelper.CreateErrorResponse("Client record not found. Please contact support to link your account to a client record."));
                    }

                    // Get the offer and verify it belongs to this client
                    var result = await _offerService.GetOfferAsync(id);
                    
                    if (result == null)
                        return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));

                    // Verify the offer belongs to this client
                    if (result.ClientId != clientId.ToString())
                    {
                        _logger.LogWarning("Customer {UserId} (ClientId: {ClientId}) attempted to access offer {OfferId} belonging to ClientId {OfferClientId}", 
                            userId, clientId, id, result.ClientId);
                        return Forbid();
                    }

                    // Only return offers that have been sent to the client
                    if (result.Status != "Sent" && 
                        result.Status != "Accepted" && 
                        result.Status != "Rejected" && 
                        result.Status != "Completed")
                    {
                        _logger.LogInformation("Customer {UserId} attempted to access offer {OfferId} with status {Status} (not yet sent)", 
                            userId, id, result.Status);
                        return NotFound(ResponseHelper.CreateErrorResponse("Offer not found or not yet sent"));
                    }

                    return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer retrieved successfully"));
                }
                else
                {
                    // For other roles (SalesSupport, SalesManager, etc.), allow access
                    var result = await _offerService.GetOfferAsync(id);
                    
                    if (result == null)
                        return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));

                    return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer retrieved successfully"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer {OfferId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offer"));
            }
        }

        /// <summary>
        /// Create new offer
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.CreateOfferAsync(createDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating offer");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating offer"));
            }
        }

        /// <summary>
        /// Update offer (PATCH) - SalesManager can modify offers regardless of status
        /// </summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> UpdateOffer(string id, [FromBody] UpdateOfferDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.UpdateOfferBySalesManagerAsync(id, updateDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating offer. OfferId: {OfferId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update offer. OfferId: {OfferId}", id);
                return Unauthorized(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer. OfferId: {OfferId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating offer"));
            }
        }

        /// <summary>
        /// Create new offer with products array (equipment items)
        /// </summary>
        [HttpPost("with-items")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> CreateOfferWithItems([FromBody] CreateOfferWithItemsDTO createOfferDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var result = await _offerService.CreateOfferWithItemsAsync(createOfferDto, GetCurrentUserId());
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating offer with items");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer with items");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating offer"));
            }
        }

        // Equipment Management Endpoints
        [HttpPost("{offerId}/equipment")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> AddEquipment(string offerId, [FromBody] CreateOfferEquipmentDTO dto)
        {
            try
            {
                var result = await _offerService.AddEquipmentAsync(offerId, dto);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Equipment added"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding equipment to offer {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse($"Error adding equipment: {ex.Message}"));
            }
        }

        [HttpGet("{offerId}/equipment")]
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin")]
        public async Task<IActionResult> GetEquipment(string offerId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await GetCurrentUserAsync();
                var userRoles = user != null ? await UserManager.GetRolesAsync(user) : new List<string>();
                
                _logger.LogInformation("GetEquipment - User {UserId} with roles [{Roles}] requesting equipment for offer {OfferId}", 
                    userId, string.Join(", ", userRoles), offerId);

                // Verify offer exists
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                {
                    _logger.LogWarning("GetEquipment - Offer {OfferId} not found", offerId);
                    return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));
                }

                var result = await _offerService.GetEquipmentListAsync(offerId);
                
                _logger.LogInformation("GetEquipment - Successfully retrieved {Count} equipment items for offer {OfferId}", 
                    result?.Count ?? 0, offerId);
                
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Equipment retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment for offer {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse($"Error retrieving equipment: {ex.Message}"));
            }
        }

        [HttpDelete("{offerId}/equipment/{equipmentId}")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> DeleteEquipment(string offerId, long equipmentId)
        {
            var result = await _offerService.DeleteEquipmentAsync(offerId, equipmentId);
            return result ? Ok(ResponseHelper.CreateSuccessResponse(null, "Deleted")) : NotFound();
        }

        [HttpPost("{offerId}/equipment/{equipmentId}/upload-image")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> UploadEquipmentImage(string offerId, long equipmentId, IFormFile file)
        {
            try
            {
                var result = await _equipmentImageService.UploadEquipmentImageAsync(file, offerId, equipmentId);
                if (!result.Success)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(result.ErrorMessage ?? "Failed to upload image"));
                }

                // Save image path to equipment
                var equipment = await _offerService.UpdateEquipmentImagePathAsync(offerId, equipmentId, result.FilePath ?? string.Empty);
                
                return Ok(ResponseHelper.CreateSuccessResponse(equipment, "Image uploaded and equipment updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading equipment image. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading the image"));
            }
        }

        /// <summary>
        /// Get equipment image URL (returns path)
        /// </summary>
        [HttpGet("{offerId}/equipment/{equipmentId}/image")]
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin")]
        public async Task<IActionResult> GetEquipmentImage(string offerId, long equipmentId)
        {
            try
            {
                // Use optimized method to get image path directly
                var imagePath = await _offerService.GetEquipmentImagePathAsync(offerId, equipmentId);
                
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Image not found for this equipment"));
                }

                // Log the image path for debugging
                _logger.LogInformation("Retrieved image path for equipment. OfferId: {OfferId}, EquipmentId: {EquipmentId}, ImagePath: {ImagePath}", offerId, equipmentId, imagePath);

                // Return the image path (client will construct full URL)
                // Use a DTO-like structure to avoid JSON property name conflicts
                var responseData = new Dictionary<string, object>
                {
                    { "imagePath", imagePath },
                    { "equipmentId", equipmentId },
                    { "offerId", offerId }
                };
                
                return Ok(ResponseHelper.CreateSuccessResponse(responseData, "Image path retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment image. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the image"));
            }
        }

        /// <summary>
        /// Get equipment image file directly (returns file stream)
        /// </summary>
        [HttpGet("{offerId}/equipment/{equipmentId}/image-file")]
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin")]
        public async Task<IActionResult> GetEquipmentImageFile(string offerId, long equipmentId)
        {
            try
            {
                var imagePath = await _offerService.GetEquipmentImagePathAsync(offerId, equipmentId);
                
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Image not found for this equipment"));
                }

                // Get the physical file path using WebRootPath
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("Image file not found at path: {FullPath}", fullPath);
                    return NotFound(ResponseHelper.CreateErrorResponse("Image file not found on server"));
                }

                // Determine content type based on file extension
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream"
                };

                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment image file. OfferId: {OfferId}, EquipmentId: {EquipmentId}", offerId, equipmentId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the image file"));
            }
        }

        // Terms Management
        [HttpPost("{offerId}/terms")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> AddOrUpdateTerms(string offerId, [FromBody] CreateOfferTermsDTO dto)
        {
            var result = await _offerService.AddOrUpdateTermsAsync(offerId, dto);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Terms saved"));
        }

        // Installment Plans
        [HttpPost("{offerId}/installments")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> CreateInstallmentPlan(string offerId, [FromBody] CreateInstallmentPlanDTO dto)
        {
            var result = await _offerService.CreateInstallmentPlanAsync(offerId, dto);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Installment plan created"));
        }

        // PDF Export (On-demand generation, no storage)
        /// <summary>
        /// Generate and download PDF for an offer (generated on-demand, not stored)
        /// </summary>
        [HttpGet("{offerId}/pdf")]
        [Authorize(Roles = "SalesMan,SalesSupport,SalesManager,SuperAdmin,Customer,Doctor,Technician")]
        public async Task<IActionResult> GetOfferPdf(string offerId, [FromQuery] string language = "en")
        {
            try
            {
                _logger.LogInformation("PDF generation requested for offer {OfferId} with language {Language}", offerId, language);

                // Validate language
                if (language != "en" && language != "ar")
                {
                    _logger.LogWarning("Invalid language '{Language}' provided, defaulting to 'en'", language);
                    language = "en";
                }

                // Verify offer exists
                var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                if (offer == null)
                {
                    _logger.LogWarning("PDF generation requested for non-existent offer {OfferId}", offerId);
                    return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));
                }

                // Check authorization based on user role
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("PDF generation requested without valid user ID");
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Authentication required"));
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("PDF generation requested for non-existent user {UserId}", userId);
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not found"));
                }

                var userRoles = await UserManager.GetRolesAsync(user);
                var isSalesMan = userRoles.Contains("SalesMan");
                var isManagerOrAdmin = userRoles.Contains("SalesManager") || 
                                      userRoles.Contains("SuperAdmin") || 
                                      userRoles.Contains("SalesSupport");
                var isCustomer = userRoles.Contains("Customer") || 
                                userRoles.Contains("Doctor") || 
                                userRoles.Contains("Technician");

                // For SalesMan role, verify they are assigned to this offer
                if (isSalesMan && !isManagerOrAdmin)
                {
                    if (string.IsNullOrEmpty(offer.AssignedTo) || offer.AssignedTo != userId)
                    {
                        _logger.LogWarning("SalesMan {UserId} attempted to access PDF for offer {OfferId} which is not assigned to them", userId, offerId);
                        return StatusCode(403, ResponseHelper.CreateErrorResponse("You can only access PDFs for offers assigned to you"));
                    }
                }

                // For Customer/Doctor/Technician, verify they are the client for this offer
                if (isCustomer && !isManagerOrAdmin)
                {
                    var client = await _unitOfWork.Clients.GetByIdAsync(offer.ClientId);
                    if (client == null || string.IsNullOrEmpty(client.Email))
                    {
                        _logger.LogWarning("Customer {UserId} attempted to access PDF for offer {OfferId} with invalid client", userId, offerId);
                        return StatusCode(403, ResponseHelper.CreateErrorResponse("You do not have permission to access this offer PDF"));
                    }

                    // Check if user email matches client email (case-insensitive)
                    if (!string.Equals(user.Email, client.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Customer {UserId} attempted to access PDF for offer {OfferId} which belongs to a different client", userId, offerId);
                        return StatusCode(403, ResponseHelper.CreateErrorResponse("You can only access PDFs for your own offers"));
                    }
                }

                // Generate PDF on-demand (no storage - streams directly to client)
                // Note: PDFs appearing in wwwroot are from browser downloads, not server-side saving
                var pdfBytes = await _pdfService.GenerateOfferPdfAsync(offerId, language);
                
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogError("PDF generation returned empty bytes for offer {OfferId}", offerId);
                    return StatusCode(500, ResponseHelper.CreateErrorResponse("PDF generation failed: empty result"));
                }

                _logger.LogInformation("PDF generated successfully for offer {OfferId} (size: {Size} bytes)", offerId, pdfBytes.Length);
                
                // Set response headers for better compatibility
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"Offer_{offerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf\"");
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                // Stream PDF directly to client - no file is saved on server
                return File(pdfBytes, "application/pdf", $"Offer_{offerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for PDF generation. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for PDF generation. OfferId: {OfferId}", offerId);
                return StatusCode(403, ResponseHelper.CreateErrorResponse("You do not have permission to access this offer PDF"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for offer {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while generating PDF"));
            }
        }

        // Send to SalesMan
        [HttpPost("{offerId}/send-to-salesman")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> SendToSalesMan(string offerId)
        {
            var result = await _offerService.SendToSalesManAsync(offerId, GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Sent"));
        }

        /// <summary>
        /// Assign or reassign offer to a SalesMan
        /// </summary>
        [HttpPut("{offerId}/assign-to-salesman")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> AssignOfferToSalesMan(string offerId, [FromBody] AssignOfferToSalesManDTO assignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.AssignOfferToSalesManAsync(offerId, assignDto.SalesManId, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer assigned to salesman successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for assigning offer to salesman");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning offer to salesman");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while assigning offer to salesman"));
            }
        }

        [HttpGet("assigned-to-me")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> GetAssignedOffers()
        {
            var result = await _offerService.GetOffersBySalesManAsync(GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Retrieved"));
        }

        /// <summary>
        /// Get offers assigned to a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("by-SalesMan/{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetOffersBySalesMan(string salesmanId)
        {
            try
            {
                var result = await _offerService.GetOffersBySalesManAsync(salesmanId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "SalesMan offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offers for salesman {SalesManId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving salesman offers"));
            }
        }

        /// <summary>
        /// Get all offers with filters and pagination (SalesManager and SuperAdmin)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetAllOffersWithFilters(
            [FromQuery] string? status = null,
            [FromQuery] string? salesmanId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Verify user is authenticated and has the correct role
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                {
                    _logger.LogWarning("GetAllOffersWithFilters - User not authenticated");
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not authenticated"));
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    _logger.LogWarning("GetAllOffersWithFilters - User not found");
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not found"));
                }

                var userRoles = await UserManager.GetRolesAsync(user);
                var isManager = userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin");
                
                if (!isManager)
                {
                    _logger.LogWarning("GetAllOffersWithFilters - User {UserId} ({UserName}) attempted to access but is not a manager. Roles: [{Roles}]", 
                        currentUserId, user.UserName, string.Join(", ", userRoles));
                    return StatusCode(403, ResponseHelper.CreateErrorResponse("Access denied. Only SalesManager and SuperAdmin can access this endpoint."));
                }

                _logger.LogInformation("GetAllOffersWithFilters - User {UserId} ({UserName}) with roles [{Roles}] accessing all offers. Filters: Status={Status}, SalesManId={SalesManId}, Page={Page}, PageSize={PageSize}",
                    currentUserId, user.UserName, string.Join(", ", userRoles), status ?? "all", salesmanId ?? "all", page, pageSize);

                var result = await _offerService.GetAllOffersWithFiltersAsync(
                    status: status,
                    salesmanId: salesmanId,
                    page: page,
                    pageSize: pageSize,
                    startDate: startDate,
                    endDate: endDate);

                // Ensure the response includes all pagination fields
                var response = new
                {
                    offers = result.Offers,
                    totalCount = result.TotalCount,
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                };

                return Ok(ResponseHelper.CreateSuccessResponse(response, "Offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all offers with filters");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offers"));
            }
        }

        /// <summary>
        /// Get offer request details for creating an offer
        /// </summary>
        [HttpGet("request/{requestId}/details")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> GetOfferRequestDetails(long requestId)
        {
            try
            {
                var result = await _offerService.GetOfferRequestDetailsAsync(requestId);
                if (result == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Offer request not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer request details retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting offer request details for request {RequestId}", requestId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offer request details"));
            }
        }

        /// <summary>
        /// Record client response to an offer (Accept/Reject)
        /// </summary>
        [HttpPost("{offerId}/client-response")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin,SalesSupport,Customer,Doctor,Technician")]
        public async Task<IActionResult> RecordClientResponse(string offerId, [FromBody] RecordClientResponseDTO dto)
        {
            try
            {
                // Log the incoming request for debugging
                _logger.LogInformation("RecordClientResponse called. OfferId: {OfferId}, UserId: {UserId}, DTO: Response={Response}, Accepted={Accepted}", 
                    offerId, GetCurrentUserId(), dto?.Response, dto?.Accepted);

                if (!ModelState.IsValid)
                {
                    var validationErrors = ValidationHelperService.FormatValidationErrors(ModelState);
                    _logger.LogWarning("ModelState validation failed for RecordClientResponse. OfferId: {OfferId}, Errors: {Errors}", 
                        offerId, validationErrors);
                    return BadRequest(validationErrors);
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in RecordClientResponse. OfferId: {OfferId}", offerId);
                    return BadRequest(ResponseHelper.CreateErrorResponse("User ID not found"));
                }

                var userRole = GetCurrentUserRole();
                
                // For customer roles, validate they can respond to this offer
                if (!string.IsNullOrEmpty(userRole) && 
                    (userRole.Equals("Customer", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Equals("Doctor", StringComparison.OrdinalIgnoreCase) ||
                     userRole.Equals("Technician", StringComparison.OrdinalIgnoreCase)))
                {
                    // Get current user to find their client record
                    var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (currentUser == null)
                    {
                        return BadRequest(ResponseHelper.CreateErrorResponse("User not found"));
                    }

                    long clientId = 0;
                    Client? client = null;

                    // Try to find client by customer's email (case-insensitive)
                    var context = _unitOfWork.GetContext();
                    if (!string.IsNullOrEmpty(currentUser.Email))
                    {
                        var userEmailLower = currentUser.Email.ToLower();
                        
                        // Try to find by Client.Email first
                        client = await context.Clients
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.Email) && 
                                                      c.Email.ToLower() == userEmailLower);

                        // If not found, try ContactPersonEmail
                        if (client == null)
                        {
                            client = await context.Clients
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => !string.IsNullOrEmpty(c.ContactPersonEmail) && 
                                                          c.ContactPersonEmail.ToLower() == userEmailLower);
                        }
                    }

                    if (client != null)
                    {
                        clientId = long.Parse(client.Id);
                    }
                    else
                    {
                        // Try to find by user's full name
                        var userFullName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                        if (!string.IsNullOrEmpty(userFullName))
                        {
                            var namePattern = $"%{userFullName}%";
                            client = await context.Clients
                                .AsNoTracking()
                                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, namePattern) ||
                                                          (c.ContactPerson != null && EF.Functions.Like(c.ContactPerson, namePattern)));

                            if (client != null)
                            {
                                clientId = long.Parse(client.Id);
                            }
                        }
                    }

                    if (clientId == 0)
                    {
                        _logger.LogWarning("Client record not found for customer user {UserId}. Email: {Email}, Name: {Name}", 
                            userId, currentUser.Email, $"{currentUser.FirstName} {currentUser.LastName}");
                        return BadRequest(ResponseHelper.CreateErrorResponse("Client record not found. Please contact support to link your account to a client record."));
                    }

                    // Get the offer and verify it belongs to this client
                    var offer = await _unitOfWork.SalesOffers.GetByIdAsync(offerId);
                    if (offer == null)
                    {
                        return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));
                    }

                    // Verify the offer belongs to this client
                    if (offer.ClientId != clientId.ToString())
                    {
                        _logger.LogWarning("Customer {UserId} (ClientId: {ClientId}) attempted to respond to offer {OfferId} belonging to ClientId {OfferClientId}", 
                            userId, clientId, offerId, offer.ClientId);
                        return Forbid();
                    }

                    // Only allow response if offer status is "Sent"
                    if (offer.Status != "Sent")
                    {
                        _logger.LogInformation("Customer {UserId} attempted to respond to offer {OfferId} with status {Status} (not in Sent status)", 
                            userId, offerId, offer.Status);
                        return BadRequest(ResponseHelper.CreateErrorResponse("You can only respond to offers that have been sent to you."));
                    }
                }

                var result = await _offerService.RecordClientResponseAsync(offerId, dto.Response, dto.Accepted, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client response recorded successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for recording client response. OfferId: {OfferId}", offerId);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for recording client response. OfferId: {OfferId}, Error: {Error}", offerId, ex.Message);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording client response. OfferId: {OfferId}, Error: {Error}, StackTrace: {StackTrace}", 
                    offerId, ex.Message, ex.StackTrace);
                return StatusCode(500, ResponseHelper.CreateErrorResponse($"An error occurred while recording client response: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mark offer as completed
        /// </summary>
        [HttpPost("{offerId}/complete")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> CompleteOffer(string offerId, [FromBody] CompleteOfferDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _offerService.CompleteOfferAsync(offerId, dto?.CompletionNotes, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer marked as completed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for completing offer. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing offer. OfferId: {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while completing offer"));
            }
        }

        /// <summary>
        /// Mark offer as needing modification
        /// SalesMan can request modifications for offers assigned to them (on behalf of clients)
        /// </summary>
        [HttpPost("{offerId}/needs-modification")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin,SalesMan")]
        public async Task<IActionResult> MarkAsNeedsModification(string offerId, [FromBody] NeedsModificationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.MarkAsNeedsModificationAsync(offerId, dto?.Reason, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer marked as needing modification successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for marking offer as needing modification. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking offer as needing modification. OfferId: {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking offer as needing modification"));
            }
        }

        /// <summary>
        /// Mark offer as under review
        /// </summary>
        [HttpPost("{offerId}/under-review")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> MarkAsUnderReview(string offerId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _offerService.MarkAsUnderReviewAsync(offerId, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer marked as under review successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for marking offer as under review. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking offer as under review. OfferId: {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking offer as under review"));
            }
        }

        /// <summary>
        /// Resume offer from under review back to sent status
        /// </summary>
        [HttpPost("{offerId}/resume-from-review")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> ResumeFromUnderReview(string offerId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _offerService.ResumeFromUnderReviewAsync(offerId, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer resumed from under review successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for resuming offer from under review. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for resuming offer from under review. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming offer from under review. OfferId: {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while resuming offer from under review"));
            }
        }

        /// <summary>
        /// Update expired offers (background job endpoint)
        /// </summary>
        [HttpPost("update-expired")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateExpiredOffers()
        {
            try
            {
                var count = await _offerService.UpdateExpiredOffersAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(new { expiredCount = count }, $"Updated {count} offers to expired status"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expired offers");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating expired offers"));
            }
        }

        /// <summary>
        /// Get recent offer activity
        /// </summary>
        [HttpGet("recent-activity")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 20)
        {
            try
            {
                var result = await _offerService.GetRecentActivityAsync(limit);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Recent activity retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving recent activity"));
            }
        }
    }
}


