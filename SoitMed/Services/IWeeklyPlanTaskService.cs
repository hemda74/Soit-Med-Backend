using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Service interface for weekly plan task business operations
    /// </summary>
    public interface IWeeklyPlanTaskService
    {
        /// <summary>
        /// Creates a new task in a weekly plan
        /// </summary>
        Task<WeeklyPlanTaskDetailResponseDTO> CreateTaskAsync(CreateWeeklyPlanTaskDTO createDto, string userId);

        /// <summary>
        /// Creates multiple tasks in a weekly plan (for batch creation during plan creation)
        /// </summary>
        Task<List<WeeklyPlanTaskDetailResponseDTO>> CreateTasksAsync(long weeklyPlanId, List<CreateWeeklyPlanTaskDTO> tasksDto, string userId);

        /// <summary>
        /// Gets a specific task by ID
        /// </summary>
        Task<WeeklyPlanTaskDetailResponseDTO?> GetTaskAsync(long taskId, string userId, string userRole);

        /// <summary>
        /// Gets all tasks for a specific weekly plan
        /// </summary>
        Task<List<WeeklyPlanTaskDetailResponseDTO>> GetTasksByPlanAsync(long weeklyPlanId, string userId, string userRole);

        /// <summary>
        /// Updates an existing task
        /// </summary>
        Task<WeeklyPlanTaskDetailResponseDTO?> UpdateTaskAsync(long taskId, UpdateWeeklyPlanTaskDTO updateDto, string userId);

        /// <summary>
        /// Deletes a task (soft delete or hard delete based on business rules)
        /// </summary>
        Task<bool> DeleteTaskAsync(long taskId, string userId);

        /// <summary>
        /// Validates if a user can modify a task
        /// </summary>
        Task<bool> CanModifyTaskAsync(long taskId, string userId);

        /// <summary>
        /// Validates task creation data
        /// </summary>
        Task<bool> ValidateTaskAsync(CreateWeeklyPlanTaskDTO createDto);
    }
}



