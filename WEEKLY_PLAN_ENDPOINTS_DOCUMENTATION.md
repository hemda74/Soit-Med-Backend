# Weekly Plans API - Complete Endpoints Documentation

**Base URL:** `http://localhost:5117/api/WeeklyPlan`

**Authentication:** All endpoints require a valid JWT Bearer token in the `Authorization` header.

---

## Table of Contents

1. [Overview](#overview)
2. [Common DTOs](#common-dtos)
3. [Endpoints Reference](#endpoints-reference)
4. [Authorization Rules](#authorization-rules)
5. [Response Format](#response-format)
6. [Testing Examples](#testing-examples)

---

## Overview

The Weekly Plans API allows salesmen to create and manage their weekly work plans, while SalesManagers and SuperAdmins can review all plans. Plans can be created, updated, submitted for review, and reviewed by managers.

---

## Common DTOs

### CreateWeeklyPlanDTO

```json
{
	"title": "string (required)",
	"description": "string (required)",
	"weekStartDate": "2025-10-27T00:00:00 (required, ISO 8601)",
	"weekEndDate": "2025-11-02T00:00:00 (required, ISO 8601)",
	"tasks": [
		{
			"taskType": "string (Visit|FollowUp|Call|Email|Meeting)",
			"clientId": "number (optional)",
			"clientName": "string (optional)",
			"clientStatus": "string (optional)",
			"clientClassification": "string (optional)",
			"plannedDate": "2025-10-27T00:00:00 (required)",
			"plannedTime": "string (optional, e.g., '10:00')",
			"purpose": "string (optional)",
			"priority": "string (High|Medium|Low)",
			"status": "string (Planned|InProgress|Completed|Cancelled)"
		}
	]
}
```

### UpdateWeeklyPlanDTO

```json
{
	"title": "string (optional)",
	"description": "string (optional)",
	"isActive": "boolean (optional)"
}
```

### ReviewWeeklyPlanDTO

```json
{
	"rating": "integer (required, 1-5)",
	"managerComment": "string (optional)"
}
```

### WeeklyPlanResponseDTO

```json
{
	"id": "number",
	"employeeId": "string",
	"employee": {
		"id": "string",
		"firstName": "string",
		"lastName": "string",
		"email": "string",
		"phoneNumber": "string (nullable)",
		"userName": "string"
	}, // Only populated for SalesManager/SuperAdmin
	"weekStartDate": "2025-10-27T00:00:00",
	"weekEndDate": "2025-11-02T00:00:00",
	"title": "string",
	"description": "string",
	"isActive": "boolean",
	"rating": "number (nullable)",
	"managerComment": "string (nullable)",
	"managerReviewedAt": "datetime (nullable)",
	"createdAt": "datetime",
	"updatedAt": "datetime",
	"tasks": [
		{
			"id": "number",
			"taskType": "string",
			"clientId": "number (nullable)",
			"clientName": "string (nullable)",
			"clientStatus": "string (nullable)",
			"clientClassification": "string (nullable)",
			"plannedDate": "datetime (nullable)",
			"plannedTime": "string (nullable)",
			"purpose": "string (nullable)",
			"priority": "string (nullable)",
			"status": "string (nullable)",
			"progressCount": "number",
			"progresses": [
				{
					"id": "number",
					"progressDate": "datetime",
					"progressType": "string",
					"description": "string",
					"visitResult": "string",
					"nextStep": "string",
					"offerRequestId": "number (nullable)"
				}
			],
			"offerRequests": [
				{
					"id": "number",
					"requestedProducts": "string",
					"requestDate": "datetime",
					"status": "string",
					"createdOfferId": "number (nullable)"
				}
			],
			"offers": [
				{
					"id": "number",
					"products": "string",
					"totalAmount": "number",
					"validUntil": "datetime",
					"status": "string",
					"sentToClientAt": "datetime (nullable)"
				}
			],
			"deals": [
				{
					"id": "number",
					"dealValue": "number",
					"closedDate": "datetime (nullable)",
					"status": "string",
					"managerApprovedAt": "datetime (nullable)",
					"superAdminApprovedAt": "datetime (nullable)"
				}
			]
		}
	]
}
```

---

## Endpoints Reference

### 1. Create Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan`

**Authorization:** All authenticated users

**Description:** Creates a new weekly plan for the authenticated user.

**Request Body:**

```json
{
	"title": "Week 1 Sales Plan",
	"description": "Focus on client visits and follow-ups",
	"weekStartDate": "2025-10-27T00:00:00",
	"weekEndDate": "2025-11-02T00:00:00",
	"tasks": [
		{
			"taskType": "Visit",
			"clientId": 1,
			"plannedDate": "2025-10-27T00:00:00",
			"plannedTime": "10:00",
			"purpose": "Product demonstration",
			"priority": "High",
			"status": "Planned"
		}
	]
}
```

**Success Response:** `201 Created`

```json
{
  "success": true,
  "data": {
    "id": 100,
    "employeeId": "Ahmed_Ashraf_Sales_001",
    "weekStartDate": "2025-10-27T00:00:00",
    "weekEndDate": "2025-11-02T00:00:00",
    "title": "Week 1 Sales Plan",
    "description": "Focus on client visits and follow-ups",
    "isActive": true,
    "rating": null,
    "managerComment": null,
    "managerReviewedAt": null,
    "createdAt": "2025-10-26T13:00:00",
    "updatedAt": "2025-10-26T13:00:00",
    "tasks": [...]
  },
  "message": "Operation completed successfully",
  "timestamp": "2025-10-26T13:00:00Z"
}
```

**Error Responses:**

- `400 Bad Request`: Plan already exists for this week
- `401 Unauthorized`: Invalid or missing token

---

### 2. Get Weekly Plans (List)

**Endpoint:** `GET /api/WeeklyPlan?page=1&pageSize=20`

**Authorization:** All authenticated users

**Description:**

- **Salesman**: Returns only their own weekly plans
- **SalesManager/SuperAdmin**: Returns all weekly plans from all salesmen

**Query Parameters:**

- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Number of items per page

**Success Response:** `200 OK`

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
				"description": "Advanced client discussions",
				"isActive": true,
				"rating": null,
				"managerComment": null,
				"managerReviewedAt": null,
				"createdAt": "2025-10-26T12:45:22.72",
				"updatedAt": "2025-10-26T12:45:22.72",
				"tasks": [
					{
						"id": 94,
						"taskType": "Visit",
						"clientId": 1,
						"clientName": null,
						"clientStatus": "Old",
						"clientClassification": null,
						"plannedDate": "2025-11-03T15:45:56.7",
						"plannedTime": "11:00",
						"purpose": "Close the deal",
						"priority": "High",
						"status": "Planned",
						"progressCount": 0
					}
				]
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 26,
			"totalPages": 2
		}
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T13:32:55Z"
}
```

**Error Responses:**

- `401 Unauthorized`: Invalid or missing token

---

### 3. Get Single Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/{id}`

**Authorization:** All authenticated users

**Description:** Returns a specific weekly plan by ID. Salesmen can only view their own plans, while SalesManagers and SuperAdmins can view any plan.

**Success Response:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 34,
    "employeeId": "Ahmed_Ashraf_Sales_001",
    "weekStartDate": "2025-10-22T00:00:00",
    "weekEndDate": "2025-10-28T00:00:00",
    "title": "Week 1 Sales Plan",
    "description": "First week of sales activities",
    "isActive": true,
    "rating": 4,
    "managerComment": "Good plan, well structured",
    "managerReviewedAt": "2025-10-26T12:41:56Z",
    "createdAt": "2025-10-22T16:56:13.35",
    "updatedAt": "2025-10-22T16:56:13.35",
    "tasks": [...]
  },
  "message": "Operation completed successfully",
  "timestamp": "2025-10-26T13:32:55Z"
}
```

**Error Responses:**

- `404 Not Found`: Plan not found
- `401 Unauthorized`: Invalid or missing token

---

### 4. Update Weekly Plan

**Endpoint:** `PUT /api/WeeklyPlan/{id}`

**Authorization:** All authenticated users (only for their own plans)

**Description:** Updates an existing weekly plan. Can only be updated if `isActive` is `true`.

**Request Body:**

```json
{
	"title": "Updated Week Plan",
	"description": "Updated description",
	"isActive": true
}
```

**Success Response:** `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 34,
		"title": "Updated Week Plan",
		"description": "Updated description",
		"updatedAt": "2025-10-26T13:45:00"
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T13:45:00Z"
}
```

**Error Responses:**

- `400 Bad Request`: Plan is not active or cannot be updated
- `404 Not Found`: Plan not found
- `401 Unauthorized`: Invalid or missing token

---

### 5. Submit Weekly Plan for Review

**Endpoint:** `POST /api/WeeklyPlan/{id}/submit`

**Authorization:** All authenticated users (only for their own plans)

**Description:** Submits a weekly plan for manager review. Makes the plan inactive so it cannot be modified further.

**Success Response:** `200 OK`

```json
{
	"success": true,
	"data": "Weekly plan submitted successfully",
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T13:50:00Z"
}
```

**Error Responses:**

- `400 Bad Request`: Plan is not active or has already been submitted
- `404 Not Found`: Plan not found
- `401 Unauthorized`: Invalid or missing token

---

### 6. Review Weekly Plan (Manager Only)

**Endpoint:** `POST /api/WeeklyPlan/{id}/review`

**Authorization:** SalesManager, SuperAdmin only

**Description:** Allows managers to review and rate a weekly plan with comments.

**Request Body:**

```json
{
	"rating": 4,
	"managerComment": "Good plan, well structured and realistic goals"
}
```

**Validation:**

- `rating`: Required, integer between 1-5
- `managerComment`: Optional string

**Success Response:** `200 OK`

```json
{
	"success": true,
	"data": "Weekly plan reviewed successfully",
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T14:00:00Z"
}
```

**Error Responses:**

- `400 Bad Request`: Invalid rating or plan has already been reviewed
- `403 Forbidden`: User not authorized (must be SalesManager or SuperAdmin)
- `404 Not Found`: Plan not found
- `401 Unauthorized`: Invalid or missing token

---

### 7. Get Current Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/current`

**Authorization:** All authenticated users

**Description:** Returns the current week's plan for the authenticated user (where current week's `weekStartDate` matches the current date).

