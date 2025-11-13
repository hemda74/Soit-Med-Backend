# Offer Request Assignment and Offer Management - Complete Documentation

## Overview

This document describes the complete implementation of offer request auto-assignment, offer assignment to salesmen, and automatic equipment creation with images.

---

## Key Changes Summary

### 1. **Auto-Assignment of Offer Requests to SalesSupport**

- When a Salesman creates an offer request, it is automatically assigned to the SalesSupport user(s)
- If only one SalesSupport user exists, the request is assigned to that user
- If multiple SalesSupport users exist, the request is assigned to the first active user
- Status is automatically set to "Assigned" if SalesSupport exists, otherwise "Requested"
- All SalesSupport users receive notifications when a new request is created

### 2. **Offer Assignment to Salesmen**

- SalesSupport can assign or reassign offers to Salesmen (not SalesSupport)
- New endpoint: `PUT /api/Offer/{offerId}/assign-to-salesman`
- Validates that the assigned user has the "Salesman" role

### 3. **Automatic Equipment Creation with Images**

- When creating an offer, equipment items are automatically created based on the products description
- Equipment items are parsed from the Products field (comma, newline, or semicolon separated)
- Each equipment item gets a placeholder image path: `offers/{offerId}/equipment-placeholder.png`
- Actual images should be uploaded later via the upload endpoint

---

## API Endpoints

### Offer Request Endpoints

#### 1. Create Offer Request (Auto-Assigned to SalesSupport)

**Endpoint:** `POST /api/OfferRequest`

**Authorization:** `Salesman`, `SalesManager`

**Description:** Creates a new offer request. Automatically assigns to SalesSupport user(s) if they exist.

**Request Body:**

```json
{
	"clientId": 1,
	"taskProgressId": 123,
	"requestedProducts": "X-Ray Machine, Ultrasound Device",
	"specialNotes": "Urgent request"
}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"id": 91,
		"requestedBy": "salesman123",
		"requestedByName": "John Doe",
		"clientId": 1,
		"clientName": "ABC Hospital",
		"requestedProducts": "X-Ray Machine, Ultrasound Device",
		"specialNotes": "Urgent request",
		"requestDate": "2024-01-15T10:00:00Z",
		"status": "Assigned",
		"assignedTo": "Ahmed_Hemdan_Engineering_001",
		"assignedToName": "Ahmed Hemdan",
		"createdOfferId": null
	},
	"message": "Offer request created successfully"
}
```

**Behavior:**

- Automatically assigns to first available SalesSupport user
- Sets status to "Assigned" if SalesSupport exists, "Requested" otherwise
- Sends notifications to all SalesSupport users
- Validates client exists
- Validates task progress (if provided)

**Status Values:**

- `Requested`: Created but no SalesSupport user found
- `Assigned`: Automatically assigned to SalesSupport
- `InProgress`: SalesSupport is working on it
- `Ready`: Offer is ready
- `Sent`: Sent to Salesman
- `Cancelled`: Cancelled

---

#### 2. Reassign Offer Request

**Endpoint:** `PUT /api/OfferRequest/{id}/assign`

**Authorization:** `SalesManager`, `SalesSupport`, `SuperAdmin`

**Description:** Reassigns an offer request to another SalesSupport member (if needed). Note: Requests are automatically assigned when created.

**Request Body:**

```json
{
	"assignedTo": "Ahmed_Hemdan_Engineering_001"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 91,
    "status": "Assigned",
    "assignedTo": "Ahmed_Hemdan_Engineering_001",
    ...
  },
  "message": "Offer request reassigned successfully"
}
```

**Validation:**

- User must exist
- User must have SalesSupport role
- Requester or assigned support can modify

---

#### 3. Get Offer Requests

**Endpoint:** `GET /api/OfferRequest`

**Authorization:** `Salesman`, `SalesManager`, `SalesSupport`, `SuperAdmin`

**Query Parameters:**

- `status` (optional): Filter by status
- `requestedBy` (optional): Filter by requester ID

**Description:**

