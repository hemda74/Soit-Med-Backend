using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

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
        private readonly IPdfExportService _pdfExportService;
        private readonly ILogger<OfferController> _logger;

        public OfferController(
            IOfferService offerService,
            IOfferEquipmentImageService equipmentImageService,
            IPdfExportService pdfExportService,
            ILogger<OfferController> logger,
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _offerService = offerService;
            _equipmentImageService = equipmentImageService;
            _pdfExportService = pdfExportService;
            _logger = logger;
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

        // Equipment Management Endpoints
        [HttpPost("{offerId}/equipment")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> AddEquipment(long offerId, [FromBody] CreateOfferEquipmentDTO dto)
        {
            var result = await _offerService.AddEquipmentAsync(offerId, dto);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Equipment added"));
        }

        [HttpGet("{offerId}/equipment")]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman")]
        public async Task<IActionResult> GetEquipment(long offerId)
        {
            var result = await _offerService.GetEquipmentListAsync(offerId);
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Equipment retrieved"));
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
            var result = await _equipmentImageService.UploadEquipmentImageAsync(file, offerId, equipmentId);
            return result.Success ? Ok(ResponseHelper.CreateSuccessResponse(result, "Uploaded")) : BadRequest(result);
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
        [HttpGet("{offerId}/export-pdf")]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman")]
        public async Task<IActionResult> ExportOfferPdf(long offerId)
        {
            var pdfBytes = await _pdfExportService.GenerateOfferPdfAsync(offerId);
            return File(pdfBytes, "application/pdf", $"offer-{offerId}.pdf");
        }

        // Send to Salesman
        [HttpPost("{offerId}/send-to-salesman")]
        [Authorize(Roles = "SalesSupport,SalesManager")]
        public async Task<IActionResult> SendToSalesman(long offerId)
        {
            var result = await _offerService.SendToSalesmanAsync(offerId, GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Sent"));
        }

        [HttpGet("assigned-to-me")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetAssignedOffers()
        {
            var result = await _offerService.GetOffersBySalesmanAsync(GetCurrentUserId());
            return Ok(ResponseHelper.CreateSuccessResponse(result, "Retrieved"));
        }
    }
}

