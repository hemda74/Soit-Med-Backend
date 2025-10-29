# 👑 Super Admin API Documentation

## 🎯 User Stories

### As a Super Admin, I want to:

1. **View all system statistics** across all roles and departments
2. **View individual salesman statistics** for any salesman
3. **View team statistics** for any team/manager
4. **View all clients** in the system
5. **View all offers** and monitor offer creation
6. **View all deals** and approve high-value deals
7. **View all task progress** across entire system
8. **Approve/reject deals** that require SuperAdmin approval
9. **View pending approvals** requiring my attention
10. **Monitor all activities** for auditing purposes

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

**Super Admin has access to ALL endpoints from:**

- ✅ **Salesman APIs** (all read access, some write)
- ✅ **Sales Manager APIs** (all read access, all write access)
- ✅ **Sales Support APIs** (all read access)

**Plus Super Admin specific endpoints:**

| #   | Endpoint                                        | Method | Purpose                                 |
| --- | ----------------------------------------------- | ------ | --------------------------------------- |
| 1   | `/api/SalesmanStatistics/all`                   | GET    | Get all salesmen statistics             |
| 2   | `/api/SalesmanStatistics/{salesmanId}`          | GET    | Get specific salesman statistics        |
| 3   | `/api/SalesmanStatistics/{salesmanId}/progress` | GET    | Get salesman progress vs targets        |
| 4   | `/api/SalesmanStatistics/targets/team`          | GET    | Get team targets                        |
| 5   | `/api/Client/search`                            | GET    | Search all clients                      |
| 6   | `/api/Client/{id}`                              | GET    | Get any client                          |
| 7   | `/api/Client/{id}/profile`                      | GET    | Get complete client profile             |
| 8   | `/api/WeeklyPlan`                               | GET    | Get all weekly plans                    |
| 9   | `/api/WeeklyPlan/{id}`                          | GET    | Get any weekly plan                     |
| 10  | `/api/TaskProgress`                             | GET    | Get all task progress                   |
| 11  | `/api/TaskProgress/employee/{employeeId}`       | GET    | Get progress for any employee           |
| 12  | `/api/TaskProgress/task/{taskId}`               | GET    | Get progress for any task               |
| 13  | `/api/TaskProgress/by-client/{clientId}`        | GET    | Get progress for any client             |
| 14  | `/api/OfferRequest`                             | GET    | Get all offer requests                  |
| 15  | `/api/OfferRequest/{id}`                        | GET    | Get any offer request                   |
| 16  | `/api/OfferRequest/{id}/assign`                 | PUT    | Assign offer requests                   |
| 17  | `/api/OfferRequest/{id}/status`                 | PUT    | Update request status                   |
| 18  | `/api/OfferRequest/salesman/{salesmanId}`       | GET    | Get requests by salesman                |
| 19  | `/api/OfferRequest/assigned/{supportId}`        | GET    | Get requests assigned to support        |
| 20  | `/api/Offer`                                    | GET    | Get all offers                          |
| 21  | `/api/Offer/{id}`                               | GET    | Get any offer                           |
| 22  | `/api/Offer/{offerId}/export-pdf`               | GET    | Export any offer as PDF                 |
| 23  | `/api/Deal`                                     | GET    | Get all deals                           |
| 24  | `/api/Deal/{id}`                                | GET    | Get any deal                            |
| 25  | `/api/Deal/by-salesman/{salesmanId}`            | GET    | Get deals by salesman                   |
| 26  | `/api/Deal/{id}/manager-approval`               | POST   | Approve/reject deals (manager level)    |
| 27  | `/api/Deal/{id}/superadmin-approval`            | POST   | Approve/reject deals (SuperAdmin level) |
| 28  | `/api/Deal/pending-manager-approvals`           | GET    | Get deals pending manager approval      |
| 29  | `/api/Deal/pending-superadmin-approvals`        | GET    | Get deals pending my approval           |
| 30  | `/api/Deal/{id}/complete`                       | POST   | Mark deal as completed                  |
| 31  | `/api/Deal/{id}/fail`                           | POST   | Mark deal as failed                     |

---

## 🔍 Detailed Endpoint Documentation

### 1. Get All Salesmen Statistics

**Endpoint:** `GET /api/SalesmanStatistics/all`

