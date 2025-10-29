# 📱 Salesman API Documentation

## 🎯 User Stories

### As a Salesman, I want to:

1. **View my statistics** so I can track my performance (visits, offers, deals)
2. **Manage my clients** (search, create, update, view profile)
3. **Create weekly plans** and track my tasks
4. **Record visit progress** when I visit clients
5. **Request offers** for interested clients
6. **View offers assigned to me** and export them as PDF
7. **Create deals** when clients accept offers
8. **View my targets** and track my progress

---

## 🔐 Authentication

**All endpoints require JWT token in Authorization header:**

```
Authorization: Bearer {your-jwt-token}
```

**Response Format:**

- **Success (200):** `{ success: true, data: {...}, message: "...", timestamp: "..." }`
- **Error (400/404/500):** `{ success: false, message: "...", errors: {...}, timestamp: "..." }`

---

## 📋 Endpoints Overview

| #   | Endpoint                                 | Method | Purpose                                |
| --- | ---------------------------------------- | ------ | -------------------------------------- |
| 1   | `/api/Client/search`                     | GET    | Search for clients                     |
| 2   | `/api/Client`                            | POST   | Create new client                      |
| 3   | `/api/Client/{id}`                       | GET    | Get client details                     |
| 4   | `/api/Client/{id}`                       | PUT    | Update client                          |
| 5   | `/api/Client/my-clients`                 | GET    | Get my assigned clients                |
| 6   | `/api/Client/follow-up-needed`           | GET    | Get clients needing follow-up          |
| 7   | `/api/Client/statistics`                 | GET    | Get my client statistics               |
| 8   | `/api/Client/{id}/profile`               | GET    | Get complete client profile            |
| 9   | `/api/SalesmanStatistics/my-statistics`  | GET    | Get my statistics                      |
| 10  | `/api/SalesmanStatistics/my-progress`    | GET    | Get my progress vs targets             |
| 11  | `/api/SalesmanStatistics/my-targets`     | GET    | Get my targets for a year              |
| 12  | `/api/WeeklyPlan`                        | POST   | Create weekly plan                     |
| 13  | `/api/WeeklyPlan`                        | GET    | Get my weekly plans                    |
| 14  | `/api/WeeklyPlan/{id}`                   | GET    | Get specific weekly plan               |
| 15  | `/api/WeeklyPlan/{id}`                   | PUT    | Update weekly plan                     |
| 16  | `/api/WeeklyPlan/{id}/submit`            | POST   | Submit weekly plan                     |
| 17  | `/api/WeeklyPlan/current`                | GET    | Get current week's plan                |
| 18  | `/api/TaskProgress`                      | POST   | Record task progress                   |
| 19  | `/api/TaskProgress/with-offer-request`   | POST   | Record progress + create offer request |
| 20  | `/api/TaskProgress`                      | GET    | Get my task progress                   |
| 21  | `/api/TaskProgress/task/{taskId}`        | GET    | Get progress for a task                |
| 22  | `/api/TaskProgress/by-client/{clientId}` | GET    | Get client visit history               |
| 23  | `/api/TaskProgress/{id}`                 | PUT    | Update task progress                   |
| 24  | `/api/OfferRequest`                      | POST   | Create offer request                   |
| 25  | `/api/OfferRequest`                      | GET    | Get my offer requests                  |
| 26  | `/api/OfferRequest/{id}`                 | GET    | Get offer request details              |
| 27  | `/api/OfferRequest/my-requests`          | GET    | Get my offer requests                  |
| 28  | `/api/Offer/assigned-to-me`              | GET    | Get offers assigned to me              |
| 29  | `/api/Offer/{id}`                        | GET    | Get offer details                      |
| 30  | `/api/Offer/{offerId}/export-pdf`        | GET    | Export offer as PDF                    |
| 31  | `/api/Deal`                              | POST   | Create new deal                        |
| 32  | `/api/Deal`                              | GET    | Get my deals                           |
| 33  | `/api/Deal/{id}`                         | GET    | Get deal details                       |
| 34  | `/api/Deal/by-client/{clientId}`         | GET    | Get client's deals                     |

---

## 🔍 Detailed Endpoint Documentation

### 1. Search Clients

**Endpoint:** `GET /api/Client/search`

**Purpose:** Search for clients by name, email, specialization, or location

**Query Parameters:**

