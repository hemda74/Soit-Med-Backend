# Notifications Table Creation Guide

## Problem

The application throws an error:

```
Invalid object name 'Notifications'
```

This happens because the `Notifications` table doesn't exist in the database, even though the code expects it.

## Solution

Run the SQL script `CreateNotificationsTable.sql` to create the table with all required columns, indexes, and foreign keys.

## Quick Fix

### Step 1: Open SQL Server Management Studio (SSMS)

1. Connect to your database server
2. Navigate to your database: `ITIWebApi44` (or your database name)

### Step 2: Run the Script

1. Open `CreateNotificationsTable.sql`
2. **Important**: Update the database name on line 10 if needed:
      ```sql
      USE [ITIWebApi44]; -- Change to your actual database name
      ```
3. Execute the script (F5 or Execute button)

### Step 3: Verify

After running, verify the table was created:

```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'Notifications';

-- Check table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Notifications'
ORDER BY ORDINAL_POSITION;

-- Check indexes
SELECT name, type_desc
FROM sys.indexes
WHERE object_id = OBJECT_ID('Notifications');
```

## Table Structure

The `Notifications` table includes:

### Columns:

- **Id** (BIGINT, Identity, Primary Key)
- **UserId** (NVARCHAR(450), Required) - User who receives the notification
- **Title** (NVARCHAR(200), Required) - Notification title
- **Message** (NVARCHAR(1000), Required) - Notification message
- **Type** (NVARCHAR(50), Required) - "Request", "Assignment", "Update", "Reminder"
- **Priority** (NVARCHAR(50), Nullable) - "Low", "Medium", "High", "Urgent"
- **RequestWorkflowId** (BIGINT, Nullable, FK) - Links to RequestWorkflows table
- **ActivityLogId** (BIGINT, Nullable, FK) - Links to ActivityLogs table
- **IsRead** (BIT, Default: 0) - Whether notification was read
- **IsMobilePush** (BIT, Default: 0) - Whether to send mobile push notification
- **CreatedAt** (DATETIME2, Default: GETUTCDATE()) - When notification was created
- **ReadAt** (DATETIME2, Nullable) - When notification was read

### Indexes:

- **IX_Notifications_UserId_IsRead** - Composite index for filtering user notifications
- **IX_Notifications_CreatedAt** - Index for sorting by date

### Foreign Keys:

- **FK_Notifications_RequestWorkflows** - Links to RequestWorkflows.Id (ON DELETE SET NULL)
- **FK_Notifications_ActivityLogs** - Links to ActivityLogs.Id (ON DELETE SET NULL)

## Features

- **Safe**: Script can be run multiple times without errors
- **Idempotent**: Uses `IF NOT EXISTS` checks
- **Complete**: Creates all columns, indexes, and foreign keys
- **Verified**: Includes verification queries at the end

## Expected Output

When you run the script, you should see:

```
============================================================================
Notifications table created successfully.
Index IX_Notifications_UserId_IsRead created successfully.
Index IX_Notifications_CreatedAt created successfully.
Foreign key FK_Notifications_RequestWorkflows created successfully.
Foreign key FK_Notifications_ActivityLogs created successfully.

============================================================================
Notifications Table Structure Verification:
============================================================================
[Table structure displayed]

============================================================================
Indexes on Notifications Table:
============================================================================
[Indexes displayed]

============================================================================
Foreign Keys on Notifications Table:
============================================================================
[Foreign keys displayed]

============================================================================
Script execution completed successfully!
============================================================================
```

## After Creation

Once the table is created, the notification system will work properly:

1. ✅ Notifications can be saved to the database
2. ✅ Real-time notifications via SignalR will work
3. ✅ Users can retrieve notifications via API: `GET /api/Notification`
4. ✅ Unread count endpoint will work: `GET /api/Notification/unread-count`
5. ✅ Mark as read functionality will work: `PUT /api/Notification/{id}/read`

## Troubleshooting

### Error: "Foreign key constraint failed"

- **Cause**: Referenced tables (RequestWorkflows, ActivityLogs) don't exist
- **Solution**: The script handles this gracefully - foreign keys are created only if referenced tables exist

### Error: "Table already exists"

- **Cause**: Table was already created
- **Solution**: This is OK - the script skips creation if table exists

### Error: "Invalid column name" after running script

- **Cause**: Table structure doesn't match Entity Framework model
- **Solution**: Check that all columns match the Notification model in `SoitMed/Models/Notification.cs`

## Next Steps

1. Run the script
2. Restart your application
3. Test notification endpoints
4. Verify notifications are being saved

---

**Last Updated:** 2025-01-02


