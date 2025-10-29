# 👔 Sales Manager API Documentation

## 🎯 User Stories

### As a Sales Manager, I want to:

1. **View team statistics** to monitor all salesmen's performance
2. **View individual salesman statistics** to track their progress
3. **Manage targets** for salesmen (create, update, delete)
4. **View team targets** and track overall performance
5. **View all clients** across my team
6. **Review weekly plans** submitted by salesmen
7. **Approve/reject deals** submitted by salesmen
8. **View all offers** and monitor offer creation
9. **Assign offer requests** to SalesSupport team
10. **View all task progress** to monitor team activities

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

| #   | Endpoint                                                    | Method | Purpose                                      |
| --- | ----------------------------------------------------------- | ------ | -------------------------------------------- |
| 1   | `/api/Client/search`                                        | GET    | Search all clients                           |
| 2   | `/api/Client/{id}`                                          | GET    | Get client details                           |
| 3   | `/api/Client/{id}/profile`                                  | GET    | Get complete client profile                  |
| 4   | `/api/SalesmanStatistics/all`                               | GET    | Get all salesmen statistics                  |
| 5   | `/api/SalesmanStatistics/{salesmanId}`                      | GET    | Get specific salesman statistics             |
| 6   | `/api/SalesmanStatistics/{salesmanId}/progress`             | GET    | Get salesman progress vs targets             |
| 7   | `/api/SalesmanStatistics/targets`                           | POST   | Create target for salesman                   |
| 8   | `/api/SalesmanStatistics/targets/{targetId}`                | PUT    | Update target                                |
| 9   | `/api/SalesmanStatistics/targets/{targetId}`                | DELETE | Delete target                                |
| 10  | `/api/SalesmanStatistics/targets/salesman/{salesmanId}`     | GET    | Get salesman targets                         |
| 11  | `/api/SalesmanStatistics/targets/team`                      | GET    | Get team targets                             |
| 12  | `/api/WeeklyPlan`                                           | GET    | Get all weekly plans (with filters)          |
| 13  | `/api/WeeklyPlan/salesmen`                                  | GET    | Get list of all salesmen                     |
| 14  | `/api/WeeklyPlan/{id}`                                      | GET    | Get weekly plan details                      |
| 15  | `/api/WeeklyPlan/{id}/review`                               | POST   | Review and rate weekly plan                  |
| 16  | `/api/TaskProgress`                                         | GET    | Get all task progress                        |
| 17  | `/api/TaskProgress/employee/{employeeId}`                   | GET    | Get progress for specific employee           |
| 18  | `/api/TaskProgress/task/{taskId}`                           | GET    | Get progress for task                        |
| 19  | `/api/TaskProgress/by-client/{clientId}`                    | GET    | Get client progress                          |
| 20  | `/api/OfferRequest`                                         | GET    | Get all offer requests                       |
| 21  | `/api/OfferRequest/{id}`                                    | GET    | Get offer request details                    |
| 22  | `/api/OfferRequest/{id}/assign`                             | PUT    | Assign request to SalesSupport               |
| 23  | `/api/OfferRequest/{id}/status`                             | PUT    | Update request status                        |
| 24  | `/api/OfferRequest/salesman/{salesmanId}`                   | GET    | Get requests by salesman                     |
| 25  | `/api/OfferRequest/assigned/{supportId}`                    | GET    | Get requests assigned to support             |
| 26  | `/api/Offer`                                                | GET    | Get all offers                               |
| 27  | `/api/Offer/my-offers`                                      | GET    | Get offers I created                         |
| 28  | `/api/Offer/{id}`                                           | GET    | Get offer details                            |
| 29  | `/api/Offer`                                                | POST   | Create new offer                             |
| 30  | `/api/Offer/{offerId}/equipment`                            | POST   | Add equipment to offer                       |
| 31  | `/api/Offer/{offerId}/equipment/{equipmentId}`              | DELETE | Delete equipment                             |
| 32  | `/api/Offer/{offerId}/equipment/{equipmentId}/upload-image` | POST   | Upload equipment image                       |
| 33  | `/api/Offer/{offerId}/terms`                                | POST   | Add/update offer terms                       |
| 34  | `/api/Offer/{offerId}/installments`                         | POST   | Create installment plan                      |
| 35  | `/api/Offer/{offerId}/send-to-salesman`                     | POST   | Send offer to salesman                       |
| 36  | `/api/Offer/by-salesman/{salesmanId}`                       | GET    | Get offers by salesman                       |
| 37  | `/api/Offer/request/{requestId}/details`                    | GET    | Get offer request details for creating offer |
| 38  | `/api/Deal`                                                 | GET    | Get all deals                                |
| 39  | `/api/Deal/{id}`                                            | GET    | Get deal details                             |
| 40  | `/api/Deal/by-salesman/{salesmanId}`                        | GET    | Get deals by salesman                        |
| 41  | `/api/Deal/{id}/manager-approval`                           | POST   | Approve/reject deal                          |
| 42  | `/api/Deal/pending-manager-approvals`                       | GET    | Get deals pending my approval                |
| 43  | `/api/Deal/{id}/complete`                                   | POST   | Mark deal as completed                       |
| 44  | `/api/Deal/{id}/fail`                                       | POST   | Mark deal as failed                          |

