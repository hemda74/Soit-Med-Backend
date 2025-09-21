# 🧪 Swagger API Testing Examples

## 📋 **Complete API Testing Guide for Role Creation**

### **Base URL**

```
http://localhost:5117
```

### **Swagger UI Access**

```
http://localhost:5117/swagger
```

---

## 🔐 **Authentication Setup**

### **Step 1: Get JWT Token**

1. **Login Endpoint:** `POST /api/Account/login`
2. **Request Body:**

```json
{
	"email": "admin@example.com",
	"password": "YourAdminPassword"
}
```

3. **Copy the `token` from the response**

### **Step 2: Authorize in Swagger**

1. Click **"Authorize"** button in Swagger UI
2. Enter: `Bearer YOUR_JWT_TOKEN_HERE`
3. Click **"Authorize"**

---

## 🏥 **Role Creation API Examples**

### **1. Doctor Creation**

**Endpoint:** `POST /api/RoleSpecificUser/doctor`

#### **Request Body (Form Data)**

```
Email: doctor@hospital.com
Password: SecurePass123
FirstName: John
LastName: Doe
Specialty: Cardiology
HospitalId: HOSP001
DepartmentId: 1
AltText: Doctor profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

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

#### **Request Body (Form Data)**

```
Email: engineer@company.com
Password: SecurePass123
FirstName: Bob
LastName: Johnson
Specialty: Biomedical Engineering
GovernorateIds: [1, 2, 3]
DepartmentId: 2
AltText: Engineer profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "ENG001",
	"email": "engineer@company.com",
	"role": "Engineer",
	"departmentName": "Engineering",
	"createdAt": "2024-01-01T00:00:00Z",
	"engineerId": 1,
	"specialty": "Biomedical Engineering",
	"assignedGovernorates": 3,
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/engineer/Bob_Johnson_Engineering_ENG001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Engineer profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Engineer 'Bob Johnson' created successfully and assigned to 3 governorates with profile image"
}
```

---

### **3. Technician Creation**

**Endpoint:** `POST /api/RoleSpecificUser/technician`

#### **Request Body (Form Data)**

```
Email: technician@hospital.com
Password: SecurePass123
FirstName: Jane
LastName: Smith
Department: Technical
HospitalId: HOSP001
DepartmentId: 3
AltText: Technician profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "TEC001",
	"email": "technician@hospital.com",
	"role": "Technician",
	"departmentName": "Technical",
	"createdAt": "2024-01-01T00:00:00Z",
	"technicianId": 1,
	"department": "Technical",
	"hospitalName": "General Hospital",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/technician/Jane_Smith_Technical_TEC001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Technician profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Technician 'Jane Smith' created successfully and assigned to hospital 'General Hospital' with profile image"
}
```

---

### **4. Admin Creation**

**Endpoint:** `POST /api/RoleSpecificUser/admin`

#### **Request Body (Form Data)**

```
Email: admin@company.com
Password: SecurePass123
FirstName: Alice
LastName: Admin
DepartmentId: 1
AltText: Admin profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "ADM001",
	"email": "admin@company.com",
	"role": "Admin",
	"departmentName": "Administration",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/admin/Alice_Admin_Administration_ADM001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Admin profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Admin 'Alice Admin' created successfully with profile image"
}
```

---

### **5. Finance Manager Creation**

**Endpoint:** `POST /api/RoleSpecificUser/finance-manager`

#### **Request Body (Form Data)**

```
Email: finance.manager@company.com
Password: SecurePass123
FirstName: Charlie
LastName: Finance
DepartmentId: 4
AltText: Finance Manager profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "FIN001",
	"email": "finance.manager@company.com",
	"role": "FinanceManager",
	"departmentName": "Finance",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/finance-manager/Charlie_Finance_Finance_FIN001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Finance Manager profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Finance Manager 'Charlie Finance' created successfully with profile image"
}
```

---

### **6. Finance Employee Creation** ⭐ **NEW**

**Endpoint:** `POST /api/RoleSpecificUser/finance-employee`

#### **Request Body (Form Data)**

```
Email: finance.employee@company.com
Password: SecurePass123
FirstName: David
LastName: Finance
DepartmentId: 4
AltText: Finance Employee profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "FEM001",
	"email": "finance.employee@company.com",
	"role": "FinanceEmployee",
	"departmentName": "Finance",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/finance-employee/David_Finance_Finance_FEM001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Finance Employee profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Finance Employee 'David Finance' created successfully with profile image"
}
```

---

### **7. Legal Manager Creation**

**Endpoint:** `POST /api/RoleSpecificUser/legal-manager`

#### **Request Body (Form Data)**

```
Email: legal.manager@company.com
Password: SecurePass123
FirstName: Eve
LastName: Legal
DepartmentId: 5
AltText: Legal Manager profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "LEG001",
	"email": "legal.manager@company.com",
	"role": "LegalManager",
	"departmentName": "Legal",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/legal-manager/Eve_Legal_Legal_LEG001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Legal Manager profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Legal Manager 'Eve Legal' created successfully with profile image"
}
```

---

### **8. Legal Employee Creation** ⭐ **NEW**

**Endpoint:** `POST /api/RoleSpecificUser/legal-employee`

#### **Request Body (Form Data)**

```
Email: legal.employee@company.com
Password: SecurePass123
FirstName: Frank
LastName: Legal
DepartmentId: 5
AltText: Legal Employee profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "LEM001",
	"email": "legal.employee@company.com",
	"role": "LegalEmployee",
	"departmentName": "Legal",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/legal-employee/Frank_Legal_Legal_LEM001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Legal Employee profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Legal Employee 'Frank Legal' created successfully with profile image"
}
```

---

### **9. Salesman Creation**

**Endpoint:** `POST /api/RoleSpecificUser/salesman`

#### **Request Body (Form Data)**

```
Email: salesman@company.com
Password: SecurePass123
FirstName: Grace
LastName: Sales
DepartmentId: 6
AltText: Salesman profile image
profileImage: [Select JPG/PNG file - Optional]
```

#### **Expected Response (200)**

```json
{
	"userId": "SAL001",
	"email": "salesman@company.com",
	"role": "Salesman",
	"departmentName": "Sales",
	"createdAt": "2024-01-01T00:00:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "uploads/salesman/Grace_Sales_Sales_SAL001/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Salesman profile image",
		"isProfileImage": true,
		"uploadedAt": "2024-01-01T00:00:00Z"
	},
	"message": "Salesman 'Grace Sales' created successfully with profile image"
}
```

---

## 🧪 **Testing Steps in Swagger**

### **Step 1: Start the Application**

```bash
cd SoitMed
dotnet run --urls "http://localhost:5117"
```

### **Step 2: Access Swagger UI**

1. Open browser: `http://localhost:5117/swagger`
2. Find the **RoleSpecificUser** section
3. You'll see all 9 endpoints listed

