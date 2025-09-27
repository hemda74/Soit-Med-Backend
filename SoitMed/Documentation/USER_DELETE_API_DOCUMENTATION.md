# User Delete API Documentation

## Overview

This document provides comprehensive documentation for the User Delete API endpoint, designed for React frontend integration.

## Endpoint Details

### DELETE /api/User/{userId}

**Description:** Deletes a user from the system by their unique user ID.

**Authorization:** Required - SuperAdmin or Admin roles only

**Method:** DELETE

**URL:** `http://localhost:5117/api/User/{userId}`

---

## Request

### Path Parameters

| Parameter | Type   | Required | Description                                 |
| --------- | ------ | -------- | ------------------------------------------- |
| `userId`  | string | Yes      | The unique identifier of the user to delete |

### Headers

| Header          | Type   | Required | Description                     |
| --------------- | ------ | -------- | ------------------------------- |
| `Authorization` | string | Yes      | Bearer token for authentication |
| `Content-Type`  | string | Yes      | application/json                |

### Example Request

```http
DELETE /api/User/0e545c91-ccc0-4824-ae30-a4c62c106650
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

---

## Response

### Success Response (200 OK)

```json
{
	"success": true,
	"message": "User 'username@example.com' deleted successfully",
	"deletedUserId": "0e545c91-ccc0-4824-ae30-a4c62c106650",
	"deletedUserName": "username@example.com",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

### Error Responses

#### 400 Bad Request - User ID Required

```json
{
	"success": false,
	"message": "User ID is required",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 400 Bad Request - Cannot Delete SuperAdmin

```json
{
	"success": false,
	"message": "Cannot delete SuperAdmin user",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 400 Bad Request - Cannot Delete Own Account

```json
{
	"success": false,
	"message": "Cannot delete your own account",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 400 Bad Request - Delete Failed

```json
{
	"success": false,
	"message": "Failed to delete user",
	"errors": ["Error description 1", "Error description 2"],
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 401 Unauthorized

```json
{
	"success": false,
	"message": "Unauthorized access",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 403 Forbidden

```json
{
	"success": false,
	"message": "Access denied. SuperAdmin or Admin role required",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

#### 404 Not Found

```json
{
	"success": false,
	"message": "User with ID 'user-id-here' not found",
	"timestamp": "2025-09-24T17:58:55.9581614Z"
}
```

---

## Business Rules

### Authorization

- Only users with `SuperAdmin` or `Admin` roles can delete users
- Users cannot delete their own accounts
- SuperAdmin users cannot be deleted

### Validation

- User ID must be provided and not empty
- User must exist in the system
- User must not be a SuperAdmin
- User must not be the current authenticated user

---

## TypeScript Interfaces

### Request Interface

```typescript
interface DeleteUserRequest {
	userId: string;
}
```

### Success Response Interface

```typescript
interface DeleteUserSuccessResponse {
	success: true;
	message: string;
	deletedUserId: string;
	deletedUserName: string;
	timestamp: string;
}
```

### Error Response Interface

```typescript
interface DeleteUserErrorResponse {
	success: false;
	message: string;
	errors?: string[];
	timestamp: string;
}
```

### Combined Response Type

```typescript
type DeleteUserResponse = DeleteUserSuccessResponse | DeleteUserErrorResponse;
```

---

## React Implementation Examples

### Basic Delete Function

```typescript
import axios from 'axios';

const API_BASE_URL = 'http://localhost:5117';

interface DeleteUserParams {
	userId: string;
	token: string;
}

export const deleteUser = async ({
	userId,
	token,
}: DeleteUserParams): Promise<DeleteUserResponse> => {
	try {
		const response = await axios.delete(
			`${API_BASE_URL}/api/User/${userId}`,
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

interface UseDeleteUserReturn {
	deleteUser: (userId: string) => Promise<void>;
	isLoading: boolean;
	error: string | null;
	success: boolean;
}

export const useDeleteUser = (token: string): UseDeleteUserReturn => {
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const [success, setSuccess] = useState(false);

	const deleteUser = async (userId: string): Promise<void> => {
		setIsLoading(true);
		setError(null);
		setSuccess(false);

		try {
			const response = await deleteUser({ userId, token });

			if (response.success) {
				setSuccess(true);
				// Optionally show success message
				console.log(
					'User deleted successfully:',
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

	return { deleteUser, isLoading, error, success };
};
```

### React Component Example

```typescript
import React from 'react';
import { useDeleteUser } from './hooks/useDeleteUser';

interface UserDeleteButtonProps {
	userId: string;
	userName: string;
	token: string;
	onUserDeleted?: () => void;
}

export const UserDeleteButton: React.FC<UserDeleteButtonProps> = ({
	userId,
	userName,
	token,
	onUserDeleted,
}) => {
	const { deleteUser, isLoading, error, success } = useDeleteUser(token);

	const handleDelete = async () => {
		if (
			window.confirm(
				`Are you sure you want to delete user "${userName}"?`
			)
		) {
			await deleteUser(userId);
			if (success) {
				onUserDeleted?.();
			}
		}
	};

	return (
		<div>
			<button
				onClick={handleDelete}
				disabled={isLoading}
				className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
			>
				{isLoading ? 'Deleting...' : 'Delete User'}
			</button>

			{error && (
				<div className="text-red-500 mt-2">
					Error: {error}
				</div>
			)}

			{success && (
				<div className="text-green-500 mt-2">
					User deleted successfully!
				</div>
			)}
		</div>
	);
};
```

---

## Error Handling Best Practices

### 1. Handle Different Error Types

```typescript
const handleDeleteError = (error: DeleteUserErrorResponse) => {
	switch (error.message) {
		case "User with ID 'user-id-here' not found":
			return 'User not found. Please refresh the page and try again.';
		case 'Cannot delete SuperAdmin user':
			return 'SuperAdmin users cannot be deleted.';
		case 'Cannot delete your own account':
			return 'You cannot delete your own account.';
		case 'User ID is required':
			return 'Invalid user ID provided.';
		default:
			return error.message || 'An unexpected error occurred.';
	}
};
```

### 2. Show User-Friendly Messages

```typescript
const getErrorMessage = (error: DeleteUserErrorResponse): string => {
	const errorMessages: Record<string, string> = {
		"User with ID 'user-id-here' not found":
			'This user no longer exists.',
		'Cannot delete SuperAdmin user':
			'SuperAdmin accounts are protected and cannot be deleted.',
		'Cannot delete your own account':
			'You cannot delete your own account for security reasons.',
		'User ID is required': 'Invalid user selection.',
		'Failed to delete user':
			'Unable to delete user. Please try again later.',
	};

	return errorMessages[error.message] || 'An unexpected error occurred.';
};
```

---

## Testing Examples

### Unit Test Example

```typescript
import { deleteUser } from './userService';

describe('deleteUser', () => {
	it('should delete user successfully', async () => {
		const mockResponse = {
			success: true,
			message: 'User deleted successfully',
			deletedUserId: 'test-user-id',
			deletedUserName: 'test@example.com',
			timestamp: '2025-09-24T17:58:55.9581614Z',
		};

		// Mock axios
		jest.spyOn(axios, 'delete').mockResolvedValue({
			data: mockResponse,
		});

		const result = await deleteUser({
			userId: 'test-user-id',
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
					timestamp: '2025-09-24T17:58:55.9581614Z',
				},
			},
		};

		jest.spyOn(axios, 'delete').mockRejectedValue(mockError);

		const result = await deleteUser({
			userId: 'non-existent',
			token: 'test-token',
		});

		expect(result.success).toBe(false);
		expect(result.message).toContain('not found');
	});
});
```

---

## Security Considerations

1. **Authentication Required**: All requests must include a valid JWT token
2. **Role-Based Access**: Only SuperAdmin and Admin users can delete users
3. **Self-Protection**: Users cannot delete their own accounts
4. **SuperAdmin Protection**: SuperAdmin accounts cannot be deleted
5. **Input Validation**: User ID is validated before processing

---

## Rate Limiting

Currently, no rate limiting is implemented. Consider implementing rate limiting for production use.

---

## Changelog

### Version 1.0.0 (2025-09-24)

- Initial implementation
- Changed from username-based to user ID-based deletion
- Added comprehensive error handling
- Added business rule validations
- Added protection against self-deletion and SuperAdmin deletion

---

## Support

For technical support or questions about this API, please contact the backend development team.

---

_This documentation is generated for React frontend integration and should be kept up to date with any API changes._