**Success Response:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 41,
    "employeeId": "Ahmed_Ashraf_Sales_001",
    "weekStartDate": "2025-11-03T00:00:00",
    "weekEndDate": "2025-11-09T00:00:00",
    "title": "Week 45 Sales Plan",
    "description": "Advanced client discussions",
    "isActive": true,
    "rating": null,
    "managerComment": null,
    "managerReviewedAt": null,
    "createdAt": "2025-10-26T12:45:22.72",
    "updatedAt": "2025-10-26T12:45:22.72",
    "tasks": [...]
  },
  "message": "Operation completed successfully",
  "timestamp": "2025-10-26T14:00:00Z"
}
```

**Error Responses:**

- `404 Not Found`: No current weekly plan exists
- `401 Unauthorized`: Invalid or missing token

---

## Authorization Rules

### Salesman

- ✅ Create weekly plans
- ✅ View own weekly plans only
- ✅ Update own weekly plans (only if active)
- ✅ Submit own weekly plans for review
- ❌ Review plans (manager only)

### SalesManager

- ✅ View all weekly plans from all salesmen
- ✅ Review and rate any weekly plan
- ✅ View any plan details

### SuperAdmin

- ✅ View all weekly plans from all salesmen
- ✅ Review and rate any weekly plan
- ✅ View any plan details
- ✅ All salesman permissions

---

## Response Format

### Success Response

All successful responses follow this format:

```json
{
	"success": true,
	"data": {
		/* response data */
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T14:00:00Z"
}
```

### Error Response

All error responses follow this format:

```json
{
	"success": false,
	"message": "Error description",
	"timestamp": "2025-10-26T14:00:00Z"
}
```

---

## Testing Examples

### 1. Create Weekly Plan (cURL)

```bash
curl -X POST "http://localhost:5117/api/WeeklyPlan" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "Week 1 Sales Plan",
    "description": "Focus on client visits",
    "weekStartDate": "2025-10-27T00:00:00",
    "weekEndDate": "2025-11-02T00:00:00",
    "tasks": [
      {
        "taskType": "Visit",
        "clientId": 1,
        "plannedDate": "2025-10-27T00:00:00",
        "plannedTime": "10:00",
        "purpose": "Product demonstration",
        "priority": "High",
        "status": "Planned"
      }
    ]
  }'
