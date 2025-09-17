# Admin User Creation Flow - Complete API Documentation

## User Story

> **As an Admin**, I want to create a user by first selecting a role, then seeing role-specific fields, filling the data, and sending it to the backend to be saved.

---

## Complete Flow - All APIs Exist!

### **Step 1: Get All Available Roles**

```http
GET /api/Role/available
Authorization: Bearer {admin-token}
```

**Response:**

```json
{
	"roles": [
		"SuperAdmin",
		"Admin",
		"Doctor",
		"Technician",
		"Salesman",
		"Engineer",
		"FinanceManager",
		"FinanceEmployee",
		"LegalManager",
		"LegalEmployee"
	]
}
```

---

### **Step 2: Get Role-Specific Fields** - **NEW API**

```http
GET /api/Role/fields/{role}
Authorization: Bearer {admin-token}
Roles: SuperAdmin, Admin
```

**Example: Get Doctor Fields**

```http
GET /api/Role/fields/Doctor
```

**Response:**

```json
{
	"role": "Doctor",
	"department": "Medical",
	"baseFields": [
		{
			"name": "userName",
			"type": "string",
			"required": true,
			"label": "Username"
		},
		{
			"name": "email",
			"type": "email",
			"required": true,
			"label": "Email Address"
		},
		{
			"name": "password",
			"type": "password",
			"required": true,
			"label": "Password"
		},
		{
			"name": "firstName",
			"type": "string",
			"required": false,
			"label": "First Name"
		},
		{
			"name": "lastName",
			"type": "string",
			"required": false,
			"label": "Last Name"
		},
		{
			"name": "departmentId",
			"type": "number",
			"required": false,
			"label": "Department ID (Optional)"
		}
	],
	"roleSpecificFields": [
		{
			"name": "specialty",
			"type": "string",
			"required": true,
			"label": "Medical Specialty"
		},
		{
			"name": "hospitalId",
			"type": "string",
			"required": true,
			"label": "Hospital ID"
		}
	],
	"requiredData": [
		{
			"endpoint": "/api/Hospital",
			"description": "Get list of hospitals for hospitalId selection"
		}
	],
	"createEndpoint": "/api/RoleSpecificUser/doctor",
	"message": "Fields required to create a Doctor user"
}
```

---

### **Step 3: Get Required Reference Data**

Based on `requiredData` from Step 2, fetch dropdown/selection data:

**For Doctor/Technician - Get Hospitals:**

```http
GET /api/Hospital
Authorization: Bearer {admin-token}
```

**For Engineer - Get Governorates:**

```http
GET /api/Governorate
Authorization: Bearer {admin-token}
```

**For All Roles - Get Departments (Optional):**

```http
GET /api/Department
Authorization: Bearer {admin-token}
```

---

### **Step 4: Create User with Role-Specific Data**

Use the `createEndpoint` from Step 2 response:

**Example: Create Doctor**

```http
POST /api/RoleSpecificUser/doctor
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "userName": "dr.smith",
  "email": "smith@hospital.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Smith",
  "specialty": "Cardiology",
  "hospitalId": "HOSP001",
  "departmentId": 2
}
```

**Response:**

```json
{
	"userId": "abc123...",
	"userName": "dr.smith",
	"email": "smith@hospital.com",
	"role": "Doctor",
	"departmentName": "Medical",
	"createdAt": "2025-09-07T16:30:00Z",
	"doctorId": 1,
	"specialty": "Cardiology",
	"hospitalName": "General Hospital",
	"message": "Doctor 'John Smith' created successfully and assigned to hospital 'General Hospital'"
}
```

---

## Role-Specific Field Examples

### **Engineer Fields**

```http
GET /api/Role/fields/Engineer
```

**Response includes:**

- `specialty` (required) - Engineering specialty
- `governorateIds` (required) - Array of governorate IDs
- **Required Data:** `/api/Governorate` for governorate selection

### **Technician Fields**

```http
GET /api/Role/fields/Technician
```

**Response includes:**

