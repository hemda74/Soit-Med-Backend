# User PATCH API Documentation

## Overview

This document provides comprehensive documentation for the User PATCH API endpoint, designed for React frontend integration.

## Endpoint Details

### PATCH /api/User/{userId}

**Description:** Updates a user's information in the system by their unique user ID.

**Authorization:** Required - SuperAdmin or Admin roles only

**Method:** PATCH

**URL:** `http://localhost:5117/api/User/{userId}`

---

## Request

### Path Parameters

| Parameter | Type   | Required | Description                                 |
| --------- | ------ | -------- | ------------------------------------------- |
| `userId`  | string | Yes      | The unique identifier of the user to update |

### Headers

| Header          | Type   | Required | Description                     |
| --------------- | ------ | -------- | ------------------------------- |
| `Authorization` | string | Yes      | Bearer token for authentication |
| `Content-Type`  | string | Yes      | application/json                |

### Request Body

| Field       | Type   | Required | Description                             |
| ----------- | ------ | -------- | --------------------------------------- |
| `email`     | string | Yes      | User's email address (used as username) |
| `role`      | string | Yes      | User's role (must be a valid role)      |
| `firstName` | string | Yes      | User's first name                       |
| `lastName`  | string | Yes      | User's last name                        |
| `password`  | string | No       | User's new password (optional)          |

### Example Request

```http
PATCH /api/User/Nada_Nada_Sales_001
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "email": "updated.nada@test.com",
  "password": "NewPassword123!",
  "role": "Salesman",
  "firstName": "Updated",
  "lastName": "Nada"
}
```

---

## Response

### Success Response (200 OK)

