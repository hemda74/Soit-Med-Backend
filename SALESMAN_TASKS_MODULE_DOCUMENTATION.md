# 📋 Salesman Tasks Module API Documentation

## 🎯 Overview

This documentation covers all CRUD endpoints for the Tasks module and its related entities that salesmen can interact with in the Sales Module:

- **Weekly Plan Tasks** - Tasks within weekly plans
- **Task Progress** - Recording visit/meeting progress
- **Offer Requests** - Requesting offers for clients
- **Offers** - Viewing and managing assigned offers
- **Deals** - Creating and tracking deals

---

## 🔐 Authentication

**All endpoints require JWT token in Authorization header:**

```
Authorization: Bearer {your-jwt-token}
```

**Response Format:**

- **Success (200/201):** `{ success: true, data: {...}, message: "...", timestamp: "..." }`
- **Error (400/404/500):** `{ success: false, message: "...", errors: {...}, timestamp: "..." }`

---

## 📋 Endpoints Overview

| #   | Endpoint                                 | Method | Purpose                             |
| --- | ---------------------------------------- | ------ | ----------------------------------- |
| 1   | `/api/WeeklyPlan`                        | POST   | Create weekly plan (includes tasks) |
| 2   | `/api/WeeklyPlan`                        | GET    | Get my weekly plans with tasks      |
| 3   | `/api/WeeklyPlan/{id}`                   | GET    | Get weekly plan with all tasks      |
| 4   | `/api/WeeklyPlan/{id}`                   | PUT    | Update weekly plan                  |
| 5   | `/api/TaskProgress`                      | POST   | Create task progress                |
| 6   | `/api/TaskProgress/with-offer-request`   | POST   | Create progress + offer request     |
| 7   | `/api/TaskProgress`                      | GET    | Get my task progress                |
| 8   | `/api/TaskProgress/task/{taskId}`        | GET    | Get progress for specific task      |
| 9   | `/api/TaskProgress/by-client/{clientId}` | GET    | Get client visit history            |
| 10  | `/api/TaskProgress/{id}`                 | PUT    | Update task progress                |
| 11  | `/api/OfferRequest`                      | POST   | Create offer request                |
| 12  | `/api/OfferRequest`                      | GET    | Get offer requests                  |
| 13  | `/api/OfferRequest/{id}`                 | GET    | Get offer request details           |
| 14  | `/api/OfferRequest/my-requests`          | GET    | Get my offer requests               |
| 15  | `/api/Offer/assigned-to-me`              | GET    | Get offers assigned to me           |
| 16  | `/api/Offer/{id}`                        | GET    | Get offer details                   |
| 17  | `/api/Offer/{offerId}/export-pdf`        | GET    | Export offer as PDF                 |
| 18  | `/api/Deal`                              | POST   | Create new deal                     |
| 19  | `/api/Deal`                              | GET    | Get my deals                        |
| 20  | `/api/Deal/{id}`                         | GET    | Get deal details                    |
| 21  | `/api/Deal/by-client/{clientId}`         | GET    | Get client's deals                  |

---

## 📝 Section 1: Weekly Plan Tasks

Tasks are created as part of Weekly Plans. When you create a weekly plan, you include tasks in the request. You can then view, update, and track progress for these tasks.

---

### 1.1 Create Weekly Plan (with Tasks)

**Endpoint:** `POST /api/WeeklyPlan`

**Purpose:** Create a new weekly plan with tasks for the week

**Request Body:**

```json
{
	"weekStartDate": "2025-11-04T00:00:00Z",
	"weekEndDate": "2025-11-10T23:59:59Z",
	"title": "Week 45 Sales Plan",
	"description": "Focus on high priority clients and follow-ups"
}
```

**Note:** Tasks are added separately after creating the plan through the weekly plan service. The tasks are returned when you retrieve the weekly plan.

**Required Fields:**

- `weekStartDate` (DateTime) - Start of the week
- `weekEndDate` (DateTime) - End of the week
- `title` (string, max 200) - Plan title

**Optional Fields:**

