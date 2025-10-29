using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
<<<<<<< HEAD
    public class WeeklyPlanTaskRepository : IWeeklyPlanTaskRepository
    {
        private readonly Context _context;

        public WeeklyPlanTaskRepository(Context context)
        {
            _context = context;
        }

        public async Task<WeeklyPlanTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<WeeklyPlanTask>> GetByWeeklyPlanIdAsync(int weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.WeeklyPlanId == weeklyPlanId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<WeeklyPlanTask> CreateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default)
        {
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            task.IsActive = true;

            _context.WeeklyPlanTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<WeeklyPlanTask> UpdateAsync(WeeklyPlanTask task, CancellationToken cancellationToken = default)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyPlanTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var task = await _context.WeeklyPlanTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);

            if (task == null)
                return false;

            // Soft delete
            task.IsActive = false;
            task.UpdatedAt = DateTime.UtcNow;
            _context.WeeklyPlanTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .AnyAsync(t => t.Id == id && t.IsActive, cancellationToken);
        }

        public async Task<bool> BelongsToWeeklyPlanAsync(int taskId, int weeklyPlanId, CancellationToken cancellationToken = default)
        {
            return await _context.WeeklyPlanTasks
                .AnyAsync(t => t.Id == taskId && t.WeeklyPlanId == weeklyPlanId && t.IsActive, cancellationToken);
=======
    /// <summary>
    /// Repository for managing weekly plan tasks
    /// </summary>
    public class WeeklyPlanTaskRepository : BaseRepository<WeeklyPlanTask>, IWeeklyPlanTaskRepository
    {
        public WeeklyPlanTaskRepository(Context context) : base(context)
        {
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByWeeklyPlanIdAsync(long weeklyPlanId)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.WeeklyPlanId == weeklyPlanId)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Where(t => t.WeeklyPlan.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(t => t.PlannedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.PlannedDate <= endDate.Value);

            return await query
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetOverdueTasksAsync(string employeeId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Where(t => t.WeeklyPlan.EmployeeId == employeeId &&
                           t.PlannedDate < today &&
                           t.Status != "Completed" &&
                           t.Status != "Cancelled")
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByStatusAsync(string status)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.Status == status)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByPriorityAsync(string priority)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.Priority == priority)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByClientIdAsync(long clientId)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByTaskTypeAsync(string taskType)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.TaskType == taskType)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByClientClassificationAsync(string classification)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.ClientClassification == classification)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<WeeklyPlanTask?> GetTaskWithDetailsAsync(long taskId)
        {
            return await _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Include(t => t.Client)
                .Include(t => t.Progresses)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.PlannedDate >= startDate && t.PlannedDate <= endDate)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<int> GetTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Where(t => t.WeeklyPlan.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(t => t.PlannedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.PlannedDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<int> GetCompletedTaskCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Where(t => t.WeeklyPlan.EmployeeId == employeeId && t.Status == "Completed");

            if (startDate.HasValue)
                query = query.Where(t => t.PlannedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.PlannedDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksNeedingFollowUpAsync(string employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var followUpDate = today.AddDays(1); // Tasks planned for tomorrow

            return await _context.WeeklyPlanTasks
                .Include(t => t.WeeklyPlan)
                .Where(t => t.WeeklyPlan.EmployeeId == employeeId &&
                           t.PlannedDate <= followUpDate &&
                           t.Status == "Planned")
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByClientStatusAsync(string clientStatus)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.ClientStatus == clientStatus)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
        }

        public async Task<List<WeeklyPlanTask>> GetTasksByPlaceTypeAsync(string placeType)
        {
            return await _context.WeeklyPlanTasks
                .Where(t => t.PlaceType == placeType)
                .OrderBy(t => t.PlannedDate)
                .ToListAsync();
>>>>>>> dev
        }
    }
}



