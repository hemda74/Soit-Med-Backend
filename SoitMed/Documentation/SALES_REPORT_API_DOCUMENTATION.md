# Sales Report API Documentation

## Overview

The Sales Report API provides a complete cycle for managing sales reports in the SoitMed system. It supports creating, reading, updating, deleting, and rating sales reports with role-based access control. Sales employees can manage their own reports, while sales managers can view all reports and rate them.

## System Architecture

### Roles and Permissions

- **SalesEmployee**: Can create, read, update, and delete their own reports
- **SalesManager**: Can view all reports and rate any report
- **Other Roles**: No access to sales report functionality

### Report Types

- `daily`: Daily sales reports
- `weekly`: Weekly sales reports
- `monthly`: Monthly sales reports
- `custom`: Custom period reports

## API Endpoints

### 1. Create Sales Report

**Endpoint**: `POST /api/SalesReport`  
**Authorization**: SalesEmployee only  
**Description**: Creates a new sales report for the authenticated employee.

#### Request Body

```typescript
interface CreateSalesReportDto {
	title: string; // Required, max 100 characters
	body: string; // Required, max 2000 characters
	type: string; // Required, one of: "daily", "weekly", "monthly", "custom"
	reportDate: string; // Required, format: "YYYY-MM-DD", cannot be future date
}
```

#### Example Request

```http
POST /api/SalesReport
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Daily Sales Report - January 15, 2024",
  "body": "Today we achieved 85% of our daily target. Key highlights include...",
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
		"title": "Daily Sales Report - January 15, 2024",
		"body": "Today we achieved 85% of our daily target...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "user-123",
		"employeeName": "John Doe",
		"rating": null,
		"comment": null,
		"createdAt": "2024-01-15T10:30:00.000Z",
		"updatedAt": "2024-01-15T10:30:00.000Z",
		"isActive": true
	},
	"message": "Report created successfully",
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
		"type": [
			"Type must be one of: daily, weekly, monthly, custom."
		],
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

### 2. Update Sales Report

**Endpoint**: `PUT /api/SalesReport/{id}`  
**Authorization**: SalesEmployee only (own reports)  
**Description**: Updates an existing sales report.

#### Request Body

```typescript
interface UpdateSalesReportDto {
	title: string; // Required, max 100 characters
	body: string; // Required, max 2000 characters
	type: string; // Required, one of: "daily", "weekly", "monthly", "custom"
	reportDate: string; // Required, format: "YYYY-MM-DD", cannot be future date
}
```

#### Example Request

```http
PUT /api/SalesReport/123
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Updated Daily Sales Report - January 15, 2024",
  "body": "Updated report content with more details...",
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
		"title": "Updated Daily Sales Report - January 15, 2024",
		"body": "Updated report content with more details...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "user-123",
		"employeeName": "John Doe",
		"rating": null,
		"comment": null,
		"createdAt": "2024-01-15T10:30:00.000Z",
		"updatedAt": "2024-01-15T11:45:00.000Z",
		"isActive": true
	},
	"message": "Report updated successfully",
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

### 3. Delete Sales Report

**Endpoint**: `DELETE /api/SalesReport/{id}`  
**Authorization**: SalesEmployee only (own reports)  
**Description**: Soft deletes a sales report (sets IsActive to false).

#### Example Request

```http
DELETE /api/SalesReport/123
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**

```json
{
	"success": true,
	"message": "Report deleted successfully",
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

### 4. Get Sales Report by ID

**Endpoint**: `GET /api/SalesReport/{id}`  
**Authorization**: Authenticated users  
**Description**: Retrieves a specific sales report by ID. Sales employees can only view their own reports, while sales managers can view any report.

#### Example Request

```http
GET /api/SalesReport/123
Authorization: Bearer <jwt-token>
```

#### Response Codes

**200 OK - Success**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"title": "Daily Sales Report - January 15, 2024",
		"body": "Today we achieved 85% of our daily target...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "user-123",
		"employeeName": "John Doe",
		"rating": 4,
		"comment": "Good work on the daily targets!",
		"createdAt": "2024-01-15T10:30:00.000Z",
		"updatedAt": "2024-01-15T10:30:00.000Z",
		"isActive": true
	},
	"message": "Report retrieved successfully",
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

### 5. Get Sales Reports (List with Filtering)

**Endpoint**: `GET /api/SalesReport`  
**Authorization**: Authenticated users  
**Description**: Retrieves a paginated list of sales reports with optional filtering. Sales employees see only their own reports, while sales managers see all reports.

#### Query Parameters

