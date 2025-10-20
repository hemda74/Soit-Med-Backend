# Weekly Planning and Client Management API Documentation

## Overview

This document provides comprehensive API documentation for the Weekly Planning and Client Management system. All endpoints require authentication using JWT tokens.

## Base URL

```
https://your-api-domain.com/api
```

## Authentication

All requests must include the Authorization header:

```
Authorization: Bearer {jwt_token}
```

---

## 1. Weekly Plan Management Endpoints

### 1.1 Create Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan`

**Description:** Creates a new weekly plan for the authenticated employee.

**Request Body:**

```json
{
	"weekStartDate": "2024-01-15T00:00:00Z",
	"weekEndDate": "2024-01-21T23:59:59Z",
	"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
	"planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير"
}
```

**Response (201 Created):**

```json
{
	"success": true,
	"message": "تم إنشاء الخطة الأسبوعية بنجاح",
	"data": {
		"id": 1,
		"employeeId": "user-123",
		"weekStartDate": "2024-01-15T00:00:00Z",
		"weekEndDate": "2024-01-21T23:59:59Z",
		"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
		"planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير",
		"status": "Draft",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z"
	}
}
```

**Error Response (400 Bad Request):**

```json
{
	"success": false,
	"message": "يوجد خطة أسبوعية بالفعل لهذا الأسبوع"
}
```

### 1.2 Get Weekly Plans

**Endpoint:** `GET /api/WeeklyPlan`

**Description:** Retrieves paginated list of weekly plans for the authenticated employee.

**Query Parameters:**

- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 1,
				"employeeId": "user-123",
				"weekStartDate": "2024-01-15T00:00:00Z",
				"weekEndDate": "2024-01-21T23:59:59Z",
				"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
				"planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير",
				"status": "Draft",
				"createdAt": "2024-01-15T10:30:00Z",
				"updatedAt": "2024-01-15T10:30:00Z",
				"planItems": []
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 5,
			"totalPages": 1
		}
	}
}
```

### 1.3 Get Weekly Plan by ID

**Endpoint:** `GET /api/WeeklyPlan/{id}`

**Description:** Retrieves a specific weekly plan with all its items.

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"employeeId": "user-123",
		"weekStartDate": "2024-01-15T00:00:00Z",
		"weekEndDate": "2024-01-21T23:59:59Z",
		"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
		"planDescription": "خطة زيارات العملاء للأسبوع الأول من يناير",
		"status": "Draft",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z",
		"planItems": [
			{
				"id": 1,
				"clientName": "د. أحمد محمد",
				"clientType": "Doctor",
				"clientSpecialization": "أمراض القلب",
				"plannedVisitDate": "2024-01-16T10:00:00Z",
				"visitPurpose": "مناقشة عقد توريد أجهزة طبية",
				"priority": "High",
				"status": "Planned"
			}
		]
	}
}
```

**Error Response (404 Not Found):**

```json
{
	"success": false,
	"message": "الخطة الأسبوعية غير موجودة"
}
```

### 1.4 Update Weekly Plan

**Endpoint:** `PUT /api/WeeklyPlan/{id}`

**Description:** Updates an existing weekly plan (only if status is Draft).

**Request Body:**

```json
{
	"planTitle": "الخطة الأسبوعية المحدثة",
	"planDescription": "وصف محدث للخطة"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم تحديث الخطة الأسبوعية بنجاح",
	"data": {
		"id": 1,
		"planTitle": "الخطة الأسبوعية المحدثة",
		"planDescription": "وصف محدث للخطة",
		"updatedAt": "2024-01-15T11:00:00Z"
	}
}
```

**Error Response (403 Forbidden):**

```json
{
	"success": false,
	"message": "ليس لديك صلاحية لتعديل هذه الخطة"
}
```

### 1.5 Submit Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/submit`

**Description:** Submits a weekly plan for approval.

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم إرسال الخطة الأسبوعية بنجاح"
}
```

**Error Response (400 Bad Request):**

```json
{
	"success": false,
	"message": "تم إرسال هذه الخطة بالفعل"
}
```

### 1.6 Approve Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/approve`

