# Sales Report Module - Frontend API Documentation

## 📋 **Overview**

This document provides comprehensive API documentation for the Sales Report module, designed for frontend developers. The module supports role-based access control with two main user types:

- **Sales Manager**: Can view all reports and rate them
- **Sales Employee (Salesman)**: Can create, view, and update their own reports

---

## 🔐 **Authentication**

All endpoints require JWT authentication. Include the token in the Authorization header:

```javascript
const headers = {
	Authorization: `Bearer ${token}`,
	'Content-Type': 'application/json',
};
```

---

## 📊 **Data Models**

### **CreateSalesReportDto**

```typescript
interface CreateSalesReportDto {
	title: string; // Required, max 100 characters
	body: string; // Required, max 2000 characters
	type: 'daily' | 'weekly' | 'monthly'; // Required, only these values allowed
	reportDate: string; // Required, format: YYYY-MM-DD
}
```

### **UpdateSalesReportDto**

```typescript
interface UpdateSalesReportDto {
	title?: string; // Optional, max 100 characters
	body?: string; // Optional, max 2000 characters
	type?: 'daily' | 'weekly' | 'monthly'; // Optional, only these values allowed
	reportDate?: string; // Optional, format: YYYY-MM-DD
}
```

### **RateSalesReportDto**

```typescript
interface RateSalesReportDto {
	rating: number; // Required, 1-5
	comment?: string; // Optional, max 500 characters
}
```

### **FilterSalesReportsDto**

```typescript
interface FilterSalesReportsDto {
	page?: number; // Optional, default: 1
	pageSize?: number; // Optional, default: 10, max: 100
	type?: 'daily' | 'weekly' | 'monthly'; // Optional filter
	startDate?: string; // Optional, format: YYYY-MM-DD
	endDate?: string; // Optional, format: YYYY-MM-DD
	employeeId?: string; // Optional, for managers to filter by employee
}
```

### **SalesReportResponseDto**

```typescript
interface SalesReportResponseDto {
	id: number;
	title: string;
	body: string;
	type: 'daily' | 'weekly' | 'monthly';
	reportDate: string; // Format: YYYY-MM-DD
	rating?: number; // 1-5, only if rated
	comment?: string; // Manager's comment, only if rated
	employeeId: string;
	employeeName: string; // Format: "FirstName LastName"
	employeeEmail: string;
	createdAt: string; // ISO 8601 format
	updatedAt: string; // ISO 8601 format
}
```

### **PaginatedSalesReportsResponseDto**

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

---

## 🚀 **API Endpoints**

### **1. Create Sales Report**

**POST** `/api/SalesReport`

**Authorization:** Salesman only

**Request Body:**

```json
{
	"title": "Daily Sales Report - January 15",
	"body": "Today we achieved excellent results with 15 new clients and $50,000 in sales. The team performed exceptionally well.",
	"type": "daily",
	"reportDate": "2024-01-15"
}
```

**Success Response (201):**

```json
{
	"success": true,
	"message": "Sales report created successfully",
	"data": {
		"id": 1,
		"title": "Daily Sales Report - January 15",
		"body": "Today we achieved excellent results with 15 new clients and $50,000 in sales. The team performed exceptionally well.",
		"type": "daily",
		"reportDate": "2024-01-15",
		"rating": null,
		"comment": null,
		"employeeId": "John_Doe_Sales_001",
		"employeeName": "John Doe",
		"employeeEmail": "john.doe@company.com",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z"
	},
	"timestamp": "2024-01-15T10:30:00Z"
}
```

**Error Responses:**