```typescript
interface FilterSalesReportsDto {
	employeeId?: string; // Optional, filter by specific employee
	startDate?: string; // Optional, format: "YYYY-MM-DD"
	endDate?: string; // Optional, format: "YYYY-MM-DD"
	type?: string; // Optional, one of: "daily", "weekly", "monthly", "custom"
	page?: number; // Optional, default: 1, must be > 0
	pageSize?: number; // Optional, default: 10, must be 1-100
}
```

#### Example Request

```http
GET /api/SalesReport?type=daily&startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=10
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
				"title": "Daily Sales Report - January 15, 2024",
				"body": "Today we achieved 85% of our daily target...",
				"type": "daily",
				"reportDate": "2024-01-15",
				"employeeId": "user-123",
				"employeeName": "John Doe",
				"rating": 4,
				"comment": "Good work!",
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
	"message": "Found 25 report(s)",
	"timestamp": "2024-01-15T10:30:00.000Z"
}
```

---

### 6. Rate Sales Report

**Endpoint**: `POST /api/SalesReport/{id}/rate`  
**Authorization**: SalesManager only  
**Description**: Rates a sales report with optional comment.

#### Request Body

```typescript
interface RateSalesReportDto {
	rating?: number; // Optional, 1-5 stars
	comment?: string; // Optional, max 500 characters
	// Note: At least one of rating or comment must be provided
}
```

#### Example Request

```http
POST /api/SalesReport/123/rate
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "rating": 4,
  "comment": "Excellent work on meeting the daily targets. Keep it up!"
}
```

#### Response Codes

**200 OK - Success**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"title": "Daily Sales Report - January 15, 2024",
		"body": "Today we achieved 85% of our daily target...",
		"type": "daily",
		"reportDate": "2024-01-15",
		"employeeId": "user-123",
		"employeeName": "John Doe",
		"rating": 4,
		"comment": "Excellent work on meeting the daily targets. Keep it up!",
		"createdAt": "2024-01-15T10:30:00.000Z",
		"updatedAt": "2024-01-15T10:30:00.000Z",
		"isActive": true
	},
	"message": "Report rated successfully",
	"timestamp": "2024-01-15T10:30:00.000Z"
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

---

## Data Models

### SalesReportResponseDto

```typescript
interface SalesReportResponseDto {
	id: number;
	title: string;
	body: string;
	type: string;
	reportDate: string; // Format: "YYYY-MM-DD"
	employeeId: string;
	employeeName: string;
	rating: number | null; // 1-5 stars, null if not rated
	comment: string | null; // Manager's comment, null if not provided
	createdAt: string; // ISO 8601 format
	updatedAt: string; // ISO 8601 format
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

---

## Frontend Integration Examples

### React Hook for Sales Reports

```typescript
import { useState, useEffect } from 'react';

interface UseSalesReportsReturn {
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
}

export function useSalesReports(): UseSalesReportsReturn {
	const [reports, setReports] = useState<SalesReportResponseDto[]>([]);
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const [pagination, setPagination] = useState({
		page: 1,
		pageSize: 10,
		totalPages: 0,
		hasNextPage: false,
		hasPreviousPage: false,
	});

	const apiCall = async (url: string, options: RequestInit = {}) => {
		const token = localStorage.getItem('authToken');
		const response = await fetch(url, {
			...options,
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
				...options.headers,
			},
		});

		if (!response.ok) {
			const errorData = await response.json();
			throw new Error(
				errorData.message || 'API request failed'
			);
		}

		return response.json();
	};

	const createReport = async (data: CreateSalesReportDto) => {
		setLoading(true);
		setError(null);
		try {
			const result = await apiCall('/api/SalesReport', {
				method: 'POST',
				body: JSON.stringify(data),
			});
			await fetchReports(); // Refresh the list
		} catch (err) {
			setError(
				err instanceof Error
					? err.message
					: 'Failed to create report'
			);
			throw err;
		} finally {
			setLoading(false);
		}
	};

	const updateReport = async (id: number, data: UpdateSalesReportDto) => {
		setLoading(true);
		setError(null);
		try {
			await apiCall(`/api/SalesReport/${id}`, {
				method: 'PUT',
				body: JSON.stringify(data),
			});
			await fetchReports(); // Refresh the list
		} catch (err) {
			setError(
				err instanceof Error
					? err.message
					: 'Failed to update report'
			);
			throw err;
		} finally {
			setLoading(false);
		}
	};

	const deleteReport = async (id: number) => {
		setLoading(true);
		setError(null);
		try {
			await apiCall(`/api/SalesReport/${id}`, {
				method: 'DELETE',
			});
			await fetchReports(); // Refresh the list
		} catch (err) {
			setError(
				err instanceof Error
					? err.message
					: 'Failed to delete report'
			);
			throw err;
		} finally {
			setLoading(false);
		}
	};

	const rateReport = async (id: number, data: RateSalesReportDto) => {
		setLoading(true);
		setError(null);
		try {
			await apiCall(`/api/SalesReport/${id}/rate`, {
				method: 'POST',
				body: JSON.stringify(data),
			});
			await fetchReports(); // Refresh the list
		} catch (err) {
			setError(
				err instanceof Error
					? err.message
					: 'Failed to rate report'
			);
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
			if (filters.employeeId)
				queryParams.append(
					'employeeId',
					filters.employeeId
				);
			if (filters.startDate)
				queryParams.append(
					'startDate',
					filters.startDate
				);
			if (filters.endDate)
				queryParams.append('endDate', filters.endDate);
			if (filters.type)
				queryParams.append('type', filters.type);
			if (filters.page)
				queryParams.append(
					'page',
					filters.page.toString()
				);
			if (filters.pageSize)
				queryParams.append(
					'pageSize',
					filters.pageSize.toString()
				);

			const result = await apiCall(
				`/api/SalesReport?${queryParams.toString()}`
			);
			setReports(result.data.data);
			setPagination({
				page: result.data.page,
				pageSize: result.data.pageSize,
				totalPages: result.data.totalPages,
				hasNextPage: result.data.hasNextPage,
				hasPreviousPage: result.data.hasPreviousPage,
			});
		} catch (err) {
			setError(
				err instanceof Error
					? err.message
					: 'Failed to fetch reports'
			);
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
	};
}
```

### React Component Example

```typescript
import React, { useState } from 'react';
import { useSalesReports } from './hooks/useSalesReports';

