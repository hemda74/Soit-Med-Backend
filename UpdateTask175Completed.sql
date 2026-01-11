-- Update task 175 to mark it as completed
UPDATE WeeklyPlanTasks
SET 
    IsCompleted = 1,
    Status = 'Completed',
    UpdatedAt = GETUTCDATE()
WHERE Id = 175;

-- Verify the update
SELECT 
    Id as TaskId,
    Title,
    IsCompleted,
    Status,
    UpdatedAt
FROM WeeklyPlanTasks
WHERE Id = 175;
