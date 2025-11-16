# SoitMed Backend - Maintenance Module Branch

## Business Overview

The Maintenance Module provides a complete solution for managing equipment maintenance requests from initial customer submission through resolution. It streamlines the maintenance workflow, coordinates between customers, engineers, and support staff, and ensures efficient spare parts management.

### Business Functions

#### 1. Maintenance Request Management
- **Request Submission**: Customers (doctors, technicians, institution managers) can submit maintenance requests through mobile app with:
  - Equipment selection (linked to hospital or customer)
  - Detailed problem description
  - Symptoms and observations
  - Multimedia attachments (images, videos, audio recordings)
- **Request Tracking**: Real-time status tracking similar to delivery systems:
  - Pending: Request submitted, awaiting assignment
  - Assigned: Assigned to engineer
  - In Progress: Engineer working on request
  - Needs Second Visit: Requires follow-up visit
  - Needs Spare Part: Waiting for spare part
  - Waiting For Spare Part: Spare part ordered
  - Waiting For Customer Approval: Customer needs to approve spare part price
  - Completed: Maintenance completed successfully
  - Cancelled: Request cancelled
  - On Hold: Temporarily paused
- **Request History**: Complete history of all maintenance requests with status changes

#### 2. Engineer Visit Management
- **Visit Creation**: Engineers create maintenance visits for assigned requests
- **Equipment Identification**: 
  - QR Code scanning to load equipment data
  - Manual serial number entry if QR code unavailable
- **Visit Reporting**: Engineers create detailed visit reports with:
  - Visit outcome (Completed, Needs Second Visit, Needs Spare Part)
  - Detailed notes and observations
  - Work performed
  - Recommendations
- **Visit History**: Track all visits for each maintenance request

#### 3. Spare Parts Management
- **Spare Part Requests**: Engineers can request spare parts during visits
- **Availability Checking**: 
  - Local availability check by Spare Parts Coordinator
  - Global availability check if not available locally
- **Pricing Workflow**:
  - If local: Inventory Manager prepares part for engineer
  - If global: Maintenance Manager sets price (original price + company revenue)
  - Customer receives notification with price
  - Customer approves or rejects spare part
- **Spare Part Tracking**: Track spare part status from request to delivery

#### 4. Engineer Assignment
- **Automatic Assignment**: 
  - Based on equipment location (hospital location)
  - Engineer governorate assignments
  - Current workload (least active requests)
- **Manual Assignment**: Maintenance Support can manually assign engineers
- **Assignment Notifications**: Engineers receive notifications when assigned

#### 5. Customer Rating System
- **Engineer Rating**: Customers can rate engineers after service completion
- **Rating Tracking**: Track and display engineer ratings
- **Quality Assurance**: Use ratings for engineer performance evaluation

#### 6. File Attachments
- **Multimedia Support**: 
  - Images (JPG, PNG, GIF, BMP, WEBP) - up to 10MB
  - Videos (MP4, AVI, MOV, WMV, FLV, MKV, WEBM) - up to 100MB
  - Audio (MP3, WAV, OGG, M4A, AAC, WMA) - up to 20MB
  - Documents (PDF, DOC, DOCX, TXT, XLS, XLSX) - up to 10MB
- **Attachment Management**: Upload, view, and delete attachments
- **Attachment Organization**: Attachments organized by maintenance request

### Business Workflows

#### Standard Maintenance Workflow
1. Customer submits maintenance request with equipment, description, and attachments
2. Request appears in Maintenance Support dashboard
3. System attempts automatic engineer assignment based on location
4. If auto-assignment fails, Maintenance Support manually assigns engineer
5. Engineer receives notification and creates maintenance visit
6. Engineer scans QR code or enters serial number to load equipment data
7. Engineer performs maintenance and creates visit report
8. Engineer marks outcome: Completed, Needs Second Visit, or Needs Spare Part
9. If completed: Customer can rate engineer
10. If needs spare part: Spare parts workflow initiated