- `description` (string, max 1000) - Plan description

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"employeeId": "salesman-123",
		"weekStartDate": "2025-11-04T00:00:00Z",
		"weekEndDate": "2025-11-10T23:59:59Z",
		"title": "Week 45 Sales Plan",
		"description": "Focus on high priority clients and follow-ups",
		"isActive": true,
		"tasks": [],
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Plan already exists for this week or validation errors
- **401 Unauthorized:** Missing or invalid token

---

### 1.2 Get My Weekly Plans (with Tasks)

**Endpoint:** `GET /api/WeeklyPlan`

**Purpose:** Get all my weekly plans with their tasks

**Query Parameters:**

- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page
- `weekStartDate` (DateTime, optional) - Filter by start date
- `weekEndDate` (DateTime, optional) - Filter by end date

**Note:** Salesmen can only filter by date range, not by employeeId or isViewed status.

**Request Example:**

```http
GET /api/WeeklyPlan?page=1&pageSize=20&weekStartDate=2025-11-01T00:00:00Z&weekEndDate=2025-11-30T23:59:59Z
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 50,
				"weekStartDate": "2025-11-04T00:00:00Z",
				"weekEndDate": "2025-11-10T23:59:59Z",
				"title": "Week 45 Sales Plan",
				"isActive": true,
				"isViewed": false,
				"tasks": [
					{
						"id": 101,
						"taskType": "Visit",
						"clientId": 123,
						"clientName": "Ahmed Hospital",
						"clientStatus": "Active",
						"plannedDate": "2025-11-05T10:00:00Z",
						"priority": "High",
						"status": "Pending",
						"progressCount": 0,
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
			"totalCount": 15,
			"totalPages": 1
		}
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 1.3 Get Specific Weekly Plan (with Tasks)

**Endpoint:** `GET /api/WeeklyPlan/{id}`

**Purpose:** Get detailed weekly plan with all tasks and their related data

**Path Parameters:**

- `id` (long) - Weekly plan ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"weekStartDate": "2025-11-04T00:00:00Z",
		"weekEndDate": "2025-11-10T23:59:59Z",
		"title": "Week 45 Sales Plan",
		"description": "Focus on high priority clients",
		"isActive": true,
		"tasks": [
			{
				"id": 101,
				"taskType": "Visit",
				"clientId": 123,
				"clientName": "Ahmed Hospital",
				"clientStatus": "Active",
				"plannedDate": "2025-11-05T10:00:00Z",
				"plannedTime": "10:00 AM",
				"purpose": "Follow up on equipment needs",
				"priority": "High",
				"status": "InProgress",
				"progressCount": 2,
				"progresses": [
					{
						"id": 500,
						"progressDate": "2025-11-05T10:00:00Z",
						"progressType": "Visit",
						"description": "Visited client, discussed needs",
						"visitResult": "Interested",
						"nextStep": "NeedsOffer",
						"offerRequestId": 25
					}
				],
				"offerRequests": [
					{
						"id": 25,
						"requestedProducts": "X-Ray Machine",
						"requestDate": "2025-11-05T11:00:00Z",
						"status": "Assigned",
						"createdOfferId": 50
					}
				],
				"offers": [
					{
						"id": 50,
						"products": "X-Ray Machine Model XYZ",
						"totalAmount": 50000.0,
						"validUntil": "2025-11-20T23:59:59Z",
						"status": "Sent",
						"sentToClientAt": "2025-11-06T09:00:00Z"
					}
				],
				"deals": []
			}
		]
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Plan doesn't exist or doesn't belong to you

---

### 1.4 Update Weekly Plan

**Endpoint:** `PUT /api/WeeklyPlan/{id}`

**Purpose:** Update weekly plan title and description (only if not submitted)

**Path Parameters:**

- `id` (long) - Weekly plan ID

**Request Body:**

```json
{
	"title": "Updated Week Plan Title",
	"description": "Updated description"
}
```

**Note:** Both fields are optional. Only send what you want to update.

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"title": "Updated Week Plan Title",
		"description": "Updated description",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Plan already submitted or validation errors
- **404 Not Found:** Plan doesn't exist or doesn't belong to you

---

## 📊 Section 2: Task Progress

Task Progress records represent visits, calls, meetings, or other interactions related to a task. These are the core records of your sales activities.

---

### 2.1 Create Task Progress

**Endpoint:** `POST /api/TaskProgress`

**Purpose:** Record progress for a task (visit, call, meeting, etc.)

**Request Body:**

```json
{
	"taskId": 101,
	"progressDate": "2025-10-29T10:00:00Z",
	"progressType": "Visit",
	"description": "Visited client, discussed equipment needs",
	"notes": "Client showed interest in X-Ray machine",
	"visitResult": "Interested",
	"notInterestedComment": null,
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-11-05T10:00:00Z",
	"followUpNotes": "Follow up on offer delivery timeline",
	"satisfactionRating": 4,
	"feedback": "Very positive meeting, client eager to move forward"
}
```

**Required Fields:**

- `taskId` (long) - ID of the task this progress is for
- `progressDate` (DateTime) - Date and time of the progress
- `progressType` (string) - Type: "Visit", "Call", "Meeting", "Email"

**Optional Fields:**

- `description` (string, max 2000) - Description of the interaction
- `notes` (string, max 2000) - Additional notes
- `visitResult` (string) - "Interested" or "NotInterested"
- `notInterestedComment` (string, max 2000) - Required if visitResult is "NotInterested"
- `nextStep` (string) - "NeedsDeal" or "NeedsOffer"
- `nextFollowUpDate` (DateTime) - When to follow up next
- `followUpNotes` (string, max 1000) - Notes for next follow-up
- `satisfactionRating` (int, 1-5) - Rating from 1 to 5
- `feedback` (string, max 2000) - Client feedback

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 101,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"employeeId": "salesman-123",
		"employeeName": "Ahmed Mohamed",
		"progressDate": "2025-10-29T10:00:00Z",
		"progressType": "Visit",
		"description": "Visited client, discussed equipment needs",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"offerRequestId": null,
		"offerId": null,
		"dealId": null,
		"nextFollowUpDate": "2025-11-05T10:00:00Z",
		"satisfactionRating": 4,
		"createdAt": "2025-10-29T10:05:00Z"
	},
	"message": "Task progress created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors (missing required fields, invalid taskId, etc.)
- **401 Unauthorized:** Not authenticated

---

### 2.2 Create Progress with Offer Request

**Endpoint:** `POST /api/TaskProgress/with-offer-request`

**Purpose:** Record progress AND automatically create an offer request (when client is interested)

**Request Body:**

```json
{
	"taskId": 101,
	"progressDate": "2025-10-29T10:00:00Z",
	"progressType": "Visit",
	"description": "Visited client, discussed equipment needs",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"clientId": 123,
	"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device Model ABC",
	"specialNotes": "Client needs delivery within 2 weeks, urgent requirement"
}
```

**Required Fields:**

- All fields from CreateTaskProgressDTO
- `clientId` (long) - Client ID
- `requestedProducts` (string, max 2000) - Products/services requested

**Optional Fields:**

- `specialNotes` (string, max 2000) - Special notes for the offer request

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"progressId": 500,
		"offerRequestId": 25,
		"message": "Progress and offer request created successfully"
	},
	"message": "Task progress created and offer request triggered successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors or invalid task/client
- **401 Unauthorized:** Not authenticated

---

### 2.3 Get My Task Progress

**Endpoint:** `GET /api/TaskProgress`

**Purpose:** Get all my task progress records (filtered by date range)

**Query Parameters:**

- `startDate` (DateTime, optional) - Filter start date
- `endDate` (DateTime, optional) - Filter end date

**Request Example:**

```http
GET /api/TaskProgress?startDate=2025-10-01T00:00:00Z&endDate=2025-10-31T23:59:59Z
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
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"employeeId": "salesman-123",
			"employeeName": "Ahmed Mohamed",
			"progressDate": "2025-10-29T10:00:00Z",
			"progressType": "Visit",
			"description": "Visited client",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer",
			"offerRequestId": 25,
			"offerId": null,
			"dealId": null,
			"createdAt": "2025-10-29T10:05:00Z"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 2.4 Get Progress for Specific Task