```

### 2. Get All Weekly Plans (Salesman)

```bash
curl -X GET "http://localhost:5117/api/WeeklyPlan?page=1&pageSize=20" \
  -H "Authorization: Bearer YOUR_SALESMAN_TOKEN"
```

### 3. Get All Weekly Plans (SalesManager - All Plans)

```bash
curl -X GET "http://localhost:5117/api/WeeklyPlan?page=1&pageSize=20" \
  -H "Authorization: Bearer YOUR_MANAGER_TOKEN"
```

### 4. Update Weekly Plan

```bash
curl -X PUT "http://localhost:5117/api/WeeklyPlan/34" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "Updated Plan",
    "description": "Updated description",
    "isActive": true
  }'
```

### 5. Submit Weekly Plan

```bash
curl -X POST "http://localhost:5117/api/WeeklyPlan/34/submit" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 6. Review Weekly Plan (Manager Only)

```bash
curl -X POST "http://localhost:5117/api/WeeklyPlan/34/review" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_MANAGER_TOKEN" \
  -d '{
    "rating": 4,
    "managerComment": "Excellent work!"
  }'
```

### 7. Get Current Weekly Plan

```bash
curl -X GET "http://localhost:5117/api/WeeklyPlan/current" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 8. Get Single Weekly Plan

```bash
curl -X GET "http://localhost:5117/api/WeeklyPlan/34" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## React Example

