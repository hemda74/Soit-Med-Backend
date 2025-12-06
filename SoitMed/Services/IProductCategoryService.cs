using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IProductCategoryService
    {
        Task<List<ProductCategoryDTO>> GetAllCategoriesAsync();
        Task<List<CategoryHierarchyDTO>> GetCategoryHierarchyAsync();
        Task<List<ProductCategoryDTO>> GetMainCategoriesAsync();
        Task<List<ProductCategoryDTO>> GetSubCategoriesAsync(long parentCategoryId);
        Task<ProductCategoryDTO?> GetCategoryByIdAsync(long id);
        Task<ProductCategoryDTO> CreateCategoryAsync(CreateProductCategoryDTO createDto);
        Task<ProductCategoryDTO> UpdateCategoryAsync(long id, UpdateProductCategoryDTO updateDto);
        Task<bool> DeleteCategoryAsync(long id);
    }
}

