# Finance Sales Report API - Complete Testing Guide

## Overview

This guide provides comprehensive documentation for testing all Finance Sales Report APIs with complete request/response examples and all possible error scenarios.

## Authentication Requirements

### Required Roles
- **FinanceEmployee**: For creating, updating, deleting, and viewing own reports
- **FinanceManager**: For viewing all reports and rating them
- **SuperAdmin/Admin**: For data seeding operations

### JWT Token Format
```
Authorization: Bearer <your-jwt-token>
```

---

## API Endpoints Testing Guide

### 1. Get All Finance Sales Reports (FinanceManager)

**Endpoint**: `GET /api/FinanceSalesReport/all`

#### Request Examples

**Basic Request**
```http
GET /api/FinanceSalesReport/all
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

**With Filtering**
```http
GET /api/FinanceSalesReport/all?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=10
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

#### Possible Responses

**200 OK - Success (No Reports)**
```json
{
  "success": true,
  "data": {
    "data": [],
    "totalCount": 0,
    "page": 1,
    "pageSize": 10,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "message": "Found 0 finance sales report(s)",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**200 OK - Success (With Reports)**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 1,
        "title": "Daily Financial Analysis - January 15, 2024",
        "body": "Today's financial analysis shows positive revenue trends with controlled expenses. Key highlights include increased cash flow and improved budget utilization...",
        "type": "daily",
        "reportDate": "2024-01-15",
        "employeeId": "fin-emp-001",
        "employeeName": "Ahmed Hassan",
        "rating": 4,
        "comment": "Excellent financial analysis with clear insights and recommendations.",
        "createdAt": "2024-01-15T10:30:00.000Z",
        "updatedAt": "2024-01-15T10:30:00.000Z",
        "isActive": true
      },
      {
        "id": 2,
        "title": "Weekly Financial Summary - Week 3, January 2024",
        "body": "Weekly financial performance shows strong growth with effective cost management. Revenue streams are performing well above projections...",
        "type": "weekly",
        "reportDate": "2024-01-21",
        "employeeId": "fin-emp-002",
        "employeeName": "Fatma Mohamed",
        "rating": 5,
        "comment": "Outstanding weekly analysis with comprehensive data breakdown.",
        "createdAt": "2024-01-21T14:20:00.000Z",
        "updatedAt": "2024-01-21T14:20:00.000Z",
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

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Validation failed. Please check the following fields:",
  "errors": {
    "page": ["Page must be greater than 0."],
    "pageSize": ["Page size cannot exceed 100."]
  },
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**401 Unauthorized - Missing/Invalid Token**
```json
{
  "success": false,
  "message": "Unauthorized",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**403 Forbidden - Insufficient Role**
```json
{
  "success": false,
  "message": "Access denied. FinanceManager role required.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 2. Rate Finance Sales Report (FinanceManager)

**Endpoint**: `POST /api/FinanceSalesReport/{id}/rate`

#### Request Examples

**Rate with Both Rating and Comment**
```http
POST /api/FinanceSalesReport/1/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "rating": 4,
  "comment": "Excellent financial analysis with clear insights and actionable recommendations. The revenue projections are well-supported by data."
}
```

**Rate with Only Comment**
```http
POST /api/FinanceSalesReport/1/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "comment": "Good work on the financial analysis. Consider adding more detailed cost breakdown in future reports."
}
```

**Rate with Only Rating**
```http
POST /api/FinanceSalesReport/1/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "rating": 5
}
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-001",
    "employeeName": "Ahmed Hassan",
    "rating": 4,
    "comment": "Excellent financial analysis with clear insights and actionable recommendations. The revenue projections are well-supported by data.",
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T11:45:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report rated successfully",
  "timestamp": "2024-01-15T11:45:00.000Z"
}
```

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Validation failed. Please check the following fields:",
  "errors": {
    "rating": ["Rating must be between 1 and 5."],
    "": ["Either rating or comment must be provided."]
  },
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**404 Not Found - Report Not Found**
```json
{
  "success": false,
  "message": "Report not found.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**403 Forbidden - Insufficient Role**
```json
{
  "success": false,
  "message": "Access denied. FinanceManager role required.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 3. Create Finance Sales Report (FinanceEmployee)

**Endpoint**: `POST /api/FinanceSalesReport`

#### Request Examples

**Daily Report**
```http
POST /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Daily Financial Analysis - January 15, 2024",
  "body": "Today's financial analysis shows positive revenue trends with controlled expenses. Key highlights include:\n\n1. Revenue increased by 15% compared to yesterday\n2. Operating costs remained within budget\n3. Cash flow improved by 8%\n4. New client acquisitions: 3\n\nRecommendations:\n- Continue current cost management strategies\n- Focus on high-value client acquisition\n- Monitor cash flow trends closely",
  "type": "daily",
  "reportDate": "2024-01-15"
}
```

**Weekly Report**
```http
POST /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Weekly Financial Summary - Week 3, January 2024",
  "body": "Weekly financial performance analysis covering January 15-21, 2024:\n\nRevenue Analysis:\n- Total weekly revenue: $125,000\n- Growth rate: 12% week-over-week\n- Top performing service: Equipment Sales (45% of revenue)\n\nExpense Analysis:\n- Operating expenses: $85,000\n- Cost reduction: 5% compared to previous week\n- Most significant cost: Equipment maintenance (30%)\n\nFinancial Health:\n- Profit margin: 32%\n- Cash flow: Positive $40,000\n- Outstanding receivables: $15,000\n\nRecommendations for next week:\n1. Focus on receivables collection\n2. Optimize maintenance scheduling\n3. Explore new revenue streams",
  "type": "weekly",
  "reportDate": "2024-01-21"
}
```

**Monthly Report**
```http
POST /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Monthly Financial Review - January 2024",
  "body": "Comprehensive monthly financial analysis for January 2024:\n\nExecutive Summary:\nThe month of January showed strong financial performance with significant improvements in revenue and cost management.\n\nKey Metrics:\n- Total Revenue: $485,000 (18% increase from December)\n- Operating Expenses: $320,000 (5% decrease from December)\n- Net Profit: $165,000 (35% increase)\n- Profit Margin: 34% (up from 28% in December)\n\nRevenue Breakdown:\n- Equipment Sales: 60% ($291,000)\n- Service Contracts: 25% ($121,250)\n- Maintenance Services: 15% ($72,750)\n\nCost Analysis:\n- Personnel: 45% ($144,000)\n- Equipment: 30% ($96,000)\n- Marketing: 15% ($48,000)\n- Other: 10% ($32,000)\n\nStrategic Recommendations:\n1. Increase investment in marketing (current 15% is below industry average)\n2. Expand service contract offerings\n3. Implement cost tracking system for better visibility\n4. Consider equipment upgrade to improve efficiency",
  "type": "monthly",
  "reportDate": "2024-01-31"
}
```

#### Possible Responses

**201 Created - Success**
```json
{
  "success": true,
  "data": {
    "id": 15,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends with controlled expenses...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-001",
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
    "body": ["Body is required."],
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

**403 Forbidden - Insufficient Role**
```json
{
  "success": false,
  "message": "Access denied. FinanceEmployee role required.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 4. Update Finance Sales Report (FinanceEmployee)

**Endpoint**: `PUT /api/FinanceSalesReport/{id}`

#### Request Examples

**Update Report**
```http
PUT /api/FinanceSalesReport/15
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Updated Daily Financial Analysis - January 15, 2024",
  "body": "Updated financial analysis with additional insights:\n\nToday's financial analysis shows positive revenue trends with controlled expenses. Key highlights include:\n\n1. Revenue increased by 18% compared to yesterday (updated from 15%)\n2. Operating costs remained within budget\n3. Cash flow improved by 12% (updated from 8%)\n4. New client acquisitions: 5 (updated from 3)\n\nAdditional Analysis:\n- Market trends show continued growth potential\n- Competitor analysis reveals opportunities for expansion\n- Customer satisfaction scores improved by 15%\n\nUpdated Recommendations:\n- Accelerate current cost management strategies\n- Focus on premium client acquisition\n- Implement real-time cash flow monitoring\n- Consider expanding service offerings",
  "type": "daily",
  "reportDate": "2024-01-15"
}
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 15,
    "title": "Updated Daily Financial Analysis - January 15, 2024",
    "body": "Updated financial analysis with additional insights...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-001",
    "employeeName": "Ahmed Hassan",
    "rating": null,
    "comment": null,
    "createdAt": "2024-01-15T10:30:00.000Z",
    "updatedAt": "2024-01-15T12:15:00.000Z",
    "isActive": true
  },
  "message": "Finance sales report updated successfully",
  "timestamp": "2024-01-15T12:15:00.000Z"
}
```

**404 Not Found - Report Not Found or No Permission**
```json
{
  "success": false,
  "message": "Report not found or you don't have permission to update it.",
  "timestamp": "2024-01-15T12:15:00.000Z"
}
```

---

### 5. Delete Finance Sales Report (FinanceEmployee)

**Endpoint**: `DELETE /api/FinanceSalesReport/{id}`

#### Request Examples

**Delete Report**
```http
DELETE /api/FinanceSalesReport/15
Authorization: Bearer <jwt-token>
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "message": "Finance sales report deleted successfully",
  "timestamp": "2024-01-15T12:30:00.000Z"
}
```

**404 Not Found - Report Not Found or No Permission**
```json
{
  "success": false,
  "message": "Report not found or you don't have permission to delete it.",
  "timestamp": "2024-01-15T12:30:00.000Z"
}
```

---

### 6. Get Finance Sales Report by ID (FinanceEmployee)

**Endpoint**: `GET /api/FinanceSalesReport/{id}`

#### Request Examples

**Get Specific Report**
```http
GET /api/FinanceSalesReport/15
Authorization: Bearer <jwt-token>
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "id": 15,
    "title": "Daily Financial Analysis - January 15, 2024",
    "body": "Today's financial analysis shows positive revenue trends...",
    "type": "daily",
    "reportDate": "2024-01-15",
    "employeeId": "fin-emp-001",
    "employeeName": "Ahmed Hassan",
    "rating": 4,
    "comment": "Excellent analysis with clear insights and actionable recommendations.",
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

