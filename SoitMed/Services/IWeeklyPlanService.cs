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
        Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansAsync(string userId, int page, int pageSize);

        /// <summary>
        /// Gets a specific weekly plan by ID
        /// </summary>
        Task<WeeklyPlanResponseDTO?> GetWeeklyPlanAsync(long id, string userId);

        /// <summary>
        /// Updates a weekly plan
        /// </summary>
        Task<WeeklyPlanResponseDTO?> UpdateWeeklyPlanAsync(long id, UpdateWeeklyPlanDTO updateDto, string userId);

        /// <summary>
        /// Submits a weekly plan for approval
        /// </summary>
        Task<bool> SubmitWeeklyPlanAsync(long id, string userId);

        /// <summary>
        /// Approves a weekly plan
        /// </summary>
        Task<bool> ApproveWeeklyPlanAsync(long id, ApprovePlanDTO approveDto, string userId);

        /// <summary>
        /// Rejects a weekly plan
        /// </summary>
        Task<bool> RejectWeeklyPlanAsync(long id, RejectPlanDTO rejectDto, string userId);

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
