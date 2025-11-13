# 🛠️ Sales Support - Offer Requests CRUD Documentation

## 🎯 Overview

This documentation covers **all CRUD operations** for Offer Requests that SalesSupport members can perform. Salesmen create offer requests, and SalesSupport manages them through the complete workflow.

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

## 📋 Offer Request CRUD Endpoints

| #   | Endpoint                                 | Method | Purpose                               |
| --- | ---------------------------------------- | ------ | ------------------------------------- |
| 1   | `/api/OfferRequest`                      | GET    | Get all offer requests (with filters) |
| 2   | `/api/OfferRequest/{id}`                 | GET    | Get offer request details             |
| 3   | `/api/OfferRequest/{id}/assign`          | PUT    | Assign request to support             |
| 4   | `/api/OfferRequest/{id}/status`          | PUT    | Update request status                 |
| 5   | `/api/OfferRequest/assigned/{supportId}` | GET    | Get requests assigned to support      |

**Note:** SalesSupport **cannot create** offer requests. Only Salesmen can create them. SalesSupport **can only** view, assign, update status, and manage them.

---

## 📖 Section 1: READ Operations

### 1.1 Get All Offer Requests

**Endpoint:** `GET /api/OfferRequest`

**Purpose:** Get all offer requests in the system (SalesSupport can see all requests, not just assigned ones)

**Query Parameters:**

- `status` (string, optional) - Filter by status: `"Pending"`, `"Assigned"`, `"InProgress"`, `"Completed"`, `"Cancelled"`
- `requestedBy` (string, optional) - Filter by salesman ID

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
			"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device Model ABC",
			"specialNotes": "Client needs delivery within 2 weeks, urgent requirement",
			"requestDate": "2025-10-29T10:00:00Z",
			"status": "Pending",
			"assignedTo": null,
			"assignedToName": null,
			"createdOfferId": null,
			"taskProgressId": 500,
			"completedAt": null,
			"completionNotes": null
		},
		{
			"id": 26,
			"requestedBy": "salesman-124",
			"requestedByName": "Mohamed Ali",
			"clientId": 124,
			"clientName": "Cairo Medical Center",
			"requestedProducts": "MRI Machine",
			"specialNotes": null,
			"requestDate": "2025-10-29T11:00:00Z",
			"status": "Assigned",
			"assignedTo": "support-789",
			"assignedToName": "Support User",
			"createdOfferId": null,
			"taskProgressId": 501,
			"completedAt": null,
			"completionNotes": null
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Response Fields:**

- `id` - Offer request ID
- `requestedBy` - Salesman user ID
- `requestedByName` - Salesman full name
- `clientId` - Client ID
- `clientName` - Client name
- `requestedProducts` - Products/services requested
- `specialNotes` - Special instructions from salesman
- `requestDate` - When request was created
- `status` - Current status
- `assignedTo` - Support user ID (null if not assigned)
- `assignedToName` - Support user name
- `createdOfferId` - ID of offer created from this request (null if not created yet)
- `taskProgressId` - Related task progress ID
- `completedAt` - Completion date (if completed)
- `completionNotes` - Completion notes

**Error Responses:**

- **401 Unauthorized:** Missing or invalid token
- **500 Internal Server Error:** Server error

---

### 1.2 Get Offer Request Details

**Endpoint:** `GET /api/OfferRequest/{id}`

**Purpose:** Get detailed information about a specific offer request

**Path Parameters:**

- `id` (long) - Offer request ID

**Request Example:**

```http
GET /api/OfferRequest/25
Authorization: Bearer {token}
```

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
		"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device Model ABC",
		"specialNotes": "Client needs delivery within 2 weeks, urgent requirement",
		"requestDate": "2025-10-29T10:00:00Z",
		"status": "Pending",
		"assignedTo": null,
		"assignedToName": null,
		"createdOfferId": null,
		"taskProgressId": 500,
		"completedAt": null,
		"completionNotes": null
	},
	"message": "Offer request retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Request doesn't exist
- **401 Unauthorized:** Missing or invalid token

---

### 1.3 Get Requests Assigned to Support

**Endpoint:** `GET /api/OfferRequest/assigned/{supportId}`

**Purpose:** Get all requests assigned to a specific support member (usually yourself)

**Path Parameters:**

- `supportId` (string) - SalesSupport user ID

**Query Parameters:**

- `status` (string, optional) - Filter by status: `"Pending"`, `"Assigned"`, `"InProgress"`, `"Completed"`, `"Cancelled"`

**Request Example:**

