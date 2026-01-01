# SoitMed Backend - Comprehensive Documentation

## Business Overview

SoitMed is a comprehensive medical equipment management and sales system designed to streamline operations for medical equipment suppliers, hospitals, clinics, and healthcare professionals.

### Core Business Functions

#### 1. Sales Module
- **Client Management**: Track and manage client information including hospitals, clinics, Doctors, and Technicians
- **Offer Management**: Create, manage, and track sales offers with detailed equipment specifications, pricing, and terms
- **Deal Processing**: Handle deal creation, approval workflows, and deal closure
- **SalesMan Management**: Track salesman activities, weekly plans, tasks, and performance statistics
- **Sales Reports**: Generate comprehensive sales reports and analytics

#### 2. Maintenance Module
- **Maintenance Requests**: Customers can submit maintenance requests for their equipment with descriptions, symptoms, and multimedia attachments
- **Visit Management**: Engineers can create maintenance visits, scan QR codes, and create detailed visit reports
- **Spare Parts Management**: Handle spare part requests with warehouse approval workflow
- **Engineer Assignment**: Automatic assignment of Engineers based on location and workload
- **Payment Processing**: Complete payment workflow with Cash and Gateway payment methods
- **Status Tracking**: Real-time status tracking for maintenance requests

#### 3. Payment Module
- **Multiple Payment Methods**: Support for Cash, Credit Card, Paymob, and other gateways
- **Payment Strategy Pattern**: Extensible architecture for future installment payments
- **Invoice Management**: Automatic invoice generation with cost breakdown
- **Accounting Management**: Accounting roles can confirm or reject payments

#### 4. Equipment Management
- **Equipment Registration**: Register equipment with QR codes, serial numbers, and detailed specifications
- **QR Code Management**: Generate and track QR codes for equipment
- **Equipment Linking**: Equipment can be linked to hospitals or directly to customers
- **Maintenance History**: Track maintenance history and repair visits for each equipment

#### 5. Legacy Data Integration
- **Legacy Data Import**: Import clients, equipment, and maintenance visits from legacy SOIT system
- **Legacy Media Files**: Serve legacy media files through integrated media API
- **Data Migration**: Comprehensive migration tools for historical data

---

## Technical Architecture

### Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Real-time Communication**: SignalR for notifications
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Repository Pattern with Unit of Work
- **Payment Integration**: Paymob Egypt payment gateway

### Project Structure
```
SoitMed/
├── Controllers/          # API endpoint controllers
├── Services/            # Business logic services
│   └── Payment/        # Payment strategy implementations
├── Repositories/        # Data access layer
├── Models/             # Entity Framework models
│   ├── Equipment/      # Maintenance-related entities
│   ├── Payment/        # Payment entities
│   └── Legacy/         # Legacy system models
├── DTO/                # Data Transfer Objects
├── Common/             # Shared utilities and helpers
├── Integrations/       # Third-party integrations
│   └── Paymob/         # Paymob payment gateway DTOs
├── Documentation/      # Technical documentation
└── Scripts/            # Database migration scripts
```

---

## Key Features Implementation

### 1. Maintenance Request & Lifecycle Module

Complete end-to-end workflow for maintenance requests from creation to payment closure.

#### Workflow
1. **Request Creation**: Customer or Call Center creates ticket with description and images
2. **Assignment**: Admin/Coordinator assigns Engineer
3. **QR Verification**: Engineer scans QR code to verify arrival
4. **Diagnosis**: Engineer diagnoses the issue
5. **Spare Parts Request**: Engineer requests parts if needed
6. **Warehouse Approval**: Warehouse Keeper approves/rejects parts in Dashboard
7. **Customer Approval**: Customer approves part pricing
8. **Cost Calculation**: System calculates Total Cost (Labor Fees + Approved Spare Parts)
9. **Payment Processing**: Engineer collects payment (Cash or Gateway)
10. **Closure**: Ticket marked as completed

#### Database Schema
- **MaintenanceRequest**: Extended with `LaborFees`, future installment fields
- **SparePartRequest**: Added warehouse approval workflow fields
- **Invoice**: New entity for invoice management with cost breakdown
- **PaymentTransaction**: Enhanced with payment method and status tracking

