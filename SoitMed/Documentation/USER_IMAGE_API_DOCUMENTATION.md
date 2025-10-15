# User Image Management API Documentation

## Overview

The User Image Management system allows users to upload, manage, and serve profile images and other user-related images. This system supports multiple images per user with one designated as the profile image.

## Features

- Upload images with validation (file type, size)
- Set any uploaded image as profile image
- Update image metadata (alt text, profile status)
- Delete images (soft delete)
- Serve images via API endpoints
- Automatic file organization by user ID

## API Endpoints

### 1. Get My Images

**GET** `/api/UserImage/my-images`

Returns all active images for the current user.

**Response:**

```json
{
	"data": [
		{
			"id": 1,
			"userId": "user123",
			"fileName": "profile.jpg",
			"filePath": "uploads/user-images/user123/guid.jpg",
			"contentType": "image/jpeg",
			"fileSize": 1024000,
			"altText": "Profile picture",
			"uploadedAt": "2025-09-17T18:30:00Z",
			"isActive": true,
			"isProfileImage": true
		}
	],
	"message": "Found 1 image(s)",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 2. Get My Profile Image

**GET** `/api/UserImage/my-profile-image`

Returns the current user's profile image.

**Response:**

```json
{
	"data": {
		"id": 1,
		"userId": "user123",
		"fileName": "profile.jpg",
		"filePath": "uploads/user-images/user123/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile picture",
		"uploadedAt": "2025-09-17T18:30:00Z",
		"isActive": true,
		"isProfileImage": true
	},
	"message": "Profile image retrieved successfully",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 3. Upload Image

**POST** `/api/UserImage/upload`

Upload a new image for the current user.

**Request:** `multipart/form-data`

- `file` (required): Image file
- `altText` (optional): Alternative text for the image
- `isProfileImage` (optional): Set as profile image (default: false)

**Supported file types:** JPEG, JPG, PNG, GIF, WebP
**Maximum file size:** 10MB

**Response:**

```json
{
	"data": {
		"id": 1,
		"userId": "user123",
		"fileName": "profile.jpg",
		"filePath": "uploads/user-images/user123/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile picture",
		"uploadedAt": "2025-09-17T18:30:00Z",
		"isProfileImage": true,
		"message": "Image uploaded successfully"
	},
	"message": "Image uploaded successfully",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 4. Set as Profile Image

**PUT** `/api/UserImage/{imageId}/set-profile`

Set an existing image as the user's profile image.

**Response:**

```json
{
	"message": "Profile image updated successfully",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 5. Update Image

**PUT** `/api/UserImage/{imageId}`

Update image metadata.

**Request Body:**

```json
{
	"altText": "Updated description",
	"isProfileImage": true
}
```

**Response:**

```json
{
	"message": "Image updated successfully",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 6. Delete Image

**DELETE** `/api/UserImage/{imageId}`

Soft delete an image (sets IsActive to false and removes physical file).

**Response:**

```json
{
	"message": "Image deleted successfully",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

### 7. Serve Image

**GET** `/api/UserImage/serve/{imageId}`

Serve the image file directly. Returns the image file with appropriate content type.

## Database Schema

### UserImage Entity

```sql
CREATE TABLE [UserImages] (
    [Id] int IDENTITY(1,1) PRIMARY KEY,
    [UserId] nvarchar(450) NOT NULL,
    [FileName] nvarchar(500) NOT NULL,
    [FilePath] nvarchar(1000) NOT NULL,
    [ContentType] nvarchar(100) NULL,
    [FileSize] bigint NOT NULL,
    [AltText] nvarchar(500) NULL,
    [UploadedAt] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [IsProfileImage] bit NOT NULL,
    FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

-- Unique constraint: Only one profile image per user
CREATE UNIQUE INDEX [IX_UserImages_UserId_IsProfileImage]
ON [UserImages] ([UserId], [IsProfileImage])
WHERE [IsProfileImage] = 1;
```

## File Storage

Images are stored in the following structure:

```
wwwroot/
└── uploads/
    └── user-images/
        └── {userId}/
            └── {guid}.{extension}
```

## Integration with User Data

User images are automatically included in user data responses:

### UserDataDTO

```json
{
	"id": "user123",
	"userName": "john.doe",
	"email": "john@example.com",
	"firstName": "John",
	"lastName": "Doe",
	"fullName": "John Doe",
	"isActive": true,
	"createdAt": "2025-09-17T18:00:00Z",
	"lastLoginAt": "2025-09-17T18:30:00Z",
	"roles": ["User"],
	"departmentId": 1,
	"departmentName": "Engineering",
	"departmentDescription": "Technical and engineering staff",
	"profileImage": {
		"id": 1,
		"userId": "user123",
		"fileName": "profile.jpg",
		"filePath": "uploads/user-images/user123/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile picture",
		"uploadedAt": "2025-09-17T18:30:00Z",
		"isActive": true,
		"isProfileImage": true
	},
	"userImages": [
		// Array of all user images
	]
}
```

## Error Handling

### Common Error Responses

**400 Bad Request - No file provided:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"File": ["No file provided"]
	},
	"timestamp": "2025-09-17T18:30:00Z"
}
```

**400 Bad Request - Invalid file type:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"File": [
			"Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed"
		]
	},
	"timestamp": "2025-09-17T18:30:00Z"
}
```

**400 Bad Request - File too large:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"File": ["File size cannot exceed 10MB"]
	},
	"timestamp": "2025-09-17T18:30:00Z"
}
```

**404 Not Found - Image not found:**

```json
{
	"error": "Image not found",
	"timestamp": "2025-09-17T18:30:00Z"
}
```

## Security Considerations

1. **File Type Validation**: Only image files are allowed
2. **File Size Limits**: Maximum 10MB per file
3. **User Isolation**: Users can only access their own images
4. **Authentication Required**: All endpoints require authentication
5. **File Path Security**: Files are stored with GUID names to prevent path traversal
6. **Soft Delete**: Images are soft deleted to maintain referential integrity

## Usage Examples

### Upload Profile Image with cURL

```bash
curl -X POST "http://localhost:5117/api/UserImage/upload" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@profile.jpg" \
  -F "altText=My profile picture" \
  -F "isProfileImage=true"
```

### Set Existing Image as Profile

```bash
curl -X PUT "http://localhost:5117/api/UserImage/1/set-profile" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Get All My Images

```bash
curl -X GET "http://localhost:5117/api/UserImage/my-images" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Notes

- Images are automatically organized by user ID in the file system
- Only one profile image is allowed per user
- Setting a new profile image automatically unsets the previous one
- Deleted images are soft deleted (IsActive = false) and physical files are removed
- All timestamps are in UTC format
- File paths use forward slashes for cross-platform compatibility

