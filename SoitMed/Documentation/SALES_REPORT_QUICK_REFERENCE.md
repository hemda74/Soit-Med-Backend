# Sales Report API - Quick Reference Guide

## Quick Overview

The Sales Report API provides a complete cycle for managing sales reports with role-based access control.

### Roles

- **SalesEmployee**: Create, read, update, delete own reports
- **SalesManager**: View all reports, rate any report

### Report Types

- `daily`, `weekly`, `monthly`, `custom`

## API Endpoints Summary

| Method | Endpoint                     | Role          | Description                 |
| ------ | ---------------------------- | ------------- | --------------------------- |
| POST   | `/api/SalesReport`           | SalesEmployee | Create new report           |
| PUT    | `/api/SalesReport/{id}`      | SalesEmployee | Update own report           |
| DELETE | `/api/SalesReport/{id}`      | SalesEmployee | Delete own report           |
| GET    | `/api/SalesReport/{id}`      | All           | Get specific report         |
| GET    | `/api/SalesReport`           | All           | Get reports list (filtered) |
| POST   | `/api/SalesReport/{id}/rate` | SalesManager  | Rate a report               |

## Request/Response Examples

### Create Report

```typescript
// Request
POST /api/SalesReport
{
  "title": "Daily Sales Report",
  "body": "Report content...",
  "type": "daily",
  "reportDate": "2024-01-15"
}

// Response (201)
{
  "success": true,
  "data": { /* report object */ },
  "message": "Report created successfully"
}
```

### Get Reports with Filter

```typescript
// Request
GET /api/SalesReport?type=daily&startDate=2024-01-01&page=1&pageSize=10

// Response (200)
{
  "success": true,
  "data": {
    "data": [ /* array of reports */ ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Rate Report

```typescript
// Request
POST /api/SalesReport/123/rate
{
  "rating": 4,
  "comment": "Great work!"
}

// Response (200)
{
  "success": true,
  "data": { /* updated report with rating */ },
  "message": "Report rated successfully"
}
```

## Data Models

### SalesReportResponseDto

```typescript
{
	id: number;
	title: string;
	body: string;
	type: string;
	reportDate: string; // "YYYY-MM-DD"
	employeeId: string;
	employeeName: string;
	rating: number | null; // 1-5 stars
	comment: string | null; // Manager comment
	createdAt: string; // ISO 8601
	updatedAt: string; // ISO 8601
	isActive: boolean;
}
```

### Filter Parameters

```typescript
{
  employeeId?: string;       // Filter by employee
  startDate?: string;        // "YYYY-MM-DD"
  endDate?: string;          // "YYYY-MM-DD"
  type?: string;             // "daily" | "weekly" | "monthly" | "custom"
  page?: number;             // Default: 1
  pageSize?: number;         // Default: 10, Max: 100
}
```

## Common Error Codes

| Code | Description  | Common Causes                     |
| ---- | ------------ | --------------------------------- |
| 400  | Bad Request  | Validation errors, invalid data   |
| 401  | Unauthorized | Missing/invalid JWT token         |
| 403  | Forbidden    | Insufficient role permissions     |
| 404  | Not Found    | Report not found or no access     |
| 409  | Conflict     | Duplicate report (same type/date) |

## Validation Rules

### Create/Update Report

- `title`: Required, max 100 characters
- `body`: Required, max 2000 characters
- `type`: Required, must be "daily", "weekly", "monthly", or "custom"
- `reportDate`: Required, format "YYYY-MM-DD", cannot be future date

### Rate Report

- `rating`: Optional, 1-5 stars
- `comment`: Optional, max 500 characters
- At least one of rating or comment must be provided

## React Integration Tips

1. **Authentication**: Include JWT token in Authorization header
2. **Error Handling**: Check response.success and handle errors appropriately
3. **Pagination**: Use the pagination data for navigation controls
4. **Role-based UI**: Show/hide features based on user role
5. **Real-time Updates**: Refresh data after create/update/delete operations

## Quick Test Commands

```bash
# Create report
curl -X POST "https://api.example.com/api/SalesReport" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","body":"Content","type":"daily","reportDate":"2024-01-15"}'

# Get reports
curl -X GET "https://api.example.com/api/SalesReport?page=1&pageSize=10" \
  -H "Authorization: Bearer TOKEN"

# Rate report
curl -X POST "https://api.example.com/api/SalesReport/123/rate" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"rating":4,"comment":"Good work!"}'
```

## Notes

- All dates use "YYYY-MM-DD" format
- All timestamps are in ISO 8601 format
- Reports are soft-deleted (IsActive = false)
- Duplicate reports are prevented (same employee + type + date)
- Pagination is required for large datasets
