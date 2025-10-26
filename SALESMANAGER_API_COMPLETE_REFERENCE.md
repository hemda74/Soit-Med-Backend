# SalesManager API - Complete Reference

**Version**: 1.0  
**Date**: 2025-10-26  
**Base URL**: `http://localhost:5117/api`  
**Authentication**: JWT Bearer Token

---

## SalesManager Role Overview

**SalesManager** is authorized for:

- ✅ View all offers (system-wide)
- ✅ View all deals
- ✅ Approve deals
- ✅ Assign offer requests
- ✅ View task progress (all employees)
- ✅ Review weekly plans
- ✅ Rate sales reports
- ✅ All Salesman capabilities

---

## Table of Contents

1. [Client Management](#client-management)
2. [Task Progress Management](#task-progress-management)
3. [Offer Request Management](#offer-request-management)
4. [Offer Management](#offer-management)
5. [Deal Management](#deal-management)
6. [Sales Report Management](#sales-report-management)
7. [Weekly Plan Management](#weekly-plan-management)

---

# 1. Client Management

## Get Client Profile with History

**Endpoint**: `GET /api/client/{id}/profile`  
**Authorization**: SalesManager, Salesman, SuperAdmin

### Request Example

```http
GET /api/client/24/profile
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"client": {
			"id": 24,
			"name": "Cairo University Hospital",
			"type": "Hospital",
			"specialization": "General Medicine",
			"status": "Active",
			"priority": "High",
			"location": "Cairo",
			"phone": "01234567890",
			"email": "info@hospital.com"
		},
		"history": {
			"tasks": [
				{
					"id": 80,
					"title": "Visit Cairo Hospital",
					"status": "Completed",
					"createdAt": "2025-10-20T10:00:00"
				}
			],
			"offers": [
				{
					"id": 1,
					"products": "MRI Scanner Package",
					"totalAmount": 2500000.0,
					"status": "Accepted",
					"createdAt": "2025-10-15T10:00:00"
				}
			],
			"deals": [
				{
					"id": 1,
					"dealValue": 2500000.0,
					"status": "Approved",
					"closedDate": "2025-10-20T10:00:00"
				}
			],
			"visits": [
				{
					"id": 1,
					"progressType": "Visit",
					"description": "Initial client visit",
					"progressDate": "2025-10-20T10:00:00"
				}
			]
		},
		"statistics": {
			"totalTasks": 15,
			"totalOffers": 5,
			"totalDeals": 2,
			"totalValue": 5000000.0,
			"averageDealValue": 2500000.0
		}
	},
	"message": "Client profile retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**403 Forbidden**

```json
{
	"success": false,
	"message": "You don't have permission to view this client profile"
}
```

**404 Not Found**

```json
{
	"success": false,
	"message": "Client not found"
}
```

**500 Internal Server Error**

```json
{
	"success": false,
	"message": "An error occurred while retrieving client profile"
}
```

---

# 2. Task Progress Management

## Get All Task Progress (All Employees)

**Endpoint**: `GET /api/taskprogress`  
**Authorization**: SalesManager, Salesman, SuperAdmin

### Request Example

```http
GET /api/taskprogress
Authorization: Bearer {token}

# With optional filters
GET /api/taskprogress?startDate=2025-10-01&endDate=2025-10-26
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"taskId": 80,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"employeeId": "Ahmed_Ashraf_Sales_001",
			"employeeName": "Ahmed Ashraf",
			"progressDate": "2025-10-26T10:00:00",
			"progressType": "Visit",
			"description": "Initial client visit completed successfully",
			"visitResult": "Interested",
			"nextStep": "NeedsOffer",
			"status": "Completed"
		}
	],
	"message": "All task progress retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Note**: SalesManager sees ALL employees' progress, not just their own.

### Error Responses

**401 Unauthorized**

```json
{
	"success": false,
	"message": "Unauthorized access"
}
```

**500 Internal Server Error**

```json
{
	"success": false,
	"message": "An error occurred while retrieving task progress"
}
```

---

## Get Progress by Employee

**Endpoint**: `GET /api/taskprogress/employee/{employeeId}`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
GET /api/taskprogress/employee/Ahmed_Ashraf_Sales_001?startDate=2025-10-01&endDate=2025-10-26
Authorization: Bearer {token}
```

### Success Response (200 OK)

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
	"message": "Employee progress retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

- `400 Bad Request` - Invalid employee ID
- `500 Internal Server Error` - Server error

---

# 3. Offer Request Management

## Get All Offer Requests

**Endpoint**: `GET /api/offerrequest`  
**Authorization**: SalesManager, Salesman, SalesSupport, SuperAdmin

### Request Example

```http
GET /api/offerrequest?status=Requested
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"requestedBy": "Ahmed_Ashraf_Sales_001",
			"requestedByName": "Ahmed Ashraf",
			"assignedTo": null,
			"assignedToName": null,
			"requestedProducts": "MRI Scanner Model X1",
			"specialNotes": "Urgent request for Q2 delivery",
			"status": "Requested",
			"createdAt": "2025-10-26T10:00:00"
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Note**: SalesManager sees ALL offer requests (all salesmen).

---

## Assign Offer Request

**Endpoint**: `PUT /api/offerrequest/{id}/assign`  
**Authorization**: SalesManager, SalesSupport, SuperAdmin

### Request Example

```http
PUT /api/offerrequest/1/assign
Authorization: Bearer {token}
Content-Type: application/json

{
  "assignedTo": "Ahmed_Hemdan_Engineering_001"
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"assignedTo": "Ahmed_Hemdan_Engineering_001",
		"assignedToName": "Ahmed Hemdan",
		"status": "InProgress",
		"updatedAt": "2025-10-26T12:00:00"
	},
	"message": "Offer request assigned successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**400 Bad Request**

```json
{
	"success": false,
	"message": "AssignedTo is required"
}
```

---

## Update Offer Request Status

**Endpoint**: `PUT /api/offerrequest/{id}/status`  
**Authorization**: SalesManager, SalesSupport, SuperAdmin

### Request Example

```http
PUT /api/offerrequest/1/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Completed",
  "notes": "Offer created successfully from this request"
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"status": "Completed",
		"notes": "Offer created successfully",
		"updatedAt": "2025-10-26T12:00:00"
	},
	"message": "Offer request status updated successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Get Offer Requests by Salesman

**Endpoint**: `GET /api/offerrequest/salesman/{salesmanId}`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
GET /api/offerrequest/salesman/Ahmed_Ashraf_Sales_001?status=Requested
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"clientId": 24,
			"requestedProducts": "MRI Scanner Model X1",
			"status": "Requested"
		}
	],
	"message": "Salesman offer requests retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

# 4. Offer Management

## Get All Offers (System-Wide)

**Endpoint**: `GET /api/offer`  
**Authorization**: SalesManager, SalesSupport, SuperAdmin

### Request Example

```http
GET /api/offer?status=Draft&clientId=24&startDate=2025-10-01&endDate=2025-10-26
Authorization: Bearer {token}
```

**Available Filters**:

- `status` - Filter by status (Draft, Sent, Accepted, Rejected)
- `clientId` - Filter by client ID
- `startDate` - Filter from date
- `endDate` - Filter until date

### Success Response (200 OK)

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
			"createdByName": "Ahmed Hemdan",
			"assignedTo": "Ahmed_Ashraf_Sales_001",
			"assignedToName": "Ahmed Ashraf",
			"products": "Complete MRI scanner package",
			"totalAmount": 2500000.0,
			"paymentTerms": "50% Down, 50% Delivery",
			"deliveryTerms": "30 days FOB Cairo",
			"validUntil": "2025-11-25T14:17:39",
			"status": "Draft",
			"createdAt": "2025-10-26T11:17:39"
		}
	],
	"message": "Offers retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Note**: SalesManager sees ALL offers from ALL users.

