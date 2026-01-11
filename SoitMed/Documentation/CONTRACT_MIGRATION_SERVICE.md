# Contract Migration Service Documentation

## Overview

The Contract Migration Service migrates contracts, installments, and payment data from the legacy TBS database to the new ITIWebApi44 database on the same SQL Server instance. It implements idempotency checks, media path transformation, and comprehensive error handling.

## Architecture

### Components

1. **Contract Entity Models** (`Models/Contract/`)

      - `Contract.cs` - Main contract entity with lifecycle management
      - `ContractNegotiation.cs` - Negotiation history and notes
      - `InstallmentSchedule.cs` - Installment payment schedules

2. **Legacy TBS Context** (`Models/Legacy/TbsDbContext.cs`)

      - Separate DbContext for reading from TBS database
      - Models: `TbsMaintenanceContract`, `TbsSalesContract`, `TbsCustomer`, `TbsSalesInvoice`

3. **MediaPathTransformer Service** (`Services/MediaPathTransformer.cs`)

      - Transforms legacy file paths to new API URL format
      - Based on logic from `soitmed_data_backend`
      - Example: `D:\Soit-Med\legacy\SOIT\UploadFiles\Files\image.jpg` → `/api/LegacyMedia/files/image.jpg`

4. **ContractMigrationService** (`Services/ContractMigrationService.cs`)

      - Main migration logic with idempotency checks
      - Maps TBS contracts to new schema
      - Creates installments and negotiation notes

5. **Admin Endpoint** (`Controllers/ContractMigrationController.cs`)
      - `POST /api/ContractMigration/migrate-all` - Migrate all contracts
      - `POST /api/ContractMigration/migrate/{legacyContractId}` - Migrate specific contract
      - `GET /api/ContractMigration/statistics` - Get migration statistics

## Configuration

### appsettings.json

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=...;Database=ITIWebApi44;...",
		"TbsConnection": "Server=...;Database=TBS;..."
	},
	"ConnectionSettings": {
		"LegacyMediaApiBaseUrl": "http://10.10.9.100:5266"
	}
}
```

## Migration Logic

### 1. Contract Mapping (TBS → ITIWebApi44)

| TBS Field                 | New Field                           | Transformation                    |
| ------------------------- | ----------------------------------- | --------------------------------- |
| `ContractId`              | `LegacyContractId`                  | Direct mapping                    |
| `ContractCode`            | `ContractNumber`                    | Converted to string               |
| `CusId`                   | `ClientId`                          | Lookup via `LegacyCustomerId`     |
| `SoId`                    | `DealId`                            | Lookup in `SalesDeals` (nullable) |
| `StartDate`               | `DraftedAt`, `SignedAt`             | Direct mapping                    |
| `EndDate`                 | Used for status determination       | -                                 |
| `ContractTotalValue`      | `CashAmount` or `InstallmentAmount` | Based on payment plan             |
| `ScFile`                  | `DocumentUrl`                       | **Media path transformation**     |
| `NotesTech/Finance/Admin` | `ContractContent`                   | Combined into single field        |

### 2. Status Determination

- **Expired**: `EndDate < DateTime.UtcNow`
- **Signed**: `StartDate <= now && EndDate >= now`
- **Default**: `Signed` (for completed legacy contracts)

### 3. Media Path Transformation

The `MediaPathTransformer` service:

1. **Extracts filename** from legacy path:

      - Handles paths like: `D:\Soit-Med\legacy\SOIT\UploadFiles\Files\image.jpg`
      - Extracts: `image.jpg`

2. **Builds new API URL**:

      - If `LegacyMediaApiBaseUrl` configured: `http://10.10.9.100:5266/api/Media/files/image.jpg`
      - Otherwise: `/api/LegacyMedia/files/image.jpg`

3. **Handles multiple files**:
      - Input: `"file1.jpg|file2.jpg"`
      - Output: `"/api/LegacyMedia/files/file1.jpg,/api/LegacyMedia/files/file2.jpg"`

### 4. Installment Schedule Creation

If `InstallmentMonths` and `InstallmentAmount` exist in TBS:

- Calculates per-installment amount: `totalAmount / installmentMonths`
- Creates `InstallmentSchedule` records for each month
- Sets `DueDate` = `startDate.AddMonths(installmentNumber)`
- Sets status: `Overdue` if `dueDate < now`, else `Pending`

### 5. System Note Creation

Every migrated contract gets a system note in `ContractNegotiations`:

- **ActionType**: "System"
- **Notes**: "Migrated from Legacy TBS"
- **SubmittedBy**: Admin user ID
- **SubmitterRole**: "System"

## Idempotency

The service checks for existing contracts before migration:

```csharp
var existingContract = await _context.Contracts
    .FirstOrDefaultAsync(c => c.LegacyContractId == legacyContract.ContractId);

if (existingContract != null)
{
    // Skip - already migrated
    continue;
}
```

This ensures:

- Safe to run migration multiple times
- No duplicate contracts created
- Can resume after partial failures

## Error Handling

- **Transaction-based**: All operations within a single transaction
- **Rollback on error**: Changes are rolled back if any step fails
- **Detailed logging**: All errors logged with context
- **Error collection**: Multiple errors collected and returned in response

## Usage Examples

### Migrate All Contracts

```http
POST /api/ContractMigration/migrate-all
Authorization: Bearer {admin_token}
```

**Response:**

```json
{
	"success": true,
	"message": "Migration completed: 150 contracts migrated, 0 errors",
	"data": {
		"contractsMigrated": 150,
		"installmentsMigrated": 450,
		"negotiationsCreated": 150,
		"errors": 0
	}
}
```

### Migrate Specific Contract

```http
POST /api/ContractMigration/migrate/1234
Authorization: Bearer {admin_token}
```

### Get Statistics

```http
GET /api/ContractMigration/statistics
Authorization: Bearer {admin_token}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"totalLegacyContracts": 200,
		"migratedContracts": 150,
		"pendingContracts": 50,
		"failedContracts": 0
	}
}
```

## Key Features

1. **Cross-Database Queries**: Uses separate DbContexts for TBS and ITIWebApi44
2. **Media Path Transformation**: Applies transformation logic from `soitmed_data_backend`
3. **Idempotency**: Safe to run multiple times
4. **Transaction Safety**: All-or-nothing migration per contract
5. **Comprehensive Mapping**: Maps all relevant fields including notes and financial data
6. **Installment Support**: Creates installment schedules from payment plans
7. **System Notes**: Automatically creates migration notes in negotiations

## Dependencies

- Entity Framework Core (for database access)
- ASP.NET Core Identity (for user authentication)
- ILogger (for logging)
- IUnitOfWork (for transaction management)

## Notes

- The service requires both databases (TBS and ITIWebApi44) to be on the same SQL Server instance
- Client lookup is done via `LegacyCustomerId` - ensure clients are migrated first
- Deal lookup is optional - contracts can be created without deals if `SoId` is null
- Media paths are transformed but files must be accessible via the legacy media API
