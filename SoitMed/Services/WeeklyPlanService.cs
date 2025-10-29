using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Linq.Expressions;

namespace SoitMed.Services
{
    public class WeeklyPlanService : IWeeklyPlanService
    {
        private readonly IWeeklyPlanRepository _weeklyPlanRepository;
        private readonly IWeeklyPlanTaskRepository _taskRepository;
        private readonly IDailyProgressRepository _progressRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WeeklyPlanService(
            IWeeklyPlanRepository weeklyPlanRepository,
            IWeeklyPlanTaskRepository taskRepository,
            IDailyProgressRepository progressRepository,
            UserManager<ApplicationUser> userManager)
        {
            _weeklyPlanRepository = weeklyPlanRepository;
            _taskRepository = taskRepository;
            _progressRepository = progressRepository;
            _userManager = userManager;
        }

        #region Weekly Plan Operations

        public async Task<WeeklyPlanResponseDto?> CreateWeeklyPlanAsync(CreateWeeklyPlanDto createDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify employee exists and has correct role
            if (!await ServiceHelper.ValidateUserRoleAsync(employeeId, "Salesman", _userManager))
                return null;

            // Check if employee already has a plan for this week
            if (await _weeklyPlanRepository.HasPlanForWeekAsync(employeeId, createDto.WeekStartDate, null, cancellationToken))
                return null;

            // Validate week dates
            if (createDto.WeekEndDate <= createDto.WeekStartDate)
                return null;

            var weeklyPlan = new WeeklyPlan
            {
                Title = createDto.Title,
                Description = createDto.Description,
                WeekStartDate = createDto.WeekStartDate,
                WeekEndDate = createDto.WeekEndDate,
                EmployeeId = employeeId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdPlan = await _weeklyPlanRepository.CreateAsync(weeklyPlan, cancellationToken);

            // Add tasks if provided
            if (createDto.Tasks != null && createDto.Tasks.Any())
            {
                foreach (var taskDto in createDto.Tasks)
                {
                    var task = new WeeklyPlanTask
                    {
                        WeeklyPlanId = createdPlan.Id,
                        Title = taskDto.Title,
                        Description = taskDto.Description,
                        DisplayOrder = taskDto.DisplayOrder,
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _taskRepository.CreateAsync(task, cancellationToken);
                }
            }

            // Reload plan with all details
            var planWithDetails = await _weeklyPlanRepository.GetByIdWithDetailsAsync(createdPlan.Id, cancellationToken);
            return MapToResponseDto(planWithDetails!);
        }

        public async Task<WeeklyPlanResponseDto?> UpdateWeeklyPlanAsync(int id, UpdateWeeklyPlanDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var existingPlan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(id, employeeId, cancellationToken);
            if (existingPlan == null)
                return null;

            existingPlan.Title = updateDto.Title;
            existingPlan.Description = updateDto.Description;
            existingPlan.UpdatedAt = DateTime.UtcNow;

            var updatedPlan = await _weeklyPlanRepository.UpdateAsync(existingPlan, cancellationToken);
            
            // Reload with details
            var planWithDetails = await _weeklyPlanRepository.GetByIdWithDetailsAsync(updatedPlan.Id, cancellationToken);
            return MapToResponseDto(planWithDetails!);
        }

        public async Task<bool> DeleteWeeklyPlanAsync(int id, string employeeId, CancellationToken cancellationToken = default)
        {
            var exists = await _weeklyPlanRepository.ExistsForEmployeeAsync(id, employeeId, cancellationToken);
            if (!exists)
                return false;

            return await _weeklyPlanRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<WeeklyPlanResponseDto?> GetWeeklyPlanByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var plan = await _weeklyPlanRepository.GetByIdWithDetailsAsync(id, cancellationToken);
            return plan != null ? MapToResponseDto(plan) : null;
        }

        public async Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansAsync(FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default)
        {
            Expression<Func<WeeklyPlan, bool>>? predicate = null;

            if (!string.IsNullOrEmpty(filterDto.EmployeeId))
            {
                // Check if EmployeeId is a GUID or a username
                var employeeIdToUse = filterDto.EmployeeId;
                
                // If it's not a valid GUID, try to find user by username
                if (!Guid.TryParse(filterDto.EmployeeId, out _))
                {
                    var user = await _userManager.FindByNameAsync(filterDto.EmployeeId);
                    if (user != null)
                    {
                        employeeIdToUse = user.Id;
                    }
                }
                
                predicate = wp => wp.EmployeeId == employeeIdToUse;
            }

            if (filterDto.StartDate.HasValue)
            {
                Expression<Func<WeeklyPlan, bool>> startDatePredicate = wp => wp.WeekStartDate >= filterDto.StartDate.Value;
                predicate = predicate == null ? startDatePredicate : CombineExpressions(predicate, startDatePredicate);
            }

            if (filterDto.EndDate.HasValue)
            {
                Expression<Func<WeeklyPlan, bool>> endDatePredicate = wp => wp.WeekEndDate <= filterDto.EndDate.Value;
                predicate = predicate == null ? endDatePredicate : CombineExpressions(predicate, endDatePredicate);
            }

            if (filterDto.HasManagerReview.HasValue)
            {
                Expression<Func<WeeklyPlan, bool>> reviewPredicate = filterDto.HasManagerReview.Value
                    ? wp => wp.ManagerReviewedAt != null
                    : wp => wp.ManagerReviewedAt == null;
                predicate = predicate == null ? reviewPredicate : CombineExpressions(predicate, reviewPredicate);
            }

            if (filterDto.MinRating.HasValue)
            {
                Expression<Func<WeeklyPlan, bool>> minRatingPredicate = wp => wp.Rating >= filterDto.MinRating.Value;
                predicate = predicate == null ? minRatingPredicate : CombineExpressions(predicate, minRatingPredicate);
            }

            if (filterDto.MaxRating.HasValue)
            {
                Expression<Func<WeeklyPlan, bool>> maxRatingPredicate = wp => wp.Rating <= filterDto.MaxRating.Value;
                predicate = predicate == null ? maxRatingPredicate : CombineExpressions(predicate, maxRatingPredicate);
            }

            var (plans, totalCount) = await _weeklyPlanRepository.GetPaginatedAsync(
                predicate,
                filterDto.Page,
                filterDto.PageSize,
                cancellationToken);

            var totalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize);

            return new PaginatedWeeklyPlansResponseDto
            {
                Data = plans.Select(MapToResponseDto).ToList(),
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = totalPages,
                HasNextPage = filterDto.Page < totalPages,
                HasPreviousPage = filterDto.Page > 1
            };
        }

        public async Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansForEmployeeAsync(string employeeId, FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default)
        {
            // Override employee filter to ensure employee can only see their own plans
            filterDto.EmployeeId = employeeId;
            return await GetWeeklyPlansAsync(filterDto, cancellationToken);
        }

        public async Task<bool> CanAccessWeeklyPlanAsync(int planId, string userId, bool isManager, CancellationToken cancellationToken = default)
        {
            var plan = await _weeklyPlanRepository.GetByIdAsync(planId, cancellationToken);
            if (plan == null)
                return false;

            // Managers can access all plans, employees can only access their own
            return isManager || plan.EmployeeId == userId;
        }

        #endregion

        #region Task Operations

        public async Task<WeeklyPlanTaskResponseDto?> AddTaskToWeeklyPlanAsync(int weeklyPlanId, AddTaskToWeeklyPlanDto taskDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return null;

            var task = new WeeklyPlanTask
            {
                WeeklyPlanId = weeklyPlanId,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DisplayOrder = taskDto.DisplayOrder,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdTask = await _taskRepository.CreateAsync(task, cancellationToken);
            return MapTaskToResponseDto(createdTask);
        }

        public async Task<WeeklyPlanTaskResponseDto?> UpdateTaskAsync(int weeklyPlanId, int taskId, UpdateWeeklyPlanTaskDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return null;

            // Verify the task belongs to the plan
            if (!await _taskRepository.BelongsToWeeklyPlanAsync(taskId, weeklyPlanId, cancellationToken))
                return null;

            var task = await _taskRepository.GetByIdAsync(taskId, cancellationToken);
            if (task == null)
                return null;

            task.Title = updateDto.Title;
            task.Description = updateDto.Description;
            task.IsCompleted = updateDto.IsCompleted;
            task.DisplayOrder = updateDto.DisplayOrder;
            task.UpdatedAt = DateTime.UtcNow;

            var updatedTask = await _taskRepository.UpdateAsync(task, cancellationToken);
            return MapTaskToResponseDto(updatedTask);
        }

        public async Task<bool> DeleteTaskAsync(int weeklyPlanId, int taskId, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return false;

            // Verify the task belongs to the plan
            if (!await _taskRepository.BelongsToWeeklyPlanAsync(taskId, weeklyPlanId, cancellationToken))
                return false;

            return await _taskRepository.DeleteAsync(taskId, cancellationToken);
        }

        #endregion

        #region Daily Progress Operations

        public async Task<DailyProgressResponseDto?> AddDailyProgressAsync(int weeklyPlanId, CreateDailyProgressDto progressDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return null;

            // Check if progress already exists for this date
            var existingProgress = await _progressRepository.GetByWeeklyPlanAndDateAsync(weeklyPlanId, progressDto.ProgressDate, cancellationToken);
            if (existingProgress != null)
                return null; // Already has progress for this date

            // Validate that progress date is within the week
            if (progressDto.ProgressDate < plan.WeekStartDate || progressDto.ProgressDate > plan.WeekEndDate)
                return null;

            var progress = new DailyProgress
            {
                WeeklyPlanId = weeklyPlanId,
                ProgressDate = progressDto.ProgressDate,
                Notes = progressDto.Notes,
                TasksWorkedOn = progressDto.TasksWorkedOn != null && progressDto.TasksWorkedOn.Any()
                    ? string.Join(",", progressDto.TasksWorkedOn)
                    : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdProgress = await _progressRepository.CreateAsync(progress, cancellationToken);
            return MapProgressToResponseDto(createdProgress);
        }

        public async Task<DailyProgressResponseDto?> UpdateDailyProgressAsync(int weeklyPlanId, int progressId, UpdateDailyProgressDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return null;

            // Verify the progress belongs to the plan
            if (!await _progressRepository.BelongsToWeeklyPlanAsync(progressId, weeklyPlanId, cancellationToken))
                return null;

            var progress = await _progressRepository.GetByIdAsync(progressId, cancellationToken);
            if (progress == null)
                return null;

            progress.Notes = updateDto.Notes;
            progress.TasksWorkedOn = updateDto.TasksWorkedOn != null && updateDto.TasksWorkedOn.Any()
                ? string.Join(",", updateDto.TasksWorkedOn)
                : null;
            progress.UpdatedAt = DateTime.UtcNow;

            var updatedProgress = await _progressRepository.UpdateAsync(progress, cancellationToken);
            return MapProgressToResponseDto(updatedProgress);
        }

        public async Task<bool> DeleteDailyProgressAsync(int weeklyPlanId, int progressId, string employeeId, CancellationToken cancellationToken = default)
        {
            // Verify the plan belongs to the employee
            var plan = await _weeklyPlanRepository.GetByIdAndEmployeeIdAsync(weeklyPlanId, employeeId, cancellationToken);
            if (plan == null)
                return false;

            // Verify the progress belongs to the plan
            if (!await _progressRepository.BelongsToWeeklyPlanAsync(progressId, weeklyPlanId, cancellationToken))
                return false;

            return await _progressRepository.DeleteAsync(progressId, cancellationToken);
        }

        #endregion

        #region Manager Review Operations

        public async Task<WeeklyPlanResponseDto?> ReviewWeeklyPlanAsync(int id, ReviewWeeklyPlanDto reviewDto, CancellationToken cancellationToken = default)
        {
            var plan = await _weeklyPlanRepository.GetByIdWithDetailsAsync(id, cancellationToken);
            if (plan == null)
                return null;

            // Only update if at least one field is provided
            if (reviewDto.Rating.HasValue || !string.IsNullOrEmpty(reviewDto.ManagerComment))
            {
                if (reviewDto.Rating.HasValue)
                    plan.Rating = reviewDto.Rating.Value;

                if (!string.IsNullOrEmpty(reviewDto.ManagerComment))
                    plan.ManagerComment = reviewDto.ManagerComment;

                plan.ManagerReviewedAt = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;

                var updatedPlan = await _weeklyPlanRepository.UpdateAsync(plan, cancellationToken);
                
                // Reload with details
                var planWithDetails = await _weeklyPlanRepository.GetByIdWithDetailsAsync(updatedPlan.Id, cancellationToken);
                return MapToResponseDto(planWithDetails!);
            }

            return MapToResponseDto(plan);
        }

        #endregion

        #region Mapping Methods

        private static WeeklyPlanResponseDto MapToResponseDto(WeeklyPlan plan)
        {
            var totalTasks = plan.Tasks?.Count(t => t.IsActive) ?? 0;
            var completedTasks = plan.Tasks?.Count(t => t.IsActive && t.IsCompleted) ?? 0;
            var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

            return new WeeklyPlanResponseDto
            {
                Id = plan.Id,
                Title = plan.Title,
                Description = plan.Description,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                EmployeeId = plan.EmployeeId,
                EmployeeName = $"{plan.Employee.FirstName} {plan.Employee.LastName}".Trim(),
                Rating = plan.Rating,
                ManagerComment = plan.ManagerComment,
                ManagerReviewedAt = plan.ManagerReviewedAt,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                IsActive = plan.IsActive,
                Tasks = plan.Tasks?.Where(t => t.IsActive)
                    .OrderBy(t => t.DisplayOrder)
                    .ThenBy(t => t.CreatedAt)
                    .Select(MapTaskToResponseDto)
                    .ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                DailyProgresses = plan.DailyProgresses?.Where(dp => dp.IsActive)
                    .OrderBy(dp => dp.ProgressDate)
                    .Select(MapProgressToResponseDto)
                    .ToList() ?? new List<DailyProgressResponseDto>(),
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                CompletionPercentage = Math.Round(completionPercentage, 2)
            };
        }

        private static WeeklyPlanTaskResponseDto MapTaskToResponseDto(WeeklyPlanTask task)
        {
            return new WeeklyPlanTaskResponseDto
            {
                Id = task.Id,
                WeeklyPlanId = task.WeeklyPlanId,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                DisplayOrder = task.DisplayOrder,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        private static DailyProgressResponseDto MapProgressToResponseDto(DailyProgress progress)
        {
            var tasksWorkedOn = !string.IsNullOrEmpty(progress.TasksWorkedOn)
                ? progress.TasksWorkedOn.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
                : new List<int>();

            return new DailyProgressResponseDto
            {
                Id = progress.Id,
                WeeklyPlanId = progress.WeeklyPlanId,
                ProgressDate = progress.ProgressDate,
                Notes = progress.Notes,
                TasksWorkedOn = tasksWorkedOn,
                CreatedAt = progress.CreatedAt,
                UpdatedAt = progress.UpdatedAt
            };
        }

        private static Expression<Func<WeeklyPlan, bool>> CombineExpressions(
            Expression<Func<WeeklyPlan, bool>> left,
            Expression<Func<WeeklyPlan, bool>> right)
        {
            var parameter = Expression.Parameter(typeof(WeeklyPlan), "wp");
            var leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
            var rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
            var combinedBody = Expression.AndAlso(leftBody, rightBody);
            return Expression.Lambda<Func<WeeklyPlan, bool>>(combinedBody, parameter);
        }

        private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }

        #endregion
    }
}



