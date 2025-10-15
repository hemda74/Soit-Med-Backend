# Role-Specific User Creation with Images API

## Overview

All RoleSpecificUser endpoints now support optional profile image uploads in addition to user creation. Images are completely optional and can be uploaded during user creation.

## Updated Endpoints

All endpoints now accept `multipart/form-data` instead of `application/json` and include optional image fields.

### 1. Create Doctor with Profile Image

**POST** `/api/RoleSpecificUser/doctor`

**Request:** `multipart/form-data`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)
- `Specialty` (string): Medical specialty
- `HospitalId` (string): Hospital ID

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Medical if not provided)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/doctor" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=dr.ahmed.hassan@hospital.com" \
  -F "Password=SecurePass123!" \
  -F "FirstName=Ahmed" \
  -F "LastName=Hassan" \
  -F "Specialty=Cardiology" \
  -F "HospitalId=HOSP001" \
  -F "ProfileImage=@profile.jpg" \
  -F "ImageAltText=Dr. Ahmed Hassan profile picture"
```

#### Response:

```json
{
	"userId": "Ahmed_Hassan_CairoUniversityHospital_001",
	"email": "dr.ahmed.hassan@hospital.com",
	"role": "Doctor",
	"departmentName": "Medical",
	"createdAt": "2025-09-17T20:00:00Z",
	"doctorId": 1,
	"specialty": "Cardiology",
	"hospitalName": "Cairo University Hospital",
	"profileImage": {
		"id": 1,
		"userId": "Ahmed_Hassan_CairoUniversityHospital_001",
		"fileName": "profile.jpg",
		"filePath": "uploads/user-images/Ahmed_Hassan_CairoUniversityHospital_001/guid.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Dr. Ahmed Hassan profile picture",
		"uploadedAt": "2025-09-17T20:00:00Z",
		"isActive": true,
		"isProfileImage": true
	},
	"message": "Doctor 'Ahmed Hassan' created successfully and assigned to hospital 'Cairo University Hospital' with profile image"
}
```

### 2. Create Engineer with Profile Image

**POST** `/api/RoleSpecificUser/engineer`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)
- `Specialty` (string): Engineering specialty
- `GovernorateIds` (array): Array of governorate IDs

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Engineering if not provided)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/engineer" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=eng.mohamed.ali@company.com" \
  -F "Password=SecurePass123!" \
  -F "FirstName=Mohamed" \
  -F "LastName=Ali" \
  -F "Specialty=Biomedical Engineering" \
  -F "GovernorateIds=[1,2,3]" \
  -F "ProfileImage=@engineer-profile.jpg" \
  -F "ImageAltText=Mohamed Ali - Biomedical Engineer"
```

### 3. Create Technician with Profile Image

**POST** `/api/RoleSpecificUser/technician`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)
- `Department` (string): Technical department
- `HospitalId` (string): Hospital ID

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Medical if not provided)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/technician" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=tech.sara.mahmoud@hospital.com" \
  -F "Password=SecurePass123!" \
  -F "FirstName=Sara" \
  -F "LastName=Mahmoud" \
  -F "Department=Radiology" \
  -F "HospitalId=HOSP001" \
  -F "ProfileImage=@technician-profile.jpg" \
  -F "ImageAltText=Sara Mahmoud - Radiology Technician"
```

### 4. Create Admin with Profile Image

**POST** `/api/RoleSpecificUser/admin`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Administration if not provided)
- `AccessLevel` (string): Access level (Full, Limited, etc.)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

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

### 5. Create Finance Manager with Profile Image

**POST** `/api/RoleSpecificUser/finance-manager`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Finance if not provided)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/finance-manager" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=finance.mary.smith@company.com" \
  -F "Password=FinancePass123!" \
  -F "FirstName=Mary" \
  -F "LastName=Smith" \
  -F "ProfileImage=@finance-profile.jpg" \
  -F "ImageAltText=Mary Smith - Finance Manager"
```

