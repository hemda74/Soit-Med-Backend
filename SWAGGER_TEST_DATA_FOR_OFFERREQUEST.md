# Swagger Test Data for GET /api/OfferRequest

## Endpoint Information

**Endpoint**: `GET /api/offerrequest`  
**Authorization**: Salesman, SalesManager, SalesSupport, SuperAdmin  
**Base URL**: `http://localhost:5117`

## Test Token (SalesSupport)

```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNzdXBwb3J0QHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fRW5naW5lZXJpbmdfMDAxIiwianRpIjoiNzRmZTkwZjgtZjdkYi00ZjE3LTk0ZGItOTk3MjdhN2RhZmY4IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNTdXBwb3J0IiwiZXhwIjoxOTE4OTEwNjc5LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.sDouicyU-bdLpgF2kmQ39Sk0sSm6Fs8zx2nvGFh_tEE
```

## Sample Test Scenarios

### Test 1: Get All Offer Requests (No Filters)

```
GET /api/offerrequest
Authorization: Bearer {token}
```

**Expected Response**: List of all offer requests based on user role

---

### Test 2: Filter by Status

```
GET /api/offerrequest?status=Requested
Authorization: Bearer {token}
```

**Expected Query Parameters**:

- status: Requested
- requestedBy: (leave empty)

**Expected Response**: Only offer requests with status "Requested"

**Available Status Values**:

- Requested
- InProgress
- Completed
- Cancelled

---

### Test 3: Filter by Requested By (Salesman ID)

```
GET /api/offerrequest?requestedBy=Ahmed_Ashraf_Sales_001
Authorization: Bearer {token}
```

**Expected Response**: Only offer requests created by this salesman

---

### Test 4: Combined Filters

```
GET /api/offerrequest?status=InProgress&requestedBy=Ahmed_Ashraf_Sales_001
Authorization: Bearer {token}
```

**Expected Response**: Offer requests matching both criteria

---

## Expected Response Format

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
			"assignedTo": "Ahmed_Hemdan_Engineering_001",
			"assignedToName": "Ahmed Hemdan",
			"requestedProducts": "MRI Scanner Model X1",
			"specialNotes": "Urgent request for Q2 delivery. Client has approved budget.",
			"status": "InProgress",
			"createdAt": "2025-10-26T10:00:00",
			"updatedAt": "2025-10-26T10:00:00",
			"offerRequestId": null,
			"taskProgressId": null
		},
		{
			"id": 2,
			"clientId": 25,
			"clientName": "Al-Azhar University Hospital",
			"requestedBy": "Ahmed_Ashraf_Sales_001",
			"requestedByName": "Ahmed Ashraf",
			"assignedTo": "Ahmed_Hemdan_Engineering_001",
			"assignedToName": "Ahmed Hemdan",
			"requestedProducts": "Cardiac Monitoring System - Advanced Package",
			"specialNotes": "Client needs 20 units for cardiology department.",
			"status": "InProgress",
			"createdAt": "2025-10-26T10:00:00",
			"updatedAt": "2025-10-26T10:00:00",
			"offerRequestId": null,
			"taskProgressId": null
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-26T11:30:00Z"
}
```

---

## Sample Data in Database

The following offer requests should exist:

| ID  | Client ID | Requested By           | Status     | Assigned To                  | Products                      |
| --- | --------- | ---------------------- | ---------- | ---------------------------- | ----------------------------- |
| 1   | 24        | Ahmed_Ashraf_Sales_001 | InProgress | Ahmed_Hemdan_Engineering_001 | MRI Scanner Model X1          |
| 2   | 25        | Ahmed_Ashraf_Sales_001 | Requested  | null                         | Cardiac Monitoring System     |
| 3   | 26        | Ahmed_Ashraf_Sales_001 | Requested  | null                         | Orthopedic Surgical Equipment |

---

## How to Test in Swagger

### Step 1: Add Authorization

1. Click the **"Authorize"** button at the top of Swagger UI
2. Enter the Bearer token (without the word "Bearer"):
      ```
      eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic2FsZXNzdXBwb3J0QHNvaXRtZWQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiJBaG1lZF9IZW1kYW5fRW5naW5lZXJpbmdfMDAxIiwianRpIjoiNzRmZTkwZjgtZjdkYi00ZjE3LTk0ZGItOTk3MjdhN2RhZmY4IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiU2FsZXNTdXBwb3J0IiwiZXhwIjoxOTE4OTEwNjc5LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjUxMTciLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDAifQ.sDouicyU-bdLpgF2kmQ39Sk0sSm6Fs8zx2nvGFh_tEE
      ```
3. Click **"Authorize"** then **"Close"**

### Step 2: Navigate to Endpoint

1. Find **GET /api/OfferRequest** endpoint
2. Click **"Try it out"**

### Step 3: Test Scenarios

#### Test A: No Parameters

- Leave both fields empty
- Click **"Execute"**
- Expected: Returns all offer requests assigned to SalesSupport

#### Test B: Filter by Status

- status: `InProgress`
- requestedBy: (leave empty)
- Click **"Execute"**
- Expected: Returns only InProgress requests

#### Test C: Filter by Salesman

- status: (leave empty)
- requestedBy: `Ahmed_Ashraf_Sales_001`
- Click **"Execute"**
- Expected: Returns only requests by this salesman

#### Test D: Combined Filters

- status: `Requested`
- requestedBy: `Ahmed_Ashraf_Sales_001`
- Click **"Execute"**
- Expected: Returns matching requests

---

## Role-Based Access

### SalesSupport (Your Current Token)

- Can see only offer requests **assigned to them**
- Filtered by: `AssignedTo = CurrentUser`

### Salesman

- Can see only their **own offer requests**
- Filtered by: `RequestedBy = CurrentUser`

### SalesManager & SuperAdmin

- Can see **all offer requests**
- No filtering applied

---

## Expected Sample Response

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
			"assignedTo": "Ahmed_Hemdan_Engineering_001",
			"assignedToName": "Ahmed Hemdan",
			"requestedProducts": "Complete MRI scanner package including installation, training, and 2-year warranty",
			"specialNotes": "Urgent request for Q2 delivery. Client has approved budget.",
			"status": "InProgress",
			"createdAt": "2025-10-26T10:00:00Z",
			"updatedAt": "2025-10-26T10:00:00Z"
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-26T11:30:00Z"
}
```

---

## Common Error Responses

### 401 Unauthorized

- **Cause**: Missing or invalid token
- **Solution**: Add valid Bearer token in Authorization header

### 403 Forbidden

- **Cause**: User role not authorized for this endpoint
- **Solution**: Use Salesman, SalesManager, SalesSupport, or SuperAdmin role

### 500 Internal Server Error

- **Cause**: Server/database error
- **Solution**: Check server logs for details

---

## Quick Test Checklist

- [ ] Authorized with valid Bearer token
- [ ] Test without filters - should return assigned requests
- [ ] Test with status=Requested filter
- [ ] Test with status=InProgress filter
- [ ] Test with requestedBy parameter
- [ ] Verify response structure matches expected format
- [ ] Verify only authorized data is returned