const SalesReportsPage: React.FC = () => {
	const {
		reports,
		loading,
		error,
		pagination,
		createReport,
		updateReport,
		deleteReport,
		rateReport,
		fetchReports,
	} = useSalesReports();

	const [showCreateForm, setShowCreateForm] = useState(false);
	const [editingReport, setEditingReport] =
		useState<SalesReportResponseDto | null>(null);
	const [filters, setFilters] = useState<FilterSalesReportsDto>({});

	const handleCreateReport = async (data: CreateSalesReportDto) => {
		try {
			await createReport(data);
			setShowCreateForm(false);
		} catch (error) {
			console.error('Failed to create report:', error);
		}
	};

	const handleUpdateReport = async (data: UpdateSalesReportDto) => {
		if (!editingReport) return;
		try {
			await updateReport(editingReport.id, data);
			setEditingReport(null);
		} catch (error) {
			console.error('Failed to update report:', error);
		}
	};

	const handleDeleteReport = async (id: number) => {
		if (
			window.confirm(
				'Are you sure you want to delete this report?'
			)
		) {
			try {
				await deleteReport(id);
			} catch (error) {
				console.error(
					'Failed to delete report:',
					error
				);
			}
		}
	};

	const handleRateReport = async (
		id: number,
		rating: number,
		comment: string
	) => {
		try {
			await rateReport(id, { rating, comment });
		} catch (error) {
			console.error('Failed to rate report:', error);
		}
	};

	const handleFilterChange = (newFilters: FilterSalesReportsDto) => {
		setFilters(newFilters);
		fetchReports(newFilters);
	};

	if (loading) return <div>Loading...</div>;
	if (error) return <div>Error: {error}</div>;

	return (
		<div className="sales-reports-page">
			<h1>Sales Reports</h1>

			{/* Filter Controls */}
			<div className="filters">
				<select
					value={filters.type || ''}
					onChange={(e) =>
						handleFilterChange({
							...filters,
							type:
								e.target
									.value ||
								undefined,
						})
					}
				>
					<option value="">All Types</option>
					<option value="daily">Daily</option>
					<option value="weekly">Weekly</option>
					<option value="monthly">Monthly</option>
					<option value="custom">Custom</option>
				</select>

				<input
					type="date"
					value={filters.startDate || ''}
					onChange={(e) =>
						handleFilterChange({
							...filters,
							startDate:
								e.target
									.value ||
								undefined,
						})
					}
					placeholder="Start Date"
				/>

				<input
					type="date"
					value={filters.endDate || ''}
					onChange={(e) =>
						handleFilterChange({
							...filters,
							endDate:
								e.target
									.value ||
								undefined,
						})
					}
					placeholder="End Date"
				/>
			</div>

			{/* Create Report Button */}
			<button onClick={() => setShowCreateForm(true)}>
				Create New Report
			</button>

			{/* Reports List */}
			<div className="reports-list">
				{reports.map((report) => (
					<div
						key={report.id}
						className="report-card"
					>
						<h3>{report.title}</h3>
						<p>
							<strong>Type:</strong>{' '}
							{report.type}
						</p>
						<p>
							<strong>Date:</strong>{' '}
							{report.reportDate}
						</p>
						<p>
							<strong>
								Employee:
							</strong>{' '}
							{report.employeeName}
						</p>
						<p>
							<strong>Body:</strong>{' '}
							{report.body}
						</p>

						{report.rating && (
							<div className="rating">
								<p>
									<strong>
										Rating:
									</strong>{' '}
									{
										report.rating
									}
									/5
								</p>
								{report.comment && (
									<p>
										<strong>
											Comment:
										</strong>{' '}
										{
											report.comment
										}
									</p>
								)}
							</div>
						)}

						<div className="actions">
							<button
								onClick={() =>
									setEditingReport(
										report
									)
								}
							>
								Edit
							</button>
							<button
								onClick={() =>
									handleDeleteReport(
										report.id
									)
								}
							>
								Delete
							</button>
							{/* Rating form for managers */}
							<RatingForm
								reportId={
									report.id
								}
								onRate={
									handleRateReport
								}
							/>
						</div>
					</div>
				))}
			</div>

			{/* Pagination */}
			<div className="pagination">
				<button
					disabled={!pagination.hasPreviousPage}
					onClick={() =>
						handleFilterChange({
							...filters,
							page:
								pagination.page -
								1,
						})
					}
				>
					Previous
				</button>
				<span>
					Page {pagination.page} of{' '}
					{pagination.totalPages}
				</span>
				<button
					disabled={!pagination.hasNextPage}
					onClick={() =>
						handleFilterChange({
							...filters,
							page:
								pagination.page +
								1,
						})
					}
				>
					Next
				</button>
			</div>

			{/* Create/Edit Forms */}
			{showCreateForm && (
				<CreateReportForm
					onSubmit={handleCreateReport}
					onCancel={() =>
						setShowCreateForm(false)
					}
				/>
			)}

			{editingReport && (
				<EditReportForm
					report={editingReport}
					onSubmit={handleUpdateReport}
					onCancel={() => setEditingReport(null)}
				/>
			)}
		</div>
	);
};