#### Spare Parts Workflow
1. Engineer requests spare part during visit
2. Spare Parts Coordinator checks availability
3. **If Local Available**:
   - Coordinator marks as local available
   - Inventory Manager prepares part
   - Part marked as ready for engineer
   - Engineer notified to pick up part
4. **If Global Required**:
   - Coordinator marks as global required
   - Maintenance Manager receives notification
   - Manager sets customer price (original + company revenue)
   - Customer receives notification with price
   - Customer approves or rejects
   - If approved: Part ordered and delivered
   - If rejected: Request marked as completed without spare part

#### Equipment Linking
- Equipment can be linked to:
  - **Hospital**: Equipment belongs to hospital, accessible to all hospital staff
  - **Customer (Doctor)**: Equipment directly linked to doctor, even if doctor is in hospital
- System automatically determines hospital from equipment or doctor's hospital associations

### Business Benefits

- **Efficient Coordination**: Automated workflows reduce manual coordination effort
- **Real-time Tracking**: Customers can track request status in real-time
- **Quick Response**: Automatic engineer assignment ensures quick response times
- **Transparent Pricing**: Clear spare part pricing with customer approval
- **Quality Assurance**: Rating system ensures service quality
- **Complete Documentation**: All requests, visits, and attachments documented
- **Flexible Equipment Linking**: Supports both hospital and individual customer equipment

---

## Technical Implementation

### Architecture

#### Models
- **MaintenanceRequest**: Core maintenance request entity with status tracking
- **MaintenanceVisit**: Engineer visit records with outcomes
- **MaintenanceRequestAttachment**: File attachments with type and metadata
- **SparePartRequest**: Spare part requests with availability status
- **MaintenanceRequestRating**: Customer ratings for engineers
- **Equipment**: Enhanced to support customer linking (HospitalId or CustomerId)

#### Enums
- **MaintenanceRequestStatus**: Request status enumeration
- **MaintenanceVisitOutcome**: Visit outcome types
- **SparePartAvailabilityStatus**: Spare part availability states
- **AttachmentType**: File attachment types (Image, Video, Audio, Document)

#### Repositories
- **MaintenanceRequestRepository**: Request data access with custom queries
- **MaintenanceVisitRepository**: Visit management operations
- **MaintenanceRequestAttachmentRepository**: Attachment file management
- **SparePartRequestRepository**: Spare part request operations

#### Services
- **MaintenanceRequestService**: 
  - Request creation and management
  - Auto-assignment logic based on location and workload
  - Status tracking and updates
- **MaintenanceVisitService**: 
  - Visit creation and reporting
  - Equipment data loading (QR code or serial number)
  - Outcome processing
- **MaintenanceAttachmentService**: 
  - File upload handling
  - File type validation
  - File size limits enforcement
  - File storage management
- **SparePartRequestService**: 
  - Spare part request creation
  - Availability checking workflow
  - Price setting and customer approval

#### Controllers
- **MaintenanceRequestController**: Request CRUD and assignment operations
- **MaintenanceVisitController**: Visit creation and management
- **MaintenanceAttachmentController**: File upload and management
- **SparePartRequestController**: Spare part request operations

### Key Features Implementation

#### Auto-Assignment Algorithm
```csharp
1. Get equipment location (from hospital or customer)
2. Find all active engineers
3. Filter engineers by governorate matching equipment location
4. Get current workload for each engineer (active requests count)
5. Select engineer with least workload
6. Assign request and notify engineer
```

#### File Upload Service
- **Storage Location**: `wwwroot/maintenance-requests/{requestId}/attachments/`
- **File Naming**: Unique GUID-based filenames to prevent conflicts
- **Validation**: 
  - File type validation based on AttachmentType
  - File size limits per type
  - MIME type checking
- **Security**: File type whitelist, size limits, path validation

#### QR Code Integration
- Equipment QR codes stored in Equipment table
- QR code scanning loads complete equipment data
- Fallback to serial number entry if QR unavailable
- Equipment data includes: Name, Model, Manufacturer, Purchase Date, Warranty, etc.

