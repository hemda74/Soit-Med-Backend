using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoitMed.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyPlanTaskColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to WeeklyPlanTasks table if they don't exist
            migrationBuilder.Sql(@"
                -- Add PlaceName column
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlaceName')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD PlaceName nvarchar(200) NULL;
                END

                -- Add PlaceType column
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlaceType')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD PlaceType nvarchar(50) NULL;
                END

                -- Add PlannedTime column
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlannedTime')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD PlannedTime nvarchar(20) NULL;
                END

                -- Add Priority column with DEFAULT constraint
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Priority')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD Priority nvarchar(50) NOT NULL DEFAULT 'Medium';
                END

                -- Add Purpose column
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Purpose')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD Purpose nvarchar(500) NULL;
                END

                -- Add Status column with DEFAULT constraint
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Status')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD Status nvarchar(50) NOT NULL DEFAULT 'Planned';
                END

                -- Add TaskType column with DEFAULT constraint
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'TaskType')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks ADD TaskType nvarchar(50) NOT NULL DEFAULT 'Visit';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns if they exist (for rollback)
            migrationBuilder.Sql(@"
                -- Remove TaskType column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'TaskType')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN TaskType;
                END

                -- Remove Status column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Status')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN Status;
                END

                -- Remove Purpose column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Purpose')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN Purpose;
                END

                -- Remove Priority column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'Priority')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN Priority;
                END

                -- Remove PlannedTime column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlannedTime')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN PlannedTime;
                END

                -- Remove PlaceType column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlaceType')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN PlaceType;
                END

                -- Remove PlaceName column
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlanTasks') AND name = 'PlaceName')
                BEGIN
                    ALTER TABLE WeeklyPlanTasks DROP COLUMN PlaceName;
                END
            ");
        }
    }
}
