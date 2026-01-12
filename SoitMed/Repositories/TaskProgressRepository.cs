using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository for managing task progress
    /// </summary>
    public class TaskProgressRepository : BaseRepository<TaskProgress>, ITaskProgressRepository
    {
        public TaskProgressRepository(Context context) : base(context)
        {
        }

        public async Task<List<TaskProgress>> GetProgressesByTaskIdAsync(int taskId)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.TaskId == taskId)
                .OrderBy(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByClientIdAsync(long clientId)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.ClientId == clientId.ToString())
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(tp => tp.ProgressDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(tp => tp.ProgressDate <= endDate.Value);

            return await query
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetAllProgressesAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TaskProgresses
                .AsNoTracking()
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(tp => tp.ProgressDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(tp => tp.ProgressDate <= endDate.Value);

            return await query
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.ProgressDate >= startDate && tp.ProgressDate <= endDate)
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByProgressTypeAsync(string progressType)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.ProgressType == progressType)
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByVisitResultAsync(string visitResult)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.VisitResult == visitResult)
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByNextStepAsync(string nextStep)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.NextStep == nextStep)
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetOverdueFollowUpsAsync(string employeeId, DateTime currentDate)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.EmployeeId == employeeId &&
                           tp.NextFollowUpDate.HasValue &&
                           tp.NextFollowUpDate.Value.Date <= currentDate.Date)
                .OrderBy(tp => tp.NextFollowUpDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesWithOfferRequestsAsync()
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => !string.IsNullOrEmpty(tp.OfferRequestId))
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesWithDealsAsync()
        {
            // Since TaskProgress doesn't have DealId, we'll return all progresses
            // In a real implementation, you might want to join with a deals table
            return await _context.TaskProgresses
                .AsNoTracking()
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }

        public async Task<TaskProgress?> GetProgressWithDetailsAsync(long progressId)
        {
            return await _context.TaskProgresses
                .Include(tp => tp.Task)
                .Include(tp => tp.Client)
                .Include(tp => tp.Employee)
                .Include(tp => tp.OfferRequest)
                .FirstOrDefaultAsync(tp => tp.Id == progressId.ToString());
        }

       

        public async Task<int> GetProgressCountByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TaskProgresses
                .AsNoTracking()
                .Where(tp => tp.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(tp => tp.ProgressDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(tp => tp.ProgressDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<int> GetProgressCountByClientAsync(long clientId)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .CountAsync(tp => tp.ClientId == clientId.ToString());
        }

      

        public async Task<List<TaskProgress>> GetRecentProgressesAsync(int count)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .OrderByDescending(tp => tp.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<TaskProgress>> GetProgressesByTaskStatusAsync(string taskStatus)
        {
            return await _context.TaskProgresses
                .AsNoTracking()
                .Include(tp => tp.Task)
                // Status removed from Task - now tracked through progress existence
                .OrderByDescending(tp => tp.ProgressDate)
                .ToListAsync();
        }
    }
}
