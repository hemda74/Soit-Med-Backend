# Salesman API - TaskProgress & WeeklyPlan Endpoints Examples

This document provides comprehensive examples for all TaskProgress and WeeklyPlan endpoints available to Salesmen, including all possible request/response scenarios.

**Base URL:** `http://localhost:5117` (or your API URL)

**Authorization:** All requests require Bearer token authentication.

**Test Token:**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWhtZWRAc29pdG1lZC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkFobWVkX0FzaHJhZl9TYWxlc18wMDEiLCJqdGkiOiIzYmMwZWVkZi03NGU5LTQ1ZjgtOWIxMC0xNzVmNDBiMjExYjUiLCJhcHBsaWNhdGlvbiI6IlNvaXQtTWVkIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNtYW4iLCJleHAiOjE5MTk4NDMwMDIsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTExNyIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDIwMCJ9.Z9Lu6s_fdzVaFOgI7Q_blYPmMi2VWfvM-IlLekf_ils
```

---

## TaskProgress Endpoints

### 1. Create Task Progress

**Endpoint:** `POST /api/TaskProgress`

**Purpose:** Create a new progress record for a task.

**Authorization:** `Salesman, SalesManager`

**Request Headers:**

```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body Examples:**

#### Example 1: Visit with Interested Client

```json
{
	"taskId": 101,
	"progressDate": "2025-01-15T10:00:00Z",
	"progressType": "Visit",
	"description": "Met with Dr. Ahmed at hospital. Discussed new X-ray machine requirements.",
	"notes": "Client showed strong interest in Model XR-500",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-01-22T14:00:00Z",
	"followUpNotes": "Send technical specifications",
	"satisfactionRating": 5,
	"feedback": "Very positive meeting, client was very engaged"
}
```

#### Example 2: Call with Not Interested Client

```json
{
	"taskId": 102,
	"progressDate": "2025-01-15T11:30:00Z",
	"progressType": "Call",
	"description": "Called client to follow up on previous visit",
	"notes": "Client was unavailable, spoke with receptionist",
	"visitResult": "NotInterested",
	"notInterestedComment": "Budget constraints, not planning to purchase this year",
	"nextFollowUpDate": "2025-06-01T10:00:00Z",
	"followUpNotes": "Follow up after budget planning cycle"
}
```

#### Example 3: Meeting with Follow-up

```json
{
	"taskId": 103,
	"progressDate": "2025-01-15T14:00:00Z",
	"progressType": "Meeting",
	"description": "Detailed technical presentation at client facility",
	"notes": "Presented 3 equipment options, client needs to review internally",
	"nextFollowUpDate": "2025-01-20T10:00:00Z",
	"followUpNotes": "Call to get decision on preferred model",
	"satisfactionRating": 4
}
```

#### Example 4: Email Communication

```json
{
	"taskId": 104,
	"progressDate": "2025-01-15T09:00:00Z",
	"progressType": "Email",
	"description": "Sent product catalog and pricing information",
	"notes": "Client requested specific models via email",
	"nextFollowUpDate": "2025-01-18T10:00:00Z"
}
```

**Valid Values:**

- **progressType:** `Visit`, `Call`, `Meeting`, `Email`
- **visitResult:** `Interested`, `NotInterested` (optional)
- **nextStep:** `NeedsDeal`, `NeedsOffer` (required if visitResult is "Interested")
     - **Note:** Only these two values are validated by the API. You may see other values like "SendOffer" or "FollowUp" in existing data, but these are legacy values and won't be accepted for new records.
- **satisfactionRating:** 1-5 (optional)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 101,
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employeeName": "Ahmed Ashraf",
		"progressDate": "2025-01-15T10:00:00Z",
		"progressType": "Visit",
		"description": "Met with Dr. Ahmed at hospital. Discussed new X-ray machine requirements.",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"offerRequestId": null,
		"offerId": null,
		"dealId": null,
		"nextFollowUpDate": "2025-01-22T14:00:00Z",
		"satisfactionRating": 5,
		"createdAt": "2025-01-15T10:05:00Z"
	},
	"message": "Task progress created successfully",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**Error Responses:**