```javascript
import axios from 'axios';

const API_URL = 'http://localhost:5117/api/WeeklyPlan';

// Get all weekly plans
export const getWeeklyPlans = async (token, page = 1, pageSize = 20) => {
	try {
		const response = await axios.get(API_URL, {
			params: { page, pageSize },
			headers: {
				Authorization: `Bearer ${token}`,
			},
		});
		return response.data;
	} catch (error) {
		console.error('Error fetching weekly plans:', error);
		throw error;
	}
};

// Create weekly plan
export const createWeeklyPlan = async (token, planData) => {
	try {
		const response = await axios.post(API_URL, planData, {
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		});
		return response.data;
	} catch (error) {
		console.error('Error creating weekly plan:', error);
		throw error;
	}
};

// Submit weekly plan
export const submitWeeklyPlan = async (token, planId) => {
	try {
		const response = await axios.post(
			`${API_URL}/${planId}/submit`,
			{},
			{
				headers: {
					Authorization: `Bearer ${token}`,
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error submitting weekly plan:', error);
		throw error;
	}
};
```

---

## React Native Example

```javascript
import axios from 'axios';

const API_URL = 'http://your-server-ip:5117/api/WeeklyPlan';

// Get all weekly plans
export const getWeeklyPlans = async (token, page = 1, pageSize = 20) => {
	try {
		const response = await axios.get(API_URL, {
			params: { page, pageSize },
			headers: {
				Authorization: `Bearer ${token}`,
			},
		});
		return response.data;
	} catch (error) {
		console.error('Error fetching weekly plans:', error);
		throw error;
	}
};

// Create weekly plan
export const createWeeklyPlan = async (token, planData) => {
	try {
		const response = await axios.post(API_URL, planData, {
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		});
		return response.data;
	} catch (error) {
		console.error('Error creating weekly plan:', error);
		throw error;
	}
};

// Submit weekly plan
export const submitWeeklyPlan = async (token, planId) => {
	try {
		const response = await axios.post(
			`${API_URL}/${planId}/submit`,
			{},
			{
				headers: {
					Authorization: `Bearer ${token}`,
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error submitting weekly plan:', error);
		throw error;
	}
};
```

---

## Notes

1. **Pagination**: The list endpoint supports pagination with `page` and `pageSize` query parameters.

2. **Role-Based Access**: Salesman users see only their own plans, while SalesManagers and SuperAdmins see all plans.

3. **Employee Information**: The `employee` field in the response is only populated for SalesManager and SuperAdmin users. Regular salesmen will see `null` for this field.

4. **Complete Data Structure**: Each task in the response includes:

      - **Progresses**: All task progress updates with visit results
      - **OfferRequests**: All offer requests created from task progresses
      - **Offers**: All sales offers created from offer requests
      - **Deals**: All deals closed from offers

5. **Active Plans**: Only active plans (`isActive: true`) can be updated.

6. **Review Process**: Once a plan is submitted for review, it becomes inactive and cannot be modified. Only managers can review plans.

7. **Task Types**: Valid values are: `Visit`, `FollowUp`, `Call`, `Email`, `Meeting`.

8. **Priority**: Valid values are: `High`, `Medium`, `Low`.

9. **Task Status**: Valid values are: `Planned`, `InProgress`, `Completed`, `Cancelled`.

10. **Rating Scale**: Managers can rate plans from 1 (poor) to 5 (excellent).

---

## Summary

This API provides complete functionality for managing weekly plans with role-based access control. Salesmen can create and manage their own plans, while managers can review and rate all plans across the organization.
