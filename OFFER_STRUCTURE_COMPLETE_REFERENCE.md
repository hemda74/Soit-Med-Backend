# Offer Structure - Complete Reference

## Overview

An **Offer** is a sales proposal sent to clients. It's created by SalesSupport staff and assigned to Salesmen for client delivery.

---

## Database Model Structure

### `SalesOffer` Entity

```csharp
public class SalesOffer : BaseEntity
{
    // ========== BASIC INFORMATION ==========
    public long Id { get; set; }                          // Auto-generated
    public long? OfferRequestId { get; set; }            // Link to original request
    public long ClientId { get; set; }                   // Required - Which client
    public string CreatedBy { get; set; }                // SalesSupport ID
    public string AssignedTo { get; set; }               // Salesman ID

    // ========== OFFER DETAILS ==========
    public string Products { get; set; }                 // Max 2000 chars - Product description
    public decimal TotalAmount { get; set; }             // Offer total value
    public string? PaymentTerms { get; set; }           // Max 2000 chars
    public string? DeliveryTerms { get; set; }           // Max 2000 chars
    public DateTime ValidUntil { get; set; }             // Required - Offer expiration

    // ========== STATUS & WORKFLOW ==========
    public string Status { get; set; }                  // Draft, Sent, UnderReview, Accepted, Rejected, etc.
    public DateTime? SentToClientAt { get; set; }       // When offer was sent
    public string? ClientResponse { get; set; }         // Max 2000 chars - Client feedback

    // ========== ADDITIONAL INFO ==========
    public string? Documents { get; set; }               // JSON array of file paths
    public string? Notes { get; set; }                  // Max 2000 chars - Internal notes

    // ========== AUTO-MANAGED ==========
    public DateTime CreatedAt { get; set; }             // Auto-generated
    public DateTime UpdatedAt { get; set; }             // Auto-updated

    // ========== NAVIGATION ==========
    public virtual OfferRequest? OfferRequest { get; set; }
    public virtual Client Client { get; set; }
    public virtual ApplicationUser Creator { get; set; }    // SalesSupport
    public virtual ApplicationUser Salesman { get; set; }
    public virtual SalesDeal? Deal { get; set; }
}
```

---

## Status Values

### Available Statuses

| Status                | Description            | When Set            | Can Modify |
| --------------------- | ---------------------- | ------------------- | ---------- |
| **Draft**             | Just created, not sent | Creation            | ✅ Yes     |
| **Sent**              | Sent to client         | When marked as sent | ❌ No      |
| **UnderReview**       | Client is reviewing    | Manual update       | ❌ No      |
| **Accepted**          | Client accepted        | Client response     | ❌ No      |
| **Rejected**          | Client rejected        | Client response     | ❌ No      |
| **NeedsModification** | Client wants changes   | Client response     | ✅ Yes     |
| **Expired**           | ValidUntil date passed | Automatically       | ❌ No      |

---

## API Request Structure

### Create Offer (POST /api/offer)

```json
{
	"offerRequestId": 6,
	"clientId": 24,
	"assignedTo": "Ahmed_Ashraf_Sales_001",
	"products": "Complete MRI scanner package including installation, training, and 2-year warranty",
	"totalAmount": 2500000.0,
	"paymentTerms": "50% Down payment, 50% on delivery",
	"deliveryTerms": "FOB Cairo, 30 days delivery",
	"validUntil": "2025-11-25T14:17:39",
	"notes": "High priority offer. Client has approved budget."
}
```

**Required Fields:**

- ✅ `offerRequestId` - Link to request
- ✅ `clientId` - Which client
- ✅ `assignedTo` - Salesman ID
- ✅ `products` - Product description (max 2000 chars)
- ✅ `totalAmount` - Must be > 0.01
- ✅ `validUntil` - Offer expiration date

**Optional Fields:**

- ❌ `paymentTerms` - Payment conditions
- ❌ `deliveryTerms` - Delivery conditions
- ❌ `notes` - Internal notes

---

## API Response Structure

### Get Offer (GET /api/offer/{id})

