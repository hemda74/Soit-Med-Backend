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
    /// Controller for managing product catalog
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IImageUploadService imageUploadService,
            ILogger<ProductController> logger,
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        /// <summary>
        /// Get all products with optional filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin,Customer")]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? category = null,
            [FromQuery] bool? inStock = null)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(category, inStock);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving products"));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> GetProduct(long id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id);
                
                if (result == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Product not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving product"));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{category}")]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                var result = await _productService.GetProductsByCategoryAsync(category);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category. Category: {Category}", category);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving products"));
            }
        }

        /// <summary>
        /// Search products by name, model, provider, or description
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "SalesSupport,SalesManager,Salesman,SuperAdmin")]
        public async Task<IActionResult> SearchProducts([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Search term is required"));
                }

                var result = await _productService.SearchProductsAsync(q);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products. SearchTerm: {SearchTerm}", q);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while searching products"));
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized"));

                var result = await _productService.CreateProductAsync(createDto, userId);
                return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, 
                    ResponseHelper.CreateSuccessResponse(result, "Product created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating product");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating product"));
            }
        }

        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized"));

                var result = await _productService.UpdateProductAsync(id, updateDto, userId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Product updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating product. ProductId: {ProductId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating product"));
            }
        }

        /// <summary>
        /// Delete product (soft delete - sets IsActive to false)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                
                if (!result)
                    return NotFound(ResponseHelper.CreateErrorResponse("Product not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(null, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while deleting product"));
            }
        }

        /// <summary>
        /// Upload product image
        /// </summary>
        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UploadProductImage(long id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File is required"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Invalid file type. Only JPG, PNG, and GIF are allowed"));
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File size exceeds 5MB limit"));
                }

                // Upload image
                var uploadResult = await _imageUploadService.UploadImageAsync(file, "products");
                if (!uploadResult.Success)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload image"));
                }

                // Update product with image path
                var result = await _productService.UpdateProductImageAsync(id, uploadResult.FilePath ?? string.Empty);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Product image uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for uploading product image. ProductId: {ProductId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading product image"));
            }
        }
    }
}



