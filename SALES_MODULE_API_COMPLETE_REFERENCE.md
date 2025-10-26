# Sales Module API - Complete Reference

**Version**: 1.0  
**Date**: 2025-10-26  
**Base URL**: `http://localhost:5117/api`  
**Authentication**: JWT Bearer Token

---

## Table of Contents

1. [Client Management Endpoints](#client-management-endpoints)
2. [Task Progress Endpoints](#task-progress-endpoints)
3. [Offer Request Endpoints](#offer-request-endpoints)
4. [Sales Offer Endpoints](#sales-offer-endpoints)
5. [Deal Management Endpoints](#deal-management-endpoints)
6. [Sales Report Endpoints](#sales-report-endpoints)
7. [Weekly Plan Endpoints](#weekly-plan-endpoints)
8. [Authorization Roles](#authorization-roles)
9. [Response Formats](#response-formats)

---

## Authorization Roles

- **Salesman** (`Salesman`) - Field sales representative
- **SalesSupport** (`SalesSupport`) - Sales support staff
- **SalesManager** (`SalesManager`) - Sales manager
- **SuperAdmin** (`SuperAdmin`) - System administrator

---

## Client Management Endpoints

### 1. Search Clients

**Endpoint**: `GET /api/client/search`  
**Authorization**: All authenticated users

**Query Parameters**:

- `searchTerm` (string, optional) - Search query
- `status` (string, optional) - Filter by status
- `priority` (string, optional) - Filter by priority
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page

**Example Request**:

```http
GET /api/client/search?searchTerm=Cairo&status=Active&page=1&pageSize=20
Authorization: Bearer {token}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"items": [
			{
				"id": 1,
				"name": "Cairo University Hospital",
				"type": "Hospital",
				"specialization": "General Medicine",
				"status": "Active",
				"priority": "High",
				"location": "Cairo"
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 150,
			"totalPages": 8,
			"hasPrevious": false,
			"hasNext": true
		}
	},
	"message": "Success",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Invalid query parameters
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 2. Create Client

**Endpoint**: `POST /api/client`  
**Authorization**: All authenticated users

**Request Body**:

```json
{
	"name": "New Hospital",
	"type": "Hospital",
	"specialization": "Cardiology",
	"location": "Cairo",
	"phone": "01234567890",
	"email": "info@hospital.com",
	"address": "123 Main St",
	"city": "Cairo",
	"governorate": "Cairo",
	"status": "Active",
	"priority": "High"
}
```

**Response**: `201 Created`

```json
{
	"success": true,
	"data": {
		"id": 25,
		"name": "New Hospital",
		"type": "Hospital",
		"status": "Active"
	},
	"message": "Client created successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 3. Get Client by ID

**Endpoint**: `GET /api/client/{id}`  
**Authorization**: All authenticated users

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"name": "Cairo University Hospital",
		"type": "Hospital",
		"specialization": "General Medicine",
		"status": "Active",
		"priority": "High",
		"location": "Cairo",
		"phone": "01234567890",
		"email": "info@hospital.com"
	},
	"message": "Success",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `404 Not Found` - Client not found
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 4. Get Client Profile (with History)

**Endpoint**: `GET /api/client/{id}/profile`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Response**: `200 OK`

```json
{
  "success": true,
  "data": {
    "client": {
      "id": 1,
      "name": "Cairo University Hospital",
      "type": "Hospital",
      "status": "Active"
    },
    "history": {
      "tasks": [...],
      "offers": [...],
      "deals": [...],
      "visits": [...]
    },
    "statistics": {
      "totalTasks": 15,
      "totalOffers": 5,
      "totalDeals": 2,
      "totalValue": 5000000
    }
  },
  "message": "Client profile retrieved successfully",
  "timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `403 Forbidden` - User not authorized to view this profile
- `404 Not Found` - Client not found
- `500 Internal Server Error` - Server error

---

### 5. Get My Clients

**Endpoint**: `GET /api/client/my-clients`  
**Authorization**: All authenticated users

**Response**: `200 OK`  
Returns clients assigned to the current user.

---

## Task Progress Endpoints

### 1. Create Task Progress

**Endpoint**: `POST /api/taskprogress`  
**Authorization**: Salesman, SalesManager

**Request Body**:

```json
{
	"taskId": 80,
	"progressDate": "2025-10-26T10:00:00",
	"progressType": "Visit",
	"description": "Initial client visit completed",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-11-02T10:00:00"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"taskId": 80,
		"clientId": 24,
		"progressDate": "2025-10-26T10:00:00",
		"progressType": "Visit",
		"description": "Initial client visit completed",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"status": "Completed"
	},
	"message": "Task progress created successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Not authorized
- `500 Internal Server Error` - Server error

---

### 2. Create Progress with Offer Request

**Endpoint**: `POST /api/taskprogress/with-offer-request`  
**Authorization**: Salesman, SalesManager

**Request Body**:

```json
{
	"taskId": 80,
	"clientId": 24,
	"progressDate": "2025-10-26T10:00:00",
	"progressType": "Visit",
	"description": "Client interested in MRI scanner",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"requestedProducts": "MRI Scanner Model X1",
	"specialNotes": "Urgent request for Q2 delivery"
}
```

**Response**: `200 OK`  
Returns created task progress with offer request ID.

**Error Responses**:

- `400 Bad Request` - Validation errors or transaction failed
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error (transaction rolled back)

---

### 3. Get All Task Progress

**Endpoint**: `GET /api/taskprogress`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Query Parameters**:

- `startDate` (DateTime, optional) - Filter by start date
- `endDate` (DateTime, optional) - Filter by end date

**Response**: `200 OK`

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"taskId": 80,
			"clientId": 24,
			"progressDate": "2025-10-26T10:00:00",
			"progressType": "Visit",
			"description": "Client visit completed",
			"visitResult": "Interested"
		}
	],
	"message": "Task progress retrieved successfully",
	"timestamp": "2025-10-26T..."
}
```

**Note**: Salesman sees only their own progress. SalesManager and SuperAdmin see all progress.

---

### 4. Get Progress by Task ID

**Endpoint**: `GET /api/taskprogress/task/{taskId}`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Response**: `200 OK` - List of all progress for the specified task.

**Error Responses**:

- `403 Forbidden` - Not authorized to view this task's progress
- `500 Internal Server Error` - Server error

---

### 5. Get Progress by Client ID

**Endpoint**: `GET /api/taskprogress/by-client/{clientId}`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Response**: `200 OK` - List of all progress for the specified client.

---

## Offer Request Endpoints

### 1. Create Offer Request

**Endpoint**: `POST /api/offerrequest`  
**Authorization**: Salesman, SalesManager

**Request Body**:

```json
{
	"clientId": 24,
	"requestedProducts": "MRI Scanner Model X1",
	"specialNotes": "Urgent request for Q2 delivery"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"clientId": 24,
		"requestedBy": "Ahmed_Ashraf_Sales_001",
		"requestedProducts": "MRI Scanner Model X1",
		"status": "Requested",
		"createdAt": "2025-10-26T10:00:00"
	},
	"message": "Offer request created successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 2. Get All Offer Requests

**Endpoint**: `GET /api/offerrequest`  
**Authorization**: Salesman, SalesManager, SalesSupport, SuperAdmin

**Query Parameters**:

- `status` (string, optional) - Filter by status
- `requestedBy` (string, optional) - Filter by requester ID

**Response**: `200 OK`

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"clientId": 24,
			"requestedProducts": "MRI Scanner Model X1",
			"status": "InProgress",
			"assignedTo": "Ahmed_Hemdan_Engineering_001"
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-26T..."
}
```

**Note**: SalesSupport sees only assigned requests. Salesman sees their own requests.

---

### 3. Get Offer Request by ID

**Endpoint**: `GET /api/offerrequest/{id}`  
**Authorization**: Salesman, SalesManager, SalesSupport, SuperAdmin

**Response**: `200 OK` - Single offer request details.

**Error Responses**:

- `403 Forbidden` - Not authorized to view this request
- `404 Not Found` - Offer request not found
- `500 Internal Server Error` - Server error

---

### 4. Assign Offer Request

**Endpoint**: `PUT /api/offerrequest/{id}/assign`  
**Authorization**: SalesManager, SalesSupport, SuperAdmin

**Request Body**:

```json
{
	"assignedTo": "Ahmed_Hemdan_Engineering_001"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"assignedTo": "Ahmed_Hemdan_Engineering_001",
		"status": "InProgress"
	},
	"message": "Offer request assigned successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 5. Get Assigned Offer Requests

**Endpoint**: `GET /api/offerrequest/assigned/{supportId}`  
**Authorization**: SalesSupport, SalesManager, SuperAdmin

**Query Parameters**:

- `status` (string, optional) - Filter by status

**Response**: `200 OK` - List of offer requests assigned to the specified support user.

---

## Sales Offer Endpoints

### 1. Get All Offers

**Endpoint**: `GET /api/offer`  
**Authorization**: SalesSupport, SalesManager, SuperAdmin

**Query Parameters**:

- `status` (string, optional) - Filter by status
- `clientId` (long, optional) - Filter by client ID

**Response**: `200 OK`

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"offerRequestId": 6,
			"clientId": 24,
			"products": "Complete MRI scanner package",
			"totalAmount": 2500000.0,
			"status": "Draft",
			"validUntil": "2025-11-25T14:17:39",
			"createdBy": "Ahmed_Hemdan_Engineering_001"
		}
	],
	"message": "Offers retrieved successfully",
	"timestamp": "2025-10-26T..."
}
```

---

### 2. Get My Offers (Created by SalesSupport)

**Endpoint**: `GET /api/offer/my-offers`  
**Authorization**: SalesSupport, SalesManager, SuperAdmin

**Query Parameters**:

- `status` (string, optional) - Filter by status (Draft, Sent, Accepted, Rejected)

**Example Request**:

```http
GET /api/offer/my-offers?status=Draft
Authorization: Bearer {token}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": [
		{
			"id": 3,
			"offerRequestId": 6,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"createdBy": "Ahmed_Hemdan_Engineering_001",
			"assignedTo": "Ahmed_Ashraf_Sales_001",
			"products": "Complete MRI scanner package",
			"totalAmount": 2500000.0,
			"paymentTerms": "50% Down payment, 50% on delivery",
			"deliveryTerms": "FOB Cairo, 30 days delivery",
			"validUntil": "2025-11-25T14:17:39",
			"status": "Draft",
			"createdAt": "2025-10-26T11:17:39"
		}
	],
	"message": "My offers retrieved successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Not authorized (role mismatch)
- `500 Internal Server Error` - Server error

---

### 3. Get Offer by ID

**Endpoint**: `GET /api/offer/{id}`  
**Authorization**: SalesSupport, SalesManager, SuperAdmin, Salesman

**Response**: `200 OK` - Single offer details.

**Error Responses**:

- `404 Not Found` - Offer not found
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 4. Create Offer

**Endpoint**: `POST /api/offer`  
**Authorization**: SalesSupport, SalesManager

**Request Body**:

```json
{
	"offerRequestId": 6,
	"clientId": 24,
	"assignedTo": "Ahmed_Ashraf_Sales_001",
	"products": "Complete MRI scanner package",
	"totalAmount": 2500000.0,
	"paymentTerms": "50% Down payment, 50% on delivery",
	"deliveryTerms": "FOB Cairo, 30 days delivery",
	"validUntil": "2025-11-25T14:17:39",
	"status": "Draft",
	"notes": "High priority offer"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 3,
		"offerRequestId": 6,
		"clientId": 24,
		"products": "Complete MRI scanner package",
		"totalAmount": 2500000.0,
		"status": "Draft",
		"createdBy": "Ahmed_Hemdan_Engineering_001"
	},
	"message": "Offer created successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

## Deal Management Endpoints

### 1. Create Deal

**Endpoint**: `POST /api/deal`  
**Authorization**: Salesman, SalesManager

**Request Body**:

```json
{
	"offerId": 1,
	"clientId": 24,
	"dealValue": 2500000.0,
	"paymentTerms": "50% Down, 50% on delivery",
	"deliveryTerms": "FOB Cairo, 30 days",
	"notes": "Deal notes here"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 1,
		"clientId": 24,
		"salesmanId": "Ahmed_Ashraf_Sales_001",
		"dealValue": 2500000.0,
		"status": "PendingManagerApproval",
		"createdAt": "2025-10-26T10:00:00"
	},
	"message": "Deal created successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors or offer not found
- `401 Unauthorized` - Missing or invalid token
- `500 Internal Server Error` - Server error

---

### 2. Get All Deals

**Endpoint**: `GET /api/deal`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Query Parameters**:

- `status` (string, optional) - Filter by status
- `salesmanId` (string, optional) - Filter by salesman ID

**Response**: `200 OK` - List of deals based on authorization.

**Note**: Salesman sees only their own deals. SalesManager and SuperAdmin see all deals.

---

### 3. Get Deal by ID

**Endpoint**: `GET /api/deal/{id}`  
**Authorization**: Salesman, SalesManager, SuperAdmin

**Response**: `200 OK` - Single deal details.

**Error Responses**:

- `403 Forbidden` - Not authorized to view this deal
- `404 Not Found` - Deal not found
- `500 Internal Server Error` - Server error

---

### 4. Manager Approval

**Endpoint**: `POST /api/deal/{id}/manager-approval`  
**Authorization**: SalesManager, SuperAdmin

**Request Body**:

```json
{
	"approved": true,
	"comments": "Approved by sales manager"
}
```

**Response**: `200 OK`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"status": "PendingSuperAdminApproval",
		"managerApprovedBy": "Ahmed_Hemdan_Sales_002",
		"managerApprovedAt": "2025-10-26T10:30:00",
		"managerComments": "Approved by sales manager"
	},
	"message": "Manager approval processed successfully",
	"timestamp": "2025-10-26T..."
}
```

**Error Responses**:

- `400 Bad Request` - Validation errors or deal not found
- `403 Forbidden` - Not authorized to approve deals
- `500 Internal Server Error` - Server error

---

### 5. SuperAdmin Approval

**Endpoint**: `POST /api/deal/{id}/superadmin-approval`  
**Authorization**: SuperAdmin

**Response**: `200 OK` - Deal approved by SuperAdmin, status changed to "Approved".

---

### 6. Get Pending Manager Approvals

**Endpoint**: `GET /api/deal/pending-manager-approvals`  
**Authorization**: SalesManager, SuperAdmin

**Response**: `200 OK` - List of deals pending manager approval.

---

### 7. Get Pending SuperAdmin Approvals

**Endpoint**: `GET /api/deal/pending-superadmin-approvals`  
**Authorization**: SuperAdmin

**Response**: `200 OK` - List of deals pending SuperAdmin approval.

---

## Sales Report Endpoints

### 1. Create Sales Report

**Endpoint**: `POST /api/salesreport`  
**Authorization**: Salesman

**Request Body**:

```json
{
	"title": "Weekly Sales Report",
	"body": "Sales activities for the week",
	"type": "Weekly",
	"reportDate": "2025-10-26T00:00:00"
}
```

**Response**: `201 Created`

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Weekly Sales Report",
		"type": "Weekly",
		"reportDate": "2025-10-26T00:00:00",
		"employeeId": "Ahmed_Ashraf_Sales_001"
	},
	"message": "Report created successfully",
	"timestamp": "2025-10-26T..."
}
```