---

## Get My Created Offers

**Endpoint**: `GET /api/offer/my-offers`  
**Authorization**: SalesManager, SalesSupport, SuperAdmin

### Request Example

```http
GET /api/offer/my-offers?status=Draft&startDate=2025-10-01&endDate=2025-10-26
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 3,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"products": "MRI Scanner Package",
			"totalAmount": 2500000.0,
			"status": "Draft"
		}
	],
	"message": "My offers retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Create Offer

**Endpoint**: `POST /api/offer`  
**Authorization**: SalesManager, SalesSupport

### Request Example

```http
POST /api/offer
Authorization: Bearer {token}
Content-Type: application/json

{
  "offerRequestId": 6,
  "clientId": 24,
  "assignedTo": "Ahmed_Ashraf_Sales_001",
  "products": "Complete MRI scanner package including installation, training, and 2-year warranty",
  "totalAmount": 2500000.00,
  "paymentTerms": "50% Down payment, 50% on delivery",
  "deliveryTerms": "FOB Cairo, 30 days delivery",
  "validUntil": "2025-11-25T14:17:39",
  "notes": "High priority offer"
}
```

### Success Response (200 OK)

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
		"createdBy": "Ahmed_Hemdan_Sales_002",
		"createdAt": "2025-10-26T12:00:00"
	},
	"message": "Offer created successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**400 Bad Request**

```json
{
	"success": false,
	"message": "Validation errors",
	"errors": [
		"Products field is required",
		"TotalAmount must be greater than 0"
	]
}
```

---

# 5. Deal Management

## Create Deal

**Endpoint**: `POST /api/deal`  
**Authorization**: SalesManager, Salesman

### Request Example

```http
POST /api/deal
Authorization: Bearer {token}
Content-Type: application/json

