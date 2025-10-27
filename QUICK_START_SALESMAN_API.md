# Quick Start: Weekly Plan API for Salesmen

## Important Notes

The `/api/WeeklyPlan/salesmen` endpoint is **ONLY for SalesManager and SuperAdmin**. Regular salesmen will get 403 Forbidden. This is correct behavior.

## For Salesmen: Use the Main Endpoint

### Endpoint
```
GET /api/WeeklyPlan
```

### Available Filters for Salesmen

| Filter | Example | Description |
|--------|---------|-------------|
| No filters | `GET /api/WeeklyPlan` | Returns all your weekly plans |
| Date range | `GET /api/WeeklyPlan?weekStartDate=2025-11-01&weekEndDate=2025-11-30` | Returns plans in date range |
| Start date only | `GET /api/WeeklyPlan?weekStartDate=2025-11-01` | Returns plans from start date onwards |
| End date only | `GET /api/WeeklyPlan?weekEndDate=2025-11-30` | Returns plans up to end date |

## Testing in Swagger

1. Login as a Salesman
2. In Swagger UI, go to `/api/WeeklyPlan` endpoint
3. Click "Try it out"
4. Add date filters (optional):
   - `weekStartDate`: `2025-11-01`
   - `weekEndDate`: `2025-11-30`
5. Click "Execute"

## What You'll Get

Returns your weekly plans with:
- All your plans (if no filters)
- Filtered by date range (if date filters provided)
- Pagination support
- Complete task details
- Manager reviews and ratings (if reviewed)

## Security

✅ **You can only see YOUR OWN plans** - Automatic restriction  
✅ **Date filtering works** - Filter your plans by week  
❌ **Cannot see other salesmen's plans** - Access denied  
❌ **Cannot use /salesmen endpoint** - Manager only feature  

## Example Response

```json
{
  "success": true,
  "data": {
    "plans": [
      {
        "id": 41,
        "employeeId": "Ahmed_Ashraf_Sales_001",
        "weekStartDate": "2025-11-03T00:00:00",
        "weekEndDate": "2025-11-09T00:00:00",
        "title": "Week 45 Sales Plan",
        "tasks": [...]
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 10,
      "totalPages": 1
    }
  }
}
```