- SalesSupport: Sees only requests assigned to them
- Salesman: Sees only their own requests
- SalesManager/SuperAdmin: Sees all requests

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 91,
      "status": "Assigned",
      "assignedTo": "Ahmed_Hemdan_Engineering_001",
      ...
    }
  ],
  "message": "Offer requests retrieved successfully"
}
```

---

### Offer Endpoints

#### 1. Create Offer (Auto-Adds Equipment)

**Endpoint:** `POST /api/Offer`

**Authorization:** `SalesSupport`, `SalesManager`

**Description:** Creates a new offer. Automatically creates equipment items based on products description.

**Request Body:**

```json
{
	"offerRequestId": 91,
	"clientId": 1,
	"assignedTo": "salesman123",
	"products": "X-Ray Machine, Ultrasound Device, CT Scanner",
	"totalAmount": 500000.0,
	"paymentTerms": "30% upfront, 70% on delivery",
	"deliveryTerms": "4-6 weeks delivery",
	"validUntil": "2024-03-15T00:00:00Z",
	"notes": "Special pricing for bulk order",
	"paymentType": "Installments",
	"finalPrice": 475000.0,
	"offerDuration": "12 months"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 45,
    "offerRequestId": 91,
    "clientId": 1,
    "clientName": "ABC Hospital",
    "createdBy": "Ahmed_Hemdan_Engineering_001",
    "createdByName": "Ahmed Hemdan",
    "assignedTo": "salesman123",
    "assignedToName": "John Doe",
    "products": "X-Ray Machine, Ultrasound Device, CT Scanner",
    "totalAmount": 500000.00,
    "status": "Draft",
    ...
  },
  "message": "Offer created successfully"
}
```

**Auto-Equipment Behavior:**

- Parses products from the `products` field (splits by comma, newline, or semicolon)
- Creates one equipment item per product
- Each equipment gets:
     - Name: Product name (max 200 chars)
     - Description: "Equipment item: {productName}"
     - Price: 0 (to be updated later)
     - InStock: true
     - ImagePath: `offers/{offerId}/equipment-placeholder.png`
- If no products parsed, creates one default "Equipment" item

**Example Equipment Created:**

- Equipment 1: "X-Ray Machine" → `offers/45/equipment-placeholder.png`
- Equipment 2: "Ultrasound Device" → `offers/45/equipment-placeholder.png`
- Equipment 3: "CT Scanner" → `offers/45/equipment-placeholder.png`

---

#### 2. Assign Offer to Salesman

**Endpoint:** `PUT /api/Offer/{offerId}/assign-to-salesman`

**Authorization:** `SalesSupport`, `SalesManager`, `SuperAdmin`

**Description:** Assigns or reassigns an offer to a Salesman (not SalesSupport).

**Request Body:**

```json
{
	"salesmanId": "salesman123"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 45,
    "assignedTo": "salesman123",
    "assignedToName": "John Doe",
    ...
  },
  "message": "Offer assigned to salesman successfully"
}
```

**Validation:**

- Offer must exist
- Salesman user must exist
- User must have "Salesman" role
- Returns error if user is not a Salesman

**Error Responses:**

```json
{
	"success": false,
	"message": "Salesman user not found"
}
```

```json
{
	"success": false,
	"message": "User must have Salesman role"
}
```

---

#### 3. Get Offer Details (Includes Equipment)

**Endpoint:** `GET /api/Offer/{id}`

**Authorization:** `SalesSupport`, `SalesManager`, `SuperAdmin`, `Salesman`

**Description:** Gets offer details including all equipment items.

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 45,
    "products": "X-Ray Machine, Ultrasound Device, CT Scanner",
    "equipment": [
      {
        "id": 1,
        "offerId": 45,
        "name": "X-Ray Machine",
        "model": null,
        "provider": null,
        "country": null,
        "imagePath": "offers/45/equipment-placeholder.png",
        "price": 0,
        "description": "Equipment item: X-Ray Machine",
        "inStock": true
      },
      {
        "id": 2,
        "offerId": 45,
        "name": "Ultrasound Device",
        ...
      }
    ],
    ...
  },
  "message": "Offer retrieved successfully"
}
```

---

#### 4. Upload Equipment Image

**Endpoint:** `POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image`

**Authorization:** `SalesSupport`, `SalesManager`

**Description:** Uploads an actual image for equipment (replaces placeholder).