### 6. Create Legal Manager with Profile Image

**POST** `/api/RoleSpecificUser/legal-manager`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Legal if not provided)
- `LegalSpecialty` (string): Legal specialty (contracts, compliance, etc.)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/legal-manager" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=legal.david.wilson@company.com" \
  -F "Password=LegalPass123!" \
  -F "FirstName=David" \
  -F "LastName=Wilson" \
  -F "LegalSpecialty=Contract Law" \
  -F "ProfileImage=@legal-profile.jpg" \
  -F "ImageAltText=David Wilson - Legal Manager"
```

### 7. Create Salesman with Profile Image

**POST** `/api/RoleSpecificUser/salesman`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Sales if not provided)
- `Territory` (string): Sales territory
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/salesman" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=sales.mike.johnson@company.com" \
  -F "Password=SalesPass123!" \
  -F "FirstName=Mike" \
  -F "LastName=Johnson" \
  -F "Territory=North Region" \
  -F "ProfileImage=@sales-profile.jpg" \
  -F "ImageAltText=Mike Johnson - Sales Representative"
```

### 8. Create Finance Employee with Profile Image

**POST** `/api/RoleSpecificUser/finance-employee`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Finance if not provided)
- `JobTitle` (string): Job title (Accountant, Financial Analyst, etc.)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/finance-employee" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=finance.employee@company.com" \
  -F "Password=FinanceEmpPass123!" \
  -F "FirstName=Sarah" \
  -F "LastName=Wilson" \
  -F "JobTitle=Financial Analyst" \
  -F "ProfileImage=@finance-employee-profile.jpg" \
  -F "ImageAltText=Sarah Wilson - Financial Analyst"
```

### 9. Create Legal Employee with Profile Image

**POST** `/api/RoleSpecificUser/legal-employee`

#### Required Fields:

- `Email` (string): Email address
- `Password` (string): Password (min 6 characters)

#### Optional Fields:

- `FirstName` (string): First name
- `LastName` (string): Last name
- `DepartmentId` (int): Department ID (auto-assigned to Legal if not provided)
- `JobTitle` (string): Job title (Legal Assistant, Paralegal, etc.)
- `LegalSpecialty` (string): Legal specialty (contracts, compliance, etc.)
- `ProfileImage` (file): Profile image file
- `ImageAltText` (string): Alternative text for the image

#### Example Request:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/legal-employee" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=legal.employee@company.com" \
  -F "Password=LegalEmpPass123!" \
  -F "FirstName=David" \
  -F "LastName=Brown" \
  -F "JobTitle=Legal Assistant" \
  -F "LegalSpecialty=Contract Review" \
  -F "ProfileImage=@legal-employee-profile.jpg" \
  -F "ImageAltText=David Brown - Legal Assistant"
```

## Image Upload Features

### File Validation

- **Supported Types**: JPEG, JPG, PNG, GIF, WebP
- **Maximum Size**: 10MB
- **Silent Failure**: Invalid images are skipped without error (user is still created)

### Image Processing

- **Automatic Organization**: Files are organized by user ID in `uploads/user-images/{userId}/`
- **Unique Filenames**: Generated using GUID to prevent conflicts
- **Profile Image**: Automatically set as profile image
- **Database Record**: Creates UserImage record with metadata

### Response Format

All endpoints now return a `profileImage` field in the response containing:

- Image metadata (ID, filename, path, size, etc.)
- Upload timestamp
- Alt text
- Profile image status

## Frontend Integration

### HTML Form Example

