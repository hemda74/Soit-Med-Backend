using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IProductCategoryService
    {
        Task<List<ProductCategoryDTO>> GetAllCategoriesAsync();
        Task<List<CategoryHierarchyDTO>> GetCategoryHierarchyAsync();
        Task<List<ProductCategoryDTO>> GetMainCategoriesAsync();
        Task<List<ProductCategoryDTO>> GetSubCategoriesAsync(string parentCategoryId);
        Task<ProductCategoryDTO?> GetCategoryByIdAsync(string id);
        Task<ProductCategoryDTO> CreateCategoryAsync(CreateProductCategoryDTO createDto);
        Task<ProductCategoryDTO> UpdateCategoryAsync(string id, UpdateProductCategoryDTO updateDto);
        Task<bool> DeleteCategoryAsync(string id);
    }
}