**Endpoint:** `GET /api/TaskProgress/task/{taskId}`

**Purpose:** Get all progress records for a specific task

**Path Parameters:**

- `taskId` (long) - Task ID

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"progressDate": "2025-10-29T10:00:00Z",
			"progressType": "Visit",
			"description": "Initial visit",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer"
		},
		{
			"id": 501,
			"progressDate": "2025-10-30T14:00:00Z",
			"progressType": "Call",
			"description": "Follow-up call",
			"visitResult": null,
			"nextStep": null
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **403 Forbidden:** Task doesn't belong to you

---

### 2.5 Get Client Visit History

**Endpoint:** `GET /api/TaskProgress/by-client/{clientId}`

**Purpose:** Get all visits/progress for a specific client

**Path Parameters:**

- `clientId` (long) - Client ID

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"taskId": 101,
			"progressDate": "2025-10-29T10:00:00Z",
			"progressType": "Visit",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer"
		}
	],
	"message": "Client progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **403 Forbidden:** Client not assigned to you

---

### 2.6 Update Task Progress

**Endpoint:** `PUT /api/TaskProgress/{id}`

**Purpose:** Update an existing progress record (only if it's yours)

**Path Parameters:**

- `id` (long) - Progress ID

**Request Body:** Same structure as CreateTaskProgressDTO (all fields optional, only send what you want to update)

```json
{
	"description": "Updated description",
	"notes": "Updated notes",
	"satisfactionRating": 5
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"description": "Updated description",
		"notes": "Updated notes",
		"satisfactionRating": 5,
		"createdAt": "2025-10-29T10:05:00Z"
	},
	"message": "Task progress updated successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors
- **403 Forbidden:** Progress doesn't belong to you
- **404 Not Found:** Progress record doesn't exist

---

## 🔔 Section 3: Offer Requests

Offer Requests are created when a client shows interest and needs an offer. You create the request, and SalesSupport will create the actual offer.

---

### 3.1 Create Offer Request

**Endpoint:** `POST /api/OfferRequest`

**Purpose:** Request an offer to be created for a client

**Request Body:**

```json
{
	"clientId": 123,
	"taskProgressId": 500,
	"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device Model ABC",
	"specialNotes": "Client needs delivery within 2 weeks, urgent requirement"
}
```

**Required Fields:**

- `clientId` (long) - Client ID
- `requestedProducts` (string, max 2000) - Products/services requested

**Optional Fields:**

- `taskProgressId` (long) - Related task progress ID
- `specialNotes` (string, max 2000) - Special instructions

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"requestedBy": "salesman-123",
		"requestedByName": "Ahmed Mohamed",
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Client needs delivery within 2 weeks",
		"requestDate": "2025-10-29T12:00:00Z",
		"status": "Pending",
		"assignedTo": null,
		"assignedToName": null,
		"createdOfferId": null
	},
	"message": "Offer request created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors or client doesn't exist
- **401 Unauthorized:** Not authenticated

---

### 3.2 Get Offer Requests

**Endpoint:** `GET /api/OfferRequest`

**Purpose:** Get offer requests (filtered by your role - salesmen see only their requests)

**Query Parameters:**

- `status` (string, optional) - Filter by status: "Pending", "Assigned", "InProgress", "Completed", "Cancelled"
- `requestedBy` (string, optional) - Auto-filled to your user ID for salesmen

**Request Example:**

```http
GET /api/OfferRequest?status=Pending
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 25,
			"requestedBy": "salesman-123",
			"requestedByName": "Ahmed Mohamed",
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"requestedProducts": "X-Ray Machine",
			"requestDate": "2025-10-29T12:00:00Z",
			"status": "Assigned",
			"assignedTo": "support-456",
			"assignedToName": "Support User",
			"createdOfferId": null
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 3.3 Get Offer Request Details

**Endpoint:** `GET /api/OfferRequest/{id}`

**Purpose:** Get detailed information about an offer request

**Path Parameters:**

- `id` (long) - Offer request ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"requestedBy": "salesman-123",
		"requestedByName": "Ahmed Mohamed",
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Client needs fast delivery",
		"status": "Assigned",
		"assignedTo": "support-456",
		"assignedToName": "Support User",
		"createdOfferId": 50,
		"requestDate": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Request doesn't exist
- **403 Forbidden:** Not your request

---

### 3.4 Get My Offer Requests

**Endpoint:** `GET /api/OfferRequest/my-requests`

**Purpose:** Get all my offer requests with optional status filter

**Query Parameters:**

- `status` (string, optional) - Filter by status: "Pending", "Assigned", "InProgress", "Completed", "Cancelled"

**Request Example:**

```http
GET /api/OfferRequest/my-requests?status=Completed
Authorization: Bearer {token}
```

**Success Response (200):**

Same format as endpoint 3.2

---

## 📄 Section 4: Offers

Offers are created by SalesSupport based on your offer requests. Once created and assigned to you, you can view them and export them as PDF to share with clients.

---

### 4.1 Get Offers Assigned to Me

**Endpoint:** `GET /api/Offer/assigned-to-me`

**Purpose:** Get all offers assigned to me (sent by SalesSupport)

**Request Example:**

```http
GET /api/Offer/assigned-to-me
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 50,
			"offerRequestId": 25,
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"products": "X-Ray Machine Model XYZ",
			"totalAmount": 50000.0,
			"paymentTerms": "50% advance, 50% on delivery",
			"deliveryTerms": "2 weeks delivery",
			"validUntil": "2025-11-15T23:59:59Z",
			"status": "Sent",
			"sentToClientAt": "2025-10-25T10:00:00Z",
			"clientResponse": null,
			"createdAt": "2025-10-25T09:00:00Z"
		}
	],
	"message": "Retrieved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 4.2 Get Offer Details

**Endpoint:** `GET /api/Offer/{id}`

**Purpose:** Get full offer details including equipment, terms, and installments

**Path Parameters:**

- `id` (long) - Offer ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"offerRequestId": 25,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"createdBy": "support-456",
		"createdByName": "Support User",
		"assignedTo": "salesman-123",
		"assignedToName": "Ahmed Mohamed",
		"products": "X-Ray Machine Model XYZ",
		"totalAmount": 50000.0,
		"finalPrice": 48000.0,
		"paymentTerms": "50% advance, 50% on delivery",
		"deliveryTerms": "2 weeks delivery",
		"paymentType": "Installments",
		"offerDuration": "3 months",
		"validUntil": "2025-11-15T23:59:59Z",
		"status": "Sent",
		"sentToClientAt": "2025-10-25T10:00:00Z",
		"equipment": [
			{
				"id": 1,
				"offerId": 50,
				"name": "X-Ray Machine",
				"model": "Model XYZ",
				"provider": "Provider ABC",
				"country": "Germany",
				"imagePath": "/uploads/equipment/xray.jpg",
				"price": 50000.0,
				"description": "High-quality X-Ray machine",
				"inStock": true
			}
		],
		"terms": {
			"id": 1,
			"offerId": 50,
			"warrantyPeriod": "2 years",
			"deliveryTime": "2 weeks",
			"maintenanceTerms": "Free maintenance for 1 year",
			"otherTerms": "Installation included"
		},
		"installments": [
			{
				"id": 1,
				"offerId": 50,
				"installmentNumber": 1,
				"amount": 24000.0,
				"dueDate": "2025-11-15T00:00:00Z",
				"status": "Pending",
				"notes": null
			},
			{
				"id": 2,
				"offerId": 50,
				"installmentNumber": 2,
				"amount": 24000.0,
				"dueDate": "2025-12-15T00:00:00Z",
				"status": "Pending",
				"notes": null
			}
		],
		"createdAt": "2025-10-25T09:00:00Z"
	},
	"message": "Offer retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Offer doesn't exist

---

### 4.3 Export Offer as PDF

**Endpoint:** `GET /api/Offer/{offerId}/export-pdf`

**Purpose:** Download offer as PDF file

**Path Parameters:**

- `offerId` (long) - Offer ID

**Success Response (200):**

- **Content-Type:** `application/pdf`
- **Body:** PDF file bytes
- **Headers:** `Content-Disposition: attachment; filename="offer-50.pdf"`

**Usage in Frontend:**

```javascript
const response = await fetch(`/api/Offer/${offerId}/export-pdf`, {
	headers: { Authorization: `Bearer ${token}` },
});
const blob = await response.blob();
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = `offer-${offerId}.pdf`;
a.click();
```

**Error Responses:**

- **404 Not Found:** Offer doesn't exist

---

## 🤝 Section 5: Deals

Deals are created when a client accepts an offer. Salesmen can create deals and track their approval status through manager and SuperAdmin.

---

### 5.1 Create Deal

**Endpoint:** `POST /api/Deal`

**Purpose:** Create a new deal when client accepts an offer

**Request Body:**

```json
{
	"offerId": 50,
	"clientId": 123,
	"dealValue": 48000.0,
	"paymentTerms": "50% advance, 50% on delivery",
	"deliveryTerms": "2 weeks delivery",
	"expectedDeliveryDate": "2025-11-12T00:00:00Z",
	"notes": "Client accepted offer, waiting for manager approval"
}
```

**Required Fields:**

- `offerId` (long) - ID of the accepted offer
- `clientId` (long) - Client ID
- `dealValue` (decimal) - Final deal value

**Optional Fields:**

- `paymentTerms` (string, max 2000) - Payment terms
- `deliveryTerms` (string, max 2000) - Delivery terms
- `expectedDeliveryDate` (DateTime) - Expected delivery date
- `notes` (string, max 2000) - Additional notes

**Note:** The deal status is automatically set to "PendingManagerApproval" when created.

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"offerId": 50,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"salesmanId": "salesman-123",
		"salesmanName": "Ahmed Mohamed",
		"dealValue": 48000.0,
		"closedDate": "2025-10-29T14:00:00Z",
		"status": "PendingManagerApproval",
		"managerRejectionReason": null,
		"managerComments": null,
		"superAdminRejectionReason": null,
		"superAdminComments": null,
		"createdAt": "2025-10-29T14:00:00Z"
	},
	"message": "Deal created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors, offer already has a deal, or offer not assigned to you
- **401 Unauthorized:** Not authenticated

---

### 5.2 Get My Deals

**Endpoint:** `GET /api/Deal`

**Purpose:** Get all my deals with optional status filter

**Query Parameters:**

- `status` (string, optional) - Filter by status
- `salesmanId` (string, optional) - Auto-filled to your ID for salesmen

**Request Example:**

```http
GET /api/Deal?status=PendingManagerApproval
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 15,
			"offerId": 50,
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"salesmanId": "salesman-123",
			"salesmanName": "Ahmed Mohamed",
			"dealValue": 48000.0,
			"closedDate": "2025-10-29T14:00:00Z",
			"status": "PendingManagerApproval",
			"createdAt": "2025-10-29T14:00:00Z"
		}
	],
	"message": "Deals retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 5.3 Get Deal Details