- `searchTerm` (string, optional) - Search query
- `status` (string, optional) - Filter by status: "Potential", "Active", "Inactive", "Lost"
- `priority` (string, optional) - Filter by priority: "Low", "Medium", "High"
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20, max: 100) - Items per page

**Request Example:**

```http
GET /api/Client/search?searchTerm=ahmed&status=Active&page=1&pageSize=20
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "Ahmed Hospital",
			"type": "Hospital",
			"specialization": "Cardiology",
			"location": "Cairo",
			"phone": "+20123456789",
			"email": "ahmed@hospital.com",
			"status": "Active",
			"priority": "High",
			"assignedTo": "salesman-123"
		}
	],
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Invalid pagination parameters
     ```json
     {
     	"success": false,
     	"message": "معاملات الصفحة غير صحيحة",
     	"timestamp": "2025-10-29T12:00:00Z"
     }
     ```
- **401 Unauthorized:** Missing or invalid token
- **500 Internal Server Error:** Server error

---

### 2. Create Client

**Endpoint:** `POST /api/Client`

**Purpose:** Create a new client record

**Request Body:**

```json
{
	"name": "New Hospital",
	"type": "Hospital",
	"specialization": "Oncology",
	"location": "Alexandria",
	"phone": "+20198765432",
	"email": "hospital@example.com",
	"website": "https://hospital.com",
	"address": "123 Main St",
	"city": "Alexandria",
	"governorate": "Alexandria",
	"postalCode": "12345",
	"notes": "New client, high priority",
	"status": "Potential",
	"priority": "High",
	"potentialValue": 500000,
	"contactPerson": "Dr. Mohamed",
	"contactPersonPhone": "+20198887766",
	"contactPersonEmail": "mohamed@hospital.com",
	"contactPersonPosition": "Director"
}
```

**Required Fields:**

- `name` (string, max 200)
- `type` (string, max 50) - "Hospital", "Clinic", "Pharmacy", etc.

**Optional Fields:**

- `specialization`, `location`, `phone`, `email`, `website`, `address`, `city`, `governorate`, `postalCode`, `notes`, `status`, `priority`, `potentialValue`, `contactPerson`, etc.

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"name": "New Hospital",
		"type": "Hospital",
		"specialization": "Oncology",
		"status": "Potential",
		"priority": "High",
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors
     ```json
     {
     	"success": false,
     	"message": "اسم العميل مطلوب, نوع العميل مطلوب",
     	"timestamp": "2025-10-29T12:00:00Z"
     }
     ```
- **401 Unauthorized:** Not authenticated

---

### 3. Get Client Details

**Endpoint:** `GET /api/Client/{id}`

**Purpose:** Get full details of a specific client

**Path Parameters:**

- `id` (long) - Client ID

**Request Example:**

```http
GET /api/Client/123
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"name": "Ahmed Hospital",
		"type": "Hospital",
		"specialization": "Cardiology",
		"location": "Cairo",
		"phone": "+20123456789",
		"email": "ahmed@hospital.com",
		"status": "Active",
		"priority": "High",
		"assignedTo": "salesman-123",
		"createdAt": "2025-01-15T10:00:00Z",
		"updatedAt": "2025-10-29T11:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Client doesn't exist
     ```json
     {
     	"success": false,
     	"message": "العميل غير موجود",
     	"timestamp": "2025-10-29T12:00:00Z"
     }
     ```
- **401 Unauthorized:** Not authenticated

---

### 4. Update Client

**Endpoint:** `PUT /api/Client/{id}`

**Purpose:** Update client information

**Path Parameters:**

- `id` (long) - Client ID

**Request Body:** (All fields optional - only send what you want to update)

```json
{
	"name": "Updated Hospital Name",
	"phone": "+20987654321",
	"status": "Active",
	"priority": "Medium",
	"notes": "Updated notes"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"name": "Updated Hospital Name",
		"phone": "+20987654321",
		"status": "Active",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors
- **404 Not Found:** Client doesn't exist

---

### 5. Get My Clients

**Endpoint:** `GET /api/Client/my-clients`

**Purpose:** Get paginated list of clients assigned to me

**Query Parameters:**

- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)

**Request Example:**

```http
GET /api/Client/my-clients?page=1&pageSize=20
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "Client 1",
			"type": "Hospital",
			"status": "Active",
			"priority": "High"
		},
		{
			"id": 2,
			"name": "Client 2",
			"type": "Clinic",
			"status": "Potential",
			"priority": "Medium"
		}
	],
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Invalid pagination
- **401 Unauthorized:** Not authenticated