#### API Endpoints
- `POST /api/MaintenanceRequest` - Create maintenance request
- `POST /api/MaintenanceRequest/{id}/assign` - Assign engineer
- `POST /api/SparePartRequest` - Request spare parts (Engineer)
- `POST /api/SparePartRequest/{id}/warehouse-approval` - Approve/reject parts (Warehouse)
- `POST /api/MaintenanceRequest/{id}/finalize-job` - Finalize job and process payment

**See**: `Documentation/MAINTENANCE_LIFECYCLE_IMPLEMENTATION.md` for complete details

### 2. Payment Strategy Pattern

Extensible payment processing architecture using Strategy Pattern for future installment support.

#### Implemented Strategies
- **CashPaymentStrategy**: Handles cash payments (requires manual confirmation)
- **VisaPaymentStrategy**: Handles credit card and gateway payments (Paymob, Stripe, etc.)
- **InstallmentPaymentStrategy**: Reserved for future implementation

#### Architecture Benefits
- Easy to add new payment methods
- Installment payment support ready (database fields and strategy class exist)
- Clean separation of concerns
- Testable payment processing logic

**Location**: `Services/Payment/`

### 3. Legacy Data Integration

Comprehensive integration with legacy SOIT system for data migration and media file access.

#### Legacy Data Import
- **Service**: `LegacyImporterService`
- **Models**: `LegacyCustomer`, `LegacyOrderOutItem`, `LegacyMaintenanceVisit`, `LegacyMaintenanceContract`
- **Features**:
  - Import clients from `Stk_Customers`
  - Import equipment from `Stk_Order_Out_Items`
  - Import maintenance visits from `MNT_Visiting`
  - Automatic QR token generation for equipment
  - Client-to-User linking support
  - File-based error logging

#### Legacy Media Files Integration
- **Service**: `LegacyMediaService`
- **Controller**: `LegacyMediaController`
- **Integration**: Proxies requests to legacy media API (`soitmed_data_backend`)
- **Media Paths**:
  - `D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports`
  - `D:\Soit-Med\legacy\SOIT\UploadFiles\Images`
  - `D:\Soit-Med\legacy\SOIT\UploadFiles\Files`

#### API Endpoints
- `GET /api/LegacyMedia/files/{fileName}` - Serve legacy media files (proxy)
- `GET /api/LegacyMedia/url/{fileName}` - Get media file URL
- `GET /api/LegacyMedia/check/{fileName}` - Check if file exists
- `POST /api/LegacyImport/import-all` - Import all legacy data
- `POST /api/LegacyImport/import-clients` - Import clients only
- `POST /api/LegacyImport/import-equipment` - Import equipment only
- `POST /api/LegacyImport/import-visits` - Import maintenance visits only

**See**: `Documentation/LEGACY_MEDIA_INTEGRATION.md` for complete details

### 4. Connection Settings (Local/Remote Modes)

Flexible connection management for local development and remote server access.

#### Configuration
```json
{
  "ConnectionSettings": {
    "Mode": "Remote",  // "Local" or "Remote"
    "LocalConnectionString": "...",
    "RemoteConnectionString": "...",
    "LocalMediaPath": "C:\\LegacyMedia",
    "RemoteMediaPath": "\\\\10.10.9.104\\LegacyMedia",
    "LegacyMediaApiBaseUrl": "http://10.10.9.104:5266",
    "LegacyMediaPaths": "D:\\Soit-Med\\legacy\\SOIT\\..."
  }
}
```

#### Usage
- Switch between Local and Remote modes by changing `Mode` in appsettings.json
- Automatic connection string selection based on mode
- Media file path selection based on mode
- Legacy media API URL configuration

**Location**: `Common/ConnectionSettings.cs`

### 5. Admin & Support Capabilities

#### Admin: Client-to-User Linking
- `POST /api/Admin/Client/LinkAccount` - Link Client to User account
- `POST /api/Admin/Client/UnlinkAccount` - Unlink Client from User account
- Updates `Client.RelatedUserId` field
- Validates Client and User existence

#### Support: QR Code Management
- `POST /api/Equipment/Qr/MarkPrinted` - Mark QR code as printed
- Updates `Equipment.IsQrPrinted` and `Equipment.QrLastPrintedDate`
- Role: Support, Admin, SuperAdmin

#### DTO Updates
- **ClientResponseDTO**: Added `HasAccount` (bool) - indicates if client is linked to user
- **EquipmentResponseDTO**: Added `QrToken` (Guid) and `IsQrPrinted` (bool)