{
  "offerId": 1,
  "clientId": 24,
  "dealValue": 2500000.00,
  "paymentTerms": "50% Down, 50% Delivery",
  "deliveryTerms": "30 days FOB Cairo",
  "notes": "Deal approved by client"
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 1,
		"clientId": 24,
		"clientName": "Cairo University Hospital",
		"salesmanId": "Ahmed_Ashraf_Sales_001",
		"salesmanName": "Ahmed Ashraf",
		"dealValue": 2500000.0,
		"status": "PendingManagerApproval",
		"createdAt": "2025-10-26T10:00:00"
	},
	"message": "Deal created successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**400 Bad Request**

```json
{
	"success": false,
	"message": "Offer not found"
}
```

---

## Get All Deals

**Endpoint**: `GET /api/deal`  
**Authorization**: SalesManager, Salesman, SuperAdmin

### Request Example

```http
GET /api/deal?status=PendingManagerApproval&salesmanId=Ahmed_Ashraf_Sales_001
Authorization: Bearer {token}
```

**Available Filters**:

- `status` - Filter by status (PendingManagerApproval, Approved, Rejected, etc.)
- `salesmanId` - Filter by salesman

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"offerId": 1,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"salesmanId": "Ahmed_Ashraf_Sales_001",
			"salesmanName": "Ahmed Ashraf",
			"dealValue": 2500000.0,
			"status": "PendingManagerApproval",
			"createdAt": "2025-10-26T10:00:00"
		}
	],
	"message": "Deals retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Note**: SalesManager sees ALL deals (all salesmen).

---

## Get Deal by ID

**Endpoint**: `GET /api/deal/{id}`  
**Authorization**: SalesManager, Salesman, SuperAdmin

### Request Example

