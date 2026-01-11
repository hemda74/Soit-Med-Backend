-- Add Status column to WeeklyPlanTask table
ALTER TABLE WeeklyPlanTasks 
ADD Status NVARCHAR(20) NOT NULL DEFAULT 'Planned';

-- Create index on Status for better query performance
CREATE INDEX IX_WeeklyPlanTasks_Status ON WeeklyPlanTasks(Status);
