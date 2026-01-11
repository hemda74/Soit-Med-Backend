-- Check progress records for task 175
SELECT 
    t.Id as TaskId,
    t.Title as TaskTitle,
    t.IsCompleted,
    t.Status,
    p.Id as ProgressId,
    p.ProgressDate,
    p.ProgressType,
    p.Description,
    p.CreatedAt
FROM WeeklyPlanTasks t
LEFT JOIN WeeklyPlanTaskProgresses p ON t.Id = p.WeeklyPlanTaskId
WHERE t.Id = 175
ORDER BY p.CreatedAt;
