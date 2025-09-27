# 📤 POST User Image API Documentation

## **Overview**

The POST User Image API allows users to upload their profile images. This is a simple, clean API that handles image uploads with proper validation and role-based storage.

---

## **🔧 API Endpoint**

### **Upload User Profile Image**

```http
POST /api/User/image
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Request Body (Form Data):**

- `profileImage` (IFormFile, required) - The profile image file
- `AltText` (string, optional) - Alternative text for the image

#### **Response:**

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
		"altText": "Profile image description",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T10:30:00Z",
		"isActive": true
	},
	"updatedAt": "2025-01-21T10:30:00Z"
}
```

---

## **🔐 Authentication & Authorization**

### **Required Authentication:**

- Bearer token obtained from `/api/Account/login`
- Users can only upload images for their own account

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
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server-side errors

---

## **🧪 Testing**

### **Test Script:**

```powershell
# Run the POST image API test
.\test-post-image-api.ps1
```

### **Manual Testing with Swagger:**

1. **Start the application:**

      ```bash
      dotnet run --urls "http://localhost:5117"
      ```

2. **Access Swagger UI:**

      - Navigate to: `http://localhost:5117/swagger`
      - Find the `User` section
      - Look for `POST /api/User/image`

3. **Test with curl:**
      ```bash
      curl -X POST "http://localhost:5117/api/User/image" \
           -H "Authorization: Bearer YOUR_TOKEN" \
           -F "profileImage=@profile.jpg" \
           -F "AltText=My profile picture"
      ```

---

## **🔧 Implementation Details**

### **Key Features:**

- ✅ **Simple POST endpoint** - Easy to use for image uploads
- ✅ **Role-based storage** - Images stored in role-specific folders
- ✅ **Automatic deactivation** - Old images are deactivated when new ones are uploaded
- ✅ **File validation** - Size and type validation
- ✅ **Error handling** - Comprehensive error responses
- ✅ **Swagger documentation** - Full API documentation

### **File Structure:**

- **Controller**: `UserController.cs` - POST endpoint
- **DTO**: `UpdateUserImageDTO.cs` - Request/response models
- **Service**: `IRoleBasedImageUploadService` - Image upload logic
- **Model**: `UserImage` - Database entity

---

## **📊 Usage Examples**

### **1. Upload Profile Image via Swagger:**

1. Open `http://localhost:5117/swagger`
2. Authorize with your token
3. Find `POST /api/User/image`
4. Click "Try it out"
5. Upload an image file
6. Add optional alt text
7. Execute the request

### **2. Upload Profile Image via Code:**

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

    var response = await client.PostAsync("http://localhost:5117/api/User/image", form);
    var result = await response.Content.ReadAsStringAsync();
}
```

### **3. Upload Profile Image via PowerShell:**

```powershell
$headers = @{
    "Authorization" = "Bearer YOUR_TOKEN"
}

$form = @{
    profileImage = Get-Item "profile.jpg"
    AltText = "My profile picture"
}

$response = Invoke-RestMethod -Uri "http://localhost:5117/api/User/image" -Method POST -Headers $headers -Form $form
Write-Host "Upload successful: $($response.message)"
```

---

## **🎯 Success Indicators**

### **What You Should See:**

- ✅ Image upload returns success with image details
- ✅ Image is stored in the correct role-specific folder
- ✅ Old images are automatically deactivated
- ✅ Image appears in user data when calling `/api/User/me`
- ✅ Proper validation for file size and type

### **Verification Steps:**

1. Upload an image and verify it's stored correctly
2. Check that the image appears in user data
3. Verify old images are marked as inactive
4. Test with different file types and sizes
5. Verify images are stored in the correct role folder

---

## **🚀 Next Steps**

### **Immediate Actions:**

1. Start the application: `dotnet run --urls "http://localhost:5117"`
2. Test the POST image API using Swagger UI
3. Upload your first profile image
4. Verify the image appears in user data

### **Future Enhancements:**

- Add image resizing/compression
- Support for multiple image formats
- Image cropping functionality
- Bulk image operations

---

## **📞 Support**

If you encounter any issues:

1. Check the application logs
2. Verify the authentication token
3. Ensure the image file meets validation requirements
4. Run the test script for diagnostics

---

**🎉 POST User Image API is ready for use!**