**Endpoint:** `GET /api/Deal/{id}`

**Purpose:** Get full deal details including approval status

**Path Parameters:**

- `id` (long) - Deal ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"offerId": 50,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"salesmanId": "salesman-123",
		"salesmanName": "Ahmed Mohamed",
		"dealValue": 48000.0,
		"closedDate": "2025-10-29T14:00:00Z",
		"status": "PendingManagerApproval",
		"managerRejectionReason": null,
		"managerComments": null,
		"superAdminRejectionReason": null,
		"superAdminComments": null,
		"createdAt": "2025-10-29T14:00:00Z"
	},
	"message": "Deal retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Deal doesn't exist
- **403 Forbidden:** Deal doesn't belong to you

---

### 5.4 Get Client's Deals

**Endpoint:** `GET /api/Deal/by-client/{clientId}`

**Purpose:** Get all deals for a specific client

**Path Parameters:**

- `clientId` (long) - Client ID

**Success Response (200):**

Same format as endpoint 5.2

**Error Responses:**

- **403 Forbidden:** Client not assigned to you

---

## 📊 Status Values Reference

### Task Status:

- `"Pending"` - Task not started
- `"InProgress"` - Task in progress
- `"Completed"` - Task completed
- `"Cancelled"` - Task cancelled

### Progress Types:

