using SoitMed.DTO;
using SoitMed.Models;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for weekly plan business operations
    /// </summary>
    public interface IWeeklyPlanService
    {
        /// <summary>
        /// Creates a new weekly plan
        /// </summary>
        Task<WeeklyPlanResponseDTO> CreateWeeklyPlanAsync(CreateWeeklyPlanDTO createDto, string userId);

        /// <summary>
        /// Gets weekly plans for a user with pagination
        /// </summary>
        Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansAsync(string userId, string userRole, int page, int pageSize);

        /// <summary>
        /// Gets weekly plans with filters for managers/admins
        /// </summary>
        Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansWithFiltersAsync(WeeklyPlanFiltersDTO filters, string userRole, int page, int pageSize);

        /// <summary>
        /// Gets a specific weekly plan by ID
        /// </summary>
        Task<WeeklyPlanResponseDTO?> GetWeeklyPlanAsync(long id, string userId, string userRole);

        /// <summary>
        /// Updates a weekly plan
        /// </summary>
        Task<WeeklyPlanResponseDTO?> UpdateWeeklyPlanAsync(long id, UpdateWeeklyPlanDTO updateDto, string userId);

        /// <summary>
        /// Submits a weekly plan for approval
        /// </summary>
        Task<bool> SubmitWeeklyPlanAsync(long id, string userId);

        /// <summary>
        /// Reviews a weekly plan (manager rating and comment)
        /// </summary>
        Task<bool> ReviewWeeklyPlanAsync(long id, ReviewWeeklyPlanDTO reviewDto, string userId);

        /// <summary>
        /// Gets the current week's plan for a user
        /// </summary>
        Task<WeeklyPlanResponseDTO?> GetCurrentWeeklyPlanAsync(string userId);

        /// <summary>
        /// Checks if a user has a plan for a specific week
        /// </summary>
        Task<bool> HasPlanForWeekAsync(string userId, DateTime weekStartDate);
    }
}
