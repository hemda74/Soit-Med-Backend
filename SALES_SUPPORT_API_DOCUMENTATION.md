# 🛠️ Sales Support API Documentation

### GET /api/Offer/salesmen

Returns all salesmen for assignment. Supports optional search.

- Roles: `SalesSupport, SalesManager, SuperAdmin`
- Query params:
     - `q` (optional): case-insensitive substring to match against `firstName`, `lastName`, or `userName`.

Request examples:

```http
GET /api/Offer/salesmen HTTP/1.1
Authorization: Bearer <token>
```

```http
GET /api/Offer/salesmen?q=ahmed HTTP/1.1
Authorization: Bearer <token>
```

Response 200:

```json
{
	"success": true,
	"data": [
		{
			"id": "string",
			"firstName": "string",
			"lastName": "string",
			"email": "string",
			"phoneNumber": "string",
			"userName": "string",
			"isActive": true
		}
	]
}
```

Notes:

- Use this list to populate assignment dropdowns and implement live search with `q`.
- Returned fields are minimal for selection UIs; fetch detailed user data elsewhere if needed.

## 🎯 User Stories

### As a Sales Support member, I want to:

1. **View offer requests** assigned to me
2. **Get request details** to understand client needs
3. **Create offers** based on requests
4. **Add equipment** to offers with specifications
5. **Upload equipment images** for visual reference
6. **Add payment terms** and conditions
7. **Create installment plans** for clients
8. **Send offers** to salesmen
9. **Update offer request status** as I work
10. **View all offers** I created
11. **Export offers as PDF** for printing

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

| #   | Endpoint                                                    | Method | Purpose                                     |
| --- | ----------------------------------------------------------- | ------ | ------------------------------------------- |
| 1   | `/api/OfferRequest`                                         | GET    | Get all offer requests                      |
| 2   | `/api/OfferRequest/{id}`                                    | GET    | Get offer request details                   |
| 3   | `/api/OfferRequest/{id}/assign`                             | PUT    | Assign request to myself or another support |
| 4   | `/api/OfferRequest/{id}/status`                             | PUT    | Update request status                       |
| 5   | `/api/OfferRequest/assigned/{supportId}`                    | GET    | Get requests assigned to support            |
| 6   | `/api/Offer`                                                | GET    | Get all offers                              |
| 7   | `/api/Offer/my-offers`                                      | GET    | Get offers I created                        |
| 8   | `/api/Offer/{id}`                                           | GET    | Get offer details                           |
| 9   | `/api/Offer`                                                | POST   | Create new offer                            |
| 10  | `/api/Offer/{offerId}/equipment`                            | POST   | Add equipment to offer                      |
| 11  | `/api/Offer/{offerId}/equipment`                            | GET    | Get equipment list                          |
| 12  | `/api/Offer/{offerId}/equipment/{equipmentId}`              | DELETE | Delete equipment                            |
| 13  | `/api/Offer/{offerId}/equipment/{equipmentId}/upload-image` | POST   | Upload equipment image                      |
| 14  | `/api/Offer/{offerId}/terms`                                | POST   | Add/update offer terms                      |
| 15  | `/api/Offer/{offerId}/installments`                         | POST   | Create installment plan                     |
| 16  | `/api/Offer/{offerId}/export-pdf`                           | GET    | Export offer as PDF                         |
| 17  | `/api/Offer/{offerId}/send-to-salesman`                     | POST   | Send offer to salesman                      |
| 18  | `/api/Offer/request/{requestId}/details`                    | GET    | Get request details for creating offer      |

---

## 🔍 Detailed Endpoint Documentation

### 1. Get All Offer Requests

**Endpoint:** `GET /api/OfferRequest`

**Purpose:** Get all offer requests (filtered by status or requester)

**Query Parameters:**

- `status` (string, optional) - "Pending", "Assigned", "InProgress", "Completed", "Cancelled"
- `requestedBy` (string, optional) - Filter by salesman ID

