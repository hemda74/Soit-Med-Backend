# User Management APIs Documentation

This document describes the enhanced user management APIs for SuperAdmin operations.

## Overview

The user management system now includes comprehensive APIs for user activation/deactivation, statistics, and advanced filtering/sorting capabilities.

## New APIs

### 1. User Activation/Deactivation

**Endpoint:** `PUT /api/User/activate-deactivate`  
**Authorization:** SuperAdmin only  
**Description:** Allows SuperAdmin to activate or deactivate users

#### Request Body

```json
{
	"userId": "string (required)",
	"action": "string (required) - 'activate' or 'deactivate'",
	"reason": "string (optional) - Reason for the action"
}
```

#### Response

```json
{
	"userId": "string",
	"userName": "string",
	"email": "string",
	"isActive": "boolean",
	"action": "string",
	"reason": "string",
	"actionDate": "datetime",
	"message": "string"
}
```

#### Example Usage

```bash
PUT /api/User/activate-deactivate
{
  "userId": "DOC001_2024_01_15",
  "action": "deactivate",
  "reason": "Violation of hospital policies"
}
```

### 2. User Statistics (Detailed)

**Endpoint:** `GET /api/User/statistics`  
**Authorization:** SuperAdmin only  
**Description:** Returns comprehensive user statistics

#### Response

```json
{
	"totalUsers": 150,
	"activeUsers": 142,
	"inactiveUsers": 8,
	"usersByRole": 150,
	"generatedAt": "2024-01-15T10:30:00Z",
	"usersByRoleBreakdown": {
		"Doctor": 45,
		"Engineer": 12,
		"Technician": 38,
		"Admin": 8,
		"FinanceManager": 15,
		"LegalManager": 10,
		"Salesman": 22
	},
	"usersByDepartment": {
		"Medical": 83,
		"Engineering": 12,
		"Administration": 8,
		"Finance": 15,
		"Legal": 10,
		"Sales": 22
	}
}
```

### 3. User Counts (Simple)

**Endpoint:** `GET /api/User/counts`  
**Authorization:** SuperAdmin only  
**Description:** Returns basic user counts

#### Response

```json
{
	"totalUsers": 150,
	"activeUsers": 142,
	"inactiveUsers": 8,
	"generatedAt": "2024-01-15T10:30:00Z"
}
```

### 4. Enhanced Get All Users (with Filtering & Sorting)

**Endpoint:** `GET /api/User/all`  
**Authorization:** SuperAdmin, Admin, Doctor  
**Description:** Returns paginated, filtered, and sorted user data

#### Query Parameters

| Parameter      | Type     | Description                                                 | Default     |
| -------------- | -------- | ----------------------------------------------------------- | ----------- |
| `searchTerm`   | string   | Search in name, email, username                             | null        |
| `role`         | string   | Filter by specific role                                     | null        |
| `departmentId` | int      | Filter by department ID                                     | null        |
| `isActive`     | boolean  | Filter by active status                                     | null        |
| `createdFrom`  | datetime | Filter by creation date (from)                              | null        |
| `createdTo`    | datetime | Filter by creation date (to)                                | null        |
| `sortBy`       | string   | Sort field: CreatedAt, FirstName, LastName, Email, IsActive | "CreatedAt" |
| `sortOrder`    | string   | Sort order: asc, desc                                       | "desc"      |
| `pageNumber`   | int      | Page number for pagination                                  | 1           |
| `pageSize`     | int      | Number of items per page                                    | 50          |

#### Response

