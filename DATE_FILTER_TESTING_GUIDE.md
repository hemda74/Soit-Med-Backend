# Date Filter Testing Guide for SalesSupport Offers

## ✅ NEW: Date Range Filtering Implemented!

### Endpoints with Date Filtering

1. **GET /api/offer/my-offers** - Your created offers
2. **GET /api/offer** - All system offers

### Available Date Parameters

- **startDate** (DateTime, optional) - Filter from this date
- **endDate** (DateTime, optional) - Filter until this date

---

## Swagger Test Examples

### Test 1: Get Offers From Last 30 Days

**Endpoint**: `GET /api/offer/my-offers`

**Parameters**:

- startDate: `2025-09-26` (or leave empty)
- endDate: `2025-10-26` (or leave empty)

**Example**:

```
GET /api/offer/my-offers?startDate=2025-09-26&endDate=2025-10-26
```

---

### Test 2: Get Draft Offers in Date Range

**Endpoint**: `GET /api/offer/my-offers`

**Parameters**:

- status: `Draft`
- startDate: `2025-10-01`
- endDate: `2025-10-26`

**Example**:

```
GET /api/offer/my-offers?status=Draft&startDate=2025-10-01&endDate=2025-10-26
```

---

### Test 3: Get All Offers From Start Date

**Endpoint**: `GET /api/offer`

**Parameters**:

- startDate: `2025-10-26`
- endDate: (leave empty)

**Example**:

```
GET /api/offer?startDate=2025-10-26
```

**Expected**: Returns all offers created from October 26, 2025 onwards

---

### Test 4: Get All Offers Until End Date

**Endpoint**: `GET /api/offer`

**Parameters**:

- startDate: (leave empty)
- endDate: `2025-10-26`

**Example**:

```
GET /api/offer?endDate=2025-10-26
```

**Expected**: Returns all offers created up to October 26, 2025

---

### Test 5: Combined Filters - Status, Client, Date

**Endpoint**: `GET /api/offer`

**Parameters**:

- status: `Sent`
- clientId: `24`
- startDate: `2025-10-01`
- endDate: `2025-10-26`

**Example**:

```
GET /api/offer?status=Sent&clientId=24&startDate=2025-10-01&endDate=2025-10-26
```

**Expected**: Returns "Sent" offers for client 24 created between Oct 1-26

---

## Date Format in Swagger

**Use ISO 8601 format**:

- Format: `YYYY-MM-DD` or `YYYY-MM-DDTHH:mm:ss`
- Examples:
     - `2025-10-26` (date only)
     - `2025-10-26T10:30:00` (date with time)
     - `2025-10-26T00:00:00` (start of day)
     - `2025-10-26T23:59:59` (end of day)

---

## Quick Test Scenarios for Swagger

### Scenario 1: Check What You Created This Week

```
GET /api/offer/my-offers?startDate=2025-10-20&endDate=2025-10-26
```

### Scenario 2: Check All Draft Offers Last Month

```
GET /api/offer/my-offers?status=Draft&startDate=2025-09-26&endDate=2025-10-26
```

### Scenario 3: Find All Accepted Offers This Month

```
GET /api/offer?status=Accepted&startDate=2025-10-01&endDate=2025-10-26
```

### Scenario 4: Track Client Activity in Date Range

```
GET /api/offer?clientId=24&startDate=2025-10-01&endDate=2025-10-26
```

---

## Expected Responses

### With Date Filter Applied

```json
{
	"success": true,
	"data": [
		{
			"id": 3,
			"clientId": 24,
			"clientName": "Cairo University Hospital",
			"products": "Complete MRI scanner package",
			"totalAmount": 2500000.0,
			"status": "Draft",
			"createdAt": "2025-10-26T11:17:39",
			"validUntil": "2025-11-25T14:17:39"
		}
	],
	"message": "My offers retrieved successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

## How to Test in Swagger

1. **Navigate to Endpoint**: `GET /api/offer/my-offers` or `GET /api/offer`
2. **Click "Try it out"**
3. **Fill Parameters**:
      - **status**: (optional) `Draft`, `Sent`, `Accepted`, or `Rejected`
      - **startDate**: (optional) `2025-10-01`
      - **endDate**: (optional) `2025-10-26`
      - **clientId**: (optional) `24` (for GET /api/offer only)
4. **Click "Execute"**

---

## Common Use Cases

### Use Case 1: Monthly Report

```
GET /api/offer/my-offers?startDate=2025-10-01&endDate=2025-10-31
```

**Purpose**: Get all your offers created in October 2025

### Use Case 2: Recent Draft Offers

```
GET /api/offer/my-offers?status=Draft&startDate=2025-10-20
```

**Purpose**: See recent draft offers that need to be sent

### Use Case 3: Accepted Offers This Quarter

```
GET /api/offer?status=Accepted&startDate=2025-10-01&endDate=2025-12-31
```

**Purpose**: Review successful offers this quarter

### Use Case 4: Client Activity This Week

```
GET /api/offer?clientId=24&startDate=2025-10-20&endDate=2025-10-26
```

**Purpose**: Track all offers for a client this week

---

## Tips

✅ **Use date range for period-based reports**  
✅ **Combine with status for targeted queries**  
✅ **Use startDate only to get all future data**  
✅ **Use endDate only to get all past data**  
✅ **Leave both empty for all data**

❌ **Don't use dates in future**  
❌ **Don't use invalid date format**

---

## Summary

| Filter    | Status | My Offers | All Offers | Client Filter |
| --------- | ------ | --------- | ---------- | ------------- |
| status    | ✅     | ✅        | ✅         | ✅            |
| startDate | ✅     | ✅        | ✅         | ❌            |
| endDate   | ✅     | ✅        | ✅         | ❌            |
| clientId  | ✅     | ❌        | ✅         | ❌            |

**Status**: All date filtering is now LIVE and ready to test! 🎉
