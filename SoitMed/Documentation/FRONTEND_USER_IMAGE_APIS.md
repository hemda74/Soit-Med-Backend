# 🖼️ User Image Management APIs - Frontend Integration Guide

## **Overview**

This document provides complete API specifications for user image management in the SoitMed Backend system. All endpoints require authentication and support full CRUD operations for user profile images.

---

## **🔐 Authentication**

All endpoints require a valid JWT token obtained from `/api/Account/login`.

**Header Format:**

```
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## **📋 API Endpoints**

### **1. Upload User Profile Image**

```http
POST /api/User/image
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Request Body (Form Data):**

| Field          | Type   | Required | Description                                |
| -------------- | ------ | -------- | ------------------------------------------ |
| `profileImage` | File   | Yes      | Image file (JPEG, PNG, GIF, max 5MB)       |
| `AltText`      | String | No       | Alternative text for image (max 500 chars) |

#### **Request Examples:**

**JavaScript/Fetch:**

```javascript
const uploadUserImage = async (imageFile, altText = '') => {
	const formData = new FormData();
	formData.append('profileImage', imageFile);
	if (altText) formData.append('AltText', altText);

	try {
		const response = await fetch(
			'http://localhost:5117/api/User/image',
			{
				method: 'POST',
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
				body: formData,
			}
		);

		if (!response.ok) throw new Error('Upload failed');
		return await response.json();
	} catch (error) {
		console.error('Error uploading image:', error);
		throw error;
	}
};

// Usage
const fileInput = document.getElementById('imageInput');
const imageFile = fileInput.files[0];
uploadUserImage(imageFile, 'My profile picture');
```

**Axios:**

```javascript
import axios from 'axios';

const uploadUserImage = async (imageFile, altText = '') => {
	const formData = new FormData();
	formData.append('profileImage', imageFile);
	if (altText) formData.append('AltText', altText);

	try {
		const response = await axios.post(
			'http://localhost:5117/api/User/image',
			formData,
			{
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
					'Content-Type': 'multipart/form-data',
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error uploading image:', error);
		throw error;
	}
};
```

**cURL:**

```bash
curl -X POST "http://localhost:5117/api/User/image" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -F "profileImage=@profile.jpg" \
     -F "AltText=My profile picture"
```

#### **Success Response (200):**

```json
{
	"userId": "USER-ID-123",
	"message": "Profile image uploaded successfully",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "/uploads/doctor/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "My profile picture",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T10:30:00Z",
		"isActive": true
	},
	"updatedAt": "2025-01-21T10:30:00Z"
}
```

---

### **2. Update User Profile Image**