---

## 🔍 Detailed Endpoint Documentation

### 1. Search All Clients

**Endpoint:** `GET /api/Client/search`

**Purpose:** Search clients across all salesmen in your team

**Query Parameters:** Same as Salesman endpoint

**Success Response (200):** Same format as Salesman endpoint, but includes all team clients

---

### 2. Get Client Details

**Endpoint:** `GET /api/Client/{id}`

**Purpose:** Get any client in the system

**Success Response (200):** Same format as Salesman endpoint

---

### 3. Get Client Profile

**Endpoint:** `GET /api/Client/{id}/profile`

**Purpose:** Get complete profile of any client

**Success Response (200):** Same format as Salesman endpoint

---

### 4. Get All Salesmen Statistics

**Endpoint:** `GET /api/SalesmanStatistics/all`

**Purpose:** Get statistics for all salesmen in your team

**Query Parameters:**

- `year` (int, optional) - Year (default: current year)
- `quarter` (int, optional) - Quarter 1-4

**Request Example:**

```http
GET /api/SalesmanStatistics/all?year=2025&quarter=4
Authorization: Bearer {token}
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"year": 2025,
		"quarter": 4,
		"teamStatistics": {
			"totalVisits": 500,
			"totalOffers": 100,
			"totalDeals": 60,
			"totalRevenue": 6000000.0,
			"averageSuccessRate": 75.5
		},
		"salesmen": [
			{
				"salesmanId": "salesman-123",
				"salesmanName": "Ahmed Mohamed",
				"totalVisits": 120,
				"totalOffers": 25,
				"totalDeals": 15,
				"totalRevenue": 1500000.0,
				"successRate": 80.0
			},
			{
				"salesmanId": "salesman-456",
				"salesmanName": "Mohamed Ali",
				"totalVisits": 100,
				"totalOffers": 20,
				"totalDeals": 12,
				"totalRevenue": 1200000.0,
				"successRate": 70.0
			}
		]
	},
	"message": "Statistics retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 5. Get Specific Salesman Statistics

**Endpoint:** `GET /api/SalesmanStatistics/{salesmanId}`

**Purpose:** Get detailed statistics for a specific salesman

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Query Parameters:**

- `year` (int, optional)
- `quarter` (int, optional)

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

### 6. Get Salesman Progress vs Targets

**Endpoint:** `GET /api/SalesmanStatistics/{salesmanId}/progress`

**Purpose:** Get salesman's progress against their targets

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Query Parameters:**

- `year` (int, optional)
- `quarter` (int, optional)

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

### 7. Create Target for Salesman

**Endpoint:** `POST /api/SalesmanStatistics/targets`

**Purpose:** Create a new target for a salesman (quarterly or yearly)

**Request Body:**

```json
{
	"salesmanId": "salesman-123",
	"year": 2025,
	"quarter": 4,
	"targetVisits": 100,
	"targetSuccessfulVisits": 70,
	"targetOffers": 20,
	"targetDeals": 12,
	"targetOfferAcceptanceRate": 60.0
}
```

**Required Fields:**

- `salesmanId` (string)
- `year` (int)
- `targetVisits` (int)
- `targetSuccessfulVisits` (int)
- `targetOffers` (int)
- `targetDeals` (int)

**Optional Fields:**

- `quarter` (int, 1-4) - If omitted, target is for entire year
- `targetOfferAcceptanceRate` (decimal)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 10,
		"salesmanId": "salesman-123",
		"salesmanName": "Ahmed Mohamed",
		"year": 2025,
		"quarter": 4,
		"targetVisits": 100,
		"targetSuccessfulVisits": 70,
		"targetOffers": 20,
		"targetDeals": 12,
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": "Target created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Target already exists for this period, or validation errors

---

### 8. Update Target

**Endpoint:** `PUT /api/SalesmanStatistics/targets/{targetId}`

**Purpose:** Update an existing target

**Path Parameters:**

- `targetId` (long) - Target ID

**Request Body:** Same structure as endpoint #7 (all fields optional)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 10,
		"targetVisits": 110,
		"targetDeals": 15,
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Target updated successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Target doesn't exist

---

### 9. Delete Target

**Endpoint:** `DELETE /api/SalesmanStatistics/targets/{targetId}`

**Purpose:** Delete a target

**Success Response (200):**

```json
{
	"success": true,
	"data": "Target deleted successfully",
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 10. Get Salesman Targets

**Endpoint:** `GET /api/SalesmanStatistics/targets/salesman/{salesmanId}`

**Purpose:** Get all targets for a specific salesman

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Query Parameters:**

- `year` (int, required) - Year

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 10,
			"year": 2025,
			"quarter": 4,
			"targetVisits": 100,
			"targetSuccessfulVisits": 70,
			"targetOffers": 20,
			"targetDeals": 12
		},
		{
			"id": 11,
			"year": 2025,
			"quarter": null,
			"targetVisits": 400,
			"targetDeals": 50
		}
	],
	"message": "Targets retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 11. Get Team Targets

