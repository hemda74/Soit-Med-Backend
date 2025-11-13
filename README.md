# Soit-Med Hospital Management System

A comprehensive hospital management system with equipment tracking, repair request management, sales module, and multi-departmental user roles. Built with ASP.NET Core 8, Entity Framework Core, and JWT authentication.

## Features

### Real-Time Notifications System
- **Multi-Channel Delivery**: Real-time notifications via SignalR (when app is open) and push notifications (when app is closed)
- **Cross-Platform Support**: Works seamlessly on Web Dashboard and Mobile App
- **User-Centric Design**: Notifications are delivered to specific users regardless of business context
- **Flexible Notification Types**: Support for offers, deals, tasks, clients, workflows, and system notifications
- **Priority Levels**: High, Medium, and Low priority notifications
- **Push Notifications**: Native push notifications with sound and banner for mobile devices
- **In-App Notifications**: Real-time notification list with unread count badge

📖 **Technical Documentation**: See [NOTIFICATION.md](../NOTIFICATION.md) for complete implementation details

### Multi-Departmental Organization
- **6 Departments**: Administration, Medical, Sales, Engineering, Finance, Legal
- **10+ User Roles**: SuperAdmin, Admin, Doctor, Technician, Salesman, SalesManager, SalesSupport, Engineer, FinanceManager, FinanceEmployee, LegalManager, LegalEmployee
- **Role-based Access Control** with hierarchical permissions

### Hospital Network Management
- **Hospital Registration** with unique codes and contact information
- **Doctor & Technician Management** linked to hospitals
- **Geographic Coverage** through governorate-engineer assignments

### Equipment Tracking System
- **QR Code Integration** for unique equipment identification
- **Equipment Status Tracking**: Operational, Under Maintenance, Out of Order, Retired
- **Maintenance History** with detailed repair logs
- **Visit Counter** for repair frequency tracking

### Repair Request Management
- **Automated Assignment** to engineers based on hospital location
- **Priority-based Queuing**: Emergency, Critical, High, Medium, Low
- **Complete Workflow**: Pending → Assigned → In Progress → Completed
- **Cost Tracking** with parts and labor documentation
- **Time Estimation** vs actual hours reporting

### Sales Module (Complete CRM System)
- **Client Management**: Full client lifecycle tracking with profiles, history, and statistics
- **Weekly Planning**: Salesmen create weekly plans with tasks and track progress
- **Task Progress Tracking**: Record visits, calls, meetings with detailed notes and follow-ups
- **Offer Request System**: Salesmen request offers, SalesSupport creates detailed offers with equipment, terms, and installments
- **Deal Management**: Multi-level approval workflow (Salesman → Manager → SuperAdmin)
- **Statistics & Reporting**: Performance tracking, targets, and progress monitoring
- **Target Management**: Set and track quarterly/yearly targets for salesmen and teams

### Advanced Security
- **JWT Authentication** with 5-year token validity
- **Role-based Authorization** for all endpoints
- **Secure API** with CORS support and Swagger documentation

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/hemda74/Soit-Med-Backend.git
cd Soit-Med-Backend/SoitMed
```

2. **Update Connection String**
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SoitMedDB;Trusted_Connection=true"
  },
  "JWT": {
    "ValidIss": "https://localhost:7271",
    "ValidAud": "https://localhost:7271",
    "SecritKey": "YourSuperSecretKeyHere123456789"
  }
}
```

3. **Run Database Migrations**
```bash
dotnet ef database update
```

4. **Start the Application**
```bash
dotnet run
```

5. **Access Swagger UI**
Navigate to: `https://localhost:5117/swagger`

## Equipment Management

### Equipment Lifecycle
1. **Registration**: Add equipment with QR code and hospital assignment
2. **Operation**: Track status and maintenance schedules
3. **Repair**: Handle repair requests and track visit counts
4. **Retirement**: Mark equipment as retired when end-of-life

### QR Code Integration
- **Unique Identifiers**: Each equipment has a unique QR code
- **Quick Access**: API endpoint for QR code lookup
- **Mobile Friendly**: Designed for mobile scanning applications

## Repair Request Workflow

### Request Creation
1. **Doctor/Technician** identifies equipment issue
2. **Scans QR code** or selects equipment
3. **Submits repair request** with description and priority
4. **System increments** equipment repair visit count

### Automatic Assignment
1. **System identifies** hospital location
2. **Finds engineers** in matching governorate
3. **Assigns to engineer** with lowest current workload
4. **Updates status** to "Assigned"

### Priority Levels
- **Emergency (5)**: Immediate attention required
- **Critical (4)**: Urgent repair needed
- **High (3)**: Important but not critical
- **Medium (2)**: Standard priority
- **Low (1)**: Can be scheduled flexibly