**Description:** Approves a submitted weekly plan (Admin/SalesManager only).

**Request Body:**

```json
{
	"notes": "موافقة على الخطة"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم الموافقة على الخطة الأسبوعية بنجاح"
}
```

### 1.7 Reject Weekly Plan

**Endpoint:** `POST /api/WeeklyPlan/{id}/reject`

**Description:** Rejects a submitted weekly plan (Admin/SalesManager only).

**Request Body:**

```json
{
	"reason": "سبب الرفض"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم رفض الخطة الأسبوعية بنجاح"
}
```

### 1.8 Get Current Weekly Plan

**Endpoint:** `GET /api/WeeklyPlan/current`

**Description:** Retrieves the current week's plan for the authenticated employee.

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"employeeId": "user-123",
		"weekStartDate": "2024-01-15T00:00:00Z",
		"weekEndDate": "2024-01-21T23:59:59Z",
		"planTitle": "الخطة الأسبوعية - الأسبوع الأول",
		"status": "Draft",
		"planItems": []
	}
}
```

**Error Response (404 Not Found):**

```json
{
	"success": false,
	"message": "لا توجد خطة أسبوعية حالية"
}
```

---

## 2. Weekly Plan Items Management Endpoints

### 2.1 Create Plan Item

**Endpoint:** `POST /api/WeeklyPlanItem`

**Description:** Adds a new item to a weekly plan.

**Request Body:**

```json
{
	"weeklyPlanId": 1,
	"clientName": "د. أحمد محمد",
	"clientType": "Doctor",
	"clientSpecialization": "أمراض القلب",
	"clientLocation": "الرياض",
	"clientPhone": "+966501234567",
	"clientEmail": "ahmed.mohamed@hospital.com",
	"plannedVisitDate": "2024-01-16T10:00:00Z",
	"plannedVisitTime": "10:00 AM",
	"visitPurpose": "مناقشة عقد توريد أجهزة طبية",
	"visitNotes": "زيارة مبدئية لمناقشة المتطلبات",
	"priority": "High",
	"isNewClient": false
}
```

**Response (201 Created):**

```json
{
	"success": true,
	"message": "تم إضافة عنصر الخطة بنجاح",
	"data": {
		"id": 1,
		"weeklyPlanId": 1,
		"clientName": "د. أحمد محمد",
		"clientType": "Doctor",
		"clientSpecialization": "أمراض القلب",
		"plannedVisitDate": "2024-01-16T10:00:00Z",
		"visitPurpose": "مناقشة عقد توريد أجهزة طبية",
		"priority": "High",
		"status": "Planned",
		"isNewClient": false,
		"createdAt": "2024-01-15T10:30:00Z"
	}
}
```

### 2.2 Get Plan Items

**Endpoint:** `GET /api/WeeklyPlanItem/plan/{planId}`

**Description:** Retrieves all items for a specific weekly plan.

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"weeklyPlanId": 1,
			"clientName": "د. أحمد محمد",
			"clientType": "Doctor",
			"clientSpecialization": "أمراض القلب",
			"plannedVisitDate": "2024-01-16T10:00:00Z",
			"visitPurpose": "مناقشة عقد توريد أجهزة طبية",
			"priority": "High",
			"status": "Planned",
			"isNewClient": false
		},
		{
			"id": 2,
			"weeklyPlanId": 1,
			"clientName": "مستشفى الملك فهد",
			"clientType": "Hospital",
			"plannedVisitDate": "2024-01-17T14:00:00Z",
			"visitPurpose": "عرض منتجات جديدة",
			"priority": "Medium",
			"status": "Planned",
			"isNewClient": true
		}
	]
}
```

### 2.3 Update Plan Item

**Endpoint:** `PUT /api/WeeklyPlanItem/{id}`

**Description:** Updates an existing plan item.

**Request Body:**

