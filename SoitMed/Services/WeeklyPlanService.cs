using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for weekly plan business operations
    /// </summary>
    public class WeeklyPlanService : BaseService, IWeeklyPlanService
    {
        public WeeklyPlanService(IUnitOfWork unitOfWork, ILogger<WeeklyPlanService> logger) 
            : base(unitOfWork, logger)
        {
        }

        public async Task<WeeklyPlanResponseDTO> CreateWeeklyPlanAsync(CreateWeeklyPlanDTO createDto, string userId)
        {
            // Check if plan already exists for this week
            var existingPlan = await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, createDto.WeekStartDate);
            if (existingPlan)
            {
                throw new InvalidOperationException("يوجد خطة أسبوعية بالفعل لهذا الأسبوع");
            }

            var plan = new Models.WeeklyPlan
            {
                EmployeeId = userId,
                WeekStartDate = createDto.WeekStartDate,
                WeekEndDate = createDto.WeekEndDate,
                Title = createDto.Title,
                Description = createDto.Description,
                IsActive = true
            };

            await UnitOfWork.WeeklyPlans.CreateAsync(plan);
            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                Title = plan.Title,
                Description = plan.Description,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt
            };
        }

        public async Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansAsync(string userId, int page, int pageSize)
        {
            var plans = await UnitOfWork.WeeklyPlans.GetEmployeePlansAsync(userId, page, pageSize);
            var totalCount = await UnitOfWork.WeeklyPlans.CountAsync(p => p.EmployeeId == userId);

            var planDtos = plans.Select(p => new WeeklyPlanResponseDTO
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                WeekStartDate = p.WeekStartDate,
                WeekEndDate = p.WeekEndDate,
                Title = p.Title,
                Description = p.Description,
                IsActive = p.IsActive,
                Rating = p.Rating,
                ManagerComment = p.ManagerComment,
                ManagerReviewedAt = p.ManagerReviewedAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Tasks = p.Tasks.Select(t => new WeeklyPlanTaskResponseDTO
                {
                    Id = t.Id,
                    TaskType = t.TaskType,
                    ClientId = t.ClientId,
                    ClientName = t.ClientName,
                    ClientStatus = t.ClientStatus,
                    ClientClassification = t.ClientClassification,
                    PlannedDate = t.PlannedDate,
                    PlannedTime = t.PlannedTime,
                    Purpose = t.Purpose,
                    Priority = t.Priority,
                    Status = t.Status,
                    ProgressCount = t.Progresses.Count
                }).ToList()
            });

            return (planDtos, totalCount);
        }

        public async Task<WeeklyPlanResponseDTO?> GetWeeklyPlanAsync(long id, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null || plan.EmployeeId != userId)
            {
                return null;
            }

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                Title = plan.Title,
                Description = plan.Description,
                IsActive = plan.IsActive,
                Rating = plan.Rating,
                ManagerComment = plan.ManagerComment,
                ManagerReviewedAt = plan.ManagerReviewedAt,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                Tasks = plan.Tasks.Select(t => new WeeklyPlanTaskResponseDTO
                {
                    Id = t.Id,
                    TaskType = t.TaskType,
                    ClientId = t.ClientId,
                    ClientName = t.ClientName,
                    ClientStatus = t.ClientStatus,
                    ClientClassification = t.ClientClassification,
                    PlannedDate = t.PlannedDate,
                    PlannedTime = t.PlannedTime,
                    Purpose = t.Purpose,
                    Priority = t.Priority,
                    Status = t.Status,
                    ProgressCount = t.Progresses.Count
                }).ToList()
            };
        }

        public async Task<WeeklyPlanResponseDTO?> UpdateWeeklyPlanAsync(long id, UpdateWeeklyPlanDTO updateDto, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null || plan.EmployeeId != userId)
            {
                return null;
            }

            // Only allow updates if plan is active
            if (!plan.IsActive)
            {
                throw new InvalidOperationException("لا يمكن تعديل الخطة غير النشطة");
            }

            if (!string.IsNullOrEmpty(updateDto.Title))
                plan.Title = updateDto.Title;
            if (updateDto.Description != null)
                plan.Description = updateDto.Description;

            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                Title = plan.Title,
                Description = plan.Description,
                IsActive = plan.IsActive,
                UpdatedAt = plan.UpdatedAt
            };
        }

        public async Task<bool> SubmitWeeklyPlanAsync(long id, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null || plan.EmployeeId != userId)
            {
                return false;
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException("لا يمكن إرسال خطة غير نشطة");
            }

            // Plan is already active, no need to change status
            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReviewWeeklyPlanAsync(long id, ReviewWeeklyPlanDTO reviewDto, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null)
            {
                return false;
            }

            if (!plan.IsActive)
            {
                throw new InvalidOperationException("لا يمكن مراجعة خطة غير نشطة");
            }

            plan.Rating = reviewDto.Rating;
            plan.ManagerComment = reviewDto.Comment;
            plan.ManagerReviewedAt = DateTime.UtcNow;
            plan.ReviewedBy = userId;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<WeeklyPlanResponseDTO?> GetCurrentWeeklyPlanAsync(string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetCurrentWeekPlanAsync(userId);
            if (plan == null)
            {
                return null;
            }

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                Title = plan.Title,
                Description = plan.Description,
                IsActive = plan.IsActive,
                Rating = plan.Rating,
                ManagerComment = plan.ManagerComment,
                ManagerReviewedAt = plan.ManagerReviewedAt,
                Tasks = plan.Tasks.Select(t => new WeeklyPlanTaskResponseDTO
                {
                    Id = t.Id,
                    TaskType = t.TaskType,
                    ClientId = t.ClientId,
                    ClientName = t.ClientName,
                    ClientStatus = t.ClientStatus,
                    ClientClassification = t.ClientClassification,
                    PlannedDate = t.PlannedDate,
                    PlannedTime = t.PlannedTime,
                    Purpose = t.Purpose,
                    Priority = t.Priority,
                    Status = t.Status,
                    ProgressCount = t.Progresses.Count
                }).ToList()
            };
        }

        public async Task<bool> HasPlanForWeekAsync(string userId, DateTime weekStartDate)
        {
            return await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, weekStartDate);
        }
    }
}