**400 Bad Request - Validation Error:**

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": [
		{
			"field": "title",
			"message": "Title is required"
		},
		{
			"field": "type",
			"message": "Type must be either 'daily' or 'weekly'"
		}
	],
	"timestamp": "2024-01-15T10:30:00Z"
}
```

**400 Bad Request - Duplicate Report:**

```json
{
	"success": false,
	"message": "A report of type 'daily' already exists for the date 2024-01-15",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

**403 Forbidden:**

```json
{
	"success": false,
	"message": "Access denied. Only Salesman role can create reports",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

**401 Unauthorized:**

```json
{
	"success": false,
	"message": "Unauthorized. Please provide a valid token",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

---

### **2. Get Sales Reports**

**GET** `/api/SalesReport`

**Authorization:** All authenticated users

- **Sales Manager**: Can see all reports
- **Salesman**: Can see only their own reports

**Query Parameters:**

```
?page=1&pageSize=10&type=daily&startDate=2024-01-01&endDate=2024-01-31&employeeId=John_Doe_Sales_001
```

**Success Response (200):**

```json
{
	"success": true,
	"message": "Sales reports retrieved successfully",
	"data": {
		"data": [
			{
				"id": 1,
				"title": "Daily Sales Report - January 15",
				"body": "Today we achieved excellent results...",
				"type": "daily",
				"reportDate": "2024-01-15",
				"rating": 5,
				"comment": "Excellent work!",
				"employeeId": "John_Doe_Sales_001",
				"employeeName": "John Doe",
				"employeeEmail": "john.doe@company.com",
				"createdAt": "2024-01-15T10:30:00Z",
				"updatedAt": "2024-01-15T10:30:00Z"
			},
			{
				"id": 2,
				"title": "Monthly Sales Report - January 2024",
				"body": "This week we exceeded our targets...",
				"type": "monthly",
				"reportDate": "2024-01-31",
				"rating": null,
				"comment": null,
				"employeeId": "Jane_Smith_Sales_002",
				"employeeName": "Jane Smith",
				"employeeEmail": "jane.smith@company.com",
				"createdAt": "2024-01-14T16:45:00Z",
				"updatedAt": "2024-01-14T16:45:00Z"
			}
		],
		"totalCount": 2,
		"page": 1,
		"pageSize": 10,
		"totalPages": 1,
		"hasNextPage": false,
		"hasPreviousPage": false
	},
	"timestamp": "2024-01-15T10:30:00Z"
}
```

**Error Responses:**

**400 Bad Request - Invalid Query Parameters:**

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": [
		{
			"field": "pageSize",
			"message": "Page size cannot exceed 100"
		},
		{
			"field": "type",
			"message": "Type must be either 'daily' or 'weekly'"
		}
	],
	"timestamp": "2024-01-15T10:30:00Z"
}
```

---

### **3. Update Sales Report**

**PUT** `/api/SalesReport/{id}`

**Authorization:** Salesman only (can only update their own reports)

**Request Body:**

```json
{
	"title": "Updated Daily Sales Report - January 15",
	"body": "Updated report content with more details about the sales performance.",
	"type": "daily",
	"reportDate": "2024-01-15"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"message": "Sales report updated successfully",
	"data": {
		"id": 1,
		"title": "Updated Daily Sales Report - January 15",
		"body": "Updated report content with more details about the sales performance.",
		"type": "daily",
		"reportDate": "2024-01-15",
		"rating": 5,
		"comment": "Excellent work!",
		"employeeId": "John_Doe_Sales_001",
		"employeeName": "John Doe",
		"employeeEmail": "john.doe@company.com",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T11:45:00Z"
	},
	"timestamp": "2024-01-15T11:45:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "Sales report not found",
	"timestamp": "2024-01-15T11:45:00Z"
}
```

**403 Forbidden - Not Owner:**

```json
{
	"success": false,
	"message": "Access denied. You can only update your own reports",
	"timestamp": "2024-01-15T11:45:00Z"
}
```

**400 Bad Request - Duplicate Report:**

```json
{
	"success": false,
	"message": "A report of type 'daily' already exists for the date 2024-01-15",
	"timestamp": "2024-01-15T11:45:00Z"
}
```

---

### **4. Delete Sales Report**

**DELETE** `/api/SalesReport/{id}`

**Authorization:** Salesman only (can only delete their own reports)

**Success Response (200):**

```json
{
	"success": true,
	"message": "Sales report deleted successfully",
	"timestamp": "2024-01-15T12:00:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "Sales report not found",
	"timestamp": "2024-01-15T12:00:00Z"
}
```

**403 Forbidden - Not Owner:**

```json
{
	"success": false,
	"message": "Access denied. You can only delete your own reports",
	"timestamp": "2024-01-15T12:00:00Z"
}
```

---

### **5. Rate Sales Report**

**POST** `/api/SalesReport/{id}/rate`

**Authorization:** SalesManager only

**Request Body:**

```json
{
	"rating": 5,
	"comment": "Excellent work! This report shows great attention to detail and provides valuable insights."
}
```

**Success Response (200):**

```json
{
	"success": true,
	"message": "Report rated successfully",
	"data": {
		"id": 1,
		"title": "Daily Sales Report - January 15",
		"body": "Today we achieved excellent results...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"rating": 5,
		"comment": "Excellent work! This report shows great attention to detail and provides valuable insights.",
		"employeeId": "John_Doe_Sales_001",
		"employeeName": "John Doe",
		"employeeEmail": "john.doe@company.com",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T12:30:00Z"
	},
	"timestamp": "2024-01-15T12:30:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "Sales report not found",
	"timestamp": "2024-01-15T12:30:00Z"
}
```

**403 Forbidden:**

```json
{
	"success": false,
	"message": "Access denied. Only SalesManager role can rate reports",
	"timestamp": "2024-01-15T12:30:00Z"
}
```

**400 Bad Request - Invalid Rating:**

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": [
		{
			"field": "rating",
			"message": "Rating must be between 1 and 5"
		}
	],
	"timestamp": "2024-01-15T12:30:00Z"
}
```

