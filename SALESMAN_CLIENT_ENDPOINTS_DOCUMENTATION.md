# 📱 Salesman Client Endpoints Documentation

Complete documentation for all client-related endpoints accessible to Salesmen.

---

## 🔐 Authorization

**All endpoints require:**

- ✅ Authentication (JWT Bearer token)
- ✅ Salesman role (or SalesManager/SuperAdmin)

**Header:**

```
Authorization: Bearer {your_jwt_token}
Content-Type: application/json
```

---

## 📋 Available Endpoints

| Endpoint                       | Method | Purpose                            |
| ------------------------------ | ------ | ---------------------------------- |
| `/api/Client/search`           | GET    | 🔍 Search for clients              |
| `/api/Client/my-clients`       | GET    | 📋 Get my assigned clients         |
| `/api/Client/{id}`             | GET    | 👤 Get client details              |
| `/api/Client/{id}`             | PUT    | ✏️ Update client                   |
| `/api/Client`                  | POST   | ➕ Create new client               |
| `/api/Client/{id}/profile`     | GET    | 📊 Get client profile with history |
| `/api/Client/follow-up-needed` | GET    | 🔔 Get clients needing follow-up   |
| `/api/Client/statistics`       | GET    | 📈 Get my client statistics        |
| `/api/Client/find-or-create`   | POST   | 🔍 Find or create client           |

---

## 🔍 1. Search Clients

**Endpoint:** `GET /api/Client/search`

**Purpose:** Search for clients by name, email, specialization, location, or phone number. Results are filtered based on your permissions.

**Authorization:** ✅ Salesmen can access this endpoint

**Query Parameters:**

| Parameter    | Type   | Required | Description                                                          |
| ------------ | ------ | -------- | -------------------------------------------------------------------- |
| `searchTerm` | string | No       | Search query (searches name, email, specialization, location, phone) |
| `status`     | string | No       | Filter by status: `"Potential"`, `"Active"`, `"Inactive"`, `"Lost"`  |
| `priority`   | string | No       | Filter by priority: `"Low"`, `"Medium"`, `"High"`                    |
| `page`       | int    | No       | Page number (default: `1`)                                           |
| `pageSize`   | int    | No       | Items per page (default: `20`, max: `100`)                           |

**Request Example:**

```http
GET /api/Client/search?searchTerm=ahmed&status=Active&priority=High&page=1&pageSize=20
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Search Examples:**

1. **Search by name:**

      ```
      GET /api/Client/search?searchTerm=hospital
      ```

2. **Search with filters:**

      ```
      GET /api/Client/search?searchTerm=cairo&status=Active&priority=High
      ```

3. **Get all active clients (paginated):**

      ```
      GET /api/Client/search?status=Active&page=1&pageSize=50
      ```

4. **Search by email:**
      ```
      GET /api/Client/search?searchTerm=contact@hospital.com
      ```

**Success Response (200 OK):**

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
			"assignedTo": "Ahmed_Ashraf_Sales_001",
			"createdAt": "2025-01-15T10:00:00Z",
			"updatedAt": "2025-10-29T11:00:00Z"
		},
		{
			"id": 2,
			"name": "Cairo Medical Center",
			"type": "Clinic",
			"specialization": "General Medicine",
			"location": "Cairo",
			"phone": "+20987654321",
			"email": "info@cairomedical.com",
			"status": "Active",
			"priority": "Medium",
			"assignedTo": "Ahmed_Ashraf_Sales_001",
			"createdAt": "2025-02-01T08:00:00Z",
			"updatedAt": "2025-10-20T14:00:00Z"
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

     ```json
     {
     	"success": false,
     	"message": "غير مصرح لك",
     	"timestamp": "2025-10-29T12:00:00Z"
     }
     ```

- **500 Internal Server Error:** Server error

**Search Fields:**
The search looks through:

- ✅ Client name
- ✅ Email address
- ✅ Specialization
- ✅ Location
- ✅ Phone number

**Notes:**

- Search is **case-insensitive**
- Search uses **partial matching** (e.g., "ahm" matches "Ahmed")
- Results are filtered based on your access permissions
- Use pagination for large result sets

---

## 📋 2. Get My Clients

**Endpoint:** `GET /api/Client/my-clients`

**Purpose:** Get a paginated list of clients assigned to you.

**Authorization:** ✅ Salesmen can access this endpoint

**Query Parameters:**

| Parameter  | Type | Required | Default | Description                 |
| ---------- | ---- | -------- | ------- | --------------------------- |
| `page`     | int  | No       | `1`     | Page number                 |
| `pageSize` | int  | No       | `20`    | Items per page (max: `100`) |

**Request Example:**

```http
GET /api/Client/my-clients?page=1&pageSize=20
Authorization: Bearer {token}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "Ahmed Hospital",
			"type": "Hospital",
			"status": "Active",
			"priority": "High",
			"location": "Cairo"
		},
		{
			"id": 2,
			"name": "Cairo Medical Center",
			"type": "Clinic",
			"status": "Active",
			"priority": "Medium",
			"location": "Cairo"
		}
	],
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

