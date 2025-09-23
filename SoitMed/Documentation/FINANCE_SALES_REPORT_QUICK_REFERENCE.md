# Finance Sales Report API - Quick Reference Guide

## Quick Overview

The Finance Sales Report API provides specialized financial reporting functionality for finance employees and managers within the SoitMed system.

### Roles

- **FinanceEmployee**: Create, read, update, delete own financial reports
- **FinanceManager**: View all financial reports, rate any report

### Report Types

- `daily`, `weekly`, `monthly`, `custom`

## API Endpoints Summary

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/FinanceSalesReport` | FinanceEmployee | Create new financial report |
| PUT | `/api/FinanceSalesReport/{id}` | FinanceEmployee | Update own report |
| DELETE | `/api/FinanceSalesReport/{id}` | FinanceEmployee | Delete own report |
| GET | `/api/FinanceSalesReport/{id}` | FinanceEmployee | Get specific report |
| GET | `/api/FinanceSalesReport` | FinanceEmployee | Get own reports list (filtered) |
| GET | `/api/FinanceSalesReport/all` | FinanceManager | Get all financial reports |
| POST | `/api/FinanceSalesReport/{id}/rate` | FinanceManager | Rate a report |

## Data Seeding Endpoints

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| POST | `/api/DataSeeding/finance-sales-reports` | SuperAdmin, Admin | Seed dummy financial reports |
| POST | `/api/DataSeeding/finance-manager-ratings` | SuperAdmin, Admin | Seed dummy ratings |
| POST | `/api/DataSeeding/finance-all` | SuperAdmin, Admin | Seed all finance data |

## Request/Response Examples

### Create Financial Report

```typescript
// Request
POST /api/FinanceSalesReport
{
  "title": "Daily Financial Analysis",
  "body": "Financial analysis content...",
  "type": "daily",
  "reportDate": "2024-01-15"
}

// Response (201)
{
  "success": true,
  "data": { /* report object */ },
  "message": "Finance sales report created successfully"
}
```

### Get Financial Reports

```typescript
// Request
GET /api/FinanceSalesReport?type=daily&startDate=2024-01-01&page=1&pageSize=10

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

### Rate Financial Report

```typescript
// Request
POST /api/FinanceSalesReport/123/rate
{
  "rating": 4,
  "comment": "Excellent financial analysis!"
}

// Response (200)
{
  "success": true,
  "data": { /* updated report with rating */ },
  "message": "Finance sales report rated successfully"
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
  reportDate: string;        // "YYYY-MM-DD"
  employeeId: string;
  employeeName: string;
  rating: number | null;     // 1-5 stars
  comment: string | null;    // Manager comment
  createdAt: string;         // ISO 8601
  updatedAt: string;         // ISO 8601
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

| Code | Description | Common Causes |
|------|-------------|---------------|
| 400 | Bad Request | Validation errors, invalid data |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | Insufficient role permissions |
| 404 | Not Found | Report not found or no access |
| 409 | Conflict | Duplicate report (same type/date) |

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
# Create financial report
curl -X POST "https://api.example.com/api/FinanceSalesReport" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Daily Financial Analysis","body":"Content","type":"daily","reportDate":"2024-01-15"}'

# Get financial reports
curl -X GET "https://api.example.com/api/FinanceSalesReport?page=1&pageSize=10" \
  -H "Authorization: Bearer TOKEN"

# Rate financial report
curl -X POST "https://api.example.com/api/FinanceSalesReport/123/rate" \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"rating":4,"comment":"Great analysis!"}'

# Seed finance data
curl -X POST "https://api.example.com/api/DataSeeding/finance-all" \
  -H "Authorization: Bearer TOKEN"
```

## Sample Financial Report Content

### Daily Reports
- Revenue analysis
- Expense tracking
- Cash flow monitoring
- Budget variance analysis

### Weekly Reports
- Financial performance summary
- Cost analysis
- Revenue trends
- Budget utilization

### Monthly Reports
- Comprehensive financial review
- Profit/loss analysis
- Budget performance
- Financial recommendations

### Custom Reports
- Quarterly reviews
- Annual budgets
- Special project analysis
- Financial audits

## Notes

- All dates use "YYYY-MM-DD" format
- All timestamps are in ISO 8601 format
- Reports are soft-deleted (IsActive = false)
- Duplicate reports are prevented (same employee + type + date)
- Pagination is required for large datasets
- Finance employees can only see their own reports
- Finance managers can see all reports and rate them