- `"Visit"` - Physical visit
- `"Call"` - Phone call
- `"Meeting"` - Meeting
- `"Email"` - Email communication

### Visit Results:

- `"Interested"` - Client is interested
- `"NotInterested"` - Client is not interested

### Next Steps:

- `"NeedsOffer"` - Client needs an offer
- `"NeedsDeal"` - Client ready for deal

### Offer Request Status:

- `"Pending"` - Just created, not assigned
- `"Assigned"` - Assigned to SalesSupport
- `"InProgress"` - Offer being created
- `"Completed"` - Offer created and sent
- `"Cancelled"` - Request cancelled

### Offer Status:

- `"Draft"` - Being created (not visible to salesmen)
- `"Sent"` - Sent to salesman/client
- `"Accepted"` - Client accepted
- `"Rejected"` - Client rejected
- `"Expired"` - Past validUntil date

### Deal Status:

- `"PendingManagerApproval"` - Waiting for manager approval
- `"PendingSuperAdminApproval"` - Manager approved, waiting for SuperAdmin
- `"Approved"` - Fully approved
- `"Rejected"` - Rejected by manager/SuperAdmin
- `"Completed"` - Deal completed
- `"Failed"` - Deal failed

---

## 🔄 Common Workflows

### Workflow 1: Complete Sales Cycle