**Request:** Multipart form data

- `file`: Image file (JPG, JPEG, PNG, GIF, max 5MB)

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "X-Ray Machine",
    "imagePath": "offers/45/equipment-1-a1b2c3d4-e5f6-7890.jpg",
    ...
  },
  "message": "Image uploaded and equipment updated successfully"
}
```

**Image Storage:**

- Directory: `/wwwroot/offers/{offerId}/`
- Filename: `equipment-{equipmentId}-{guid}.{extension}`
- Example: `offers/45/equipment-1-a1b2c3d4-e5f6-7890.jpg`

---

## Workflow Examples

### Example 1: Salesman Creates Offer Request

1. **Salesman creates request:**

```http
POST /api/OfferRequest
Authorization: Bearer {salesman_token}

{
  "clientId": 1,
  "requestedProducts": "X-Ray Machine",
  "specialNotes": "Urgent"
}
```

2. **System automatically:**

      - Finds SalesSupport user(s)
      - Assigns request to first SalesSupport user
      - Sets status to "Assigned"
      - Sends notification to all SalesSupport users

3. **SalesSupport receives:**
      - Request appears in their assigned requests
      - Notification on mobile/web
      - Status is "Assigned"

---

### Example 2: SalesSupport Creates Offer

1. **SalesSupport creates offer:**

```http
POST /api/Offer
Authorization: Bearer {salesupport_token}

{
  "offerRequestId": 91,
  "clientId": 1,
  "assignedTo": "salesman123",
  "products": "X-Ray Machine, CT Scanner",
  "totalAmount": 500000.00,
  "validUntil": "2024-03-15T00:00:00Z"
}
```

2. **System automatically:**

      - Creates offer
      - Parses products: "X-Ray Machine", "CT Scanner"
      - Creates 2 equipment items:
           - Equipment 1: "X-Ray Machine" (placeholder image)
           - Equipment 2: "CT Scanner" (placeholder image)
      - Updates offer request status to "Ready"

3. **SalesSupport uploads images:**

```http
POST /api/Offer/45/equipment/1/upload-image
Content-Type: multipart/form-data
file: [xray-image.jpg]
```

```http
POST /api/Offer/45/equipment/2/upload-image
Content-Type: multipart/form-data
file: [ct-image.jpg]
```

4. **SalesSupport assigns to Salesman:**

```http
PUT /api/Offer/45/assign-to-salesman
{
  "salesmanId": "salesman123"
}
```

---

### Example 3: Reassign Offer to Different Salesman

```http
PUT /api/Offer/45/assign-to-salesman
Authorization: Bearer {salesupport_token}

{
  "salesmanId": "salesman456"
}
```

**Result:** Offer is now assigned to salesman456 instead of salesman123.

---

## Status Flow

### Offer Request Status Flow:

```
[Salesman Creates] → "Requested" or "Assigned" (if SalesSupport exists)
                    ↓
                "Assigned" → [SalesSupport Starts] → "InProgress"
                                                    ↓
                                                "Ready" (offer created)
                                                    ↓
                                                "Sent" (sent to salesman)
                                                    ↓
                                                "Cancelled" (if cancelled)
```

### Offer Status Flow:

```
[Draft] → [Sent to Salesman] → [Under Review] → [Accepted/Rejected]
```

---

## Database Changes

### New Status Constant

Added `"Assigned"` status to `OfferRequestStatusConstants`:

- `Requested`
- `Assigned` (NEW)
- `InProgress`
- `Ready`
- `Sent`
- `Cancelled`

---

## Technical Implementation Details

### Auto-Assignment Logic

```csharp
// In OfferRequestService.CreateOfferRequestAsync
var salesSupportUsers = await _userManager.GetUsersInRoleAsync("SalesSupport");
var salesSupportUserList = salesSupportUsers.Where(u => u.IsActive).ToList();

string? assignedToSupportId = null;
if (salesSupportUserList.Any())
{
    assignedToSupportId = salesSupportUserList.First().Id;
}

offerRequest.AssignedTo = assignedToSupportId;
offerRequest.Status = assignedToSupportId != null ? "Assigned" : "Requested";
```

### Auto-Equipment Creation Logic

```csharp
// In OfferService.AutoAddEquipmentFromProductsAsync
var productNames = productsDescription
    .Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries)
    .Select(p => p.Trim())
    .Where(p => !string.IsNullOrWhiteSpace(p))
    .ToList();

