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
    /// Controller for managing product catalog
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IPdfUploadService _pdfUploadService;
        private readonly ILogger<ProductController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductController(
            IProductService productService,
            IImageUploadService imageUploadService,
            IPdfUploadService pdfUploadService,
            ILogger<ProductController> logger,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment) 
            : base(userManager)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
            _pdfUploadService = pdfUploadService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Get all products with optional filters
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin,Doctor")]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] string? category = null,
            [FromQuery] string? categoryId = null,
            [FromQuery] bool? inStock = null)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(category, categoryId, inStock);
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
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin,Doctor")]
        public async Task<IActionResult> GetProduct(string id)
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
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin,Doctor")]
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
        [Authorize(Roles = "SalesSupport,SalesManager,SalesMan,SuperAdmin,Doctor")]
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
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDTO updateDto)
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
        public async Task<IActionResult> DeleteProduct(string id)
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
        public async Task<IActionResult> UploadProductImage(string id, IFormFile file)
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

        /// <summary>
        /// Upload provider logo/image for a product
        /// </summary>
        [HttpPost("{id}/upload-provider-image")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UploadProviderImage(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File is required"));
                }

                // Validate using shared service helpers
                if (!_imageUploadService.IsValidImageFile(file))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Invalid file type. Only JPG, PNG, GIF, and SVG are allowed"));
                }

                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File size exceeds 5MB limit"));
                }

                var uploadResult = await _imageUploadService.UploadImageAsync(file, "providers");
                if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.FilePath))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload provider image"));
                }

                var result = await _productService.UpdateProviderImageAsync(id, uploadResult.FilePath);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Provider image uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for uploading provider image. ProductId: {ProductId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading provider image. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading provider image"));
            }
        }

        /// <summary>
        /// Handle CORS preflight for provider image endpoint
        /// </summary>
        [HttpOptions("{id}/provider-image-file")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult OptionsProviderImageFile()
        {
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
            Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            Response.Headers.Append("Access-Control-Max-Age", "86400");
            return Ok();
        }

        /// <summary>
        /// Get provider image file directly (returns file stream)
        /// AllowAnonymous is required because img tags cannot send Authorization headers
        /// </summary>
        [HttpGet("{id}/provider-image-file")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetProviderImageFile(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Product not found"));
                }

                if (string.IsNullOrWhiteSpace(product.ProviderImagePath))
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Provider image not found for this product"));
                }

                // Get the physical file path using WebRootPath
                var fullPath = Path.Combine(_environment.WebRootPath, product.ProviderImagePath.Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("Provider image file not found at path: {FullPath}", fullPath);
                    return NotFound(ResponseHelper.CreateErrorResponse("Provider image file not found on server"));
                }

                // Determine content type based on file extension
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".svg" => "image/svg+xml",
                    _ => "application/octet-stream"
                };

                // Add CORS headers to allow cross-origin image loading
                // These headers must be set before returning the file to prevent ORB (Opaque Response Blocking)
                Response.Headers.Append("Access-Control-Allow-Origin", "*");
                Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                Response.Headers.Append("Access-Control-Expose-Headers", "Content-Length, Content-Type");
                
                // Read file bytes and return with proper content type
                // File() method will set Content-Type header automatically
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider image file. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the provider image file"));
            }
        }

        /// <summary>
        /// Update inventory quantity for a product
        /// </summary>
        [HttpPut("{id}/inventory")]
        [Authorize(Roles = "InventoryManager,SuperAdmin")]
        public async Task<IActionResult> UpdateInventoryQuantity(string id, [FromBody] UpdateInventoryDTO updateDto)
        {
            try
            {
                if (updateDto == null)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Inventory update data is required"));
                }

                var result = await _productService.UpdateInventoryQuantityAsync(id, updateDto.InventoryQuantity);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Inventory quantity updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating inventory. ProductId: {ProductId}", id);
                return NotFound(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inventory quantity. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating inventory quantity"));
            }
        }

        /// <summary>
        /// Upload data sheet PDF for a product
        /// </summary>
        [HttpPost("{id}/upload-data-sheet")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UploadDataSheet(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File is required"));
                }

                if (!_pdfUploadService.IsValidPdfFile(file))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Invalid file type. Only PDF files are allowed (max 5MB)"));
                }

                var uploadResult = await _pdfUploadService.UploadPdfAsync(file, "product-documents");
                if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.FilePath))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload data sheet"));
                }

                var result = await _productService.UpdateDataSheetAsync(id, uploadResult.FilePath);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Data sheet uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for uploading data sheet. ProductId: {ProductId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading data sheet. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading data sheet"));
            }
        }

        /// <summary>
        /// Upload catalog PDF for a product
        /// </summary>
        [HttpPost("{id}/upload-catalog")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UploadCatalog(string id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("File is required"));
                }

                if (!_pdfUploadService.IsValidPdfFile(file))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Invalid file type. Only PDF files are allowed (max 5MB)"));
                }

                var uploadResult = await _pdfUploadService.UploadPdfAsync(file, "product-documents");
                if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.FilePath))
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload catalog"));
                }

                var result = await _productService.UpdateCatalogAsync(id, uploadResult.FilePath);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Catalog uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for uploading catalog. ProductId: {ProductId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading catalog. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading catalog"));
            }
        }

        /// <summary>
        /// Get data sheet PDF file
        /// </summary>
        [HttpGet("{id}/data-sheet-file")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetDataSheetFile(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Product not found"));
                }

                if (string.IsNullOrWhiteSpace(product.DataSheetPath))
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Data sheet not found for this product"));
                }

                var fullPath = Path.Combine(_environment.WebRootPath, product.DataSheetPath.Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("Data sheet file not found at path: {FullPath}", fullPath);
                    return NotFound(ResponseHelper.CreateErrorResponse("Data sheet file not found on server"));
                }

                Response.Headers.Append("Access-Control-Allow-Origin", "*");
                Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                Response.Headers.Append("Access-Control-Expose-Headers", "Content-Length, Content-Type");
                Response.Headers.Append("Content-Disposition", "inline");
                
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data sheet file. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the data sheet file"));
            }
        }

        /// <summary>
        /// Get catalog PDF file
        /// </summary>
        [HttpGet("{id}/catalog-file")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetCatalogFile(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                
                if (product == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Product not found"));
                }

                if (string.IsNullOrWhiteSpace(product.CatalogPath))
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Catalog not found for this product"));
                }

                var fullPath = Path.Combine(_environment.WebRootPath, product.CatalogPath.Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogWarning("Catalog file not found at path: {FullPath}", fullPath);
                    return NotFound(ResponseHelper.CreateErrorResponse("Catalog file not found on server"));
                }

                Response.Headers.Append("Access-Control-Allow-Origin", "*");
                Response.Headers.Append("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                Response.Headers.Append("Access-Control-Expose-Headers", "Content-Length, Content-Type");
                Response.Headers.Append("Content-Disposition", "inline");
                
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving catalog file. ProductId: {ProductId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the catalog file"));
            }
        }
    }
}