---

### 6. Get Clients Needing Follow-up

**Endpoint:** `GET /api/Client/follow-up-needed`

**Purpose:** Get clients that need follow-up based on NextContactDate

**Request Example:**

```http
GET /api/Client/follow-up-needed
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 10,
			"name": "Client Needing Follow-up",
			"phone": "+20123456789",
			"nextContactDate": "2025-10-30T00:00:00Z",
			"status": "Active"
		}
	],
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 7. Get Client Statistics

**Endpoint:** `GET /api/Client/statistics`

**Purpose:** Get statistics about my clients (visits, offers, deals)

**Request Example:**

```http
GET /api/Client/statistics
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"totalVisits": 45,
		"totalOffers": 12,
		"successfulDeals": 8,
		"failedDeals": 2,
		"totalRevenue": 2500000.0,
		"averageSatisfaction": 4.5
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 8. Get Client Profile

**Endpoint:** `GET /api/Client/{id}/profile`

**Purpose:** Get complete client profile with visit history, offers, and deals

**Path Parameters:**

- `id` (long) - Client ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"clientInfo": {
			"id": 123,
			"name": "Ahmed Hospital",
			"type": "Hospital",
			"status": "Active"
		},
		"allVisits": [
			{
				"id": 1,
				"progressDate": "2025-10-15T10:00:00Z",
				"progressType": "Visit",
				"visitResult": "Interested",
				"nextStep": "NeedsOffer"
			}
		],
		"allOffers": [
			{
				"id": 10,
				"totalAmount": 50000,
				"status": "Sent",
				"createdAt": "2025-10-20T09:00:00Z"
			}
		],
		"allDeals": [
			{
				"id": 5,
				"dealValue": 50000,
				"status": "Completed",
				"closedDate": "2025-10-25T14:00:00Z"
			}
		],
		"statistics": {
			"totalVisits": 10,
			"totalOffers": 3,
			"successfulDeals": 2
		}
	},
	"message": "Client profile retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 9. Get My Statistics

**Endpoint:** `GET /api/SalesmanStatistics/my-statistics`

**Purpose:** Get my sales statistics (visits, offers, deals) for a period

**Query Parameters:**

- `year` (int, optional) - Year (default: current year)
- `quarter` (int, optional) - Quarter 1-4

**Request Example:**

```http
GET /api/SalesmanStatistics/my-statistics?year=2025&quarter=4
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"salesmanId": "salesman-123",
		"salesmanName": "Ahmed Mohamed",
		"year": 2025,
		"quarter": 4,
		"totalVisits": 120,
		"successfulVisits": 85,
		"failedVisits": 35,
		"visitSuccessRate": 70.83,
		"totalOffers": 25,
		"acceptedOffers": 15,
		"rejectedOffers": 10,
		"offerAcceptanceRate": 60.0,
		"totalDeals": 15,
		"completedDeals": 12,
		"failedDeals": 3,
		"totalRevenue": 1500000.0,
		"averageDealValue": 100000.0
	},
	"message": "Statistics retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 10. Get My Progress

**Endpoint:** `GET /api/SalesmanStatistics/my-progress`

**Purpose:** Get my progress vs targets for a period

**Query Parameters:**

- `year` (int, required) - Year
- `quarter` (int, optional) - Quarter 1-4

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"salesmanId": "salesman-123",
		"year": 2025,
		"quarter": 4,
		"statistics": {
			"totalVisits": 120,
			"successfulVisits": 85,
			"totalOffers": 25,
			"totalDeals": 15
		},
		"targets": [
			{
				"targetVisits": 100,
				"targetSuccessfulVisits": 70,
				"targetOffers": 20,
				"targetDeals": 12,
				"progressVisits": 120.0,
				"progressSuccessfulVisits": 121.43,
				"progressOffers": 125.0,
				"progressDeals": 125.0
			}
		]
	},
	"message": "Progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 11. Get My Targets

**Endpoint:** `GET /api/SalesmanStatistics/my-targets`

**Purpose:** Get my targets for a specific year

**Query Parameters:**

- `year` (int, required) - Year

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"year": 2025,
			"quarter": 4,
			"targetVisits": 100,
			"targetSuccessfulVisits": 70,
			"targetOffers": 20,
			"targetDeals": 12,
			"targetOfferAcceptanceRate": 60.0,
			"isTeamTarget": false
		}
	],
	"message": "Targets retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 12. Create Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan`

**Purpose:** Create a new weekly plan