export default SalesReportsPage;
```

---

## Error Handling

### Common Error Scenarios

1. **Validation Errors (400)**

      - Missing required fields
      - Invalid field formats
      - Field length violations
      - Invalid enum values

2. **Authentication Errors (401)**

      - Missing or invalid JWT token
      - Expired token

3. **Authorization Errors (403)**

      - Insufficient role permissions
      - Attempting to access/modify other users' reports

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
	errors?: Record<string, string[]>; // For validation errors
	timestamp: string;
}
```

---

## Business Rules

### Report Creation

- Only SalesEmployee role can create reports
- Each employee can have only one report per type per date
- Report date cannot be in the future
- All fields are required and have length limits

### Report Management

- Employees can only modify their own reports
- Managers can view all reports but cannot modify them
- Reports are soft-deleted (IsActive = false)

### Report Rating

- Only SalesManager role can rate reports
- Rating must be between 1-5 stars
- Either rating or comment (or both) must be provided
- Rating can be updated by managers

### Data Filtering

- Employees see only their own reports
- Managers see all reports
- Filtering by date range, type, and employee is supported
- Pagination is required for large datasets

---

## Performance Considerations

- Pagination is implemented to handle large datasets efficiently
- Database queries are optimized with proper indexing
- Soft deletion preserves data integrity
- Caching can be implemented for frequently accessed reports

---

## Security Considerations

- JWT token authentication required for all endpoints
- Role-based authorization enforced
- Input validation prevents malicious data
- SQL injection protection through Entity Framework
- XSS protection through proper data encoding

---

## Testing Examples

### cURL Commands

**Create Report**

```bash
curl -X POST "https://your-api-domain.com/api/SalesReport" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Daily Sales Report",
    "body": "Report content here",
    "type": "daily",
    "reportDate": "2024-01-15"
  }'
```

**Get Reports with Filter**

```bash
curl -X GET "https://your-api-domain.com/api/SalesReport?type=daily&page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Rate Report**

```bash
curl -X POST "https://your-api-domain.com/api/SalesReport/123/rate" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 4,
    "comment": "Great work!"
  }'
```

---

## Version History

- **v1.0** (2024-01-15): Initial implementation with full CRUD operations and rating system

---

## Support

For additional support or to report issues with the Sales Report API, please contact the development team or refer to the main API documentation.
