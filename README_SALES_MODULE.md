# SoitMed Backend - Sales Module Branch

## Business Overview

The Sales Module is a comprehensive solution for managing the complete sales lifecycle in the medical equipment industry. It enables sales teams to efficiently track clients, create offers, manage deals, and generate detailed sales reports.

### Business Functions

#### 1. Client Management
- **Client Registration**: Register and manage client information including hospitals, clinics, doctors, and technicians
- **Client Types**: Support for different client types (Hospital, Clinic, Doctor, Technician, etc.)
- **Client History**: Track all interactions, visits, and offers for each client
- **Client Analytics**: Analyze client behavior, preferences, and purchase patterns

#### 2. Offer Management
- **Offer Creation**: Create detailed sales offers with:
  - Multiple equipment items with specifications
  - Pricing and discount options
  - Payment terms and installment plans
  - Delivery terms and conditions
  - Equipment images and documentation
- **Offer Tracking**: Track offer status (Draft, Sent, Under Review, Accepted, Rejected)
- **Offer Modifications**: Update offers based on client feedback
- **Offer Comparison**: Compare different offers for the same client
- **PDF Export**: Generate professional PDF offers for client presentation

#### 3. Deal Processing
- **Deal Creation**: Convert accepted offers into deals
- **Deal Approval**: Multi-level approval workflow for deals
- **Deal Tracking**: Track deal progress from creation to closure
- **Deal Analytics**: Analyze deal success rates and conversion metrics

#### 4. Salesman Management
- **Weekly Planning**: Salesmen can create and manage weekly plans with tasks
- **Task Management**: Create, update, and track tasks within weekly plans
- **Task Progress**: Track task completion and progress
- **Performance Tracking**: Monitor salesman performance metrics including:
  - Number of clients
  - Offers created
  - Deals closed
  - Revenue generated
  - Target achievement

#### 5. Sales Reports
- **Sales Statistics**: Comprehensive sales statistics and analytics
- **Performance Reports**: Individual and team performance reports
- **Revenue Reports**: Revenue tracking and forecasting
- **Client Reports**: Client acquisition and retention reports

### Business Workflows

#### Sales Process Workflow
1. **Client Acquisition**: Salesman identifies and registers new client
2. **Client Visit**: Salesman visits client, understands requirements
3. **Offer Creation**: Salesman creates offer with equipment, pricing, and terms
4. **Offer Review**: Client reviews offer, may request modifications
5. **Offer Acceptance**: Client accepts offer
6. **Deal Creation**: Offer converted to deal
7. **Deal Closure**: Deal finalized and closed
8. **Reporting**: Sales reports generated for analysis

#### Weekly Planning Workflow
1. Salesman creates weekly plan
2. Salesman adds tasks to weekly plan
3. Salesman updates task progress throughout the week
4. System tracks task completion and performance
5. Reports generated for management review

### Business Benefits

- **Improved Sales Efficiency**: Streamlined processes reduce time from client contact to deal closure
- **Better Client Management**: Comprehensive client tracking improves relationship management
- **Enhanced Reporting**: Detailed analytics help in decision making
- **Performance Tracking**: Monitor and improve salesman performance
- **Professional Presentation**: PDF export creates professional offer documents

---

## Technical Implementation

### Architecture

#### Models
- **Client**: Client information with type, contact details, and relationships
- **Offer**: Sales offers with equipment, pricing, and terms
- **SalesOffer**: Enhanced offer management with status tracking
- **Deal**: Closed deals with approval workflow
- **WeeklyPlan**: Salesman weekly planning structure
- **WeeklyPlanTask**: Tasks within weekly plans
- **TaskProgress**: Task progress tracking
- **SalesmanTarget**: Salesman performance targets

#### Repositories
- **ClientRepository**: Client data access with custom queries
- **OfferRepository**: Offer management with filtering and search
- **SalesOfferRepository**: Enhanced offer operations
- **DealRepository**: Deal management and tracking
- **WeeklyPlanRepository**: Weekly plan operations
- **WeeklyPlanTaskRepository**: Task management
- **TaskProgressRepository**: Progress tracking

#### Services
- **ClientService**: Client management business logic
- **OfferService**: Offer creation, management, and PDF export
- **DealService**: Deal processing and approval workflows
- **WeeklyPlanService**: Weekly planning operations
- **WeeklyPlanTaskService**: Task management operations
- **TaskProgressService**: Progress tracking and updates
- **SalesmanStatisticsService**: Performance metrics and reporting

