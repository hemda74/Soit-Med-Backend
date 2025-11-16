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

        public async Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansAsync(string userId, string userRole, int page, int pageSize)
        {
            // SalesManager and SuperAdmin can view all plans
            var plans = (userRole == "SalesManager" || userRole == "SuperAdmin") 
                ? await UnitOfWork.WeeklyPlans.GetAllPlansAsync(page, pageSize)
                : await UnitOfWork.WeeklyPlans.GetEmployeePlansAsync(userId, page, pageSize);
            
            var totalCount = (userRole == "SalesManager" || userRole == "SuperAdmin")
                ? await UnitOfWork.WeeklyPlans.CountAsync()
                : await UnitOfWork.WeeklyPlans.CountAsync(p => p.EmployeeId == userId);

            var planDtos = plans.Select(p => new WeeklyPlanResponseDTO
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                Employee = p.Employee != null ? new EmployeeInfoDTO
                {
                    Id = p.Employee.Id,
                    FirstName = p.Employee.FirstName,
                    LastName = p.Employee.LastName,
                    Email = p.Employee.Email ?? string.Empty,
                    PhoneNumber = p.Employee.PhoneNumber,
                    UserName = p.Employee.UserName ?? string.Empty
                } : null,
                WeekStartDate = p.WeekStartDate,
                WeekEndDate = p.WeekEndDate,
                Title = p.Title,
                Description = p.Description,
                IsActive = p.IsActive,
                Rating = p.Rating,
                ManagerComment = p.ManagerComment,
                ManagerReviewedAt = p.ManagerReviewedAt,
                ManagerViewedAt = p.ManagerViewedAt,
                ViewedBy = p.ViewedBy,
                IsViewed = p.ManagerViewedAt.HasValue,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Tasks = p.Tasks.Select(t => MapTaskToDTO(t)).ToList()
            });

            return (planDtos, totalCount);
        }

        public async Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansWithFiltersAsync(WeeklyPlanFiltersDTO filters, string userRole, int page, int pageSize)
        {
            // Only SalesManager and SuperAdmin can use filters
            if (userRole != "SalesManager" && userRole != "SuperAdmin")
            {
                throw new UnauthorizedAccessException("Only SalesManager and SuperAdmin can use filters");
            }

            var plans = await UnitOfWork.WeeklyPlans.GetAllPlansWithFiltersAsync(
                filters.EmployeeId,
                filters.WeekStartDate,
                filters.WeekEndDate,
                filters.IsViewed,
                page,
                pageSize
            );

            var totalCount = await UnitOfWork.WeeklyPlans.CountAllPlansWithFiltersAsync(
                filters.EmployeeId,
                filters.WeekStartDate,
                filters.WeekEndDate,
                filters.IsViewed
            );

            var planDtos = plans.Select(p => new WeeklyPlanResponseDTO
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                Employee = p.Employee != null ? new EmployeeInfoDTO
                {
                    Id = p.Employee.Id,
                    FirstName = p.Employee.FirstName,
                    LastName = p.Employee.LastName,
                    Email = p.Employee.Email ?? string.Empty,
                    PhoneNumber = p.Employee.PhoneNumber,
                    UserName = p.Employee.UserName ?? string.Empty
                } : null,
                WeekStartDate = p.WeekStartDate,
                WeekEndDate = p.WeekEndDate,
                Title = p.Title,
                Description = p.Description,
                IsActive = p.IsActive,
                Rating = p.Rating,
                ManagerComment = p.ManagerComment,
                ManagerReviewedAt = p.ManagerReviewedAt,
                ManagerViewedAt = p.ManagerViewedAt,
                ViewedBy = p.ViewedBy,
                IsViewed = p.ManagerViewedAt.HasValue,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Tasks = p.Tasks.Select(t => MapTaskToDTO(t)).ToList()
            });

            return (planDtos, totalCount);
        }

        public async Task<WeeklyPlanResponseDTO?> GetWeeklyPlanAsync(long id, string userId, string userRole)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetPlanWithFullDetailsAsync(id);
            if (plan == null)
            {
                return null;
            }

            // Check authorization: managers can view any plan, users can only view their own
            if (userRole != "SalesManager" && userRole != "SuperAdmin" && plan.EmployeeId != userId)
            {
                return null;
            }

            // Mark as viewed if manager/admin is viewing
            if ((userRole == "SalesManager" || userRole == "SuperAdmin") && !plan.ManagerViewedAt.HasValue)
            {
                plan.ManagerViewedAt = DateTime.UtcNow;
                plan.ViewedBy = userId;
                await UnitOfWork.SaveChangesAsync();
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
                ManagerViewedAt = plan.ManagerViewedAt,
                ViewedBy = plan.ViewedBy,
                IsViewed = plan.ManagerViewedAt.HasValue,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                Tasks = plan.Tasks.Select(t => MapTaskToDTO(t)).ToList()
            };
        }

        private WeeklyPlanTaskResponseDTO MapTaskToDTO(WeeklyPlanTask task)
        {
            // Get offer requests from task progresses
            var offerRequestIds = task.Progresses
                .Where(p => p.OfferRequestId.HasValue)
                .Select(p => p.OfferRequestId.Value)
                .Distinct()
                .ToList();

            var offerRequests = new List<OfferRequestSimpleDTO>();
            var offers = new List<SalesOfferSimpleDTO>();
            var deals = new List<SalesDealSimpleDTO>();

            // Load offer requests, offers, and deals if needed
            if (offerRequestIds.Any())
            {
                var offerRequestsData = UnitOfWork.OfferRequests.GetAllAsync().Result
                    .Where(or => offerRequestIds.Contains(or.Id))
                    .ToList();

                offerRequests = offerRequestsData.Select(or => new OfferRequestSimpleDTO
                {
                    Id = or.Id,
                    RequestedProducts = or.RequestedProducts,
                    RequestDate = or.RequestDate,
                    Status = or.Status,
                    CreatedOfferId = or.CreatedOfferId
                }).ToList();

                // Get offers created from these offer requests
                if (offerRequestsData.Any(or => or.CreatedOfferId.HasValue))
                {
                    var offerIds = offerRequestsData
                        .Where(or => or.CreatedOfferId.HasValue)
                        .Select(or => or.CreatedOfferId.Value)
                        .Distinct()
                        .ToList();

                    var offersData = UnitOfWork.SalesOffers.GetAllAsync().Result
                        .Where(o => offerIds.Contains(o.Id))
                        .ToList();

                    offers = offersData.Select(o => new SalesOfferSimpleDTO
                    {
                        Id = o.Id,
                        Products = o.Products ?? string.Empty,
                        TotalAmount = o.TotalAmount,
                        ValidUntil = o.ValidUntil,
                        Status = o.Status,
                        SentToClientAt = o.SentToClientAt
                    }).ToList();

                    // Get deals created from these offers
                    if (offersData.Any(o => o.Deal != null))
                    {
                        var dealIds = offersData
                            .Where(o => o.Deal != null)
                            .Select(o => o.Deal.Id)
                            .Distinct()
                            .ToList();

                        deals = UnitOfWork.SalesDeals.GetAllAsync().Result
                            .Where(d => dealIds.Contains(d.Id))
                            .Select(d => new SalesDealSimpleDTO
                            {
                                Id = d.Id,
                                DealValue = d.DealValue,
                                ClosedDate = d.ClosedDate,
                                Status = d.Status,
                                ManagerApprovedAt = d.ManagerApprovedAt,
                                SuperAdminApprovedAt = d.SuperAdminApprovedAt
                            })
                            .ToList();
                    }
                }
            }

            return new WeeklyPlanTaskResponseDTO
            {
                Id = task.Id,
                WeeklyPlanId = task.WeeklyPlanId,
                Title = task.Title,
                ClientId = task.ClientId,
                ClientName = task.ClientName,
                ClientStatus = task.ClientStatus,
                ClientPhone = task.ClientPhone,
                ClientAddress = task.ClientAddress,
                ClientLocation = task.ClientLocation,
                ClientClassification = task.ClientClassification,
                PlannedDate = task.PlannedDate,
                Notes = task.Notes,
                ProgressCount = task.Progresses.Count,
                Progresses = task.Progresses.Select(p => new TaskProgressSimpleDTO
                {
                    Id = p.Id,
                    ProgressDate = p.ProgressDate,
                    ProgressType = p.ProgressType,
                    Description = p.Description,
                    VisitResult = p.VisitResult,
                    NextStep = p.NextStep,
                    OfferRequestId = p.OfferRequestId
                }).ToList(),
                OfferRequests = offerRequests,
                Offers = offers,
                Deals = deals
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
                    WeeklyPlanId = t.WeeklyPlanId,
                    Title = t.Title,
                    ClientId = t.ClientId,
                    ClientName = t.ClientName,
                    ClientStatus = t.ClientStatus,
                    ClientPhone = t.ClientPhone,
                    ClientAddress = t.ClientAddress,
                    ClientLocation = t.ClientLocation,
                    ClientClassification = t.ClientClassification,
                    PlannedDate = t.PlannedDate,
                    Notes = t.Notes,
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