```http
PUT /api/User/image
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Request Body (Form Data):**

| Field          | Type   | Required | Description                                |
| -------------- | ------ | -------- | ------------------------------------------ |
| `profileImage` | File   | Yes      | New image file (JPEG, PNG, GIF, max 5MB)   |
| `AltText`      | String | No       | Alternative text for image (max 500 chars) |

#### **Request Examples:**

**JavaScript/Fetch:**

```javascript
const updateUserImage = async (imageFile, altText = '') => {
	const formData = new FormData();
	formData.append('profileImage', imageFile);
	if (altText) formData.append('AltText', altText);

	try {
		const response = await fetch(
			'http://localhost:5117/api/User/image',
			{
				method: 'PUT',
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
				body: formData,
			}
		);

		if (!response.ok) throw new Error('Update failed');
		return await response.json();
	} catch (error) {
		console.error('Error updating image:', error);
		throw error;
	}
};
```

**Axios:**

```javascript
const updateUserImage = async (imageFile, altText = '') => {
	const formData = new FormData();
	formData.append('profileImage', imageFile);
	if (altText) formData.append('AltText', altText);

	try {
		const response = await axios.put(
			'http://localhost:5117/api/User/image',
			formData,
			{
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
					'Content-Type': 'multipart/form-data',
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error updating image:', error);
		throw error;
	}
};
```

**cURL:**

```bash
curl -X PUT "http://localhost:5117/api/User/image" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -F "profileImage=@new-profile.jpg" \
     -F "AltText=Updated profile picture"
```

#### **Success Response (200):**

```json
{
	"userId": "USER-ID-123",
	"message": "Profile image updated successfully",
	"profileImage": {
		"id": 2,
		"fileName": "new-profile.jpg",
		"filePath": "/uploads/doctor/new-profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 2048000,
		"altText": "Updated profile picture",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T11:00:00Z",
		"isActive": true
	},
	"updatedAt": "2025-01-21T11:00:00Z"
}
```

---

### **3. Delete User Profile Image**

```http
DELETE /api/User/image
Authorization: Bearer {token}
```

#### **Request Body:**

No body required

#### **Request Examples:**

**JavaScript/Fetch:**

```javascript
const deleteUserImage = async () => {
	try {
		const response = await fetch(
			'http://localhost:5117/api/User/image',
			{
				method: 'DELETE',
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);

		if (!response.ok) throw new Error('Delete failed');
		return await response.json();
	} catch (error) {
		console.error('Error deleting image:', error);
		throw error;
	}
};
```

**Axios:**

```javascript
const deleteUserImage = async () => {
	try {
		const response = await axios.delete(
			'http://localhost:5117/api/User/image',
			{
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error deleting image:', error);
		throw error;
	}
};
```

**cURL:**

```bash
curl -X DELETE "http://localhost:5117/api/User/image" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### **Success Response (200):**

```json
{
	"userId": "USER-ID-123",
	"message": "Profile image deleted successfully",
	"deletedAt": "2025-01-21T11:30:00Z"
}
```

---

### **4. Get User Profile Image**

```http
GET /api/User/image
Authorization: Bearer {token}
```

#### **Request Body:**

No body required

#### **Request Examples:**

**JavaScript/Fetch:**

```javascript
const getUserImage = async () => {
	try {
		const response = await fetch(
			'http://localhost:5117/api/User/image',
			{
				method: 'GET',
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);

		if (!response.ok) throw new Error('Failed to get image');
		return await response.json();
	} catch (error) {
		console.error('Error getting image:', error);
		throw error;
	}
};
```

**Axios:**

```javascript
const getUserImage = async () => {
	try {
		const response = await axios.get(
			'http://localhost:5117/api/User/image',
			{
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error getting image:', error);
		throw error;
	}
};
```

**cURL:**

```bash
curl -X GET "http://localhost:5117/api/User/image" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### **Success Response (200):**

```json
{
	"id": 1,
	"fileName": "profile.jpg",
	"filePath": "/uploads/doctor/profile.jpg",
	"contentType": "image/jpeg",
	"fileSize": 1024000,
	"altText": "My profile picture",
	"isProfileImage": true,
	"uploadedAt": "2025-01-21T10:30:00Z",
	"isActive": true
}
```

---

### **5. Get Current User Data (includes image)**

```http
GET /api/User/me
Authorization: Bearer {token}
```

#### **Request Body:**

No body required

#### **Request Examples:**

**JavaScript/Fetch:**

```javascript
const getCurrentUser = async () => {
	try {
		const response = await fetch(
			'http://localhost:5117/api/User/me',
			{
				method: 'GET',
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);

		if (!response.ok) throw new Error('Failed to get user data');
		return await response.json();
	} catch (error) {
		console.error('Error getting user data:', error);
		throw error;
	}
};
```

**Axios:**

```javascript
const getCurrentUser = async () => {
	try {
		const response = await axios.get(
			'http://localhost:5117/api/User/me',
			{
				headers: {
					Authorization: `Bearer ${localStorage.getItem(
						'token'
					)}`,
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error getting user data:', error);
		throw error;
	}
};
```

**cURL:**

```bash
curl -X GET "http://localhost:5117/api/User/me" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### **Success Response (200):**

```json
{
	"id": "USER-ID-123",
	"userName": "user@example.com",
	"email": "user@example.com",
	"firstName": "John",
	"lastName": "Doe",
	"fullName": "John Doe",
	"isActive": true,
	"createdAt": "2025-01-21T10:00:00Z",
	"lastLoginAt": "2025-01-21T10:30:00Z",
	"roles": ["Doctor"],
	"departmentId": 1,
	"departmentName": "Medical",
	"departmentDescription": "Medical Department",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "/uploads/doctor/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "My profile picture",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T10:30:00Z",
		"isActive": true
	},
	"emailConfirmed": true,
	"phoneNumberConfirmed": false,
	"phoneNumber": null
}
```

---

## **❌ Error Responses**

### **400 Bad Request:**

```json
{
	"type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
	"title": "One or more validation errors occurred.",
	"status": 400,
	"errors": {
		"ProfileImage": ["Profile image is required"],
		"AltText": ["Alt text cannot exceed 500 characters"]
	}
}
```

### **401 Unauthorized:**

```json
{
	"type": "https://tools.ietf.org/html/rfc7235#section-3.1",
	"title": "Unauthorized",
	"status": 401
}
```

### **404 Not Found:**

```json
{
	"type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
	"title": "Not Found",
	"status": 404,
	"detail": "No profile image found"
}
```

---

## **📝 Validation Rules**

### **File Upload Validation:**

- **File Size**: Maximum 5MB
- **File Types**: JPEG, JPG, PNG, GIF only
- **Required Field**: `profileImage` (for POST/PUT)
- **Alt Text**: Maximum 500 characters (optional)

### **Common Error Codes:**

- `IMAGE_REQUIRED` - No image file provided
- `IMAGE_TOO_LARGE` - File size exceeds 5MB
- `INVALID_IMAGE_TYPE` - Unsupported file format
- `USER_ID_NOT_FOUND` - Invalid or missing token
- `USER_NOT_FOUND` - User doesn't exist
- `NO_IMAGE_FOUND` - No image to delete/retrieve
- `UPLOAD_FAILED` - Server error during upload

---

## **🔧 Frontend Integration Examples**

### **Complete Image Management Component (React):**

```jsx
import React, { useState, useEffect } from 'react';

const UserImageManager = () => {
	const [userImage, setUserImage] = useState(null);
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState('');

	// Get current user image
	const fetchUserImage = async () => {
		try {
			const response = await fetch(
				'http://localhost:5117/api/User/image',
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'token'
						)}`,
					},
				}
			);

			if (response.ok) {
				const data = await response.json();
				setUserImage(data);
			} else if (response.status === 404) {
				setUserImage(null); // No image
			}
		} catch (error) {
			console.error('Error fetching image:', error);
		}
	};

	// Upload new image
	const handleImageUpload = async (event) => {
		const file = event.target.files[0];
		if (!file) return;

		setLoading(true);
		setError('');

		const formData = new FormData();
		formData.append('profileImage', file);
		formData.append('AltText', 'User profile picture');

		try {
			const response = await fetch(
				'http://localhost:5117/api/User/image',
				{
					method: 'POST',
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'token'
						)}`,
					},
					body: formData,
				}
			);

			if (response.ok) {
				const data = await response.json();
				setUserImage(data.profileImage);
				alert('Image uploaded successfully!');
			} else {
				const errorData = await response.json();
				setError(errorData.detail || 'Upload failed');
			}
		} catch (error) {
			setError('Network error occurred');
		} finally {
			setLoading(false);
		}
	};

	// Update existing image
	const handleImageUpdate = async (event) => {
		const file = event.target.files[0];
		if (!file) return;

		setLoading(true);
		setError('');

		const formData = new FormData();
		formData.append('profileImage', file);
		formData.append('AltText', 'Updated profile picture');

		try {
			const response = await fetch(
				'http://localhost:5117/api/User/image',
				{
					method: 'PUT',
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'token'
						)}`,
					},
					body: formData,
				}
			);

			if (response.ok) {
				const data = await response.json();
				setUserImage(data.profileImage);
				alert('Image updated successfully!');
			} else {
				const errorData = await response.json();
				setError(errorData.detail || 'Update failed');
			}
		} catch (error) {
			setError('Network error occurred');
		} finally {
			setLoading(false);
		}
	};

	// Delete image
	const handleImageDelete = async () => {
		if (
			!confirm(
				'Are you sure you want to delete your profile image?'
			)
		)
			return;

		setLoading(true);
		setError('');

		try {
			const response = await fetch(
				'http://localhost:5117/api/User/image',
				{
					method: 'DELETE',
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'token'
						)}`,
					},
				}
			);

			if (response.ok) {
				setUserImage(null);
				alert('Image deleted successfully!');
			} else {
				const errorData = await response.json();
				setError(errorData.detail || 'Delete failed');
			}
		} catch (error) {
			setError('Network error occurred');
		} finally {
			setLoading(false);
		}
	};

	useEffect(() => {
		fetchUserImage();
	}, []);

	return (
		<div className="user-image-manager">
			<h3>Profile Image Management</h3>

			{userImage ? (
				<div className="current-image">
					<img
						src={`http://localhost:5117${userImage.filePath}`}
						alt={
							userImage.altText ||
							'Profile image'
						}
						style={{
							width: '150px',
							height: '150px',
							objectFit: 'cover',
						}}
					/>
					<p>
						Current image:{' '}
						{userImage.fileName}
					</p>
					<p>
						Size:{' '}
						{(
							userImage.fileSize /
							1024
						).toFixed(1)}{' '}
						KB
					</p>
				</div>
			) : (
				<p>No profile image set</p>
			)}

			<div className="image-actions">
				<input
					type="file"
					accept="image/jpeg,image/jpg,image/png,image/gif"
					onChange={
						userImage
							? handleImageUpdate
							: handleImageUpload
					}
					disabled={loading}
				/>

				{userImage && (
					<button
						onClick={handleImageDelete}
						disabled={loading}
					>
						Delete Image
					</button>
				)}
			</div>

			{loading && <p>Processing...</p>}
			{error && (
				<p style={{ color: 'red' }}>Error: {error}</p>
			)}
		</div>
	);
};