```json
{
	"clientName": "د. أحمد محمد",
	"plannedVisitDate": "2024-01-16T10:00:00Z",
	"visitPurpose": "مناقشة العقد النهائي",
	"priority": "High"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم تحديث عنصر الخطة بنجاح",
	"data": {
		"id": 1,
		"clientName": "د. أحمد محمد",
		"plannedVisitDate": "2024-01-16T10:00:00Z",
		"visitPurpose": "مناقشة العقد النهائي",
		"priority": "High",
		"updatedAt": "2024-01-15T11:00:00Z"
	}
}
```

### 2.4 Complete Plan Item

**Endpoint:** `POST /api/WeeklyPlanItem/{id}/complete`

**Description:** Marks a plan item as completed with results and feedback.

**Request Body:**

```json
{
	"results": "تم الاتفاق على العقد بنجاح",
	"feedback": "العميل راضي عن العرض",
	"satisfactionRating": 5,
	"nextVisitDate": "2024-01-23T10:00:00Z",
	"followUpNotes": "متابعة تسليم الأجهزة"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم تحديث حالة العنصر بنجاح"
}
```

### 2.5 Cancel Plan Item

**Endpoint:** `POST /api/WeeklyPlanItem/{id}/cancel`

**Description:** Cancels a plan item with a reason.

**Request Body:**

```json
{
	"reason": "العميل غير متاح"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم إلغاء العنصر بنجاح"
}
```

### 2.6 Postpone Plan Item

**Endpoint:** `POST /api/WeeklyPlanItem/{id}/postpone`

**Description:** Postpones a plan item to a new date.

**Request Body:**

```json
{
	"newDate": "2024-01-18T10:00:00Z",
	"reason": "تأجيل بناءً على طلب العميل"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم تأجيل العنصر بنجاح"
}
```

### 2.7 Get Overdue Items

**Endpoint:** `GET /api/WeeklyPlanItem/overdue`

**Description:** Retrieves all overdue plan items for the authenticated employee.

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 2,
			"clientName": "مستشفى الملك فهد",
			"plannedVisitDate": "2024-01-14T10:00:00Z",
			"visitPurpose": "عرض منتجات جديدة",
			"priority": "High",
			"status": "Planned"
		}
	]
}
```

### 2.8 Get Upcoming Items

**Endpoint:** `GET /api/WeeklyPlanItem/upcoming`

**Description:** Retrieves upcoming plan items for the authenticated employee.

**Query Parameters:**

- `days` (optional): Number of days ahead to look (default: 7)

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 3,
			"clientName": "عيادة النور",
			"plannedVisitDate": "2024-01-17T09:00:00Z",
			"visitPurpose": "متابعة طلب سابق",
			"priority": "Medium",
			"status": "Planned"
		}
	]
}
```

---

## 3. Client Management Endpoints

### 3.1 Search Clients

**Endpoint:** `GET /api/Client/search`

**Description:** Searches for clients by name, specialization, location, phone, or email.

**Query Parameters:**

- `query` (required): Search term
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "د. أحمد محمد",
			"type": "Doctor",
			"specialization": "أمراض القلب",
			"location": "الرياض",
			"phone": "+966501234567",
			"email": "ahmed.mohamed@hospital.com",
			"status": "Active",
			"priority": "High",
			"createdAt": "2024-01-15T10:30:00Z"
		}
	]
}
```

### 3.2 Create Client

**Endpoint:** `POST /api/Client`

**Description:** Creates a new client.

**Request Body:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب",
	"location": "الرياض",
	"phone": "+966501234567",
	"email": "ahmed.mohamed@hospital.com",
	"website": "www.ahmedclinic.com",
	"address": "شارع الملك فهد، الرياض",
	"city": "الرياض",
	"governorate": "منطقة الرياض",
	"postalCode": "12345",
	"notes": "عميل مهم - متخصص في أمراض القلب",
	"status": "Potential",
	"priority": "High",
	"potentialValue": 500000,
	"contactPerson": "د. أحمد محمد",
	"contactPersonPhone": "+966501234567",
	"contactPersonEmail": "ahmed.mohamed@hospital.com",
	"contactPersonPosition": "رئيس قسم أمراض القلب",
	"assignedTo": "user-456"
}
```

**Response (201 Created):**

