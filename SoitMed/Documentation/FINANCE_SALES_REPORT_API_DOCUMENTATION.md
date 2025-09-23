# Finance Sales Report API Documentation

## Overview

The Finance Sales Report API provides a specialized system for managing financial sales reports within the SoitMed system. Finance employees can create, manage, and track their financial reports, while finance managers can oversee all reports and provide ratings and feedback.

## System Architecture

### Roles and Permissions
- **FinanceEmployee**: Can create, read, update, and delete their own financial reports
- **FinanceManager**: Can view all financial reports and rate any report
- **Other Roles**: No access to finance sales report functionality

### Report Types
- `daily`: Daily financial analysis reports
- `weekly`: Weekly financial summary reports
- `monthly`: Monthly financial review reports
- `custom`: Custom period financial reports (quarterly, annual, special projects)

## API Endpoints

### 1. Create Finance Sales Report

**Endpoint**: `POST /api/FinanceSalesReport`  
**Authorization**: FinanceEmployee only  
**Description**: Creates a new financial sales report for the authenticated finance employee.

#### Request Body
```typescript
interface CreateSalesReportDto {
  title: string;        // Required, max 100 characters
  body: string;         // Required, max 2000 characters
  type: string;         // Required, one of: "daily", "weekly", "monthly", "custom"
  reportDate: string;   // Required, format: "YYYY-MM-DD", cannot be future date
}
```

#### Example Request
```http
POST /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Daily Financial Analysis - January 15, 2024",
  "body": "Today's financial analysis shows positive revenue trends with controlled expenses. Key highlights include increased cash flow and improved budget utilization...",
  "type": "daily",
  "reportDate": "2024-01-15"
}
```

#### Response Codes

**201 Created - Success**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-123",
    "employeeName": "Ahmed Hassan",
    "rating": null,
    "comment": null,
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T10:30:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report created successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Validation failed. Please check the following fields:",
  "errors": {
    "title": ["Title is required."],
    "type": ["Type must be one of: daily, weekly, monthly, custom."],
    "reportDate": ["Report date cannot be in the future."]
  },
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**409 Conflict - Duplicate Report**
```json
{
  "success": false,
  "message": "A report with the same type and date already exists for this employee.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 2. Update Finance Sales Report

**Endpoint**: `PUT /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Updates an existing financial sales report.

#### Request Body
```typescript
interface UpdateSalesReportDto {
  title: string;        // Required, max 100 characters
  body: string;         // Required, max 2000 characters
  type: string;         // Required, one of: "daily", "weekly", "monthly", "custom"
  reportDate: string;   // Required, format: "YYYY-MM-DD", cannot be future date
}
```

#### Example Request
```http
PUT /api/FinanceSalesReport/123
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Updated Daily Financial Analysis - January 15, 2024",
  "body": "Updated financial analysis with additional insights and recommendations...",
  "type": "daily",
  "reportDate": "2024-01-15"
}
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "title": "Updated Daily Financial Analysis - January 15, 2024",
    "body": "Updated financial analysis with additional insights...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-123",
    "employeeName": "Ahmed Hassan",
    "rating": null,
    "comment": null,
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T11:45:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report updated successfully",
  "timestamp": "2024-01-15T11:45:00.000Z"
}
```

**404 Not Found - Report Not Found or No Permission**
```json
{
  "success": false,
  "message": "Report not found or you don't have permission to update it.",
  "timestamp": "2024-01-15T11:45:00.000Z"
}
```

---

### 3. Delete Finance Sales Report

**Endpoint**: `DELETE /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Soft deletes a financial sales report (sets IsActive to false).

#### Example Request
```http
DELETE /api/FinanceSalesReport/123
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "message": "Finance sales report deleted successfully",
  "timestamp": "2024-01-15T12:00:00.000Z"
}
```

**404 Not Found - Report Not Found or No Permission**
```json
{
  "success": false,
  "message": "Report not found or you don't have permission to delete it.",
  "timestamp": "2024-01-15T12:00:00.000Z"
}
```

---

### 4. Get Finance Sales Report by ID

**Endpoint**: `GET /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Retrieves a specific financial sales report by ID.