```http
GET /api/deal/1
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 1,
		"clientId": 24,
		"clientName": "Cairo University Hospital",
		"salesmanId": "Ahmed_Ashraf_Sales_001",
		"dealValue": 2500000.0,
		"status": "PendingManagerApproval",
		"managerApprovedBy": null,
		"managerApprovedAt": null,
		"superAdminApprovedBy": null,
		"createdAt": "2025-10-26T10:00:00"
	},
	"message": "Deal retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Manager Approval

**Endpoint**: `POST /api/deal/{id}/manager-approval`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
POST /api/deal/1/manager-approval
Authorization: Bearer {token}
Content-Type: application/json

{
  "approved": true,
  "comments": "Approved by sales manager. Good deal."
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"status": "PendingSuperAdminApproval",
		"managerApprovedBy": "Ahmed_Hemdan_Sales_002",
		"managerApprovedAt": "2025-10-26T12:00:00",
		"managerComments": "Approved by sales manager. Good deal."
	},
	"message": "Manager approval processed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**400 Bad Request** (If Rejected)

```json
{
	"success": false,
	"message": "Deal rejected",
	"errors": [
		"managerRejectionReason field is required when approved is false"
	]
}
```

---

## Get Pending Manager Approvals

**Endpoint**: `GET /api/deal/pending-manager-approvals`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
GET /api/deal/pending-manager-approvals
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"offerId": 1,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"salesmanId": "Ahmed_Ashraf_Sales_001",
			"dealValue": 2500000.0,
			"status": "PendingManagerApproval",
			"createdAt": "2025-10-26T10:00:00"
		}
	],
	"message": "Pending manager approvals retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Get Deals by Salesman

**Endpoint**: `GET /api/deal/by-salesman/{salesmanId}`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
GET /api/deal/by-salesman/Ahmed_Ashraf_Sales_001?status=Approved
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"clientId": 24,
			"dealValue": 2500000.0,
			"status": "Approved",
			"createdAt": "2025-10-26T10:00:00"
		}
	],
	"message": "Salesman deals retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Mark Deal as Completed

**Endpoint**: `POST /api/deal/{id}/complete`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
POST /api/deal/1/complete
Authorization: Bearer {token}
Content-Type: application/json

{
  "completionNotes": "Deal completed successfully. Delivery done."
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"status": "Completed",
		"completedAt": "2025-10-26T12:00:00",
		"completionNotes": "Deal completed successfully."
	},
	"message": "Deal marked as completed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Mark Deal as Failed

**Endpoint**: `POST /api/deal/{id}/fail`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
POST /api/deal/1/fail
Authorization: Bearer {token}
Content-Type: application/json

{
  "failureNotes": "Client cancelled the deal due to budget constraints"
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"status": "Failed",
		"failureNotes": "Client cancelled the deal"
	},
	"message": "Deal marked as failed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

# 6. Sales Report Management

## Get All Sales Reports

**Endpoint**: `GET /api/salesreport`  
**Authorization**: All authenticated users (SalesManager sees ALL reports)

### Request Example

```http
GET /api/salesreport?type=Weekly&reportDate=2025-10-26&page=1&pageSize=20
Authorization: Bearer {token}
```

**Available Filters**:

- `type` - Weekly, Monthly, etc.
- `reportDate` - Filter by date
- `page` - Page number
- `pageSize` - Items per page

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"items": [
			{
				"id": 1,
				"title": "Weekly Sales Report",
				"body": "Sales activities for the week",
				"type": "Weekly",
				"reportDate": "2025-10-26",
				"employeeId": "Ahmed_Ashraf_Sales_001",
				"employeeName": "Ahmed Ashraf",
				"rating": 4.5,
				"createdAt": "2025-10-26T10:00:00"
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 50,
			"totalPages": 3,
			"hasPrevious": false,
			"hasNext": true
		}
	},
	"message": "Found 50 report(s)",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Note**: SalesManager sees ALL reports from ALL salesmen.

---

## Get Sales Report by ID

**Endpoint**: `GET /api/salesreport/{id}`  
**Authorization**: All authenticated users

### Request Example

```http
GET /api/salesreport/1
Authorization: Bearer {token}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"title": "Weekly Sales Report",
		"body": "Sales activities for the week...",
		"type": "Weekly",
		"reportDate": "2025-10-26",
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employeeName": "Ahmed Ashraf",
		"rating": 4.5,
		"comments": "Good report, well detailed"
	},
	"message": "Report retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Rate Sales Report

**Endpoint**: `POST /api/salesreport/{id}/rate`  
**Authorization**: SalesManager, SuperAdmin

### Request Example

```http
POST /api/salesreport/1/rate
Authorization: Bearer {token}
Content-Type: application/json

{
  "rating": 4.5,
  "comments": "Good report, well detailed. Keep up the good work!"
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 1,
		"rating": 4.5,
		"comments": "Good report, well detailed. Keep up the good work!",
		"ratedAt": "2025-10-26T12:00:00",
		"ratedBy": "Ahmed_Hemdan_Sales_002"
	},
	"message": "Report rated successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

### Error Responses

**400 Bad Request**

```json
{
	"success": false,
	"message": "Rating must be between 0 and 5"
}
```

**404 Not Found**

```json
{
	"success": false,
	"message": "Report not found"
}
```

---

# 7. Weekly Plan Management

## Review Weekly Plan

**Endpoint**: `POST /api/weeklyplan/{id}/review`  
**Authorization**: Admin, SalesManager

### Request Example

```http
POST /api/weeklyplan/34/review
Authorization: Bearer {token}
Content-Type: application/json

