# User Creation with Images - Complete Examples

## Overview

This document provides comprehensive examples for creating users and managing their images in the Soit-Med system. Images are completely optional and can be added after user creation.

## User Creation Flow

### 1. Create User (Without Image)

First, create the user account using any of the existing user creation endpoints.

#### Example: Create Doctor User

```http
POST /api/RoleSpecificUser/doctor
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "userName": "dr.ahmed.hassan",
  "email": "ahmed.hassan@hospital.com",
  "password": "SecurePass123!",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "specialty": "Cardiology",
  "hospitalId": "HOSP001",
  "departmentId": 2
}
```

**Response:**

```json
{
	"data": {
		"id": "Ahmed_Hassan_CairoUniversityHospital_001",
		"email": "ahmed.hassan@hospital.com",
		"firstName": "Ahmed",
		"lastName": "Hassan",
		"role": "Doctor",
		"specialty": "Cardiology",
		"hospitalId": "HOSP001"
	},
	"message": "Doctor 'Ahmed Hassan' created successfully",
	"timestamp": "2025-09-17T19:00:00Z"
}
```

### 2. Upload Profile Image

After user creation, upload a profile image.

#### Example: Upload Profile Image

```http
POST /api/UserImage/upload
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: multipart/form-data

file: [profile-image.jpg]
altText: "Dr. Ahmed Hassan profile picture"
isProfileImage: true
```

**Response:**

```json
{
	"data": {
		"id": 1,
		"userId": "Ahmed_Hassan_CairoUniversityHospital_001",
		"fileName": "profile-image.jpg",
		"filePath": "uploads/user-images/Ahmed_Hassan_CairoUniversityHospital_001/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Dr. Ahmed Hassan profile picture",
		"uploadedAt": "2025-09-17T19:05:00Z",
		"isProfileImage": true,
		"message": "Image uploaded successfully"
	},
	"message": "Image uploaded successfully",
	"timestamp": "2025-09-17T19:05:00Z"
}
```

### 3. Get User Data (With Images)

Retrieve user data to see the complete profile including images.

#### Example: Get User Data

```http
GET /api/User/me
Authorization: Bearer YOUR_TOKEN_HERE
```

**Response:**

```json
{
	"id": "Ahmed_Hassan_CairoUniversityHospital_001",
	"userName": "dr.ahmed.hassan",
	"email": "ahmed.hassan@hospital.com",
	"firstName": "Ahmed",
	"lastName": "Hassan",
	"fullName": "Ahmed Hassan",
	"isActive": true,
	"createdAt": "2025-09-17T19:00:00Z",
	"lastLoginAt": "2025-09-17T19:05:00Z",
	"roles": ["Doctor"],
	"departmentId": 2,
	"departmentName": "Medical",
	"departmentDescription": "Medical staff including doctors and technicians",
	"emailConfirmed": true,
	"phoneNumberConfirmed": false,
	"phoneNumber": null,
	"profileImage": {
		"id": 1,
		"userId": "Ahmed_Hassan_CairoUniversityHospital_001",
		"fileName": "profile-image.jpg",
		"filePath": "uploads/user-images/Ahmed_Hassan_CairoUniversityHospital_001/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Dr. Ahmed Hassan profile picture",
		"uploadedAt": "2025-09-17T19:05:00Z",
		"isActive": true,
		"isProfileImage": true
	},
	"userImages": [
		{
			"id": 1,
			"userId": "Ahmed_Hassan_CairoUniversityHospital_001",
			"fileName": "profile-image.jpg",
			"filePath": "uploads/user-images/Ahmed_Hassan_CairoUniversityHospital_001/guid.jpg",
			"contentType": "image/jpeg",
			"fileSize": 1024000,
			"altText": "Dr. Ahmed Hassan profile picture",
			"uploadedAt": "2025-09-17T19:05:00Z",
			"isActive": true,
			"isProfileImage": true
		}
	]
}
```

## Complete User Creation Examples

### Example 1: Engineer with Profile Image

#### Step 1: Create Engineer

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/engineer" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "eng.mohamed.ali",
    "email": "mohamed.ali@company.com",
    "password": "SecurePass123!",
    "firstName": "Mohamed",
    "lastName": "Ali",
    "specialty": "Biomedical Engineering",
    "governorateIds": [1, 2, 3]
  }'