### **Step 3: Test Each Endpoint**

1. **Click on any endpoint** (e.g., `POST /api/RoleSpecificUser/doctor`)
2. **Click "Try it out"**
3. **Fill in the form data** using the examples above
4. **Upload an image** (optional) - JPG, PNG, or GIF (max 5MB)
5. **Click "Execute"**
6. **Check the response** - should be 200 OK with user details

### **Step 4: Test Authorization**

- **SuperAdmin/Admin**: Can create all roles
- **FinanceManager**: Can only create FinanceEmployee
- **LegalManager**: Can only create LegalEmployee

---

## 🚨 **Common Error Scenarios to Test**

### **1. Invalid Hospital ID (Doctor/Technician)**

```
HospitalId: INVALID_HOSPITAL
```

**Expected:** 400 Bad Request with error message

### **2. Invalid Governorate IDs (Engineer)**

```
GovernorateIds: [999, 998]
```

**Expected:** 400 Bad Request with error message

### **3. Duplicate Email**

```
Email: existing@email.com
```

**Expected:** 400 Bad Request with validation error

### **4. Weak Password**

```
Password: 123
```

**Expected:** 400 Bad Request with password validation error

### **5. Invalid Image File**

```
profileImage: [Upload .txt file]
```

**Expected:** 400 Bad Request with file type validation error

### **6. Unauthorized Access**

- Try creating users without proper authorization
  **Expected:** 401 Unauthorized or 403 Forbidden

---

## 📁 **Image Upload Testing**

### **Supported Formats**

- **JPG/JPEG**: `profile.jpg`
- **PNG**: `profile.png`
- **GIF**: `profile.gif`

### **File Size Limit**

- **Maximum**: 5MB
- **Test with large file**: Upload >5MB file
  **Expected:** 400 Bad Request with size validation error

### **Folder Structure Created**

```
wwwroot/uploads/
├── doctor/John_Doe_Medical_DOC001/profile.jpg
├── engineer/Bob_Johnson_Engineering_ENG001/profile.jpg
├── technician/Jane_Smith_Technical_TEC001/profile.jpg
├── admin/Alice_Admin_Administration_ADM001/profile.jpg
├── finance-manager/Charlie_Finance_Finance_FIN001/profile.jpg
├── finance-employee/David_Finance_Finance_FEM001/profile.jpg
├── legal-manager/Eve_Legal_Legal_LEG001/profile.jpg
├── legal-employee/Frank_Legal_Legal_LEM001/profile.jpg
└── salesman/Grace_Sales_Sales_SAL001/profile.jpg
```

---

## 🎯 **Quick Test Checklist**

- [ ] **Authentication**: Login and get JWT token
- [ ] **Authorization**: Set Bearer token in Swagger
- [ ] **Doctor Creation**: Test with valid hospital ID
- [ ] **Engineer Creation**: Test with valid governorate IDs
- [ ] **Technician Creation**: Test with valid hospital ID
- [ ] **Admin Creation**: Test basic creation
- [ ] **Finance Manager Creation**: Test basic creation
- [ ] **Finance Employee Creation**: Test new role
- [ ] **Legal Manager Creation**: Test basic creation
- [ ] **Legal Employee Creation**: Test new role
- [ ] **Salesman Creation**: Test basic creation
- [ ] **Image Upload**: Test with valid image file
- [ ] **Error Scenarios**: Test invalid data
- [ ] **Authorization**: Test role-based access

---

## 🚀 **Ready to Test!**

All endpoints are ready for testing in Swagger UI. The examples above provide complete request/response scenarios for comprehensive testing of the role creation system.

**Happy Testing! 🧪✨**