```json
{
	"success": true,
	"message": "User 'updated.nada@test.com' updated successfully",
	"updatedUserId": "Nada_Nada_Sales_001",
	"updatedUserName": "updated.nada@test.com",
	"newRole": "Salesman",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

### Error Responses

#### 400 Bad Request - User ID Required

```json
{
	"success": false,
	"message": "User ID is required",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 400 Bad Request - Role Required

```json
{
	"success": false,
	"message": "Role field is required",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 400 Bad Request - Invalid Role

```json
{
	"success": false,
	"message": "Invalid role. Valid roles are: SuperAdmin, Admin, Doctor, Technician, Engineer, SalesManager, Salesman, FinanceManager, FinanceEmployee, LegalManager, LegalEmployee",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 400 Bad Request - Cannot Update SuperAdmin

```json
{
	"success": false,
	"message": "Cannot update SuperAdmin user",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 400 Bad Request - Password Update Failed

```json
{
	"success": false,
	"message": "Failed to update password",
	"errors": ["Password must be at least 6 characters long"],
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 400 Bad Request - User Update Failed

```json
{
	"success": false,
	"message": "Failed to update user",
	"errors": ["Email is already taken"],
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 401 Unauthorized

```json
{
	"success": false,
	"message": "Unauthorized access",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 403 Forbidden

```json
{
	"success": false,
	"message": "Access denied. SuperAdmin or Admin role required",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

#### 404 Not Found

```json
{
	"success": false,
	"message": "User with ID 'user-id-here' not found",
	"timestamp": "2025-09-24T18:10:53.9348577Z"
}
```

---

## Business Rules

### Authorization

- Only users with `SuperAdmin` or `Admin` roles can update users
- SuperAdmin users cannot be updated (protected)

### Validation

- User ID must be provided and not empty
- User must exist in the system
- User must not be a SuperAdmin
- Role must be provided and valid
- Email must be unique (if changed)
- Password is optional but must meet requirements if provided

### Valid Roles

- `SuperAdmin`
- `Admin`
- `Doctor`
- `Technician`
- `Engineer`
- `SalesManager`
- `Salesman`
- `FinanceManager`
- `FinanceEmployee`
- `LegalManager`
- `LegalEmployee`

---

## TypeScript Interfaces

### Request Interface

```typescript
interface UpdateUserRequest {
	email: string;
	role: string;
	firstName: string;
	lastName: string;
	password?: string;
}
```

### Success Response Interface

```typescript
interface UpdateUserSuccessResponse {
	success: true;
	message: string;
	updatedUserId: string;
	updatedUserName: string;
	newRole: string;
	timestamp: string;
}
```

### Error Response Interface

```typescript
interface UpdateUserErrorResponse {
	success: false;
	message: string;
	errors?: string[];
	timestamp: string;
}
```

### Combined Response Type

```typescript
type UpdateUserResponse = UpdateUserSuccessResponse | UpdateUserErrorResponse;
```

---

## React Implementation Examples

### Basic Update Function

```typescript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5117';

interface UpdateUserParams {
	userId: string;
	userData: UpdateUserRequest;
	token: string;
}

export const updateUser = async ({
	userId,
	userData,
	token,
}: UpdateUserParams): Promise<UpdateUserResponse> => {
	try {
		const response = await axios.patch(
			`${API_BASE_URL}/api/User/${userId}`,
			userData,
			{
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'application/json',
				},
			}
		);

		return response.data;
	} catch (error: any) {
		if (error.response) {
			return error.response.data;
		}
		throw error;
	}
};
```

### React Hook Implementation

```typescript
import { useState } from 'react';

interface UseUpdateUserReturn {
	updateUser: (
		userId: string,
		userData: UpdateUserRequest
	) => Promise<void>;
	isLoading: boolean;
	error: string | null;
	success: boolean;
}

export const useUpdateUser = (token: string): UseUpdateUserReturn => {
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const [success, setSuccess] = useState(false);

	const updateUser = async (
		userId: string,
		userData: UpdateUserRequest
	): Promise<void> => {
		setIsLoading(true);
		setError(null);
		setSuccess(false);

		try {
			const response = await updateUser({
				userId,
				userData,
				token,
			});

			if (response.success) {
				setSuccess(true);
				console.log(
					'User updated successfully:',
					response.message
				);
			} else {
				setError(response.message);
			}
		} catch (err: any) {
			setError(err.message || 'An unexpected error occurred');
		} finally {
			setIsLoading(false);
		}
	};

	return { updateUser, isLoading, error, success };
};
```

### React Component Example

```typescript
import React, { useState } from 'react';
import { useUpdateUser } from './hooks/useUpdateUser';

interface UserUpdateFormProps {
	userId: string;
	currentUser: {
		email: string;
		firstName: string;
		lastName: string;
		role: string;
	};
	token: string;
	onUserUpdated?: () => void;
}

export const UserUpdateForm: React.FC<UserUpdateFormProps> = ({
	userId,
	currentUser,
	token,
	onUserUpdated,
}) => {
	const { updateUser, isLoading, error, success } = useUpdateUser(token);
	const [formData, setFormData] = useState({
		email: currentUser.email,
		firstName: currentUser.firstName,
		lastName: currentUser.lastName,
		role: currentUser.role,
		password: '',
	});

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();

		const updateData: UpdateUserRequest = {
			email: formData.email,
			firstName: formData.firstName,
			lastName: formData.lastName,
			role: formData.role,
		};

		if (formData.password) {
			updateData.password = formData.password;
		}

		await updateUser(userId, updateData);
		if (success) {
			onUserUpdated?.();
		}
	};

	return (
		<form
			onSubmit={handleSubmit}
			className="space-y-4"
		>
			<div>
				<label className="block text-sm font-medium text-gray-700">
					Email
				</label>
				<input
					type="email"
					value={formData.email}
					onChange={(e) =>
						setFormData({
							...formData,
							email: e.target.value,
						})
					}
					className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
					required
				/>
			</div>

			<div>
				<label className="block text-sm font-medium text-gray-700">
					First Name
				</label>
				<input
					type="text"
					value={formData.firstName}
					onChange={(e) =>
						setFormData({
							...formData,
							firstName: e.target
								.value,
						})
					}
					className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
					required
				/>
			</div>

			<div>
				<label className="block text-sm font-medium text-gray-700">
					Last Name
				</label>
				<input
					type="text"
					value={formData.lastName}
					onChange={(e) =>
						setFormData({
							...formData,
							lastName: e.target
								.value,
						})
					}
					className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
					required
				/>
			</div>

			<div>
				<label className="block text-sm font-medium text-gray-700">
					Role
				</label>
				<select
					value={formData.role}
					onChange={(e) =>
						setFormData({
							...formData,
							role: e.target.value,
						})
					}
					className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
					required
				>
					<option value="Admin">Admin</option>
					<option value="Doctor">Doctor</option>
					<option value="Technician">
						Technician
					</option>
					<option value="Engineer">
						Engineer
					</option>
					<option value="SalesManager">
						Sales Manager
					</option>
					<option value="Salesman">
						Salesman
					</option>
					<option value="FinanceManager">
						Finance Manager
					</option>
					<option value="FinanceEmployee">
						Finance Employee
					</option>
					<option value="LegalManager">
						Legal Manager
					</option>
					<option value="LegalEmployee">
						Legal Employee
					</option>
				</select>
			</div>

			<div>
				<label className="block text-sm font-medium text-gray-700">
					New Password (Optional)
				</label>
				<input
					type="password"
					value={formData.password}
					onChange={(e) =>
						setFormData({
							...formData,
							password: e.target
								.value,
						})
					}
					className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500"
					placeholder="Leave empty to keep current password"
				/>
			</div>

			<button
				type="submit"
				disabled={isLoading}
				className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
			>
				{isLoading ? 'Updating...' : 'Update User'}
			</button>

			{error && (
				<div className="text-red-500 text-sm">
					Error: {error}
				</div>
			)}

			{success && (
				<div className="text-green-500 text-sm">
					User updated successfully!
				</div>
			)}
		</form>
	);
};
```

---

## Error Handling Best Practices

### 1. Handle Different Error Types

```typescript
const handleUpdateError = (error: UpdateUserErrorResponse) => {
	switch (error.message) {
		case "User with ID 'user-id-here' not found":
			return 'User not found. Please refresh the page and try again.';
		case 'Cannot update SuperAdmin user':
			return 'SuperAdmin users cannot be updated.';
		case 'Role field is required':
			return 'Please select a role for the user.';
		case 'Invalid role. Valid roles are: ...':
			return 'Please select a valid role from the dropdown.';
		case 'Failed to update password':
			return 'Password update failed. Please check the password requirements.';
		case 'Failed to update user':
			return 'User update failed. Please check the provided information.';
		default:
			return error.message || 'An unexpected error occurred.';
	}
};
```

### 2. Show User-Friendly Messages

```typescript
const getErrorMessage = (error: UpdateUserErrorResponse): string => {
	const errorMessages: Record<string, string> = {
		"User with ID 'user-id-here' not found":
			'This user no longer exists.',
		'Cannot update SuperAdmin user':
			'SuperAdmin accounts are protected and cannot be updated.',
		'Role field is required': 'Please select a role for the user.',
		'Invalid role. Valid roles are: ...':
			'Please select a valid role from the available options.',
		'Failed to update password':
			'Unable to update password. Please ensure it meets the requirements.',
		'Failed to update user':
			'Unable to update user. Please check all fields and try again.',
	};

	return errorMessages[error.message] || 'An unexpected error occurred.';
};
```

---

## Testing Examples

### Unit Test Example

```typescript
import { updateUser } from './userService';

describe('updateUser', () => {
	it('should update user successfully', async () => {
		const mockResponse = {
			success: true,
			message: 'User updated successfully',
			updatedUserId: 'test-user-id',
			updatedUserName: 'test@example.com',
			newRole: 'Salesman',
			timestamp: '2025-09-24T18:10:53.9348577Z',
		};

		jest.spyOn(axios, 'patch').mockResolvedValue({
			data: mockResponse,
		});

		const result = await updateUser({
			userId: 'test-user-id',
			userData: {
				email: 'test@example.com',
				role: 'Salesman',
				firstName: 'Test',
				lastName: 'User',
			},
			token: 'test-token',
		});

		expect(result).toEqual(mockResponse);
	});

	it('should handle user not found error', async () => {
		const mockError = {
			response: {
				status: 404,
				data: {
					success: false,
					message: "User with ID 'non-existent' not found",
					timestamp: '2025-09-24T18:10:53.9348577Z',
				},
			},
		};

		jest.spyOn(axios, 'patch').mockRejectedValue(mockError);

		const result = await updateUser({
			userId: 'non-existent',
			userData: {
				email: 'test@example.com',
				role: 'Salesman',
				firstName: 'Test',
				lastName: 'User',
			},
			token: 'test-token',
		});

		expect(result.success).toBe(false);
		expect(result.message).toContain('not found');
	});

	it('should handle SuperAdmin protection', async () => {
		const mockError = {
			response: {
				status: 400,
				data: {
					success: false,
					message: 'Cannot update SuperAdmin user',
					timestamp: '2025-09-24T18:10:53.9348577Z',
				},
			},
		};

		jest.spyOn(axios, 'patch').mockRejectedValue(mockError);

		const result = await updateUser({
			userId: 'superadmin-id',
			userData: {
				email: 'test@example.com',
				role: 'Salesman',
				firstName: 'Test',
				lastName: 'User',
			},
			token: 'test-token',
		});

		expect(result.success).toBe(false);
		expect(result.message).toContain('Cannot update SuperAdmin');
	});
});
```

---

## Security Considerations

1. **Authentication Required**: All requests must include a valid JWT token
2. **Role-Based Access**: Only SuperAdmin and Admin users can update users
3. **SuperAdmin Protection**: SuperAdmin accounts cannot be updated
4. **Input Validation**: All fields are validated before processing
5. **Password Security**: Passwords are hashed using secure methods
6. **Email Uniqueness**: Email addresses must be unique across the system

---

## Rate Limiting

Currently, no rate limiting is implemented. Consider implementing rate limiting for production use.

---

## Changelog

### Version 1.0.0 (2025-09-24)

- Initial implementation
- Changed from PUT to PATCH method
- Changed from username-based to user ID-based updates
- Added comprehensive error handling
- Added business rule validations
- Added protection against SuperAdmin updates
- Added optional password update functionality

---

## Support

For technical support or questions about this API, please contact the backend development team.

---

_This documentation is generated for React frontend integration and should be kept up to date with any API changes._