```http
GET /api/OfferRequest/assigned/support-789?status=InProgress
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
			"requestedProducts": "X-Ray Machine Model XYZ",
			"specialNotes": "Client needs delivery within 2 weeks",
			"requestDate": "2025-10-29T10:00:00Z",
			"status": "InProgress",
			"assignedTo": "support-789",
			"assignedToName": "Support User",
			"createdOfferId": null
		}
	],
	"message": "Assigned offer requests retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Note:** Typically, you would use your own user ID to get requests assigned to you.

**Error Responses:**

- **401 Unauthorized:** Missing or invalid token
- **500 Internal Server Error:** Server error

---

## ✏️ Section 2: UPDATE Operations

### 2.1 Assign Offer Request

**Endpoint:** `PUT /api/OfferRequest/{id}/assign`

**Purpose:** Assign an offer request to yourself or another SalesSupport member

**Path Parameters:**

- `id` (long) - Offer request ID

**Request Body:**

```json
{
	"assignedTo": "support-789"
}
```

**Required Fields:**

- `assignedTo` (string) - SalesSupport user ID to assign the request to

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "Assigned",
		"assignedTo": "support-789",
		"assignedToName": "Support User",
		"requestDate": "2025-10-29T10:00:00Z",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"clientName": "Ahmed Hospital"
	},
	"message": "Offer request assigned successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Important Notes:**

- **Status automatically changes** from `"Pending"` to `"Assigned"` when you assign a request
- You can assign requests to **yourself** or **another SalesSupport member**
- Once assigned, the status becomes `"Assigned"` and can be changed to `"InProgress"` when you start working

**Error Responses:**

- **400 Bad Request:**
     - Invalid support user ID
     - Support user doesn't exist or doesn't have SalesSupport role
     - Request already assigned
     - Validation errors
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** You don't have permission to assign this request
- **404 Not Found:** Request doesn't exist

---

### 2.2 Update Offer Request Status

**Endpoint:** `PUT /api/OfferRequest/{id}/status`

**Purpose:** Update the status of an offer request as you work on it

**Path Parameters:**

- `id` (long) - Offer request ID

**Request Body:**

```json
{
	"status": "InProgress",
	"notes": "Gathering equipment specifications and pricing information"
}
```

**Required Fields:**

- `status` (string) - New status value

**Optional Fields:**

- `notes` (string, max 1000) - Status update notes

**Valid Status Values:**

- `"Pending"` - Just created by salesman (not assigned yet)
- `"Assigned"` - Assigned to SalesSupport (set automatically on assignment)
- `"InProgress"` - SalesSupport is actively working on it
- `"Completed"` - Offer has been created and sent (set automatically or manually)
- `"Cancelled"` - Request cancelled

**Status Flow:**

```
Pending → Assigned → InProgress → Completed
  ↓                        ↓
