using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for product catalog management service
    /// </summary>
    public interface IProductService
    {
        Task<List<ProductResponseDTO>> GetAllProductsAsync(string? category = null, bool? inStock = null);
        Task<ProductResponseDTO?> GetProductByIdAsync(long id);
        Task<List<ProductResponseDTO>> GetProductsByCategoryAsync(string category);
        Task<List<ProductResponseDTO>> SearchProductsAsync(string searchTerm);
        Task<ProductResponseDTO> CreateProductAsync(CreateProductDTO createDto, string userId);
        Task<ProductResponseDTO> UpdateProductAsync(long id, UpdateProductDTO updateDto, string userId);
        Task<bool> DeleteProductAsync(long id);
        Task<ProductResponseDTO> UpdateProductImageAsync(long id, string imagePath);
    }
}



