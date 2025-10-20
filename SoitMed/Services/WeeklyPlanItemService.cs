using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for weekly plan item business operations
    /// </summary>
    public class WeeklyPlanItemService : BaseService, IWeeklyPlanItemService
    {
        public WeeklyPlanItemService(IUnitOfWork unitOfWork, ILogger<WeeklyPlanItemService> logger) 
            : base(unitOfWork, logger)
        {
        }

        public async Task<WeeklyPlanItemResponseDTO> CreatePlanItemAsync(CreateWeeklyPlanItemDTO createDto, string userId)
        {
            // Verify the weekly plan exists and belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(createDto.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لإضافة عناصر لهذه الخطة");
            }

            // If it's a new client, create the client first
            long? clientId = createDto.ClientId;
            if (createDto.IsNewClient && !clientId.HasValue)
            {
                var client = await UnitOfWork.Clients.FindOrCreateClientAsync(
                    createDto.ClientName,
                    createDto.ClientType ?? "Unknown",
                    createDto.ClientSpecialization,
                    userId);
                clientId = client.Id;
            }

            var planItem = new Models.WeeklyPlanItem
            {
                WeeklyPlanId = createDto.WeeklyPlanId,
                ClientId = clientId,
                ClientName = createDto.ClientName,
                ClientType = createDto.ClientType,
                ClientSpecialization = createDto.ClientSpecialization,
                ClientLocation = createDto.ClientLocation,
                ClientPhone = createDto.ClientPhone,
                ClientEmail = createDto.ClientEmail,
                PlannedVisitDate = createDto.PlannedVisitDate,
                PlannedVisitTime = createDto.PlannedVisitTime,
                VisitPurpose = createDto.VisitPurpose,
                VisitNotes = createDto.VisitNotes,
                Priority = createDto.Priority,
                Status = "Planned",
                IsNewClient = createDto.IsNewClient
            };

            await UnitOfWork.WeeklyPlanItems.CreateAsync(planItem);
            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanItemResponseDTO
            {
                Id = planItem.Id,
                WeeklyPlanId = planItem.WeeklyPlanId,
                ClientId = planItem.ClientId,
                ClientName = planItem.ClientName,
                ClientType = planItem.ClientType,
                ClientSpecialization = planItem.ClientSpecialization,
                PlannedVisitDate = planItem.PlannedVisitDate,
                VisitPurpose = planItem.VisitPurpose,
                Priority = planItem.Priority,
                Status = planItem.Status,
                IsNewClient = planItem.IsNewClient,
                CreatedAt = planItem.CreatedAt
            };
        }

        public async Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetPlanItemsAsync(long planId, string userId)
        {
            // Verify the weekly plan exists and belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(planId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لعرض عناصر هذه الخطة");
            }

            var items = await UnitOfWork.WeeklyPlanItems.GetPlanItemsAsync(planId);

            return items.Select(item => new WeeklyPlanItemResponseDTO
            {
                Id = item.Id,
                WeeklyPlanId = item.WeeklyPlanId,
                ClientId = item.ClientId,
                ClientName = item.ClientName,
                ClientType = item.ClientType,
                ClientSpecialization = item.ClientSpecialization,
                ClientLocation = item.ClientLocation,
                ClientPhone = item.ClientPhone,
                ClientEmail = item.ClientEmail,
                PlannedVisitDate = item.PlannedVisitDate,
                PlannedVisitTime = item.PlannedVisitTime,
                VisitPurpose = item.VisitPurpose,
                VisitNotes = item.VisitNotes,
                Priority = item.Priority,
                Status = item.Status,
                IsNewClient = item.IsNewClient,
                ActualVisitDate = item.ActualVisitDate,
                Results = item.Results,
                Feedback = item.Feedback,
                SatisfactionRating = item.SatisfactionRating,
                NextVisitDate = item.NextVisitDate,
                FollowUpNotes = item.FollowUpNotes,
                CancellationReason = item.CancellationReason,
                PostponementReason = item.PostponementReason,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            });
        }

        public async Task<WeeklyPlanItemResponseDTO?> GetPlanItemAsync(long id, string userId)
        {
            var item = await UnitOfWork.WeeklyPlanItems.GetByIdAsync(id);
            if (item == null)
            {
                return null;
            }

            // Verify the weekly plan belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(item.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لعرض هذا العنصر");
            }

            return new WeeklyPlanItemResponseDTO
            {
                Id = item.Id,
                WeeklyPlanId = item.WeeklyPlanId,
                ClientId = item.ClientId,
                ClientName = item.ClientName,
                ClientType = item.ClientType,
                ClientSpecialization = item.ClientSpecialization,
                ClientLocation = item.ClientLocation,
                ClientPhone = item.ClientPhone,
                ClientEmail = item.ClientEmail,
                PlannedVisitDate = item.PlannedVisitDate,
                PlannedVisitTime = item.PlannedVisitTime,
                VisitPurpose = item.VisitPurpose,
                VisitNotes = item.VisitNotes,
                Priority = item.Priority,
                Status = item.Status,
                IsNewClient = item.IsNewClient,
                ActualVisitDate = item.ActualVisitDate,
                Results = item.Results,
                Feedback = item.Feedback,
                SatisfactionRating = item.SatisfactionRating,
                NextVisitDate = item.NextVisitDate,
                FollowUpNotes = item.FollowUpNotes,
                CancellationReason = item.CancellationReason,
                PostponementReason = item.PostponementReason,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<WeeklyPlanItemResponseDTO?> UpdatePlanItemAsync(long id, UpdateWeeklyPlanItemDTO updateDto, string userId)
        {
            var item = await UnitOfWork.WeeklyPlanItems.GetByIdAsync(id);
            if (item == null)
            {
                return null;
            }

            // Verify the weekly plan belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(item.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتعديل هذا العنصر");
            }

            // Only allow updates if status is Planned
            if (item.Status != "Planned")
            {
                throw new InvalidOperationException("لا يمكن تعديل العنصر بعد تنفيذه");
            }

            if (!string.IsNullOrEmpty(updateDto.ClientName))
                item.ClientName = updateDto.ClientName;
            if (updateDto.ClientType != null)
                item.ClientType = updateDto.ClientType;
            if (updateDto.ClientSpecialization != null)
                item.ClientSpecialization = updateDto.ClientSpecialization;
            if (updateDto.ClientLocation != null)
                item.ClientLocation = updateDto.ClientLocation;
            if (updateDto.ClientPhone != null)
                item.ClientPhone = updateDto.ClientPhone;
            if (updateDto.ClientEmail != null)
                item.ClientEmail = updateDto.ClientEmail;
            if (updateDto.PlannedVisitDate.HasValue)
                item.PlannedVisitDate = updateDto.PlannedVisitDate.Value;
            if (updateDto.PlannedVisitTime != null)
                item.PlannedVisitTime = updateDto.PlannedVisitTime;
            if (updateDto.VisitPurpose != null)
                item.VisitPurpose = updateDto.VisitPurpose;
            if (updateDto.VisitNotes != null)
                item.VisitNotes = updateDto.VisitNotes;
            if (!string.IsNullOrEmpty(updateDto.Priority))
                item.Priority = updateDto.Priority;

            await UnitOfWork.SaveChangesAsync();

            return new WeeklyPlanItemResponseDTO
            {
                Id = item.Id,
                ClientName = item.ClientName,
                PlannedVisitDate = item.PlannedVisitDate,
                VisitPurpose = item.VisitPurpose,
                Priority = item.Priority,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<bool> CompletePlanItemAsync(long id, CompletePlanItemDTO completeDto, string userId)
        {
            var item = await UnitOfWork.WeeklyPlanItems.GetByIdAsync(id);
            if (item == null)
            {
                return false;
            }

            // Verify the weekly plan belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(item.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتحديث هذا العنصر");
            }

            if (item.Status != "Planned")
            {
                throw new InvalidOperationException("لا يمكن تحديث حالة العنصر");
            }

            item.Status = "Completed";
            item.ActualVisitDate = DateTime.UtcNow;
            item.Results = completeDto.Results;
            item.Feedback = completeDto.Feedback;
            item.SatisfactionRating = completeDto.SatisfactionRating;
            item.NextVisitDate = completeDto.NextVisitDate;
            item.FollowUpNotes = completeDto.FollowUpNotes;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPlanItemAsync(long id, CancelPlanItemDTO cancelDto, string userId)
        {
            var item = await UnitOfWork.WeeklyPlanItems.GetByIdAsync(id);
            if (item == null)
            {
                return false;
            }

            // Verify the weekly plan belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(item.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لإلغاء هذا العنصر");
            }

            if (item.Status != "Planned")
            {
                throw new InvalidOperationException("لا يمكن إلغاء العنصر");
            }

            item.Status = "Cancelled";
            item.CancellationReason = cancelDto.Reason;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PostponePlanItemAsync(long id, PostponePlanItemDTO postponeDto, string userId)
        {
            var item = await UnitOfWork.WeeklyPlanItems.GetByIdAsync(id);
            if (item == null)
            {
                return false;
            }

            // Verify the weekly plan belongs to the user
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(item.WeeklyPlanId);
            if (plan == null || plan.EmployeeId != userId)
            {
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتأجيل هذا العنصر");
            }

            if (item.Status != "Planned")
            {
                throw new InvalidOperationException("لا يمكن تأجيل العنصر");
            }

            item.PlannedVisitDate = postponeDto.NewDate;
            item.PostponementReason = postponeDto.Reason;

            await UnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetOverdueItemsAsync(string userId)
        {
            var items = await UnitOfWork.WeeklyPlanItems.GetOverdueItemsAsync(userId);

            return items.Select(item => new WeeklyPlanItemResponseDTO
            {
                Id = item.Id,
                ClientName = item.ClientName,
                PlannedVisitDate = item.PlannedVisitDate,
                VisitPurpose = item.VisitPurpose,
                Priority = item.Priority,
                Status = item.Status
            });
        }

        public async Task<IEnumerable<WeeklyPlanItemResponseDTO>> GetUpcomingItemsAsync(string userId, int days = 7)
        {
            var items = await UnitOfWork.WeeklyPlanItems.GetUpcomingItemsAsync(userId, days);

            return items.Select(item => new WeeklyPlanItemResponseDTO
            {
                Id = item.Id,
                ClientName = item.ClientName,
                PlannedVisitDate = item.PlannedVisitDate,
                VisitPurpose = item.VisitPurpose,
                Priority = item.Priority,
                Status = item.Status
            });
        }
    }
}