#### Example Request
```http
GET /api/FinanceSalesReport/123
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-123",
    "employeeName": "Ahmed Hassan",
    "rating": 4,
    "comment": "Excellent financial analysis with clear insights and recommendations.",
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T10:30:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report retrieved successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**404 Not Found - Report Not Found or No Permission**
```json
{
  "success": false,
  "message": "Report not found or you don't have permission to view it.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 5. Get Finance Sales Reports (List with Filtering)

**Endpoint**: `GET /api/FinanceSalesReport`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Retrieves a paginated list of financial sales reports with optional filtering.

#### Query Parameters
```typescript
interface FilterSalesReportsDto {
  employeeId?: string;    // Optional, filter by specific employee
  startDate?: string;     // Optional, format: "YYYY-MM-DD"
  endDate?: string;       // Optional, format: "YYYY-MM-DD"
  type?: string;          // Optional, one of: "daily", "weekly", "monthly", "custom"
  page?: number;          // Optional, default: 1, must be > 0
  pageSize?: number;      // Optional, default: 10, must be 1-100
}
```

#### Example Request
```http
GET /api/FinanceSalesReport?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=10
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 123,
        "title": "Daily Financial Analysis - January 15, 2024",
        "body": "Today's financial analysis shows positive revenue trends...",
        "type": "daily",
        "reportDate": "2024-01-15",
        "employeeId": "fin-emp-123",
        "employeeName": "Ahmed Hassan",
        "rating": 4,
        "comment": "Excellent work!",
        "createdAt": "2024-01-15T10:30:00.000Z",
        "updatedAt": "2024-01-15T10:30:00.000Z",
        "isActive": true
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "Found 25 finance sales report(s)",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 6. Get All Finance Sales Reports (FinanceManager only)

**Endpoint**: `GET /api/FinanceSalesReport/all`  
**Authorization**: FinanceManager only  
**Description**: Retrieves all financial sales reports with optional filtering.

#### Example Request
```http
GET /api/FinanceSalesReport/all?type=monthly&startDate=2024-01-01&page=1&pageSize=20
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 123,
        "title": "Monthly Financial Review - January 2024",
        "body": "Comprehensive monthly financial analysis...",
        "type": "monthly",
        "reportDate": "2024-01-31",
        "employeeId": "fin-emp-123",
        "employeeName": "Ahmed Hassan",
        "rating": 5,
        "comment": "Outstanding monthly analysis!",
        "createdAt": "2024-01-31T10:30:00.000Z",
        "updatedAt": "2024-01-31T10:30:00.000Z",
        "isActive": true
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "Found 150 finance sales report(s)",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 7. Rate Finance Sales Report

**Endpoint**: `POST /api/FinanceSalesReport/{id}/rate`  
**Authorization**: FinanceManager only  
**Description**: Rates a financial sales report with optional comment.

#### Request Body
```typescript
interface RateSalesReportDto {
  rating?: number;       // Optional, 1-5 stars
  comment?: string;      // Optional, max 500 characters
  // Note: At least one of rating or comment must be provided
}
```

#### Example Request
```http
POST /api/FinanceSalesReport/123/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "rating": 4,
  "comment": "Excellent financial analysis with clear insights and actionable recommendations. Keep up the great work!"
}
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-123",
    "employeeName": "Ahmed Hassan",
    "rating": 4,
    "comment": "Excellent financial analysis with clear insights and actionable recommendations. Keep up the great work!",
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T10:30:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report rated successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

## Data Seeding Endpoints

### 8. Seed Finance Sales Reports

**Endpoint**: `POST /api/DataSeeding/finance-sales-reports`  
**Authorization**: SuperAdmin, Admin only  
**Description**: Seeds the database with dummy finance sales reports for existing finance employees.

#### Example Request
```http
POST /api/DataSeeding/finance-sales-reports
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**
```json
{
  "success": true,
  "message": "Finance sales reports seeded successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

### 9. Seed Finance Manager Ratings

**Endpoint**: `POST /api/DataSeeding/finance-manager-ratings`  
**Authorization**: SuperAdmin, Admin only  
**Description**: Seeds the database with dummy ratings for existing finance sales reports.

#### Example Request
```http
POST /api/DataSeeding/finance-manager-ratings
Authorization: Bearer <jwt-token>
```

### 10. Seed All Finance Data

**Endpoint**: `POST /api/DataSeeding/finance-all`  
**Authorization**: SuperAdmin, Admin only  
**Description**: Seeds all finance-related data (reports and ratings).

#### Example Request
```http
POST /api/DataSeeding/finance-all
Authorization: Bearer <jwt-token>
```

---

## Frontend Integration Examples

### React Hook for Finance Sales Reports
```typescript
import { useState, useEffect } from 'react';

interface UseFinanceSalesReportsReturn {
  reports: SalesReportResponseDto[];
  loading: boolean;
  error: string | null;
  pagination: {
    page: number;
    pageSize: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  createReport: (data: CreateSalesReportDto) => Promise<void>;
  updateReport: (id: number, data: UpdateSalesReportDto) => Promise<void>;
  deleteReport: (id: number) => Promise<void>;
  fetchReports: (filters?: FilterSalesReportsDto) => Promise<void>;
}

export function useFinanceSalesReports(): UseFinanceSalesReportsReturn {
  const [reports, setReports] = useState<SalesReportResponseDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });

  const apiCall = async (url: string, options: RequestInit = {}) => {
    const token = localStorage.getItem('authToken');
    const response = await fetch(url, {
      ...options,
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        ...options.headers
      }
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'API request failed');
    }

    return response.json();
  };

  const createReport = async (data: CreateSalesReportDto) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall('/api/FinanceSalesReport', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      await fetchReports(); // Refresh the list
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateReport = async (id: number, data: UpdateSalesReportDto) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall(`/api/FinanceSalesReport/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      await fetchReports(); // Refresh the list
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const deleteReport = async (id: number) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall(`/api/FinanceSalesReport/${id}`, {
        method: 'DELETE'
      });
      await fetchReports(); // Refresh the list
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const fetchReports = async (filters: FilterSalesReportsDto = {}) => {
    setLoading(true);
    setError(null);
    try {
      const queryParams = new URLSearchParams();
      if (filters.employeeId) queryParams.append('employeeId', filters.employeeId);
      if (filters.startDate) queryParams.append('startDate', filters.startDate);
      if (filters.endDate) queryParams.append('endDate', filters.endDate);
      if (filters.type) queryParams.append('type', filters.type);
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());

      const result = await apiCall(`/api/FinanceSalesReport?${queryParams.toString()}`);
      setReports(result.data.data);
      setPagination({
        page: result.data.page,
        pageSize: result.data.pageSize,
        totalPages: result.data.totalPages,
        hasNextPage: result.data.hasNextPage,
        hasPreviousPage: result.data.hasPreviousPage
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch reports');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, []);

  return {
    reports,
    loading,
    error,
    pagination,
    createReport,
    updateReport,
    deleteReport,
    fetchReports
  };
}
```

---

## Business Rules

### Report Creation
- Only FinanceEmployee role can create reports
- Each employee can have only one report per type per date
- Report date cannot be in the future
- All fields are required and have length limits

### Report Management
- Finance employees can only modify their own reports
- Finance managers can view all reports but cannot modify them
- Reports are soft-deleted (IsActive = false)

### Report Rating
- Only FinanceManager role can rate reports
- Rating must be between 1-5 stars
- Either rating or comment (or both) must be provided
- Rating can be updated by managers

### Data Filtering
- Finance employees see only their own reports
- Finance managers see all reports
- Filtering by date range, type, and employee is supported
- Pagination is required for large datasets

---

## Testing Examples

### cURL Commands

**Create Finance Report**
```bash
curl -X POST "https://your-api-domain.com/api/FinanceSalesReport" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Daily Financial Analysis",
    "body": "Financial analysis content here",
    "type": "daily",
    "reportDate": "2024-01-15"
  }'
```

**Get Finance Reports**
```bash
curl -X GET "https://your-api-domain.com/api/FinanceSalesReport?type=daily&page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Rate Finance Report**
```bash
curl -X POST "https://your-api-domain.com/api/FinanceSalesReport/123/rate" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 4,
    "comment": "Great financial analysis!"
  }'
```

**Seed Finance Data**
```bash
curl -X POST "https://your-api-domain.com/api/DataSeeding/finance-all" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Version History

- **v1.0** (2024-01-15): Initial implementation with full CRUD operations and rating system for finance employees

---

## Support

For additional support or to report issues with the Finance Sales Report API, please contact the development team or refer to the main API documentation.
