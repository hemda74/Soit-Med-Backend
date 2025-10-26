# SalesSupport Offer APIs - Complete Guide

## Available Endpoints for SalesSupport

### 🔹 Your Created Offers

**Endpoint**: `GET /api/offer/my-offers`  
**Role Required**: SalesSupport, SalesManager, SuperAdmin  
**Returns**: Offers created by YOU

#### Filters Available

- ✅ **status** (Draft, Sent, Accepted, Rejected)
- ✅ **startDate** (DateTime) - Filter from this date
- ✅ **endDate** (DateTime) - Filter until this date

#### Example Requests

```http
# Get all your offers
GET /api/offer/my-offers

# Get only Draft offers
GET /api/offer/my-offers?status=Draft

# Get offers from last 30 days
GET /api/offer/my-offers?startDate=2025-09-26&endDate=2025-10-26

# Get Draft offers in date range
GET /api/offer/my-offers?status=Draft&startDate=2025-10-01&endDate=2025-10-26
```

#### Expected Response

```json
{
	"success": true,
	"data": [
		{
			"id": 3,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"createdBy": "Ahmed_Hemdan_Engineering_001",
			"products": "Complete MRI scanner package",
			"totalAmount": 2500000.0,
			"status": "Draft",
			"validUntil": "2025-11-25T14:17:39"
		}
	]
}
```

---

### 🔹 All Offers (System-Wide)

**Endpoint**: `GET /api/offer`  
**Role Required**: SalesSupport, SalesManager, SuperAdmin  
**Returns**: ALL offers in system (all users)

#### Filters Available

- ✅ **status** (Draft, Sent, Accepted, Rejected)
- ✅ **clientId** (filter by specific client)
- ✅ **startDate** (DateTime) - Filter from this date
- ✅ **endDate** (DateTime) - Filter until this date

#### Example Requests

```http
# Get all offers
GET /api/offer

# Get all Draft offers
GET /api/offer?status=Draft

# Get offers for specific client
GET /api/offer?clientId=24

# Get offers in date range
GET /api/offer?startDate=2025-09-26&endDate=2025-10-26

# Combined filters
GET /api/offer?status=Sent&clientId=24&startDate=2025-10-01
```

---

### 🔹 Assigned Offer Requests

**Endpoint**: `GET /api/offerrequest`  
**Role Required**: SalesSupport (filtered to assigned requests)  
**Returns**: Offer requests assigned to YOU

#### Filters Available

- ✅ **status** (Requested, InProgress, Completed, Cancelled)
- ✅ **requestedBy** (filter by salesman ID)

#### Example Requests

```http
# Get all your assigned requests
GET /api/offerrequest

# Get only InProgress requests
GET /api/offerrequest?status=InProgress

# Get requests from specific salesman
GET /api/offerrequest?requestedBy=Ahmed_Ashraf_Sales_001

# Combined filter
GET /api/offerrequest?status=Requested&requestedBy=Ahmed_Ashraf_Sales_001
```

#### Expected Response

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"requestedBy": "Ahmed_Ashraf_Sales_001",
			"requestedProducts": "MRI Scanner Model X1",
			"status": "InProgress",
			"assignedTo": "Ahmed_Hemdan_Engineering_001"
		}
	]
}
```

---

## Swagger Testing Guide

### Step 1: Authorize

1. Click **"Authorize"** button
2. Enter Bearer token (without "Bearer" word):

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNzdXBwb3J0QHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fRW5naW5lZXJpbmdfMDAxIiwianRpIjoiNzRmZTkwZjgtZjdkYi00ZjE3LTk0ZGItOTk3MjdhN2RhZmY4IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNTdXBwb3J0IiwiZXhwIjoxOTE4OTEwNjc5LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.sDouicyU-bdLpgF2kmQ39Sk0sSm6Fs8zx2nvGFh_tEE
```

### Step 2: Test Each Endpoint

#### Test A: See Your Created Offers

- Navigate to: `GET /api/offer/my-offers`
- Click **"Try it out"**
- Leave status empty OR enter: `Draft`
- Click **"Execute"**

#### Test B: See All Offers

- Navigate to: `GET /api/offer`
- Click **"Try it out"**
- status: `Sent` (or leave empty)
- clientId: `24` (or leave empty)
- Click **"Execute"**

#### Test C: See Assigned Requests

- Navigate to: `GET /api/offerrequest`
- Click **"Try it out"**
- status: `InProgress` (or leave empty)
- requestedBy: `Ahmed_Ashraf_Sales_001` (or leave empty)
- Click **"Execute"**

---

## Complete API Summary for SalesSupport

| Endpoint                          | Method | View                        | Filters             | Notes                          |
| --------------------------------- | ------ | --------------------------- | ------------------- | ------------------------------ |
| `/api/offer/my-offers`            | GET    | Your created offers         | status              | ✅ Only your offers            |
| `/api/offer`                      | GET    | All system offers           | status, clientId    | ✅ All users' offers           |
| `/api/offer/{id}`                 | GET    | Single offer                | -                   | ✅ Get details                 |
| `/api/offerrequest`               | GET    | Assigned requests           | status, requestedBy | ✅ Only assigned to you        |
| `/api/offerrequest/assigned/{id}` | GET    | Specific user's assignments | status              | ✅ Other support's assignments |

---

## Quick Reference: Status Values

### Offer Statuses

- **Draft** - Offer created, not sent yet
- **Sent** - Offer sent to client
- **Accepted** - Client accepted the offer
- **Rejected** - Client rejected the offer

### Request Statuses

- **Requested** - Just created by salesman
- **InProgress** - Assigned and being worked on
- **Completed** - Offer created from request
- **Cancelled** - Request cancelled

---

## Common Use Cases

### Use Case 1: Check Your Draft Offers

```
GET /api/offer/my-offers?status=Draft
```

**When**: Before sending offers to clients

### Use Case 2: See All Accepted Offers

```
GET /api/offer?status=Accepted
```

**When**: Review successful offers across system

### Use Case 3: Get Your Pending Assignments

```
GET /api/offerrequest?status=InProgress
```

**When**: Check your work queue

### Use Case 4: Filter by Client

```
GET /api/offer?clientId=24
```

**When**: See all offers for specific hospital

---

## Currently Missing Filters

❌ **Not Yet Implemented:**

- Salesman filter for offers
- Pagination support
- Priority filtering

✅ **Just Implemented:**

- Date range filtering (startDate, endDate) - **NOW AVAILABLE!**

**Need more filters?** I can add them quickly! Just let me know.