#### Controllers
- **ClientController**: Client CRUD operations
- **OfferController**: Offer management endpoints
- **DealController**: Deal processing endpoints
- **WeeklyPlanController**: Weekly plan management
- **WeeklyPlanTaskController**: Task management
- **TaskProgressController**: Progress tracking
- **SalesmanStatisticsController**: Statistics and reports

### Key Features

#### PDF Export
- **Offer PDF Generation**: Generate professional PDF offers with:
  - Company letterhead
  - Equipment details and images
  - Pricing breakdown
  - Terms and conditions
  - Payment plans
- **PDF Service**: Integrated PDF generation using PDF libraries

#### Offer Equipment Management
- **Multiple Equipment**: Support for multiple equipment items per offer
- **Equipment Images**: Upload and manage equipment images
- **Equipment Specifications**: Detailed specifications for each equipment
- **Equipment Pricing**: Individual pricing for each equipment item

#### Payment Terms
- **Installment Plans**: Support for multiple installment plans
- **Payment Terms**: Flexible payment terms configuration
- **Payment Tracking**: Track payment status for each offer/deal

#### Recent Activity Tracking
- **Activity Logs**: Track all activities on offers
- **Recent Offer Activity**: Quick access to recent changes
- **Activity History**: Complete history of offer modifications

### API Endpoints

#### Client Management
- `GET /api/Client` - Get all clients (filtered by role)
- `POST /api/Client` - Create new client
- `GET /api/Client/{id}` - Get client details
- `PUT /api/Client/{id}` - Update client
- `DELETE /api/Client/{id}` - Delete client

#### Offer Management
- `GET /api/Offer` - Get all offers
- `POST /api/Offer` - Create new offer
- `GET /api/Offer/{id}` - Get offer details
- `PUT /api/Offer/{id}` - Update offer
- `POST /api/Offer/{id}/export-pdf` - Export offer as PDF
- `GET /api/Offer/client/{clientId}` - Get offers for client

#### Deal Management
- `GET /api/Deal` - Get all deals
- `POST /api/Deal` - Create deal from offer
- `GET /api/Deal/{id}` - Get deal details
- `PUT /api/Deal/{id}/approve` - Approve deal
- `PUT /api/Deal/{id}/reject` - Reject deal

#### Weekly Planning
- `GET /api/WeeklyPlan` - Get weekly plans
- `POST /api/WeeklyPlan` - Create weekly plan
- `GET /api/WeeklyPlan/{id}` - Get weekly plan details
- `PUT /api/WeeklyPlan/{id}` - Update weekly plan

#### Task Management
- `GET /api/WeeklyPlanTask` - Get tasks
- `POST /api/WeeklyPlanTask` - Create task
- `PUT /api/WeeklyPlanTask/{id}` - Update task
- `POST /api/TaskProgress` - Update task progress

#### Statistics
- `GET /api/SalesmanStatistics/dashboard` - Get salesman dashboard
- `GET /api/SalesmanStatistics/performance` - Get performance metrics
- `GET /api/SalesmanStatistics/targets` - Get target information

### Database Design

#### Key Tables
- **Clients**: Client information
- **Offers**: Sales offers
- **SalesOffers**: Enhanced offer management
- **Deals**: Closed deals
- **WeeklyPlans**: Weekly planning structure
- **WeeklyPlanTasks**: Tasks within plans
- **TaskProgress**: Progress tracking
- **SalesmanTargets**: Performance targets
- **OfferEquipment**: Equipment in offers
- **OfferTerms**: Payment terms
- **InstallmentPlans**: Installment configurations
- **RecentOfferActivity**: Activity tracking

### Integration Points

- **Maintenance Module**: Equipment from sales can be tracked for maintenance
- **Payment Module**: Payment processing for deals and offers
- **Notification System**: Real-time notifications for offer status changes
- **User Management**: Role-based access for salesmen, sales support, and managers

### Security

- **Role-Based Access**: Different permissions for Salesman, SalesSupport, SalesManager
- **Data Isolation**: Salesmen can only see their own clients and offers
- **Approval Workflows**: Multi-level approval for deals
- **Audit Trail**: Complete activity logging

### Performance Optimizations

- **Indexed Queries**: Database indexes on frequently queried fields
- **Pagination**: Paginated results for large datasets
- **Caching**: Strategic caching for frequently accessed data
- **Eager Loading**: Optimized data loading with Include statements


