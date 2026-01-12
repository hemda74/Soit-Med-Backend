using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using System.Linq;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing task progress in the sales workflow
    /// </summary>
    public class TaskProgressService : ITaskProgressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOfferRequestService _offerRequestService;
        private readonly ILogger<TaskProgressService> _logger;

        public TaskProgressService(
            IUnitOfWork unitOfWork,
            IOfferRequestService offerRequestService,
            ILogger<TaskProgressService> logger)
        {
            _unitOfWork = unitOfWork;
            _offerRequestService = offerRequestService;
            _logger = logger;
        }

        #region Task Progress Management

        public async Task<TaskProgressResponseDTO> CreateProgressAsync(CreateTaskProgressDTO createDto, string userId)
        {
            try
            {
                // Validate the task exists and user has access
                var task = await _unitOfWork.WeeklyPlanTasks.GetByIdAsync(createDto.TaskId);
                if (task == null)
                    throw new ArgumentException("Task not found", nameof(createDto.TaskId));

                // Check if user can create progress for this task
                // User can create progress for their own tasks
                var weeklyPlan = await _unitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                if (weeklyPlan == null)
                    throw new ArgumentException("Weekly plan not found for this task");

                if (weeklyPlan.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You can only create progress for tasks in your own weekly plan. This task belongs to another employee's plan.");

                var progress = new TaskProgress
                {
                    TaskId = createDto.TaskId,
                    ClientId = task.ClientId,
                    EmployeeId = userId,
                    ProgressDate = createDto.ProgressDate,
                    ProgressType = createDto.ProgressType,
                    Description = createDto.Description,
                    Notes = createDto.Notes,
					VoiceDescriptionUrl = createDto.VoiceDescriptionUrl,
                    VisitResult = createDto.VisitResult,
                    NotInterestedComment = createDto.NotInterestedComment,
                    NextStep = createDto.NextStep,
                    NextFollowUpDate = createDto.NextFollowUpDate,
                    FollowUpNotes = createDto.FollowUpNotes
                };

                await _unitOfWork.TaskProgresses.CreateAsync(progress);
                await _unitOfWork.SaveChangesAsync();

                // Update task status if needed
                // Task completion is now tracked through TaskProgress existence
                // No need to update task status as it's been removed

                // Auto-create OfferRequest if client is interested and needs offer
                _logger.LogInformation(" Checking auto-create conditions: VisitResult={VisitResult}, NextStep={NextStep}, HasClientId={HasClientId}", 
                    createDto.VisitResult, createDto.NextStep, !string.IsNullOrEmpty(task.ClientId));
                
                if (createDto.VisitResult == "Interested" && createDto.NextStep == "NeedsOffer" && !string.IsNullOrEmpty(task.ClientId))
                {
                    try
                    {
                        _logger.LogInformation("Conditions met! Auto-creating OfferRequest for ProgressId: {ProgressId}, ClientId: {ClientId}", progress.Id, task.ClientId);
                        
                        // Combine description and notes for comprehensive offer request details
                        var requestDetails = new System.Text.StringBuilder();
                        
                        if (!string.IsNullOrWhiteSpace(createDto.Description))
                        {
                            requestDetails.Append(createDto.Description);
                        }
                        else
                        {
                            requestDetails.Append("Products requested from task progress");
                        }
                        
                        var specialNotes = createDto.Notes ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(createDto.FollowUpNotes))
                        {
                            if (!string.IsNullOrWhiteSpace(specialNotes))
                                specialNotes += "\n\n";
                            specialNotes += $"Follow-up Notes: {createDto.FollowUpNotes}";
                        }
                        
                        var offerRequestDto = new CreateOfferRequestDTO
                        {
                            ClientId = task.ClientId,
                            TaskProgressId = progress.Id,
                            RequestedProducts = requestDetails.ToString(),
                            SpecialNotes = string.IsNullOrWhiteSpace(specialNotes) ? null : specialNotes
                        };

                        var offerRequest = await _offerRequestService.CreateOfferRequestAsync(offerRequestDto, userId);

                        // Update progress with offer request ID
                        progress.OfferRequestId = offerRequest.Id;
                        await _unitOfWork.TaskProgresses.UpdateAsync(progress);
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("OfferRequest auto-created successfully. OfferRequestId: {OfferRequestId} for ProgressId: {ProgressId}", 
                            offerRequest.Id, progress.Id);
                    }
                    catch (Exception offerRequestEx)
                    {
                        // Log but don't fail the progress creation
                        _logger.LogError(offerRequestEx, "Failed to auto-create OfferRequest for ProgressId: {ProgressId}. Progress was still created successfully. Error: {Error}", 
                            progress.Id, offerRequestEx.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Auto-create OfferRequest skipped. VisitResult={VisitResult}, NextStep={NextStep}, HasClientId={HasClientId}", 
                        createDto.VisitResult, createDto.NextStep, !string.IsNullOrEmpty(task.ClientId));
                }

                _logger.LogInformation("Task progress created successfully. ProgressId: {ProgressId}, TaskId: {TaskId}, OfferRequestId: {OfferRequestId}", 
                    progress.Id, createDto.TaskId, progress.OfferRequestId);

                return await MapToResponseDTO(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task progress");
                throw;
            }
        }

        public async Task<TaskProgressResponseDTO> CreateProgressAndOfferRequestAsync(CreateTaskProgressWithOfferRequestDTO createDto, string userId)
        {
            try
            {
                // First create the progress
                var progressDto = new CreateTaskProgressDTO
                {
                    TaskId = createDto.TaskId,
                    ProgressDate = createDto.ProgressDate,
                    ProgressType = createDto.ProgressType,
                    Description = createDto.Description,
                    Notes = createDto.Notes,
					VoiceDescriptionUrl = createDto.VoiceDescriptionUrl,
                    VisitResult = createDto.VisitResult,
                    NotInterestedComment = createDto.NotInterestedComment,
                    NextStep = createDto.NextStep,
                    NextFollowUpDate = createDto.NextFollowUpDate,
                    FollowUpNotes = createDto.FollowUpNotes
                };

                var progress = await CreateProgressAsync(progressDto, userId);

                // If client is interested and needs offer, create offer request
                if (createDto.VisitResult == "Interested" && createDto.NextStep == "NeedsOffer")
                {
                    // Combine all relevant information for the offer request
                    var requestDetails = new System.Text.StringBuilder();
                    
                    // Use RequestedProducts if provided, otherwise use Description from progress
                    if (!string.IsNullOrWhiteSpace(createDto.RequestedProducts))
                    {
                        requestDetails.Append(createDto.RequestedProducts);
                    }
                    else if (!string.IsNullOrWhiteSpace(createDto.Description))
                    {
                        requestDetails.Append(createDto.Description);
                    }
                    else
                    {
                        requestDetails.Append("Products requested from task progress");
                    }
                    
                    // Combine special notes with follow-up notes
                    var specialNotes = createDto.SpecialNotes ?? createDto.Notes ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(createDto.FollowUpNotes))
                    {
                        if (!string.IsNullOrWhiteSpace(specialNotes))
                            specialNotes += "\n\n";
                        specialNotes += $"Follow-up Notes: {createDto.FollowUpNotes}";
                    }
                    
                    var offerRequestDto = new CreateOfferRequestDTO
                    {
                        ClientId = createDto.ClientId,
                        TaskProgressId = progress.Id,
                        RequestedProducts = requestDetails.ToString(),
                        SpecialNotes = string.IsNullOrWhiteSpace(specialNotes) ? null : specialNotes
                    };

                    var offerRequest = await _offerRequestService.CreateOfferRequestAsync(offerRequestDto, userId);

                    // Update progress with offer request ID
                    var progressEntity = await _unitOfWork.TaskProgresses.GetByIdAsync(progress.Id);
                    if (progressEntity != null)
                    {
                        progressEntity.OfferRequestId = offerRequest.Id;
                        await _unitOfWork.TaskProgresses.UpdateAsync(progressEntity);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    progress.OfferRequestId = offerRequest.Id;
                }

                return progress;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task progress with offer request");
                throw;
            }
        }

        public async Task<TaskProgressResponseDTO?> GetProgressAsync(long progressId, string userId, string userRole)
        {
            try
            {
                var progress = await _unitOfWork.TaskProgresses.GetByIdAsync(progressId);
                if (progress == null)
                    return null;

                // Check authorization
                if (userRole != "SuperAdmin" && progress.EmployeeId != userId)
                    throw new UnauthorizedAccessException("You don't have permission to view this progress");

                return await MapToResponseDTO(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task progress");
                throw;
            }
        }

        public async Task<List<TaskProgressResponseDTO>> GetProgressesByTaskAsync(int taskId, string userId, string userRole)
        {
            try
            {
                var task = await _unitOfWork.WeeklyPlanTasks.GetByIdAsync(taskId);
                if (task == null)
                    throw new ArgumentException("Task not found", nameof(taskId));

                // Check authorization
                if (userRole != "SuperAdmin")
                {
                    var taskOwnerId = task.WeeklyPlan?.EmployeeId;
                    if (string.IsNullOrEmpty(taskOwnerId))
                    {
                        var weeklyPlan = await _unitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                        taskOwnerId = weeklyPlan?.EmployeeId;
                    }

                    if (taskOwnerId != userId)
                        throw new UnauthorizedAccessException("You don't have permission to view this task's progress");
                }

                var progresses = await _unitOfWork.TaskProgresses.GetProgressesByTaskIdAsync(taskId);
                var result = new List<TaskProgressResponseDTO>();

                foreach (var progress in progresses)
                {
                    result.Add(await MapToResponseDTO(progress));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task progresses");
                throw;
            }
        }

        public async Task<List<TaskProgressResponseDTO>> GetProgressesByClientAsync(long clientId, string userId, string userRole)
        {
            try
            {
                var progresses = await _unitOfWork.TaskProgresses.GetProgressesByClientIdAsync(clientId);
                var result = new List<TaskProgressResponseDTO>();

                foreach (var progress in progresses)
                {
                    // Check authorization - SuperAdmin and SalesManager can see all progress, SalesMan can only see their own
                    if (userRole == "SuperAdmin" || userRole == "SalesManager" || progress.EmployeeId == userId)
                    {
                        result.Add(await MapToResponseDTO(progress));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client progresses");
                throw;
            }
        }

        public async Task<List<TaskProgressResponseDTO>> GetProgressesByEmployeeAsync(string employeeId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var progresses = await _unitOfWork.TaskProgresses.GetProgressesByEmployeeAsync(employeeId, startDate, endDate);
                var result = new List<TaskProgressResponseDTO>();

                foreach (var progress in progresses)
                {
                    result.Add(await MapToResponseDTO(progress));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee progresses");
                throw;
            }
        }

        public async Task<List<TaskProgressResponseDTO>> GetAllProgressesAsync(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var progresses = await _unitOfWork.TaskProgresses.GetAllProgressesAsync(startDate, endDate);
                var result = new List<TaskProgressResponseDTO>();

                foreach (var progress in progresses)
                {
                    result.Add(await MapToResponseDTO(progress));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all progresses");
                throw;
            }
        }

        public async Task<TaskProgressResponseDTO> UpdateProgressAsync(long progressId, CreateTaskProgressDTO updateDto, string userId)
        {
            try
            {
                var progress = await _unitOfWork.TaskProgresses.GetByIdAsync(progressId);
                if (progress == null)
                    throw new ArgumentException("Progress not found", nameof(progressId));

                if (!await CanModifyProgressAsync(progressId, userId))
                    throw new UnauthorizedAccessException("You don't have permission to modify this progress");

                // Update progress fields
                progress.ProgressDate = updateDto.ProgressDate;
                progress.ProgressType = updateDto.ProgressType;
                progress.Description = updateDto.Description;
                progress.Notes = updateDto.Notes;
				progress.VoiceDescriptionUrl = updateDto.VoiceDescriptionUrl;
                progress.VisitResult = updateDto.VisitResult;
                progress.NotInterestedComment = updateDto.NotInterestedComment;
                progress.NextStep = updateDto.NextStep;
                progress.NextFollowUpDate = updateDto.NextFollowUpDate;
                progress.FollowUpNotes = updateDto.FollowUpNotes;

                await _unitOfWork.TaskProgresses.UpdateAsync(progress);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task progress updated successfully. ProgressId: {ProgressId}", progressId);

                return await MapToResponseDTO(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress");
                throw;
            }
        }

        public async Task<bool> DeleteProgressAsync(long progressId, string userId)
        {
            try
            {
                var progress = await _unitOfWork.TaskProgresses.GetByIdAsync(progressId);
                if (progress == null)
                    return false;

                if (!await CanModifyProgressAsync(progressId, userId))
                    throw new UnauthorizedAccessException("You don't have permission to delete this progress");

                await _unitOfWork.TaskProgresses.DeleteAsync(progress);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Task progress deleted successfully. ProgressId: {ProgressId}", progressId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task progress");
                throw;
            }
        }

        #endregion

        #region Business Logic Methods

        public async Task<bool> ValidateProgressAsync(CreateTaskProgressDTO progressDto)
        {
            try
            {
                // Check if task exists
                var task = await _unitOfWork.WeeklyPlanTasks.GetByIdAsync(progressDto.TaskId);
                if (task == null)
                    return false;

                // Validate progress type
                if (!Models.ProgressTypeConstants.IsValidType(progressDto.ProgressType))
                    return false;

                // Validate visit result if provided
                if (!string.IsNullOrEmpty(progressDto.VisitResult) && !Models.VisitResultConstants.IsValidResult(progressDto.VisitResult))
                    return false;

                // Validate next step if provided
                if (!string.IsNullOrEmpty(progressDto.NextStep) && !Models.NextStepConstants.IsValidStep(progressDto.NextStep))
                    return false;


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating task progress");
                return false;
            }
        }

        public async Task<bool> CanModifyProgressAsync(long progressId, string userId)
        {
            try
            {
                var progress = await _unitOfWork.TaskProgresses.GetByIdAsync(progressId);
                if (progress == null)
                    return false;

                // User can modify their own progress
                if (progress.EmployeeId == userId)
                    return true;

                // Check if user is manager of the task owner
                var task = await _unitOfWork.WeeklyPlanTasks.GetByIdAsync((int)progress.TaskId);
                if (task != null)
                {
                    var weeklyPlan = await _unitOfWork.WeeklyPlans.GetByIdAsync(task.WeeklyPlanId);
                    if (weeklyPlan != null && weeklyPlan.ReviewedBy == userId)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking progress modification permission");
                return false;
            }
        }

        public async Task<List<TaskProgressSummaryDTO>> GetProgressSummaryByClientAsync(long clientId)
        {
            try
            {
                var progresses = await _unitOfWork.TaskProgresses.GetProgressesByClientIdAsync(clientId);
                return progresses.Select(p => new TaskProgressSummaryDTO
                {
                    Id = long.Parse(p.Id),
                    ProgressDate = p.ProgressDate,
                    ProgressType = p.ProgressType,
                    VisitResult = p.VisitResult,
                    NextStep = p.NextStep
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress summary for client");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<TaskProgressResponseDTO> MapToResponseDTO(TaskProgress progress)
        {
            var client = !string.IsNullOrEmpty(progress.ClientId) ? await _unitOfWork.Clients.GetByIdAsync(progress.ClientId) : null;
            var employee = await _unitOfWork.Users.GetByIdAsync(progress.EmployeeId);

            return new TaskProgressResponseDTO
            {
                Id = progress.Id,
                TaskId = progress.TaskId,
                ClientId = progress.ClientId,
                ClientName = client?.Name ?? "Unknown",
                EmployeeId = progress.EmployeeId,
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                ProgressDate = progress.ProgressDate,
                ProgressType = progress.ProgressType,
                Description = progress.Description,
				VoiceDescriptionUrl = progress.VoiceDescriptionUrl,
                VisitResult = progress.VisitResult,
                NotInterestedComment = progress.NotInterestedComment,
                NextStep = progress.NextStep,
                OfferRequestId = progress.OfferRequestId,
                OfferId = null, // Not available in current model
                DealId = null, // Not available in current model
                NextFollowUpDate = progress.NextFollowUpDate,
                CreatedAt = progress.CreatedAt
            };
        }

        #endregion
    }
}