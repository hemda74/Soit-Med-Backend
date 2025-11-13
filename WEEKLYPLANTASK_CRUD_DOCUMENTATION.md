# WeeklyPlanTask CRUD Endpoints Documentation

## Overview

Full CRUD (Create, Read, Update, Delete) operations for WeeklyPlanTask entities have been implemented. Salesmen can now create and manage tasks within their weekly plans, with proper authorization and validation.

---

## 🎯 Features

✅ **Full CRUD Operations** - Create, Read, Update, Delete tasks  
✅ **Task Creation During Plan Creation** - Add tasks when creating a weekly plan  
✅ **Comprehensive Validation** - Task type, priority, status, client validation  
✅ **Authorization** - Salesmen can only manage their own tasks  
✅ **Soft Delete** - Tasks with progress are cancelled instead of deleted  
✅ **Detailed Responses** - Includes related progress, offers, and deals

---

## 📋 API Endpoints

### 1. Create Task

**Endpoint:** `POST /api/WeeklyPlanTask`

**Authorization:** `Bearer {token}` (Salesman, SalesManager, SuperAdmin)

**Request Body:**

**For Existing Clients (Old):**

```json
{
	"weeklyPlanId": 116,
	"title": "Visit Ahmed Hospital",
	"taskType": "Visit",
	"clientId": 25,
	"clientStatus": "Old",
	"plannedDate": "2025-01-15T10:00:00Z",
	"plannedTime": "10:00 AM",
	"purpose": "Discuss new equipment requirements",
	"notes": "Focus on X-Ray machines",
	"priority": "High",
	"status": "Planned",
	"clientClassification": "A"
}
```

**⚠️ Validation Rules:**

- When `clientStatus = "Old"`: `clientId` is **REQUIRED** and will be validated (client must exist)
- When `clientStatus = "New"`: `clientName` is **REQUIRED** and `clientId` is **NOT validated** (no client exists yet)

**For New Clients:**

```json
{
	"weeklyPlanId": 116,
	"title": "Visit New Hospital",
	"taskType": "Visit",
	"clientStatus": "New",
	"clientName": "New Hospital Name",
	"placeName": "Main Building",
	"placeType": "Hospital",
	"clientPhone": "+20 1234567890",
	"clientAddress": "123 Main Street",
	"clientLocation": "Cairo",
	"plannedDate": "2025-01-16T14:00:00Z",
	"priority": "Medium",
	"status": "Planned"
}
```

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 200,
		"weeklyPlanId": 116,
		"title": "Visit Ahmed Hospital",
		"taskType": "Visit",
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"clientStatus": "Old",
		"plannedDate": "2025-01-15T10:00:00Z",
		"plannedTime": "10:00 AM",
		"purpose": "Discuss new equipment requirements",
		"notes": "Focus on X-Ray machines",
		"priority": "High",
		"status": "Planned",
		"progressCount": 0,
		"createdAt": "2025-01-13T08:00:00Z",
		"updatedAt": "2025-01-13T08:00:00Z",
		"progresses": [],
		"offerRequests": [],
		"offers": [],
		"deals": []
	},
	"message": "Task created successfully"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors, invalid task type/priority/status, missing required fields
- **403 Forbidden:** User doesn't own the weekly plan
- **404 Not Found:** Weekly plan or client not found

---

### 2. Get Task by ID

**Endpoint:** `GET /api/WeeklyPlanTask/{id}`

**Authorization:** `Bearer {token}` (Salesman, SalesManager, SuperAdmin)

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 200,
		"weeklyPlanId": 116,
		"title": "Visit Ahmed Hospital",
		"taskType": "Visit",
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"clientStatus": "Old",
		"plannedDate": "2025-01-15T10:00:00Z",
		"priority": "High",
		"status": "Planned",
		"progressCount": 2,
		"progresses": [
			{
				"id": 50,
				"progressDate": "2025-01-15T10:30:00Z",
				"progressType": "Visit",
				"description": "Met with procurement manager",
				"visitResult": "Interested",
				"nextStep": "NeedsOffer"
			}
		],
		"offerRequests": [],
		"offers": [],
		"deals": []
	}
}
```

**Error Responses:**

- **403 Forbidden:** User doesn't have permission to view this task
- **404 Not Found:** Task not found

---

### 3. Get Tasks by Weekly Plan

**Endpoint:** `GET /api/WeeklyPlanTask/by-plan/{weeklyPlanId}`

**Authorization:** `Bearer {token}` (Salesman, SalesManager, SuperAdmin)

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 200,
			"title": "Visit Ahmed Hospital",
			"taskType": "Visit",
			"priority": "High",
			"status": "Planned",
			"progressCount": 0
		},
		{
			"id": 201,
			"title": "Follow up on offer",
			"taskType": "FollowUp",
			"priority": "Medium",
			"status": "InProgress",
			"progressCount": 1
		}
	],
	"message": "Found 2 task(s)"
}
```

**Error Responses:**

- **403 Forbidden:** User doesn't own the weekly plan
- **404 Not Found:** Weekly plan not found

---

### 4. Update Task

**Endpoint:** `PUT /api/WeeklyPlanTask/{id}`

**Authorization:** `Bearer {token}` (Salesman, SalesManager, SuperAdmin)

**Request Body (All fields optional):**

```json
{
	"title": "Updated Task Title",
	"taskType": "FollowUp",
	"priority": "Medium",
	"status": "InProgress",
	"plannedDate": "2025-01-20T15:00:00Z",
	"notes": "Updated notes",
	"purpose": "Updated purpose"
}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 200,
		"title": "Updated Task Title",
		"taskType": "FollowUp",
		"priority": "Medium",
		"status": "InProgress",
		"progressCount": 1,
		"updatedAt": "2025-01-13T10:00:00Z"
	},
	"message": "Task updated successfully"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors, invalid values
- **403 Forbidden:** User doesn't own the task
- **404 Not Found:** Task not found

---

### 5. Delete Task

**Endpoint:** `DELETE /api/WeeklyPlanTask/{id}`

**Authorization:** `Bearer {token}` (Salesman, SalesManager, SuperAdmin)

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": null,
	"message": "Task deleted successfully"
}
```