**Request Example:**

```http
GET /api/OfferRequest?status=Assigned
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
			"requestedProducts": "X-Ray Machine Model XYZ, Ultrasound Device",
			"status": "Assigned",
			"assignedTo": "support-789",
			"assignedToName": "Support User",
			"requestDate": "2025-10-29T10:00:00Z"
		}
	],
	"message": "Offer requests retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 2. Get Offer Request Details

**Endpoint:** `GET /api/OfferRequest/{id}`

**Purpose:** Get detailed information about an offer request

**Path Parameters:**

- `id` (long) - Offer request ID

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
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Client needs fast delivery within 2 weeks",
		"status": "Assigned",
		"assignedTo": "support-789",
		"assignedToName": "Support User",
		"createdOfferId": null,
		"requestDate": "2025-10-29T10:00:00Z"
	},
	"message": "Offer request retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Request doesn't exist

---

### 3. Assign Offer Request

**Endpoint:** `PUT /api/OfferRequest/{id}/assign`

**Purpose:** Assign an offer request to yourself or another support member

**Path Parameters:**

- `id` (long) - Offer request ID

**Request Body:**

```json
{
	"assignedTo": "support-789"
}
```

**Required Fields:**

- `assignedTo` (string) - SalesSupport user ID (can be your own ID)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "Assigned",
		"assignedTo": "support-789",
		"assignedToName": "Support User",
		"assignedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request assigned successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- Status automatically changes to "Assigned"
- Only Managers and SalesSupport can assign requests

---

### 4. Update Offer Request Status

**Endpoint:** `PUT /api/OfferRequest/{id}/status`

**Purpose:** Update the status as you work on the request

**Request Body:**

```json
{
	"status": "InProgress",
	"notes": "Gathering equipment specifications and pricing"
}
```

**Required Fields:**

- `status` (string) - "Pending", "Assigned", "InProgress", "Completed", "Cancelled"

**Optional Fields:**

- `notes` (string) - Status update notes

**Status Flow:**

1. **Pending** → Just created by salesman
2. **Assigned** → Assigned to support (automatic when assigned)
3. **InProgress** → Support is working on it
4. **Completed** → Offer created and sent
5. **Cancelled** → Request cancelled

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 25,
		"status": "InProgress",
		"notes": "Gathering equipment specifications and pricing",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer request status updated successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 5. Get Requests Assigned to Support

**Endpoint:** `GET /api/OfferRequest/assigned/{supportId}`

**Purpose:** Get all requests assigned to a specific support member (usually yourself)

**Path Parameters:**

- `supportId` (string) - SalesSupport user ID

**Query Parameters:**

- `status` (string, optional)

**Request Example:**

```http
GET /api/OfferRequest/assigned/support-789?status=InProgress
Authorization: Bearer {token}
```

**Success Response (200):** Array of offer requests

---

### 6. Get All Offers

**Endpoint:** `GET /api/Offer`

**Purpose:** Get all offers in the system (with filters)

**Query Parameters:**

- `status` (string, optional) - Filter by status
- `clientId` (string, optional) - Filter by client
- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 50,
			"clientId": 123,
			"clientName": "Ahmed Hospital",
			"products": "X-Ray Machine Model XYZ",
			"totalAmount": 50000.0,
			"status": "Sent",
			"assignedTo": "salesman-123",
			"createdAt": "2025-10-29T12:00:00Z"
		}
	],
	"message": "Offers retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 7. Get My Offers

**Endpoint:** `GET /api/Offer/my-offers`

**Purpose:** Get offers you created

**Query Parameters:**

- `status` (string, optional)
- `startDate` (DateTime, optional)
- `endDate` (DateTime, optional)

**Success Response (200):** Same format as endpoint #6, filtered to your offers

---

### 8. Get Offer Details

**Endpoint:** `GET /api/Offer/{id}`

**Purpose:** Get full offer details including equipment, terms, installments

**Path Parameters:**

- `id` (long) - Offer ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"offerRequestId": 25,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"assignedTo": "salesman-123",
		"products": "X-Ray Machine Model XYZ",
		"totalAmount": 50000.0,
		"discountAmount": 2000.0,
		"finalPrice": 48000.0,
		"status": "Draft",
		"validUntil": "2025-11-15T23:59:59Z",
		"paymentTerms": "50% advance payment, 50% on delivery",
		"deliveryTerms": "Delivery within 2 weeks",
		"warrantyTerms": "2 years warranty included",
		"equipment": [
			{
				"id": 1,
				"name": "X-Ray Machine",
				"model": "Model XYZ",
				"manufacturer": "MedicalTech Inc",
				"quantity": 1,
				"unitPrice": 50000.0,
				"totalPrice": 50000.0,
				"specifications": "High resolution, digital imaging",
				"warrantyPeriod": "2 years",
				"imagePath": "/uploads/equipment/xray-xyz.jpg"
			}
		],
		"terms": {
			"warrantyPeriod": "2 years",
			"deliveryTime": "2 weeks",
			"installationIncluded": true,
			"trainingIncluded": true,
			"maintenanceTerms": "Free maintenance for first year",
			"paymentTerms": "50% advance, 50% on delivery"
		},
		"installmentPlan": [
			{
				"id": 1,
				"installmentNumber": 1,
				"amount": 24000.0,
				"dueDate": "2025-11-15T00:00:00Z",
				"status": "Pending"
			},
			{
				"id": 2,
				"installmentNumber": 2,
				"amount": 24000.0,
				"dueDate": "2025-12-15T00:00:00Z",
				"status": "Pending"
			}
		],
		"createdAt": "2025-10-29T12:00:00Z",
		"createdBy": "support-789"
	},
	"message": "Offer retrieved successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 9. Create Offer

**Endpoint:** `POST /api/Offer`

**Purpose:** Create a new offer based on an offer request

**Request Body:**

```json
{
	"offerRequestId": 25,
	"clientId": 123,
	"assignedTo": "salesman-123",
	"products": "X-Ray Machine Model XYZ",
	"totalAmount": 50000.0,
	"discountAmount": 2000.0,
	"paymentTerms": "50% advance payment, 50% on delivery",
	"deliveryTerms": "Delivery within 2 weeks",
	"warrantyTerms": "2 years warranty included",
	"validUntil": "2025-11-15T23:59:59Z"
}
```

**Required Fields:**

- `clientId` (long)
- `assignedTo` (string) - Salesman user ID who will present the offer
- `products` (string, max 2000) - Description of products
- `totalAmount` (decimal)

**Optional Fields:**

- `offerRequestId` (long) - Link to the request (recommended)
- `discountAmount` (decimal)
- `paymentTerms`, `deliveryTerms`, `warrantyTerms` (strings)
- `validUntil` (DateTime)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"offerRequestId": 25,
		"clientId": 123,
		"assignedTo": "salesman-123",
		"products": "X-Ray Machine Model XYZ",
		"totalAmount": 50000.0,
		"finalPrice": 50000.0,
		"status": "Draft",
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": "Offer created successfully",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

#### 9.1 Create Offer (with items array)

**Endpoint:** `POST /api/Offer/with-items`

**Purpose:** Create an offer and its equipment in one call using a structured products array.

**Request Body:**

```json
{
	"offerRequestId": 91,
	"clientId": 1,
	"assignedTo": "salesman123",
	"products": [
		{
			"name": "X-Ray Machine",
			"model": "XR-200",
			"factory": "MedTech Co.",
			"country": "Germany",
			"year": 2023,
			"price": 250000.0,
			"imageUrl": "https://cdn.example.com/xray.jpg",
			"description": "Digital X-Ray device",
			"inStock": true
		},
		{
			"name": "Ultrasound Device",
			"model": "US-90",
			"factory": "SonoWorks",
			"country": "Japan",
			"year": 2024,
			"price": 120000.0,
			"imageUrl": "https://cdn.example.com/ultrasound.jpg",
			"description": "Portable ultrasound",
			"inStock": true
		}
	],
	"totalAmount": 370000.0,
	"paymentType": "Installments",
	"paymentTerms": "30% upfront, 70% on delivery",
	"deliveryTerms": "4-6 weeks",
	"validUntil": "2025-12-31T00:00:00Z",
	"notes": "Include installation and training"
}
```

**Notes:**

- Each product maps to one equipment item.
- Field mapping to equipment: `factory → Provider`, `imageUrl → ImagePath`, `year → Year`.
- If `imageUrl` is omitted, a placeholder `offers/{offerId}/equipment-placeholder.png` will be used.

**Error Responses:**

- **400 Bad Request:** Validation errors (missing required fields, invalid amounts)
- **401 Unauthorized:** Not authenticated

**Logic:**

- Offer starts as "Draft" status
- Add equipment, terms, installments before sending
- After sending, status becomes "Sent"

---

### 10. Add Equipment to Offer

**Endpoint:** `POST /api/Offer/{offerId}/equipment`

**Purpose:** Add equipment item to an offer

**Path Parameters:**

- `offerId` (long) - Offer ID

**Request Body:**

```json
{
	"name": "X-Ray Machine",
	"model": "Model XYZ",
	"manufacturer": "MedicalTech Inc",
	"quantity": 1,
	"unitPrice": 50000.0,
	"specifications": "High resolution digital imaging, DICOM compatible",
	"warrantyPeriod": "2 years"
}
```

**Required Fields:**

- `name` (string)
- `quantity` (int)
- `unitPrice` (decimal)

**Optional Fields:**

- `model`, `manufacturer`, `specifications`, `warrantyPeriod`

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 50,
		"name": "X-Ray Machine",
		"model": "Model XYZ",
		"manufacturer": "MedicalTech Inc",
		"quantity": 1,
		"unitPrice": 50000.0,
		"totalPrice": 50000.0,
		"specifications": "High resolution digital imaging",
		"warrantyPeriod": "2 years",
		"imagePath": null
	},
	"message": "Equipment added",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Note:** `totalPrice` is automatically calculated as `quantity * unitPrice`

---

### 11. Get Equipment List

**Endpoint:** `GET /api/Offer/{offerId}/equipment`

**Purpose:** Get all equipment items for an offer

**Success Response (200):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "X-Ray Machine",
			"model": "Model XYZ",
			"quantity": 1,
			"unitPrice": 50000.0,
			"totalPrice": 50000.0,
			"imagePath": "/uploads/equipment/xray-xyz.jpg"
		},
		{
			"id": 2,
			"name": "Ultrasound Device",
			"model": "Model ABC",
			"quantity": 1,
			"unitPrice": 30000.0,
			"totalPrice": 30000.0,
			"imagePath": "/uploads/equipment/ultrasound-abc.jpg"
		}
	],
	"message": "Equipment retrieved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

---

### 12. Delete Equipment

**Endpoint:** `DELETE /api/Offer/{offerId}/equipment/{equipmentId}`

**Purpose:** Remove equipment item from offer

**Path Parameters:**

- `offerId` (long) - Offer ID
- `equipmentId` (long) - Equipment ID

**Success Response (200):**

```json
{
	"success": true,
	"data": null,
	"message": "Deleted",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **404 Not Found:** Equipment doesn't exist or not in this offer

---

### 13. Upload Equipment Image

**Endpoint:** `POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image`

**Purpose:** Upload image for equipment item

**Path Parameters:**

- `offerId` (long) - Offer ID
- `equipmentId` (long) - Equipment ID

**Request Type:** `multipart/form-data`

**Request Body:**

- `file` (File) - Image file (jpg, png, etc.)

**Request Example (JavaScript):**

```javascript
const formData = new FormData();
formData.append('file', imageFile);

const response = await fetch(
	`/api/Offer/${offerId}/equipment/${equipmentId}/upload-image`,
	{
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
		},
		body: formData,
	}
);
```

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"imagePath": "/uploads/equipment/xray-xyz-123456.jpg",
		"equipmentId": 1,
		"uploadedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Uploaded",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Error Responses:**

- **400 Bad Request:** Invalid file type or size
- **404 Not Found:** Equipment doesn't exist

**Supported Formats:** JPG, PNG, GIF (max size: typically 5MB)

---

### 14. Add/Update Offer Terms

**Endpoint:** `POST /api/Offer/{offerId}/terms`

**Purpose:** Add or update payment, delivery, warranty terms

**Path Parameters:**

- `offerId` (long) - Offer ID

**Request Body:**

```json
{
	"warrantyPeriod": "2 years",
	"deliveryTime": "2 weeks from order confirmation",
	"installationIncluded": true,
	"trainingIncluded": true,
	"maintenanceTerms": "Free maintenance for first year, then annual contract",
	"paymentTerms": "50% advance payment within 7 days, 50% on delivery",
	"deliveryTerms": "Delivery to client location, unpacking and positioning included",
	"returnPolicy": "14-day return policy if not satisfied"
}
```

**All Fields Optional:**

- `warrantyPeriod` (string)
- `deliveryTime` (string)
- `installationIncluded` (bool)
- `trainingIncluded` (bool)
- `maintenanceTerms` (string)
- `paymentTerms` (string)
- `deliveryTerms` (string)
- `returnPolicy` (string)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"offerId": 50,
		"warrantyPeriod": "2 years",
		"deliveryTime": "2 weeks from order confirmation",
		"installationIncluded": true,
		"trainingIncluded": true,
		"maintenanceTerms": "Free maintenance for first year",
		"paymentTerms": "50% advance, 50% on delivery",
		"updatedAt": "2025-10-29T12:00:00Z"
	},
	"message": "Terms saved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- If terms don't exist, they are created