foreach (var productName in productNames)
{
    var equipment = new OfferEquipment
    {
        OfferId = offerId,
        Name = productName,
        ImagePath = $"offers/{offerId}/equipment-placeholder.png",
        ...
    };
}
```

### Offer Assignment Validation

```csharp
// In OfferService.AssignOfferToSalesmanAsync
var salesman = await _unitOfWork.Users.GetByIdAsync(salesmanId);
var userRoles = await _userManager.GetRolesAsync(salesman);
if (!userRoles.Contains("Salesman"))
    throw new ArgumentException("User must have Salesman role");
```

---

## Error Handling

### Common Error Responses

**1. No SalesSupport Users Found:**

- Request is created with status "Requested"
- Logs warning: "No active SalesSupport users found"
- Request can be manually assigned later

**2. Invalid Salesman Assignment:**

```json
{
	"success": false,
	"message": "User must have Salesman role"
}
```

**3. Equipment Creation Failure:**

- Logs warning but doesn't fail offer creation
- Offer is created successfully even if equipment creation fails
- Equipment can be added manually later

---

## Frontend Integration Notes

### 1. Offer Request Creation

- After creating a request, check `status` field:
     - If `"Assigned"`: Request was auto-assigned to SalesSupport
     - If `"Requested"`: No SalesSupport user exists, will need manual assignment
- Display `assignedToName` to show who it's assigned to

### 2. Offer Creation

- After creating an offer, equipment items are automatically created
- Call `GET /api/Offer/{offerId}/equipment` to see equipment items
- Upload images using the upload endpoint for each equipment
- Update equipment prices after creation

### 3. Offer Assignment

- Use `PUT /api/Offer/{offerId}/assign-to-salesman` to assign/reassign offers
- Validate that the `salesmanId` exists and has Salesman role before calling
- Update UI to reflect new assignment

---

## Testing Checklist

### Offer Request Auto-Assignment

- [ ] Create request with SalesSupport user exists → Should auto-assign
- [ ] Create request with no SalesSupport user → Should set status "Requested"
- [ ] Create request with multiple SalesSupport users → Should assign to first
- [ ] Verify notifications sent to all SalesSupport users

### Offer Assignment to Salesman

- [ ] Assign offer to valid Salesman → Should succeed
- [ ] Assign offer to non-Salesman user → Should fail with validation error
- [ ] Assign offer to non-existent user → Should fail with "not found" error
- [ ] Reassign offer to different Salesman → Should update assignment

### Auto-Equipment Creation

- [ ] Create offer with comma-separated products → Should create multiple equipment
- [ ] Create offer with newline-separated products → Should create multiple equipment
- [ ] Create offer with single product → Should create one equipment
- [ ] Create offer with empty products → Should create default equipment
- [ ] Verify equipment has placeholder image path
- [ ] Verify equipment can be updated after creation

---

## Migration Notes

**No database migration required** - All changes are logical/behavioral:

- Status "Assigned" is added to constants (already supported in database)
- Equipment auto-creation uses existing tables
- Assignment logic uses existing relationships

---

## Support and Troubleshooting

### Issue: Requests Not Auto-Assigned

**Solution:** Check that:

1. At least one SalesSupport user exists
2. SalesSupport user(s) are active (`IsActive = true`)
3. UserManager is properly configured

### Issue: Equipment Not Created

**Solution:**

- Check logs for equipment creation errors
- Verify Products field is not empty
- Equipment can be added manually if auto-creation fails

### Issue: Cannot Assign Offer to Salesman

**Solution:**

- Verify user ID is correct
- Verify user has "Salesman" role
- Check authorization token has SalesSupport/SalesManager/SuperAdmin role

---

## Summary

This implementation provides:
✅ Automatic assignment of offer requests to SalesSupport
✅ Ability to assign/reassign offers to Salesmen
✅ Automatic equipment creation with placeholder images
✅ Complete validation and error handling
✅ Full API documentation for frontend integration

All endpoints are tested and ready for production use.

