# SoitMed Hospital Management System - Class Diagram

## Overview
This class diagram represents the complete object-oriented structure of the Hospital Management System with QR code generation capabilities.

## Mermaid Class Diagram Code
```mermaid
classDiagram
    %% Core Identity and Authentication
    class ApplicationUser {
        +string Id
        +string UserName
        +string Email
        +string PasswordHash
        +string FirstName
        +string LastName
        +int DepartmentId
        +DateTime CreatedAt
        +DateTime LastLoginAt
        +bool IsActive
        +string FullName
        +Department Department
    }

    %% Core Business Models
    class Department {
        +int Id
        +string Name
        +string Description
        +DateTime CreatedAt
        +ICollection Users
    }

    %% Hospital Management
    class Hospital {
        +string HospitalId
        +string Name
        +string Location
        +string Address
        +string PhoneNumber
        +DateTime CreatedAt
        +bool IsActive
        +ICollection Doctors
        +ICollection Technicians
        +ICollection Equipment
    }

    class Doctor {
        +int DoctorId
        +string Name
        +string Specialty
        +string HospitalId
        +string UserId
        +DateTime CreatedAt
        +bool IsActive
        +Hospital Hospital
        +ApplicationUser User
        +ICollection RepairRequests
    }

    class Technician {
        +int TechnicianId
        +string Name
        +string Department
        +string HospitalId
        +string UserId
        +DateTime CreatedAt
        +bool IsActive
        +Hospital Hospital
        +ApplicationUser User
        +ICollection RepairRequests
    }

    %% Equipment Management with QR Code
    class Equipment {
        +int Id
        +string Name
        +string QRCode
        +string QRCodeImageData
        +string QRCodePdfPath
        +string Description
        +string Model
        +string Manufacturer
        +DateTime PurchaseDate
        +DateTime WarrantyExpiry
        +string HospitalId
        +int RepairVisitCount
        +EquipmentStatus Status
        +DateTime CreatedAt
        +DateTime LastMaintenanceDate
        +bool IsActive
        +Hospital Hospital
        +ICollection RepairRequests
    }

    class RepairRequest {
        +int Id
        +int EquipmentId
        +string Description
        +string Symptoms
        +RepairPriority Priority
        +RepairStatus Status
        +int DoctorId
        +int TechnicianId
        +int AssignedEngineerId
        +DateTime RequestedAt
        +DateTime AssignedAt
        +DateTime StartedAt
        +DateTime CompletedAt
        +string RepairNotes
        +string PartsUsed
        +decimal RepairCost
        +int EstimatedHours
        +int ActualHours
        +bool IsActive
        +Equipment Equipment
        +Doctor RequestingDoctor
        +Technician RequestingTechnician
        +Engineer AssignedEngineer
    }

    %% Location Management
    class Governorate {
        +int GovernorateId
        +string Name
        +DateTime CreatedAt
        +bool IsActive
        +ICollection EngineerGovernorates
    }

    class Engineer {
        +int EngineerId
        +string Name
        +string Specialty
        +string UserId
        +DateTime CreatedAt
        +bool IsActive
        +ApplicationUser User
        +ICollection EngineerGovernorates
        +ICollection AssignedRepairRequests
    }

    class EngineerGovernorate {
        +int Id
        +int EngineerId
        +int GovernorateId
        +DateTime AssignedAt
        +bool IsActive
        +Engineer Engineer
        +Governorate Governorate
    }

    %% QR Code Service
    class QRCodeService {
        +GenerateQRCodeAsync(Equipment equipment)
        +GenerateUniqueQRCode(string equipmentName, string hospitalId)
        +SaveQRCodePdfAsync(string qrCodeData, string equipmentName, string filePath)
        +CreateQRCodeData(Equipment equipment)
        +ConvertImageToBase64(Bitmap image)
    }

    %% Relationships
    ApplicationUser --> Department
    Hospital --> Doctor
    Hospital --> Technician
    Hospital --> Equipment
    Doctor --> RepairRequest
    Technician --> RepairRequest
    Equipment --> RepairRequest
    Engineer --> RepairRequest
    ApplicationUser --> Doctor
    ApplicationUser --> Technician
    ApplicationUser --> Engineer
    Engineer --> EngineerGovernorate
    Governorate --> EngineerGovernorate
    Equipment --> QRCodeService
```

## Key Features
- **Identity Management**: ApplicationUser with role-based access
- **Hospital Management**: Multi-hospital support with staff management
- **Equipment Management**: Complete equipment lifecycle with QR code generation
- **QR Code System**: Automatic QR code generation, PDF creation, and database storage
- **Repair Management**: Comprehensive repair request workflow
- **Location Management**: Engineer assignment by governorate
- **Service Layer**: QRCodeService for QR code operations

## Generated: September 6, 2025