**Request Body:**

```json
{
	"weekStartDate": "2025-11-04T00:00:00Z",
	"weekEndDate": "2025-11-10T23:59:59Z",
	"title": "Week 45 Plan",
	"description": "Focus on high priority clients"
}
```

**Required Fields:**

- `weekStartDate` (DateTime)
- `weekEndDate` (DateTime)
- `title` (string, max 200)

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"employeeId": "salesman-123",
		"weekStartDate": "2025-11-04T00:00:00Z",
		"weekEndDate": "2025-11-10T23:59:59Z",
		"title": "Week 45 Plan",
		"description": "Focus on high priority clients",
		"isActive": true,
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Plan already exists for this week, or validation errors

---

### 13. Get My Weekly Plans

**Endpoint:** `GET /api/WeeklyPlan`

**Purpose:** Get my weekly plans with pagination and optional date filters

**Query Parameters:**

- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `weekStartDate` (DateTime, optional) - Filter start date
- `weekEndDate` (DateTime, optional) - Filter end date

**Note:** Salesmen can ONLY filter by date. Cannot filter by employeeId or isViewed.

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
				"title": "Week 45 Plan",
				"isActive": true,
				"isViewed": false
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

### 14. Get Specific Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/{id}`

**Purpose:** Get detailed weekly plan with all tasks

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
		"title": "Week 45 Plan",
		"description": "Focus on high priority clients",
		"isActive": true,
		"tasks": [
			{
				"id": 101,
				"clientId": 123,
				"clientName": "Ahmed Hospital",
				"taskType": "Visit",
				"priority": "High",
				"plannedDate": "2025-11-05T10:00:00Z",
				"status": "Pending"
			}
		]
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Plan doesn't exist or not yours

---

### 15. Update Weekly Plan

**Endpoint:** `PUT /api/WeeklyPlan/{id}`

**Purpose:** Update weekly plan title/description (only if not submitted)

**Request Body:**

```json
{
	"title": "Updated Week Plan",
	"description": "Updated description"
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"title": "Updated Week Plan",
		"description": "Updated description",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 16. Submit Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/submit`

**Purpose:** Submit weekly plan for manager review (cannot edit after submission)

**Success Response (200):**

```json
{
	"success": true,
	"data": "Weekly plan submitted successfully",
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Plan already submitted or has no tasks
- **404 Not Found:** Plan doesn't exist

---

### 17. Get Current Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/current`

**Purpose:** Get the current week's plan

**Success Response (200):**

```json
{
  "success": true,
  "data": {
    "id": 50,
    "weekStartDate": "2025-10-28T00:00:00Z",
    "weekEndDate": "2025-11-03T23:59:59Z",
    "title": "Current Week Plan",
    "tasks": [...]
  },
  "message": null,
  "timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** No plan exists for current week

---

### 18. Record Task Progress

**Endpoint:** `POST /api/TaskProgress`

**Purpose:** Record progress for a task (visit, call, meeting, etc.)

**Request Body:**

```json
{
	"taskId": 101,
	"progressDate": "2025-10-29T10:00:00Z",
	"progressType": "Visit",
	"description": "Visited client, discussed equipment needs",
	"notes": "Client showed interest",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-11-05T10:00:00Z",
	"followUpNotes": "Follow up on offer",
	"satisfactionRating": 4,
	"feedback": "Very positive meeting"
}
```

**Required Fields:**

- `taskId` (long)
- `progressDate` (DateTime)
- `progressType` (string) - "Visit", "Call", "Meeting", "Email"

**Optional Fields:**

- `visitResult` (string) - "Interested", "NotInterested"
- `nextStep` (string) - "NeedsDeal", "NeedsOffer"
- `satisfactionRating` (int, 1-5)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 101,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"progressDate": "2025-10-29T10:00:00Z",
		"progressType": "Visit",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"createdAt": "2025-10-29T10:05:00Z"
	},
	"message": "Task progress created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 19. Record Progress + Create Offer Request

**Endpoint:** `POST /api/TaskProgress/with-offer-request`

**Purpose:** Record progress AND automatically create an offer request (when client is interested)

**Request Body:**

```json
{
	"taskId": 101,
	"progressDate": "2025-10-29T10:00:00Z",
	"progressType": "Visit",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"offerRequest": {
		"clientId": 123,
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Client needs fast delivery"
	}
}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"progressId": 500,
		"offerRequestId": 25,
		"message": "Progress and offer request created"
	},
	"message": "Task progress created and offer request triggered successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 20. Get My Task Progress

