-- Check what's actually in the database for WeeklyPlan 126
SELECT 
    wp.Id as WeeklyPlanId,
    wp.Title as PlanTitle,
    t.Id as TaskId,
    t.Title as TaskTitle,
    t.IsCompleted,
    t.Status,
    t.CreatedAt,
    t.UpdatedAt
FROM WeeklyPlans wp
LEFT JOIN WeeklyPlanTasks t ON wp.Id = t.WeeklyPlanId
WHERE wp.Id = 126
ORDER BY t.Id;

-- Also check the summary
SELECT 
    wp.Id as WeeklyPlanId,
    wp.Title as PlanTitle,
    COUNT(t.Id) as TotalTasks,
    SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) as CompletedTasks,
    CAST(SUM(CASE WHEN t.IsCompleted = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(t.Id) as decimal(10,2)) as CompletionPercentage
FROM WeeklyPlans wp
LEFT JOIN WeeklyPlanTasks t ON wp.Id = t.WeeklyPlanId
WHERE wp.Id = 126
GROUP BY wp.Id, wp.Title;