---

### 2. Get All Sales Reports

**Endpoint**: `GET /api/salesreport`  
**Authorization**: All authenticated users

**Query Parameters**:

- `type` (string, optional) - Filter by report type
- `reportDate` (DateTime, optional) - Filter by date
- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page

**Note**: Salesman sees only their own reports. SalesManager and SuperAdmin see all reports.

---

### 3. Get Sales Report by ID

**Endpoint**: `GET /api/salesreport/{id}`  
**Authorization**: All authenticated users (role-based access)

**Response**: `200 OK` - Single report details.

**Error Responses**:

- `403 Forbidden` - Not authorized to view this report
- `404 Not Found` - Report not found

---

### 4. Rate Sales Report

**Endpoint**: `POST /api/salesreport/{id}/rate`  
**Authorization**: SalesManager, SuperAdmin

**Request Body**:

```json
{
	"rating": 4.5,
	"comments": "Good report, well detailed"
}
```

**Response**: `200 OK` - Report rated successfully.

---

## Weekly Plan Endpoints

### 1. Create Weekly Plan

**Endpoint**: `POST /api/weeklyplan`  
**Authorization**: All authenticated users

**Request Body**:

```json
{
	"title": "Week 1 Plan",
	"description": "Weekly sales activities",
	"weekStartDate": "2025-10-26T00:00:00",
	"weekEndDate": "2025-11-01T00:00:00"
}
```