1. **Create Weekly Plan** → `POST /api/WeeklyPlan`
2. **Record Visit Progress** → `POST /api/TaskProgress` (with `visitResult: "Interested"`, `nextStep: "NeedsOffer"`)
3. **Create Offer Request** → `POST /api/OfferRequest` or use `POST /api/TaskProgress/with-offer-request`
4. **Check Request Status** → `GET /api/OfferRequest/my-requests`
5. **Wait for SalesSupport to create offer**
6. **Get Assigned Offer** → `GET /api/Offer/assigned-to-me`
7. **View Offer Details** → `GET /api/Offer/{id}`
8. **Export Offer PDF** → `GET /api/Offer/{id}/export-pdf`
9. **Share PDF with client**
10. **When client accepts, create deal** → `POST /api/Deal`
11. **Track Deal Status** → `GET /api/Deal/{id}`

### Workflow 2: Quick Offer Request

1. **Record Visit with Offer Request** → `POST /api/TaskProgress/with-offer-request`
2. **Check request status** → `GET /api/OfferRequest/my-requests`
3. **Follow Workflow 1 steps 5-11**

### Workflow 3: View Client History

1. **Get Client Profile** → `GET /api/Client/{id}/profile` (includes visits, offers, deals)
2. **Get Client Visit History** → `GET /api/TaskProgress/by-client/{clientId}`
3. **Get Client's Deals** → `GET /api/Deal/by-client/{clientId}`

