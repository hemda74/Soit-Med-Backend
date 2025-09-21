# Soit-Med Backend API Documentation for Frontend Team

## Complete User Creation API Reference

### **Base URL**

```
http://localhost:5117/api/RoleSpecificUser
```

### **Authentication**

All endpoints require JWT Bearer token authentication:

```
Authorization: Bearer <your-jwt-token>
```

---

## Available User Creation Endpoints

### **1. Doctor Creation**

**Endpoint:** `POST /api/RoleSpecificUser/doctor`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `Specialty`    | string | ✅       | Medical specialty    | Max 100 chars               |
| `HospitalId`   | string | ✅       | Hospital ID          | Max 50 chars                |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

#### **Example Request (JavaScript)**

```javascript
const formData = new FormData();
formData.append('Email', 'doctor@hospital.com');
formData.append('Password', 'SecurePass123');
formData.append('FirstName', 'John');
formData.append('LastName', 'Doe');
formData.append('Specialty', 'Cardiology');
formData.append('HospitalId', 'HOSP001');
formData.append('profileImage', fileInput.files[0]); // Optional

fetch('/api/RoleSpecificUser/doctor', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
	},
	body: formData,
});
```

#### **Response (Success - 200)**

```json
{
	"userId": "DOC001",
	"email": "doctor@hospital.com",
	"role": "Doctor",
	"departmentName": "Medical",
	"createdAt": "2024-01-01T00:00:00Z",
	"doctorId": 1,
	"specialty": "Cardiology",
	"hospitalName": "General Hospital",
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

---

### **2. Engineer Creation**

**Endpoint:** `POST /api/RoleSpecificUser/engineer`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field            | Type   | Required | Description              | Validation                  |
| ---------------- | ------ | -------- | ------------------------ | --------------------------- |
| `Email`          | string | ✅       | User's email address     | Email format, max 256 chars |
| `Password`       | string | ✅       | User's password          | Min 6 chars, max 100 chars  |
| `FirstName`      | string | ❌       | User's first name        | Max 100 chars               |
| `LastName`       | string | ❌       | User's last name         | Max 100 chars               |
| `Specialty`      | string | ✅       | Engineering specialty    | Max 100 chars               |
| `GovernorateIds` | int[]  | ✅       | Array of governorate IDs | At least 1 governorate      |
| `DepartmentId`   | int    | ❌       | Department ID            | Positive number             |
| `AltText`        | string | ❌       | Image alt text           | Max 500 chars               |
| `profileImage`   | file   | ❌       | Profile image file       | JPG, PNG, GIF, max 5MB      |

#### **Example Request (JavaScript)**

```javascript
const formData = new FormData();
formData.append('Email', 'engineer@company.com');
formData.append('Password', 'SecurePass123');
formData.append('FirstName', 'Bob');
formData.append('LastName', 'Johnson');
formData.append('Specialty', 'Biomedical Engineering');
formData.append('GovernorateIds', JSON.stringify([1, 2, 3]));
formData.append('profileImage', fileInput.files[0]); // Optional

fetch('/api/RoleSpecificUser/engineer', {
	method: 'POST',
	headers: {
		Authorization: 'Bearer ' + token,
	},
	body: formData,
});
```

---

### **3. Technician Creation**

**Endpoint:** `POST /api/RoleSpecificUser/technician`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `Department`   | string | ✅       | Technical department | Max 100 chars               |
| `HospitalId`   | string | ✅       | Hospital ID          | Max 50 chars                |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **4. Admin Creation**

**Endpoint:** `POST /api/RoleSpecificUser/admin`  
**Authorization:** `SuperAdmin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **5. Finance Manager Creation**

**Endpoint:** `POST /api/RoleSpecificUser/finance-manager`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **6. Finance Employee Creation** ⭐ **NEW**

**Endpoint:** `POST /api/RoleSpecificUser/finance-employee`  
**Authorization:** `SuperAdmin`, `Admin`, `FinanceManager`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **7. Legal Manager Creation**

**Endpoint:** `POST /api/RoleSpecificUser/legal-manager`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **8. Legal Employee Creation** ⭐ **NEW**

**Endpoint:** `POST /api/RoleSpecificUser/legal-employee`  
**Authorization:** `SuperAdmin`, `Admin`, `LegalManager`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

### **9. Salesman Creation**

**Endpoint:** `POST /api/RoleSpecificUser/salesman`  
**Authorization:** `SuperAdmin`, `Admin`  
**Content-Type:** `multipart/form-data`

#### **Request Body (Form Data)**

