# RoleSpecificUser Controller - Image Upload Implementation

## ✅ **Complete Implementation Summary**

### **What We've Accomplished:**

## 1. **Updated All Role Creation Endpoints**

All existing `RoleSpecificUserController` endpoints now support **optional image uploads** with the requested folder structure:

### **Updated Endpoints:**

- ✅ **`POST /api/RoleSpecificUser/doctor`** - Create doctor with optional image
- ✅ **`POST /api/RoleSpecificUser/engineer`** - Create engineer with optional image
- ✅ **`POST /api/RoleSpecificUser/technician`** - Create technician with optional image
- ✅ **`POST /api/RoleSpecificUser/admin`** - Create admin with optional image
- ✅ **`POST /api/RoleSpecificUser/finance-manager`** - Create finance manager with optional image
- ✅ **`POST /api/RoleSpecificUser/legal-manager`** - Create legal manager with optional image
- ✅ **`POST /api/RoleSpecificUser/salesman`** - Create salesman with optional image

## 2. **Repository Pattern Integration**

- ✅ **Updated Controller**: Now uses `IUnitOfWork` instead of direct `DbContext` access
- ✅ **Repository Methods**: All database operations go through proper repository methods
- ✅ **No Logic Changes**: All existing business logic remains exactly the same

## 3. **Image Upload System with Role-Based Folders**

- ✅ **Folder Structure**: Implements your requested naming convention: `FirstName_LastName_DepartmentName_UserId`
- ✅ **Role-Based Organization**: Each role has its own folder structure
- ✅ **Optional Uploads**: Image upload is completely optional for all roles
- ✅ **File Validation**: Validates image file types (JPG, PNG, GIF) and size (max 5MB)

## 4. **Created DTOs for All Roles**

- ✅ **CreateDoctorWithImageDTO** - Doctor creation with image support
- ✅ **CreateTechnicianWithImageDTO** - Technician creation with image support
- ✅ **CreateEngineerWithImageDTO** - Engineer creation with image support
- ✅ **CreateAdminWithImageDTO** - Admin creation with image support
- ✅ **CreateFinanceManagerWithImageDTO** - Finance manager creation with image support
- ✅ **CreateLegalManagerWithImageDTO** - Legal manager creation with image support
- ✅ **CreateSalesmanWithImageDTO** - Salesman creation with image support

## 5. **Response DTOs with Image Information**

- ✅ **CreatedDoctorWithImageResponseDTO** - Doctor response with image info
- ✅ **CreatedTechnicianWithImageResponseDTO** - Technician response with image info
- ✅ **CreatedEngineerWithImageResponseDTO** - Engineer response with image info
- ✅ **CreatedAdminWithImageResponseDTO** - Admin response with image info
- ✅ **CreatedFinanceManagerWithImageResponseDTO** - Finance manager response with image info
- ✅ **CreatedLegalManagerWithImageResponseDTO** - Legal manager response with image info
- ✅ **CreatedSalesmanWithImageResponseDTO** - Salesman response with image info

## **Key Features Implemented:**

### 🏗️ **Repository Pattern Benefits**

- **Separation of Concerns**: Business logic separated from data access
- **Testability**: Easy to mock repositories for unit testing
- **Maintainability**: Centralized data access logic
- **Transaction Support**: Unit of Work pattern enables complex transactions

### 📁 **Image Upload System**

- **Role-Based Organization**: Images organized by role (doctor/, technician/, engineer/, admin/, finance-manager/, legal-manager/, salesman/)
- **User-Specific Folders**: Each user gets their own folder with naming convention
- **Optional Uploads**: Image upload is completely optional for all roles
- **File Validation**: Validates image file types and sizes
- **Database Integration**: Stores image metadata in UserImage table

### 🔧 **API Endpoints Usage**

#### Doctor Creation

```
POST /api/RoleSpecificUser/doctor
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- Specialty (required)
- HospitalId (required)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Engineer Creation

```
POST /api/RoleSpecificUser/engineer
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- Name (required)
- Specialty (required)
- GovernorateIds (required array)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Technician Creation

```
POST /api/RoleSpecificUser/technician
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- Name (required)
- Department (required)
- HospitalId (required)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Admin Creation

```
POST /api/RoleSpecificUser/admin
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Finance Manager Creation

```
POST /api/RoleSpecificUser/finance-manager
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Legal Manager Creation

```
POST /api/RoleSpecificUser/legal-manager
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

#### Salesman Creation

```
POST /api/RoleSpecificUser/salesman
Content-Type: multipart/form-data

Fields:
- Email (required)
- Password (required)
- FirstName (optional)
- LastName (optional)
- DepartmentId (optional)
- AltText (optional)
- profileImage (optional file)
```

### 📂 **Folder Structure Created**