**Business Logic:**

- If task has progress records → **Soft delete** (status changed to "Cancelled")
- If task has no progress → **Hard delete** (permanently removed)

**Error Responses:**

- **403 Forbidden:** User doesn't own the task
- **404 Not Found:** Task not found

---

## 🔧 Create Weekly Plan with Tasks

**Endpoint:** `POST /api/WeeklyPlan`

**New Feature:** You can now create tasks when creating a weekly plan!

**Request Body:**

```json
{
	"weekStartDate": "2025-01-13T00:00:00Z",
	"weekEndDate": "2025-01-19T23:59:59Z",
	"title": "Week 3 - January 2025 Sales Plan",
	"description": "Focus on high priority clients",
	"tasks": [
		{
			"title": "Visit Ahmed Hospital",
			"taskType": "Visit",
			"clientId": 25,
			"clientStatus": "Old",
			"plannedDate": "2025-01-15T10:00:00Z",
			"priority": "High",
			"status": "Planned"
		},
		{
			"title": "Visit New Hospital",
			"taskType": "Visit",
			"clientStatus": "New",
			"clientName": "New Hospital Name",
			"placeType": "Hospital",
			"plannedDate": "2025-01-16T14:00:00Z",
			"priority": "Medium",
			"status": "Planned"
		}
	]
}
```

**Note:** When creating tasks during plan creation, you don't need to provide `weeklyPlanId` - it will be automatically set.

---

## 📝 Validation Rules

### Task Type

- **Valid Values:** `Visit`, `FollowUp`
- **Default:** `Visit`

### Priority

- **Valid Values:** `High`, `Medium`, `Low`
- **Default:** `Medium`

### Status

- **Valid Values:** `Planned`, `InProgress`, `Completed`, `Cancelled`
- **Default:** `Planned`

### Client Status

- **Valid Values:** `Old`, `New`
- **Required Fields:**
     - If `Old`: `clientId` is required
     - If `New`: `clientName` is required

### Client Classification

- **Valid Values:** `A`, `B`, `C`, `D`

---

## 🔒 Authorization Rules

1. **Salesmen:**

      - Can only create/update/delete tasks in their own weekly plans
      - Can only view tasks from their own weekly plans

2. **SalesManager/SuperAdmin:**
      - Can view all tasks
      - Cannot create/update/delete tasks (must be done by plan owner)

---

## 💡 Usage Examples

### Example 1: Create a Task for Existing Client

```http
POST /api/WeeklyPlanTask
Authorization: Bearer {token}
Content-Type: application/json

{
  "weeklyPlanId": 116,
  "title": "Follow up on X-Ray offer",
  "taskType": "FollowUp",
  "clientId": 25,
  "clientStatus": "Old",
  "plannedDate": "2025-01-18T11:00:00Z",
  "priority": "High",
  "purpose": "Check status of pending offer",
  "notes": "Client requested urgent response"
}
```

### Example 2: Update Task Status

```http
PUT /api/WeeklyPlanTask/200
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Completed",
  "notes": "Task completed successfully. Client satisfied."
}
```

### Example 3: Create Plan with Multiple Tasks

```http
POST /api/WeeklyPlan
Authorization: Bearer {token}
Content-Type: application/json

{
  "weekStartDate": "2025-01-20T00:00:00Z",
  "weekEndDate": "2025-01-26T23:59:59Z",
  "title": "Week 4 Plan",
  "tasks": [
    {
      "title": "Client Visit 1",
      "taskType": "Visit",
      "clientId": 25,
      "clientStatus": "Old",
      "plannedDate": "2025-01-21T10:00:00Z",
      "priority": "High"
    },
    {
      "title": "Client Visit 2",
      "taskType": "Visit",
      "clientStatus": "New",
      "clientName": "New Client Hospital",
      "placeType": "Hospital",
      "plannedDate": "2025-01-22T14:00:00Z",
      "priority": "Medium"
    }
  ]
}
```

---

## ✅ Best Practices

1. **Always validate client data** before creating tasks for new clients
2. **Use appropriate priorities** - High for urgent clients, Medium for regular, Low for follow-ups
3. **Update task status** as work progresses (Planned → InProgress → Completed)
4. **Add detailed notes** to track task progress and client interactions
5. **Set planned dates** within the weekly plan's date range for better organization

---

## 🐛 Error Handling

All endpoints return standardized error responses:

```json
{
	"success": false,
	"message": "Error message here",
	"errors": {
		"fieldName": ["Error details"]
	}
}
```

Common error codes:

- **400:** Bad Request (validation errors)
- **401:** Unauthorized (missing/invalid token)
- **403:** Forbidden (no permission)
- **404:** Not Found (resource doesn't exist)
- **500:** Internal Server Error (server-side error)

---

## 📚 Related Endpoints

- **Weekly Plans:** `/api/WeeklyPlan`
- **Task Progress:** `/api/TaskProgress`
- **Clients:** `/api/Client`

---

## 🔄 Changelog

**2025-01-13:**

- ✅ Added full CRUD operations for WeeklyPlanTask
- ✅ Added task creation during weekly plan creation
- ✅ Added comprehensive validation
- ✅ Added soft delete for tasks with progress
- ✅ Added detailed response DTOs with related data

---

**Created by:** Senior Developer  
**Last Updated:** 2025-01-13
