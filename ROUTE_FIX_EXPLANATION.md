# Route Fix Explanation

## Problem

The endpoint `/api/WeeklyPlan/salesmen` was returning a validation error:

```json
{
	"errors": {
		"id": ["The value 'salesmen' is not valid."]
	}
}
```

## Root Cause

**ASP.NET Core route ordering issue.**

The specific route `[HttpGet("salesmen")]` was defined AFTER the parameterized route `[HttpGet("{id}")]`. When the request came in:

1. Request: `GET /api/WeeklyPlan/salesmen`
2. Framework tried first: `GET /api/WeeklyPlan/{id}` (matches!)
3. Tried to parse "salesmen" as `long id`
4. Validation failed: "salesmen" is not a valid long

## Solution

**Moved specific routes BEFORE parameterized routes.**

In ASP.NET Core, route matching follows this priority:

1. ✅ **Literal routes first** (e.g., `/salesmen`, `/current`)
2. ✅ **Parameterized routes last** (e.g., `/{id}`)

### Correct Route Order

```csharp
[HttpGet]                      // GET /api/WeeklyPlan (with query params)
public async Task<IActionResult> GetWeeklyPlans(...)

[HttpGet("salesmen")]          // GET /api/WeeklyPlan/salesmen
[Authorize(Roles = "SalesManager,SuperAdmin")]
public async Task<IActionResult> GetAllSalesmen(...)

[HttpGet("current")]           // GET /api/WeeklyPlan/current
public async Task<IActionResult> GetCurrentWeeklyPlan(...)

[HttpGet("{id}")]              // GET /api/WeeklyPlan/{id}
public async Task<IActionResult> GetWeeklyPlan(long id, ...)
```

## Why This Works

When a request comes in:

- `GET /api/WeeklyPlan/salesmen` → Matches `[HttpGet("salesmen")]` ✅
- `GET /api/WeeklyPlan/current` → Matches `[HttpGet("current")]` ✅
- `GET /api/WeeklyPlan/123` → Matches `[HttpGet("{id}")]` with id=123 ✅

If specific routes came after the parameterized route, the framework would never reach them because `/{id}` would match everything first.

## Files Modified

- `SoitMed/Controllers/WeeklyPlanController.cs`
     - Moved `GetAllSalesmen()` endpoint to before `GetWeeklyPlan()` endpoint
     - Removed duplicate endpoint definition

## Testing

The endpoint is now accessible at:

```
GET http://localhost:5117/api/WeeklyPlan/salesmen
Authorization: Bearer [SALESMANAGER_TOKEN]
```

It should now appear in Swagger UI and work correctly from the frontend.

## Lesson Learned

**Always define specific/literal routes before parameterized routes in ASP.NET Core controllers.**