```

#### Step 2: Upload Profile Image

```bash
curl -X POST "http://localhost:5117/api/UserImage/upload" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@engineer-profile.jpg" \
  -F "altText=Mohamed Ali - Biomedical Engineer" \
  -F "isProfileImage=true"
```

### Example 2: Technician with Multiple Images

#### Step 1: Create Technician

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/technician" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "tech.sara.mahmoud",
    "email": "sara.mahmoud@hospital.com",
    "password": "SecurePass123!",
    "firstName": "Sara",
    "lastName": "Mahmoud",
    "department": "Radiology",
    "hospitalId": "HOSP001",
    "departmentId": 2
  }'
```

#### Step 2: Upload Profile Image

```bash
curl -X POST "http://localhost:5117/api/UserImage/upload" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@technician-profile.jpg" \
  -F "altText=Sara Mahmoud - Radiology Technician" \
  -F "isProfileImage=true"
```

#### Step 3: Upload Additional Image

```bash
curl -X POST "http://localhost:5117/api/UserImage/upload" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@certificate.jpg" \
  -F "altText=Professional Certificate" \
  -F "isProfileImage=false"
```

### Example 3: Admin User with Image Management

#### Step 1: Create Admin User with Profile Image

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/admin" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=admin.john.doe@company.com" \
  -F "Password=AdminPass123!" \
  -F "FirstName=John" \
  -F "LastName=Doe" \
  -F "AccessLevel=Full" \
  -F "ProfileImage=@admin-profile.jpg" \
  -F "ImageAltText=John Doe - System Administrator"
```

## Image Management Operations

### Upload Additional Images

```bash
curl -X POST "http://localhost:5117/api/UserImage/upload" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@additional-image.jpg" \
  -F "altText=Additional image description" \
  -F "isProfileImage=false"
```

### Set Different Image as Profile

```bash
curl -X PUT "http://localhost:5117/api/UserImage/2/set-profile" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Update Image Metadata

```bash
curl -X PUT "http://localhost:5117/api/UserImage/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "altText": "Updated description",
    "isProfileImage": false
  }'
```

### Get All User Images

```bash
curl -X GET "http://localhost:5117/api/UserImage/my-images" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Get Profile Image Only

```bash
curl -X GET "http://localhost:5117/api/UserImage/my-profile-image" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Serve Image File

```bash
curl -X GET "http://localhost:5117/api/UserImage/serve/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  --output image.jpg
```

### Delete Image

```bash
curl -X DELETE "http://localhost:5117/api/UserImage/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## User Data Response Examples

### User Without Images

```json
{
	"id": "user123",
	"userName": "john.doe",
	"email": "john@example.com",
	"firstName": "John",
	"lastName": "Doe",
	"fullName": "John Doe",
	"isActive": true,
	"createdAt": "2025-09-17T19:00:00Z",
	"lastLoginAt": "2025-09-17T19:05:00Z",
	"roles": ["User"],
	"departmentId": 1,
	"departmentName": "Administration",
	"departmentDescription": "Administrative and management roles",
	"profileImage": null,
	"userImages": []
}
```

### User With Images

```json
{
	"id": "user123",
	"userName": "john.doe",
	"email": "john@example.com",
	"firstName": "John",
	"lastName": "Doe",
	"fullName": "John Doe",
	"isActive": true,
	"createdAt": "2025-09-17T19:00:00Z",
	"lastLoginAt": "2025-09-17T19:05:00Z",
	"roles": ["User"],
	"departmentId": 1,
	"departmentName": "Administration",
	"departmentDescription": "Administrative and management roles",
	"profileImage": {
		"id": 1,
		"userId": "user123",
		"fileName": "profile.jpg",
		"filePath": "uploads/user-images/user123/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile picture",
		"uploadedAt": "2025-09-17T19:05:00Z",
		"isActive": true,
		"isProfileImage": true
	},
	"userImages": [
		{
			"id": 1,
			"userId": "user123",
			"fileName": "profile.jpg",
			"filePath": "uploads/user-images/user123/guid.jpg",
			"contentType": "image/jpeg",
			"fileSize": 1024000,
			"altText": "Profile picture",
			"uploadedAt": "2025-09-17T19:05:00Z",
			"isActive": true,
			"isProfileImage": true
		},
		{
			"id": 2,
			"userId": "user123",
			"fileName": "certificate.jpg",
			"filePath": "uploads/user-images/user123/guid2.jpg",
			"contentType": "image/jpeg",
			"fileSize": 512000,
			"altText": "Professional Certificate",
			"uploadedAt": "2025-09-17T19:10:00Z",
			"isActive": true,
			"isProfileImage": false
		}
	]
}
```

## Frontend Integration Examples

### HTML Form for User Creation with Image Upload

```html
<form id="userCreationForm">
	<div>
		<label>Username:</label>
		<input
			type="text"
			name="userName"
			required
		/>
	</div>

	<div>
		<label>Email:</label>
		<input
			type="email"
			name="email"
			required
		/>
	</div>

	<div>
		<label>Password:</label>
		<input
			type="password"
			name="password"
			required
		/>
	</div>

	<div>
		<label>First Name:</label>
		<input
			type="text"
			name="firstName"
		/>
	</div>

	<div>
		<label>Last Name:</label>
		<input
			type="text"
			name="lastName"
		/>
	</div>

	<div>
		<label>Role:</label>
		<select
			name="role"
			required
		>
			<option value="Doctor">Doctor</option>
			<option value="Technician">Technician</option>
			<option value="Engineer">Engineer</option>
			<option value="Admin">Admin</option>
		</select>
	</div>

	<div>
		<label>Profile Image (Optional):</label>
		<input
			type="file"
			name="profileImage"
			accept="image/*"
		/>
	</div>

	<button type="submit">Create User</button>
