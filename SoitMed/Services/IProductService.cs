using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for product catalog management service
    /// </summary>
    public interface IProductService
    {
        Task<List<ProductResponseDTO>> GetAllProductsAsync(string? category = null, string? categoryId = null, bool? inStock = null);
        Task<ProductResponseDTO?> GetProductByIdAsync(string id);
        Task<List<ProductResponseDTO>> GetProductsByCategoryAsync(string category);
        Task<List<ProductResponseDTO>> SearchProductsAsync(string searchTerm);
        Task<ProductResponseDTO> CreateProductAsync(CreateProductDTO createDto, string userId);
        Task<ProductResponseDTO> UpdateProductAsync(string id, UpdateProductDTO updateDto, string userId);
        Task<bool> DeleteProductAsync(string id);
        Task<ProductResponseDTO> UpdateProductImageAsync(string id, string imagePath);
        Task<ProductResponseDTO> UpdateProviderImageAsync(string id, string imagePath);
        Task<ProductResponseDTO> UpdateDataSheetAsync(string id, string pdfPath);
        Task<ProductResponseDTO> UpdateCatalogAsync(string id, string pdfPath);
        Task<ProductResponseDTO> UpdateInventoryQuantityAsync(string id, int inventoryQuantity);
    }
}



