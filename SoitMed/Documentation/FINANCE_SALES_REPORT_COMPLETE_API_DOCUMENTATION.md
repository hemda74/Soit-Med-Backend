# Finance Sales Report API - Complete Documentation with All Responses

## Overview

This document provides comprehensive API documentation for the Finance Sales Report system, including all possible request/response scenarios for frontend integration.

## Authentication

All endpoints require JWT authentication:
```
Authorization: Bearer <your-jwt-token>
```

## API Endpoints

### 1. Data Seeding APIs

#### 1.1 Seed All Finance Data

**Endpoint**: `POST /api/DataSeeding/finance-all`  
**Authorization**: SuperAdmin, Admin only  
**Description**: Seeds the database with dummy finance sales reports and ratings.

##### Request
```http
POST /api/DataSeeding/finance-all
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

##### Possible Responses

**200 OK - Success**
```json
{
  "success": true,
  "message": "All finance data seeded successfully",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**400 Bad Request - No Finance Employees**
```json
{
  "success": false,
  "message": "Error seeding finance data",
  "error": "No FinanceEmployee users found. Please create finance employees first.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**400 Bad Request - Database Error**
```json
{
  "success": false,
  "message": "Error seeding finance data",
  "error": "Database connection failed",
  "innerError": "Timeout expired",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**401 Unauthorized - Invalid Token**
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
  "message": "Access denied. SuperAdmin or Admin role required.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 2. Finance Manager APIs

#### 2.1 Get All Finance Sales Reports

**Endpoint**: `GET /api/FinanceSalesReport/all`  
**Authorization**: FinanceManager only  
**Description**: Retrieves all finance sales reports with optional filtering.

##### Request Examples

**Basic Request**
```http
GET /api/FinanceSalesReport/all
Authorization: Bearer <jwt-token>
```

**With Filtering**
```http
GET /api/FinanceSalesReport/all?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=10
Authorization: Bearer <jwt-token>
```

##### Possible Responses

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
        "body": "Today's financial analysis shows positive revenue trends with controlled expenses. Key highlights include increased cash flow and improved budget utilization. Revenue streams are performing well above projections with effective cost management initiatives showing positive results.",
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
        "body": "Weekly financial performance shows strong growth with effective cost management. Revenue streams are performing well above projections with detailed financial review indicating stable growth patterns.",
        "type": "weekly",
        "reportDate": "2024-01-21",
        "employeeId": "fin-emp-002",
        "employeeName": "Fatma Mohamed",
        "rating": 5,
        "comment": "Outstanding weekly analysis with comprehensive data breakdown.",
        "createdAt": "2024-01-21T14:20:00.000Z",
        "updatedAt": "2024-01-21T14:20:00.000Z",
        "isActive": true
      },
      {
        "id": 3,
        "title": "Monthly Financial Review - January 2024",
        "body": "Comprehensive monthly financial analysis reveals excellent performance with strong cash flow management and effective cost control measures. Budget variance analysis shows minor deviations within acceptable limits.",
        "type": "monthly",
        "reportDate": "2024-01-31",
        "employeeId": "fin-emp-003",
        "employeeName": "Omar Ali",
        "rating": null,
        "comment": null,
        "createdAt": "2024-01-31T16:45:00.000Z",
        "updatedAt": "2024-01-31T16:45:00.000Z",
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
    "pageSize": ["Page size cannot exceed 100."],
    "startDate": ["Start date must be less than or equal to end date."]
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

#### 2.2 Rate Finance Sales Report

**Endpoint**: `POST /api/FinanceSalesReport/{id}/rate`  
**Authorization**: FinanceManager only  
**Description**: Rates a finance sales report with optional comment.

##### Request Examples

**Rate with Both Rating and Comment**
```http
POST /api/FinanceSalesReport/1/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "rating": 4,
  "comment": "Excellent financial analysis with clear insights and actionable recommendations. The revenue projections are well-supported by data and the cost analysis is thorough."
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

##### Possible Responses

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
    "comment": "Excellent financial analysis with clear insights and actionable recommendations. The revenue projections are well-supported by data and the cost analysis is thorough.",
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
    "comment": ["Comment cannot exceed 500 characters."],
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

### 3. Finance Employee APIs

#### 3.1 Create Finance Sales Report

**Endpoint**: `POST /api/FinanceSalesReport`  
**Authorization**: FinanceEmployee only  
**Description**: Creates a new finance sales report.

##### Request Examples

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

**Custom Report**
```http
POST /api/FinanceSalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Quarterly Financial Report - Q1 2024",
  "body": "Quarterly financial analysis for Q1 2024 (January-March):\n\nExecutive Summary:\nQ1 2024 demonstrated exceptional financial performance with record-breaking revenue and improved operational efficiency.\n\nQuarterly Metrics:\n- Total Revenue: $1,450,000 (25% increase from Q4 2023)\n- Operating Expenses: $950,000 (8% decrease from Q4 2023)\n- Net Profit: $500,000 (65% increase)\n- Profit Margin: 34.5% (up from 25% in Q4 2023)\n\nMonthly Breakdown:\n- January: $485,000\n- February: $480,000\n- March: $485,000\n\nKey Achievements:\n1. Exceeded quarterly revenue target by 15%\n2. Reduced operational costs by 8%\n3. Improved profit margin by 9.5%\n4. Successfully launched new service offerings\n\nStrategic Initiatives:\n- Implemented new cost management system\n- Expanded client base by 20%\n- Launched digital transformation initiatives\n- Enhanced financial reporting capabilities\n\nRecommendations for Q2:\n1. Continue cost optimization efforts\n2. Invest in technology infrastructure\n3. Expand market reach\n4. Focus on client retention",
  "type": "custom",
  "reportDate": "2024-03-31"
}
```

##### Possible Responses

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
    "title": ["Title is required.", "Title cannot exceed 100 characters."],
    "body": ["Body is required.", "Body cannot exceed 2000 characters."],
    "type": ["Type is required.", "Type must be one of: daily, weekly, monthly, custom."],
    "reportDate": ["Report date is required.", "Report date cannot be in the future."]
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
  "message": "Access denied. FinanceEmployee role required.",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

#### 3.2 Update Finance Sales Report

**Endpoint**: `PUT /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Updates an existing finance sales report.

##### Request Examples

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

##### Possible Responses

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
  "timestamp": "2024-01-15T12:15:00.000Z"
}
```

#### 3.3 Delete Finance Sales Report

**Endpoint**: `DELETE /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Soft deletes a finance sales report.