```json
{
	"success": true,
	"data": {
		"id": 3,
		"offerRequestId": 6,
		"clientId": 24,
		"clientName": "Cairo University Hospital",
		"createdBy": "Ahmed_Hemdan_Engineering_001",
		"createdByName": "Ahmed Hemdan",
		"assignedTo": "Ahmed_Ashraf_Sales_001",
		"assignedToName": "Ahmed Ashraf",
		"products": "Complete MRI scanner package including installation, training, and 2-year warranty",
		"totalAmount": 2500000.0,
		"paymentTerms": "50% Down payment, 50% on delivery",
		"deliveryTerms": "FOB Cairo, 30 days delivery",
		"validUntil": "2025-11-25T14:17:39",
		"status": "Draft",
		"sentToClientAt": null,
		"clientResponse": null,
		"createdAt": "2025-10-26T11:17:39"
	},
	"message": "Offer retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## Field Details

### Basic Information Fields

| Field            | Type   | Required | Max Length | Description                    |
| ---------------- | ------ | -------- | ---------- | ------------------------------ |
| `Id`             | long   | Auto     | -          | Unique identifier              |
| `OfferRequestId` | long?  | No       | -          | Original request ID (nullable) |
| `ClientId`       | long   | ✅ Yes   | -          | Client's ID                    |
| `CreatedBy`      | string | ✅ Yes   | 450        | SalesSupport user ID           |
| `AssignedTo`     | string | ✅ Yes   | 450        | Salesman user ID               |

### Offer Details Fields

| Field           | Type     | Required | Max Length | Description         |
| --------------- | -------- | -------- | ---------- | ------------------- |
| `Products`      | string   | ✅ Yes   | 2000       | Product description |
| `TotalAmount`   | decimal  | ✅ Yes   | -          | Must be >= 0.01     |
| `PaymentTerms`  | string?  | No       | 2000       | Payment conditions  |
| `DeliveryTerms` | string?  | No       | 2000       | Delivery conditions |
| `ValidUntil`    | DateTime | ✅ Yes   | -          | Offer expiration    |

### Status Fields

| Field            | Type      | Required | Description                        |
| ---------------- | --------- | -------- | ---------------------------------- |
| `Status`         | string    | ✅ Yes   | Current status (Draft, Sent, etc.) |
| `SentToClientAt` | DateTime? | No       | When offer was sent                |
| `ClientResponse` | string?   | No       | Client feedback (max 2000)         |

### Additional Fields

| Field       | Type     | Required | Max Length | Description           |
| ----------- | -------- | -------- | ---------- | --------------------- |
| `Documents` | string?  | No       | 2000       | JSON array of files   |
| `Notes`     | string?  | No       | 2000       | Internal notes        |
| `CreatedAt` | DateTime | Auto     | -          | Creation timestamp    |
| `UpdatedAt` | DateTime | Auto     | -          | Last update timestamp |

---

## Relationships

### Navigation Properties

```
SalesOffer
    ├── OfferRequest (one-to-one) - Original request
    ├── Client (many-to-one) - Target client
    ├── Creator (many-to-one) - SalesSupport user
    ├── Salesman (many-to-one) - Assigned salesman
    └── Deal (one-to-one) - Resulting deal
```

### Foreign Keys

- `OfferRequestId` → `OfferRequests.Id`
- `ClientId` → `Clients.Id`
- `CreatedBy` → `AspNetUsers.Id`
- `AssignedTo` → `AspNetUsers.Id`

---

## Business Logic Methods

### Available Methods

```csharp
// Mark offer as sent
offer.MarkAsSent();

// Record client response
offer.RecordClientResponse("Client approved!", true);

// Check if expired
bool expired = offer.IsExpired();

// Check if can be modified
bool canModify = offer.CanBeModified(); // Only Draft or NeedsModification
```

---

## Data Validation Rules

### Validation Rules

| Field         | Validation                    |
| ------------- | ----------------------------- |
| `TotalAmount` | Must be >= 0.01               |
| `Products`    | Required, max 2000 characters |
| `ValidUntil`  | Must be future date           |
| `Status`      | Must be valid enum value      |

### Business Rules

1. ✅ Can only modify Draft or NeedsModification status
2. ✅ Cannot delete offers with associated deals
3. ✅ Expired offers cannot be modified
4. ✅ Accepted offers create deals

---

## Example Usage Scenarios

### Scenario 1: Create Draft Offer

**Request:**

```json
POST /api/offer
{
  "offerRequestId": 6,
  "clientId": 24,
  "assignedTo": "Ahmed_Ashraf_Sales_001",
  "products": "MRI Scanner Model X1",
  "totalAmount": 2500000.00,
  "validUntil": "2025-11-25T14:17:39",
  "paymentTerms": "50% Down, 50% Delivery",
  "deliveryTerms": "30 days FOB Cairo"
}
```

**Status:** Draft (auto-set)

---

### Scenario 2: Send to Client

**Request:**

```
POST /api/offer/{id}/send
```

**What Happens:**

- Status changes to "Sent"
- `SentToClientAt` = Current timestamp
- Cannot be modified anymore

---

### Scenario 3: Client Accepts

**What Happens:**

- Status changes to "Accepted"
- `ClientResponse` recorded
- Can now create deal from this offer

---

## Complete Field Reference

### CreateOfferDTO (Request)

```typescript
interface CreateOfferDTO {
	offerRequestId: number; // Required
	clientId: number; // Required
	assignedTo: string; // Required - Salesman ID
	products: string; // Required, max 2000 chars
	totalAmount: number; // Required, >= 0.01
	paymentTerms?: string; // Optional, max 2000 chars
	deliveryTerms?: string; // Optional, max 2000 chars
	validUntil: string; // Required - ISO DateTime
	notes?: string; // Optional, max 2000 chars
}
```

### OfferResponseDTO (Response)

```typescript
interface OfferResponseDTO {
	id: number;
	offerRequestId?: number;
	clientId: number;
	clientName: string; // Populated
	createdBy: string;
	createdByName: string; // Populated
	assignedTo: string;
	assignedToName: string; // Populated
	products: string;
	totalAmount: number;
	paymentTerms?: string;
	deliveryTerms?: string;
	validUntil: string; // ISO DateTime
	status: string; // Draft, Sent, Accepted, etc.
	sentToClientAt?: string; // ISO DateTime
	clientResponse?: string;
	createdAt: string; // ISO DateTime
}
```

---

## Status Workflow

```
[Draft] → [Sent] → [UnderReview] → [Accepted] ✓
                                    [Rejected] ✗
                                    [NeedsModification] → [Draft] → [Sent]

[Any Status] → [Expired] (if ValidUntil passed)
```

---

**End of Offer Structure Documentation**
