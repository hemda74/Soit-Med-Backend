# Soit-Med Hospital Management System API Documentation

## Overview

This API provides comprehensive hospital management functionality including user management, equipment tracking, repair requests, and role-based access control.

## Base URL

```
http://localhost:5117
```

## Authentication

All protected endpoints require JWT Bearer token authentication.

### Getting a Token

```http
POST /api/Account/login
Content-Type: application/json

{
  "userName": "your-username",
  "password": "your-password"
}
```

### Using the Token

Add to request headers:

```
Authorization: Bearer {your-jwt-token}
```

---

## Authentication & Account Management

### Login

```http
POST /api/Account/login
```

**Body:**

```json
{
	"userName": "string",
	"password": "string"
}
```

**Response:** JWT token with 5-year validity

### Register

```http
POST /api/Account/register
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"role": "string",
	"firstName": "string (optional)",
	"lastName": "string (optional)",
	"departmentId": "integer (optional)"
}
```

---

## User Management APIs

### Get Current User Data

```http
GET /api/User/me
Authorization: Bearer {token}
```

**Response:** Complete current user information including roles and department

### Get All Users (Admin Only)

```http
GET /api/User/all
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

### Get User by ID

```http
GET /api/User/{id}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

### Get User by Username

```http
GET /api/User/username/{username}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

### Get Users by Role

```http
GET /api/User/role/{role}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

### Get Users by Department

```http
GET /api/User/department/{departmentId}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

---

## Role-Specific User Creation APIs

### Create Doctor

```http
POST /api/RoleSpecificUser/doctor
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"specialty": "string",
	"hospitalId": "string",
	"departmentId": "integer (optional)"
}
```

**Response:** Doctor details with user account and hospital assignment

### Create Engineer

```http
POST /api/RoleSpecificUser/engineer
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"specialty": "string",
	"governorateIds": [1, 2, 3],
	"departmentId": "integer (optional)"
}
```

**Response:** Engineer details with user account and governorate assignments

### Create Technician

```http
POST /api/RoleSpecificUser/technician
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"department": "string",
	"hospitalId": "string",
	"departmentId": "integer (optional)"
}
```

**Response:** Technician details with user account and hospital assignment

### Create Admin

```http
POST /api/RoleSpecificUser/admin
Authorization: Bearer {token}
Roles: SuperAdmin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"accessLevel": "string (optional)",
	"departmentId": "integer (optional)"
}
```

### Create Finance Manager

```http
POST /api/RoleSpecificUser/finance-manager
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"budgetAuthority": "string (optional)",
	"departmentId": "integer (optional)"
}
```

### Create Legal Manager

```http
POST /api/RoleSpecificUser/legal-manager
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"legalSpecialty": "string (optional)",
	"departmentId": "integer (optional)"
}
```

### Create Salesman

```http
POST /api/RoleSpecificUser/salesman
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"userName": "string",
	"email": "string",
	"password": "string",
	"firstName": "string",
	"lastName": "string",
	"territory": "string (optional)",
	"salesTarget": "string (optional)",
	"departmentId": "integer (optional)"
}
```

---

## Department Management

### Get All Departments

```http
GET /api/Department
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

### Get Department by ID

```http
GET /api/Department/{id}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

### Create Department

```http
POST /api/Department
Authorization: Bearer {token}
Roles: SuperAdmin
```

**Body:**

```json
{
	"name": "string",
	"description": "string"
}
```

### Get Department Users

```http
GET /api/Department/{id}/users
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, FinanceManager, LegalManager
```

---

## Hospital Management

### Get All Hospitals

```http
GET /api/Hospital
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

### Get Hospital by ID

```http
GET /api/Hospital/{hospitalId}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

### Create Hospital

```http
POST /api/Hospital
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"hospitalId": "string",
	"name": "string",
	"location": "string",
	"address": "string",
	"phoneNumber": "string"
}
```

### Add Doctor to Hospital

```http
POST /api/Hospital/{hospitalId}/doctors
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"name": "string",
	"specialty": "string",
	"userId": "string (optional)"
}
```

### Add Technician to Hospital

```http
POST /api/Hospital/{hospitalId}/technicians
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"name": "string",
	"department": "string",
	"userId": "string (optional)"
}
```

---

## Governorate Management (Egypt)

### Get All Governorates

```http
GET /api/Governorate
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

### Get Governorate by ID