```json
{
	"users": [
		{
			"id": "string",
			"userName": "string",
			"email": "string",
			"firstName": "string",
			"lastName": "string",
			"fullName": "string",
			"isActive": "boolean",
			"createdAt": "datetime",
			"lastLoginAt": "datetime",
			"roles": ["string"],
			"departmentId": "int",
			"departmentName": "string",
			"departmentDescription": "string"
		}
	],
	"totalCount": 150,
	"pageNumber": 1,
	"pageSize": 50,
	"totalPages": 3,
	"hasPreviousPage": false,
	"hasNextPage": true,
	"appliedFilters": {
		"searchTerm": "john",
		"role": "Doctor",
		"departmentId": 1,
		"isActive": true,
		"createdFrom": "2024-01-01T00:00:00Z",
		"createdTo": "2024-12-31T23:59:59Z",
		"sortBy": "CreatedAt",
		"sortOrder": "desc",
		"pageNumber": 1,
		"pageSize": 50
	}
}
```

## Example API Calls

### 1. Get All Active Doctors

```bash
GET /api/User/all?role=Doctor&isActive=true&sortBy=FirstName&sortOrder=asc
```

### 2. Search Users by Name

```bash
GET /api/User/all?searchTerm=john&pageSize=20
```

### 3. Get Users Created This Month

```bash
GET /api/User/all?createdFrom=2024-01-01T00:00:00Z&createdTo=2024-01-31T23:59:59Z
```

### 4. Get Users by Department

```bash
GET /api/User/all?departmentId=1&sortBy=CreatedAt&sortOrder=desc
```

### 5. Deactivate a User

```bash
PUT /api/User/activate-deactivate
{
  "userId": "DOC001_2024_01_15",
  "action": "deactivate",
  "reason": "Account compromised"
}
```

### 6. Get User Statistics

```bash
GET /api/User/statistics
```

## Error Handling

All APIs use the enhanced validation system with detailed error messages:

### Validation Errors

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"UserId": ["User ID is required"],
		"Action": ["Action must be either 'activate' or 'deactivate'"]
	},
	"generalErrors": [],
	"timestamp": "2024-01-15T10:30:00Z"
}
```

### Business Logic Errors

```json
{
	"success": false,
	"message": "User with ID 'INVALID_ID' not found",
	"field": "UserId",
	"code": "USER_NOT_FOUND",
	"timestamp": "2024-01-15T10:30:00Z"
}
```

## Security Features

1. **SuperAdmin Protection**: Cannot deactivate SuperAdmin users
2. **Role-based Access**: Different endpoints have different authorization levels
3. **Input Validation**: All inputs are validated with detailed error messages
4. **Audit Trail**: All activation/deactivation actions are logged with timestamps

## Performance Considerations

1. **Pagination**: Large user lists are paginated to improve performance
2. **Efficient Queries**: Database queries are optimized with proper indexing
3. **Caching**: User statistics can be cached for better performance
4. **Filtering**: Server-side filtering reduces data transfer

## DTOs

### UserActivationDTO

- `UserId` (required): User ID to activate/deactivate
- `Action` (required): "activate" or "deactivate"
- `Reason` (optional): Reason for the action

### UserFilterDTO

- `SearchTerm`: Search in name, email, username
- `Role`: Filter by specific role
- `DepartmentId`: Filter by department
- `IsActive`: Filter by active status
- `CreatedFrom/To`: Date range filtering
- `SortBy`: Sort field
- `SortOrder`: Sort direction
- `PageNumber/PageSize`: Pagination

### UserStatisticsDTO

- `TotalUsers`: Total number of users
- `ActiveUsers`: Number of active users
- `InactiveUsers`: Number of inactive users
- `UsersByRoleBreakdown`: Count by each role
- `UsersByDepartment`: Count by each department
- `GeneratedAt`: Timestamp of statistics generation

## Benefits

1. **Comprehensive User Management**: Full control over user activation status
2. **Detailed Analytics**: Rich statistics for system monitoring
3. **Advanced Filtering**: Powerful search and filter capabilities
4. **Pagination**: Efficient handling of large user lists
5. **Security**: Protected operations with proper authorization
6. **User-Friendly**: Clear error messages and responses
7. **Audit Trail**: Complete tracking of user management actions

This enhanced user management system provides SuperAdmins with powerful tools to manage users effectively while maintaining security and performance.