##### Request Examples

**Delete Report**
```http
DELETE /api/FinanceSalesReport/15
Authorization: Bearer <jwt-token>
```

##### Possible Responses

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

**401 Unauthorized - Missing/Invalid Token**
```json
{
  "success": false,
  "message": "Unauthorized",
  "timestamp": "2024-01-15T12:30:00.000Z"
}
```

**403 Forbidden - Insufficient Role**
```json
{
  "success": false,
  "message": "Access denied. FinanceEmployee role required.",
  "timestamp": "2024-01-15T12:30:00.000Z"
}
```

#### 3.4 Get Finance Sales Report by ID

**Endpoint**: `GET /api/FinanceSalesReport/{id}`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Retrieves a specific finance sales report.

##### Request Examples

**Get Specific Report**
```http
GET /api/FinanceSalesReport/15
Authorization: Bearer <jwt-token>
```

##### Possible Responses

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

#### 3.5 Get Finance Sales Reports List

**Endpoint**: `GET /api/FinanceSalesReport`  
**Authorization**: FinanceEmployee only (own reports)  
**Description**: Retrieves a paginated list of finance sales reports with optional filtering.

##### Request Examples

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

##### Possible Responses

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
      },
      {
        "id": 16,
        "title": "Weekly Financial Summary - Week 3, January 2024",
        "body": "Weekly financial performance shows strong growth...",
        "type": "weekly",
        "reportDate": "2024-01-21",
        "employeeId": "fin-emp-001",
        "employeeName": "Ahmed Hassan",
        "rating": 5,
        "comment": "Outstanding analysis!",
        "createdAt": "2024-01-21T14:20:00.000Z",
        "updatedAt": "2024-01-21T14:20:00.000Z",
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

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Validation failed. Please check the following fields:",
  "errors": {
    "page": ["Page must be greater than 0."],
    "pageSize": ["Page size cannot exceed 100."],
    "startDate": ["Start date must be less than or equal to end date."]
  },
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

## Data Models

### SalesReportResponseDto
```typescript
interface SalesReportResponseDto {
  id: number;
  title: string;
  body: string;
  type: string;                    // "daily" | "weekly" | "monthly" | "custom"
  reportDate: string;              // Format: "YYYY-MM-DD"
  employeeId: string;
  employeeName: string;
  rating: number | null;           // 1-5 stars, null if not rated
  comment: string | null;          // Manager's comment, null if not provided
  createdAt: string;               // ISO 8601 format
  updatedAt: string;               // ISO 8601 format
  isActive: boolean;
}
```

### PaginatedSalesReportsResponseDto
```typescript
interface PaginatedSalesReportsResponseDto {
  data: SalesReportResponseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
```

### CreateSalesReportDto
```typescript
interface CreateSalesReportDto {
  title: string;                   // Required, max 100 characters
  body: string;                    // Required, max 2000 characters
  type: string;                    // Required, one of: "daily", "weekly", "monthly", "custom"
  reportDate: string;              // Required, format: "YYYY-MM-DD", cannot be future date
}
```

### UpdateSalesReportDto
```typescript
interface UpdateSalesReportDto {
  title: string;                   // Required, max 100 characters
  body: string;                    // Required, max 2000 characters
  type: string;                    // Required, one of: "daily", "weekly", "monthly", "custom"
  reportDate: string;              // Required, format: "YYYY-MM-DD", cannot be future date
}
```

### RateSalesReportDto
```typescript
interface RateSalesReportDto {
  rating?: number;                 // Optional, 1-5 stars
  comment?: string;                // Optional, max 500 characters
  // Note: At least one of rating or comment must be provided
}
```

### FilterSalesReportsDto
```typescript
interface FilterSalesReportsDto {
  employeeId?: string;             // Optional, filter by specific employee
  startDate?: string;              // Optional, format: "YYYY-MM-DD"
  endDate?: string;                // Optional, format: "YYYY-MM-DD"
  type?: string;                   // Optional, one of: "daily", "weekly", "monthly", "custom"
  page?: number;                   // Optional, default: 1, must be > 0
  pageSize?: number;               // Optional, default: 10, must be 1-100
}
```

---

## Error Handling

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

### Error Response Format
```typescript
interface ApiErrorResponse {
  success: false;
  message: string;
  errors?: Record<string, string[]>;  // For validation errors
  error?: string;                     // For general errors
  innerError?: string;                // For detailed error information
  timestamp: string;
}
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
  rateReport: (id: number, data: RateSalesReportDto) => Promise<void>;
  fetchReports: (filters?: FilterSalesReportsDto) => Promise<void>;
  fetchAllReports: (filters?: FilterSalesReportsDto) => Promise<void>;
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
      await fetchReports();
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

  const rateReport = async (id: number, data: RateSalesReportDto) => {
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

  const fetchAllReports = async (filters: FilterSalesReportsDto = {}) => {
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

## Testing Commands

### cURL Commands

**Seed Finance Data**
```bash
curl -X POST "http://localhost:5117/api/DataSeeding/finance-all" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

**Get All Finance Reports**
```bash
curl -X GET "http://localhost:5117/api/FinanceSalesReport/all?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Create Finance Report**
```bash
curl -X POST "http://localhost:5117/api/FinanceSalesReport" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Daily Financial Analysis - 2024-01-15",
    "body": "Today financial analysis shows positive trends...",
    "type": "daily",
    "reportDate": "2024-01-15"
  }'
```

**Rate Finance Report**
```bash
curl -X POST "http://localhost:5117/api/FinanceSalesReport/1/rate" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 4,
    "comment": "Excellent analysis!"
  }'
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

This comprehensive documentation provides everything needed for frontend integration and testing of the Finance Sales Report APIs.