### 6. Paymob Payment Gateway Integration

Complete Paymob Egypt payment gateway integration with DTOs and configuration.

#### Configuration
```json
{
  "Paymob": {
    "ApiKey": "",
    "HmacSecret": "",
    "IntegrationId_Card": "",
    "IntegrationId_Wallet": "",
    "IntegrationId_Fawry": "",
    "IframeId": ""
  }
}
```

#### DTOs
All DTOs use `[JsonPropertyName("snake_case")]` to match Paymob API:
- `PaymobAuthResponse` - Authentication response
- `PaymobOrderRequest/Response` - Order creation
- `PaymobKeyRequest/Response` - Payment key generation
- `PaymobPayRequest/Response` - Payment processing
- `PaymobBillingData` - Billing information
- `PaymobPaymentSource` - Wallet/Fawry payment source

**Location**: `Integrations/Paymob/DTOs/`

---

## Database Schema

### Key Entities

#### MaintenanceRequest
- Core maintenance request entity
- Fields: `LaborFees`, `TotalAmount`, `PaidAmount`, `RemainingAmount`
- Future-proofing: `PaymentPlan?`, `InstallmentMonths?`, `CollectionDelegateId?`

#### SparePartRequest
- Spare part request with warehouse approval workflow
- Fields: `WarehouseApproved`, `ApprovedByWarehouseKeeperId`, `WarehouseApprovedAt`
- Pricing: `OriginalPrice`, `CompanyPrice`, `CustomerPrice`

#### Invoice
- Invoice entity for maintenance requests
- Cost breakdown: `LaborFees`, `SparePartsTotal`, `TotalAmount`
- Payment tracking: `PaidAmount`, `RemainingAmount`, `Status`
- Future-proofing: `PaymentPlan?`, `InstallmentMonths?`, `CollectionDelegateId?`

#### Equipment
- Equipment with QR code support
- Fields: `QrToken` (unique), `IsQrPrinted`, `QrLastPrintedDate`
- Legacy support: `LegacySourceId`

#### Client
- Client entity with user linking
- Fields: `RelatedUserId` (FK to ApplicationUser), `LegacyCustomerId`
- DTO includes: `HasAccount` (computed property)

### Enums

#### PaymentMethod
- `Cash = 1`, `BankTransfer = 2`, `CreditCard = 3`, `DebitCard = 4`
- `Stripe = 5`, `PayPal = 6`, `Fawry = 7`, `Paymob = 8`
- `MobileWallet = 9`, `Check = 10`, `CompanyCredit = 11`
- `Delegate = 12`, `Gateway = 13`, `Installment = 14` (reserved)

#### PaymentPlan
- `OneTime = 1`, `Installment = 2` (reserved)

#### PaymentStatus
- `NotRequired = 0`, `Pending = 1`, `Processing = 2`, `Completed = 3`
- `Failed = 4`, `Cancelled = 5`, `Refunded = 6`
- `Unpaid = 7`, `PendingCollection = 8`, `Collected = 9`, `PaidOnline = 10`

---

## API Endpoints

### Maintenance Module
- `POST /api/MaintenanceRequest` - Create request
- `GET /api/MaintenanceRequest/{id}` - Get request details
- `POST /api/MaintenanceRequest/{id}/assign` - Assign engineer
- `POST /api/MaintenanceRequest/{id}/finalize-job` - Finalize job and process payment
- `POST /api/SparePartRequest` - Request spare parts
- `POST /api/SparePartRequest/{id}/warehouse-approval` - Warehouse approval
- `POST /api/SparePartRequest/{id}/customer-decision` - Customer approval

### Admin Module
- `POST /api/Admin/Client/LinkAccount` - Link client to user
- `POST /api/Admin/Client/UnlinkAccount` - Unlink client from user

### Equipment Module
- `POST /api/Equipment/Qr/MarkPrinted` - Mark QR as printed
- `GET /api/Equipment` - Get all equipment
- `GET /api/Equipment/{id}` - Get equipment details

### Legacy Integration
- `POST /api/LegacyImport/import-all` - Import all legacy data
- `POST /api/LegacyImport/import-clients` - Import clients
- `POST /api/LegacyImport/import-equipment` - Import equipment
- `POST /api/LegacyImport/import-visits` - Import maintenance visits
- `GET /api/LegacyMedia/files/{fileName}` - Serve legacy media files
- `GET /api/LegacyMedia/url/{fileName}` - Get media file URL
- `GET /api/LegacyMedia/check/{fileName}` - Check file existence

