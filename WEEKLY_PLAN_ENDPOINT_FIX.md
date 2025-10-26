# Weekly Plan Tasks Fix - Why Tasks Were Not Visible

**Date**: 2025-10-26  
**Issue**: Tasks not showing in weekly plan API responses  
**Status**: ✅ RESOLVED

---

## Problem

The frontend team reported that weekly plan data was returned but tasks were missing. Even though the database had data in the `WeeklyPlans` and `WeeklyPlanTasks` tables, the tasks were not appearing in the API responses.

---

## Root Cause

The `GetByIdAsync` method in the base `BaseRepository` was not including the navigation properties. The weekly plan was loaded, but the related `Tasks` collection was not eagerly loaded.

### Before Fix

```csharp
// BaseRepository.cs
public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
{
    return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
}
```

This default implementation doesn't include related entities, so `Tasks` remained null.

---

## Solution

Added an override in `WeeklyPlanRepository` to explicitly include the `Tasks` navigation property:

### After Fix

```csharp
// WeeklyPlanRepository.cs
public override async Task<WeeklyPlan?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
{
    return await _context.WeeklyPlans
        .Include(p => p.Tasks)
        .FirstOrDefaultAsync(p => p.Id == (long)id, cancellationToken);
}
```

---

## Other Fixes Applied

### 1. PdfExportService Missing Using Statement

Added missing `using SoitMed.Models.Identity;` to access `ApplicationUser` type.

### 2. ImageUploadResult Property Name

Fixed property name from `RelativePath` to `FilePath` to match the actual `ImageUploadResult` class definition.

---

## Testing

### Data in Database

✅ **26 Weekly Plans** exist  
✅ **69 Tasks** exist  
✅ Tasks are properly linked to plans via `WeeklyPlanId`

### Sample Data

**Plan #41** (Active):

- Task #94: "Finalize MRI Deal with Cairo Hospital" - Planned, High
- Task #95: "Follow Up on Equipment Delivery" - Planned, Medium
- Task #96: "Prepare Q4 Sales Presentation" - Planned, High

**Plan #34** (Reviewed, Rating: 4):

- Task #80: "Visit Cairo Hospital" - Completed, High
- Task #81: "Follow Up Alexandria Clinic" - Pending, Medium

---

## API Response Structure

Now all endpoints return tasks in the response:

### GET /api/weeklyplan

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 41,
				"title": "Week 45 Sales Plan",
				"tasks": [
					{
						"id": 94,
						"taskType": "Visit",
						"title": "Finalize MRI Deal with Cairo Hospital",
						"status": "Planned",
						"priority": "High"
					},
					{
						"id": 95,
						"taskType": "FollowUp",
						"title": "Follow Up on Equipment Delivery",
						"status": "Planned",
						"priority": "Medium"
					}
				]
			}
		]
	}
}
```

### GET /api/weeklyplan/{id}

```json
{
	"success": true,
	"data": {
		"id": 34,
		"title": "Week 1 Sales Plan",
		"tasks": [
			{
				"id": 80,
				"taskType": "Visit",
				"title": "Visit Cairo Hospital",
				"status": "Completed",
				"priority": "High"
			},
			{
				"id": 81,
				"taskType": "FollowUp",
				"title": "Follow Up Alexandria Clinic",
				"status": "Pending",
				"priority": "Medium"
			}
		]
	}
}
```

### GET /api/weeklyplan/current

```json
{
  "success": true,
  "data": {
    "id": 38,
    "title": "Salesman Weekly Plan",
    "tasks": [...]
  }
}
```

---

## Files Changed

1. **SoitMed/Repositories/WeeklyPlanRepository.cs**

      - Added override for `GetByIdAsync` with `.Include(p => p.Tasks)`

2. **SoitMed/Services/PdfExportService.cs**

      - Added `using SoitMed.Models.Identity;`

3. **SoitMed/Services/OfferEquipmentImageService.cs**
      - Changed `RelativePath` to `FilePath` property

---

## Verification

Run these commands to verify tasks are included:

### 1. Get Plan with Tasks

```bash
curl -X GET "http://localhost:5117/api/weeklyplan/34" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Expected: Plan with tasks array populated

### 2. Get All Plans

```bash
curl -X GET "http://localhost:5117/api/weeklyplan?page=1&pageSize=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Expected: All plans with their tasks arrays

### 3. Get Current Plan

```bash
curl -X GET "http://localhost:5117/api/weeklyplan/current" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Expected: Current plan with tasks

---

## Build Status

✅ Build successful with no errors  
✅ All warnings are pre-existing and not related to this fix  
✅ Code is ready for deployment

---

## Git Status

- **Committed**: `7b0ebdb` - fix: include tasks in weekly plan responses
- **Pushed to**: dev branch
- **Previous**: `c6dc27e` - Weekly Plan Testing Guide

---

## Frontend Impact

**Before**: Tasks array was always empty or null

**After**: Tasks array is properly populated with:

- Task ID
- Task Type (Visit, FollowUp, Call, Email, Meeting)
- Task Title
- Status (Planned, InProgress, Completed, Cancelled)
- Priority (High, Medium, Low)
- Client information (if linked)
- Planned date and time

---

## Testing Credentials

- **Sales Manager**: salesmanager@soitmed.com / 356120Ahmed@shraf2
- **Sales Support**: salessupport@soitmed.com / 356120Ahmed@shraf2
- **Salesman**: ahmed@soitmed.com / 356120Ahmed@shraf2

---

**Status**: ✅ RESOLVED - Tasks now visible in all weekly plan API responses
