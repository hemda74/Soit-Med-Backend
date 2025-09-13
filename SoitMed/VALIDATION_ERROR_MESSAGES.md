# Role-Specific User Creation API Validation Error Messages

This document describes the improved validation error messages for the role-specific user creation APIs.

## Overview

All role-specific user creation endpoints now return detailed, user-friendly validation error messages that help identify exactly which fields have issues and what needs to be corrected.

## API Endpoints

The following endpoints have been enhanced with improved validation messages:

- `POST /api/RoleSpecificUser/doctor`
- `POST /api/RoleSpecificUser/engineer`
- `POST /api/RoleSpecificUser/technician`
- `POST /api/RoleSpecificUser/admin`
- `POST /api/RoleSpecificUser/finance-manager`
- `POST /api/RoleSpecificUser/legal-manager`
- `POST /api/RoleSpecificUser/salesman`

## Error Response Format

### Model Validation Errors

When input validation fails, the API returns a structured error response:

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"Email": ["Email is required"],
		"Password": ["Password must be at least 6 characters long"],
		"Specialty": ["Medical specialty is required"],
		"HospitalId": ["Hospital ID is required"]
	},
	"generalErrors": [],
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### Business Logic Validation Errors

When business logic validation fails (e.g., hospital not found), the API returns:

```json
{
	"success": false,
	"message": "Hospital with ID 'HOSP123' not found. Please verify the hospital ID is correct.",
	"field": "HospitalId",
	"code": "HOSPITAL_NOT_FOUND",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### Multiple Business Logic Errors

For multiple validation issues:

```json
{
	"success": false,
	"message": "User creation failed. Please check the following issues:",
	"errors": {
		"Password": "Password must contain at least one uppercase letter; Password must contain at least one digit"
	},
	"timestamp": "2024-01-15T10:30:00Z"
}
```

## Field-Specific Validation Messages

### Common Fields (All Roles)

| Field          | Validation Rule | Error Message                                 |
| -------------- | --------------- | --------------------------------------------- |
| `Email`        | Required        | "Email is required"                           |
| `Email`        | Invalid format  | "Please provide a valid email address"        |
| `Email`        | Too long        | "Email cannot exceed 256 characters"          |
| `Password`     | Required        | "Password is required"                        |
| `Password`     | Too short       | "Password must be at least 6 characters long" |
| `Password`     | Too long        | "Password cannot exceed 100 characters"       |
| `FirstName`    | Too long        | "First name cannot exceed 100 characters"     |
| `LastName`     | Too long        | "Last name cannot exceed 100 characters"      |
| `DepartmentId` | Invalid range   | "Department ID must be a positive number"     |

### Doctor-Specific Fields

| Field        | Validation Rule | Error Message                                                               |
| ------------ | --------------- | --------------------------------------------------------------------------- |
| `Specialty`  | Required        | "Medical specialty is required"                                             |
| `Specialty`  | Too long        | "Specialty cannot exceed 100 characters"                                    |
| `HospitalId` | Required        | "Hospital ID is required"                                                   |
| `HospitalId` | Too long        | "Hospital ID cannot exceed 50 characters"                                   |
| `HospitalId` | Not found       | "Hospital with ID 'X' not found. Please verify the hospital ID is correct." |

### Engineer-Specific Fields

| Field            | Validation Rule | Error Message                                                                            |
| ---------------- | --------------- | ---------------------------------------------------------------------------------------- |
| `Specialty`      | Required        | "Engineering specialty is required"                                                      |
| `Specialty`      | Too long        | "Specialty cannot exceed 100 characters"                                                 |
| `GovernorateIds` | Required        | "At least one governorate must be assigned"                                              |
| `GovernorateIds` | Empty list      | "At least one governorate must be assigned"                                              |
| `GovernorateIds` | Not found       | "Governorates with IDs [X, Y] not found. Please verify the governorate IDs are correct." |

### Technician-Specific Fields

| Field        | Validation Rule | Error Message                                                               |
| ------------ | --------------- | --------------------------------------------------------------------------- |
| `Department` | Required        | "Technical department is required"                                          |
| `Department` | Too long        | "Department cannot exceed 100 characters"                                   |
| `HospitalId` | Required        | "Hospital ID is required"                                                   |
| `HospitalId` | Too long        | "Hospital ID cannot exceed 50 characters"                                   |
| `HospitalId` | Not found       | "Hospital with ID 'X' not found. Please verify the hospital ID is correct." |

### Admin-Specific Fields

| Field         | Validation Rule | Error Message                              |
| ------------- | --------------- | ------------------------------------------ |
| `AccessLevel` | Too long        | "Access level cannot exceed 50 characters" |

### Finance Manager Fields

No additional validation beyond common fields.

### Legal Manager Fields

| Field            | Validation Rule | Error Message                                  |
| ---------------- | --------------- | ---------------------------------------------- |
| `LegalSpecialty` | Too long        | "Legal specialty cannot exceed 100 characters" |

### Salesman Fields

| Field       | Validation Rule | Error Message                            |
| ----------- | --------------- | ---------------------------------------- |
| `Territory` | Too long        | "Territory cannot exceed 100 characters" |

## System-Level Validation Messages

### Department Not Found

When the required department doesn't exist in the system:

```json
{
	"success": false,
	"message": "Medical department not found. Please ensure departments are seeded in the system.",
	"field": "DepartmentId",
	"code": "DEPARTMENT_NOT_FOUND",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### User Creation Failures

When ASP.NET Identity user creation fails:

```json
{
	"success": false,
	"message": "User creation failed. Please check the following issues:",
	"errors": {
		"Password": "Password must contain at least one uppercase letter; Password must contain at least one digit"
	},
	"timestamp": "2024-01-15T10:30:00Z"
}
```

## Error Codes

| Code                    | Description                                         |
| ----------------------- | --------------------------------------------------- |
| `HOSPITAL_NOT_FOUND`    | The specified hospital ID doesn't exist             |
| `DEPARTMENT_NOT_FOUND`  | The required department doesn't exist in the system |
| `GOVERNORATE_NOT_FOUND` | One or more specified governorate IDs don't exist   |

## Testing the Validation

### Example: Testing Doctor Creation with Invalid Data

**Request:**

```json
POST /api/RoleSpecificUser/doctor
{
  "email": "invalid-email",
  "password": "123",
  "specialty": "",
  "hospitalId": "NONEXISTENT"
}
```

**Response:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"Email": ["Please provide a valid email address"],
		"Password": ["Password must be at least 6 characters long"],
		"Specialty": ["Medical specialty is required"]
	},
	"generalErrors": [],
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### Example: Testing Engineer Creation with Invalid Governorates