### Payment Module
- `POST /api/Payment` - Create payment
- `POST /api/Payment/{id}/stripe` - Process Stripe payment
- `POST /api/Payment/{id}/paypal` - Process PayPal payment
- `POST /api/Payment/{id}/cash` - Record cash payment
- `POST /api/Accounting/{paymentId}/confirm` - Confirm payment
- `POST /api/Accounting/{paymentId}/reject` - Reject payment

---

## Configuration

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "Redis": "localhost:6379"
  },
  "ConnectionSettings": {
    "Mode": "Remote",
    "LocalConnectionString": "...",
    "RemoteConnectionString": "...",
    "LocalMediaPath": "C:\\LegacyMedia",
    "RemoteMediaPath": "\\\\10.10.9.104\\LegacyMedia",
    "LegacyMediaApiBaseUrl": "http://10.10.9.104:5266",
    "LegacyMediaPaths": "D:\\Soit-Med\\legacy\\SOIT\\..."
  },
  "Paymob": {
    "ApiKey": "",
    "HmacSecret": "",
    "IntegrationId_Card": "",
    "IntegrationId_Wallet": "",
    "IntegrationId_Fawry": "",
    "IframeId": ""
  },
  "ContractMaintenance": {
    "ScheduleDaysAhead": 7,
    "RunHour": 2
  },
  "JWT": {
    "ValidIss": "http://localhost:58868",
    "ValidAud": "http://localhost:4200",
    "SecritKey": "..."
  }
}
```

---

## Database Migrations

### Key Migrations
- `AddLegacyAndQrSupport` - Adds QR token, legacy fields to Equipment and Client
- `MaintenanceLifecycleModule_Migration` - Complete maintenance lifecycle schema
- Manual SQL scripts available in `Scripts/` directory

### Migration Commands
```bash
# Create migration
dotnet ef migrations add MigrationName --project SoitMed

# Apply migration
dotnet ef database update --project SoitMed

# Generate SQL script
dotnet ef migrations script --project SoitMed
```

---

## Development Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (Local or Remote)
- Visual Studio 2022 or VS Code
- Redis (optional, for distributed caching)

### Setup Steps
1. Clone repository
2. Update `appsettings.json` with connection strings
3. Restore NuGet packages: `dotnet restore`
4. Apply database migrations: `dotnet ef database update`
5. Run application: `dotnet run`

### Swagger Documentation
Access Swagger UI at: `http://localhost:58868/swagger`

---

## Important Notes for Future Development

### 1. Installment Payment Support (Reserved)
- **Database Fields**: Already added to `MaintenanceRequest` and `Invoice` entities
  - `PaymentPlan?` (enum)
  - `InstallmentMonths?` (int)
  - `CollectionDelegateId?` (FK to ApplicationUser)
- **Code Structure**: `InstallmentPaymentStrategy` class exists but returns error
- **Enum**: `PaymentMethod.Installment` exists but not used in UI
- **To Implement**: 
  - Implement `InstallmentPaymentStrategy.ProcessPaymentAsync()`
  - Create `InstallmentSchedule` entity
  - Add monthly payment tracking
  - Add collection delegate assignment
  - Update UI to show installment option

### 2. Legacy Media Files
- **Legacy Media API**: Separate API project (`soitmed_data_backend`) serves legacy files
- **Integration**: Main backend proxies requests to legacy API
- **File Paths**: Configured in `ConnectionSettings.LegacyMediaPaths`
- **Media API URL**: Configured in `ConnectionSettings.LegacyMediaApiBaseUrl`
- **Important**: Ensure legacy media API is running and accessible

### 3. Connection Settings
- **Mode Switching**: Change `ConnectionSettings.Mode` to switch between Local/Remote
- **Media Paths**: Different paths for local vs remote access
- **Legacy API**: URL must be accessible from main backend server

### 4. Payment Gateway Integration
- **Paymob**: DTOs ready, configuration needed in appsettings.json
- **Strategy Pattern**: Easy to add new payment gateways
- **Current Support**: Cash and Gateway (Credit Card) payments only
- **Future**: Installment payments reserved but not implemented

