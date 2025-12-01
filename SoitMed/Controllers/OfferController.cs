using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;
using System.IO;

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
        private readonly ILogger<OfferController> _logger;
        private readonly IWebHostEnvironment _environment;

        public OfferController(
            IOfferService offerService,
            IOfferEquipmentImageService equipmentImageService,
            ILogger<OfferController> logger,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment) 
            : base(userManager)
        {
            _offerService = offerService;
            _equipmentImageService = equipmentImageService;
            _logger = logger;
            _environment = environment;
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
                var salesmen = await UserManager.GetUsersInRoleAsync("Salesman");

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
        public async Task<IActionResult> SalesManagerApproval(long id, [FromBody] ApproveOfferDTO approvalDto)
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
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin,Salesman")]
        public async Task<IActionResult> GetOffer(long id)
        {
            try
            {
                var result = await _offerService.GetOfferAsync(id);
                
                if (result == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Offer not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer");
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
        public async Task<IActionResult> UpdateOffer(long id, [FromBody] UpdateOfferDTO updateDto)
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
        public async Task<IActionResult> AddEquipment(long offerId, [FromBody] CreateOfferEquipmentDTO dto)
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
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> GetEquipment(long offerId)
        {
            try
            {
                var result = await _offerService.GetEquipmentListAsync(offerId);
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
        public async Task<IActionResult> DeleteEquipment(long offerId, long equipmentId)
        {
            var result = await _offerService.DeleteEquipmentAsync(offerId, equipmentId);
            return result ? Ok(ResponseHelper.CreateSuccessResponse(null, "Deleted")) : NotFound();
        }

        [HttpPost("{offerId}/equipment/{equipmentId}/upload-image")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> UploadEquipmentImage(long offerId, long equipmentId, IFormFile file)
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
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> GetEquipmentImage(long offerId, long equipmentId)
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
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> GetEquipmentImageFile(long offerId, long equipmentId)
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
        public async Task<IActionResult> AddOrUpdateTerms(long offerId, [FromBody] CreateOfferTermsDTO dto)
        {
            var result = await _offerService.AddOrUpdateTermsAsync(offerId, dto);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Terms saved"));
        }

        // Installment Plans
        [HttpPost("{offerId}/installments")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> CreateInstallmentPlan(long offerId, [FromBody] CreateInstallmentPlanDTO dto)
        {
            var result = await _offerService.CreateInstallmentPlanAsync(offerId, dto);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Installment plan created"));
        }

        // PDF Export
      
       

        // Send to Salesman
        [HttpPost("{offerId}/send-to-salesman")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> SendToSalesman(long offerId)
        {
            var result = await _offerService.SendToSalesmanAsync(offerId, GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Sent"));
        }

        /// <summary>
        /// Assign or reassign offer to a Salesman
        /// </summary>
        [HttpPut("{offerId}/assign-to-salesman")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> AssignOfferToSalesman(long offerId, [FromBody] AssignOfferToSalesmanDTO assignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.AssignOfferToSalesmanAsync(offerId, assignDto.SalesmanId, userId);

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
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetAssignedOffers()
        {
            var result = await _offerService.GetOffersBySalesmanAsync(GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Retrieved"));
        }

        /// <summary>
        /// Get offers assigned to a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("by-salesman/{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetOffersBySalesman(string salesmanId)
        {
            try
            {
                var result = await _offerService.GetOffersBySalesmanAsync(salesmanId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Salesman offers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offers for salesman {SalesmanId}", salesmanId);
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

                _logger.LogInformation("GetAllOffersWithFilters - User {UserId} ({UserName}) with roles [{Roles}] accessing all offers. Filters: Status={Status}, SalesmanId={SalesmanId}, Page={Page}, PageSize={PageSize}",
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
        [Authorize(Roles = "Salesman,SalesManager,SuperAdmin")]
        public async Task<IActionResult> RecordClientResponse(long offerId, [FromBody] RecordClientResponseDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerService.RecordClientResponseAsync(offerId, dto.Response, dto.Accepted, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client response recorded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for recording client response. OfferId: {OfferId}", offerId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording client response. OfferId: {OfferId}", offerId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while recording client response"));
            }
        }

        /// <summary>
        /// Mark offer as completed
        /// </summary>
        [HttpPost("{offerId}/complete")]
        [Authorize(Roles = "Salesman,SalesManager,SuperAdmin")]
        public async Task<IActionResult> CompleteOffer(long offerId, [FromBody] CompleteOfferDTO dto)
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
        /// Salesman can request modifications for offers assigned to them (on behalf of clients)
        /// </summary>
        [HttpPost("{offerId}/needs-modification")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin,Salesman")]
        public async Task<IActionResult> MarkAsNeedsModification(long offerId, [FromBody] NeedsModificationDTO dto)
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
        public async Task<IActionResult> MarkAsUnderReview(long offerId)
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
        public async Task<IActionResult> ResumeFromUnderReview(long offerId)
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


