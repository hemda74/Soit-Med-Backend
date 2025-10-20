# Sales Support API Examples for React Frontend

This document provides comprehensive examples for React developers to integrate with the Sales Support user creation API.

## API Endpoint

```
POST /api/RoleSpecificUser/sales-support
```

## Authentication

All requests require authentication. Include the JWT token in the Authorization header:

```javascript
const token = localStorage.getItem('authToken'); // or however you store your token
const headers = {
	Authorization: `Bearer ${token}`,
	// Don't set Content-Type for FormData - browser will set it automatically with boundary
};
```

## React Component Examples

### 1. Basic Form Component

```jsx
import React, { useState } from 'react';
import axios from 'axios';

const CreateSalesSupportForm = () => {
	const [formData, setFormData] = useState({
		email: '',
		password: '',
		firstName: '',
		lastName: '',
		phoneNumber: '',
		personalMail: '',
		supportSpecialization: '',
		supportLevel: '',
		notes: '',
		altText: '',
	});
	const [profileImage, setProfileImage] = useState(null);
	const [loading, setLoading] = useState(false);
	const [message, setMessage] = useState('');

	const handleInputChange = (e) => {
		const { name, value } = e.target;
		setFormData((prev) => ({
			...prev,
			[name]: value,
		}));
	};

	const handleImageChange = (e) => {
		setProfileImage(e.target.files[0]);
	};

	const handleSubmit = async (e) => {
		e.preventDefault();
		setLoading(true);
		setMessage('');

		try {
			const formDataToSend = new FormData();

			// Add all form fields
			Object.keys(formData).forEach((key) => {
				if (formData[key]) {
					formDataToSend.append(
						key,
						formData[key]
					);
				}
			});

			// Add profile image if selected
			if (profileImage) {
				formDataToSend.append(
					'profileImage',
					profileImage
				);
			}

			const response = await axios.post(
				'/api/RoleSpecificUser/sales-support',
				formDataToSend,
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'authToken'
						)}`,
					},
				}
			);

			setMessage(`Success: ${response.data.message}`);
			console.log(
				'Created Sales Support User:',
				response.data
			);

			// Reset form
			setFormData({
				email: '',
				password: '',
				firstName: '',
				lastName: '',
				phoneNumber: '',
				personalMail: '',
				supportSpecialization: '',
				supportLevel: '',
				notes: '',
				altText: '',
			});
			setProfileImage(null);
		} catch (error) {
			console.error(
				'Error creating sales support user:',
				error
			);
			if (error.response?.data?.errors) {
				setMessage(
					`Error: ${JSON.stringify(
						error.response.data.errors
					)}`
				);
			} else {
				setMessage('Error creating sales support user');
			}
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
			<h2 className="text-2xl font-bold mb-6">
				Create Sales Support User
			</h2>

			<form
				onSubmit={handleSubmit}
				className="space-y-4"
			>
				{/* Basic Information */}
				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Email *
						</label>
						<input
							type="email"
							name="email"
							value={formData.email}
							onChange={
								handleInputChange
							}
							required
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Password *
						</label>
						<input
							type="password"
							name="password"
							value={
								formData.password
							}
							onChange={
								handleInputChange
							}
							required
							minLength={6}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							First Name
						</label>
						<input
							type="text"
							name="firstName"
							value={
								formData.firstName
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Last Name
						</label>
						<input
							type="text"
							name="lastName"
							value={
								formData.lastName
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Phone Number
						</label>
						<input
							type="tel"
							name="phoneNumber"
							value={
								formData.phoneNumber
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Personal Email
						</label>
						<input
							type="email"
							name="personalMail"
							value={
								formData.personalMail
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>
				</div>

				{/* Sales Support Specific Fields */}
				<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Support Specialization
						</label>
						<input
							type="text"
							name="supportSpecialization"
							value={
								formData.supportSpecialization
							}
							onChange={
								handleInputChange
							}
							placeholder="e.g., Customer Support, Technical Support"
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						/>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1">
							Support Level
						</label>
						<select
							name="supportLevel"
							value={
								formData.supportLevel
							}
							onChange={
								handleInputChange
							}
							className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
						>
							<option value="">
								Select Level
							</option>
							<option value="Junior">
								Junior
							</option>
							<option value="Senior">
								Senior
							</option>
							<option value="Lead">
								Lead
							</option>
							<option value="Specialist">
								Specialist
							</option>
						</select>
					</div>
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						Notes
					</label>
					<textarea
						name="notes"
						value={formData.notes}
						onChange={handleInputChange}
						rows={3}
						placeholder="Additional notes about the sales support role"
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
				</div>

				{/* Profile Image Upload */}
				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						Profile Image
					</label>
					<input
						type="file"
						accept="image/*"
						onChange={handleImageChange}
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
					{profileImage && (
						<p className="text-sm text-gray-600 mt-1">
							Selected:{' '}
							{profileImage.name}
						</p>
					)}
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1">
						Image Alt Text
					</label>
					<input
						type="text"
						name="altText"
						value={formData.altText}
						onChange={handleInputChange}
						placeholder="Description of the profile image"
						className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
					/>
				</div>

				{/* Submit Button */}
				<button
					type="submit"
					disabled={loading}
					className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
				>
					{loading
						? 'Creating...'
						: 'Create Sales Support User'}
				</button>

				{/* Message Display */}
				{message && (
					<div
						className={`p-4 rounded-md ${
							message.startsWith(
								'Success'
							)
								? 'bg-green-100 text-green-800'
								: 'bg-red-100 text-red-800'
						}`}
					>
						{message}
					</div>
				)}
			</form>
		</div>
	);
};

export default CreateSalesSupportForm;
```

### 2. Using React Hook Form (Advanced)

```jsx
import React from 'react';
import { useForm } from 'react-hook-form';
import axios from 'axios';

const CreateSalesSupportWithHookForm = () => {
	const {
		register,
		handleSubmit,
		formState: { errors },
		reset,
		watch,
	} = useForm();
	const [loading, setLoading] = useState(false);
	const [message, setMessage] = useState('');

	const onSubmit = async (data) => {
		setLoading(true);
		setMessage('');

		try {
			const formData = new FormData();

			// Add form data
			Object.keys(data).forEach((key) => {
				if (data[key] && key !== 'profileImage') {
					formData.append(key, data[key]);
				}
			});

			// Add profile image
			const profileImage = watch('profileImage');
			if (profileImage && profileImage[0]) {
				formData.append(
					'profileImage',
					profileImage[0]
				);
			}

			const response = await axios.post(
				'/api/RoleSpecificUser/sales-support',
				formData,
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'authToken'
						)}`,
					},
				}
			);

			setMessage(`Success: ${response.data.message}`);
			reset();
		} catch (error) {
			console.error('Error:', error);
			setMessage('Error creating sales support user');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			onSubmit={handleSubmit(onSubmit)}
			className="space-y-4"
		>
			{/* Email */}
			<div>
				<label className="block text-sm font-medium text-gray-700 mb-1">
					Email *
				</label>
				<input
					{...register('email', {
						required: 'Email is required',
						pattern: {
							value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
							message: 'Invalid email address',
						},
					})}
					className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
				/>
				{errors.email && (
					<p className="text-red-600 text-sm mt-1">
						{errors.email.message}
					</p>
				)}
			</div>

			{/* Password */}
			<div>
				<label className="block text-sm font-medium text-gray-700 mb-1">
					Password *
				</label>
				<input
					type="password"
					{...register('password', {
						required: 'Password is required',
						minLength: {
							value: 6,
							message: 'Password must be at least 6 characters',
						},
					})}
					className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
				/>
				{errors.password && (
					<p className="text-red-600 text-sm mt-1">
						{errors.password.message}
					</p>
				)}
			</div>

			{/* Other fields... */}

			<button
				type="submit"
				disabled={loading}
				className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
			>
				{loading
					? 'Creating...'
					: 'Create Sales Support User'}
			</button>
		</form>
	);
};
```

### 3. API Service Class

```javascript
// services/salesSupportApi.js
import axios from 'axios';

class SalesSupportApiService {
	constructor() {
		this.baseURL = process.env.REACT_APP_API_BASE_URL || '';
		this.api = axios.create({
			baseURL: this.baseURL,
		});

		// Add request interceptor to include auth token
		this.api.interceptors.request.use((config) => {
			const token = localStorage.getItem('authToken');
			if (token) {
				config.headers.Authorization = `Bearer ${token}`;
			}
			return config;
		});
	}

	async createSalesSupport(formData) {
		try {
			const response = await this.api.post(
				'/api/RoleSpecificUser/sales-support',
				formData
			);
			return {
				success: true,
				data: response.data,
				message: response.data.message,
			};
		} catch (error) {
			console.error('Error creating sales support:', error);
			return {
				success: false,
				error: error.response?.data || error.message,
				message: 'Failed to create sales support user',
			};
		}
	}

	// Helper method to create FormData from object
	createFormData(data) {
		const formData = new FormData();

		Object.keys(data).forEach((key) => {
			if (
				data[key] !== null &&
				data[key] !== undefined &&
				data[key] !== ''
			) {
				if (
					key === 'profileImage' &&
					data[key] instanceof File
				) {
					formData.append(
						'profileImage',
						data[key]
					);
				} else if (key !== 'profileImage') {
					formData.append(key, data[key]);
				}
			}
		});

		return formData;
	}
}

export default new SalesSupportApiService();
```

### 4. Custom Hook

```javascript
// hooks/useSalesSupport.js
import { useState } from 'react';
import salesSupportApi from '../services/salesSupportApi';

export const useSalesSupport = () => {
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState(null);

	const createSalesSupport = async (userData) => {
		setLoading(true);
		setError(null);

		try {
			const formData =
				salesSupportApi.createFormData(userData);
			const result = await salesSupportApi.createSalesSupport(
				formData
			);

			if (!result.success) {
				setError(result.error);
				return { success: false, error: result.error };
			}

			return { success: true, data: result.data };
		} catch (err) {
			setError(err.message);
			return { success: false, error: err.message };
		} finally {
			setLoading(false);
		}
	};

	return {
		createSalesSupport,
		loading,
		error,
	};
};
```

### 5. Usage with Custom Hook

```jsx
import React, { useState } from 'react';
import { useSalesSupport } from '../hooks/useSalesSupport';

const SalesSupportManager = () => {
	const { createSalesSupport, loading, error } = useSalesSupport();
	const [formData, setFormData] = useState({
		email: '',
		password: '',
		firstName: '',
		lastName: '',
		phoneNumber: '',
		personalMail: '',
		supportSpecialization: '',
		supportLevel: '',
		notes: '',
		profileImage: null,
	});

	const handleSubmit = async (e) => {
		e.preventDefault();
		const result = await createSalesSupport(formData);

		if (result.success) {
			console.log('Sales support created:', result.data);
			// Handle success (show notification, redirect, etc.)
		} else {
			console.error('Error:', result.error);
		}
	};

	return (
		<div>
			{/* Your form JSX here */}
			{loading && <p>Creating sales support user...</p>}
			{error && (
				<p className="text-red-600">Error: {error}</p>
			)}
		</div>
	);
};
```

## Response Format

### Success Response (200)

```json
{
	"userId": "SalesSupport_John_Doe_Sales_001",
	"email": "john.doe@example.com",
	"role": "SalesSupport",
	"departmentName": "Sales",
	"createdAt": "2024-01-15T10:30:00Z",
	"profileImage": {
		"id": 123,
		"fileName": "profile.jpg",
		"filePath": "/uploads/sales-support/...",
		"contentType": "image/jpeg",
		"fileSize": 156789,
		"altText": "John Doe profile picture",
		"isProfileImage": true,
		"uploadedAt": "2024-01-15T10:30:00Z"
	},
	"message": "Sales Support 'john.doe@example.com' created successfully with profile image",
	"supportSpecialization": "Customer Support",
	"supportLevel": "Senior",
	"notes": "Experienced in technical support"
}
```

### Error Response (400)

```json
{
	"errors": {
		"Email": "Email is required",
		"Password": "Password must be at least 6 characters long"
	},
	"message": "Validation failed"
}
```

## Required vs Optional Fields

### Required Fields:

- `email` (string, valid email format)
- `password` (string, minimum 6 characters)

### Optional Fields:

- `firstName` (string, max 100 characters)
- `lastName` (string, max 100 characters)
- `phoneNumber` (string, valid phone format, max 20 characters)
- `personalMail` (string, valid email format, max 200 characters)
- `departmentId` (number, will auto-assign to Sales department if not provided)
- `supportSpecialization` (string, max 200 characters)
- `supportLevel` (string, max 100 characters)
- `notes` (string, max 500 characters)
- `altText` (string, max 500 characters)
- `profileImage` (file, image format)

## Authorization

Only users with the following roles can create sales support users:

- `SuperAdmin`
- `Admin`
- `SalesManager`

## Notes

1. **FormData**: Always use FormData when sending files (profile images)
2. **Content-Type**: Don't manually set Content-Type header when using FormData - the browser will set it automatically with the correct boundary
3. **File Validation**: The API accepts common image formats (jpg, jpeg, png, gif, etc.)
4. **User ID Generation**: The system automatically generates custom user IDs following the pattern: `SalesSupport_FirstName_LastName_Department_Number`
5. **Department Assignment**: If no department ID is provided, users are automatically assigned to the "Sales" department

## Error Handling

Always implement proper error handling for:

- Network errors
- Validation errors (400)
- Authorization errors (401/403)
- Server errors (500)
- File upload errors

This ensures a smooth user experience and proper error reporting.