**Endpoint:** `GET /api/TaskProgress`

**Purpose:** Get all my task progress records (filtered by date range)

**Query Parameters:**

- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"taskId": 101,
			"clientName": "Ahmed Hospital",
			"progressDate": "2025-10-29T10:00:00Z",
			"progressType": "Visit",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 21. Get Progress for Task

**Endpoint:** `GET /api/TaskProgress/task/{taskId}`

**Purpose:** Get all progress records for a specific task

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 500,
			"progressDate": "2025-10-29T10:00:00Z",
			"progressType": "Visit",
			"visitResult": "Interested"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **403 Forbidden:** Task doesn't belong to you

---

### 22. Get Client Visit History

**Endpoint:** `GET /api/TaskProgress/by-client/{clientId}`

**Purpose:** Get all visits/progress for a specific client

**Success Response (200):** Same format as endpoint #20

**Error Responses:**

- **403 Forbidden:** Client not assigned to you

---

### 23. Update Task Progress

**Endpoint:** `PUT /api/TaskProgress/{id}`

**Purpose:** Update an existing progress record (only if it's yours)

**Request Body:** Same as endpoint #18

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 500,
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Task progress updated successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **403 Forbidden:** Progress doesn't belong to you
- **404 Not Found:** Progress record doesn't exist

---

### 24. Create Offer Request

**Endpoint:** `POST /api/OfferRequest`

**Purpose:** Request an offer to be created for a client

**Request Body:**

```json
{
	"clientId": 123,
	"taskProgressId": 500,
	"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device",
	"specialNotes": "Client needs delivery within 2 weeks"
}
```

**Required Fields:**

- `clientId` (long)
- `requestedProducts` (string, max 2000)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"status": "Pending",
		"requestDate": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 25. Get My Offer Requests

**Endpoint:** `GET /api/OfferRequest` or `GET /api/OfferRequest/my-requests`

**Purpose:** Get my offer requests

**Query Parameters:**

- `status` (string, optional) - "Pending", "Assigned", "InProgress", "Completed", "Cancelled"
- `requestedBy` (string, optional) - Your user ID (auto-filled)

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 25,
			"clientName": "Ahmed Hospital",
			"requestedProducts": "X-Ray Machine",
			"status": "Assigned",
			"assignedTo": "support-456",
			"requestDate": "2025-10-29T12:00:00Z"
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 26. Get Offer Request Details

**Endpoint:** `GET /api/OfferRequest/{id}`

**Purpose:** Get detailed information about an offer request

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
- **403 Forbidden:** Not your request (unless you're manager/support)

---

### 27. Get Offers Assigned to Me

**Endpoint:** `GET /api/Offer/assigned-to-me`

**Purpose:** Get all offers assigned to me (sent by SalesSupport)

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 50,
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"products": "X-Ray Machine Model XYZ",
			"totalAmount": 50000.0,
			"status": "Sent",
			"validUntil": "2025-11-15T23:59:59Z",
			"createdAt": "2025-10-25T09:00:00Z",
			"sentToClientAt": "2025-10-25T10:00:00Z"
		}
	],
	"message": "Retrieved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 28. Get Offer Details

**Endpoint:** `GET /api/Offer/{id}`

**Purpose:** Get full offer details including equipment, terms, installments

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"products": "X-Ray Machine Model XYZ",
		"totalAmount": 50000.0,
		"finalPrice": 48000.0,
		"status": "Sent",
		"validUntil": "2025-11-15T23:59:59Z",
		"paymentTerms": "50% advance, 50% on delivery",
		"deliveryTerms": "2 weeks delivery",
		"equipment": [
			{
				"id": 1,
				"name": "X-Ray Machine",
				"model": "Model XYZ",
				"price": 50000.0,
				"imagePath": "/uploads/equipment/xray.jpg"
			}
		],
		"terms": {
			"warrantyPeriod": "2 years",
			"deliveryTime": "2 weeks",
			"maintenanceTerms": "Free maintenance for 1 year"
		},
		"installmentPlan": [
			{
				"installmentNumber": 1,
				"amount": 24000.0,
				"dueDate": "2025-11-15T00:00:00Z",
				"status": "Pending"
			}
		]
	},
	"message": "Offer retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 29. Export Offer as PDF

**Endpoint:** `GET /api/Offer/{offerId}/export-pdf`

**Purpose:** Download offer as PDF file

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