| Field          | Type   | Required | Description          | Validation                  |
| -------------- | ------ | -------- | -------------------- | --------------------------- |
| `Email`        | string | ✅       | User's email address | Email format, max 256 chars |
| `Password`     | string | ✅       | User's password      | Min 6 chars, max 100 chars  |
| `FirstName`    | string | ❌       | User's first name    | Max 100 chars               |
| `LastName`     | string | ❌       | User's last name     | Max 100 chars               |
| `DepartmentId` | int    | ❌       | Department ID        | Positive number             |
| `AltText`      | string | ❌       | Image alt text       | Max 500 chars               |
| `profileImage` | file   | ❌       | Profile image file   | JPG, PNG, GIF, max 5MB      |

---

## 📁 **Image Upload System**

### **Folder Structure**

Images are organized by role with user-specific folders:

```
wwwroot/uploads/
├── doctor/
│   └── John_Doe_Medical_DOC001/
│       └── profile.jpg
├── engineer/
│   └── Bob_Johnson_Engineering_ENG001/
│       └── profile.jpg
├── technician/
│   └── Jane_Smith_Technical_TEC001/
│       └── profile.jpg
├── admin/
│   └── Alice_Admin_Administration_ADM001/
│       └── profile.jpg
├── finance-manager/
│   └── Charlie_Finance_Finance_FIN001/
│       └── profile.jpg
├── finance-employee/
│   └── David_Finance_Finance_FEM001/
│       └── profile.jpg
├── legal-manager/
│   └── Eve_Legal_Legal_LEG001/
│       └── profile.jpg
├── legal-employee/
│   └── Frank_Legal_Legal_LEM001/
│       └── profile.jpg
└── salesman/
    └── Grace_Sales_Sales_SAL001/
        └── profile.jpg
```

### **Image Naming Convention**

```
{FirstName}_{LastName}_{DepartmentName}_{UserId}
```

### **Supported Image Formats**

- **Types:** JPG, JPEG, PNG, GIF
- **Max Size:** 5MB
- **Validation:** Automatic file type and size validation

---

## 🔒 **Authorization Matrix**

| Role             | Can Create                    |
| ---------------- | ----------------------------- |
| `SuperAdmin`     | All roles                     |
| `Admin`          | All roles except `SuperAdmin` |
| `FinanceManager` | `FinanceEmployee`             |
| `LegalManager`   | `LegalEmployee`               |

---

## 📝 **Error Handling**

### **Common Error Responses**

#### **400 Bad Request - Validation Errors**

```json
{
	"errors": {
		"Email": ["Email is required"],
		"Password": ["Password must be at least 6 characters long"]
	},
	"message": "Validation failed"
}
```

#### **400 Bad Request - Business Logic Errors**

```json
{
	"error": "Hospital with ID 'INVALID' not found. Please verify the hospital ID is correct.",
	"field": "HospitalId",
	"code": "HOSPITAL_NOT_FOUND"
}
```

#### **401 Unauthorized**

```json
{
	"error": "Unauthorized access. Please provide a valid token."
}
```

#### **403 Forbidden**

```json
{
	"error": "Access denied. You don't have permission to perform this action."
}
```

---

## 🚀 **Frontend Implementation Examples**

### **React/JavaScript Example**

```javascript
// User creation function
async function createUser(role, userData, profileImage = null) {
	const formData = new FormData();

	// Add all required fields
	Object.keys(userData).forEach((key) => {
		if (userData[key] !== null && userData[key] !== undefined) {
			if (
				key === 'GovernorateIds' &&
				Array.isArray(userData[key])
			) {
				formData.append(
					key,
					JSON.stringify(userData[key])
				);
			} else {
				formData.append(key, userData[key]);
			}
		}
	});

	// Add profile image if provided
	if (profileImage) {
		formData.append('profileImage', profileImage);
	}

	try {
		const response = await fetch(`/api/RoleSpecificUser/${role}`, {
			method: 'POST',
			headers: {
				Authorization: `Bearer ${getToken()}`,
			},
			body: formData,
		});

		if (!response.ok) {
			const errorData = await response.json();
			throw new Error(
				errorData.message || 'User creation failed'
			);
		}

		return await response.json();
	} catch (error) {
		console.error('Error creating user:', error);
		throw error;
	}
}

// Usage examples
const doctorData = {
	Email: 'doctor@hospital.com',
	Password: 'SecurePass123',
	FirstName: 'John',
	LastName: 'Doe',
	Specialty: 'Cardiology',
	HospitalId: 'HOSP001',
};

const engineerData = {
	Email: 'engineer@company.com',
	Password: 'SecurePass123',
	FirstName: 'Bob',
	LastName: 'Johnson',
	Specialty: 'Biomedical Engineering',
	GovernorateIds: [1, 2, 3],
};

// Create doctor
const doctor = await createUser('doctor', doctorData, profileImageFile);

// Create engineer
const engineer = await createUser('engineer', engineerData);
```