#### Notification Integration
- Notifications sent at each workflow step:
  - Request creation → Maintenance Support (role group)
  - Assignment → Engineer (individual)
  - Visit completion → Maintenance Support (role group)
  - Spare part request → Coordinator (role group)
  - Price setting → Customer (individual)
  - Part ready → Engineer (individual)
- Uses SignalR for real-time notifications
- Mobile push notifications for critical updates

### API Endpoints

#### Maintenance Requests
- `GET /api/MaintenanceRequest` - Get all requests (role-filtered)
- `POST /api/MaintenanceRequest` - Create new request
- `GET /api/MaintenanceRequest/{id}` - Get request details
- `GET /api/MaintenanceRequest/customer/{customerId}` - Get customer requests
- `PUT /api/MaintenanceRequest/{id}/assign` - Assign to engineer
- `PUT /api/MaintenanceRequest/{id}/status` - Update status

#### Maintenance Visits
- `POST /api/MaintenanceVisit` - Create visit
- `GET /api/MaintenanceVisit/request/{requestId}` - Get visits for request
- `GET /api/MaintenanceVisit/engineer/{engineerId}` - Get engineer visits

#### Attachments
- `POST /api/MaintenanceAttachment/upload` - Upload attachment
- `GET /api/MaintenanceAttachment/{id}` - Get attachment
- `GET /api/MaintenanceAttachment/request/{requestId}` - Get request attachments
- `DELETE /api/MaintenanceAttachment/{id}` - Delete attachment

#### Spare Parts
- `POST /api/SparePartRequest` - Create spare part request
- `GET /api/SparePartRequest/{id}` - Get spare part request
- `PUT /api/SparePartRequest/{id}/check-availability` - Check availability
- `PUT /api/SparePartRequest/{id}/set-price` - Set customer price
- `PUT /api/SparePartRequest/{id}/customer-approval` - Customer approval/rejection

### Database Design

#### Key Tables
- **MaintenanceRequests**: Core request table with status, assignment, and payment tracking
- **MaintenanceVisits**: Visit records linked to requests
- **MaintenanceRequestAttachments**: File attachments with metadata
- **SparePartRequests**: Spare part requests with availability and pricing
- **MaintenanceRequestRatings**: Customer ratings
- **Equipment**: Enhanced with CustomerId for direct customer linking

#### Relationships
- MaintenanceRequest → Equipment (Many-to-One)
- MaintenanceRequest → Customer (Many-to-One)
- MaintenanceRequest → Engineer (Many-to-One, nullable)
- MaintenanceVisit → MaintenanceRequest (Many-to-One)
- MaintenanceRequestAttachment → MaintenanceRequest (Many-to-One)
- SparePartRequest → MaintenanceRequest (One-to-One)
- MaintenanceRequestRating → MaintenanceRequest (Many-to-One)

#### Constraints
- Equipment must be linked to either Hospital OR Customer (check constraint)
- Foreign keys configured with appropriate cascade behaviors
- Status transitions enforced at service layer

### Integration Points

- **Payment Module**: Payments linked to maintenance requests and spare parts
- **Equipment Module**: Equipment data loading via QR code or serial number
- **User Management**: Role-based access for all maintenance roles
- **Notification System**: Real-time notifications throughout workflow

### Security

- **Role-Based Access**: 
  - Customers: Create requests, view own requests, rate engineers
  - Engineers: View assigned requests, create visits, request spare parts
  - Maintenance Support: View all requests, assign engineers
  - Coordinators: Check spare part availability
  - Managers: Set prices, approve purchases
- **File Upload Security**: File type validation, size limits, path sanitization
- **Data Isolation**: Customers can only see their own requests
- **Audit Trail**: Complete logging of all status changes and assignments

### Performance Considerations

- **Indexed Queries**: Database indexes on frequently queried fields (Status, CustomerId, EngineerId)
- **Eager Loading**: Optimized data loading with Include statements
- **File Storage**: Efficient file storage with organized directory structure
- **Caching**: Strategic caching for equipment data and engineer lists

### Future Enhancements

- Scheduled maintenance reminders
- Maintenance history analytics
- Predictive maintenance based on equipment age and usage
- Integration with inventory management system
- Mobile app optimizations for field engineers


