using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IWeeklyPlanService
    {
        // Weekly Plan operations
        Task<WeeklyPlanResponseDto?> CreateWeeklyPlanAsync(CreateWeeklyPlanDto createDto, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanResponseDto?> UpdateWeeklyPlanAsync(long id, UpdateWeeklyPlanDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteWeeklyPlanAsync(long id, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanResponseDto?> GetWeeklyPlanByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansAsync(FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default);
        Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansForEmployeeAsync(string employeeId, FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default);
        Task<bool> CanAccessWeeklyPlanAsync(long planId, string userId, bool isManager, CancellationToken cancellationToken = default);
        
        // Task operations
        Task<WeeklyPlanTaskResponseDto?> AddTaskToWeeklyPlanAsync(long weeklyPlanId, AddTaskToWeeklyPlanDto taskDto, string employeeId, CancellationToken cancellationToken = default);
        Task<WeeklyPlanTaskResponseDto?> UpdateTaskAsync(long weeklyPlanId, int taskId, UpdateWeeklyPlanTaskDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteTaskAsync(long weeklyPlanId, int taskId, string employeeId, CancellationToken cancellationToken = default);
        
        // Daily Progress operations
        Task<DailyProgressResponseDto?> AddDailyProgressAsync(long weeklyPlanId, CreateDailyProgressDto progressDto, string employeeId, CancellationToken cancellationToken = default);
        Task<DailyProgressResponseDto?> UpdateDailyProgressAsync(long weeklyPlanId, int progressId, UpdateDailyProgressDto updateDto, string employeeId, CancellationToken cancellationToken = default);
        Task<bool> DeleteDailyProgressAsync(long weeklyPlanId, int progressId, string employeeId, CancellationToken cancellationToken = default);
        
        // Manager Review operations
        Task<WeeklyPlanResponseDto?> ReviewWeeklyPlanAsync(long id, ReviewWeeklyPlanDto reviewDto, CancellationToken cancellationToken = default);
        
        // Current week plan
        Task<WeeklyPlanResponseDto?> GetCurrentWeeklyPlanAsync(string userId);
    }
}