- If terms exist, they are updated

---

### 15. Create Installment Plan

**Endpoint:** `POST /api/Offer/{offerId}/installments`

**Purpose:** Create payment installment plan for the offer

**Path Parameters:**

- `offerId` (long) - Offer ID

**Request Body:**

```json
{
	"numberOfInstallments": 4,
	"firstPaymentAmount": 12000.0,
	"firstPaymentDate": "2025-11-15T00:00:00Z",
	"paymentFrequency": "Monthly",
	"totalAmount": 48000.0
}
```

**Required Fields:**

- `numberOfInstallments` (int)
- `firstPaymentAmount` (decimal)
- `firstPaymentDate` (DateTime)
- `paymentFrequency` (string) - "Monthly", "Quarterly", "Yearly"
- `totalAmount` (decimal)

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 50,
		"numberOfInstallments": 4,
		"totalAmount": 48000.0,
		"installments": [
			{
				"installmentNumber": 1,
				"amount": 12000.0,
				"dueDate": "2025-11-15T00:00:00Z",
				"status": "Pending"
			},
			{
				"installmentNumber": 2,
				"amount": 12000.0,
				"dueDate": "2025-12-15T00:00:00Z",
				"status": "Pending"
			},
			{
				"installmentNumber": 3,
				"amount": 12000.0,
				"dueDate": "2026-01-15T00:00:00Z",
				"status": "Pending"
			},
			{
				"installmentNumber": 4,
				"amount": 12000.0,
				"dueDate": "2026-02-15T00:00:00Z",
				"status": "Pending"
			}
		],
		"createdAt": "2025-10-29T12:00:00Z"
	},
	"message": "Installment plan created",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- Installments are automatically calculated based on frequency