---

### 30. Create Deal

**Endpoint:** `POST /api/Deal`

**Purpose:** Create a new deal when client accepts an offer

**Request Body:**

```json
{
	"offerId": 50,
	"clientId": 123,
	"dealValue": 48000.0,
	"closedDate": "2025-10-29T14:00:00Z",
	"paymentTerms": "50% advance, 50% on delivery",
	"deliveryTerms": "2 weeks delivery",
	"expectedDeliveryDate": "2025-11-12T00:00:00Z",
	"status": "PendingManagerApproval",
	"notes": "Client accepted offer, waiting for manager approval"
}
```

**Required Fields:**

- `clientId` (long)
- `dealValue` (decimal)
- `closedDate` (DateTime)
- `status` (string) - "PendingManagerApproval"

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"offerId": 50,
		"clientId": 123,
		"dealValue": 48000.0,
		"status": "PendingManagerApproval",
		"closedDate": "2025-10-29T14:00:00Z",
		"createdAt": "2025-10-29T14:00:00Z"
	},
	"message": "Deal created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors or offer already has a deal
- **401 Unauthorized:** Not authenticated

---

### 31. Get My Deals

**Endpoint:** `GET /api/Deal`

**Purpose:** Get deals (filtered by status and salesman)

**Query Parameters:**

- `status` (string, optional) - Filter by status
- `salesmanId` (string, optional) - For managers only (auto-filled to your ID for salesmen)

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 15,
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"dealValue": 48000.0,
			"status": "PendingManagerApproval",
			"closedDate": "2025-10-29T14:00:00Z"
		}
	],
	"message": "Deals retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 32. Get Deal Details

**Endpoint:** `GET /api/Deal/{id}`

**Purpose:** Get full deal details

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"offerId": 50,
		"clientId": 123,
		"dealValue": 48000.0,
		"status": "PendingManagerApproval",
		"closedDate": "2025-10-29T14:00:00Z",
		"managerApprovedBy": null,
		"managerApprovedAt": null,
		"superAdminApprovedBy": null,
		"notes": "Waiting for approval"
	},
	"message": "Deal retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Deal doesn't exist
- **403 Forbidden:** Deal doesn't belong to you

---

### 33. Get Client's Deals

**Endpoint:** `GET /api/Deal/by-client/{clientId}`

**Purpose:** Get all deals for a specific client

**Success Response (200):** Same format as endpoint #31

**Error Responses:**

- **403 Forbidden:** Client not assigned to you

---

## 🔄 Common Workflows

### Workflow 1: Visit Client → Request Offer → Create Deal

1. **Create Weekly Plan** → `POST /api/WeeklyPlan`
2. **Record Visit Progress** → `POST /api/TaskProgress` (with `visitResult: "Interested"`, `nextStep: "NeedsOffer"`)
3. **Create Offer Request** → `POST /api/OfferRequest`
4. **Wait for SalesSupport to create offer**
5. **Get Assigned Offer** → `GET /api/Offer/assigned-to-me`
6. **Export Offer PDF** → `GET /api/Offer/{id}/export-pdf`
7. **When client accepts, create deal** → `POST /api/Deal`

### Workflow 2: Quick Offer Request

1. **Record Visit with Offer Request** → `POST /api/TaskProgress/with-offer-request`
2. **Check request status** → `GET /api/OfferRequest/my-requests`
3. **Follow workflow 1 steps 4-7**

---

## 📊 Status Values Reference

### Client Status:

- `"Potential"` - New client, not contacted
- `"Active"` - Active client
- `"Inactive"` - Not responding
- `"Lost"` - Lost client

### Client Priority:

- `"Low"`
- `"Medium"`
- `"High"`

### Task Progress Types:

- `"Visit"`
- `"Call"`
- `"Meeting"`
- `"Email"`

### Visit Results:

- `"Interested"`
- `"NotInterested"`

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

- `"Draft"` - Being created
- `"Sent"` - Sent to salesman/client
- `"Accepted"` - Client accepted
- `"Rejected"` - Client rejected
- `"Expired"` - Past validUntil date

### Deal Status:

- `"PendingManagerApproval"` - Waiting for manager
- `"PendingSuperAdminApproval"` - Manager approved, waiting for SuperAdmin
- `"Approved"` - Fully approved
- `"Rejected"` - Rejected by manager/SuperAdmin
- `"Completed"` - Deal completed
- `"Failed"` - Deal failed

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

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29
