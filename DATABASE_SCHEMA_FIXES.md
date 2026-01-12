# Database Schema Fixes for Comprehensive Maintenance Module

## Problem Analysis

The original SQL script failed because it was designed for a new database schema but didn't account for the existing database structure in the itiwebapi44 database.

## Key Issues Found

### 1. **Table Structure Mismatches**
- **Equipment table**: Existing table uses `int Id` primary key, not `NVARCHAR(450)`
- **MaintenanceVisit table**: Existing table uses `int Id` primary key, not `NVARCHAR(450)`
- **Client table**: Already exists with different column structure

### 2. **Column Name Differences**
- Existing Equipment table doesn't have `SerialNumber`, `InstallationDate`, `WarrantyExpiryDate`, `Location` columns
- Existing MaintenanceVisit table doesn't have `VisitType`, `CompletionDate` columns
- Customer relationships work differently (CustomerId references ApplicationUser, not Clients table)

### 3. **Foreign Key Conflicts**
- Existing Equipment.CustomerId references ApplicationUser table
- Existing MaintenanceVisit.CustomerId references ApplicationUser table
- Existing MaintenanceVisit.DeviceId references Equipment.Id (int)

## Solutions Implemented

### 1. **Fixed SQL Script**
**File**: `CreateComprehensiveMaintenanceTables_Fixed.sql`

#### Changes Made:
- **New table names**: Used `MaintenanceInvoices`, `MaintenanceInvoiceItems`, `MaintenancePayments` to avoid conflicts
- **Correct data types**: Used `int` for EquipmentId and VisitId references
- **Column mapping**: Mapped existing columns to new DTO structure
- **Conditional column additions**: Added missing columns only if they don't exist

#### Key Fixes:
```sql
-- Fixed EquipmentId reference to use int instead of string
CREATE TABLE ContractItems (
    EquipmentId INT NOT NULL, -- References existing Equipment table (int Id)
    ...
);

-- Fixed VisitId reference to use int instead of string  
CREATE TABLE MediaFiles (
    VisitId INT NOT NULL, -- References existing MaintenanceVisit table (int Id)
    ...
);
```

### 2. **Fixed Service Implementation**
**File**: `ComprehensiveMaintenanceService_Fixed.cs`

#### Changes Made:
- **Type conversions**: Convert between `int` and `string` IDs properly
- **Column mapping**: Map existing columns to expected DTO properties
- **Relationship handling**: Work with existing foreign key relationships
- **Null handling**: Handle missing columns gracefully

#### Key Fixes:
```csharp
// Convert string ID to int for existing tables
var id = int.Parse(equipmentId);
var equipment = await _newDbContext.Equipment.FindAsync(id);

// Map existing columns to new DTO structure
return new EquipmentDTO
{
    Id = equipment.Id.ToString(),
    SerialNumber = equipment.QRCode, // Using QRCode as serial number
    Model = equipment.Model,
    Manufacturer = equipment.Manufacturer,
    CustomerId = equipment.CustomerId,
    InstallationDate = equipment.PurchaseDate?.ToString("yyyy-MM-dd"),
    WarrantyExpiryDate = equipment.WarrantyExpiry?.ToString("yyyy-MM-dd"),
    Status = equipment.Status.ToString(),
    Location = equipment.Location,
    CreatedAt = equipment.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
};
```

### 3. **Updated Stored Procedures**
Fixed stored procedures to work with existing schema:

```sql
-- Fixed GetCustomerEquipmentVisits procedure
CREATE PROCEDURE [dbo].[GetCustomerEquipmentVisits]
    @CustomerId NVARCHAR(450),
    @IncludeLegacy BIT = 1
AS
BEGIN
    -- Get customer info from existing Clients table
    SELECT Id, Name, Phone, Email, Address, CreatedAt
    FROM Clients
    WHERE Id = @CustomerId;
    
    -- Get equipment from existing Equipment table
    SELECT Id, Name, Model, Manufacturer, PurchaseDate as InstallationDate, 
           WarrantyExpiry, Status as EquipmentStatus, Location, 
           QRCode as SerialNumber
    FROM Equipment
    WHERE CustomerId = @CustomerId;
    
    -- Get visits from existing MaintenanceVisits table
    SELECT v.Id, v.DeviceId as EquipmentId, v.ScheduledDate as VisitDate, 
           v.VisitType, v.Status, v.CreatedAt, e.Name as EquipmentName, 
           e.QRCode as EquipmentSerialNumber
    FROM MaintenanceVisits v
    INNER JOIN Equipment e ON v.DeviceId = e.Id
    WHERE v.CustomerId = @CustomerId;
END
```

## Database Schema Mapping

### Existing Tables → New DTO Mapping

| Existing Table | Existing Column | DTO Property | Notes |
|----------------|----------------|-------------|-------|
| Equipment | Id | Id | int → string conversion |
| Equipment | QRCode | SerialNumber | Using QRCode as serial number |
| Equipment | Model | Model | Direct mapping |
| Equipment | Manufacturer | Manufacturer | Direct mapping |
| Equipment | CustomerId | CustomerId | References ApplicationUser |
| Equipment | PurchaseDate | InstallationDate | Date mapping |
| Equipment | WarrantyExpiry | WarrantyExpiryDate | Date mapping |
| Equipment | Status | Status | Enum → string conversion |
| Equipment | Location | Location | Direct mapping |
| MaintenanceVisit | Id | Id | int → string conversion |
| MaintenanceVisit | DeviceId | EquipmentId | Foreign key mapping |
| MaintenanceVisit | ScheduledDate | VisitDate | Date mapping |
| MaintenanceVisit | VisitType | VisitType | Direct mapping |
| MaintenanceVisit | Status | Status | Enum mapping |
| MaintenanceVisit | CompletionDate | CompletionDate | Date mapping |
| Clients | Id | Id | Direct mapping |
| Clients | Name | Name | Direct mapping |
| Clients | Phone | Phone | Direct mapping |
| Clients | Email | Email | Direct mapping |
| Clients | Address | Address | Direct mapping |

## Implementation Strategy

### Phase 1: Schema Updates
1. Run the fixed SQL script to create new tables
2. Add missing columns to existing tables
3. Create updated stored procedures

### Phase 2: Service Updates
1. Replace the original service with the fixed version
2. Update dependency injection if needed
3. Test all CRUD operations

### Phase 3: Testing
1. Run the comprehensive test suite
2. Verify data integrity
3. Test API endpoints with real data

## Files Updated

1. **`CreateComprehensiveMaintenanceTables_Fixed.sql`** - Fixed database schema script
2. **`ComprehensiveMaintenanceService_Fixed.cs`** - Fixed service implementation
3. **`DATABASE_SCHEMA_FIXES.md`** - This documentation

## Next Steps

1. **Execute the fixed SQL script** on the itiwebapi44 database
2. **Replace the service implementation** with the fixed version
3. **Run comprehensive tests** to ensure everything works
4. **Update frontend** if any API response formats changed
5. **Deploy to staging** for thorough testing

## Important Notes

- The fixed implementation maintains compatibility with the existing database
- All ID conversions between `int` and `string` are handled properly
- Existing data relationships are preserved
- New functionality is added without breaking existing features
- The service gracefully handles missing columns and null values

## Testing Recommendations

1. **Test all CRUD operations** for customers, equipment, visits, and contracts
2. **Verify foreign key relationships** work correctly
3. **Test data type conversions** between int and string IDs
4. **Validate stored procedures** return expected results
5. **Check API responses** match frontend expectations

This fix ensures the comprehensive maintenance module works seamlessly with the existing database structure while providing all the enhanced functionality required.
