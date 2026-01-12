# Migration Service API Documentation

## Overview

The Migration Service is an MCP-like service that provides comprehensive equipment-to-client linking operations between the legacy TBS database and the new ITIWebApi44 database. It implements all 4 linking methods as described in `MIGRATION_SUMMARY.md`.

## Architecture

### Service Layer

- **IMigrationService**: Interface defining all migration operations
- **MigrationService**: Implementation with 4 linking methods
- **MigrationController**: REST API endpoints for migration operations

### Linking Methods

1. **Via Visits**: `MNT_Visiting.Cus_ID → MNT_VisitingReport.OOI_ID → Equipment`
2. **Via Maintenance Contracts**: `MNT_MaintenanceContract.Cus_ID → MNT_MaintenanceContract_Items.OOI_ID → Equipment`
3. **Via Sales Invoices**: `Stk_Sales_Inv → Stk_Order_Out → Stk_Order_Out_Items.OOI_ID → Equipment`
4. **Via Order Out**: `Stk_Order_Out.Cus_ID → Stk_Order_Out_Items.OOI_ID → Equipment`

## API Endpoints

### 1. Link Equipment to Clients (All Methods)

**POST** `/api/Migration/link-equipment`

Links equipment to clients using all 4 methods.

**Query Parameters:**

- `adminUserId` (optional): Admin user ID to identify unlinked equipment. Defaults to configuration value.

**Response:**

```json
{
	"success": true,
	"message": "Successfully linked 1234 equipment items using 4 methods.",
	"startTime": "2026-01-06T10:00:00Z",
	"endTime": "2026-01-06T10:05:00Z",
	"duration": "00:05:00",
	"viaVisits": {
		"methodName": "Via Visits",
		"success": true,
		"linkedCount": 450,
		"skippedCount": 12,
		"duration": "00:01:30"
	},
	"viaMaintenanceContracts": {
		"methodName": "Via Maintenance Contracts",
		"success": true,
		"linkedCount": 380,
		"skippedCount": 8,
		"duration": "00:01:15"
	},
	"viaSalesInvoices": {
		"methodName": "Via Sales Invoices",
		"success": true,
		"linkedCount": 250,
		"skippedCount": 5,
		"duration": "00:01:00"
	},
	"viaOrderOut": {
		"methodName": "Via Order Out",
		"success": true,
		"linkedCount": 154,
		"skippedCount": 3,
		"duration": "00:00:45"
	},
	"totalLinked": 1234,
	"totalSkipped": 28,
	"totalErrors": 0,
	"errors": [],
	"warnings": []
}
```

### 2. Link Via Visits (Method 1)

**POST** `/api/Migration/link-via-visits`

Links equipment to clients through maintenance visits.

**Query Parameters:**

- `adminUserId` (optional): Admin user ID

**Response:**

```json
{
	"methodName": "Via Visits",
	"success": true,
	"linkedCount": 450,
	"skippedCount": 12,
	"duration": "00:01:30"
}
```

### 3. Link Via Maintenance Contracts (Method 2)

**POST** `/api/Migration/link-via-contracts`

Links equipment to clients through maintenance contracts.

**Query Parameters:**

- `adminUserId` (optional): Admin user ID

**Response:**

```json
{
	"methodName": "Via Maintenance Contracts",
	"success": true,
	"linkedCount": 380,
	"skippedCount": 8,
	"duration": "00:01:15"
}
```

### 4. Link Via Sales Invoices (Method 3)

**POST** `/api/Migration/link-via-sales-invoices`

Links equipment to clients through sales invoices.

**Query Parameters:**

- `adminUserId` (optional): Admin user ID

**Response:**

```json
{
	"methodName": "Via Sales Invoices",
	"success": true,
	"linkedCount": 250,
	"skippedCount": 5,
	"duration": "00:01:00"
}
```

### 5. Link Via Order Out (Method 4)

**POST** `/api/Migration/link-via-order-out`

Links equipment to clients through order out records.

**Query Parameters:**

- `adminUserId` (optional): Admin user ID

**Response:**

```json
{
	"methodName": "Via Order Out",
	"success": true,
	"linkedCount": 154,
	"skippedCount": 3,
	"duration": "00:00:45"
}
```

### 6. Get Diagnostics

**GET** `/api/Migration/diagnostics`

Returns diagnostic statistics about equipment linking status.

**Response:**

