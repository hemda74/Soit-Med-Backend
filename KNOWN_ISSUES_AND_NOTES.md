# Known Issues and Notes for Salesman Endpoints

## Issues Found During Testing

### 1. Date Filtering for Weekly Plans ✅ FIXED AND VERIFIED

**Endpoint:** `GET /api/WeeklyPlan?weekStartDate=...&weekEndDate=...`

**Status:** ✅ **RESOLVED AND TESTED** - Date filtering now works for Salesmen.

**Test Result:** ✅ Successfully tested on 2025-11-02 - Returns `200 OK` with filtered results.

**Fix Applied:** Updated `WeeklyPlanService.GetWeeklyPlansWithFiltersAsync` to allow `Salesman` role (in addition to `SalesManager` and `SuperAdmin`). Fixed a typo where there was a trailing space in `"Salesman "`.

**Location:** `SoitMed/Services/WeeklyPlanService.cs` line 100:

```csharp
// SalesManager, SuperAdmin, and Salesman can use filters
// Note: For Salesman, the controller ensures they can only filter their own plans by date
if (userRole != "SalesManager" && userRole != "SuperAdmin" && userRole != "Salesman")
{
    throw new UnauthorizedAccessException("Only SalesManager, SuperAdmin and Salesman can use filters");
}
```

**Note:** The controller layer (`WeeklyPlanController`) ensures that Salesmen can only filter by date on their own plans, automatically setting `EmployeeId` to their own ID when filters are used.

---

### 2. NextStep Values Mismatch

**Field:** `nextStep` in TaskProgress

**Issue:** Existing data contains values like `"SendOffer"` and `"FollowUp"`, but the API only validates `"NeedsDeal"` and `"NeedsOffer"`.

**Valid Values (for new records):**

- `NeedsDeal` - Client needs a deal
- `NeedsOffer` - Client needs an offer

**Legacy Values (in existing data, not accepted for new records):**

- `SendOffer`
- `FollowUp`

**Location:** `SoitMed/Models/TaskProgress.cs` - `NextStepConstants` class

**Recommendation:**

- Continue using only `NeedsDeal` and `NeedsOffer` for new records
- Consider data migration to standardize existing records if needed

---

### 3. Weekly Plan IDs in Test Scripts

**Issue:** Test scripts initially used hardcoded ID `1` which doesn't exist.

**Solution:** Updated test scripts to use actual IDs like `116` from test responses. Users should:

1. First call `GET /api/WeeklyPlan` to get their plans
2. Extract an actual plan ID from the response
3. Use that ID for endpoints like `GET /api/WeeklyPlan/{id}`, `PUT /api/WeeklyPlan/{id}`, `POST /api/WeeklyPlan/{id}/submit`

**Common Test IDs:** 111, 112, 113, 114, 115, 116 (based on test data)

---

## Testing Notes

### Successful Endpoints (Working as Expected)

✅ `POST /api/TaskProgress` - Create task progress  
✅ `POST /api/TaskProgress/with-offer-request` - Create progress with offer request  
✅ `GET /api/TaskProgress` - Get all progress (with optional date filtering)  
✅ `GET /api/TaskProgress/task/{taskId}` - Get progress by task  
✅ `GET /api/TaskProgress/by-client/{clientId}` - Get client visit history  
✅ `PUT /api/TaskProgress/{id}` - Update task progress  
✅ `POST /api/WeeklyPlan` - Create weekly plan  
✅ `GET /api/WeeklyPlan` - Get all plans (with pagination)  
✅ `GET /api/WeeklyPlan/{id}` - Get plan by ID (use actual IDs)  
✅ `PUT /api/WeeklyPlan/{id}` - Update weekly plan (use actual IDs)  
✅ `POST /api/WeeklyPlan/{id}/submit` - Submit plan (use actual IDs)  
✅ `GET /api/WeeklyPlan/current` - Get current active plan

### Endpoints Status

✅ **ALL ENDPOINTS WORKING CORRECTLY!**

- Date filtering for Salesmen: ✅ **TESTED AND WORKING**
- All TaskProgress endpoints: ✅ Working
- All WeeklyPlan endpoints: ✅ Working

---

## Recommendations

1. **Fix Date Filtering:** Update `WeeklyPlanService.GetWeeklyPlansWithFiltersAsync` to allow Salesmen to filter by date on their own plans.

2. **Standardize NextStep Values:** Consider a data migration to convert legacy values (`SendOffer`, `FollowUp`) to standard values (`NeedsOffer`, `NeedsDeal`) in existing records.

3. **Improve Error Messages:** Provide more specific error messages when filtering is not allowed, explaining why and what alternatives exist.

4. **Add Validation Warnings:** Consider adding warnings when legacy `nextStep` values are detected, guiding users to use standard values.