```json
{
	"success": true,
	"message": "تم إنشاء العميل بنجاح",
	"data": {
		"id": 1,
		"name": "د. أحمد محمد",
		"type": "Doctor",
		"specialization": "أمراض القلب",
		"location": "الرياض",
		"phone": "+966501234567",
		"email": "ahmed.mohamed@hospital.com",
		"status": "Potential",
		"priority": "High",
		"createdBy": "user-123",
		"createdAt": "2024-01-15T10:30:00Z"
	}
}
```

**Error Response (400 Bad Request):**

```json
{
	"success": false,
	"message": "العميل موجود بالفعل"
}
```

### 3.3 Get Client by ID

**Endpoint:** `GET /api/Client/{id}`

**Description:** Retrieves detailed information about a specific client.

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"name": "د. أحمد محمد",
		"type": "Doctor",
		"specialization": "أمراض القلب",
		"location": "الرياض",
		"phone": "+966501234567",
		"email": "ahmed.mohamed@hospital.com",
		"website": "www.ahmedclinic.com",
		"address": "شارع الملك فهد، الرياض",
		"city": "الرياض",
		"governorate": "منطقة الرياض",
		"postalCode": "12345",
		"notes": "عميل مهم - متخصص في أمراض القلب",
		"status": "Active",
		"priority": "High",
		"potentialValue": 500000,
		"contactPerson": "د. أحمد محمد",
		"contactPersonPhone": "+966501234567",
		"contactPersonEmail": "ahmed.mohamed@hospital.com",
		"contactPersonPosition": "رئيس قسم أمراض القلب",
		"lastContactDate": "2024-01-15T10:30:00Z",
		"nextContactDate": "2024-01-22T10:00:00Z",
		"satisfactionRating": 5,
		"createdBy": "user-123",
		"assignedTo": "user-456",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z",
		"visits": [],
		"interactions": [],
		"offers": [],
		"deals": []
	}
}
```

### 3.4 Update Client

**Endpoint:** `PUT /api/Client/{id}`

**Description:** Updates an existing client's information.

**Request Body:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب",
	"phone": "+966501234567",
	"email": "ahmed.mohamed@hospital.com",
	"status": "Active",
	"priority": "High",
	"potentialValue": 750000,
	"contactPerson": "د. أحمد محمد",
	"contactPersonPhone": "+966501234567",
	"contactPersonEmail": "ahmed.mohamed@hospital.com",
	"assignedTo": "user-456"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم تحديث العميل بنجاح",
	"data": {
		"id": 1,
		"name": "د. أحمد محمد",
		"status": "Active",
		"priority": "High",
		"potentialValue": 750000,
		"updatedAt": "2024-01-15T11:00:00Z"
	}
}
```

### 3.5 Find or Create Client

**Endpoint:** `POST /api/Client/find-or-create`

**Description:** Searches for a client by name, and creates one if not found.

**Request Body:**

```json
{
	"name": "د. أحمد محمد",
	"type": "Doctor",
	"specialization": "أمراض القلب"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"message": "تم العثور على العميل أو إنشاؤه بنجاح",
	"data": {
		"id": 1,
		"name": "د. أحمد محمد",
		"type": "Doctor",
		"specialization": "أمراض القلب",
		"status": "Potential",
		"priority": "Medium",
		"createdBy": "user-123",
		"createdAt": "2024-01-15T10:30:00Z"
	}
}
```

### 3.6 Get My Clients

**Endpoint:** `GET /api/Client/my-clients`

**Description:** Retrieves clients created by or assigned to the authenticated employee.

**Query Parameters:**

- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "د. أحمد محمد",
			"type": "Doctor",
			"specialization": "أمراض القلب",
			"status": "Active",
			"priority": "High",
			"lastContactDate": "2024-01-15T10:30:00Z",
			"nextContactDate": "2024-01-22T10:00:00Z"
		}
	]
}
```

### 3.7 Get Clients Needing Follow-up

**Endpoint:** `GET /api/Client/follow-up-needed`

**Description:** Retrieves clients that need follow-up based on next contact date.

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "د. أحمد محمد",
			"type": "Doctor",
			"nextContactDate": "2024-01-16T10:00:00Z",
			"priority": "High",
			"status": "Active"
		}
	]
}
```