**Purpose:** Get comprehensive statistics for all salesmen across the entire system

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
		"systemStatistics": {
			"totalSalesmen": 15,
			"activeSalesmen": 12,
			"totalVisits": 1500,
			"totalOffers": 300,
			"totalDeals": 180,
			"totalRevenue": 18000000.0,
			"averageSuccessRate": 75.5,
			"averageDealValue": 100000.0
		},
		"teamStatistics": [
			{
				"teamId": "team-1",
				"teamName": "Cairo Team",
				"managerId": "manager-123",
				"managerName": "Manager Name",
				"totalSalesmen": 5,
				"totalVisits": 500,
				"totalOffers": 100,
				"totalDeals": 60,
				"totalRevenue": 6000000.0
			}
		],
		"salesmen": [
			{
				"salesmanId": "salesman-123",
				"salesmanName": "Ahmed Mohamed",
				"teamId": "team-1",
				"managerId": "manager-123",
				"totalVisits": 120,
				"totalOffers": 25,
				"totalDeals": 15,
				"totalRevenue": 1500000.0,
				"successRate": 80.0,
				"ranking": 1
			}
		]
	},
	"message": "Statistics retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 2. Get Specific Salesman Statistics

**Endpoint:** `GET /api/SalesmanStatistics/{salesmanId}`

**Purpose:** Get detailed statistics for any salesman in the system

**Path Parameters:**

- `salesmanId` (string) - Salesman user ID

**Query Parameters:**

- `year` (int, optional)
- `quarter` (int, optional)

**Success Response (200):** Same format as Sales Manager endpoint

---

### 3. Get Salesman Progress vs Targets

**Endpoint:** `GET /api/SalesmanStatistics/{salesmanId}/progress`

**Purpose:** Get any salesman's progress against their targets

**Success Response (200):** Same format as Sales Manager endpoint

---

### 4. Get Team Targets

**Endpoint:** `GET /api/SalesmanStatistics/targets/team`

**Purpose:** Get team-wide targets for any period

**Query Parameters:**

- `year` (int, required)
- `quarter` (int, optional)

**Success Response (200):** Same format as Sales Manager endpoint

---

### 5-7. Client Management

**Super Admin has FULL access to all client endpoints:**

- `GET /api/Client/search` - Search all clients
- `GET /api/Client/{id}` - Get any client
- `GET /api/Client/{id}/profile` - Get complete profile of any client

**No restrictions** - Can view any client regardless of assignment

**Success Responses:** Same formats as Salesman documentation

---

### 8-9. Weekly Plan Management

**Super Admin can view ALL weekly plans:**

**Endpoint:** `GET /api/WeeklyPlan`

**Query Parameters:**

- `page`, `pageSize`
- `employeeId` (string, optional) - Filter by any salesman
- `weekStartDate`, `weekEndDate` (DateTime, optional)
- `isViewed` (bool, optional)

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
					"lastName": "Mohamed"
				},
				"weekStartDate": "2025-11-04T00:00:00Z",
				"weekEndDate": "2025-11-10T23:59:59Z",
				"title": "Week 45 Plan",
				"isActive": true,
				"isViewed": false,
				"managerViewedAt": null,
				"managerReviewedAt": null,
				"rating": null
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 200,
			"totalPages": 10
		}
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Note:** Super Admin can view plans but typically doesn't review them (Manager's responsibility)

---

### 10-13. Task Progress Management

**Super Admin has FULL access:**

- `GET /api/TaskProgress` - All progress across system
- `GET /api/TaskProgress/employee/{employeeId}` - Any employee's progress
- `GET /api/TaskProgress/task/{taskId}` - Any task's progress
- `GET /api/TaskProgress/by-client/{clientId}` - Any client's progress

**No restrictions** - Can view any progress record

---

### 14-19. Offer Request Management

**Super Admin has FULL access:**

- `GET /api/OfferRequest` - All requests
- `GET /api/OfferRequest/{id}` - Any request
- `PUT /api/OfferRequest/{id}/assign` - Assign requests
- `PUT /api/OfferRequest/{id}/status` - Update status
- `GET /api/OfferRequest/salesman/{salesmanId}` - Requests by any salesman
- `GET /api/OfferRequest/assigned/{supportId}` - Requests assigned to any support

**No restrictions**

---

### 20-22. Offer Management

**Super Admin has READ-ONLY access to offers:**

- `GET /api/Offer` - All offers
- `GET /api/Offer/{id}` - Any offer
- `GET /api/Offer/{offerId}/export-pdf` - Export any offer

**Note:** Super Admin typically doesn't CREATE offers (Sales Support does)

---

### 23-31. Deal Management (CRITICAL)

**Super Admin has FULL access to deals, especially approvals:**

#### 23. Get All Deals

**Endpoint:** `GET /api/Deal`

**Query Parameters:**