export default UserImageManager;
```

### **Vue.js Component:**

```vue
<template>
	<div class="user-image-manager">
		<h3>Profile Image Management</h3>

		<div
			v-if="userImage"
			class="current-image"
		>
			<img
				:src="`http://localhost:5117${userImage.filePath}`"
				:alt="userImage.altText || 'Profile image'"
				style="width: 150px; height: 150px; object-fit: cover;"
			/>
			<p>Current image: {{ userImage.fileName }}</p>
			<p>
				Size:
				{{ (userImage.fileSize / 1024).toFixed(1) }} KB
			</p>
		</div>

		<p v-else>No profile image set</p>

		<div class="image-actions">
			<input
				type="file"
				accept="image/jpeg,image/jpg,image/png,image/gif"
				@change="handleFileSelect"
				:disabled="loading"
			/>

			<button
				v-if="userImage"
				@click="deleteImage"
				:disabled="loading"
			>
				Delete Image
			</button>
		</div>

		<p v-if="loading">Processing...</p>
		<p
			v-if="error"
			style="color: red;"
		>
			Error: {{ error }}
		</p>
	</div>
</template>

<script>
	export default {
		data() {
			return {
				userImage: null,
				loading: false,
				error: '',
			};
		},
		async mounted() {
			await this.fetchUserImage();
		},
		methods: {
			async fetchUserImage() {
				try {
					const response = await fetch(
						'http://localhost:5117/api/User/image',
						{
							headers: {
								Authorization: `Bearer ${localStorage.getItem(
									'token'
								)}`,
							},
						}
					);

					if (response.ok) {
						this.userImage =
							await response.json();
					} else if (response.status === 404) {
						this.userImage = null;
					}
				} catch (error) {
					console.error(
						'Error fetching image:',
						error
					);
				}
			},

			async handleFileSelect(event) {
				const file = event.target.files[0];
				if (!file) return;

				this.loading = true;
				this.error = '';

				const formData = new FormData();
				formData.append('profileImage', file);
				formData.append(
					'AltText',
					'User profile picture'
				);

				try {
					const method = this.userImage
						? 'PUT'
						: 'POST';
					const response = await fetch(
						'http://localhost:5117/api/User/image',
						{
							method,
							headers: {
								Authorization: `Bearer ${localStorage.getItem(
									'token'
								)}`,
							},
							body: formData,
						}
					);

					if (response.ok) {
						const data =
							await response.json();
						this.userImage =
							data.profileImage;
						alert(
							'Image uploaded successfully!'
						);
					} else {
						const errorData =
							await response.json();
						this.error =
							errorData.detail ||
							'Upload failed';
					}
				} catch (error) {
					this.error = 'Network error occurred';
				} finally {
					this.loading = false;
				}
			},

			async deleteImage() {
				if (
					!confirm(
						'Are you sure you want to delete your profile image?'
					)
				)
					return;

				this.loading = true;
				this.error = '';

				try {
					const response = await fetch(
						'http://localhost:5117/api/User/image',
						{
							method: 'DELETE',
							headers: {
								Authorization: `Bearer ${localStorage.getItem(
									'token'
								)}`,
							},
						}
					);

					if (response.ok) {
						this.userImage = null;
						alert(
							'Image deleted successfully!'
						);
					} else {
						const errorData =
							await response.json();
						this.error =
							errorData.detail ||
							'Delete failed';
					}
				} catch (error) {
					this.error = 'Network error occurred';
				} finally {
					this.loading = false;
				}
			},
		},
	};
</script>
```

---

## **🚀 Quick Start Checklist**

- [ ] Get JWT token from `/api/Account/login`
- [ ] Store token in localStorage or secure storage
- [ ] Include token in Authorization header for all requests
- [ ] Use multipart/form-data for file uploads (POST/PUT)
- [ ] Handle file validation (5MB max, JPEG/PNG/GIF only)
- [ ] Implement proper error handling for all scenarios
- [ ] Display image using the filePath from response
- [ ] Test all CRUD operations

---

## **📞 Support**

For any questions or issues:

1. Check the API response for specific error messages
2. Verify the JWT token is valid and not expired
3. Ensure file meets validation requirements
4. Check network connectivity to the backend

---

**🎉 Ready for frontend integration!**