### 3.8 Get Client Statistics

**Endpoint:** `GET /api/Client/statistics`

**Description:** Retrieves client statistics for the authenticated employee.

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"myClientsCount": 25,
		"totalClientsCount": 150,
		"clientsByType": [
			{ "type": "Doctor", "count": 15 },
			{ "type": "Hospital", "count": 8 },
			{ "type": "Clinic", "count": 2 }
		],
		"clientsByStatus": [
			{ "status": "Active", "count": 20 },
			{ "status": "Potential", "count": 5 }
		],
		"clientsByPriority": [
			{ "priority": "High", "count": 10 },
			{ "priority": "Medium", "count": 12 },
			{ "priority": "Low", "count": 3 }
		]
	}
}
```

---

## 4. Error Responses

### 4.1 Common Error Codes

| Status Code | Description                             |
| ----------- | --------------------------------------- |
| 200         | OK - Request successful                 |
| 201         | Created - Resource created successfully |
| 400         | Bad Request - Invalid request data      |
| 401         | Unauthorized - Authentication required  |
| 403         | Forbidden - Insufficient permissions    |
| 404         | Not Found - Resource not found          |
| 500         | Internal Server Error - Server error    |

### 4.2 Error Response Format

```json
{
	"success": false,
	"message": "Error message in Arabic",
	"errors": {
		"fieldName": "Field-specific error message"
	},
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### 4.3 Validation Error Example

```json
{
	"success": false,
	"message": "حدث خطأ في معالجة الطلب",
	"errors": {
		"clientName": "اسم العميل مطلوب",
		"plannedVisitDate": "تاريخ الزيارة مطلوب",
		"email": "البريد الإلكتروني غير صحيح"
	}
}
```

---

## 5. Authentication and Authorization

### 5.1 Required Headers

All requests must include:

```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

### 5.2 Role-Based Access

| Role         | Permissions                         |
| ------------ | ----------------------------------- |
| Employee     | Create/update own plans and clients |
| SalesManager | Approve/reject plans, view all data |
| Admin        | Full access to all resources        |

---

## 6. Rate Limiting

- **Rate Limit:** 1000 requests per hour per user
- **Headers:** Rate limit information included in response headers
- **Exceeded:** Returns 429 Too Many Requests

---

## 7. Pagination

All list endpoints support pagination:

**Query Parameters:**

- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)

**Response Format:**

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

---

## 8. Date Formats

All dates are in ISO 8601 format:

- **Format:** `YYYY-MM-DDTHH:mm:ssZ`
- **Example:** `2024-01-15T10:30:00Z`
- **Timezone:** UTC

---

## 9. Example Usage

### 9.1 Complete Workflow Example

```javascript
// 1. Create a weekly plan
const plan = await fetch('/api/WeeklyPlan', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		weekStartDate: '2024-01-15T00:00:00Z',
		weekEndDate: '2024-01-21T23:59:59Z',
		planTitle: 'الخطة الأسبوعية - الأسبوع الأول',
		planDescription: 'خطة زيارات العملاء للأسبوع الأول من يناير',
	}),
});

// 2. Add plan items
const item = await fetch('/api/WeeklyPlanItem', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		weeklyPlanId: plan.id,
		clientName: 'د. أحمد محمد',
		clientType: 'Doctor',
		plannedVisitDate: '2024-01-16T10:00:00Z',
		visitPurpose: 'مناقشة عقد توريد أجهزة طبية',
		priority: 'High',
		isNewClient: false,
	}),
});

// 3. Submit plan for approval
await fetch(`/api/WeeklyPlan/${plan.id}/submit`, {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
	},
});

// 4. Complete a visit
await fetch(`/api/WeeklyPlanItem/${item.id}/complete`, {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify({
		results: 'تم الاتفاق على العقد بنجاح',
		feedback: 'العميل راضي عن العرض',
		satisfactionRating: 5,
		nextVisitDate: '2024-01-23T10:00:00Z',
	}),
});
```

This documentation provides comprehensive coverage of all API endpoints with detailed request/response examples and error handling scenarios.
