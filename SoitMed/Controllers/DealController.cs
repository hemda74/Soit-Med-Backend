using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Services;
using SoitMed.Models.Identity;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing deals in the sales workflow
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DealController : BaseController
    {
        private readonly IDealService _dealService;
        private readonly ILogger<DealController> _logger;
        private readonly IImageUploadService _imageUploadService;

        public DealController(
            IDealService dealService,
            ILogger<DealController> logger,
            UserManager<ApplicationUser> userManager,
            IImageUploadService imageUploadService) 
            : base(userManager)
        {
            _dealService = dealService;
            _logger = logger;
            _imageUploadService = imageUploadService;
        }

        /// <summary>
        /// Create new deal
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesMan,SalesManager")]
        public async Task<IActionResult> CreateDeal([FromBody] CreateDealDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.CreateDealAsync(createDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deal created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating deal");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating deal");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating deal"));
            }
        }

        /// <summary>
        /// Get all deals
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin,Admin")]
        public async Task<IActionResult> GetDeals([FromQuery] string? status = null, [FromQuery] string? salesmanId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var result = await _dealService.GetDealsAsync(status, salesmanId, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deals"));
            }
        }

        /// <summary>
        /// Get deals awaiting salesman report
        /// Returns deals that have been approved and account created, but report not yet submitted
        /// SalesMan can only see their own deals, SalesManager and SuperAdmin can see all
        /// </summary>
        [HttpGet("awaiting-report")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetDealsAwaitingReport()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not authenticated"));
                }

                var userRole = GetCurrentUserRole();
                var result = await _dealService.GetDealsAwaitingReportAsync(userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deals awaiting report retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deals awaiting report");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deals awaiting report"));
            }
        }

        /// <summary>
        /// Get deal by ID
        /// </summary>
        [HttpGet("{id:long}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin,Admin")]
        public async Task<IActionResult> GetDeal(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not authenticated"));
                }

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not found"));
                }

                // Get user roles
                var userRoles = await UserManager.GetRolesAsync(user);
                var userRole = userRoles.Contains("SuperAdmin") ? "SuperAdmin" 
                    : userRoles.Contains("SalesManager") ? "SalesManager"
                    : userRoles.Contains("SalesMan") ? "SalesMan"
                    : "SalesMan"; // Default fallback
                
                _logger.LogInformation("GetDeal - UserId: {UserId}, UserName: {UserName}, Roles: [{Roles}], UserRole: {UserRole}, DealId: {DealId}",
                    userId, user.UserName, string.Join(", ", userRoles), userRole, id);

                var result = await _dealService.GetDealAsync(id, userId, userRole);

                if (result == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Deal not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deal retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to deal {DealId}: {Message}", id, ex.Message);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deal");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deal"));
            }
        }

        /// <summary>
        /// Get deals by client
        /// </summary>
        [HttpGet("by-client/{clientId}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin,Admin")]
        public async Task<IActionResult> GetDealsByClient(long clientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var result = await _dealService.GetDealsByClientAsync(clientId, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client deals retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to client deals");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client deals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving client deals"));
            }
        }

        /// <summary>
        /// Get deals by salesman
        /// </summary>
        [HttpGet("by-SalesMan/{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetDealsBySalesMan(string salesmanId, [FromQuery] string? status = null)
        {
            try
            {
                var result = await _dealService.GetDealsBySalesManAsync(salesmanId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "SalesMan deals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salesman deals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving salesman deals"));
            }
        }

        /// <summary>
        /// Manager approval for deal
        /// </summary>
        [HttpPost("{id}/manager-approval")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> ManagerApproval(long id, [FromBody] ApproveDealDTO approvalDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.ManagerApprovalAsync(id, approvalDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Manager approval processed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for manager approval");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing manager approval");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while processing manager approval"));
            }
        }

        /// <summary>
        /// SuperAdmin approval for deal
        /// </summary>
        [HttpPost("{id}/superAdmin-approval")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuperAdminApproval(long id, [FromBody] ApproveDealDTO approvalDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.SuperAdminApprovalAsync(id, approvalDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "SuperAdmin approval processed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for SuperAdmin approval");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SuperAdmin approval");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while processing SuperAdmin approval"));
            }
        }

        /// <summary>
        /// Get pending manager approvals
        /// </summary>
        [HttpGet("pending-manager-approvals")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetPendingManagerApprovals()
        {
            try
            {
                var result = await _dealService.GetPendingManagerApprovalsAsync();

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Pending manager approvals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending manager approvals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving pending manager approvals"));
            }
        }

        /// <summary>
        /// Get pending SuperAdmin approvals
        /// </summary>
        [HttpGet("pending-superAdmin-approvals")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPendingSuperAdminApprovals()
        {
            try
            {
                var result = await _dealService.GetPendingSuperAdminApprovalsAsync();

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Pending SuperAdmin approvals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending SuperAdmin approvals");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving pending SuperAdmin approvals"));
            }
        }

        /// <summary>
        /// Mark deal as completed
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> MarkDealAsCompleted(long id, [FromBody] CompleteDealDTO completeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.MarkDealAsCompletedAsync(id, completeDto.CompletionNotes, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deal marked as completed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for completing deal");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing deal");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while completing deal"));
            }
        }

        /// <summary>
        /// Mark deal as failed
        /// </summary>
        [HttpPost("{id}/fail")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> MarkDealAsFailed(long id, [FromBody] FailDealDTO failDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.MarkDealAsFailedAsync(id, failDto.FailureNotes, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deal marked as failed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for failing deal");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error failing deal");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while failing deal"));
            }
        }

        /// <summary>
        /// Mark client account as created (by Admin)
        /// </summary>
        [HttpPost("{id}/mark-account-created")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> MarkClientAccountCreated(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _dealService.MarkClientAccountCreatedAsync(id, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client account marked as created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for marking account created");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for marking account created");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking account as created");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking account as created"));
            }
        }

        /// <summary>
        /// Upload report attachment image
        /// </summary>
        [HttpPost("upload-report-attachment")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> UploadReportAttachment([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File is required"));
                }

                // Validate file type
                if (!_imageUploadService.IsValidImageFile(file))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Invalid image file. Please upload a valid JPG, JPEG, PNG, or GIF image (max 5MB)."));
                }

                // Upload image to deal-reports folder
                var uploadResult = await _imageUploadService.UploadImageAsync(file, "deal-reports");

                if (!uploadResult.Success)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload image"));
                }

                // Return the file path that can be used to construct the URL
                // The path will be relative like "deal-reports/guid.jpg"
                return Ok(ResponseHelper.CreateSuccessResponse(new { 
                    filePath = uploadResult.FilePath,
                    fileName = uploadResult.FileName 
                }, "Image uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading report attachment");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading image"));
            }
        }

        /// <summary>
        /// Submit salesman report
        /// </summary>
        [HttpPost("{id}/submit-report")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> SubmitSalesManReport(long id, [FromBody] SubmitReportDTO reportDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.SubmitSalesManReportAsync(id, reportDto.ReportText, reportDto.ReportAttachments, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Report submitted successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for submitting report");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for submitting report");
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for submitting report");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting report");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while submitting report"));
            }
        }

        /// <summary>
        /// Get deals for legal department
        /// </summary>
        [HttpGet("legal")]
        [CaseInsensitiveRoleAuthorization("LegalManager", "LegalEmployee", "SuperAdmin")]
        public async Task<IActionResult> GetDealsForLegal()
        {
            try
            {
                var result = await _dealService.GetDealsForLegalAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deals for legal");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deals"));
            }
        }

        /// <summary>
        /// Get total deals count
        /// </summary>
        [HttpGet("total-count")]
        [Authorize(Roles = "LegalManager,LegalEmployee,SuperAdmin,SalesManager")]
        public async Task<IActionResult> GetTotalDealsCount()
        {
            try
            {
                var count = await _dealService.GetTotalDealsCountAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(new { totalCount = count }, "Total deals count retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total deals count");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving total deals count"));
            }
        }

        /// <summary>
        /// Get deal statistics for accounting dashboard
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "FinanceManager,FinanceEmployee,SuperAdmin")]
        public async Task<IActionResult> GetDealStatistics()
        {
            try
            {
                var statistics = await _dealService.GetDealStatisticsAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(statistics, "Deal statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deal statistics");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deal statistics"));
            }
        }

        /// <summary>
        /// Mark deal as reviewed by legal (archive)
        /// </summary>
        [HttpPost("{id}/mark-as-reviewed")]
        [Authorize(Roles = "LegalManager,LegalEmployee")]
        public async Task<IActionResult> MarkDealAsLegalReviewed(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not authenticated"));
                }

                var result = await _dealService.MarkDealAsLegalReviewedAsync(id, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deal marked as reviewed successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for marking deal as reviewed");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for marking deal as reviewed");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking deal as reviewed");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking deal as reviewed"));
            }
        }

        /// <summary>
        /// Submit first salesman review
        /// </summary>
        [HttpPost("{id}/submit-first-review")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> SubmitFirstSalesManReview(long id, [FromBody] SubmitReviewDTO reviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.SubmitFirstSalesManReviewAsync(id, reviewDto.ReviewText, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "First review submitted successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for submitting first review");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for submitting first review");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for submitting first review");
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting first review");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while submitting first review"));
            }
        }

        /// <summary>
        /// Submit second salesman review
        /// </summary>
        [HttpPost("{id}/submit-second-review")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> SubmitSecondSalesManReview(long id, [FromBody] SubmitReviewDTO reviewDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.SubmitSecondSalesManReviewAsync(id, reviewDto.ReviewText, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Second review submitted successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for submitting second review");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for submitting second review");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for submitting second review");
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting second review");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while submitting second review"));
            }
        }

        /// <summary>
        /// Set client username and password (by Admin)
        /// </summary>
        [HttpPost("{id}/set-credentials")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> SetClientCredentials(long id, [FromBody] SetClientCredentialsDTO credentialsDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _dealService.SetClientCredentialsAsync(id, credentialsDto.Username, credentialsDto.Password, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client credentials set successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for setting credentials");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation for setting credentials");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting client credentials");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while setting client credentials"));
            }
        }

        /// <summary>
        /// Get deals awaiting reviews and account setup
        /// </summary>
        [HttpGet("awaiting-reviews-and-setup")]
        [Authorize(Roles = "SalesMan,Admin,SuperAdmin")]
        public async Task<IActionResult> GetDealsAwaitingReviewsAndAccountSetup()
        {
            try
            {
                var result = await _dealService.GetDealsAwaitingReviewsAndAccountSetupAsync();

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Deals retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deals awaiting reviews and account setup");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving deals"));
            }
        }
    }
}