```html
<form
	id="doctorCreationForm"
	enctype="multipart/form-data"
>
	<div>
		<label>Email:</label>
		<input
			type="email"
			name="Email"
			required
		/>
	</div>

	<div>
		<label>Password:</label>
		<input
			type="password"
			name="Password"
			required
		/>
	</div>

	<div>
		<label>First Name:</label>
		<input
			type="text"
			name="FirstName"
		/>
	</div>

	<div>
		<label>Last Name:</label>
		<input
			type="text"
			name="LastName"
		/>
	</div>

	<div>
		<label>Specialty:</label>
		<input
			type="text"
			name="Specialty"
			required
		/>
	</div>

	<div>
		<label>Hospital ID:</label>
		<input
			type="text"
			name="HospitalId"
			required
		/>
	</div>

	<div>
		<label>Profile Image (Optional):</label>
		<input
			type="file"
			name="ProfileImage"
			accept="image/*"
		/>
	</div>

	<div>
		<label>Image Description (Optional):</label>
		<input
			type="text"
			name="ImageAltText"
			placeholder="Describe the image"
		/>
	</div>

	<button type="submit">Create Doctor</button>
</form>
```

### JavaScript Example

```javascript
document.getElementById('doctorCreationForm').addEventListener(
	'submit',
	async function (e) {
		e.preventDefault();

		const formData = new FormData(this);

		try {
			const response = await fetch(
				'/api/RoleSpecificUser/doctor',
				{
					method: 'POST',
					headers: {
						Authorization: `Bearer ${getAuthToken()}`,
					},
					body: formData,
				}
			);

			const result = await response.json();

			if (response.ok) {
				console.log(
					'Doctor created successfully:',
					result
				);
				alert(
					`Doctor created successfully${
						result.profileImage
							? ' with profile image'
							: ''
					}!`
				);
				this.reset();
			} else {
				console.error('Error creating doctor:', result);
				alert(
					'Error creating doctor: ' +
						(result.message ||
							'Unknown error')
				);
			}
		} catch (error) {
			console.error('Network error:', error);
			alert('Network error occurred. Please try again.');
		}
	}
);
```

## Error Handling

### Common Error Responses

**400 Bad Request - Validation Errors:**

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"Email": ["Email is required"],
		"Password": ["Password must be at least 6 characters long"]
	}
}
```

**400 Bad Request - Business Logic Errors:**

```json
{
	"success": false,
	"message": "Hospital with ID 'INVALID' not found. Please verify the hospital ID is correct.",
	"field": "HospitalId",
	"code": "HOSPITAL_NOT_FOUND"
}
```

**401 Unauthorized:**

```json
{
	"success": false,
	"message": "Unauthorized access. Please provide a valid token."
}
```

## Database Impact

Each endpoint creates the following database records:

- User account in `AspNetUsers`
- Role assignment in `AspNetUserRoles`
- Role-specific record (Doctor, Engineer, Technician, etc.)
- Image record in `UserImages` (if image provided)
- Department assignment
- Governorate assignments (for Engineers)

## Migration from JSON to Form Data

### Before (JSON):

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/doctor" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "Email": "dr.ahmed.hassan@hospital.com",
    "Password": "SecurePass123!",
    "FirstName": "Ahmed",
    "LastName": "Hassan",
    "Specialty": "Cardiology",
    "HospitalId": "HOSP001"
  }'
```

### After (Form Data with Image):

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/doctor" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "Email=dr.ahmed.hassan@hospital.com" \
  -F "Password=SecurePass123!" \
  -F "FirstName=Ahmed" \
  -F "LastName=Hassan" \
  -F "Specialty=Cardiology" \
  -F "HospitalId=HOSP001" \
  -F "ProfileImage=@profile.jpg" \
  -F "ImageAltText=Dr. Ahmed Hassan profile picture"
```

## Benefits

1. **Single Request**: Create user and upload image in one call
2. **Optional Images**: Images are completely optional
3. **Consistent API**: All role-specific endpoints support images
4. **Backward Compatible**: Existing functionality remains unchanged
5. **Flexible**: Can create users with or without images
6. **Validated**: Same image validation as separate image API
7. **Organized**: Images are automatically organized by user

This comprehensive update provides a streamlined way to create users with profile images across all role-specific endpoints while maintaining all existing functionality!
