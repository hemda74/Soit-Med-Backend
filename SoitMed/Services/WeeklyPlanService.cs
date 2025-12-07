using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using System.Text.Json;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for weekly plan business operations
    /// </summary>
    public class WeeklyPlanService : BaseService, IWeeklyPlanService
    {
        private readonly IWeeklyPlanTaskService _taskService;

        public WeeklyPlanService(
            IUnitOfWork unitOfWork, 
            ILogger<WeeklyPlanService> logger,
            IWeeklyPlanTaskService taskService) 
            : base(unitOfWork, logger)
        {
            _taskService = taskService;
        }

        public async Task<WeeklyPlanResponseDTO> CreateWeeklyPlanAsync(CreateWeeklyPlanDTO createDto, string userId)
        {
            // Check if plan already exists for this week
            var existingPlan = await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, createDto.WeekStartDate);
            if (existingPlan)
            {
                throw new InvalidOperationException("يوجد خطة أسبوعية بالفعل لهذا الأسبوع");
            }

            // Auto-calculate end date as 7 days from start date
            var weekEndDate = createDto.WeekStartDate.AddDays(6); // 7 days total (start day + 6 more days)

            var plan = new Models.WeeklyPlan
            {
                EmployeeId = userId,
                WeekStartDate = createDto.WeekStartDate,
                WeekEndDate = weekEndDate, // Auto-calculated: 7 days from start
                Title = createDto.Title,
                Description = createDto.Description,
                IsActive = true
            };

            await UnitOfWork.WeeklyPlans.CreateAsync(plan);
            await UnitOfWork.SaveChangesAsync();

            // Create tasks if provided
            if (createDto.Tasks != null && createDto.Tasks.Any())
            {
                // Set WeeklyPlanId for all tasks (it might not be set in the DTO)
                foreach (var taskDto in createDto.Tasks)
                {
                    taskDto.WeeklyPlanId = plan.Id;
                }
                
                var createdTasks = await _taskService.CreateTasksAsync(plan.Id, createDto.Tasks, userId);
                Logger.LogInformation("Created {Count} tasks during weekly plan creation for plan {PlanId}", createdTasks.Count, plan.Id);
            }

            // Reload plan with tasks
            var planWithTasks = await UnitOfWork.WeeklyPlans.GetPlanWithFullDetailsAsync(plan.Id);
            
            // Map tasks sequentially to avoid DbContext concurrency issues
            var tasksList = planWithTasks?.Tasks?.ToList() ?? new List<WeeklyPlanTask>();
            var mappedTasks = new List<WeeklyPlanTaskResponseDTO>();
            
            foreach (var task in tasksList)
            {
                mappedTasks.Add(await MapTaskToDTO(task));
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
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                Tasks = mappedTasks
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

            // OPTIMIZATION: Map tasks sequentially to avoid DbContext concurrency issues
            // MapTaskToDTO makes database calls, so we can't parallelize it safely
            var plansList = plans.ToList();
            var planDtos = new List<WeeklyPlanResponseDTO>();
            
            foreach (var p in plansList)
            {
                var tasksList = p.Tasks.ToList();
                var mappedTasks = new List<WeeklyPlanTaskResponseDTO>();
                
                foreach (var task in tasksList)
                {
                    mappedTasks.Add(await MapTaskToDTO(task));
                }
                
                planDtos.Add(new WeeklyPlanResponseDTO
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                Employee = p.Employee != null ? new EmployeeInfoDTO
                {
                    Id = p.Employee.Id,
                    FirstName = p.Employee.FirstName ?? string.Empty,
                    LastName = p.Employee.LastName ?? string.Empty,
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
                    Tasks = mappedTasks
            });
            }

            return (planDtos, totalCount);
        }

        public async Task<(IEnumerable<WeeklyPlanResponseDTO> Plans, int TotalCount)> GetWeeklyPlansWithFiltersAsync(WeeklyPlanFiltersDTO filters, string userRole, int page, int pageSize)
        {
            // SalesManager, SuperAdmin, and Salesman can use filters
            // Note: For Salesman, the controller ensures they can only filter their own plans by date
            if (userRole != "SalesManager" && userRole != "SuperAdmin" && userRole != "Salesman")
            {
                throw new UnauthorizedAccessException("Only SalesManager, SuperAdmin and Salesman can use filters");
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

            // OPTIMIZATION: Map tasks sequentially to avoid DbContext concurrency issues
            // MapTaskToDTO makes database calls, so we can't parallelize it safely
            var plansList = plans.ToList();
            var planDtos = new List<WeeklyPlanResponseDTO>();
            
            foreach (var p in plansList)
            {
                var tasksList = p.Tasks.ToList();
                var mappedTasks = new List<WeeklyPlanTaskResponseDTO>();
                
                foreach (var task in tasksList)
                {
                    mappedTasks.Add(await MapTaskToDTO(task));
                }
                
                planDtos.Add(new WeeklyPlanResponseDTO
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                Employee = p.Employee != null ? new EmployeeInfoDTO
                {
                    Id = p.Employee.Id,
                    FirstName = p.Employee.FirstName ?? string.Empty,
                    LastName = p.Employee.LastName ?? string.Empty,
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
                    Tasks = mappedTasks
            });
            }

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

            // Map tasks sequentially to avoid DbContext concurrency issues
            var tasksList = plan.Tasks.ToList();
            var mappedTasks = new List<WeeklyPlanTaskResponseDTO>();
            
            foreach (var task in tasksList)
            {
                mappedTasks.Add(await MapTaskToDTO(task));
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
                Tasks = mappedTasks
            };
        }

        private async Task<WeeklyPlanTaskResponseDTO> MapTaskToDTO(WeeklyPlanTask task)
        {
            // Get offer requests from task progresses
            var offerRequestIds = task.Progresses
                .Where(p => p.OfferRequestId.HasValue)
                .Select(p => p.OfferRequestId ?? 0)
                .Distinct()
                .ToList();

            var offerRequests = new List<OfferRequestSimpleDTO>();
            var offers = new List<SalesOfferSimpleDTO>();
            var deals = new List<SalesDealSimpleDTO>();

            // OPTIMIZATION: Use repository methods that query by IDs instead of loading all data
            if (offerRequestIds.Any())
            {
                var offerRequestsData = await UnitOfWork.OfferRequests.GetByIdsAsync(offerRequestIds);

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
                        .Select(or => or.CreatedOfferId ?? 0)
                        .Distinct()
                        .ToList();

                    var offersData = await UnitOfWork.SalesOffers.GetByIdsAsync(offerIds);

                    offers = offersData.Select(o => 
                    {
                        // Deserialize ValidUntil from JSON string to List<string>
                        List<string>? validUntilList = null;
                        if (!string.IsNullOrWhiteSpace(o.ValidUntil))
                        {
                            try
                            {
                                validUntilList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(o.ValidUntil);
                            }
                            catch
                            {
                                // Fallback: treat as single string value (backward compatibility)
                                validUntilList = new List<string> { o.ValidUntil };
                            }
                        }
                        
                        return new SalesOfferSimpleDTO
                        {
                            Id = o.Id,
                            Products = o.Products,
                            TotalAmount = o.TotalAmount,
                            ValidUntil = validUntilList,
                            Status = o.Status,
                            SentToClientAt = o.SentToClientAt
                        };
                    }).ToList();

                    // Get deals created from these offers
                    if (offersData.Any(o => o.Deal != null))
                    {
                        var dealIds = offersData
                            .Where(o => o.Deal != null)
                            .Select(o => o.Deal?.Id ?? 0)
                            .Distinct()
                            .ToList();

                        var dealsData = await UnitOfWork.SalesDeals.GetByIdsAsync(dealIds);
                        deals = dealsData.Select(d => new SalesDealSimpleDTO
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
					VoiceDescriptionUrl = p.VoiceDescriptionUrl,
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

            // OPTIMIZATION: Materialize tasks list once to avoid multiple enumerations
            // Progresses are already loaded via ThenInclude in the repository
            var tasksList = plan.Tasks?.ToList() ?? new List<WeeklyPlanTask>();

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
                Tasks = tasksList.Select(t => new WeeklyPlanTaskResponseDTO
                {
                    Id = t.Id,
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
                    ProgressCount = t.Progresses?.Count ?? 0 // Progresses are already loaded
                }).ToList()
            };
        }

        public async Task<bool> HasPlanForWeekAsync(string userId, DateTime weekStartDate)
        {
            return await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, weekStartDate);
        }
    }
}
