# 🏥 Soit Medical Backend - Hospital Management System

A comprehensive hospital management system with equipment tracking, repair request management, and multi-departmental user roles. Built with ASP.NET Core 8, Entity Framework Core, and JWT authentication.

## ✨ Features

### 🏢 **Multi-Departmental Organization**
- **6 Departments**: Administration, Medical, Sales, Engineering, Finance, Legal
- **10 User Roles**: SuperAdmin, Admin, Doctor, Technician, Salesman, Engineer, FinanceManager, FinanceEmployee, LegalManager, LegalEmployee
- **Role-based Access Control** with hierarchical permissions

### 🏥 **Hospital Network Management**
- **Hospital Registration** with unique codes and contact information
- **Doctor & Technician Management** linked to hospitals
- **Geographic Coverage** through governorate-engineer assignments

### 🔧 **Equipment Tracking System**
- **QR Code Integration** for unique equipment identification
- **Equipment Status Tracking**: Operational, Under Maintenance, Out of Order, Retired
- **Maintenance History** with detailed repair logs
- **Visit Counter** for repair frequency tracking

### 🛠️ **Repair Request Management**
- **Automated Assignment** to engineers based on hospital location
- **Priority-based Queuing**: Emergency, Critical, High, Medium, Low
- **Complete Workflow**: Pending → Assigned → In Progress → Completed
- **Cost Tracking** with parts and labor documentation
- **Time Estimation** vs actual hours reporting

### 🔐 **Advanced Security**
- **JWT Authentication** with 5-year token validity
- **Role-based Authorization** for all endpoints
- **Secure API** with CORS support and Swagger documentation

## 🚀 Getting Started

### **Prerequisites**
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### **Installation**

1. **Clone the repository**
```bash
git clone https://github.com/hemda74/Soit-Med-Backend.git
cd Soit-Med-Backend/Lab1
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

## 🔧 Equipment Management

### **Equipment Lifecycle**
1. **Registration**: Add equipment with QR code and hospital assignment
2. **Operation**: Track status and maintenance schedules
3. **Repair**: Handle repair requests and track visit counts
4. **Retirement**: Mark equipment as retired when end-of-life

### **QR Code Integration**
- **Unique Identifiers**: Each equipment has a unique QR code
- **Quick Access**: API endpoint for QR code lookup
- **Mobile Friendly**: Designed for mobile scanning applications

## 🛠️ Repair Request Workflow

### **Request Creation**
1. **Doctor/Technician** identifies equipment issue
2. **Scans QR code** or selects equipment
3. **Submits repair request** with description and priority
4. **System increments** equipment repair visit count

### **Automatic Assignment**
1. **System identifies** hospital location
2. **Finds engineers** in matching governorate
3. **Assigns to engineer** with lowest current workload
4. **Updates status** to "Assigned"

### **Priority Levels**
- **Emergency (5)**: Immediate attention required
- **Critical (4)**: Urgent repair needed
- **High (3)**: Important but not critical
- **Medium (2)**: Standard priority
- **Low (1)**: Can be scheduled flexibly

## 📊 Key API Endpoints

### **Authentication**
```http
POST   /api/Account/register     # Register new user with role
POST   /api/Account/login        # Login and get JWT token (5-year validity)
```

### **Equipment Management**
```http
GET    /api/Equipment/qr/{qrCode}        # Get equipment by QR code
POST   /api/Equipment                    # Create equipment
GET    /api/Equipment/{id}/repair-history    # Get repair history
```

### **Repair Request Management**
```http
POST   /api/RepairRequest                # Create repair request (Doctor/Technician)
GET    /api/RepairRequest/pending        # Get pending requests
GET    /api/RepairRequest/engineer/{id}  # Get engineer's assigned requests
```

### **Required API Endpoints**
```http
GET    /api/Governorate/{id}/engineers   # Get engineers in governorate ⭐
POST   /api/Role/business                # Create business role ⭐
PUT    /api/Role/business/{id}           # Update business role ⭐
```

## 🏗️ Architecture

### **Project Structure**
```
Lab1/
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

## 📝 Recent Changes

### **Major System Overhaul** (Latest)
- ✅ **Equipment Management System** with QR code integration
- ✅ **Repair Request Workflow** with automated engineer assignment
- ✅ **Model Restructure** into domain-specific folders
- ✅ **Extended JWT Tokens** to 5-year validity
- ✅ **Enhanced Database Schema** with proper relationships
- ✅ **Comprehensive API Documentation**

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📞 Support

For support and questions:
- Create an issue in the repository
- Check the API documentation at `/swagger`
- Review the comprehensive endpoint documentation above

---

**Built with ❤️ for healthcare management**

*Last Updated: September 2025*