- `department` (required) - Technical department
- `hospitalId` (required) - Hospital assignment
- **Required Data:** `/api/Hospital` for hospital selection

### **Admin Fields**

```http
GET /api/Role/fields/Admin
```

**Response includes:**

- `accessLevel` (optional) - Access level specification

### **Finance Manager Fields**

```http
GET /api/Role/fields/FinanceManager
```

**Response includes:**

- `budgetAuthority` (optional) - Budget authority level

### **Legal Manager Fields**

```http
GET /api/Role/fields/LegalManager
```

**Response includes:**

- `legalSpecialty` (optional) - Legal specialty area

### **Salesman Fields**

```http
GET /api/Role/fields/Salesman
```

**Response includes:**

- `territory` (optional) - Sales territory
- `salesTarget` (optional) - Sales targets

---

## Frontend Implementation Guide

### **Step 1: Role Selection Dropdown**

```javascript
// Get available roles
const rolesResponse = await fetch('/api/Role/available', {
	headers: { Authorization: `Bearer ${token}` },
});
const { roles } = await rolesResponse.json();

// Display in dropdown
roles.forEach((role) => {
	// Add to select element
});
```

### **Step 2: Dynamic Form Generation**

```javascript
// When role is selected
const selectedRole = 'Doctor';
const fieldsResponse = await fetch(`/api/Role/fields/${selectedRole}`, {
	headers: { Authorization: `Bearer ${token}` },
});
const fieldData = await fieldsResponse.json();

// Generate form fields dynamically
const form = document.getElementById('userForm');

// Add base fields
fieldData.baseFields.forEach((field) => {
	const input = createInputElement(field);
	form.appendChild(input);
});

// Add role-specific fields
fieldData.roleSpecificFields.forEach((field) => {
	const input = createInputElement(field);
	form.appendChild(input);
});

// Fetch required reference data
fieldData.requiredData.forEach(async (dataReq) => {
	const refData = await fetch(dataReq.endpoint, {
		headers: { Authorization: `Bearer ${token}` },
	});
	// Populate dropdowns with reference data
});
```

### **Step 3: Form Submission**

```javascript
// Submit form data
const formData = new FormData(form);
const userData = Object.fromEntries(formData);

const createResponse = await fetch(fieldData.createEndpoint, {
	method: 'POST',
	headers: {
		Authorization: `Bearer ${token}`,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify(userData),
});

const result = await createResponse.json();
// Handle success/error response
```

---

## Complete API Flow Summary

| Step | API Endpoint                        | Purpose                               | Required Auth |
| ---- | ----------------------------------- | ------------------------------------- | ------------- |
| 1    | `GET /api/Role/available`           | Get all roles for selection           | Admin+        |
| 2    | `GET /api/Role/fields/{role}`       | Get fields for selected role          | Admin+        |
| 3a   | `GET /api/Hospital`                 | Get hospitals (for Doctor/Technician) | Admin+        |
| 3b   | `GET /api/Governorate`              | Get governorates (for Engineer)       | Admin+        |
| 3c   | `GET /api/Department`               | Get departments (optional)            | Admin+        |
| 4    | `POST /api/RoleSpecificUser/{role}` | Create user with role-specific data   | Admin+        |

---

## Testing the Flow

### **Test with Swagger UI:**

1. **Login as Admin:**

      ```http
      POST /api/Account/login
      {
        "userName": "your-admin",
        "password": "your-password"
      }
      ```

2. **Authorize in Swagger:**

      - Click "Authorize" button
      - Enter: `Bearer {your-token}`

3. **Test the Flow:**
      - `GET /api/Role/available`
      - `GET /api/Role/fields/Doctor`
      - `GET /api/Hospital`
      - `POST /api/RoleSpecificUser/doctor`

---

## **All APIs for Your User Story Now Exist!**

The complete admin user creation flow is fully implemented with:

- Role selection API
- **NEW** Role-specific fields API
- Reference data APIs
- Role-specific creation APIs
- Comprehensive field validation
- Dynamic form generation support
- Complete error handling

Your frontend can now implement the exact user story flow you described!