**400 Bad Request - Validation Errors:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"TaskId": ["TaskId is required"],
		"ProgressDate": ["ProgressDate is required"],
		"ProgressType": [
			"ProgressType is required",
			"ProgressType must be one of: Visit, Call, Meeting, Email"
		],
		"VisitResult": [
			"VisitResult must be either 'Interested' or 'NotInterested'"
		],
		"SatisfactionRating": [
			"SatisfactionRating must be between 1 and 5"
		]
	},
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**400 Bad Request - Invalid Task:**

```json
{
	"success": false,
	"message": "Task not found",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**403 Forbidden - Unauthorized Access:**

```json
{
	"success": false,
	"message": "You don't have permission to create progress for this task",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**500 Internal Server Error:**

```json
{
	"success": false,
	"message": "An error occurred while creating task progress",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

---

### 2. Create Task Progress with Offer Request

**Endpoint:** `POST /api/TaskProgress/with-offer-request`

**Purpose:** Create task progress and automatically create an offer request when client is interested.

**Authorization:** `Salesman, SalesManager`

**Request Body Examples:**

#### Example 1: Interested Client Needing Offer

```json
{
	"taskId": 101,
	"progressDate": "2025-01-15T10:00:00Z",
	"progressType": "Visit",
	"description": "Client visited showroom, very interested in X-ray machine",
	"notes": "Client needs detailed quote for budget approval",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-01-22T10:00:00Z",
	"followUpNotes": "Follow up after offer is prepared",
	"satisfactionRating": 5,
	"feedback": "Excellent visit, high conversion potential",
	"clientId": 25,
	"requestedProducts": "XR-500 X-Ray Machine, Mobile X-Ray Unit XR-200",
	"specialNotes": "Client prefers 12-month payment plan. Urgent requirement."
}
```

#### Example 2: Meeting Resulting in Offer Request

```json
{
	"taskId": 103,
	"progressDate": "2025-01-15T14:00:00Z",
	"progressType": "Meeting",
	"description": "Technical meeting with procurement team",
	"notes": "Discussed 3 models, client selected preferred option",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"clientId": 30,
	"requestedProducts": "Ultrasound System US-300, Patient Monitor PM-100",
	"specialNotes": "Need competitive pricing. Delivery required within 60 days."
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 101,
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employeeName": "Ahmed Ashraf",
		"progressDate": "2025-01-15T10:00:00Z",
		"progressType": "Visit",
		"description": "Client visited showroom, very interested in X-ray machine",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"offerRequestId": 45,
		"nextFollowUpDate": "2025-01-22T10:00:00Z",
		"satisfactionRating": 5,
		"createdAt": "2025-01-15T10:05:00Z"
	},
	"message": "Task progress created and offer request triggered successfully",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**Error Responses:**

**400 Bad Request - Validation Errors:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"ClientId": ["ClientId is required"],
		"RequestedProducts": ["RequestedProducts is required"],
		"VisitResult": [
			"VisitResult must be 'Interested' to create offer request"
		],
		"NextStep": [
			"NextStep must be 'NeedsOffer' to create offer request"
		]
	},
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**400 Bad Request - Invalid Request:**