**Response**: `201 Created` - Weekly plan created.

---

### 2. Get Weekly Plans

**Endpoint**: `GET /api/weeklyplan`  
**Authorization**: All authenticated users

**Query Parameters**:

- `page` (int, default: 1) - Page number
- `pageSize` (int, default: 20) - Items per page

**Response**: `200 OK` - List of weekly plans for the current user.

---

### 3. Get Weekly Plan by ID

**Endpoint**: `GET /api/weeklyplan/{id}`  
**Authorization**: All authenticated users

**Response**: `200 OK` - Single weekly plan details.

---

### 4. Submit Weekly Plan

**Endpoint**: `POST /api/weeklyplan/{id}/submit`  
**Authorization**: All authenticated users

**Response**: `200 OK` - Weekly plan submitted for review.

---

### 5. Review Weekly Plan

**Endpoint**: `POST /api/weeklyplan/{id}/review`  
**Authorization**: Admin, SalesManager

**Request Body**:

```json
{
	"reviewed": true,
	"reviewComments": "Plan looks good"
}
```

**Response**: `200 OK` - Weekly plan reviewed.

---

## Response Formats

### Success Response

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully",
  "timestamp": "2025-10-26T10:00:00Z"
}
```

### Error Response

```json
{
	"success": false,
	"message": "Error message here",
	"errors": ["Validation error 1", "Validation error 2"],
	"timestamp": "2025-10-26T10:00:00Z"
}
```

### Paginated Response

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 150,
      "totalPages": 8,
      "hasPrevious": false,
      "hasNext": true
    }
  },
  "message": "Success",
  "timestamp": "2025-10-26T10:00:00Z"
}
```

---

## HTTP Status Codes

- `200 OK` - Request successful
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict (e.g., duplicate)
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

---

## Rate Limiting

- **Global**: 100 requests per minute
- **API Endpoints**: 200 requests per minute
- **Auth Endpoints**: 10 requests per minute

When rate limit is exceeded, the API returns `429 Too Many Requests` with a `Retry-After` header.

---

**End of Documentation**
