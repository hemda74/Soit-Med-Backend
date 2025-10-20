using SoitMed.DTO;
using SoitMed.Models;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for weekly plan item business operations
    /// </summary>
    public interface IWeeklyPlanItemService
    {
        /// <summary>
        /// Creates a new weekly plan item
        /// </summary>
        Task<WeeklyPlanItemResponseDTO> CreatePlanItemAsync(CreateWeeklyPlanItemDTO createDto, string userId);

        /// <summary>
        /// Gets plan items for a specific plan
        /// </summary>
        Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetPlanItemsAsync(long planId, string userId);

        /// <summary>
        /// Gets a specific plan item by ID
        /// </summary>
        Task<WeeklyPlanItemResponseDTO?> GetPlanItemAsync(long id, string userId);

        /// <summary>
        /// Updates a plan item
        /// </summary>
        Task<WeeklyPlanItemResponseDTO?> UpdatePlanItemAsync(long id, UpdateWeeklyPlanItemDTO updateDto, string userId);

        /// <summary>
        /// Completes a plan item
        /// </summary>
        Task<bool> CompletePlanItemAsync(long id, CompletePlanItemDTO completeDto, string userId);

        /// <summary>
        /// Cancels a plan item
        /// </summary>
        Task<bool> CancelPlanItemAsync(long id, CancelPlanItemDTO cancelDto, string userId);

        /// <summary>
        /// Postpones a plan item
        /// </summary>
        Task<bool> PostponePlanItemAsync(long id, PostponePlanItemDTO postponeDto, string userId);

        /// <summary>
        /// Gets overdue items for a user
        /// </summary>
        Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetOverdueItemsAsync(string userId);

        /// <summary>
        /// Gets upcoming items for a user
        /// </summary>
        Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetUpcomingItemsAsync(string userId, int days = 7);
    }
}