## 👤 3. Get Client Details

**Endpoint:** `GET /api/Client/{id}`

**Purpose:** Get full details of a specific client.

**Authorization:** ✅ Salesmen can access this endpoint

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| `id`      | long | Yes      | Client ID   |

**Request Example:**

```http
GET /api/Client/123
Authorization: Bearer {token}
```

**Success Response (200 OK):**

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
		"website": "https://ahmedhospital.com",
		"address": "123 Main Street",
		"city": "Cairo",
		"governorate": "Cairo",
		"postalCode": "12345",
		"status": "Active",
		"priority": "High",
		"assignedTo": "Ahmed_Ashraf_Sales_001",
		"contactPerson": "Dr. Ahmed",
		"contactPersonPhone": "+20123456789",
		"contactPersonEmail": "ahmed@hospital.com",
		"contactPersonPosition": "Director",
		"notes": "High priority client",
		"potentialValue": 1000000,
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

---

## ✏️ 4. Update Client

**Endpoint:** `PUT /api/Client/{id}`

**Purpose:** Update client information. Only clients assigned to you can be updated.

**Authorization:** ✅ Salesmen can update their own clients

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| `id`      | long | Yes      | Client ID   |

**Request Body:** (All fields optional - only send what you want to update)

```json
{
	"name": "Updated Hospital Name",
	"phone": "+20987654321",
	"email": "newemail@hospital.com",
	"status": "Active",
	"priority": "Medium",
	"notes": "Updated notes",
	"contactPerson": "New Contact Person",
	"contactPersonPhone": "+20998887766"
}
```

**Request Example:**