**Endpoint:** `GET /api/SalesmanStatistics/targets/team`

**Purpose:** Get team-wide targets for a period

**Query Parameters:**

- `year` (int, required) - Year
- `quarter` (int, optional) - Quarter 1-4

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 20,
			"year": 2025,
			"quarter": 4,
			"isTeamTarget": true,
			"targetVisits": 500,
			"targetSuccessfulVisits": 350,
			"targetOffers": 100,
			"targetDeals": 60
		}
	],
	"message": "Team targets retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 12. Get All Weekly Plans

**Endpoint:** `GET /api/WeeklyPlan`

**Purpose:** Get weekly plans with advanced filters (can filter by employee, date, viewed status)

**Query Parameters:**

- `page` (int, default: 1)
- `pageSize` (int, default: 20)
- `employeeId` (string, optional) - Filter by salesman
- `weekStartDate` (DateTime, optional)
- `weekEndDate` (DateTime, optional)
- `isViewed` (bool, optional) - Filter by viewed status

**Request Example:**

```http
GET /api/WeeklyPlan?employeeId=salesman-123&isViewed=false&page=1&pageSize=20
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
				"employeeId": "salesman-123",
				"employee": {
					"id": "salesman-123",
					"firstName": "Ahmed",
					"lastName": "Mohamed",
					"email": "ahmed@example.com"
				},
				"weekStartDate": "2025-11-04T00:00:00Z",
				"weekEndDate": "2025-11-10T23:59:59Z",
				"title": "Week 45 Plan",
				"isActive": true,
				"isViewed": false,
				"managerViewedAt": null
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 50,
			"totalPages": 3
		}
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 13. Get All Salesmen

**Endpoint:** `GET /api/WeeklyPlan/salesmen`

**Purpose:** Get list of all salesmen for dropdown/filter selection

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": "salesman-123",
			"firstName": "Ahmed",
			"lastName": "Mohamed",
			"email": "ahmed@example.com",
			"phoneNumber": "+20123456789",
			"userName": "ahmed.mohamed",
			"isActive": true
		},
		{
			"id": "salesman-456",
			"firstName": "Mohamed",
			"lastName": "Ali",
			"email": "mohamed@example.com",
			"phoneNumber": "+20987654321",
			"userName": "mohamed.ali",
			"isActive": true
		}
	],
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 14. Review Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/review`