{
  "reviewed": true,
  "reviewComments": "Plan looks good. Approved."
}
```

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 34,
		"reviewedBy": "Ahmed_Hemdan_Sales_002",
		"reviewedAt": "2025-10-26T12:00:00",
		"reviewComments": "Plan looks good. Approved.",
		"status": "Reviewed"
	},
	"message": "Weekly plan reviewed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Complete Endpoint Summary for SalesManager

| Endpoint                              | Method | Summary                           | Access Level |
| ------------------------------------- | ------ | --------------------------------- | ------------ |
| `/api/client/{id}/profile`            | GET    | Client profile with history       | All          |
| `/api/taskprogress`                   | GET    | All task progress (all employees) | All          |
| `/api/taskprogress/employee/{id}`     | GET    | Specific employee's progress      | Manager only |
| `/api/offerrequest`                   | GET    | All offer requests                | All          |
| `/api/offerrequest/{id}/assign`       | PUT    | Assign request to support         | Manager      |
| `/api/offerrequest/{id}/status`       | PUT    | Update request status             | Manager      |
| `/api/offerrequest/salesman/{id}`     | GET    | Requests by salesman              | Manager      |
| `/api/offerrequest/assigned/{id}`     | GET    | Assigned requests                 | All          |
| `/api/offer`                          | GET    | All offers system-wide            | All          |
| `/api/offer/my-offers`                | GET    | Your created offers               | All          |
| `/api/offer/{id}`                     | GET    | Single offer details              | All          |
| `/api/offer`                          | POST   | Create new offer                  | Manager      |
| `/api/deal`                           | GET    | All deals                         | All          |
| `/api/deal/{id}`                      | GET    | Single deal                       | All          |
| `/api/deal`                           | POST   | Create deal                       | Manager      |
| `/api/deal/by-salesman/{id}`          | GET    | Deals by salesman                 | Manager      |
| `/api/deal/{id}/manager-approval`     | POST   | Approve/reject deal               | Manager      |
| `/api/deal/pending-manager-approvals` | GET    | Pending approvals                 | Manager      |
| `/api/deal/{id}/complete`             | POST   | Mark deal completed               | Manager      |
| `/api/deal/{id}/fail`                 | POST   | Mark deal failed                  | Manager      |
| `/api/salesreport`                    | GET    | All reports (all employees)       | All          |
| `/api/salesreport/{id}`               | GET    | Single report                     | All          |
| `/api/salesreport/{id}/rate`          | POST   | Rate report                       | Manager      |
| `/api/weeklyplan/{id}/review`         | POST   | Review plan                       | Manager      |

---

## Common Error Responses

### 401 Unauthorized

```json
{
	"success": false,
	"message": "Unauthorized"
}
```

**Cause**: Missing or invalid token  
**Solution**: Add valid Bearer token

---

### 403 Forbidden

```json
{
	"success": false,
	"message": "Forbidden"
}
```

**Cause**: Insufficient permissions  
**Solution**: Use SalesManager role token

---

### 400 Bad Request

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": ["Field X is required", "Field Y must be a valid date"]
}
```

**Cause**: Invalid request data  
**Solution**: Check request body and parameters

---

### 404 Not Found

```json
{
	"success": false,
	"message": "Resource not found"
}
```

**Cause**: Resource doesn't exist  
**Solution**: Verify ID is correct

---

### 409 Conflict

```json
{
	"success": false,
	"message": "A report with the same type and date already exists"
}
```

**Cause**: Duplicate resource  
**Solution**: Use different date/type or update existing

---

### 429 Too Many Requests

```json
{
	"success": false,
	"message": "Too many requests"
}
```

**Cause**: Rate limit exceeded  
**Solution**: Wait before retrying

---

### 500 Internal Server Error

```json
{
	"success": false,
	"message": "An error occurred while processing your request"
}
```

**Cause**: Server error  
**Solution**: Check server logs, contact support

---

## Rate Limiting

- **Global**: 100 requests per minute
- **API Endpoints**: 200 requests per minute
- **Auth Endpoints**: 10 requests per minute

---

## Test Token for SalesManager

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNtYW5hZ2VyQHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fU2FsZXNfMDAyIiwianRpIjoiYWIzZDIyZjctNTZlMy00OGQ2LWJjYTctYjhiZWZiOTVmZWRhIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNNYW5hZ2VyIiwiZXhwIjoxOTE5MjM1NTExLCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.aJdJlWQ9SLQqfWYG1b2uiDiE4Ba7PZucXSavdfj0U2k
```

**Credentials**:

- Email: `salesmanager@soitmed.com`
- Password: `356120Ahmed@shraf2`

---

**End of SalesManager API Reference**