Cancelled              Cancelled
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "InProgress",
		"notes": "Gathering equipment specifications and pricing information",
		"assignedTo": "support-789",
		"assignedToName": "Support User",
		"requestDate": "2025-10-29T10:00:00Z",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"clientName": "Ahmed Hospital",
		"updatedAt": "2025-10-29T12:30:00Z"
	},
	"message": "Offer request status updated successfully",
	"timestamp": "2025-10-29T12:30:00Z"
}
```

**Common Status Update Scenarios:**

1. **Start Working:**

      ```json
      {
      	"status": "InProgress",
      	"notes": "Started gathering equipment specifications"
      }
      ```

2. **Update Progress:**

      ```json
      {
      	"status": "InProgress",
      	"notes": "Equipment specifications collected, preparing pricing"
      }
      ```

3. **Mark Completed:**

      ```json
      {
      	"status": "Completed",
      	"notes": "Offer created and sent to salesman"
      }
      ```

4. **Cancel Request:**
      ```json
      {
      	"status": "Cancelled",
      	"notes": "Client cancelled request"
      }
      ```

**Error Responses:**

- **400 Bad Request:**
     - Invalid status value
     - Status transition not allowed (e.g., trying to go from Completed back to InProgress)
     - Request not assigned to you (for status updates)
     - Validation errors
- **401 Unauthorized:** Missing or invalid token
- **403 Forbidden:** You don't have permission to update this request
- **404 Not Found:** Request doesn't exist

---

## 📊 Section 3: Status Values Reference

### Offer Request Statuses:

| Status       | Description                            | Who Can Set             |
| ------------ | -------------------------------------- | ----------------------- |
| `Pending`    | Just created by salesman, not assigned | Automatic (on creation) |
| `Assigned`   | Assigned to SalesSupport               | Automatic (on assign)   |
| `InProgress` | SalesSupport is actively working on it | SalesSupport/Manager    |
| `Completed`  | Offer created and sent                 | SalesSupport/Manager    |
| `Cancelled`  | Request cancelled                      | SalesSupport/Manager    |

### Status Transition Rules:

- `Pending` → `Assigned` (automatic when assigned)
- `Assigned` → `InProgress` (when you start working)
- `InProgress` → `Completed` (when offer is created)
- `InProgress` → `Cancelled` (if cancelled)
- `Assigned` → `Cancelled` (if cancelled before starting)
- **Cannot go backwards:** Once `Completed`, cannot go back to `InProgress`

---

## 🔄 Section 4: Common Workflows

### Workflow 1: Assign and Work on Request

1. **Get All Pending Requests** → `GET /api/OfferRequest?status=Pending`
2. **View Request Details** → `GET /api/OfferRequest/{id}`
3. **Assign to Yourself** → `PUT /api/OfferRequest/{id}/assign` (with your user ID)
4. **Update Status to InProgress** → `PUT /api/OfferRequest/{id}/status` (with `"status": "InProgress"`)
5. **Create Offer** → `POST /api/Offer` (using offer endpoints)
6. **Update Status to Completed** → `PUT /api/OfferRequest/{id}/status` (with `"status": "Completed"`)

### Workflow 2: Track My Assigned Requests

1. **Get My Assigned Requests** → `GET /api/OfferRequest/assigned/{mySupportId}`
2. **Filter by Status** → `GET /api/OfferRequest/assigned/{mySupportId}?status=InProgress`
3. **Update Status as You Work** → `PUT /api/OfferRequest/{id}/status`

### Workflow 3: View Request History

1. **Get All Requests** → `GET /api/OfferRequest` (no filters)
2. **Get Specific Request** → `GET /api/OfferRequest/{id}`
3. **Check Created Offer** → Use `createdOfferId` to fetch offer details

---

## ⚠️ Section 5: Error Handling

### Common Error Scenarios:

#### 1. Trying to Assign Already Assigned Request

**Request:**

```http
PUT /api/OfferRequest/25/assign
{
  "assignedTo": "support-789"
}
```

**Response (400):**

```json
{
	"success": false,
	"message": "Request already assigned",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

#### 2. Invalid Status Transition

**Request:**

```http
PUT /api/OfferRequest/25/status
{
  "status": "Pending"
}
```

_(Trying to go from Completed back to Pending)_

**Response (400):**

```json
{
	"success": false,
	"message": "Invalid status transition",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

#### 3. Request Not Found

**Request:**

```http
GET /api/OfferRequest/99999
```

**Response (404):**

```json
{
	"success": false,
	"message": "Offer request not found",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

## 💡 Section 6: Best Practices

### For SalesSupport Members:

1. **Always assign requests** before starting work

      - Use `PUT /api/OfferRequest/{id}/assign` to assign to yourself
      - Status automatically becomes `"Assigned"`

2. **Update status frequently** to keep track of progress

      - Set to `"InProgress"` when you start working
      - Add notes with each status update for transparency

3. **Use filters effectively**

      - `?status=Pending` - See new requests
      - `?status=InProgress` - See active work
      - `?status=Completed` - See completed requests

4. **Link offers to requests**

      - When creating an offer, use the `offerRequestId` field
      - The request's `createdOfferId` will be automatically set

5. **Mark as Completed** when offer is sent
      - Update status to `"Completed"` after sending offer to salesman
      - Add completion notes if needed

---

## 📝 Section 7: Response Structure Details

### Offer Request Response DTO:

```typescript
interface OfferRequestResponse {
	id: number;
	requestedBy: string; // Salesman user ID
	requestedByName: string; // Salesman full name
	clientId: number;
	clientName: string;
	requestedProducts: string; // Products/services requested
	specialNotes?: string; // Special instructions
	requestDate: string; // ISO 8601 date
	status: string; // Current status
	assignedTo?: string; // Support user ID (null if not assigned)
	assignedToName?: string; // Support user name
	createdOfferId?: number; // ID of created offer (null if not created)
	taskProgressId?: number; // Related task progress ID
	completedAt?: string; // Completion date
	completionNotes?: string; // Completion notes
}
```

---

## 🔗 Section 8: Integration with Offers

### Creating Offer from Request:

Once you have an offer request:

1. **Get request details** → `GET /api/OfferRequest/{id}`
2. **Create offer** → `POST /api/Offer` (include `offerRequestId` in request body)
3. **Request automatically linked** - The request's `createdOfferId` field is set
4. **Update request status** → `PUT /api/OfferRequest/{id}/status` with `"Completed"`

**Example Create Offer Request:**

```json
{
	"offerRequestId": 25,
	"clientId": 123,
	"assignedTo": "salesman-123",
	"products": "X-Ray Machine Model XYZ",
	"totalAmount": 50000.0,
	"validUntil": "2025-11-15T23:59:59Z"
}
```

---

## 🚀 Quick Reference

### Get Pending Requests:

```http
GET /api/OfferRequest?status=Pending
```

### Assign to Myself:

```http
PUT /api/OfferRequest/25/assign
{
  "assignedTo": "my-support-id"
}
```

### Start Working:

```http
PUT /api/OfferRequest/25/status
{
  "status": "InProgress",
  "notes": "Started working on this request"
}
```

### Mark Completed:

```http
PUT /api/OfferRequest/25/status
{
  "status": "Completed",
  "notes": "Offer created and sent"
}
```

### Get My Assigned Requests:

```http
GET /api/OfferRequest/assigned/my-support-id?status=InProgress
```

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29