**Purpose:** Review, rate, and comment on a submitted weekly plan

**Request Body:**

```json
{
	"rating": 4,
	"comment": "Good plan, well structured. Consider focusing more on high priority clients."
}
```

**Required Fields:**

- None (both optional)

**Optional Fields:**

- `rating` (int, 1-5)
- `comment` (string, max 1000)

**Success Response (200):**

```json
{
	"success": true,
	"data": "Weekly plan reviewed successfully",
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Note:** This also marks the plan as viewed by manager

---

### 15. Get All Task Progress

**Endpoint:** `GET /api/TaskProgress`

**Purpose:** Get all task progress across team (with date filters)

**Query Parameters:**

- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):** Same format as Salesman endpoint, but includes all team progress

---

### 16. Get Progress by Employee

**Endpoint:** `GET /api/TaskProgress/employee/{employeeId}`

**Purpose:** Get all progress records for a specific employee

**Path Parameters:**

- `employeeId` (string) - Employee user ID

**Query Parameters:**

- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):** Array of progress records

---

### 17. Assign Offer Request to SalesSupport

**Endpoint:** `PUT /api/OfferRequest/{id}/assign`

**Purpose:** Assign an offer request to a SalesSupport member

**Path Parameters:**

- `id` (long) - Offer request ID

**Request Body:**

```json
{
	"assignedTo": "support-789"
}
```

**Required Fields:**

- `assignedTo` (string) - SalesSupport user ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "Assigned",
		"assignedTo": "support-789",
		"assignedToName": "Support User Name",
		"assignedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request assigned successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 18. Update Offer Request Status

**Endpoint:** `PUT /api/OfferRequest/{id}/status`

**Purpose:** Update status of an offer request

**Request Body:**

```json
{
	"status": "InProgress",
	"notes": "Starting to create offer"
}
```

**Required Fields:**

- `status` (string) - "Pending", "Assigned", "InProgress", "Completed", "Cancelled"

**Optional Fields:**

- `notes` (string)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "InProgress",
		"notes": "Starting to create offer",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request status updated successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 19. Get Offer Requests by Salesman

**Endpoint:** `GET /api/OfferRequest/salesman/{salesmanId}`

**Purpose:** Get all offer requests created by a specific salesman

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Query Parameters:**

- `status` (string, optional)

**Success Response (200):** Array of offer requests

---

### 20. Get Offer Requests Assigned to Support

**Endpoint:** `GET /api/OfferRequest/assigned/{supportId}`

**Purpose:** Get all requests assigned to a specific SalesSupport member

**Path Parameters:**

- `supportId` (string) - SalesSupport user ID

**Query Parameters:**

- `status` (string, optional)

**Success Response (200):** Array of offer requests

---

### 21. Get All Offers

**Endpoint:** `GET /api/Offer`

**Purpose:** Get all offers in the system (with filters)

**Query Parameters:**

- `status` (string, optional)
- `clientId` (string, optional)
- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):** Array of offers

---

### 22. Get My Offers

**Endpoint:** `GET /api/Offer/my-offers`

**Purpose:** Get offers you created

**Query Parameters:**

- `status` (string, optional)
- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):** Array of offers

---

### 23. Create Offer

**Endpoint:** `POST /api/Offer`

**Purpose:** Create a new offer (usually based on an offer request)

**Request Body:**

```json
{
	"offerRequestId": 25,
	"clientId": 123,
	"assignedTo": "salesman-123",
	"products": "X-Ray Machine Model XYZ",
	"totalAmount": 50000.0,
	"paymentTerms": "50% advance, 50% on delivery",
	"validUntil": "2025-11-15T23:59:59Z",
	"deliveryTerms": "2 weeks delivery"
}
```

**Required Fields:**

- `clientId` (long)
- `assignedTo` (string) - Salesman user ID
- `products` (string, max 2000)
- `totalAmount` (decimal)

**Optional Fields:**

- `offerRequestId` (long)
- `paymentTerms`, `deliveryTerms`, `validUntil`, etc.

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"clientId": 123,
		"assignedTo": "salesman-123",
		"products": "X-Ray Machine Model XYZ",
		"totalAmount": 50000.0,
		"status": "Draft",
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 24-28. Offer Management Endpoints

These endpoints match SalesSupport documentation:

- Add Equipment → `POST /api/Offer/{offerId}/equipment`
- Delete Equipment → `DELETE /api/Offer/{offerId}/equipment/{equipmentId}`
- Upload Equipment Image → `POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image`
- Add/Update Terms → `POST /api/Offer/{offerId}/terms`
- Create Installment Plan → `POST /api/Offer/{offerId}/installments`
- Send to Salesman → `POST /api/Offer/{offerId}/send-to-salesman`

---

### 29. Get Offers by Salesman

**Endpoint:** `GET /api/Offer/by-salesman/{salesmanId}`

**Purpose:** Get all offers assigned to a specific salesman

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Success Response (200):** Array of offers

---

### 30. Get Offer Request Details

**Endpoint:** `GET /api/Offer/request/{requestId}/details`

**Purpose:** Get offer request details for creating an offer

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"requestId": 25,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Fast delivery needed"
	},
	"message": "Offer request details retrieved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 31. Get All Deals

**Endpoint:** `GET /api/Deal`

**Purpose:** Get all deals (with filters)

**Query Parameters:**

- `status` (string, optional)
- `salesmanId` (string, optional)

**Success Response (200):** Array of deals

---

### 32. Get Deal Details

**Endpoint:** `GET /api/Deal/{id}`

**Purpose:** Get full deal details

**Success Response (200):** Deal object with full details

---

### 33. Get Deals by Salesman

**Endpoint:** `GET /api/Deal/by-salesman/{salesmanId}`

**Purpose:** Get all deals for a specific salesman

**Query Parameters:**

- `status` (string, optional)

**Success Response (200):** Array of deals

---

### 34. Approve/Reject Deal

**Endpoint:** `POST /api/Deal/{id}/manager-approval`

**Purpose:** Approve or reject a deal (first level approval)

**Request Body:**

```json
{
	"approved": true,
	"notes": "Deal approved, good pricing",
	"superAdminRequired": false
}
```

**Required Fields:**

- `approved` (bool)

**Optional Fields:**

- `notes` (string)
- `superAdminRequired` (bool) - If true, deal also needs SuperAdmin approval

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"status": "PendingSuperAdminApproval",
		"managerApprovedBy": "manager-456",
		"managerApprovedAt": "2025-10-29T14:00:00Z",
		"managerNotes": "Deal approved"
	},
	"message": "Manager approval processed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- If `approved: true` and `superAdminRequired: false` → Status becomes `"Approved"`
- If `approved: true` and `superAdminRequired: true` → Status becomes `"PendingSuperAdminApproval"`
- If `approved: false` → Status becomes `"Rejected"`

---

### 35. Get Pending Manager Approvals

**Endpoint:** `GET /api/Deal/pending-manager-approvals`

**Purpose:** Get all deals waiting for your approval

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
			"salesmanId": "salesman-123",
			"salesmanName": "Ahmed Mohamed",
			"closedDate": "2025-10-29T14:00:00Z"
		}
	],
	"message": "Pending manager approvals retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 36. Mark Deal as Completed

**Endpoint:** `POST /api/Deal/{id}/complete`

**Purpose:** Mark a deal as successfully completed

**Request Body:**

```json
{
	"completionNotes": "Deal completed successfully, delivery made on time"
}
```

**Required Fields:**

- `completionNotes` (string)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"status": "Completed",
		"completedAt": "2025-10-29T15:00:00Z",
		"completionNotes": "Deal completed successfully"
	},
	"message": "Deal marked as completed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Deal not in "Approved" status

---

### 37. Mark Deal as Failed

**Endpoint:** `POST /api/Deal/{id}/fail`

**Purpose:** Mark a deal as failed

**Request Body:**

```json
{
	"failureNotes": "Client cancelled order due to budget constraints"
}
```

**Required Fields:**

- `failureNotes` (string)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"status": "Failed",
		"failedAt": "2025-10-29T15:00:00Z",
		"failureNotes": "Client cancelled order"
	},
	"message": "Deal marked as failed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

## 🔄 Common Workflows

### Workflow 1: Review Weekly Plan → Monitor Progress → Approve Deal

1. **Get Weekly Plans** → `GET /api/WeeklyPlan?isViewed=false`
2. **Review Plan** → `POST /api/WeeklyPlan/{id}/review`
3. **Monitor Task Progress** → `GET /api/TaskProgress/employee/{employeeId}`
4. **When Deal Created, Approve** → `POST /api/Deal/{id}/manager-approval`

### Workflow 2: Manage Offer Request → Create Offer

1. **Get Offer Requests** → `GET /api/OfferRequest?status=Pending`
2. **Assign to Support** → `PUT /api/OfferRequest/{id}/assign`
3. **Get Request Details** → `GET /api/Offer/request/{requestId}/details`
4. **Create Offer** → `POST /api/Offer`
5. **Add Equipment/Terms** → Various endpoints
6. **Send to Salesman** → `POST /api/Offer/{offerId}/send-to-salesman`

### Workflow 3: Manage Targets

1. **Get Salesman List** → `GET /api/WeeklyPlan/salesmen`
2. **Create Target** → `POST /api/SalesmanStatistics/targets`
3. **View Progress** → `GET /api/SalesmanStatistics/{salesmanId}/progress`
4. **Update Target if Needed** → `PUT /api/SalesmanStatistics/targets/{targetId}`

---

## 📊 Dashboard Data Endpoints

### For Manager Dashboard:

1. **Team Overview:**

      - `GET /api/SalesmanStatistics/all?year=2025&quarter=4`
      - `GET /api/SalesmanStatistics/targets/team?year=2025&quarter=4`

2. **Pending Actions:**

      - `GET /api/Deal/pending-manager-approvals`
      - `GET /api/WeeklyPlan?isViewed=false`

3. **Team Activity:**
      - `GET /api/TaskProgress?startDate=2025-10-01&endDate=2025-10-31`
      - `GET /api/OfferRequest?status=Pending`

---

## ⚠️ Important Notes

1. **All Client Endpoints:** Managers can access ANY client (no restriction)
2. **All Task Progress:** Managers see ALL team progress (no restriction)
3. **Deal Approval:** Managers can only approve deals, not SuperAdmin-level deals
4. **Target Management:** Managers create individual salesman targets, not team targets (team targets are created by SuperAdmin)
5. **Weekly Plan Review:** Reviewing a plan marks it as viewed

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29
