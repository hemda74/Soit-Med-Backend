using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryService _categoryService;
        private readonly ILogger<ProductCategoryController> _logger;

        public ProductCategoryController(
            IProductCategoryService categoryService,
            ILogger<ProductCategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active categories (flat list)
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Allow mobile app and unauthenticated users to see categories
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get category hierarchy (tree structure with main categories and subcategories)
        /// </summary>
        [HttpGet("hierarchy")]
        [AllowAnonymous] // Allow mobile app to fetch hierarchy
        public async Task<IActionResult> GetCategoryHierarchy()
        {
            try
            {
                var hierarchy = await _categoryService.GetCategoryHierarchyAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(hierarchy));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category hierarchy");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving category hierarchy"));
            }
        }

        /// <summary>
        /// Get only main categories (no parent)
        /// </summary>
        [HttpGet("main")]
        [AllowAnonymous] // For mobile home screen
        public async Task<IActionResult> GetMainCategories()
        {
            try
            {
                var categories = await _categoryService.GetMainCategoriesAsync();
                return Ok(ResponseHelper.CreateSuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main categories");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving main categories"));
            }
        }

        /// <summary>
        /// Get subcategories for a specific parent category
        /// </summary>
        [HttpGet("{id}/subcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategories(string id)
        {
            try
            {
                var categories = await _categoryService.GetSubCategoriesAsync(id);
                return Ok(ResponseHelper.CreateSuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories for category {CategoryId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving subcategories"));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Category not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the category"));
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryDTO createDto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(createDto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, ResponseHelper.CreateSuccessResponse(category));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating the category"));
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateProductCategoryDTO updateDto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
                return Ok(ResponseHelper.CreateSuccessResponse(category));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating the category"));
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(ResponseHelper.CreateErrorResponse("Category not found"));

                return Ok(ResponseHelper.CreateSuccessResponse("Category deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while deleting the category"));
            }
        }
    }
}