### 5. Warehouse Approval Workflow
- **Required Step**: Warehouse Keeper must approve spare parts before engineer can proceed
- **Status Flow**: `Checking` → `ReadyForEngineer` (if approved) or `Cancelled` (if rejected)
- **Notification**: Engineer notified of approval/rejection decision

### 6. Cost Calculation
- **Formula**: `TotalAmount = LaborFees + Sum(Approved Spare Parts CustomerPrice)`
- **Approved Parts Only**: Only spare parts with `WarehouseApproved = true` and `CustomerApproved = true` are included
- **Invoice**: Automatically created when job is finalized

---

## Session Summary: Recent Implementation

### Implemented Features (This Session)

#### 1. Maintenance Request & Lifecycle Module
- Complete workflow from request creation to payment closure
- Warehouse approval workflow for spare parts
- Cost calculation (Labor Fees + Approved Spare Parts)
- Payment processing with Strategy Pattern
- Invoice generation and tracking

#### 2. Payment Strategy Pattern
- `IPaymentStrategy` interface for extensible payment processing
- `CashPaymentStrategy` for cash payments
- `VisaPaymentStrategy` for gateway payments
- `InstallmentPaymentStrategy` reserved for future
- `PaymentStrategyFactory` for automatic strategy selection

#### 3. Legacy Data Integration
- `LegacyImporterService` for importing clients, equipment, and visits
- Support for Local/Remote connection modes
- File-based error logging
- Automatic QR token generation
- Client-to-User linking support

#### 4. Legacy Media Files Integration
- `LegacyMediaService` for accessing legacy media files
- Proxy endpoints to legacy media API
- File existence checking
- URL generation for media files
- Integration with `soitmed_data_backend` API

#### 5. Admin & Support Capabilities
- Client-to-User account linking
- QR code print status tracking
- Enhanced DTOs with new fields

#### 6. Paymob Payment Gateway
- Complete DTO structure with snake_case JSON properties
- Configuration support in appsettings.json
- Ready for API integration

#### 7. Database Schema Updates
- `Invoice` entity for invoice management
- Extended `MaintenanceRequest` with labor fees and installment fields
- Extended `SparePartRequest` with warehouse approval fields
- Extended `Equipment` with QR code fields
- Extended `Client` with user linking fields
- Future-proofing fields for installment payments

### Key Architectural Decisions

1. **Strategy Pattern for Payments**: Allows easy extension for installment payments without modifying existing code
2. **Future-Proofing**: Database fields and code structure ready for installment payments (not implemented yet)
3. **Legacy Integration**: Separate service layer for legacy data and media access
4. **Connection Flexibility**: Support for both local and remote development modes
5. **Warehouse Workflow**: Required approval step ensures proper inventory management

### Files Created/Modified

#### New Files
- `Services/Payment/IPaymentStrategy.cs`
- `Services/Payment/CashPaymentStrategy.cs`
- `Services/Payment/VisaPaymentStrategy.cs`
- `Services/Payment/InstallmentPaymentStrategy.cs`
- `Services/Payment/PaymentStrategyFactory.cs`
- `Services/ILegacyMediaService.cs`
- `Services/LegacyMediaService.cs`
- `Controllers/LegacyMediaController.cs`
- `Models/Payment/Invoice.cs`
- `Models/Enums/PaymentPlan.cs`
- `Integrations/Paymob/DTOs/*` (11 DTO files)
- `Documentation/MAINTENANCE_LIFECYCLE_IMPLEMENTATION.md`
- `Documentation/LEGACY_MEDIA_INTEGRATION.md`

#### Modified Files
- `Models/Equipment/MaintenanceRequest.cs` - Added labor fees and installment fields
- `Models/Equipment/SparePartRequest.cs` - Added warehouse approval fields
- `Models/Equipment/Equipment.cs` - Added QR code fields
- `Models/Client.cs` - Added user linking fields
- `Models/Enums/PaymentMethod.cs` - Added Installment option
- `Services/MaintenanceRequestService.cs` - Added FinalizeJobAndProcessPaymentAsync
- `Services/SparePartRequestService.cs` - Added WarehouseApprovalAsync
- `Controllers/MaintenanceRequestController.cs` - Added finalize-job endpoint
- `Controllers/SparePartRequestController.cs` - Added warehouse-approval endpoint
- `Controllers/AdminController.cs` - Added client linking endpoints
- `Controllers/EquipmentController.cs` - Added QR mark-printed endpoint
- `Common/ConnectionSettings.cs` - Added legacy media API configuration
- `DTO/MaintenanceDTOs.cs` - Added FinalizeJobDTO and WarehouseApprovalDTO
- `DTO/ClientDTOs.cs` - Added HasAccount to ClientResponseDTO
- `DTO/EquipmentDTO.cs` - Added QrToken and IsQrPrinted to EquipmentResponseDTO