- Each installment gets sequential due dates
- Total amount should equal offer finalPrice (or close to it)

**Error Responses:**

- **400 Bad Request:** Invalid installment plan (amounts don't match, invalid dates)

---

### 16. Export Offer as PDF

**Endpoint:** `GET /api/Offer/{offerId}/export-pdf`

**Purpose:** Generate and download offer as PDF

**Path Parameters:**

- `offerId` (long) - Offer ID

**Success Response (200):**

- **Content-Type:** `application/pdf`
- **Body:** PDF file bytes
- **Headers:** `Content-Disposition: attachment; filename="offer-50.pdf"`

**Usage in Frontend:**

```javascript
const response = await fetch(`/api/Offer/${offerId}/export-pdf`, {
	headers: { Authorization: `Bearer ${token}` },
});
const blob = await response.blob();
const url = window.URL.createObjectURL(blob);
const a = document.createElement('a');
a.href = url;
a.download = `offer-${offerId}.pdf`;
a.click();
window.URL.revokeObjectURL(url);
```

**PDF Contains:**

- Client information
- Offer details (products, amounts)
- Equipment list with images
- Terms and conditions
- Installment plan (if any)
- Valid until date

---

### 17. Send Offer to Salesman

**Endpoint:** `POST /api/Offer/{offerId}/send-to-salesman`

**Purpose:** Send completed offer to salesman (changes status from "Draft" to "Sent")

**Path Parameters:**

- `offerId` (long) - Offer ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"id": 50,
		"status": "Sent",
		"sentToSalesmanAt": "2025-10-29T14:00:00Z",
		"sentBy": "support-789"
	},
	"message": "Sent",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Logic:**

