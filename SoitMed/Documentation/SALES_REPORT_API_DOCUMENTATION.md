# Sales Report API Documentation

## Overview

The Sales Report API provides comprehensive functionality for managing sales reports in the Soit-Med hospital management system. The API supports two main user roles:

- **Salesman** (Sales Employee): Can create, read, update, and delete their own reports
- **SalesManager**: Can view all reports and rate them

## Base URL

```
http://localhost:5117/api/SalesReport
```

## Authentication

All endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

---

## Endpoints

### 1. Create Sales Report

**POST** `/api/SalesReport`

**Authorization:** Salesman only

**Description:** Creates a new sales report. Sales employees can only create reports for themselves.

**Request Body:**

```json
{
	"title": "Daily Sales Report",
	"body": "Today we achieved excellent sales results...",
	"type": "daily",
	"reportDate": "2024-01-15"
}
```

**Field Validation:**

- `title`: Required, max 100 characters
- `body`: Required, max 2000 characters
- `type`: Required, must be one of: "daily", "weekly", "monthly", "custom"
- `reportDate`: Required, cannot be in the future

**Success Response (201):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Daily Sales Report",
		"body": "Today we achieved excellent sales results...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "Nada_Nada_Sales_001",
		"employeeName": "Nada Nada",
		"rating": null,
		"comment": null,
		"createdAt": "2025-09-24T00:12:57.4636856Z",
		"updatedAt": "2025-09-24T00:12:57.4637054Z",
		"isActive": true
	},
	"message": "Report created successfully",
	"timestamp": "2025-09-24T00:12:57.7219161Z"
}
```

**Error Responses:**

- `400 Bad Request`: Validation errors
- `403 Forbidden`: User doesn't have Salesman role
- `409 Conflict`: Duplicate report (same type and date for employee)

---

### 2. Get All Sales Reports

**GET** `/api/SalesReport`

**Authorization:** Salesman (own reports) or SalesManager (all reports)

**Description:** Retrieves sales reports with optional filtering and pagination.

**Query Parameters:**

- `employeeId` (optional): Filter by specific employee ID
- `startDate` (optional): Filter reports from this date (YYYY-MM-DD)
- `endDate` (optional): Filter reports to this date (YYYY-MM-DD)
- `type` (optional): Filter by report type (daily, weekly, monthly, custom)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 100)

**Example Request:**

```
GET /api/SalesReport?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=10
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"data": [
			{
				"id": 1,
				"title": "Daily Sales Report",
				"body": "Today we achieved excellent sales results...",
				"type": "daily",
				"reportDate": "2024-01-15",
				"employeeId": "Nada_Nada_Sales_001",
				"employeeName": "Nada Nada",
				"rating": 4,
				"comment": "Good work on this report",
				"createdAt": "2025-09-24T00:12:57.4636856Z",
				"updatedAt": "2025-09-24T00:12:58.0295151Z",
				"isActive": true
			}
		],
		"totalCount": 1,
		"page": 1,
		"pageSize": 10,
		"totalPages": 1,
		"hasNextPage": false,
		"hasPreviousPage": false
	},
	"message": "Found 1 report(s)",
	"timestamp": "2025-09-24T00:12:57.9356761Z"
}
```

**Error Responses:**

- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Invalid or missing token

---

### 3. Get Sales Report by ID

**GET** `/api/SalesReport/{id}`

**Authorization:** Salesman (own reports) or SalesManager (all reports)

**Description:** Retrieves a specific sales report by its ID.

**Path Parameters:**

- `id`: The report ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Daily Sales Report",
		"body": "Today we achieved excellent sales results...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "Nada_Nada_Sales_001",
		"employeeName": "Nada Nada",
		"rating": 4,
		"comment": "Good work on this report",
		"createdAt": "2025-09-24T00:12:57.4636856Z",
		"updatedAt": "2025-09-24T00:12:58.0295151Z",
		"isActive": true
	},
	"message": "Report retrieved successfully",
	"timestamp": "2025-09-24T00:12:58.137769Z"
}
```

**Error Responses:**

- `401 Unauthorized`: Invalid or missing token
- `404 Not Found`: Report not found or no permission to view

---

### 4. Update Sales Report

**PUT** `/api/SalesReport/{id}`

**Authorization:** Salesman only (own reports)

**Description:** Updates an existing sales report. Only the report creator can update it.

**Path Parameters:**

- `id`: The report ID

**Request Body:**

```json
{
	"title": "Updated Daily Sales Report",
	"body": "Updated report content...",
	"type": "daily",
	"reportDate": "2024-01-15"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Updated Daily Sales Report",
		"body": "Updated report content...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "Nada_Nada_Sales_001",
		"employeeName": "Nada Nada",
		"rating": null,
		"comment": null,
		"createdAt": "2025-09-24T00:12:57.4636856Z",
		"updatedAt": "2025-09-24T00:12:58.0295151Z",
		"isActive": true
	},
	"message": "Report updated successfully",
	"timestamp": "2025-09-24T00:12:58.047882Z"
}
```

**Error Responses:**

- `400 Bad Request`: Validation errors
- `403 Forbidden`: User doesn't have Salesman role
- `404 Not Found`: Report not found or no permission to update
- `409 Conflict`: Duplicate report (same type and date for employee)

---

### 5. Delete Sales Report

**DELETE** `/api/SalesReport/{id}`

**Authorization:** Salesman only (own reports)

**Description:** Soft deletes a sales report. Only the report creator can delete it.

**Path Parameters:**

- `id`: The report ID

**Success Response (200):**