```json
{
	"success": false,
	"message": "Client not found",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

**403 Forbidden:**

```json
{
	"success": false,
	"message": "You don't have permission to create progress for this task",
	"timestamp": "2025-01-15T10:05:00Z"
}
```

---

### 3. Get All My Task Progress

**Endpoint:** `GET /api/TaskProgress`

**Purpose:** Get all task progress records for the current salesman (optionally filtered by date range).

**Authorization:** `Salesman, SalesManager, SuperAdmin`

**Query Parameters:**

- `startDate` (optional): Filter from date (ISO 8601 format)
- `endDate` (optional): Filter to date (ISO 8601 format)

**Request Examples:**

#### Example 1: Get All Progress

```
GET /api/TaskProgress
Authorization: Bearer {token}
```

#### Example 2: Get Progress for Date Range

```
GET /api/TaskProgress?startDate=2025-01-01T00:00:00Z&endDate=2025-01-31T23:59:59Z
Authorization: Bearer {token}
```

#### Example 3: Get Progress from Start Date

```
GET /api/TaskProgress?startDate=2025-01-15T00:00:00Z
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"taskId": 101,
			"clientId": 25,
			"clientName": "Ahmed Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-15T10:00:00Z",
			"progressType": "Visit",
			"description": "Met with Dr. Ahmed at hospital",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer",
			"offerRequestId": 45,
			"nextFollowUpDate": "2025-01-22T14:00:00Z",
			"satisfactionRating": 5,
			"createdAt": "2025-01-15T10:05:00Z"
		},
		{
			"id": 501,
			"taskId": 102,
			"clientId": 26,
			"clientName": "Cairo Medical Center",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-15T11:30:00Z",
			"progressType": "Call",
			"description": "Follow-up call",
			"visitResult": "NotInterested",
			"notInterestedComment": "Budget constraints",
			"createdAt": "2025-01-15T11:35:00Z"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**Empty Result Response (200):**

```json
{
	"success": true,
	"data": [],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**Error Responses:**

**500 Internal Server Error:**

```json
{
	"success": false,
	"message": "An error occurred while retrieving task progress",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

---

### 4. Get Progress by Task

**Endpoint:** `GET /api/TaskProgress/task/{taskId}`

**Purpose:** Get all progress records for a specific task.

**Authorization:** `Salesman, SalesManager, SuperAdmin`

**Request Example:**

```
GET /api/TaskProgress/task/101
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"taskId": 101,
			"clientId": 25,
			"clientName": "Ahmed Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-15T10:00:00Z",
			"progressType": "Visit",
			"description": "Initial visit to client",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer",
			"offerRequestId": 45,
			"createdAt": "2025-01-15T10:05:00Z"
		},
		{
			"id": 510,
			"taskId": 101,
			"clientId": 25,
			"clientName": "Ahmed Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-20T14:00:00Z",
			"progressType": "Call",
			"description": "Follow-up call to check on offer status",
			"createdAt": "2025-01-20T14:05:00Z"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**Error Responses:**

**403 Forbidden - Task Not Yours:**

```json
{
	"success": false,
	"message": "You don't have permission to view this task's progress",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**400 Bad Request - Task Not Found:**

```json
{
	"success": false,
	"message": "Task not found",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

---

### 5. Get Progress by Client

**Endpoint:** `GET /api/TaskProgress/by-client/{clientId}`

**Purpose:** Get all progress/visit history for a specific client.

**Authorization:** `Salesman, SalesManager, SuperAdmin`

**Request Example:**

```
GET /api/TaskProgress/by-client/25
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"taskId": 101,
			"clientId": 25,
			"clientName": "Ahmed Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-15T10:00:00Z",
			"progressType": "Visit",
			"description": "Initial visit",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer",
			"createdAt": "2025-01-15T10:05:00Z"
		},
		{
			"id": 505,
			"taskId": 110,
			"clientId": 25,
			"clientName": "Ahmed Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-01-10T09:00:00Z",
			"progressType": "Call",
			"description": "Cold call introduction",
			"createdAt": "2025-01-10T09:05:00Z"
		}
	],
	"message": "Client progress retrieved successfully",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**Error Responses:**

**403 Forbidden - Client Not Assigned:**

```json
{
	"success": false,
	"message": "You don't have permission to view this client's progress",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

---

### 6. Update Task Progress

**Endpoint:** `PUT /api/TaskProgress/{id}`

**Purpose:** Update an existing task progress record (only if it belongs to you).

**Authorization:** `Salesman, SalesManager`

**Request Example:**

```
PUT /api/TaskProgress/500
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**

```json
{
	"taskId": 101,
	"progressDate": "2025-01-15T10:00:00Z",
	"progressType": "Visit",
	"description": "Updated description: Met with Dr. Ahmed and procurement manager",
	"notes": "Updated notes: Additional requirements discussed",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-01-25T10:00:00Z",
	"followUpNotes": "Updated follow-up: Send revised offer",
	"satisfactionRating": 5,
	"feedback": "Updated feedback: Very positive meeting"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 101,
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employeeName": "Ahmed Ashraf",
		"progressDate": "2025-01-15T10:00:00Z",
		"progressType": "Visit",
		"description": "Updated description: Met with Dr. Ahmed and procurement manager",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"nextFollowUpDate": "2025-01-25T10:00:00Z",
		"satisfactionRating": 5,
		"createdAt": "2025-01-15T10:05:00Z"
	},
	"message": "Task progress updated successfully",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**Error Responses:**

**400 Bad Request - Validation Errors:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"ProgressType": [
			"ProgressType must be one of: Visit, Call, Meeting, Email"
		]
	},
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**400 Bad Request - Progress Not Found:**

```json
{
	"success": false,
	"message": "Task progress not found",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

**403 Forbidden - Not Your Progress:**

```json
{
	"success": false,
	"message": "You don't have permission to update this progress",
	"timestamp": "2025-01-15T12:00:00Z"
}
```

---

## WeeklyPlan Endpoints

### 7. Create Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan`

**Purpose:** Create a new weekly plan.

**Authorization:** `All authenticated users` (Salesmen can create their own plans)

**Request Body Example:**

```json
{
	"weekStartDate": "2025-01-13T00:00:00Z",
	"weekEndDate": "2025-01-19T23:59:59Z",
	"title": "Week 3 - January 2025 Sales Plan",
	"description": "Focus on hospital visits in Cairo and follow-ups on pending offers"
}
```

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"weekStartDate": "2025-01-13T00:00:00Z",
		"weekEndDate": "2025-01-19T23:59:59Z",
		"title": "Week 3 - January 2025 Sales Plan",
		"description": "Focus on hospital visits in Cairo and follow-ups on pending offers",
		"isActive": true,
		"rating": null,
		"managerComment": null,
		"managerReviewedAt": null,
		"managerViewedAt": null,
		"viewedBy": null,
		"isViewed": false,
		"createdAt": "2025-01-13T08:00:00Z",
		"updatedAt": "2025-01-13T08:00:00Z",
		"tasks": []
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T08:00:00Z"
}
```

**✅ NEW FEATURE:** You can now create tasks when creating a weekly plan! Simply include a `tasks` array in the request body. Each task will be automatically linked to the created plan.

**Example with tasks:**

```json
{
	"weekStartDate": "2025-01-13T00:00:00Z",
	"weekEndDate": "2025-01-19T23:59:59Z",
	"title": "Week 3 - January 2025 Sales Plan",
	"description": "Focus on hospital visits in Cairo",
	"tasks": [
		{
			"title": "Visit Hospital A",
			"taskType": "Visit",
			"clientStatus": "New",
			"clientName": "New Hospital",
			"plannedDate": "2025-01-15T10:00:00Z",
			"priority": "High"
		}
	]
}
```

**Error Responses:**

**400 Bad Request - Plan Already Exists:**

```json
{
	"success": false,
	"message": "يوجد خطة أسبوعية بالفعل لهذا الأسبوع",
	"timestamp": "2025-01-13T08:00:00Z"
}
```

**400 Bad Request - Validation Errors:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"WeekStartDate": ["WeekStartDate is required"],
		"WeekEndDate": ["WeekEndDate is required"],
		"Title": [
			"Title is required",
			"Title cannot exceed 200 characters"
		]
	},
	"timestamp": "2025-01-13T08:00:00Z"
}
```

**401 Unauthorized:**

```json
{
	"success": false,
	"message": "Unauthorized access",
	"timestamp": "2025-01-13T08:00:00Z"
}
```

---

### 8. Get Weekly Plans

**Endpoint:** `GET /api/WeeklyPlan`

**Purpose:** Get weekly plans (filtered based on role - Salesmen see only their own).

**Authorization:** `All authenticated users`

**Query Parameters:**

- `page` (optional, default: 1): Page number
- `pageSize` (optional, default: 20): Items per page
- `employeeId` (optional): Filter by employee (Salesmen can only filter by date)
- `weekStartDate` (optional): Filter from date
- `weekEndDate` (optional): Filter to date
- `isViewed` (optional): Filter by viewed status (Salesmen cannot use this)

**Request Examples:**

#### Example 1: Get All My Plans (Salesman)

```
GET /api/WeeklyPlan
Authorization: Bearer {token}
```

#### Example 2: Get Plans with Pagination

```
GET /api/WeeklyPlan?page=1&pageSize=10
Authorization: Bearer {token}
```

#### Example 3: Get Plans by Date Range (Salesman - Allowed)

```
GET /api/WeeklyPlan?weekStartDate=2025-01-01T00:00:00Z&weekEndDate=2025-01-31T23:59:59Z
Authorization: Bearer {token}
```

**✅ Verified Working:** This endpoint has been tested and confirmed to work for Salesmen. Returns filtered plans within the specified date range.

#### Example 4: Invalid - Salesman Trying to Filter by Employee (Not Allowed)

```
GET /api/WeeklyPlan?employeeId=some-id
Authorization: Bearer {token}
```

**Success Response (200) - Salesman:**

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 50,
				"employeeId": "Ahmed_Ashraf_Sales_001",
				"employee": null,
				"weekStartDate": "2025-01-13T00:00:00Z",
				"weekEndDate": "2025-01-19T23:59:59Z",
				"title": "Week 3 - January 2025 Sales Plan",
				"description": "Focus on hospital visits in Cairo",
				"isActive": true,
				"rating": null,
				"managerComment": null,
				"managerReviewedAt": null,
				"managerViewedAt": null,
				"viewedBy": null,
				"isViewed": false,
				"createdAt": "2025-01-13T08:00:00Z",
				"updatedAt": "2025-01-13T08:00:00Z",
				"tasks": [
					{
						"id": 101,
						"taskType": "Visit",
						"clientId": 25,
						"clientName": "Ahmed Hospital",
						"clientStatus": "Active",
						"clientClassification": "Hospital",
						"plannedDate": "2025-01-15T10:00:00Z",
						"plannedTime": "10:00",
						"purpose": "Present new X-ray equipment",
						"priority": "High",
						"status": "InProgress",
						"progressCount": 2,
						"progresses": [],
						"offerRequests": [],
						"offers": [],
						"deals": []
					}
				]
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 1,
			"totalPages": 1
		}
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

