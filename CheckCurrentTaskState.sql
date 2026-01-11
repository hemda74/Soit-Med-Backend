-- Check the current state of tasks for Ahmed_Ashraf_Sales_001
SELECT 
    wp.Id as WeeklyPlanId,
    wp.Title as PlanTitle,
    wp.WeekStartDate,
    wp.WeekEndDate,
    t.Id as TaskId,
    t.Title as TaskTitle,
    t.IsCompleted,
    t.Status,
    t.CreatedAt,
    t.UpdatedAt,
    (SELECT COUNT(*) FROM WeeklyPlanTaskProgresses WHERE WeeklyPlanTaskId = t.Id) as ProgressCount
FROM WeeklyPlans wp
LEFT JOIN WeeklyPlanTasks t ON wp.Id = t.WeeklyPlanId
WHERE wp.EmployeeId = 'Ahmed_Ashraf_Sales_001'
    AND wp.WeekStartDate = '2026-01-10'
ORDER BY t.Id;
