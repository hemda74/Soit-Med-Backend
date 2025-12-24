using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using System.Text.Json;

namespace SoitMed.Services
{
    /// <summary>
    /// Service implementation for weekly plan task business operations
    /// </summary>
    public class WeeklyPlanTaskService : BaseService, IWeeklyPlanTaskService
    {
        public WeeklyPlanTaskService(IUnitOfWork unitOfWork, ILogger<WeeklyPlanTaskService> logger)
            : base(unitOfWork, logger)
        {
        }

        public async Task<WeeklyPlanTaskDetailResponseDTO> CreateTaskAsync(CreateWeeklyPlanTaskDTO createDto, string userId)
        {
            try
            {
                Logger.LogInformation("  Creating Task - WeeklyPlanId: {WeeklyPlanId}, UserId: {UserId}, Title: {Title}", 
                    createDto.WeeklyPlanId, userId, createDto.Title);
                
                // Validate weekly plan exists and belongs to user
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync((int)createDto.WeeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found", nameof(createDto.WeeklyPlanId));

                Logger.LogInformation("  Task Authorization Check - WeeklyPlanId: {WeeklyPlanId}, WeeklyPlan.EmployeeId: {PlanEmployeeId}, CurrentUserId: {CurrentUserId}", 
                    createDto.WeeklyPlanId, weeklyPlan.EmployeeId, userId);

                if (weeklyPlan.EmployeeId != userId)
                {
                    Logger.LogWarning("⛔ AUTHORIZATION FAILED - User {UserId} attempted to add task to weekly plan {WeeklyPlanId} owned by {PlanEmployeeId}", 
                        userId, createDto.WeeklyPlanId, weeklyPlan.EmployeeId);
                    throw new UnauthorizedAccessException("You don't have permission to add tasks to this weekly plan");
                }

                // Validate client status
                if (!string.IsNullOrEmpty(createDto.ClientStatus) && !Models.ClientStatusConstants.IsValidStatus(createDto.ClientStatus))
                    throw new ArgumentException($"Invalid client status. Must be one of: {string.Join(", ", Models.ClientStatusConstants.AllStatuses)}");

                // Validate client classification
                if (!string.IsNullOrEmpty(createDto.ClientClassification) && !Models.ClientClassificationConstants.IsValidClassification(createDto.ClientClassification))
                    throw new ArgumentException($"Invalid client classification. Must be one of: {string.Join(", ", Models.ClientClassificationConstants.AllClassifications)}");

                // Client resolution: If clientName is provided, search for existing client by name
                long? resolvedClientId = null;
                if (!string.IsNullOrWhiteSpace(createDto.ClientName))
                {
                    // Try to find existing client by name
                    var existingClient = await UnitOfWork.Clients.FindByNameAsync(createDto.ClientName.Trim());
                    if (existingClient != null)
                    {
                        resolvedClientId = existingClient.Id;
                        // If client found, set status to "Old" automatically
                        if (string.IsNullOrEmpty(createDto.ClientStatus))
                            createDto.ClientStatus = "Old";
                    }
                    else
                    {
                        // Client not found, treat as new client
                        if (string.IsNullOrEmpty(createDto.ClientStatus))
                            createDto.ClientStatus = "New";
                    }
                }

                // If clientId was explicitly provided, use it (for backward compatibility)
                if (createDto.ClientId.HasValue)
                {
                    var client = await UnitOfWork.Clients.GetByIdAsync(createDto.ClientId.Value);
                    if (client == null)
                        throw new ArgumentException($"Client not found. ClientId {createDto.ClientId.Value} does not exist.", nameof(createDto.ClientId));
                    resolvedClientId = createDto.ClientId.Value;
                }

                var task = new WeeklyPlanTask
                {
                    WeeklyPlanId = (int)createDto.WeeklyPlanId,
                    Title = createDto.Title,
                    Description = createDto.Notes, // Map Notes to Description
                    ClientId = resolvedClientId,
                    ClientStatus = createDto.ClientStatus,
                    ClientName = createDto.ClientName,
                    ClientPhone = createDto.ClientPhone,
                    ClientAddress = createDto.ClientAddress,
                    ClientLocation = createDto.ClientLocation,
                    ClientClassification = createDto.ClientClassification,
                    PlannedDate = createDto.PlannedDate,
                    Notes = createDto.Notes,
                    IsActive = true
                };

                await UnitOfWork.WeeklyPlanTasks.CreateAsync(task);
                await UnitOfWork.SaveChangesAsync();

                Logger.LogInformation("  Task Created Successfully - TaskId: {TaskId}, WeeklyPlanId: {WeeklyPlanId}, Task belongs to plan owned by: {PlanEmployeeId}", 
                    task.Id, task.WeeklyPlanId, weeklyPlan.EmployeeId);

                // Reload task to get generated ID and timestamps
                var createdTask = await UnitOfWork.WeeklyPlanTasks.GetByIdAsync(task.Id);
                return await MapToDetailResponseDTO(createdTask ?? task);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating weekly plan task: {Message}. StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<List<WeeklyPlanTaskDetailResponseDTO>> CreateTasksAsync(long weeklyPlanId, List<CreateWeeklyPlanTaskDTO> tasksDto, string userId)
        {
            try
            {
                // Validate weekly plan exists and belongs to user
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(weeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found", nameof(weeklyPlanId));

                if (weeklyPlan.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to add tasks to this weekly plan");

                // OPTIMIZATION: Batch client lookups and avoid repeated database queries
                var tasksToCreate = new List<WeeklyPlanTask>();
                
                // First, resolve all client IDs (can be parallelized if needed)
                foreach (var createDto in tasksDto)
                {
                    // Client resolution: If clientName is provided, search for existing client by name
                    long? resolvedClientId = null;
                    if (!string.IsNullOrWhiteSpace(createDto.ClientName))
                    {
                        // Try to find existing client by name
                        var existingClient = await UnitOfWork.Clients.FindByNameAsync(createDto.ClientName.Trim());
                        if (existingClient != null)
                        {
                            resolvedClientId = existingClient.Id;
                            // If client found, set status to "Old" automatically
                            if (string.IsNullOrEmpty(createDto.ClientStatus))
                                createDto.ClientStatus = "Old";
                        }
                        else
                        {
                            // Client not found, treat as new client
                            if (string.IsNullOrEmpty(createDto.ClientStatus))
                                createDto.ClientStatus = "New";
                        }
                    }

                    // If clientId was explicitly provided, use it (for backward compatibility)
                    if (createDto.ClientId.HasValue)
                    {
                        var client = await UnitOfWork.Clients.GetByIdAsync(createDto.ClientId.Value);
                        if (client == null)
                            throw new ArgumentException($"Client not found for task '{createDto.Title}'. ClientId {createDto.ClientId.Value} does not exist.");
                        resolvedClientId = createDto.ClientId.Value;
                    }

                    var task = new WeeklyPlanTask
                    {
                        WeeklyPlanId = (int)weeklyPlanId,
                        Title = createDto.Title,
                        Description = createDto.Notes, // Map Notes to Description
                        ClientId = resolvedClientId,
                        ClientStatus = createDto.ClientStatus,
                        ClientName = createDto.ClientName,
                        ClientPhone = createDto.ClientPhone,
                        ClientAddress = createDto.ClientAddress,
                        ClientLocation = createDto.ClientLocation,
                        ClientClassification = createDto.ClientClassification,
                        PlannedDate = createDto.PlannedDate,
                        Notes = createDto.Notes,
                        IsActive = true
                    };

                    tasksToCreate.Add(task);
                }

                // OPTIMIZATION: Use bulk create instead of individual creates
                await UnitOfWork.WeeklyPlanTasks.CreateRangeAsync(tasksToCreate);
                await UnitOfWork.SaveChangesAsync();

                // OPTIMIZATION: Get all tasks once instead of querying N times in loop
                var savedTasks = await UnitOfWork.WeeklyPlanTasks.GetByWeeklyPlanIdAsync(weeklyPlanId);
                
                // Match tasks by title and planned date, then map to DTOs sequentially to avoid DbContext concurrency issues
                var createdTasks = new List<WeeklyPlanTaskDetailResponseDTO>();
                
                foreach (var createDto in tasksDto)
                {
                    var savedTask = savedTasks.FirstOrDefault(t => 
                        t.Title == createDto.Title && 
                        t.PlannedDate == createDto.PlannedDate &&
                        t.WeeklyPlanId == weeklyPlanId);
                    
                    if (savedTask != null)
                    {
                        createdTasks.Add(await MapToDetailResponseDTO(savedTask));
                    }
                }

                Logger.LogInformation("Created {Count} tasks for weekly plan {WeeklyPlanId}", createdTasks.Count, weeklyPlanId);

                return createdTasks;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating multiple weekly plan tasks");
                throw;
            }
        }

        public async Task<WeeklyPlanTaskDetailResponseDTO?> GetTaskAsync(long taskId, string userId, string userRole)
        {
            try
            {
                var task = await UnitOfWork.WeeklyPlanTasks.GetByIdAsync((int)taskId);
                if (task == null)
                    return null;

                // Check authorization
                if (userRole != "SalesManager" && userRole != "SuperAdmin")
                {
                    var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                    if (weeklyPlan == null || weeklyPlan.EmployeeId != userId)
                        throw new UnauthorizedAccessException("You don't have permission to view this task");
                }

                return await MapToDetailResponseDTO(task);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving weekly plan task");
                throw;
            }
        }

        public async Task<List<WeeklyPlanTaskDetailResponseDTO>> GetTasksByPlanAsync(long weeklyPlanId, string userId, string userRole)
        {
            try
            {
                // Validate weekly plan exists and user has access
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(weeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found", nameof(weeklyPlanId));

                if (userRole != "SalesManager" && userRole != "SuperAdmin" && weeklyPlan.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view tasks for this weekly plan");

                var tasks = await UnitOfWork.WeeklyPlanTasks.GetByWeeklyPlanIdAsync(weeklyPlanId);
                
                // Map sequentially to avoid DbContext concurrency issues (MapToDetailResponseDTO makes DB calls)
                var tasksList = tasks.ToList();
                var result = new List<WeeklyPlanTaskDetailResponseDTO>();

                foreach (var task in tasksList)
                {
                    result.Add(await MapToDetailResponseDTO(task));
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving tasks for weekly plan");
                throw;
            }
        }

        public async Task<WeeklyPlanTaskDetailResponseDTO?> UpdateTaskAsync(long taskId, UpdateWeeklyPlanTaskDTO updateDto, string userId)
        {
            try
            {
                var task = await UnitOfWork.WeeklyPlanTasks.GetByIdAsync((int)taskId);
                if (task == null)
                    return null;

                // Check authorization
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found for this task");

                if (weeklyPlan.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to update this task");

                // Validate client status if provided
                if (!string.IsNullOrEmpty(updateDto.ClientStatus) && !Models.ClientStatusConstants.IsValidStatus(updateDto.ClientStatus))
                    throw new ArgumentException($"Invalid client status. Must be one of: {string.Join(", ", Models.ClientStatusConstants.AllStatuses)}");

                // Validate client classification if provided
                if (!string.IsNullOrEmpty(updateDto.ClientClassification) && !Models.ClientClassificationConstants.IsValidClassification(updateDto.ClientClassification))
                    throw new ArgumentException($"Invalid client classification. Must be one of: {string.Join(", ", Models.ClientClassificationConstants.AllClassifications)}");

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateDto.Title))
                    task.Title = updateDto.Title;

                // Update ClientId only if provided - validate only if setting to a non-null value
                if (updateDto.ClientId.HasValue)
                {
                    var client = await UnitOfWork.Clients.GetByIdAsync(updateDto.ClientId.Value);
                    if (client == null)
                        throw new ArgumentException($"Client not found. ClientId {updateDto.ClientId.Value} does not exist.", nameof(updateDto.ClientId));
                    task.ClientId = updateDto.ClientId;
                }
                else if (updateDto.ClientId == null && updateDto.ClientStatus == "New")
                {
                    // If explicitly setting to New client, clear ClientId
                    task.ClientId = null;
                }

                if (!string.IsNullOrEmpty(updateDto.ClientStatus))
                    task.ClientStatus = updateDto.ClientStatus;

                if (updateDto.ClientName != null)
                    task.ClientName = updateDto.ClientName;

                if (updateDto.ClientPhone != null)
                    task.ClientPhone = updateDto.ClientPhone;

                if (updateDto.ClientAddress != null)
                    task.ClientAddress = updateDto.ClientAddress;

                if (updateDto.ClientLocation != null)
                    task.ClientLocation = updateDto.ClientLocation;

                if (!string.IsNullOrEmpty(updateDto.ClientClassification))
                    task.ClientClassification = updateDto.ClientClassification;

                if (updateDto.PlannedDate.HasValue)
                    task.PlannedDate = updateDto.PlannedDate;

                if (updateDto.Notes != null)
                    task.Notes = updateDto.Notes;

                await UnitOfWork.WeeklyPlanTasks.UpdateAsync(task);
                await UnitOfWork.SaveChangesAsync();

                Logger.LogInformation("Weekly plan task updated successfully. TaskId: {TaskId}", taskId);

                return await MapToDetailResponseDTO(task);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating weekly plan task");
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(long taskId, string userId)
        {
            try
            {
                var task = await UnitOfWork.WeeklyPlanTasks.GetByIdAsync((int)taskId);
                if (task == null)
                    return false;

                // Check authorization
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found for this task");

                if (weeklyPlan.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to delete this task");

                // Check if task has progress records - if so, we might want to prevent deletion
                var hasProgress = task.Progresses != null && task.Progresses.Any();
                if (hasProgress)
                {
                    // Soft delete by marking as inactive
                    task.IsActive = false;
                    await UnitOfWork.WeeklyPlanTasks.UpdateAsync(task);
                    Logger.LogInformation("Task {TaskId} deactivated (has progress records) instead of deleted", taskId);
                }
                else
                {
                    // Hard delete
                    await UnitOfWork.WeeklyPlanTasks.DeleteAsync((int)taskId);
                    Logger.LogInformation("Task {TaskId} deleted successfully", taskId);
                }

                await UnitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting weekly plan task");
                throw;
            }
        }

        public async Task<bool> CanModifyTaskAsync(long taskId, string userId)
        {
            try
            {
                var task = await UnitOfWork.WeeklyPlanTasks.GetByIdAsync((int)taskId);
                if (task == null)
                    return false;

                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                if (weeklyPlan == null)
                    return false;

                return weeklyPlan.EmployeeId == userId;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error checking task modification permission");
                return false;
            }
        }

        public async Task<bool> ValidateTaskAsync(CreateWeeklyPlanTaskDTO createDto)
        {
            try
            {
                // Check if weekly plan exists
                var weeklyPlan = await UnitOfWork.WeeklyPlans.GetByIdAsync((int)createDto.WeeklyPlanId);
                if (weeklyPlan == null)
                    return false;

                // Validate client ONLY if ClientStatus is "Old" and ClientId is provided
                if (createDto.ClientStatus == "Old" && createDto.ClientId.HasValue)
                {
                    var client = await UnitOfWork.Clients.GetByIdAsync(createDto.ClientId.Value);
                    if (client == null)
                        return false;
                }

                // Validate client classification if provided
                if (!string.IsNullOrEmpty(createDto.ClientClassification) && 
                    !Models.ClientClassificationConstants.IsValidClassification(createDto.ClientClassification))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating weekly plan task");
                return false;
            }
        }

        private async Task<WeeklyPlanTaskDetailResponseDTO> MapToDetailResponseDTO(WeeklyPlanTask task)
        {
            // Get offer requests from task progresses (handle null case)
            var progresses = task.Progresses ?? new List<TaskProgress>();
            var offerRequestIds = progresses
                .Where(p => p != null && p.OfferRequestId.HasValue)
                .Select(p => p.OfferRequestId ?? 0)
                .Distinct()
                .ToList();

            var offerRequests = new List<OfferRequestSimpleDTO>();
            var offers = new List<SalesOfferSimpleDTO>();
            var deals = new List<SalesDealSimpleDTO>();

            // OPTIMIZATION: Use repository methods that query by IDs instead of loading all data
            if (offerRequestIds.Any())
            {
                try
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
                catch
                {
                    // If loading related data fails, just return empty lists
                    Logger.LogWarning("Failed to load related data for task {TaskId}, returning empty lists", task.Id);
                }
            }

            return new WeeklyPlanTaskDetailResponseDTO
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
                ProgressCount = progresses.Count,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Progresses = progresses.Where(p => p != null).Select(p => new TaskProgressSimpleDTO
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
    }
}