</form>
```

### JavaScript for User Creation with Image

```javascript
async function createUserWithImage(formData) {
	try {
		// Determine the correct endpoint based on role
		const role = formData.get('role');
		let endpoint = '';

		switch (role.toLowerCase()) {
			case 'doctor':
				endpoint = '/api/RoleSpecificUser/doctor';
				break;
			case 'engineer':
				endpoint = '/api/RoleSpecificUser/engineer';
				break;
			case 'technician':
				endpoint = '/api/RoleSpecificUser/technician';
				break;
			case 'admin':
				endpoint = '/api/RoleSpecificUser/admin';
				break;
			case 'finance-manager':
				endpoint =
					'/api/RoleSpecificUser/finance-manager';
				break;
			case 'legal-manager':
				endpoint =
					'/api/RoleSpecificUser/legal-manager';
				break;
			case 'salesman':
				endpoint = '/api/RoleSpecificUser/salesman';
				break;
			case 'finance-employee':
				endpoint =
					'/api/RoleSpecificUser/finance-employee';
				break;
			case 'legal-employee':
				endpoint =
					'/api/RoleSpecificUser/legal-employee';
				break;
			default:
				throw new Error('Invalid role specified');
		}

		// Create user with image in single request
		const userResponse = await fetch(endpoint, {
			method: 'POST',
			headers: {
				Authorization: `Bearer ${getAuthToken()}`,
			},
			body: formData, // Send formData directly for multipart/form-data
		});

		const userResult = await userResponse.json();

		if (userResponse.ok) {
			console.log('User created successfully:', userResult);
			return userResult;
		} else {
			throw new Error(
				userResult.message || 'User creation failed'
			);
		}
	} catch (error) {
		console.error('Error creating user:', error);
		throw error;
	}
}
```

## Important Notes

1. **Images are Optional**: Users can be created without any images
2. **Post-Creation Upload**: Images are typically uploaded after user creation
3. **Authentication Required**: Image upload requires user authentication
4. **File Validation**: Only image files (JPEG, PNG, GIF, WebP) are accepted
5. **Size Limit**: Maximum file size is 10MB
6. **One Profile Image**: Only one image can be set as profile image per user
7. **Automatic Organization**: Files are automatically organized by user ID
8. **Soft Delete**: Deleted images are soft deleted for data integrity

## Error Handling

### Common Scenarios

1. **User Creation Fails**: Handle validation errors and retry
2. **Image Upload Fails**: User is created but without image
3. **Invalid File Type**: Show user-friendly error message
4. **File Too Large**: Suggest file compression or different image
5. **Network Issues**: Implement retry logic for uploads

### Error Response Example

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"File": [
			"Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed"
		]
	},
	"timestamp": "2025-09-17T19:30:00Z"
}
```

This comprehensive guide covers all aspects of user creation with optional image support in your Soit-Med system!