- `status` (string, optional)
- `salesmanId` (string, optional)

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
			"status": "PendingSuperAdminApproval",
			"salesmanId": "salesman-123",
			"salesmanName": "Ahmed Mohamed",
			"managerId": "manager-456",
			"managerName": "Manager Name",
			"managerApprovedAt": "2025-10-29T14:00:00Z",
			"closedDate": "2025-10-29T14:00:00Z"
		}
	],
	"message": "Deals retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

#### 24. Get Deal Details

**Endpoint:** `GET /api/Deal/{id}`

**Purpose:** Get full details of any deal

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"offerId": 50,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"dealValue": 48000.0,
		"status": "PendingSuperAdminApproval",
		"closedDate": "2025-10-29T14:00:00Z",
		"salesmanId": "salesman-123",
		"salesmanName": "Ahmed Mohamed",
		"managerId": "manager-456",
		"managerName": "Manager Name",
		"managerApprovedBy": "manager-456",
		"managerApprovedAt": "2025-10-29T14:00:00Z",
		"managerNotes": "Deal approved, requires SuperAdmin approval for amount > 40K",
		"superAdminApprovedBy": null,
		"superAdminApprovedAt": null,
		"superAdminNotes": null,
		"createdAt": "2025-10-29T14:00:00Z"
	},
	"message": "Deal retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

#### 25. Get Deals by Salesman

**Endpoint:** `GET /api/Deal/by-salesman/{salesmanId}`

**Purpose:** Get all deals for any salesman

**Query Parameters:**

- `status` (string, optional)

**Success Response (200):** Array of deals

---

#### 26. Manager-Level Approval (Also Available)

**Endpoint:** `POST /api/Deal/{id}/manager-approval`

**Purpose:** Super Admin can also approve at manager level if needed

**Note:** Same format as Manager endpoint

---

#### 27. SuperAdmin Approval (CRITICAL)

**Endpoint:** `POST /api/Deal/{id}/superadmin-approval`

**Purpose:** Final approval for deals requiring SuperAdmin approval (typically high-value deals)

**Request Body:**

```json
{
	"approved": true,
	"notes": "Deal approved after thorough review. High-value transaction cleared."
}
```

**Required Fields:**

- `approved` (bool)

**Optional Fields:**

- `notes` (string)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 15,
		"status": "Approved",
		"superAdminApprovedBy": "superadmin-999",
		"superAdminApprovedAt": "2025-10-29T16:00:00Z",
		"superAdminNotes": "Deal approved after thorough review",
		"finalStatus": "Approved"
	},
	"message": "SuperAdmin approval processed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- If `approved: true` → Status becomes `"Approved"` (deal is fully approved)
- If `approved: false` → Status becomes `"Rejected"` (deal is rejected, cannot proceed)

**Error Responses:**

- **400 Bad Request:** Deal not in "PendingSuperAdminApproval" status
- **404 Not Found:** Deal doesn't exist

**Important:** This is the FINAL approval. Once approved, deal cannot be rejected.

---

#### 28. Get Pending Manager Approvals

**Endpoint:** `GET /api/Deal/pending-manager-approvals`

**Purpose:** View all deals waiting for manager approval (for monitoring)

**Success Response (200):** Array of deals with status "PendingManagerApproval"

---

#### 29. Get Pending SuperAdmin Approvals (CRITICAL)

**Endpoint:** `GET /api/Deal/pending-superadmin-approvals`

**Purpose:** Get ALL deals waiting for YOUR approval

**Request Example:**

```http
GET /api/Deal/pending-superadmin-approvals
Authorization: Bearer {token}
```

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
			"status": "PendingSuperAdminApproval",
			"salesmanId": "salesman-123",
			"salesmanName": "Ahmed Mohamed",
			"managerId": "manager-456",
			"managerName": "Manager Name",
			"managerApprovedAt": "2025-10-29T14:00:00Z",
			"managerNotes": "High-value deal, requires SuperAdmin approval",
			"closedDate": "2025-10-29T14:00:00Z",
			"offerId": 50,
			"offerProducts": "X-Ray Machine Model XYZ"
		},
		{
			"id": 16,
			"clientId": 125,
			"clientName": "Cairo Medical Center",
			"dealValue": 75000.0,
			"status": "PendingSuperAdminApproval",
			"salesmanId": "salesman-456",
			"salesmanName": "Mohamed Ali",
			"managerId": "manager-789",
			"managerName": "Another Manager",
			"managerApprovedAt": "2025-10-29T15:00:00Z",
			"closedDate": "2025-10-29T15:00:00Z"
		}
	],
	"message": "Pending SuperAdmin approvals retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Use Case:** This is your main dashboard endpoint for approvals