### Testing Checklist
- [ ] Create maintenance request
- [ ] Assign engineer
- [ ] Request spare parts
- [ ] Warehouse approves parts
- [ ] Warehouse rejects parts
- [ ] Customer approves parts
- [ ] Finalize job with Cash payment
- [ ] Finalize job with Gateway payment
- [ ] Verify invoice creation
- [ ] Verify payment transaction recording
- [ ] Test legacy data import
- [ ] Test legacy media file access
- [ ] Test client-to-user linking
- [ ] Test QR code marking

---

## Performance & Caching

### Caching Layer
- **Redis Support**: Distributed caching with Redis
- **Memory Fallback**: Automatic fallback to in-memory cache
- **Cache Keys**: Centralized in `CacheKeys.cs`
- **Cache Duration**: Configurable per data type (2-24 hours)

### Database Optimization
- **Connection Pooling**: Max 300, Min 20 connections
- **Performance Indexes**: Comprehensive indexes on frequently queried fields
- **AsNoTracking**: Used for read-only legacy data queries

---

## Security

### Authentication & Authorization
- JWT token-based authentication
- Role-based authorization with `[Authorize(Roles = "...")]`
- Custom authorization helpers for complex scenarios

### Input Validation
- Data annotations on DTOs
- FluentValidation for complex validations
- Custom validation extensions

### File Security
- Path traversal protection for file access
- File type and size validation
- Secure file upload handling

---

## Documentation

### Technical Documentation
- `Documentation/MAINTENANCE_LIFECYCLE_IMPLEMENTATION.md` - Complete maintenance lifecycle guide
- `Documentation/LEGACY_MEDIA_INTEGRATION.md` - Legacy media files integration guide
- `Documentation/API_DOCUMENTATION_REACT_TEAMS.md` - API reference for React teams
- Additional documentation files in `Documentation/` directory

### Code Documentation
- XML comments on all public APIs
- Inline comments for complex business logic
- README files for major modules

---

## Deployment

### Pre-Deployment Checklist
1. Update `appsettings.json` with production connection strings
2. Configure `ConnectionSettings.Mode` (Remote for production)
3. Set `LegacyMediaApiBaseUrl` to production legacy API URL
4. Configure Paymob credentials in `Paymob` section
5. Apply database migrations
6. Verify legacy media API is accessible
7. Test all critical endpoints
8. Configure Redis for distributed caching (if using)

### Environment Variables
Consider using environment variables for sensitive configuration:
- Database connection strings
- JWT secret key
- Payment gateway credentials
- Legacy media API URL

---

## Troubleshooting

### Common Issues

#### Legacy Media Files Not Found
1. Verify legacy media API is running
2. Check `LegacyMediaApiBaseUrl` in appsettings.json
3. Verify network connectivity to legacy API server
4. Check file paths in legacy file system

#### Payment Processing Errors
1. Verify payment gateway credentials
2. Check payment strategy registration in DI container
3. Review payment transaction logs
4. Verify invoice creation succeeded

#### Database Connection Issues
1. Check connection string format
2. Verify SQL Server is accessible
3. Check firewall rules
4. Verify connection pool settings

---

## Future Enhancements

### Planned Features
1. **Installment Payments**: Complete implementation of installment payment strategy
2. **Collection Delegates**: Assignment and tracking of collection delegates
3. **File Migration**: Option to migrate legacy files to new storage
4. **Thumbnail Generation**: Automatic thumbnail generation for images
5. **CDN Integration**: Serve media files through CDN
6. **Advanced Analytics**: Enhanced reporting and analytics dashboard

### Reserved for Future
- Installment payment processing logic
- Collection delegate assignment workflow
- Monthly payment tracking and reminders
- Installment schedule management

---

## License

Proprietary - SoitMed Medical Equipment Management System

---

## Support

For technical support or questions, refer to:
- Technical documentation in `Documentation/` directory
- API documentation via Swagger UI
- Code comments and XML documentation