```json
{
	"totalEquipment": 3348,
	"equipmentWithLegacySourceId": 1487,
	"equipmentLinkedToAdmin": 253,
	"equipmentLinkedToClients": 1234,
	"equipmentUnlinked": 861,
	"totalVisitingReportsWithOoiId": 1862,
	"equipmentWithMatchingVisits": 1450,
	"equipmentWithMatchingClients": 1234,
	"totalClients": 1806,
	"clientsWithRelatedUserId": 1650,
	"clientsWithLegacyCustomerId": 1806,
	"linkingMethodStats": {
		"Via Visits": 450,
		"Via Maintenance Contracts": 380,
		"Via Sales Invoices": 250,
		"Via Order Out": 154
	}
}
```

### 7. Get Unlinked Equipment Report

**GET** `/api/Migration/unlinked-equipment?pageNumber=1&pageSize=50`

Returns a paginated report of unlinked equipment.

**Query Parameters:**

- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 50)

**Response:**

```json
{
	"totalUnlinked": 861,
	"pageNumber": 1,
	"pageSize": 50,
	"totalPages": 18,
	"equipment": [
		{
			"id": 1234,
			"name": "Ultrasound Machine Model X",
			"legacySourceId": "58419",
			"qrCode": "EQ-1234-OOI58419",
			"customerId": "Hemdan_Test_Administration_002",
			"reason": "Linked to Admin"
		}
	]
}
```

### 8. Verify Client Equipment

**GET** `/api/Migration/verify-client/{clientId}`

Verifies equipment linking for a specific client.

**Path Parameters:**

- `clientId`: Client ID (long)

**Response:**

```json
{
	"clientId": 123,
	"clientName": "Dr. Ahmed Hospital",
	"legacyCustomerId": 456,
	"relatedUserId": "user_123_abc",
	"totalEquipment": 15,
	"equipmentFromVisits": 8,
	"equipmentFromContracts": 5,
	"equipmentFromSalesInvoices": 2,
	"equipmentFromOrderOut": 0,
	"equipmentDetails": [
		{
			"equipmentId": 789,
			"equipmentName": "X-Ray Machine",
			"legacySourceId": "58419",
			"linkingMethods": ["Via Visits"],
			"isLinked": true
		}
	],
	"issues": []
}
```

## Configuration

Add to `appsettings.json`:

```json
{
	"MigrationSettings": {
		"AdminUserId": "Hemdan_Test_Administration_002"
	},
	"ConnectionStrings": {
		"TbsConnection": "Server=10.10.9.104\\SQLEXPRESS,1433;Database=TBS;User Id=soitmed;Password=***;TrustServerCertificate=True;"
	}
}
```

## Authentication

All endpoints require **Admin** role:

```http
Authorization: Bearer {jwt_token}
```

## Error Handling

All endpoints return appropriate HTTP status codes:

- `200 OK`: Success
- `400 Bad Request`: Invalid request or partial failure
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions (not Admin)
- `500 Internal Server Error`: Server error

## Usage Examples

### Link All Equipment

```bash
curl -X POST "https://api.soitmed.com/api/Migration/link-equipment?adminUserId=Hemdan_Test_Administration_002" \
  -H "Authorization: Bearer {token}"
```

### Get Diagnostics

```bash
curl -X GET "https://api.soitmed.com/api/Migration/diagnostics" \
  -H "Authorization: Bearer {token}"
```

### Get Unlinked Equipment (Page 1)

```bash
curl -X GET "https://api.soitmed.com/api/Migration/unlinked-equipment?pageNumber=1&pageSize=50" \
  -H "Authorization: Bearer {token}"
```

## Implementation Details

### Idempotency

- All linking methods are idempotent
- Equipment already linked to clients (not admin) is skipped
- Only equipment linked to admin user is processed

### Transaction Safety

- Each method executes in its own transaction
- Errors in one method don't affect others
- Database changes are committed only on success

### Performance

- Uses `AsNoTracking()` for read-only queries
- Processes equipment in batches
- Logs progress for monitoring

### Error Handling

- Catches and logs individual equipment errors
- Continues processing even if some items fail
- Returns detailed error information

## Related Documentation

- `MIGRATION_SUMMARY.md`: Complete migration strategy and analysis
- `MigrateClientsEquipmentContractsFromTBS.sql`: SQL migration script
- `EquipmentService.cs`: Equipment retrieval service

## Support

For issues or questions, contact the development team or refer to the migration summary document.