---

#### 30. Mark Deal as Completed

**Endpoint:** `POST /api/Deal/{id}/complete`

**Purpose:** Mark a completed deal as successful

**Request Body:**

```json
{
	"completionNotes": "Deal completed successfully. Delivery made, payment received."
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
		"completedAt": "2025-10-29T17:00:00Z",
		"completionNotes": "Deal completed successfully",
		"completedBy": "superadmin-999"
	},
	"message": "Deal marked as completed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

#### 31. Mark Deal as Failed

**Endpoint:** `POST /api/Deal/{id}/fail`

**Purpose:** Mark a deal as failed (cancelled, not delivered, etc.)

**Request Body:**

```json
{
	"failureNotes": "Client cancelled order due to budget constraints. Deal terminated."
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
		"failedAt": "2025-10-29T17:00:00Z",
		"failureNotes": "Client cancelled order",
		"failedBy": "superadmin-999"
	},
	"message": "Deal marked as failed successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

## 🔄 Approval Workflow (SuperAdmin Role)

### Deal Approval Process:

```
1. Salesman creates deal
   ↓
2. Deal status: "PendingManagerApproval"
   ↓
3. Manager approves
   ↓
4. Deal status: "PendingSuperAdminApproval" (if required)
   ↓
5. SuperAdmin reviews → Approve/Reject
   ↓
6. If Approved: "Approved" → Can be marked as "Completed" or "Failed"
   If Rejected: "Rejected" (dead end)
```

### When to Use SuperAdmin Approval:

**Typically required when:**

- Deal value exceeds threshold (e.g., > 40,000)
- Manager sets `superAdminRequired: true`
- Company policy requires SuperAdmin approval

**SuperAdmin can:**

- ✅ Approve deal → Proceeds to completion
- ❌ Reject deal → Deal terminated, cannot proceed

---

## 📊 Dashboard Endpoints for SuperAdmin

### Main Dashboard Data:

1. **Pending Actions:**

      ```http
      GET /api/Deal/pending-superadmin-approvals
      ```

2. **System Overview:**

      ```http
      GET /api/SalesmanStatistics/all?year=2025&quarter=4
      ```

3. **All Activities:**

      ```http
      GET /api/TaskProgress?startDate=2025-10-01&endDate=2025-10-31
      GET /api/Deal?status=Approved
      GET /api/Offer?status=Sent
      ```

4. **Client Overview:**
      ```http
      GET /api/Client/search?page=1&pageSize=50
      ```

---

## ⚠️ Important Notes for SuperAdmin

1. **Full Access:** Super Admin has access to ALL endpoints (read and write)

2. **Deal Approval:**

      - **CRITICAL** - Final approval authority
      - Once approved, deal is final
      - Rejection cannot be undone

3. **Audit Trail:**

      - All actions are logged with your user ID
      - Timestamps recorded for all approvals/rejections
      - Notes should be clear for audit purposes

4. **No Restrictions:**

      - Can view any client, any deal, any offer
      - Can view any salesman's statistics
      - Can view any team's data

5. **Best Practices:**

      - Always add notes when approving/rejecting deals
      - Review deal details thoroughly before approval
      - Monitor high-value deals closely

6. **Deal Completion:**
      - Only mark deals as completed after verification
      - Mark as failed if delivery/payment issues occur
      - Add detailed notes for audit

---

## 🚨 Critical Endpoints Summary

### Must-Use Endpoints:

1. **Daily Approvals:**

      - `GET /api/Deal/pending-superadmin-approvals` - Check pending deals
      - `POST /api/Deal/{id}/superadmin-approval` - Approve/reject deals

2. **Weekly Review:**

      - `GET /api/SalesmanStatistics/all?year=2025&quarter=4` - System overview
      - `GET /api/Deal?status=Approved` - Review completed deals

3. **Monthly Audit:**
      - `GET /api/TaskProgress?startDate=...&endDate=...` - Activity review
      - `GET /api/Client/search` - Client overview

---

## 💡 Quick Reference

### Deal Status Flow:

```
PendingManagerApproval
  → PendingSuperAdminApproval (if required)
    → Approved (if you approve)
      → Completed/Failed
    → Rejected (if you reject)
```

### Approval Checklist:

- ✅ Review deal value
- ✅ Check client history
- ✅ Review manager notes
- ✅ Verify offer details
- ✅ Add approval notes
- ✅ Make decision (Approve/Reject)

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29
