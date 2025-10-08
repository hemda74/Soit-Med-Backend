using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IWeeklyPlanService
    {
        // Weekly Plan operations
        Task<WeeklyPlanResponseDto?> CreateWeeklyPlanAsync(CreateWeeklyPlanDto createDto, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanResponseDto?> UpdateWeeklyPlanAsync(int id, UpdateWeeklyPlanDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteWeeklyPlanAsync(int id, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanResponseDto?> GetWeeklyPlanByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansAsync(FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default);
        Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansForEmployeeAsync(string employeeId, FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default);
        Task<bool> CanAccessWeeklyPlanAsync(int planId, string userId, bool isManager, CancellationToken cancellationToken = default);
        
        // Task operations
        Task<WeeklyPlanTaskResponseDto?> AddTaskToWeeklyPlanAsync(int weeklyPlanId, AddTaskToWeeklyPlanDto taskDto, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTaskResponseDto?> UpdateTaskAsync(int weeklyPlanId, int taskId, UpdateWeeklyPlanTaskDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteTaskAsync(int weeklyPlanId, int taskId, string employeeId, CancellationToken cancellationToken = default);
        
        // Daily Progress operations
        Task<DailyProgressResponseDto?> AddDailyProgressAsync(int weeklyPlanId, CreateDailyProgressDto progressDto, string employeeId, CancellationToken cancellationToken = default);
        Task<DailyProgressResponseDto?> UpdateDailyProgressAsync(int weeklyPlanId, int progressId, UpdateDailyProgressDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteDailyProgressAsync(int weeklyPlanId, int progressId, string employeeId, CancellationToken cancellationToken = default);
        
        // Manager Review operations
        Task<WeeklyPlanResponseDto?> ReviewWeeklyPlanAsync(int id, ReviewWeeklyPlanDto reviewDto, CancellationToken cancellationToken = default);
    }
}





