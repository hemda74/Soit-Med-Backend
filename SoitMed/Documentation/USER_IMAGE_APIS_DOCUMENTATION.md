# 🖼️ User Image Management APIs

## **Overview**

The User Image Management APIs allow users to update, delete, and retrieve their profile images. These APIs are designed to work with the existing role-based image upload system and provide a seamless experience for managing user profile pictures.

---

## **🔧 Available APIs**

### **1. Update User Profile Image**

```http
PUT /api/User/image
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Request Body (Form Data):**

- `profileImage` (IFormFile, required) - The new profile image file
- `AltText` (string, optional) - Alternative text for the image

#### **Response:**

```json
{
	"userId": "USER-ID-123",
	"message": "Profile image updated successfully",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "/uploads/doctor/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile image description",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T10:30:00Z",
		"isActive": true
	},
	"updatedAt": "2025-01-21T10:30:00Z"
}
```

### **2. Get User Profile Image**

```http
GET /api/User/image
Authorization: Bearer {token}
```

#### **Response:**

```json
{
	"id": 1,
	"fileName": "profile.jpg",
	"filePath": "/uploads/doctor/profile.jpg",
	"contentType": "image/jpeg",
	"fileSize": 1024000,
	"altText": "Profile image description",
	"isProfileImage": true,
	"uploadedAt": "2025-01-21T10:30:00Z",
	"isActive": true
}
```

### **3. Delete User Profile Image**

```http
DELETE /api/User/image
Authorization: Bearer {token}
```

#### **Response:**

```json
{
	"userId": "USER-ID-123",
	"message": "Profile image deleted successfully",
	"deletedAt": "2025-01-21T10:30:00Z"
}
```

### **4. Get Current User Data (includes profile image)**

```http
GET /api/User/me
Authorization: Bearer {token}
```

#### **Response:**

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
		"altText": "Profile image description",
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

## **🔐 Authentication & Authorization**

### **Required Authentication:**

- All endpoints require a valid Bearer token
- Token must be obtained from `/api/Account/login`

### **User Permissions:**

- Users can only manage their own profile images
- No admin privileges required for basic image management

---

## **📋 Validation Rules**

### **Image Upload Validation:**

- **File Size**: Maximum 5MB
- **File Types**: JPEG, JPG, PNG, GIF only
- **Required**: Image file must be provided
- **Alt Text**: Maximum 500 characters (optional)

### **Error Responses:**

- `400 Bad Request` - Invalid image file or validation errors
- `401 Unauthorized` - Missing or invalid token
- `404 Not Found` - User not found or no image to delete
- `500 Internal Server Error` - Server-side errors

---

## **🧪 Testing**

### **Test Script:**

```powershell
# Run the comprehensive test script
.\test-image-apis.ps1
```

### **Manual Testing with Swagger:**

1. **Start the application:**

      ```bash
      dotnet run --urls "http://localhost:5117"
      ```

2. **Access Swagger UI:**

      - Navigate to: `http://localhost:5117/swagger`
      - Find the `User` section
      - Look for the image management endpoints

3. **Test with Postman/curl:**

      ```bash
      # Update image
      curl -X PUT "http://localhost:5117/api/User/image" \
           -H "Authorization: Bearer YOUR_TOKEN" \
           -F "profileImage=@profile.jpg" \
           -F "AltText=My new profile picture"

      # Get image
      curl -X GET "http://localhost:5117/api/User/image" \
           -H "Authorization: Bearer YOUR_TOKEN"

      # Delete image
      curl -X DELETE "http://localhost:5117/api/User/image" \
           -H "Authorization: Bearer YOUR_TOKEN"
      ```

---

## **🔧 Implementation Details**

### **File Structure:**

- **DTO**: `UpdateUserImageDTO.cs` - Request/response models
- **Controller**: `UserController.cs` - API endpoints
- **Service**: `IRoleBasedImageUploadService` - Image upload logic
- **Model**: `UserImage` - Database entity

### **Key Features:**

- ✅ **Role-based folder structure** - Images stored in role-specific folders
- ✅ **Automatic deactivation** - Old images are deactivated when new ones are uploaded
- ✅ **File validation** - Size and type validation
- ✅ **Error handling** - Comprehensive error responses
- ✅ **Swagger documentation** - Full API documentation
- ✅ **Authentication** - Secure token-based access

### **Database Changes:**

- Uses existing `UserImages` table
- No schema changes required
- Images are soft-deleted (marked as inactive)

---

## **📊 Usage Examples**

### **1. Update Profile Image via Swagger:**

1. Open `http://localhost:5117/swagger`
2. Authorize with your token
3. Find `PUT /api/User/image`
4. Click "Try it out"
5. Upload an image file
6. Add optional alt text
7. Execute the request

### **2. Update Profile Image via Code:**

```csharp
using (var client = new HttpClient())
{
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", "YOUR_TOKEN");

    var form = new MultipartFormDataContent();
    var fileContent = new ByteArrayContent(File.ReadAllBytes("profile.jpg"));
    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
    form.Add(fileContent, "profileImage", "profile.jpg");
    form.Add(new StringContent("My profile picture"), "AltText");

    var response = await client.PutAsync("http://localhost:5117/api/User/image", form);
    var result = await response.Content.ReadAsStringAsync();
}
```

---

## **🎯 Success Indicators**

### **What You Should See:**

- ✅ Image upload returns success with image details
- ✅ Get image returns current profile image information
- ✅ Delete image removes the profile image
- ✅ Get current user shows updated image information
- ✅ Old images are automatically deactivated
- ✅ Images are stored in role-specific folders

### **Verification Steps:**

1. Upload an image and verify it's stored correctly
2. Get the image and verify the details match
3. Delete the image and verify it's removed
4. Check that old images are marked as inactive
5. Verify images are stored in the correct role folder

---

## **🚀 Next Steps**

### **Immediate Actions:**

1. Test the implementation using the provided test script
2. Update your profile image
3. Verify the image appears in user data

### **Future Enhancements:**

- Add image resizing/compression
- Support for multiple image formats
- Image cropping functionality
- Bulk image operations
- Image metadata extraction

---

## **📞 Support**

If you encounter any issues:

1. Check the application logs
2. Verify the authentication token
3. Ensure the image file meets validation requirements
4. Run the test script for diagnostics

---

**🎉 User Image Management APIs are ready for use!**
