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
                PlanTitle = createDto.PlanTitle,
                PlanDescription = createDto.PlanDescription,
                Status = "Draft"
            };

            await UnitOfWork.WeeklyPlans.CreateAsync(plan);
            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                PlanTitle = plan.PlanTitle,
                PlanDescription = plan.PlanDescription,
                Status = plan.Status,
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
                PlanTitle = p.PlanTitle,
                PlanDescription = p.PlanDescription,
                Status = p.Status,
                SubmittedAt = p.SubmittedAt,
                ApprovedAt = p.ApprovedAt,
                RejectedAt = p.RejectedAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PlanItems = p.PlanItems.Select(pi => new WeeklyPlanItemResponseDTO
                {
                    Id = pi.Id,
                    ClientName = pi.ClientName,
                    ClientType = pi.ClientType,
                    PlannedVisitDate = pi.PlannedVisitDate,
                    VisitPurpose = pi.VisitPurpose,
                    Priority = pi.Priority,
                    Status = pi.Status
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
                PlanTitle = plan.PlanTitle,
                PlanDescription = plan.PlanDescription,
                Status = plan.Status,
                ApprovalNotes = plan.ApprovalNotes,
                RejectionReason = plan.RejectionReason,
                SubmittedAt = plan.SubmittedAt,
                ApprovedAt = plan.ApprovedAt,
                RejectedAt = plan.RejectedAt,
                ApprovedBy = plan.ApprovedBy,
                RejectedBy = plan.RejectedBy,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                PlanItems = plan.PlanItems.Select(pi => new WeeklyPlanItemResponseDTO
                {
                    Id = pi.Id,
                    WeeklyPlanId = pi.WeeklyPlanId,
                    ClientId = pi.ClientId,
                    ClientName = pi.ClientName,
                    ClientType = pi.ClientType,
                    ClientSpecialization = pi.ClientSpecialization,
                    PlannedVisitDate = pi.PlannedVisitDate,
                    VisitPurpose = pi.VisitPurpose,
                    Priority = pi.Priority,
                    Status = pi.Status,
                    IsNewClient = pi.IsNewClient
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

            // Only allow updates if status is Draft
            if (plan.Status != "Draft")
            {
                throw new InvalidOperationException("لا يمكن تعديل الخطة بعد إرسالها");
            }

            if (!string.IsNullOrEmpty(updateDto.PlanTitle))
                plan.PlanTitle = updateDto.PlanTitle;
            if (updateDto.PlanDescription != null)
                plan.PlanDescription = updateDto.PlanDescription;

            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanResponseDTO
            {
                Id = plan.Id,
                EmployeeId = plan.EmployeeId,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                PlanTitle = plan.PlanTitle,
                PlanDescription = plan.PlanDescription,
                Status = plan.Status,
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

            if (plan.Status != "Draft")
            {
                throw new InvalidOperationException("تم إرسال هذه الخطة بالفعل");
            }

            plan.Status = "Submitted";
            plan.SubmittedAt = DateTime.UtcNow;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveWeeklyPlanAsync(long id, ApprovePlanDTO approveDto, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null)
            {
                return false;
            }

            if (plan.Status != "Submitted")
            {
                throw new InvalidOperationException("لا يمكن الموافقة على هذه الخطة");
            }

            plan.Status = "Approved";
            plan.ApprovedAt = DateTime.UtcNow;
            plan.ApprovedBy = userId;
            plan.ApprovalNotes = approveDto.Notes;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectWeeklyPlanAsync(long id, RejectPlanDTO rejectDto, string userId)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id);
            if (plan == null)
            {
                return false;
            }

            if (plan.Status != "Submitted")
            {
                throw new InvalidOperationException("لا يمكن رفض هذه الخطة");
            }

            plan.Status = "Rejected";
            plan.RejectedAt = DateTime.UtcNow;
            plan.RejectedBy = userId;
            plan.RejectionReason = rejectDto.Reason;

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
                PlanTitle = plan.PlanTitle,
                Status = plan.Status,
                PlanItems = plan.PlanItems.Select(pi => new WeeklyPlanItemResponseDTO
                {
                    Id = pi.Id,
                    ClientName = pi.ClientName,
                    PlannedVisitDate = pi.PlannedVisitDate,
                    VisitPurpose = pi.VisitPurpose,
                    Priority = pi.Priority,
                    Status = pi.Status
                }).ToList()
            };
        }

        public async Task<bool> HasPlanForWeekAsync(string userId, DateTime weekStartDate)
        {
            return await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, weekStartDate);
        }
    }
}