```http
GET /api/Governorate/{id}
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Available Egypt Governorates:**

- Cairo
- Alexandria
- Giza
- Qalyubia
- Port Said
- Suez
- Luxor
- Aswan
- Asyut
- Beheira
- Beni Suef
- Dakahlia
- Damietta
- Faiyum
- Gharbia
- Ismailia
- Kafr el-Sheikh
- Matrouh
- Minya
- Monufia
- New Valley
- North Sinai
- Qena
- Red Sea
- Sharqia
- Sohag
- South Sinai

---

## Engineer Management

### Get All Engineers

```http
GET /api/Engineer
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

### Create Engineer

```http
POST /api/Engineer
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"name": "string",
	"specialty": "string",
	"userId": "string (optional)",
	"governorateIds": [1, 2, 3]
}
```

---

## Equipment Management

### Get Equipment by QR Code

```http
GET /api/Equipment/qr/{qrCode}
Authorization: Bearer {token}
```

### Get All Equipment

```http
GET /api/Equipment
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, Doctor, Technician, Engineer
```

### Create Equipment

```http
POST /api/Equipment
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

**Body:**

```json
{
	"name": "string",
	"model": "string",
	"serialNumber": "string",
	"hospitalId": "string",
	"status": "string",
	"installationDate": "datetime",
	"lastMaintenanceDate": "datetime (optional)",
	"nextMaintenanceDate": "datetime (optional)"
}
```

---

## Repair Request Management

### Get All Repair Requests

```http
GET /api/RepairRequest
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, Engineer
```

### Create Repair Request

```http
POST /api/RepairRequest
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, Doctor, Technician
```

**Body:**

```json
{
	"equipmentId": "integer",
	"description": "string",
	"priority": "integer (1-5)",
	"requestedById": "string"
}
```

### Update Repair Request Status

```http
PUT /api/RepairRequest/{id}/status
Authorization: Bearer {token}
Roles: SuperAdmin, Admin, Engineer
```

**Body:**

```json
{
	"status": "string",
	"engineerNotes": "string (optional)"
}
```

---

## Role Management

### Get Available Roles

```http
GET /api/Role/available
```

**Response:**

```json
{
	"roles": [
		"SuperAdmin",
		"Admin",
		"Doctor",
		"Technician",
		"Salesman",
		"Engineer",
		"FinanceManager",
		"FinanceEmployee",
		"LegalManager",
		"LegalEmployee"
	]
}
```

### Get All Roles

```http
GET /api/Role
Authorization: Bearer {token}
Roles: SuperAdmin, Admin
```

---

## Role Hierarchy & Permissions

### SuperAdmin

- Full system access
- Can create/manage all users and roles
- Access to all endpoints

### Admin

- Can create/manage most users (except SuperAdmin)
- Access to most management endpoints
- Cannot delete critical system data

### FinanceManager

- Access to financial and user data
- Can view departments and users
- Limited creation permissions

### LegalManager

- Access to legal and user data
- Can view departments and users
- Limited creation permissions

### Doctor

- Can create repair requests
- Can view assigned equipment
- Access to hospital-specific data

### Technician

- Can create repair requests
- Can view assigned equipment
- Access to hospital-specific data

### Engineer

- Can manage repair requests
- Can update repair status
- Access to governorate-specific assignments

### Salesman

- Basic user access
- Sales-specific permissions

---

## Response Formats

### Success Response

```json
{
  "data": {...},
  "message": "Success message",
  "timestamp": "2025-09-07T15:16:13Z"
}
```

### Error Response

```json
{
  "error": "Error message",
  "details": [...],
  "timestamp": "2025-09-07T15:16:13Z"
}
```

---

## Getting Started

1. **Start the application:**

      ```bash
      cd SoitMed
      dotnet run --urls="http://localhost:5117"
      ```

2. **Access Swagger UI:**
   Navigate to: `http://localhost:5117/swagger`

3. **Login with SuperAdmin:**

      - Create a SuperAdmin user via registration
      - Use the login endpoint to get a JWT token
      - Use the "Authorize" button in Swagger UI

4. **Create role-specific users:**
      - Use the role-specific creation endpoints
      - Assign appropriate departments and locations

---

## Notes

- JWT tokens have 5-year validity
- All dates are in UTC format
- Egypt governorates are automatically seeded on application start
- Departments are automatically created on first run
- QR codes are automatically generated for equipment
- Repair requests are automatically assigned to engineers based on hospital location

---

## Configuration

### Database Connection

Update `appsettings.json`:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SoitMedDB;Trusted_Connection=true"
	},
	"JWT": {
		"ValidIss": "http://localhost:5117",
		"ValidAud": "http://localhost:5117",
		"SecritKey": "YourSuperSecretKeyHere123456789"
	}
}
```

### Run Database Migrations

```bash
dotnet ef database update
```