```json
{
	"success": true,
	"message": "Report deleted successfully",
	"timestamp": "2025-09-24T00:12:58.137769Z"
}
```

**Error Responses:**

- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: User doesn't have Salesman role
- `404 Not Found`: Report not found or no permission to delete

---

### 6. Rate Sales Report

**POST** `/api/SalesReport/{id}/rate`

**Authorization:** SalesManager only

**Description:** Rates and comments on a sales report. Only sales managers can rate reports.

**Path Parameters:**

- `id`: The report ID

**Request Body:**

```json
{
	"rating": 4,
	"comment": "Good work on this report. Keep it up!"
}
```

**Field Validation:**

- `rating`: Optional, must be between 1-5
- `comment`: Optional, max 500 characters
- At least one of rating or comment must be provided

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Daily Sales Report",
		"body": "Today we achieved excellent sales results...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "Nada_Nada_Sales_001",
		"employeeName": "Nada Nada",
		"rating": 4,
		"comment": "Good work on this report. Keep it up!",
		"createdAt": "2025-09-24T00:12:57.4636856Z",
		"updatedAt": "2025-09-24T00:12:58.137769Z",
		"isActive": true
	},
	"message": "Report rated successfully",
	"timestamp": "2025-09-24T00:12:58.137769Z"
}
```

**Error Responses:**

- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: User doesn't have SalesManager role
- `404 Not Found`: Report not found

---

## Data Models

### SalesReportResponseDto

```json
{
    "id": 1,
    "title": "string",
    "body": "string",
    "type": "daily|weekly|monthly|custom",
    "reportDate": "2024-01-15",
    "employeeId": "string",
    "employeeName": "string",
    "rating": 1-5,
    "comment": "string",
    "createdAt": "2025-09-24T00:12:57.4636856Z",
    "updatedAt": "2025-09-24T00:12:58.0295151Z",
    "isActive": true
}
```

### CreateSalesReportDto

```json
{
	"title": "string (required, max 100 chars)",
	"body": "string (required, max 2000 chars)",
	"type": "string (required, daily|weekly|monthly|custom)",
	"reportDate": "2024-01-15 (required, not future)"
}
```

### UpdateSalesReportDto

```json
{
	"title": "string (required, max 100 chars)",
	"body": "string (required, max 2000 chars)",
	"type": "string (required, daily|weekly|monthly|custom)",
	"reportDate": "2024-01-15 (required, not future)"
}
```

### RateSalesReportDto

```json
{
    "rating": 1-5,
    "comment": "string (max 500 chars)"
}
```

### FilterSalesReportsDto

```json
{
	"employeeId": "string (optional)",
	"startDate": "2024-01-01 (optional)",
	"endDate": "2024-01-31 (optional)",
	"type": "daily|weekly|monthly|custom (optional)",
	"page": 1,
	"pageSize": 10
}
```

---

## Role-Based Access Control

### Salesman (Sales Employee)

- Create reports
- Read own reports
- Update own reports
- Delete own reports
- Rate reports
- View other employees' reports

### SalesManager

- Create reports
- Read all reports
- Update reports
- Delete reports
- Rate reports
- View all employees' reports

---

## Business Rules

1. **Duplicate Prevention**: A sales employee cannot create multiple reports of the same type for the same date
2. **Date Validation**: Report dates cannot be in the future
3. **Type Validation**: Report types must be one of: daily, weekly, monthly, custom
4. **Ownership**: Sales employees can only modify their own reports
5. **Soft Delete**: Reports are soft deleted (marked as inactive) rather than physically removed
6. **Rating**: Only sales managers can rate reports
7. **Pagination**: Default page size is 10, maximum is 100

---

## Error Handling

All endpoints return consistent error responses:

```json
{
	"success": false,
	"message": "Error description",
	"errors": {
		"fieldName": ["Error message 1", "Error message 2"]
	},
	"timestamp": "2025-09-24T00:12:58.137769Z"
}
```

Common HTTP status codes:

- `200 OK`: Success
- `201 Created`: Resource created successfully
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate resource
- `500 Internal Server Error`: Server error

---

## Testing Examples

### Test User Credentials

- **Sales Employee**: `Nada@Nada.Nada3` / `Nada@Nada.Nada3`
- **Sales Manager**: `f@f.com123A5` / `f@f.com123A5`

### Sample Test Flow

1. **Login as Sales Employee**

      ```bash
      POST /api/Account/login
      {
          "userName": "Nada@Nada.Nada3",
          "password": "Nada@Nada.Nada3"
      }
      ```

2. **Create a Report**

      ```bash
      POST /api/SalesReport
      {
          "title": "Daily Sales Report",
          "body": "Today we achieved excellent results...",
          "type": "daily",
          "reportDate": "2024-01-15"
      }
      ```

3. **Login as Sales Manager**

      ```bash
      POST /api/Account/login
      {
          "userName": "f@f.com123A5",
          "password": "f@f.com123A5"
      }
      ```

4. **View All Reports**

      ```bash
      GET /api/SalesReport
      ```

5. **Rate a Report**
      ```bash
      POST /api/SalesReport/1/rate
      {
          "rating": 4,
          "comment": "Good work!"
      }
      ```

---

## Known Issues

1. **Role Assignment**: The original sales manager user (`f@f.com123A5`) may not have the SalesManager role properly assigned in the database. This prevents them from seeing all reports.

2. **Database Consistency**: There may be issues with role assignment during user creation that need to be addressed.

3. **Token Refresh**: JWT tokens may need to be refreshed to reflect role changes.

---

## Support

For technical support or questions about the Sales Report API, please contact the development team or refer to the main API documentation.