**Error Responses:**

**403 Forbidden - Invalid Filter Usage (Salesman):**

```json
{
	"success": false,
	"message": "Salesmen can only filter by date range",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

**401 Unauthorized:**

```json
{
	"success": false,
	"message": "Unauthorized access",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

---

### 9. Get Weekly Plan by ID

**Endpoint:** `GET /api/WeeklyPlan/{id}`

**Purpose:** Get a specific weekly plan by ID.

**Authorization:** `All authenticated users` (Salesmen can only view their own)

**Request Example:**

```
GET /api/WeeklyPlan/50
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employee": null,
		"weekStartDate": "2025-01-13T00:00:00Z",
		"weekEndDate": "2025-01-19T23:59:59Z",
		"title": "Week 3 - January 2025 Sales Plan",
		"description": "Focus on hospital visits in Cairo",
		"isActive": true,
		"rating": 4,
		"managerComment": "Good plan, focus on follow-ups",
		"managerReviewedAt": "2025-01-20T10:00:00Z",
		"managerViewedAt": "2025-01-20T09:00:00Z",
		"viewedBy": "SalesManager_001",
		"isViewed": true,
		"createdAt": "2025-01-13T08:00:00Z",
		"updatedAt": "2025-01-20T10:00:00Z",
		"tasks": [
			{
				"id": 101,
				"taskType": "Visit",
				"clientId": 25,
				"clientName": "Ahmed Hospital",
				"clientStatus": "Active",
				"clientClassification": "Hospital",
				"plannedDate": "2025-01-15T10:00:00Z",
				"plannedTime": "10:00",
				"purpose": "Present new X-ray equipment",
				"priority": "High",
				"status": "Completed",
				"progressCount": 2,
				"progresses": [
					{
						"id": 500,
						"progressDate": "2025-01-15T10:00:00Z",
						"progressType": "Visit",
						"description": "Initial visit",
						"visitResult": "Interested",
						"nextStep": "NeedsOffer",
						"offerRequestId": 45
					}
				],
				"offerRequests": [
					{
						"id": 45,
						"requestedProducts": "XR-500 X-Ray Machine",
						"requestDate": "2025-01-15T10:05:00Z",
						"status": "Assigned",
						"createdOfferId": null
					}
				],
				"offers": [],
				"deals": []
			}
		]
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "Weekly plan not found",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

**403 Forbidden - Not Your Plan:**

```json
{
	"success": false,
	"message": "You don't have permission to view this plan",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

---

### 10. Update Weekly Plan

**Endpoint:** `PUT /api/WeeklyPlan/{id}`

**Purpose:** Update a weekly plan (only if it's yours and not submitted).

**Authorization:** `All authenticated users`

**Request Body:**

```json
{
	"title": "Updated Week 3 - January 2025 Sales Plan",
	"description": "Updated: Focus on hospital visits in Cairo and urgent follow-ups"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"title": "Updated Week 3 - January 2025 Sales Plan",
		"description": "Updated: Focus on hospital visits in Cairo and urgent follow-ups",
		"updatedAt": "2025-01-13T14:00:00Z"
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T14:00:00Z"
}
```

**Error Responses:**

**400 Bad Request - Plan Not Found:**

```json
{
	"success": false,
	"message": "Weekly plan not found",
	"timestamp": "2025-01-13T14:00:00Z"
}
```

**400 Bad Request - Already Submitted:**

```json
{
	"success": false,
	"message": "Cannot update a plan that has been submitted",
	"timestamp": "2025-01-13T14:00:00Z"
}
```

---

### 11. Submit Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/submit`

**Purpose:** Submit a weekly plan for manager review.

**Authorization:** `All authenticated users`

**Request Example:**

```
POST /api/WeeklyPlan/50/submit
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": "Weekly plan submitted successfully",
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T15:00:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "Weekly plan not found",
	"timestamp": "2025-01-13T15:00:00Z"
}
```

**400 Bad Request - Already Submitted:**

```json
{
	"success": false,
	"message": "This plan has already been submitted",
	"timestamp": "2025-01-13T15:00:00Z"
}
```

---

### 12. Get Current Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/current`

**Purpose:** Get the current active weekly plan for the logged-in user.

**Authorization:** `All authenticated users`

**Request Example:**

```
GET /api/WeeklyPlan/current
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"weekStartDate": "2025-01-13T00:00:00Z",
		"weekEndDate": "2025-01-19T23:59:59Z",
		"title": "Week 3 - January 2025 Sales Plan",
		"description": "Focus on hospital visits in Cairo",
		"isActive": true,
		"createdAt": "2025-01-13T08:00:00Z",
		"updatedAt": "2025-01-13T08:00:00Z",
		"tasks": []
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

**Error Responses:**

**404 Not Found:**

```json
{
	"success": false,
	"message": "No current weekly plan found",
	"timestamp": "2025-01-13T12:00:00Z"
}
```

---

## Notes for Salesmen

1. **TaskProgress Endpoints:**

      - Salesmen can only create progress for their own tasks
      - Salesmen can only view their own progress records
      - All progress types: `Visit`, `Call`, `Meeting`, `Email`
      - Visit results: `Interested`, `NotInterested`
      - Next steps: `NeedsDeal`, `NeedsOffer`

2. **WeeklyPlan Endpoints:**

      - Salesmen can only create and manage their own weekly plans
      - Salesmen can filter plans by date range only
      - Salesmen cannot use `employeeId` or `isViewed` filters
      - Salesmen cannot review plans (that's for managers)

3. **Error Handling:**
      - Always check the `success` field in responses
      - `400` = Bad Request (validation errors, invalid data)
      - `403` = Forbidden (unauthorized access)
      - `404` = Not Found (resource doesn't exist)
      - `500` = Internal Server Error (server issue)