- Status changes from "Draft" → "Sent"
- Offer becomes visible to assigned salesman
- Salesman can now export PDF and present to client

**Error Responses:**

- **400 Bad Request:** Offer already sent or has no equipment
- **404 Not Found:** Offer doesn't exist

---

### 18. Get Offer Request Details for Creating Offer

**Endpoint:** `GET /api/Offer/request/{requestId}/details`

**Purpose:** Get offer request details formatted for creating an offer

**Path Parameters:**

- `requestId` (long) - Offer request ID

**Success Response (200):**

```json
{
	"success": true,
	"data": {
		"requestId": 25,
		"clientId": 123,
		"clientName": "Ahmed Hospital",
		"clientType": "Hospital",
		"clientLocation": "Cairo",
		"clientPhone": "+20123456789",
		"requestedProducts": "X-Ray Machine Model XYZ",
		"specialNotes": "Client needs fast delivery within 2 weeks",
		"requestedBy": "salesman-123",
		"requestedByName": "Ahmed Mohamed",
		"requestDate": "2025-10-29T10:00:00Z"
	},
	"message": "Offer request details retrieved",
	"timestamp": "2025-10-29T12:00:00Z"
}
```

**Use Case:** Use this data to pre-fill offer creation form

---

## 🔄 Complete Offer Creation Workflow