## API Endpoints

### Authentication
```http
POST   /api/Account/register     # Register new user with role
POST   /api/Account/login        # Login and get JWT token (5-year validity)
```

### Equipment Management
```http
GET    /api/Equipment/qr/{qrCode}        # Get equipment by QR code
POST   /api/Equipment                    # Create equipment
GET    /api/Equipment/{id}/repair-history    # Get repair history
```

### Repair Request Management
```http
POST   /api/RepairRequest                # Create repair request (Doctor/Technician)
GET    /api/RepairRequest/pending        # Get pending requests
GET    /api/RepairRequest/engineer/{id}  # Get engineer's assigned requests
```

### Required API Endpoints
```http
GET    /api/Governorate/{id}/engineers   # Get engineers in governorate
POST   /api/Role/business                # Create business role
PUT    /api/Role/business/{id}           # Update business role
```

## Architecture

### Project Structure
```
SoitMed/
├── Controllers/           # API Controllers
├── Models/               # Data Models (Organized by Domain)
│   ├── Core/            # Core business models
│   ├── Identity/        # User & Authentication models
│   ├── Hospital/        # Hospital domain models
│   ├── Location/        # Geographic models
│   ├── Equipment/       # Equipment domain models
│   └── Context.cs       # Entity Framework DbContext
├── DTO/                 # Data Transfer Objects
├── Migrations/          # Entity Framework Migrations
└── Program.cs           # Application Entry Point
```

### Technology Stack
- **Framework**: ASP.NET Core 8
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with Domain-Driven Design

## Recent Changes

### Sales Module Implementation (2025)
- Complete Sales CRM system with client management
- Weekly planning and task tracking system
- Offer creation workflow (Salesman → SalesSupport → Salesman)
- Deal management with multi-level approval (Salesman → Manager → SuperAdmin)
- Statistics and performance tracking
- Target management system
- Performance optimizations with database indexes and computed columns
- Comprehensive unit tests for all sales endpoints

### Major System Overhaul
- Equipment Management System with QR code integration
- Repair Request Workflow with automated engineer assignment
- Model Restructure into domain-specific folders
- Extended JWT Tokens to 5-year validity
- Enhanced Database Schema with proper relationships
- Comprehensive API Documentation
- Solution renamed from Lab1 to SoitMed

## Database Schema

### Core Tables
- **ApplicationUsers** - System users with department assignments
- **Departments** - Organizational departments
- **BusinessRoles** - Custom business roles (separate from Identity roles)

### Hospital Domain
- **Hospitals** - Hospital information with unique codes
- **Doctors** - Medical staff linked to hospitals
- **Technicians** - Technical staff linked to hospitals

### Location Domain
- **Governorates** - Geographic regions
- **Engineers** - Engineering staff
- **EngineerGovernorates** - Many-to-many relationship for coverage areas

### Equipment Domain
- **Equipment** - Medical equipment with QR codes and hospital assignments
- **RepairRequests** - Repair requests with complete workflow tracking

### Sales Domain
- **Clients** - Client information with complete history tracking
- **WeeklyPlans** - Weekly planning system for salesmen
- **WeeklyPlanTasks** - Tasks within weekly plans
- **TaskProgresses** - Progress records for visits, calls, meetings
- **SalesOffers** - Sales offers with equipment, terms, and installments
- **SalesDeals** - Deals with multi-level approval workflow
- **OfferRequests** - Requests from salesmen to SalesSupport
- **SalesmanTargets** - Target tracking for salesmen and teams

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## API Documentation

### Notification System
- **Complete Notification Guide**: See [NOTIFICATION.md](../NOTIFICATION.md) for full technical documentation
- **Notification API**: `/api/Notification` endpoints for managing notifications
- **Real-Time Delivery**: SignalR hub at `/notificationHub` for instant notifications
- **Push Notifications**: Device token registration and push notification delivery

### Role-Specific API Documentation

- **Salesman API**: See `SALESMAN_API_DOCUMENTATION.md`
- **Sales Manager API**: See `SALES_MANAGER_API_DOCUMENTATION.md`
- **Sales Support API**: See `SALES_SUPPORT_API_DOCUMENTATION.md`
- **Super Admin API**: See `SUPER_ADMIN_API_DOCUMENTATION.md`

Each documentation includes:
- User stories for the role
- Complete endpoint details with request/response examples
- Status values reference
- Common workflows
- Error handling guide

## Support

For support and questions:
- Create an issue in the repository
- Check the API documentation at `/swagger`
- Review the comprehensive endpoint documentation above
- Refer to role-specific API documentation files

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Built for healthcare management excellence**

*Last Updated: September 2025*