**Request:**

```json
POST /api/RoleSpecificUser/engineer
{
  "email": "engineer@example.com",
  "password": "password123",
  "specialty": "Mechanical",
  "governorateIds": [999, 1000]
}
```

**Response:**

```json
{
	"success": false,
	"message": "Governorates with IDs [999, 1000] not found. Please verify the governorate IDs are correct.",
	"field": "GovernorateIds",
	"code": "GOVERNORATE_NOT_FOUND",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

## Benefits

1. **Clear Field Identification**: Each error message clearly identifies which field has the issue
2. **Specific Error Messages**: Detailed messages explain exactly what's wrong and how to fix it
3. **Consistent Format**: All validation errors follow the same response structure
4. **User-Friendly**: Error messages are written in plain language that users can understand
5. **Developer-Friendly**: Error codes and structured responses make it easy for frontend developers to handle different error types
6. **Comprehensive Coverage**: Both model validation and business logic validation are covered

## Implementation Details

The improved validation is implemented using:

1. **Enhanced DTOs**: Added detailed validation attributes with custom error messages
2. **ValidationHelperService**: A service class that formats validation errors consistently
3. **Controller Updates**: All role-specific user creation endpoints now use the improved error handling
4. **Business Logic Validation**: Custom validation for hospital existence, governorate validation, etc.

This ensures that users get clear, actionable feedback when creating role-specific users, making the API much more user-friendly and easier to debug.