### Step-by-Step Process:

1. **View Assigned Requests**

      ```http
      GET /api/OfferRequest/assigned/{yourSupportId}?status=Assigned
      ```

2. **Get Request Details**

      ```http
      GET /api/OfferRequest/{requestId}
      ```

      OR

      ```http
      GET /api/Offer/request/{requestId}/details
      ```

3. **Update Status to InProgress**

      ```http
      PUT /api/OfferRequest/{requestId}/status
      Body: { "status": "InProgress", "notes": "Starting work" }
      ```

4. **Create Offer**

      ```http
      POST /api/Offer
      Body: {
        "offerRequestId": 25,
        "clientId": 123,
        "assignedTo": "salesman-123",
        "products": "...",
        "totalAmount": 50000.00
      }
      ```

5. **Add Equipment Items**

      ```http
      POST /api/Offer/{offerId}/equipment
      Body: { "name": "...", "quantity": 1, "unitPrice": 50000.00 }
      ```

      Repeat for each equipment item

6. **Upload Equipment Images** (Optional)

      ```http
      POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image
      Body: FormData with file
      ```

7. **Add Terms**

      ```http
      POST /api/Offer/{offerId}/terms
      Body: { "warrantyPeriod": "...", "paymentTerms": "..." }
      ```

8. **Create Installment Plan** (Optional)

      ```http
      POST /api/Offer/{offerId}/installments
      Body: { "numberOfInstallments": 4, ... }
      ```