### **Vue.js Example**

```javascript
// Vue component method
async createUser() {
    const formData = new FormData();

    // Add form data
    formData.append('Email', this.userForm.email);
    formData.append('Password', this.userForm.password);
    formData.append('FirstName', this.userForm.firstName);
    formData.append('LastName', this.userForm.lastName);

    // Add role-specific fields
    if (this.selectedRole === 'doctor') {
        formData.append('Specialty', this.userForm.specialty);
        formData.append('HospitalId', this.userForm.hospitalId);
    } else if (this.selectedRole === 'engineer') {
        formData.append('Specialty', this.userForm.specialty);
        formData.append('GovernorateIds', JSON.stringify(this.userForm.governorateIds));
    }

    // Add profile image
    if (this.profileImage) {
        formData.append('profileImage', this.profileImage);
    }

    try {
        const response = await this.$http.post(
            `/api/RoleSpecificUser/${this.selectedRole}`,
            formData,
            {
                headers: {
                    'Authorization': `Bearer ${this.$store.getters.token}`
                }
            }
        );

        this.$toast.success('User created successfully!');
        this.resetForm();
    } catch (error) {
        this.$toast.error(error.response.data.message || 'Failed to create user');
    }
}
```

### **Angular Example**

```typescript
// Angular service
@Injectable()
export class UserService {
    constructor(private http: HttpClient) {}

    createUser(role: string, userData: any, profileImage?: File): Observable<any> {
        const formData = new FormData();

        // Add user data
        Object.keys(userData).forEach(key => {
            if (userData[key] !== null && userData[key] !== undefined) {
                if (key === 'GovernorateIds' && Array.isArray(userData[key])) {
                    formData.append(key, JSON.stringify(userData[key]));
                } else {
                    formData.append(key, userData[key]);
                }
            }
        });

        // Add profile image
        if (profileImage) {
            formData.append('profileImage', profileImage);
        }

        return this.http.post(`/api/RoleSpecificUser/${role}`, formData, {
            headers: {
                'Authorization': `Bearer ${this.getToken()}`
            }
        });
    }
}

// Component usage
createUser() {
    const userData = {
        Email: this.userForm.value.email,
        Password: this.userForm.value.password,
        FirstName: this.userForm.value.firstName,
        LastName: this.userForm.value.lastName
    };

    this.userService.createUser(this.selectedRole, userData, this.profileImage)
        .subscribe({
            next: (response) => {
                this.toastr.success('User created successfully!');
                this.resetForm();
            },
            error: (error) => {
                this.toastr.error(error.error.message || 'Failed to create user');
            }
        });
}
```

---

## 🎯 **Quick Reference**

### **All Available Endpoints**

```
POST /api/RoleSpecificUser/doctor
POST /api/RoleSpecificUser/engineer
POST /api/RoleSpecificUser/technician
POST /api/RoleSpecificUser/admin
POST /api/RoleSpecificUser/finance-manager
POST /api/RoleSpecificUser/finance-employee
POST /api/RoleSpecificUser/legal-manager
POST /api/RoleSpecificUser/legal-employee
POST /api/RoleSpecificUser/salesman
```

### **Required Headers**

```
Authorization: Bearer <jwt-token>
Content-Type: multipart/form-data
```

### **Image Upload**

- **Optional** for all roles
- **Max size:** 5MB
- **Formats:** JPG, PNG, GIF
- **Field name:** `profileImage`

### **Response Format**

All successful responses include:

- `userId`: Generated user ID
- `email`: User's email
- `role`: Assigned role
- `departmentName`: Department name
- `createdAt`: Creation timestamp
- `profileImage`: Image info (if uploaded)
- `message`: Success message

---

## 🔧 **Testing with Swagger**

1. **Start the application:** `dotnet run`
2. **Access Swagger UI:** `http://localhost:5117/swagger`
3. **Authorize:** Click "Authorize" and enter your JWT token
4. **Test endpoints:** Use the interactive API documentation

---

## 📞 **Support**

For any questions or issues with the API, please contact the backend development team.

**Happy coding! 🚀**
