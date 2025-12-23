# SoitMed Backend - Dev Branch

## Business Overview

SoitMed is a comprehensive medical equipment management and sales system designed to streamline operations for medical equipment suppliers, hospitals, clinics, and healthcare professionals.

### Core Business Functions

#### 1. Sales Module

The sales module manages the complete sales lifecycle from initial client contact to deal closure:

- **Client Management**: Track and manage client information including hospitals, clinics, Doctors, and Technicians
- **Offer Management**: Create, manage, and track sales offers with detailed equipment specifications, pricing, and terms
- **Deal Processing**: Handle deal creation, approval workflows, and deal closure
- **SalesMan Management**: Track salesman activities, weekly plans, tasks, and performance statistics
- **Sales Reports**: Generate comprehensive sales reports and analytics

#### 2. Maintenance Module

The maintenance module handles the complete lifecycle of equipment maintenance requests:

- **Maintenance Requests**: Customers can submit maintenance requests for their equipment with descriptions, symptoms, and multimedia attachments (images, videos, audio)
- **Visit Management**: Engineers can create maintenance visits, scan QR codes or enter serial numbers to load equipment data, and create detailed visit reports
- **Spare Parts Management**: Handle spare part requests with local/global availability checks, pricing by maintenance managers, and customer approval workflows
- **Engineer Assignment**: Automatic assignment of Engineers based on location and workload, with manual assignment options
- **Customer Rating**: Customers can rate Engineers after service completion
- **Status Tracking**: Real-time status tracking similar to delivery systems, allowing customers to see the current state of their maintenance requests

#### 3. Payment Module

The payment module manages all financial transactions:

- **Multiple Payment Methods**: Support for various payment methods including payment gateways (Stripe, PayPal, local gateways), cash, and bank transfers
- **Payment Processing**: Secure payment processing with transaction tracking
- **Accounting Management**: Accounting roles can confirm or reject payments with detailed notes
- **Payment Tracking**: Track payment status, amounts, and transaction history

#### 4. Equipment Management

Comprehensive equipment tracking and management:

- **Equipment Registration**: Register equipment with QR codes, serial numbers, and detailed specifications
- **Equipment Linking**: Equipment can be linked to hospitals or directly to customers (Doctors)
- **Maintenance History**: Track maintenance history and repair visits for each equipment
- **Equipment Status**: Monitor equipment operational status

#### 5. User Management

Role-based access control with multiple user types:

- **Doctors**: Can submit maintenance requests, view equipment, and manage their profile
- **Technicians**: Similar to Doctors with Technician-specific permissions
- **Engineers**: Handle maintenance visits, create reports, and manage spare part requests
- **Salesmen**: Manage clients, create offers, track deals, and manage weekly plans
- **Sales Support**: Support sales operations, manage offer requests, and handle client interactions
- **Maintenance Support**: Coordinate maintenance requests, assign Engineers, and manage workflows
- **Maintenance Manager**: Set spare part prices, approve global purchases, and manage maintenance operations
- **Spare Parts Coordinator**: Check spare part availability (local/global) and coordinate with inventory managers
- **Inventory Manager**: Prepare spare parts for Engineers when available locally
- **Finance Manager & Finance Employee**: Handle payment confirmations, rejections, and accounting operations
- **Super Admin**: Full system access and oversight
- **Admin**: Administrative access with tracking capabilities

#### 6. Notification System

Real-time notifications for all system activities:

- **Request Notifications**: Notify relevant users when maintenance requests are created, assigned, or updated
- **Payment Notifications**: Notify customers and accounting staff about payment status changes
- **Spare Part Notifications**: Notify coordinators, managers, and Engineers about spare part availability and pricing
- **Visit Notifications**: Notify maintenance support when visits are completed
- **Push Notifications**: Mobile push notifications for critical updates

### Business Workflows

#### Maintenance Request Workflow

1. Customer submits maintenance request with equipment selection, description, and attachments
2. Request appears in maintenance support dashboard
3. Maintenance support reviews and assigns to Engineer (or auto-assignment based on location)
4. Engineer receives notification and creates maintenance visit
5. Engineer scans QR code or enters serial number to load equipment data
6. Engineer creates visit report with outcome (completed, needs second visit, needs spare part)
7. If spare part needed: Coordinator checks availability → If local: Inventory manager prepares → If global: Maintenance manager sets price → Customer approves/rejects
8. Customer can rate Engineer after completion

#### Payment Workflow

1. Payment created for maintenance request or spare part
2. Customer selects payment method (gateway, cash, bank transfer)
3. Payment processed based on method
4. For cash/bank transfer: Accounting staff confirms or rejects
5. Customer receives notification about payment status

#### Sales Workflow

1. SalesMan creates client record
2. SalesMan creates offer request or directly creates offer
3. Offer includes equipment, pricing, terms, and payment plans
4. Client reviews and responds to offer
5. Deal created upon acceptance
6. Sales reports generated for tracking

### System Benefits

- **Streamlined Operations**: Automated workflows reduce manual coordination
- **Real-time Tracking**: Customers and staff can track request status in real-time
- **Comprehensive Reporting**: Detailed analytics for sales, maintenance, and payments
- **Multi-role Support**: System supports various user roles with appropriate permissions
- **Mobile Support**: Mobile applications for customers and Engineers
- **Flexible Payment Options**: Multiple payment methods to accommodate different customer preferences
- **Equipment Lifecycle Management**: Complete tracking from purchase to maintenance

---

## Technical Implementation

### Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Real-time Communication**: SignalR for notifications
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Repository Pattern with Unit of Work
- **Dependency Injection**: Built-in ASP.NET Core DI container

### Architecture

#### Repository Pattern

- **Base Repository**: Generic repository with common CRUD operations
- **Specific Repositories**: Specialized repositories for each entity with custom query methods
- **Unit of Work**: Centralized transaction management and repository access

#### Service Layer

- **Business Logic**: All business logic encapsulated in service classes
- **DTOs**: Data Transfer Objects for API communication
- **Validation**: Input validation using Data Annotations and custom validators
- **Mapping**: Entity to DTO mapping using MappingService

#### Controllers

- **RESTful APIs**: RESTful endpoints following REST conventions
- **Authorization**: Role-based authorization using [Authorize] attributes
- **Response Format**: Consistent response format using BaseController helpers

### Database Design

#### Key Models

**Maintenance Module:**

- `MaintenanceRequest`: Core maintenance request entity
- `MaintenanceVisit`: Engineer visit records
- `MaintenanceRequestAttachment`: File attachments (images, videos, audio)
- `SparePartRequest`: Spare part requests with availability tracking
- `MaintenanceRequestRating`: Customer ratings for Engineers

**Payment Module:**

- `Payment`: Payment records with multiple payment methods
- `PaymentTransaction`: Transaction history for each payment
- `PaymentGatewayConfig`: Configuration for payment gateways

**Sales Module:**

- `Client`: Client information
- `Offer`: Sales offers with equipment and terms
- `Deal`: Closed deals
- `SalesOffer`: Enhanced offer management
- `WeeklyPlan`: SalesMan weekly planning
- `TaskProgress`: Task tracking

**Equipment:**

- `Equipment`: Equipment information with QR codes
- Supports linking to hospitals or customers directly

### Key Features Implementation

#### Auto-Assignment Logic

- Engineers assigned automatically based on:
     - Equipment location (hospital location)
     - Engineer governorate assignments
     - Current workload (round-robin with least active requests)
- Falls back to manual assignment if no Engineers available

#### File Upload Service

- `MaintenanceAttachmentService`: Handles file uploads for maintenance requests
- Supports multiple file types: Images (JPG, PNG, GIF), Videos (MP4, AVI, MOV), Audio (MP3, WAV), Documents (PDF, DOC)
- File size limits: Images (10MB), Videos (100MB), Audio (20MB), Documents (10MB)
- Files stored in `wwwroot/maintenance-requests/{requestId}/attachments/`

#### Payment Processing

- `PaymentService`: Handles payment creation and processing
- Supports multiple payment methods:
     - Stripe integration (ready for API integration)
     - PayPal integration (ready for API integration)
     - Local payment gateways (Fawry, Paymob, etc.)
     - Cash payments (requires accounting confirmation)
     - Bank transfers (requires accounting confirmation)
- `AccountingService`: Handles payment confirmation and rejection

#### Notification System

- `NotificationService`: Centralized notification management
- `SignalR Hub`: Real-time notifications via WebSocket
- `MobileNotificationService`: Push notifications for mobile apps
- Notifications sent at each workflow step:
     - Request creation → Maintenance Support
     - Assignment → Engineer
     - Visit completion → Maintenance Support
     - Spare part requests → Coordinators
     - Price setting → Customers
     - Payment status → Customers and Accounting

### API Endpoints

#### Maintenance Module

- `GET /api/MaintenanceRequest` - Get all requests (filtered by role)
- `POST /api/MaintenanceRequest` - Create new request
- `GET /api/MaintenanceRequest/{id}` - Get request details
- `PUT /api/MaintenanceRequest/{id}/assign` - Assign to Engineer
- `POST /api/MaintenanceVisit` - Create visit
- `GET /api/MaintenanceVisit/request/{requestId}` - Get visits for request
- `POST /api/SparePartRequest` - Create spare part request
- `PUT /api/SparePartRequest/{id}/set-price` - Set customer price
- `POST /api/MaintenanceAttachment/upload` - Upload attachment
- `GET /api/MaintenanceAttachment/request/{requestId}` - Get attachments

#### Payment Module

- `POST /api/Payment` - Create payment
- `POST /api/Payment/{id}/stripe` - Process Stripe payment
- `POST /api/Payment/{id}/paypal` - Process PayPal payment
- `POST /api/Payment/{id}/cash` - Record cash payment
- `POST /api/Payment/{id}/bank-transfer` - Record bank transfer
- `POST /api/Accounting/{paymentId}/confirm` - Confirm payment
- `POST /api/Accounting/{paymentId}/reject` - Reject payment
- `GET /api/Accounting/dashboard` - Get accounting dashboard

### Database Migrations

- Migration: `MaintenanceAndPaymentModule`
- Includes all new tables for maintenance and payment modules
- Foreign key relationships configured with proper cascade behaviors
- Check constraints for data integrity (e.g., Equipment must be linked to either hospital or customer, not both)

### Security

- **Authentication**: JWT token-based authentication
- **Authorization**: Role-based authorization with [Authorize(Roles = "...")] attributes
- **Input Validation**: Data annotations and custom validators
- **SQL Injection Protection**: Entity Framework Core parameterized queries
- **File Upload Security**: File type and size validation

### Testing

- All endpoints tested and verified
- Migration tested and applied successfully
- Auto-assignment logic tested
- File upload functionality tested
- Payment processing flow tested

### Deployment Notes

- Database migration must be applied before deployment
- File upload directories must have write permissions
- Payment gateway credentials must be configured in appsettings
- SignalR requires WebSocket support on server