9. **Send to Salesman**

      ```http
      POST /api/Offer/{offerId}/send-to-salesman
      ```

10. **Update Request Status to Completed**
       ```http
       PUT /api/OfferRequest/{requestId}/status
       Body: { "status": "Completed", "notes": "Offer created and sent" }
       ```

---

## 📊 Status Management

### Offer Request Status Flow:

```
Pending → Assigned → InProgress → Completed
                    ↓
               Cancelled (if needed)
```

### Offer Status Flow:

```
Draft → Sent → Accepted/Rejected/Expired
```

### When to Update Request Status:

- **Assigned:** When you assign it to yourself
- **InProgress:** When you start working
- **Completed:** When offer is sent to salesman
- **Cancelled:** If request is cancelled

---

## ⚠️ Important Notes

1. **Equipment Total:** Total amount of all equipment should match offer `totalAmount`
2. **Image Upload:** Upload images after creating equipment items
3. **Terms:** Add terms before sending (important for PDF)
4. **Installments:** Optional, but recommended for large amounts
5. **Send to Salesman:** Only after all details are complete
6. **PDF Export:** Available for any offer, but best after sending

---

## 🔍 Error Handling

### Common Errors:

**400 Bad Request:**

- Missing required fields
- Invalid amounts (negative, zero)
- Invalid dates (past dates, invalid format)
- Equipment total doesn't match offer amount

**404 Not Found:**

- Offer/Request doesn't exist
- Equipment doesn't exist in offer

**401 Unauthorized:**

- Missing or invalid JWT token

**500 Internal Server Error:**

- Server issues, contact backend team

---

## 💡 Tips for Frontend Implementation

1. **Use Request Details:** Pre-fill offer form with request data
2. **Calculate Totals:** Automatically calculate total when adding equipment
3. **Validate Before Send:** Ensure all required fields are filled
4. **Show Status:** Display request status in UI
5. **Image Preview:** Show uploaded images in equipment list
6. **Save Drafts:** Allow saving incomplete offers (they remain in "Draft")
7. **Installment Calculator:** Help calculate installment amounts
8. **PDF Preview:** Consider showing PDF preview before sending

---

**Base URL:** `https://your-api-url.com` (or `http://localhost:port` for development)

**Last Updated:** 2025-10-29

---

## 🔄 Updates: Auto-Assign Requests + Assign Offers to Salesmen

### Auto-Assign on Offer Request Creation

- New behavior: When a Salesman creates an offer request (`POST /api/OfferRequest`), it is automatically assigned to a SalesSupport user if any exist.
- Resulting statuses:
     - `Assigned`: Support exists; request is auto-assigned
     - `Requested`: No support exists; reassign manually when available

### New Endpoint: Assign/Reassign Offer to Salesman

- `PUT /api/Offer/{offerId}/assign-to-salesman`
- Roles: SalesSupport, SalesManager, SuperAdmin
- Body:
     ```json
     { "salesmanId": "<salesmanId>" }
     ```
- Notes:
     - Validates the target user has the "Salesman" role
     - Updates the offer's `AssignedTo`

### Auto-Equipment on Offer Creation

- When creating an offer (`POST /api/Offer`), equipment items are auto-generated by parsing the `products` field (comma/newline/semicolon separated) with placeholder image paths (`offers/{offerId}/equipment-placeholder.png`).
- Upload actual images using `POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image`.