---

## ⚠️ Error Handling Guide

### Always Check Response Structure:

```javascript
const response = await fetch(url, options);
const result = await response.json();

if (result.success) {
	// Use result.data
	console.log(result.data);
} else {
	// Handle error
	console.error(result.message);
	if (result.errors) {
		// Validation errors
		console.error(result.errors);
	}
}
```

### Common Error Codes:

- **200** - Success
- **201** - Created
- **400** - Bad Request (validation errors)
- **401** - Unauthorized (missing/invalid token)
- **403** - Forbidden (no permission)
- **404** - Not Found
- **500** - Internal Server Error

---

## 🚀 Quick Start Tips

1. **Always include Authorization header** with JWT token
2. **Check `result.success`** before using `result.data`
3. **Handle pagination** for list endpoints (page, pageSize)
4. **Use date filters** efficiently (startDate, endDate)
5. **Validate required fields** before POST/PUT requests
6. **Handle async responses** properly with try-catch
7. **Track task progress** to maintain client history
8. **Export offers as PDF** before sharing with clients

---

## 📝 Notes for Salesmen

### Tasks Management:

- Tasks are created and managed as part of Weekly Plans
- You cannot directly create, update, or delete individual tasks outside of a weekly plan
- Tasks are visible when you retrieve a weekly plan (`GET /api/WeeklyPlan/{id}`)
- Task status updates automatically based on progress records

### Progress Recording:

- Always record progress after client interactions
- Use appropriate `progressType` (Visit, Call, Meeting, Email)
- Set `visitResult` and `nextStep` to trigger workflows
- Record satisfaction rating and feedback for better tracking

### Offer Requests:

- Create offer requests when clients show interest
- Use the combined endpoint (`/with-offer-request`) for convenience
- Track request status to know when offers are ready

### Deals:

- Only create deals when client officially accepts an offer
- Deal approval is handled by managers and SuperAdmin
- Track deal status to know approval progress

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29
