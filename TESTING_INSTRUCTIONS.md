# 🧪 Weekly Plan API - Testing Instructions

## ⚠️ IMPORTANT: Fix Database First!

Before testing, you **MUST** fix the database issue. The application cannot work without this!

### 🔧 Database Fix Steps:

**1. Run this SQL in SQL Server Management Studio or any SQL tool:**

```sql
-- Fix PersonalMail Column Issue
BEGIN TRANSACTION;

-- Add PersonalMail columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PersonalMail')
BEGIN
    ALTER TABLE [AspNetUsers] ADD [PersonalMail] nvarchar(max) NULL;
    PRINT '✅ Added PersonalMail to AspNetUsers';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Engineers') AND name = 'PersonalMail')
BEGIN
    ALTER TABLE [Engineers] ADD [PersonalMail] nvarchar(200) NULL;
    PRINT '✅ Added PersonalMail to Engineers';
END

-- Mark migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251001093910_AddPersonalMailFields')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251001093910_AddPersonalMailFields', '8.0.10');
    PRINT '✅ Marked migration as applied';
END

COMMIT TRANSACTION;
PRINT '✅ Database fixed!';
```

**2. Then run the Weekly Plan tables script:**

Open and execute: `SoitMed/FIX_DATABASE_MIGRATION.sql`

OR execute the quick version above from my previous message.

**3. Seed test data (Optional but recommended):**

Open and execute: `SoitMed/SEED_WEEKLY_PLANS.sql`

This will create 10 sample weekly plans for testing.

---

## 🚀 After Database is Fixed:

### Method 1: Using PowerShell Script

```powershell
# Run the test script
cd D:\Soit-Med\Soit-Med-Backend
powershell -ExecutionPolicy Bypass -File .\test_weekly_plan_api.ps1
```

### Method 2: Using Swagger UI

1. Start the application:

```bash
cd SoitMed
dotnet run
```

2. Open Swagger: `http://localhost:5117/swagger`

3. Login:

      - Endpoint: `POST /api/Account/Login`
      - Body:

      ```json
      {
      	"userName": "hemdan@hemdan.com",
      	"password": "356120Ahmed@shraf2"
      }
      ```

      - Copy the token from response

4. Authorize in Swagger:

      - Click the "Authorize" button
      - Enter: `Bearer {your-token}`
      - Click "Authorize"

5. Test Weekly Plan endpoints:
      - `GET /api/WeeklyPlan` - Get all plans
      - `POST /api/WeeklyPlan` - Create new plan
      - `GET /api/WeeklyPlan/{id}` - Get specific plan
      - etc.

---

## 📋 Test Scenarios

### Scenario 1: Create a Weekly Plan

**Request:**

```http
POST /api/WeeklyPlan
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Week Plan Oct 15-21",
  "description": "Focus on Cairo hospitals",
  "weekStartDate": "2024-10-15",
  "weekEndDate": "2024-10-21",
  "tasks": [
    {
      "title": "Visit Hospital A",
      "description": "Present new equipment",
      "displayOrder": 1
    },
    {
      "title": "Follow up Hospital B",
      "description": "Check on previous quote",
      "displayOrder": 2
    }
  ]
}
```

**Expected Response: 201 Created**

---

### Scenario 2: Add Daily Progress

**Request:**

```http
POST /api/WeeklyPlan/{planId}/progress
Authorization: Bearer {token}
Content-Type: application/json

{
  "progressDate": "2024-10-15",
  "notes": "Today I visited Hospital A successfully. They showed great interest in the new equipment.",
  "tasksWorkedOn": [1]
}
```

**Expected Response: 200 OK**

---

### Scenario 3: Mark Task as Complete

**Request:**

```http
PUT /api/WeeklyPlan/{planId}/tasks/{taskId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Visit Hospital A",
  "description": "Present new equipment",
  "isCompleted": true,
  "displayOrder": 1
}
```

**Expected Response: 200 OK**

---

### Scenario 4: Manager Reviews Plan (As Manager)

**Note:** You need to login as a SalesManager user for this.

**Request:**

```http
POST /api/WeeklyPlan/{planId}/review
Authorization: Bearer {manager-token}
Content-Type: application/json

{
  "rating": 5,
  "managerComment": "Excellent work this week! Keep it up."
}
```

**Expected Response: 200 OK**

---

## 🔍 Filter Examples

### Get Unreviewed Plans

```http
GET /api/WeeklyPlan?hasManagerReview=false&page=1&pageSize=10
```

### Get High-Rated Plans (4+ stars)

```http
GET /api/WeeklyPlan?minRating=4&page=1&pageSize=10
```

### Get Plans for Specific Date Range

```http
GET /api/WeeklyPlan?startDate=2024-10-01&endDate=2024-10-31&page=1&pageSize=10
```

### Get Plans for Specific Employee

```http
GET /api/WeeklyPlan?employeeId={userId}&page=1&pageSize=10
```

---

## ✅ Expected Test Results

After running all tests, you should see:

✅ Login successful
✅ Get all plans (paginated)
✅ Get plan by ID with details
✅ Filter plans by review status
✅ Filter plans by rating
✅ Create new plan with tasks
✅ Add task to plan
✅ Update task (mark as complete)
✅ Add daily progress

---

## 🐛 Troubleshooting

### Error: "Invalid column name 'PersonalMail'"

**Solution:** Run the database fix SQL scripts above.

### Error: "401 Unauthorized"

**Solution:** Make sure you're including the Bearer token in the Authorization header.

### Error: "A plan already exists for this week"

**Solution:** Each employee can only have ONE plan per week. Choose a different week or use a different employee.

### Error: "Task not found"

**Solution:** Make sure the task ID belongs to the specified plan.

---

## 📚 Full Documentation

For complete API documentation, see:

- `SoitMed/Documentation/WEEKLY_PLAN_API_DOCUMENTATION.md`
- `SoitMed/Documentation/WEEKLY_PLAN_IMPLEMENTATION_SUMMARY.md`

---

## 🎉 Summary

The Weekly Plan system is complete and ready to use! Just make sure to:

1. ✅ Fix database (run SQL scripts)
2. ✅ Start the application
3. ✅ Login to get token
4. ✅ Test the endpoints

**Good luck! 🚀**



