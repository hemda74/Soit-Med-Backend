# SoitMed User Registration & Management API Documentation

## Overview

This document provides comprehensive information about user registration, role-based access, and password management in the SoitMed system.

## Table of Contents

1. [Authentication](#authentication)
2. [User Registration by Role](#user-registration-by-role)
3. [Password Management](#password-management)
4. [API Endpoints](#api-endpoints)
5. [Examples](#examples)
6. [Error Handling](#error-handling)

## Authentication

### Login

**Endpoint:** `POST /api/Account/login`

**Request Body:**

```json
{
	"userName": "user@example.com", // Can be email or username
	"password": "YourPassword123!"
}
```

**Response:**

```json
{
	"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
	"expired": "2029-09-10T19:24:57.000Z"
}
```

## User Registration by Role

### 1. Doctor Registration

**Endpoint:** `POST /api/RoleSpecificUser/doctor`

**Request Body:**

```json
{
	"email": "dr.ahmed@hospital.com",
	"password": "Doctor123!",
	"firstName": "Ahmed",
	"lastName": "Hassan",
	"specialty": "Cardiology",
	"hospitalId": "HOSP001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Doctor created successfully",
	"data": {
		"id": "Ahmed_Hassan_CairoUniversityHospital_001",
		"email": "dr.ahmed@hospital.com",
		"firstName": "Ahmed",
		"lastName": "Hassan",
		"role": "Doctor",
		"specialty": "Cardiology",
		"hospitalId": "HOSP001"
	}
}
```

### 2. Engineer Registration

**Endpoint:** `POST /api/RoleSpecificUser/engineer`

**Request Body:**

```json
{
	"email": "eng.mohamed@company.com",
	"password": "Engineer123!",
	"firstName": "Mohamed",
	"lastName": "Ali",
	"departmentId": "DEPT001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Engineer created successfully",
	"data": {
		"id": "Mohamed_Ali_Engineering_001",
		"email": "eng.mohamed@company.com",
		"firstName": "Mohamed",
		"lastName": "Ali",
		"role": "Engineer",
		"departmentId": "DEPT001"
	}
}
```

### 3. Technician Registration

**Endpoint:** `POST /api/RoleSpecificUser/technician`

**Request Body:**

```json
{
	"email": "tech.sara@hospital.com",
	"password": "Technician123!",
	"firstName": "Sara",
	"lastName": "Mahmoud",
	"hospitalId": "HOSP001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Technician created successfully",
	"data": {
		"id": "Sara_Mahmoud_CairoUniversityHospital_001",
		"email": "tech.sara@hospital.com",
		"firstName": "Sara",
		"lastName": "Mahmoud",
		"role": "Technician",
		"hospitalId": "HOSP001"
	}
}
```

### 4. Admin Registration

**Endpoint:** `POST /api/RoleSpecificUser/admin`

**Request Body:**

```json
{
	"email": "admin@company.com",
	"password": "Admin123!",
	"firstName": "Admin",
	"lastName": "User",
	"departmentId": "DEPT001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Admin created successfully",
	"data": {
		"id": "Admin_User_Administration_001",
		"email": "admin@company.com",
		"firstName": "Admin",
		"lastName": "User",
		"role": "Admin",
		"departmentId": "DEPT001"
	}
}
```

### 5. Finance Manager Registration

**Endpoint:** `POST /api/RoleSpecificUser/finance-manager`

**Request Body:**

```json
{
	"email": "finance@company.com",
	"password": "Finance123!",
	"firstName": "Finance",
	"lastName": "Manager",
	"departmentId": "DEPT001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Finance Manager created successfully",
	"data": {
		"id": "Finance_Manager_Finance_001",
		"email": "finance@company.com",
		"firstName": "Finance",
		"lastName": "Manager",
		"role": "FinanceManager",
		"departmentId": "DEPT001"
	}
}
```

### 6. Legal Manager Registration

**Endpoint:** `POST /api/RoleSpecificUser/legal-manager`

**Request Body:**

```json
{
	"email": "legal@company.com",
	"password": "Legal123!",
	"firstName": "Legal",
	"lastName": "Manager",
	"departmentId": "DEPT001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Legal Manager created successfully",
	"data": {
		"id": "Legal_Manager_Legal_001",
		"email": "legal@company.com",
		"firstName": "Legal",
		"lastName": "Manager",
		"role": "LegalManager",
		"departmentId": "DEPT001"
	}
}
```

### 7. Salesman Registration

**Endpoint:** `POST /api/RoleSpecificUser/salesman`

**Request Body:**

```json
{
	"email": "sales@company.com",
	"password": "Sales123!",
	"firstName": "Sales",
	"lastName": "Person",
	"departmentId": "DEPT001"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Salesman created successfully",
	"data": {
		"id": "Sales_Person_Sales_001",
		"email": "sales@company.com",
		"firstName": "Sales",
		"lastName": "Person",
		"role": "Salesman",
		"departmentId": "DEPT001"
	}
}
```

## Password Management

### Change Password

**Endpoint:** `POST /api/Account/change-password`

**Request Body:**

```json
{
	"currentPassword": "OldPassword123!",
	"newPassword": "NewPassword123!",
	"confirmPassword": "NewPassword123!"
}
```

**Response:**

```json
{
	"success": true,
	"message": "Password changed successfully"
}
```

**Error Response:**

```json
{
	"success": false,
	"message": "Current password is incorrect"
}
```

## API Endpoints Summary

| Method | Endpoint                                | Description            | Authorization    |
| ------ | --------------------------------------- | ---------------------- | ---------------- |
| POST   | `/api/Account/login`                    | User login             | None             |
| POST   | `/api/Account/change-password`          | Change password        | Authenticated    |
| POST   | `/api/RoleSpecificUser/doctor`          | Create doctor          | SuperAdmin/Admin |
| POST   | `/api/RoleSpecificUser/engineer`        | Create engineer        | SuperAdmin/Admin |
| POST   | `/api/RoleSpecificUser/technician`      | Create technician      | SuperAdmin/Admin |
| POST   | `/api/RoleSpecificUser/admin`           | Create admin           | SuperAdmin       |
| POST   | `/api/RoleSpecificUser/finance-manager` | Create finance manager | SuperAdmin/Admin |
| POST   | `/api/RoleSpecificUser/legal-manager`   | Create legal manager   | SuperAdmin/Admin |
| POST   | `/api/RoleSpecificUser/salesman`        | Create salesman        | SuperAdmin/Admin |
| GET    | `/api/User/me`                          | Get current user info  | Authenticated    |
| GET    | `/api/User/all`                         | Get all users          | SuperAdmin/Admin |

## User ID Format

The system automatically generates user IDs in the following format:

- **Doctors & Technicians:** `FirstName_Lastname_Hospitalname_001`
- **All Other Roles:** `FirstName_Lastname_Departmentname_001`

**Examples:**

- `Ahmed_Hassan_CairoUniversityHospital_001` (Doctor)
- `Mohamed_Ali_Engineering_001` (Engineer)
- `Sara_Mahmoud_Administration_001` (Admin)

## Password Requirements

- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

## Error Handling

### Common Error Codes

| Code | Description           | Solution                               |
| ---- | --------------------- | -------------------------------------- |
| 400  | Bad Request           | Check request body format              |
| 401  | Unauthorized          | Provide valid authentication token     |
| 403  | Forbidden             | User doesn't have required permissions |
| 409  | Conflict              | User already exists                    |
| 500  | Internal Server Error | Contact system administrator           |

### Example Error Response

```json
{
	"type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
	"title": "One or more validation errors occurred.",
	"status": 400,
	"errors": {
		"Email": ["The Email field is required."],
		"Password": ["The Password field is required."]
	}
}
```

## Testing Examples

### 1. Login and Get Token

```bash
curl -X POST "http://localhost:5117/api/Account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "hemdan@hemdan.com",
    "password": "356120Ahmed@sharf"
  }'
```

### 2. Create a Doctor

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/doctor" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "email": "dr.ahmed@hospital.com",
    "password": "Doctor123!",
    "firstName": "Ahmed",
    "lastName": "Hassan",
    "specialty": "Cardiology",
    "hospitalId": "HOSP001"
  }'
```

### 3. Change Password

```bash
curl -X POST "http://localhost:5117/api/Account/change-password" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "currentPassword": "OldPassword123!",
    "newPassword": "NewPassword123!",
    "confirmPassword": "NewPassword123!"
  }'
```

## Notes

- All user registrations require valid email addresses
- User IDs are automatically generated and cannot be manually set
- Password changes require the current password for security
- All endpoints return JSON responses
- Authentication tokens expire after 5 years
- SuperAdmin accounts can only be created through the system scripts
