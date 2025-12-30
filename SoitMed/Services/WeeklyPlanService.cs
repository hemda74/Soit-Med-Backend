using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using System.Text.Json;
using System.Linq.Expressions;

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
            var weekStartDateOnly = DateOnly.FromDateTime(createDto.WeekStartDate);
            var existingPlan = await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, weekStartDateOnly, null);
            if (existingPlan)
            {
                throw new InvalidOperationException("يوجد خطة أسبوعية بالفعل لهذا الأسبوع");
            }

            // Auto-calculate end date as 7 days from start date
            var weekEndDate = weekStartDateOnly.AddDays(6); // 7 days total (start day + 6 more days)

            var plan = new Models.WeeklyPlan
            {
                EmployeeId = userId,
                WeekStartDate = weekStartDateOnly,
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
            var planWithTasks = await UnitOfWork.WeeklyPlans.GetByIdWithDetailsAsync(plan.Id);
            
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
                WeekStartDate = plan.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = plan.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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
            IEnumerable<WeeklyPlan> plans;
            int totalCount;
            
            if (userRole == "SalesManager" || userRole == "SuperAdmin")
            {
                var result = await UnitOfWork.WeeklyPlans.GetPaginatedAsync(null, page, pageSize);
                plans = result.Plans;
                totalCount = result.TotalCount;
            }
            else
            {
                var result = await UnitOfWork.WeeklyPlans.GetPaginatedAsync(p => p.EmployeeId == userId, page, pageSize);
                plans = result.Plans;
                totalCount = result.TotalCount;
            }

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
                WeekStartDate = p.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = p.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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
            // SalesManager, SuperAdmin, and SalesMan can use filters
            // Note: For SalesMan, the controller ensures they can only filter their own plans by date
            if (userRole != "SalesManager" && userRole != "SuperAdmin" && userRole != "SalesMan")
            {
                throw new UnauthorizedAccessException("Only SalesManager, SuperAdmin and SalesMan can use filters");
            }

            // Build predicate for filtering
            Expression<Func<WeeklyPlan, bool>>? predicate = null;
            if (!string.IsNullOrEmpty(filters.EmployeeId) || filters.WeekStartDate.HasValue || filters.WeekEndDate.HasValue || filters.IsViewed.HasValue)
            {
                predicate = p => 
                    (string.IsNullOrEmpty(filters.EmployeeId) || p.EmployeeId == filters.EmployeeId) &&
                    (!filters.WeekStartDate.HasValue || p.WeekStartDate >= DateOnly.FromDateTime(filters.WeekStartDate.Value)) &&
                    (!filters.WeekEndDate.HasValue || p.WeekEndDate <= DateOnly.FromDateTime(filters.WeekEndDate.Value)) &&
                    (!filters.IsViewed.HasValue || (filters.IsViewed.Value ? p.ManagerViewedAt.HasValue : !p.ManagerViewedAt.HasValue));
            }

            var result = await UnitOfWork.WeeklyPlans.GetPaginatedAsync(predicate, page, pageSize);
            var plans = result.Plans;
            var totalCount = result.TotalCount;

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
                WeekStartDate = p.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = p.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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
            var plan = await UnitOfWork.WeeklyPlans.GetByIdWithDetailsAsync(id);
            if (plan == null)
            {
                return null;
            }

            // Check authorization: managers can view any plan, users can only view their own
            if (userRole != "SalesManager" && userRole != "SuperAdmin" && plan.EmployeeId != userId)
            {
                return null;
            }

            // Mark as viewed if manager/Admin is viewing
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
                WeekStartDate = plan.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = plan.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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
                WeekStartDate = plan.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = plan.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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

        private async Task<WeeklyPlanResponseDTO?> GetCurrentWeeklyPlanInternalAsync(string userId)
        {
            // Get current week's plan using GetByEmployeeIdAsync and filter
            var allPlans = await UnitOfWork.WeeklyPlans.GetByEmployeeIdAsync(userId);
            var now = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var plan = allPlans.FirstOrDefault(p => p.WeekStartDate <= now && p.WeekEndDate >= now && p.IsActive);
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
                WeekStartDate = plan.WeekStartDate.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = plan.WeekEndDate.ToDateTime(TimeOnly.MinValue),
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
            var weekStartDateOnly = DateOnly.FromDateTime(weekStartDate);
            return await UnitOfWork.WeeklyPlans.HasPlanForWeekAsync(userId, weekStartDateOnly, null);
        }

        // ==================== Interface Implementation Methods ====================
        // These methods implement IWeeklyPlanService interface with lowercase 'Dto' types

        public async Task<WeeklyPlanResponseDto?> CreateWeeklyPlanAsync(CreateWeeklyPlanDto createDto, string employeeId, CancellationToken cancellationToken = default)
        {
            // Convert CreateWeeklyPlanDto to CreateWeeklyPlanDTO
            var createDtoUpper = new CreateWeeklyPlanDTO
            {
                Title = createDto.Title,
                Description = createDto.Description,
                WeekStartDate = createDto.WeekStartDate,
                WeekEndDate = createDto.WeekEndDate,
                Tasks = createDto.Tasks?.Select(t => new CreateWeeklyPlanTaskDTO
                {
                    WeeklyPlanId = 0, // Will be set by service
                    Title = t.Title,
                    ClientId = null, // Not in CreateWeeklyPlanTaskDto
                    ClientName = null,
                    ClientPhone = null,
                    ClientAddress = null,
                    ClientLocation = null,
                    ClientClassification = null,
                    PlannedDate = null,
                    Notes = t.Description,
                    ClientStatus = null
                }).ToList()
            };

            var result = await CreateWeeklyPlanAsync(createDtoUpper, employeeId);
            
            // Convert WeeklyPlanResponseDTO to WeeklyPlanResponseDto
            if (result == null) return null;
            
            return new WeeklyPlanResponseDto
            {
                Id = (int)result.Id,
                Title = result.Title,
                Description = result.Description,
                WeekStartDate = DateOnly.FromDateTime(result.WeekStartDate),
                WeekEndDate = DateOnly.FromDateTime(result.WeekEndDate),
                EmployeeId = result.EmployeeId,
                EmployeeName = result.Employee?.FirstName + " " + result.Employee?.LastName ?? "",
                Rating = result.Rating,
                ManagerComment = result.ManagerComment,
                ManagerReviewedAt = result.ManagerReviewedAt,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                IsActive = result.IsActive,
                Tasks = result.Tasks?.Select(t => new WeeklyPlanTaskResponseDto
                {
                    Id = (int)t.Id,
                    WeeklyPlanId = t.WeeklyPlanId,
                    Title = t.Title,
                    Description = t.Notes,
                    IsCompleted = false, // Not available in WeeklyPlanTaskResponseDTO
                    DisplayOrder = 0, // Not available in WeeklyPlanTaskResponseDTO
                    CreatedAt = DateTime.UtcNow, // Not available in WeeklyPlanTaskResponseDTO
                    UpdatedAt = DateTime.UtcNow // Not available in WeeklyPlanTaskResponseDTO
                }).ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                DailyProgresses = new List<DailyProgressResponseDto>(),
                TotalTasks = result.Tasks?.Count ?? 0,
                CompletedTasks = 0,
                CompletionPercentage = 0
            };
        }

        public async Task<WeeklyPlanResponseDto?> UpdateWeeklyPlanAsync(long id, UpdateWeeklyPlanDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var updateDtoUpper = new UpdateWeeklyPlanDTO
            {
                Title = updateDto.Title,
                Description = updateDto.Description
            };

            var result = await UpdateWeeklyPlanAsync(id, updateDtoUpper, employeeId);
            if (result == null) return null;

            return new WeeklyPlanResponseDto
            {
                Id = (int)result.Id,
                Title = result.Title,
                Description = result.Description,
                WeekStartDate = DateOnly.FromDateTime(result.WeekStartDate),
                WeekEndDate = DateOnly.FromDateTime(result.WeekEndDate),
                EmployeeId = result.EmployeeId,
                EmployeeName = "",
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                IsActive = result.IsActive,
                Tasks = new List<WeeklyPlanTaskResponseDto>(),
                DailyProgresses = new List<DailyProgressResponseDto>(),
                TotalTasks = 0,
                CompletedTasks = 0,
                CompletionPercentage = 0
            };
        }

        public async Task<bool> DeleteWeeklyPlanAsync(long id, string employeeId, CancellationToken cancellationToken = default)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(id, cancellationToken);
            if (plan == null || plan.EmployeeId != employeeId)
                return false;

            plan.IsActive = false;
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<WeeklyPlanResponseDto?> GetWeeklyPlanByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdWithDetailsAsync(id, cancellationToken);
            if (plan == null) return null;

            return new WeeklyPlanResponseDto
            {
                Id = (int)plan.Id,
                Title = plan.Title,
                Description = plan.Description,
                WeekStartDate = plan.WeekStartDate,
                WeekEndDate = plan.WeekEndDate,
                EmployeeId = plan.EmployeeId,
                EmployeeName = plan.Employee?.FirstName + " " + plan.Employee?.LastName ?? "",
                Rating = plan.Rating,
                ManagerComment = plan.ManagerComment,
                ManagerReviewedAt = plan.ManagerReviewedAt,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                IsActive = plan.IsActive,
                Tasks = plan.Tasks?.Select(t => new WeeklyPlanTaskResponseDto
                {
                    Id = t.Id,
                    WeeklyPlanId = t.WeeklyPlanId,
                    Title = t.Title,
                    Description = t.Notes,
                    IsCompleted = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                DailyProgresses = new List<DailyProgressResponseDto>(),
                TotalTasks = plan.Tasks?.Count ?? 0,
                CompletedTasks = 0,
                CompletionPercentage = 0
            };
        }

        public async Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansAsync(FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default)
        {
            var filtersUpper = new WeeklyPlanFiltersDTO
            {
                EmployeeId = filterDto.EmployeeId,
                WeekStartDate = filterDto.StartDate?.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = filterDto.EndDate?.ToDateTime(TimeOnly.MinValue),
                IsViewed = filterDto.HasManagerReview
            };

            var (plans, totalCount) = await GetWeeklyPlansWithFiltersAsync(filtersUpper, "SalesManager", filterDto.Page, filterDto.PageSize);

            return new PaginatedWeeklyPlansResponseDto
            {
                Data = plans.Select(p => new WeeklyPlanResponseDto
                {
                    Id = (int)p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    WeekStartDate = DateOnly.FromDateTime(p.WeekStartDate),
                    WeekEndDate = DateOnly.FromDateTime(p.WeekEndDate),
                    EmployeeId = p.EmployeeId,
                    EmployeeName = p.Employee?.FirstName + " " + p.Employee?.LastName ?? "",
                    Rating = p.Rating,
                    ManagerComment = p.ManagerComment,
                    ManagerReviewedAt = p.ManagerReviewedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActive = p.IsActive,
                    Tasks = p.Tasks?.Select(t => new WeeklyPlanTaskResponseDto
                    {
                        Id = (int)t.Id,
                        WeeklyPlanId = t.WeeklyPlanId,
                        Title = t.Title,
                        Description = t.Notes,
                        IsCompleted = false,
                        DisplayOrder = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                    DailyProgresses = new List<DailyProgressResponseDto>(),
                    TotalTasks = p.Tasks?.Count ?? 0,
                    CompletedTasks = 0,
                    CompletionPercentage = 0
                }).ToList(),
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filterDto.PageSize)
            };
        }

        public async Task<PaginatedWeeklyPlansResponseDto> GetWeeklyPlansForEmployeeAsync(string employeeId, FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default)
        {
            var filtersUpper = new WeeklyPlanFiltersDTO
            {
                EmployeeId = employeeId,
                WeekStartDate = filterDto.StartDate?.ToDateTime(TimeOnly.MinValue),
                WeekEndDate = filterDto.EndDate?.ToDateTime(TimeOnly.MinValue),
                IsViewed = filterDto.HasManagerReview
            };

            var (plans, totalCount) = await GetWeeklyPlansWithFiltersAsync(filtersUpper, "SalesMan", filterDto.Page, filterDto.PageSize);

            return new PaginatedWeeklyPlansResponseDto
            {
                Data = plans.Select(p => new WeeklyPlanResponseDto
                {
                    Id = (int)p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    WeekStartDate = DateOnly.FromDateTime(p.WeekStartDate),
                    WeekEndDate = DateOnly.FromDateTime(p.WeekEndDate),
                    EmployeeId = p.EmployeeId,
                    EmployeeName = p.Employee?.FirstName + " " + p.Employee?.LastName ?? "",
                    Rating = p.Rating,
                    ManagerComment = p.ManagerComment,
                    ManagerReviewedAt = p.ManagerReviewedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActive = p.IsActive,
                    Tasks = p.Tasks?.Select(t => new WeeklyPlanTaskResponseDto
                    {
                        Id = (int)t.Id,
                        WeeklyPlanId = t.WeeklyPlanId,
                        Title = t.Title,
                        Description = t.Notes,
                        IsCompleted = false,
                        DisplayOrder = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                    DailyProgresses = new List<DailyProgressResponseDto>(),
                    TotalTasks = p.Tasks?.Count ?? 0,
                    CompletedTasks = 0,
                    CompletionPercentage = 0
                }).ToList(),
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filterDto.PageSize)
            };
        }

        public async Task<bool> CanAccessWeeklyPlanAsync(long planId, string userId, bool isManager, CancellationToken cancellationToken = default)
        {
            var plan = await UnitOfWork.WeeklyPlans.GetByIdAsync(planId, cancellationToken);
            if (plan == null) return false;

            if (isManager) return true;
            return plan.EmployeeId == userId;
        }

        public async Task<WeeklyPlanTaskResponseDto?> AddTaskToWeeklyPlanAsync(long weeklyPlanId, AddTaskToWeeklyPlanDto taskDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var createTaskDto = new CreateWeeklyPlanTaskDTO
            {
                WeeklyPlanId = weeklyPlanId,
                Title = taskDto.Title,
                Notes = taskDto.Description,
                ClientId = null,
                ClientName = null,
                ClientPhone = null,
                ClientAddress = null,
                ClientLocation = null,
                ClientClassification = null,
                PlannedDate = null,
                ClientStatus = null
            };

            var result = await _taskService.CreateTaskAsync(createTaskDto, employeeId);
            if (result == null) return null;

            return new WeeklyPlanTaskResponseDto
            {
                Id = (int)result.Id,
                WeeklyPlanId = result.WeeklyPlanId,
                Title = result.Title,
                Description = result.Notes,
                IsCompleted = false,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<WeeklyPlanTaskResponseDto?> UpdateTaskAsync(long weeklyPlanId, int taskId, UpdateWeeklyPlanTaskDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var updateTaskDto = new UpdateWeeklyPlanTaskDTO
            {
                Title = updateDto.Title,
                Notes = updateDto.Description,
                ClientId = null,
                ClientName = null,
                ClientPhone = null,
                ClientAddress = null,
                ClientLocation = null,
                ClientClassification = null,
                PlannedDate = null,
                ClientStatus = null
            };

            var result = await _taskService.UpdateTaskAsync(taskId, updateTaskDto, employeeId);
            if (result == null) return null;

            return new WeeklyPlanTaskResponseDto
            {
                Id = (int)result.Id,
                WeeklyPlanId = result.WeeklyPlanId,
                Title = result.Title,
                Description = result.Notes,
                IsCompleted = false,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteTaskAsync(long weeklyPlanId, int taskId, string employeeId, CancellationToken cancellationToken = default)
        {
            return await _taskService.DeleteTaskAsync(taskId, employeeId);
        }

        public async Task<DailyProgressResponseDto?> AddDailyProgressAsync(long weeklyPlanId, CreateDailyProgressDto progressDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var progress = new DailyProgress
            {
                WeeklyPlanId = weeklyPlanId,
                ProgressDate = progressDto.ProgressDate,
                Notes = progressDto.Notes,
                TasksWorkedOn = progressDto.TasksWorkedOn != null && progressDto.TasksWorkedOn.Any()
                    ? string.Join(",", progressDto.TasksWorkedOn)
                    : null,
                IsActive = true
            };

            var repository = new DailyProgressRepository(UnitOfWork.GetContext());
            var created = await repository.CreateAsync(progress, cancellationToken);

            return new DailyProgressResponseDto
            {
                Id = created.Id,
                WeeklyPlanId = created.WeeklyPlanId,
                ProgressDate = created.ProgressDate,
                Notes = created.Notes,
                TasksWorkedOn = !string.IsNullOrEmpty(created.TasksWorkedOn)
                    ? created.TasksWorkedOn.Split(',').Select(int.Parse).ToList()
                    : new List<int>(),
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };
        }

        public async Task<DailyProgressResponseDto?> UpdateDailyProgressAsync(long weeklyPlanId, int progressId, UpdateDailyProgressDto updateDto, string employeeId, CancellationToken cancellationToken = default)
        {
            var repository = new DailyProgressRepository(UnitOfWork.GetContext());
            var progress = await repository.GetByIdAsync(progressId, cancellationToken);
            if (progress == null || progress.WeeklyPlanId != weeklyPlanId)
                return null;

            progress.Notes = updateDto.Notes;
            progress.TasksWorkedOn = updateDto.TasksWorkedOn != null && updateDto.TasksWorkedOn.Any()
                ? string.Join(",", updateDto.TasksWorkedOn)
                : null;
            progress.UpdatedAt = DateTime.UtcNow;

            var updated = await repository.UpdateAsync(progress, cancellationToken);

            return new DailyProgressResponseDto
            {
                Id = updated.Id,
                WeeklyPlanId = updated.WeeklyPlanId,
                ProgressDate = updated.ProgressDate,
                Notes = updated.Notes,
                TasksWorkedOn = !string.IsNullOrEmpty(updated.TasksWorkedOn)
                    ? updated.TasksWorkedOn.Split(',').Select(int.Parse).ToList()
                    : new List<int>(),
                CreatedAt = updated.CreatedAt,
                UpdatedAt = updated.UpdatedAt
            };
        }

        public async Task<bool> DeleteDailyProgressAsync(long weeklyPlanId, int progressId, string employeeId, CancellationToken cancellationToken = default)
        {
            var repository = new DailyProgressRepository(UnitOfWork.GetContext());
            return await repository.DeleteAsync(progressId, cancellationToken);
        }

        public async Task<WeeklyPlanResponseDto?> ReviewWeeklyPlanAsync(long id, ReviewWeeklyPlanDto reviewDto, CancellationToken cancellationToken = default)
        {
            var reviewDtoUpper = new ReviewWeeklyPlanDTO
            {
                Rating = reviewDto.Rating,
                Comment = reviewDto.ManagerComment
            };

            var result = await ReviewWeeklyPlanAsync(id, reviewDtoUpper, "");
            if (!result) return null;

            return await GetWeeklyPlanByIdAsync(id, cancellationToken);
        }

        public async Task<WeeklyPlanResponseDto?> GetCurrentWeeklyPlanAsync(string userId)
        {
            var result = await GetCurrentWeeklyPlanInternalAsync(userId);
            if (result == null) return null;

            return new WeeklyPlanResponseDto
            {
                Id = (int)result.Id,
                Title = result.Title,
                Description = result.Description,
                WeekStartDate = DateOnly.FromDateTime(result.WeekStartDate),
                WeekEndDate = DateOnly.FromDateTime(result.WeekEndDate),
                EmployeeId = result.EmployeeId,
                EmployeeName = result.Employee?.FirstName + " " + result.Employee?.LastName ?? "",
                Rating = result.Rating,
                ManagerComment = result.ManagerComment,
                ManagerReviewedAt = result.ManagerReviewedAt,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt,
                IsActive = result.IsActive,
                Tasks = result.Tasks?.Select(t => new WeeklyPlanTaskResponseDto
                {
                    Id = (int)t.Id,
                    WeeklyPlanId = t.WeeklyPlanId,
                    Title = t.Title,
                    Description = t.Notes,
                    IsCompleted = false,
                    DisplayOrder = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList() ?? new List<WeeklyPlanTaskResponseDto>(),
                DailyProgresses = new List<DailyProgressResponseDto>(),
                TotalTasks = result.Tasks?.Count ?? 0,
                CompletedTasks = 0,
                CompletionPercentage = 0
            };
        }
    }
}









