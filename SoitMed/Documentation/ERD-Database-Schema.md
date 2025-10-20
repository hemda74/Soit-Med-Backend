# SoitMed Hospital Management System - Entity Relationship Diagram (ERD)

## Overview
This ERD represents the complete database schema for the Hospital Management System with QR code generation capabilities.

## Mermaid ERD Code
```mermaid
erDiagram
    %% Core Identity Tables
    APPLICATIONUSER {
        string Id PK
        string UserName
        string Email
        string PasswordHash
        string FirstName
        string LastName
        int DepartmentId FK
        datetime CreatedAt
        datetime LastLoginAt
        boolean IsActive
    }

    DEPARTMENT {
        int Id PK
        string Name
        string Description
        datetime CreatedAt
    }

    %% Hospital Management Tables
    HOSPITAL {
        string HospitalId PK
        string Name
        string Location
        string Address
        string PhoneNumber
        datetime CreatedAt
        boolean IsActive
    }

    DOCTOR {
        int DoctorId PK
        string Name
        string Specialty
        string HospitalId FK
        string UserId FK
        datetime CreatedAt
        boolean IsActive
    }

    TECHNICIAN {
        int TechnicianId PK
        string Name
        string Department
        string HospitalId FK
        string UserId FK
        datetime CreatedAt
        boolean IsActive
    }

    %% Equipment Management Tables
    EQUIPMENT {
        int Id PK
        string Name
        string QRCode UK
        string QRCodeImageData
        string QRCodePdfPath
        string Description
        string Model
        string Manufacturer
        datetime PurchaseDate
        datetime WarrantyExpiry
        string HospitalId FK
        int RepairVisitCount
        int Status
        datetime CreatedAt
        datetime LastMaintenanceDate
        boolean IsActive
    }

    REPAIRREQUEST {
        int Id PK
        int EquipmentId FK
        string Description
        string Symptoms
        int Priority
        int Status
        int DoctorId FK
        int TechnicianId FK
        int AssignedEngineerId FK
        datetime RequestedAt
        datetime AssignedAt
        datetime StartedAt
        datetime CompletedAt
        string RepairNotes
        string PartsUsed
        decimal RepairCost
        int EstimatedHours
        int ActualHours
        boolean IsActive
    }

    %% Location Management Tables
    GOVERNORATE {
        int GovernorateId PK
        string Name
        datetime CreatedAt
        boolean IsActive
    }

    ENGINEER {
        int EngineerId PK
        string Name
        string Specialty
        string UserId FK
        datetime CreatedAt
        boolean IsActive
    }

    ENGINEERGOVERNORATE {
        int Id PK
        int EngineerId FK
        int GovernorateId FK
        datetime AssignedAt
        boolean IsActive
    }

    %% Relationships
    APPLICATIONUSER ||--o{ DEPARTMENT : "belongs to"
    HOSPITAL ||--o{ DOCTOR : "employs"
    HOSPITAL ||--o{ TECHNICIAN : "employs"
    HOSPITAL ||--o{ EQUIPMENT : "owns"
    DOCTOR ||--o{ REPAIRREQUEST : "requests"
    TECHNICIAN ||--o{ REPAIRREQUEST : "requests"
    EQUIPMENT ||--o{ REPAIRREQUEST : "needs repair"
    ENGINEER ||--o{ REPAIRREQUEST : "assigned to"
    APPLICATIONUSER ||--o| DOCTOR : "can be"
    APPLICATIONUSER ||--o| TECHNICIAN : "can be"
    APPLICATIONUSER ||--o| ENGINEER : "can be"
    ENGINEER ||--o{ ENGINEERGOVERNORATE : "works in"
    GOVERNORATE ||--o{ ENGINEERGOVERNORATE : "has engineers"
```

## Database Schema Details

### Core Tables
- **APPLICATIONUSER**: Identity framework users with department assignment
- **DEPARTMENT**: Organizational departments (Administration, Medical, Sales, etc.)

### Hospital Management
- **HOSPITAL**: Hospital entities with location and contact information
- **DOCTOR**: Medical staff associated with hospitals
- **TECHNICIAN**: Technical staff for equipment maintenance

### Equipment Management (QR Code Enhanced)
- **EQUIPMENT**: Equipment with QR code generation capabilities
  - `QRCode`: Unique identifier (format: EQ-{HospitalId}-{EquipmentName}-{Timestamp})
  - `QRCodeImageData`: Base64 encoded QR code image
  - `QRCodePdfPath`: Path to generated PDF file
- **REPAIRREQUEST**: Repair workflow management

### Location Management
- **GOVERNORATE**: Geographic regions
- **ENGINEER**: Field engineers for equipment repair
- **ENGINEERGOVERNORATE**: Many-to-many relationship for engineer assignments

## Key Constraints
- **Primary Keys (PK)**: Unique identifiers for each entity
- **Foreign Keys (FK)**: Referential integrity between related entities
- **Unique Keys (UK)**: QRCode field ensures no duplicate QR codes
- **Check Constraints**: RepairRequest ensures either Doctor OR Technician (not both)

## QR Code System Integration
- Equipment table includes QR-specific fields
- Automatic QR code generation on equipment creation
- PDF storage in `wwwroot/qrs/` directory
- Base64 image storage for API responses

## Generated: September 6, 2025