### 7. Get Finance Sales Reports List (FinanceEmployee)

**Endpoint**: `GET /api/FinanceSalesReport`

#### Request Examples

**Basic Request**
```http
GET /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
```

**With Filtering**
```http
GET /api/FinanceSalesReport?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=5
Authorization: Bearer <jwt-token>
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 15,
        "title": "Daily Financial Analysis - January 15, 2024",
        "body": "Today's financial analysis shows positive revenue trends...",
        "type": "daily",
        "reportDate": "2024-01-15",
        "employeeId": "fin-emp-001",
        "employeeName": "Ahmed Hassan",
        "rating": 4,
        "comment": "Excellent work!",
        "createdAt": "2024-01-15T10:30:00.000Z",
        "updatedAt": "2024-01-15T10:30:00.000Z",
        "isActive": true
      }
    ],
    "totalCount": 8,
    "page": 1,
    "pageSize": 5,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": "Found 8 finance sales report(s)",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

## Data Seeding APIs

### 8. Seed Finance Sales Reports

**Endpoint**: `POST /api/DataSeeding/finance-sales-reports`

#### Request Examples

**Seed Reports**
```http
POST /api/DataSeeding/finance-sales-reports
Authorization: Bearer <jwt-token>
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "message": "Finance sales reports seeded successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**400 Bad Request - Error**
```json
{
  "success": false,
  "message": "Error seeding finance sales reports",
  "error": "No FinanceEmployee users found. Please create finance employees first.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

### 9. Seed Finance Manager Ratings

**Endpoint**: `POST /api/DataSeeding/finance-manager-ratings`

#### Request Examples

**Seed Ratings**
```http
POST /api/DataSeeding/finance-manager-ratings
Authorization: Bearer <jwt-token>
```

### 10. Seed All Finance Data

**Endpoint**: `POST /api/DataSeeding/finance-all`

#### Request Examples

**Seed All Data**
```http
POST /api/DataSeeding/finance-all
Authorization: Bearer <jwt-token>
```

#### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "message": "All finance data seeded successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**400 Bad Request - Error**
```json
{
  "success": false,
  "message": "Error seeding finance data",
  "error": "Database connection failed",
  "innerError": "Timeout expired",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

## Frontend Integration Examples

### React Hook for Finance Sales Reports

```typescript
import { useState, useEffect } from 'react';

interface FinanceSalesReport {
  id: number;
  title: string;
  body: string;
  type: string;
  reportDate: string;
  employeeId: string;
  employeeName: string;
  rating: number | null;
  comment: string | null;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

interface UseFinanceSalesReportsReturn {
  reports: FinanceSalesReport[];
  loading: boolean;
  error: string | null;
  pagination: {
    page: number;
    pageSize: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
  createReport: (data: CreateReportData) => Promise<void>;
  updateReport: (id: number, data: UpdateReportData) => Promise<void>;
  deleteReport: (id: number) => Promise<void>;
  rateReport: (id: number, data: RateReportData) => Promise<void>;
  fetchReports: (filters?: FilterData) => Promise<void>;
  fetchAllReports: (filters?: FilterData) => Promise<void>;
}

export function useFinanceSalesReports(): UseFinanceSalesReportsReturn {
  const [reports, setReports] = useState<FinanceSalesReport[]>([]);
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

  const createReport = async (data: CreateReportData) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall('/api/FinanceSalesReport', {
        method: 'POST',
        body: JSON.stringify(data)
      });
      await fetchReports();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const updateReport = async (id: number, data: UpdateReportData) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall(`/api/FinanceSalesReport/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data)
      });
      await fetchReports();
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
      await fetchReports();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const rateReport = async (id: number, data: RateReportData) => {
    setLoading(true);
    setError(null);
    try {
      await apiCall(`/api/FinanceSalesReport/${id}/rate`, {
        method: 'POST',
        body: JSON.stringify(data)
      });
      await fetchAllReports();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to rate report');
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const fetchReports = async (filters: FilterData = {}) => {
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

  const fetchAllReports = async (filters: FilterData = {}) => {
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

      const result = await apiCall(`/api/FinanceSalesReport/all?${queryParams.toString()}`);
      setReports(result.data.data);
      setPagination({
        page: result.data.page,
        pageSize: result.data.pageSize,
        totalPages: result.data.totalPages,
        hasNextPage: result.data.hasNextPage,
        hasPreviousPage: result.data.hasPreviousPage
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch all reports');
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
    rateReport,
    fetchReports,
    fetchAllReports
  };
}
```

---

## Error Handling Best Practices

### Common Error Scenarios

1. **Authentication Errors (401)**
   - Missing Authorization header
   - Invalid JWT token
   - Expired JWT token

2. **Authorization Errors (403)**
   - Insufficient role permissions
   - Trying to access/modify other users' reports

3. **Validation Errors (400)**
   - Missing required fields
   - Invalid field formats
   - Field length violations
   - Invalid enum values

4. **Not Found Errors (404)**
   - Report doesn't exist
   - User doesn't have permission to access report

5. **Conflict Errors (409)**
   - Duplicate report (same employee, date, and type)

### Frontend Error Handling

```typescript
const handleApiError = (error: any) => {
  if (error.response) {
    const { status, data } = error.response;
    
    switch (status) {
      case 400:
        // Handle validation errors
        if (data.errors) {
          Object.keys(data.errors).forEach(field => {
            console.error(`${field}: ${data.errors[field].join(', ')}`);
          });
        }
        break;
      case 401:
        // Redirect to login
        window.location.href = '/login';
        break;
      case 403:
        // Show access denied message
        alert('Access denied. You do not have permission to perform this action.');
        break;
      case 404:
        // Show not found message
        alert('Report not found or you do not have permission to view it.');
        break;
      case 409:
        // Show conflict message
        alert('A report with the same type and date already exists.');
        break;
      default:
        // Show generic error
        alert('An error occurred. Please try again.');
    }
  } else {
    // Network error
    alert('Network error. Please check your connection.');
  }
};
```

---

## Testing Checklist

### Pre-Testing Setup
- [ ] Ensure you have valid JWT tokens for different roles
- [ ] Create test finance employees if none exist
- [ ] Seed some test data using the seeding APIs

### API Testing
- [ ] Test all CRUD operations with FinanceEmployee role
- [ ] Test viewing and rating with FinanceManager role
- [ ] Test data seeding with SuperAdmin/Admin role
- [ ] Test error scenarios (invalid data, missing permissions, etc.)
- [ ] Test pagination and filtering
- [ ] Test validation rules

### Frontend Integration
- [ ] Implement React hooks
- [ ] Add error handling
- [ ] Test role-based UI visibility
- [ ] Test form validation
- [ ] Test real-time updates

This comprehensive guide provides everything needed for frontend integration and testing of the Finance Sales Report APIs.