---

## 🔧 **Frontend Implementation Examples**

### **JavaScript/TypeScript Examples**

#### **1. Create Report Function**

```typescript
async function createSalesReport(
	reportData: CreateSalesReportDto
): Promise<SalesReportResponseDto> {
	try {
		const response = await fetch('/api/SalesReport', {
			method: 'POST',
			headers: {
				Authorization: `Bearer ${getToken()}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(reportData),
		});

		const result = await response.json();

		if (!response.ok) {
			throw new Error(
				result.message || 'Failed to create report'
			);
		}

		return result.data;
	} catch (error) {
		console.error('Error creating sales report:', error);
		throw error;
	}
}
```

#### **2. Get Reports with Pagination**

```typescript
async function getSalesReports(
	filters: FilterSalesReportsDto = {}
): Promise<PaginatedSalesReportsResponseDto> {
	try {
		const queryParams = new URLSearchParams();

		if (filters.page)
			queryParams.append('page', filters.page.toString());
		if (filters.pageSize)
			queryParams.append(
				'pageSize',
				filters.pageSize.toString()
			);
		if (filters.type) queryParams.append('type', filters.type);
		if (filters.startDate)
			queryParams.append('startDate', filters.startDate);
		if (filters.endDate)
			queryParams.append('endDate', filters.endDate);
		if (filters.employeeId)
			queryParams.append('employeeId', filters.employeeId);

		const response = await fetch(
			`/api/SalesReport?${queryParams}`,
			{
				method: 'GET',
				headers: {
					Authorization: `Bearer ${getToken()}`,
					'Content-Type': 'application/json',
				},
			}
		);

		const result = await response.json();

		if (!response.ok) {
			throw new Error(
				result.message || 'Failed to fetch reports'
			);
		}

		return result.data;
	} catch (error) {
		console.error('Error fetching sales reports:', error);
		throw error;
	}
}
```

#### **3. Rate Report Function**

```typescript
async function rateSalesReport(
	reportId: number,
	rating: number,
	comment?: string
): Promise<SalesReportResponseDto> {
	try {
		const response = await fetch(
			`/api/SalesReport/${reportId}/rate`,
			{
				method: 'POST',
				headers: {
					Authorization: `Bearer ${getToken()}`,
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({ rating, comment }),
			}
		);

		const result = await response.json();

		if (!response.ok) {
			throw new Error(
				result.message || 'Failed to rate report'
			);
		}

		return result.data;
	} catch (error) {
		console.error('Error rating sales report:', error);
		throw error;
	}
}
```

#### **4. Update Report Function**

```typescript
async function updateSalesReport(
	reportId: number,
	updateData: UpdateSalesReportDto
): Promise<SalesReportResponseDto> {
	try {
		const response = await fetch(`/api/SalesReport/${reportId}`, {
			method: 'PUT',
			headers: {
				Authorization: `Bearer ${getToken()}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(updateData),
		});

		const result = await response.json();

		if (!response.ok) {
			throw new Error(
				result.message || 'Failed to update report'
			);
		}

		return result.data;
	} catch (error) {
		console.error('Error updating sales report:', error);
		throw error;
	}
}
```

#### **5. Delete Report Function**

```typescript
async function deleteSalesReport(reportId: number): Promise<void> {
	try {
		const response = await fetch(`/api/SalesReport/${reportId}`, {
			method: 'DELETE',
			headers: {
				Authorization: `Bearer ${getToken()}`,
				'Content-Type': 'application/json',
			},
		});

		const result = await response.json();

		if (!response.ok) {
			throw new Error(
				result.message || 'Failed to delete report'
			);
		}
	} catch (error) {
		console.error('Error deleting sales report:', error);
		throw error;
	}
}
```

---

## 🎯 **Business Rules**

### **For Sales Employees (Salesman):**

1. Can create daily and weekly reports
2. Cannot create duplicate reports for the same date and type
3. Can only view, update, and delete their own reports
4. Cannot rate reports
5. Must provide all required fields when creating reports

### **For Sales Managers:**

1. Can view all reports from all employees
2. Can rate any report with a rating (1-5) and optional comment
3. Can filter reports by employee, date range, and type
4. Cannot create, update, or delete reports
5. Can see employee information (name, email) for each report

### **General Rules:**

1. All dates must be in YYYY-MM-DD format
2. Report types are case-insensitive but stored as lowercase
3. Pagination is required for large datasets
4. All timestamps are in UTC format
5. Reports are soft-deleted (marked as inactive)

---

## 🚨 **Error Handling**

### **Common HTTP Status Codes:**

- **200**: Success
- **201**: Created
- **400**: Bad Request (validation errors)
- **401**: Unauthorized (missing or invalid token)
- **403**: Forbidden (insufficient permissions)
- **404**: Not Found
- **500**: Internal Server Error

### **Error Response Format:**

```typescript
interface ErrorResponse {
	success: false;
	message: string;
	errors?: Array<{
		field: string;
		message: string;
	}>;
	timestamp: string;
}
```

---

## 📝 **Validation Rules**

### **CreateSalesReportDto:**

- `title`: Required, max 100 characters
- `body`: Required, max 2000 characters
- `type`: Required, must be 'daily', 'weekly', or 'monthly'
- `reportDate`: Required, valid date format (YYYY-MM-DD)

### **UpdateSalesReportDto:**

- `title`: Optional, max 100 characters
- `body`: Optional, max 2000 characters
- `type`: Optional, must be 'daily', 'weekly', or 'monthly'
- `reportDate`: Optional, valid date format (YYYY-MM-DD)

### **RateSalesReportDto:**

- `rating`: Required, integer between 1 and 5
- `comment`: Optional, max 500 characters

### **FilterSalesReportsDto:**

- `page`: Optional, minimum 1
- `pageSize`: Optional, minimum 1, maximum 100
- `type`: Optional, must be 'daily', 'weekly', or 'monthly'
- `startDate`: Optional, valid date format (YYYY-MM-DD)
- `endDate`: Optional, valid date format (YYYY-MM-DD)
- `employeeId`: Optional, valid user ID

---

## 🔄 **State Management Recommendations**

### **Redux/Context State Structure:**

```typescript
interface SalesReportState {
	reports: SalesReportResponseDto[];
	pagination: {
		currentPage: number;
		pageSize: number;
		totalCount: number;
		totalPages: number;
		hasNextPage: boolean;
		hasPreviousPage: boolean;
	};
	filters: FilterSalesReportsDto;
	loading: boolean;
	error: string | null;
	selectedReport: SalesReportResponseDto | null;
}
```

### **Actions to Implement:**

- `FETCH_REPORTS_START`
- `FETCH_REPORTS_SUCCESS`
- `FETCH_REPORTS_FAILURE`
- `CREATE_REPORT_START`
- `CREATE_REPORT_SUCCESS`
- `CREATE_REPORT_FAILURE`
- `UPDATE_REPORT_START`
- `UPDATE_REPORT_SUCCESS`
- `UPDATE_REPORT_FAILURE`
- `DELETE_REPORT_START`
- `DELETE_REPORT_SUCCESS`
- `DELETE_REPORT_FAILURE`
- `RATE_REPORT_START`
- `RATE_REPORT_SUCCESS`
- `RATE_REPORT_FAILURE`
- `SET_FILTERS`
- `CLEAR_ERRORS`

---

## 🎨 **UI/UX Recommendations**

### **For Sales Employees:**

1. **Dashboard**: Show recent reports with quick create buttons
2. **Create Form**: Clear validation messages, date picker for report date
3. **Report List**: Show only own reports with edit/delete actions
4. **Edit Form**: Pre-populate with existing data

### **For Sales Managers:**

1. **Dashboard**: Overview of all reports with filtering options
2. **Report List**: Show all reports with employee information
3. **Rating Interface**: Star rating component with comment field
4. **Filter Panel**: Date range picker, employee selector, type filter

### **Common Components:**

1. **Report Card**: Reusable component for displaying report information
2. **Pagination**: Handle large datasets efficiently
3. **Loading States**: Show spinners during API calls
4. **Error Messages**: User-friendly error display
5. **Success Notifications**: Confirm successful operations

---

## 🔧 **Testing Examples**

### **Unit Test Example:**

```typescript
describe('Sales Report API', () => {
	test('should create a sales report successfully', async () => {
		const reportData = {
			title: 'Test Report',
			body: 'Test body content',
			type: 'daily',
			reportDate: '2024-01-15',
		};

		const result = await createSalesReport(reportData);

		expect(result.id).toBeDefined();
		expect(result.title).toBe(reportData.title);
		expect(result.type).toBe(reportData.type);
	});

	test('should handle validation errors', async () => {
		const invalidData = {
			title: '', // Invalid: empty title
			body: 'Test body',
			type: 'invalid', // Invalid: wrong type
			reportDate: 'invalid-date', // Invalid: wrong format
		};

		await expect(createSalesReport(invalidData)).rejects.toThrow();
	});
});
```

---

## 📞 **Support**

For technical support or questions about the Sales Report API, please contact the backend development team.

**API Base URL:** `http://localhost:5117` (Development)
**Swagger Documentation:** `http://localhost:5117/swagger`

---

_Last Updated: January 2024_
_Version: 1.0_
