-- Check what the backend SHOULD be calculating for WeeklyPlan 126
SELECT 
    wp.Id as WeeklyPlanId,
    wp.Title as PlanTitle,
    COUNT(t.Id) as TotalTasks,
    SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) as CompletedTasks,
    CAST(SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(t.Id), 0) as decimal(10,2)) as CompletionPercentage,
    -- Also check the individual tasks
    STRING_AGG(
        CONCAT('Task', t.Id, ':', CASE WHEN t.IsCompleted = 1 THEN 'Completed' ELSE 'NotCompleted' END), 
        ', '
    ) as TaskStatuses
FROM WeeklyPlans wp
LEFT JOIN WeeklyPlanTasks t ON wp.Id = t.WeeklyPlanId
WHERE wp.Id = 126
GROUP BY wp.Id, wp.Title;