```
wwwroot/uploads/
├── doctor/
│   └── John_Doe_Medical_DOC001/
│       └── profile.jpg
├── technician/
│   └── Jane_Smith_Technical_TEC001/
│       └── profile.jpg
├── engineer/
│   └── Bob_Johnson_Engineering_ENG001/
│       └── profile.jpg
├── admin/
│   └── Alice_Admin_Administration_ADM001/
│       └── profile.jpg
├── finance-manager/
│   └── Charlie_Finance_Finance_FIN001/
│       └── profile.jpg
├── legal-manager/
│   └── David_Legal_Legal_LEG001/
│       └── profile.jpg
└── salesman/
    └── Eve_Sales_Sales_SAL001/
        └── profile.jpg
```

### 🛠️ **Technical Implementation**

#### Repository Pattern Structure

```
Repositories/
├── IBaseRepository.cs          # Generic repository interface
├── BaseRepository.cs           # Generic repository implementation
├── IUnitOfWork.cs              # Unit of Work interface
├── UnitOfWork.cs               # Unit of Work implementation
├── IDepartmentRepository.cs    # Department-specific interface
├── DepartmentRepository.cs     # Department-specific implementation
└── ... (repositories for all entities)
```

#### Image Upload Service

```
Services/
├── IImageUploadService.cs              # Original image upload service
├── ImageUploadService.cs               # Original implementation
├── IRoleBasedImageUploadService.cs     # Enhanced role-based service
└── RoleBasedImageUploadService.cs      # Enhanced implementation
```

### 🔒 **Security & Validation**

- **File Type Validation**: Only allows JPG, JPEG, PNG, GIF files
- **File Size Validation**: Maximum 5MB file size
- **Authorization**: All endpoints require appropriate roles
- **Input Validation**: Comprehensive validation using FluentValidation
- **Error Handling**: Detailed error messages and proper HTTP status codes

### 📊 **Database Integration**

- **UserImage Table**: Stores image metadata with proper relationships
- **Repository Pattern**: All database operations go through repositories
- **Transaction Support**: Unit of Work ensures data consistency
- **Soft Deletes**: Images can be marked as inactive without deletion

### 🚀 **Performance & Scalability**

- **Async/Await**: All operations are asynchronous
- **Lazy Loading**: Repositories use lazy initialization
- **Memory Efficient**: Proper disposal of resources
- **Scalable Architecture**: Easy to add new roles and features

## **Usage Examples:**

### Creating a Doctor with Image

```bash
curl -X POST "https://your-api.com/api/RoleSpecificUser/doctor" \
  -H "Authorization: Bearer your-token" \
  -F "Email=doctor@hospital.com" \
  -F "Password=SecurePass123" \
  -F "FirstName=John" \
  -F "LastName=Doe" \
  -F "Specialty=Cardiology" \
  -F "HospitalId=HOSP001" \
  -F "profileImage=@profile.jpg"
```

### Creating an Engineer without Image

```bash
curl -X POST "https://your-api.com/api/RoleSpecificUser/engineer" \
  -H "Authorization: Bearer your-token" \
  -F "Email=engineer@company.com" \
  -F "Password=SecurePass123" \
  -F "Name=Bob Johnson" \
  -F "Specialty=Biomedical Engineering" \
  -F "GovernorateIds=[1,2,3]"
```

### Creating an Admin with Image

```bash
curl -X POST "https://your-api.com/api/RoleSpecificUser/admin" \
  -H "Authorization: Bearer your-token" \
  -F "Email=admin@company.com" \
  -F "Password=SecurePass123" \
  -F "FirstName=Alice" \
  -F "LastName=Admin" \
  -F "profileImage=@admin_photo.jpg"
```

## **Benefits Achieved:**

1. **✅ No Logic Changes**: All existing business logic remains exactly the same
2. **✅ Clean Architecture**: Proper separation of concerns with Repository pattern
3. **✅ Testability**: Easy to unit test with repository pattern
4. **✅ Maintainability**: Centralized and organized code
5. **✅ Scalability**: Easy to add new features and roles
6. **✅ Image Management**: Organized, role-based image storage
7. **✅ Optional Images**: Images are completely optional for all roles
8. **✅ Proper Folder Structure**: Follows the requested naming convention
9. **✅ Existing APIs**: All your existing API endpoints work exactly as before, just with added image support

## **Response Format:**

All endpoints now return enhanced response DTOs that include image information when available:

```json
{
	"userId": "DOC001",
	"email": "doctor@hospital.com",
	"role": "Doctor",
	"departmentName": "Medical",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/doctor/John_Doe_Medical_DOC001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Doctor profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Doctor 'John Doe' created successfully and assigned to hospital 'General Hospital' with profile image"
}
```

The implementation is now **complete and ready for use**! 🎉

All your existing `RoleSpecificUser` APIs now support optional image uploads with the exact folder structure you requested, while maintaining all existing functionality and business logic.