```http
PUT /api/Client/123
Authorization: Bearer {token}
Content-Type: application/json

{
  "phone": "+20987654321",
  "status": "Active",
  "priority": "High"
}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"name": "Updated Hospital Name",
		"phone": "+20987654321",
		"status": "Active",
		"priority": "Medium",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors
- **403 Forbidden:** Client not assigned to you
- **404 Not Found:** Client doesn't exist

---

## ➕ 5. Create Client

**Endpoint:** `POST /api/Client`

**Purpose:** Create a new client record. The client will be automatically assigned to you.

**Authorization:** ✅ Salesmen can create clients

**Request Body:**

**⚠️ Important:** Only `name` is required. All other fields, including `type`, are optional.

**Required Fields:**

- `name` (string, max 200) - Client name

**Optional Fields:**

- `type` (string, max 50) - Client type: "Hospital", "Clinic", "Pharmacy", etc.
- `specialization` (string, max 100)
- `location` (string, max 100)
- `phone` (string, max 20)
- `email` (string)
- `website` (string)
- `address` (string, max 500)
- `city` (string, max 100)
- `governorate` (string, max 100)
- `postalCode` (string, max 20)
- `notes` (string, max 2000)
- `status` (string) - "Potential", "Active", "Inactive", "Lost" (default: "Active")
- `priority` (string) - "Low", "Medium", "High" (default: "Medium")
- `potentialValue` (decimal)
- `contactPerson` (string, max 200)
- `contactPersonPhone` (string, max 20)
- `contactPersonEmail` (string)
- `contactPersonPosition` (string, max 100)

**Request Example:**

```http
POST /api/Client
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "New Hospital",
  "type": "Hospital",
  "specialization": "Oncology",
  "location": "Alexandria",
  "phone": "+20198765432",
  "email": "hospital@example.com",
  "address": "123 Main St",
  "city": "Alexandria",
  "governorate": "Alexandria",
  "status": "Potential",
  "priority": "High",
  "contactPerson": "Dr. Mohamed",
  "contactPersonPhone": "+20198887766",
  "contactPersonEmail": "mohamed@hospital.com",
  "contactPersonPosition": "Director"
}
```

**Success Response (201 Created):**

```json
{
	"success": true,
	"data": {
		"id": 150,
		"name": "New Hospital",
		"type": "Hospital",
		"specialization": "Oncology",
		"location": "Alexandria",
		"phone": "+20198765432",
		"email": "hospital@example.com",
		"status": "Potential",
		"priority": "High",
		"assignedTo": "Ahmed_Ashraf_Sales_001",
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Validation errors
- **401 Unauthorized:** Not authenticated

---

## 📊 6. Get Client Profile (With History)

**Endpoint:** `GET /api/Client/{id}/profile`

**Purpose:** Get complete client profile with visit history, offers, and deals.

**Authorization:** ✅ Salesmen can access profiles of their assigned clients

**Path Parameters:**

| Parameter | Type | Required | Description |
| --------- | ---- | -------- | ----------- |
| `id`      | long | Yes      | Client ID   |

**Request Example:**

```http
GET /api/Client/123/profile
Authorization: Bearer {token}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"clientInfo": {
			"id": 123,
			"name": "Ahmed Hospital",
			"type": "Hospital",
			"status": "Active",
			"priority": "High"
		},
		"allVisits": [
			{
				"id": 50,
				"progressDate": "2025-01-15T10:30:00Z",
				"progressType": "Visit",
				"visitResult": "Interested",
				"nextStep": "NeedsOffer",
				"satisfactionRating": 4
			}
		],
		"allOffers": [
			{
				"id": 25,
				"totalAmount": 500000,
				"status": "Sent",
				"validUntil": "2025-02-15T00:00:00Z"
			}
		],
		"allDeals": [
			{
				"id": 10,
				"dealValue": 500000,
				"status": "Completed",
				"closedDate": "2025-02-20T00:00:00Z"
			}
		],
		"statistics": {
			"totalVisits": 5,
			"totalOffers": 2,
			"successfulDeals": 1,
			"totalRevenue": 500000
		}
	},
	"message": "Client profile retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **403 Forbidden:** Client not assigned to you
- **404 Not Found:** Client doesn't exist

---

## 🔔 7. Get Clients Needing Follow-up

**Endpoint:** `GET /api/Client/follow-up-needed`

**Purpose:** Get clients that need follow-up based on NextContactDate.

**Authorization:** ✅ Salesmen can access this endpoint

**Request Example:**

```http
GET /api/Client/follow-up-needed
Authorization: Bearer {token}
```

**Success Response (200 OK):**

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

## 📈 8. Get Client Statistics

**Endpoint:** `GET /api/Client/statistics`

**Purpose:** Get statistics about your clients (visits, offers, deals).

**Authorization:** ✅ Salesmen can access this endpoint

**Request Example:**

```http
GET /api/Client/statistics
Authorization: Bearer {token}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"totalClients": 25,
		"activeClients": 18,
		"potentialClients": 5,
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

## 🔍 9. Find or Create Client

**Endpoint:** `POST /api/Client/find-or-create`

**Purpose:** Search for a client by name and type. If found, return it. If not found, create it.

**Authorization:** ✅ Salesmen can access this endpoint

**Request Body:**

**Required Fields:**

- `name` (string) - Client name
- `type` (string) - Client type

**Optional Fields:**

- `specialization` (string)
- `location` (string)
- `phone` (string)

**Request Example:**

```http
POST /api/Client/find-or-create
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Ahmed Hospital",
  "type": "Hospital",
  "location": "Cairo",
  "phone": "+20123456789"
}
```

**Success Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"name": "Ahmed Hospital",
		"type": "Hospital",
		"wasCreated": false
	},
	"message": null,
	"timestamp": "2025-10-29T12:00:00Z"
}
```

If client was created, `wasCreated` will be `true`.

---

## 💡 Usage Examples

### Example 1: Search for Clients by Name

```bash
curl -X GET "http://localhost:5117/api/Client/search?searchTerm=hospital" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Example 2: Get My Clients with Pagination

```bash
curl -X GET "http://localhost:5117/api/Client/my-clients?page=1&pageSize=50" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Example 3: Create a New Client

```bash
curl -X POST "http://localhost:5117/api/Client" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Hospital",
    "type": "Hospital",
    "location": "Cairo",
    "phone": "+20123456789",
    "priority": "High"
  }'
```

### Example 4: Get Client Profile

```bash
curl -X GET "http://localhost:5117/api/Client/123/profile" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 🔒 Authorization Rules

1. **Search Clients:** ✅ Salesmen can search all clients
2. **My Clients:** ✅ Salesmen see only clients assigned to them
3. **Get Client:** ✅ Salesmen can view any client details
4. **Update Client:** ✅ Salesmen can only update clients assigned to them
5. **Create Client:** ✅ Salesmen can create clients (auto-assigned to them)
6. **Client Profile:** ✅ Salesmen can view profiles of their assigned clients
7. **Follow-up Needed:** ✅ Salesmen see only their clients needing follow-up
8. **Statistics:** ✅ Salesmen see statistics for their clients only

---

## ⚠️ Important Notes

1. **Search Results:** Search results are filtered based on your access permissions
2. **Client Assignment:** New clients are automatically assigned to you
3. **Update Restrictions:** You can only update clients assigned to you
4. **Profile Access:** You can view full profiles only for clients assigned to you
5. **Pagination:** Always use pagination for large datasets (max pageSize: 100)

---

## 📚 Related Endpoints

- `POST /api/WeeklyPlanTask` - Create task with client
- `POST /api/TaskProgress` - Record visit/progress for client
- `GET /api/TaskProgress/by-client/{clientId}` - Get client visit history
- `POST /api/OfferRequest` - Create offer request for client
- `GET /api/Deal/by-client/{clientId}` - Get client's deals
