# Role-Specific User Creation API Examples

## Authentication Required

All endpoints require Admin or SuperAdmin authentication:

```
Authorization: Bearer {your-jwt-token}
```

## Base URL

```
http://localhost:5117
```

---

## 1. Create Doctor

**Endpoint:** `POST /api/RoleSpecificUser/doctor`

**Required Fields:**

- userName, email, password (base fields)
- specialty (medical specialty)
- hospitalId (hospital assignment)

**Example Request:**

```http
POST /api/RoleSpecificUser/doctor
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
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

**Example Response:**

```json
{
	"userId": "d4a1c071-7d0e-422a-b872-be29be85ce23",
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

## 2. Create Engineer

**Endpoint:** `POST /api/RoleSpecificUser/engineer`

**Required Fields:**

- userName, email, password (base fields)
- specialty (engineering specialty)
- governorateIds (array of governorate IDs)

**Example Request:**

```http
POST /api/RoleSpecificUser/engineer
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "eng.ahmed",
  "email": "ahmed@company.com",
  "password": "SecurePass123!",
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "specialty": "Biomedical Engineering",
  "governorateIds": [1, 2, 3],
  "departmentId": 4
}
```

**Example Response:**

```json
{
	"userId": "a5b2d182-8e1f-533b-c983-cf3ace96df34",
	"userName": "eng.ahmed",
	"email": "ahmed@company.com",
	"role": "Engineer",
	"departmentName": "Engineering",
	"createdAt": "2025-09-07T16:30:00Z",
	"engineerId": 1,
	"specialty": "Biomedical Engineering",
	"assignedGovernorates": ["Cairo", "Alexandria", "Giza"],
	"message": "Engineer 'Ahmed Hassan' created successfully and assigned to 3 governorate(s)"
}
```

---

## 3. Create Technician

**Endpoint:** `POST /api/RoleSpecificUser/technician`

**Required Fields:**

- userName, email, password (base fields)
- department (technical department)
- hospitalId (hospital assignment)

**Example Request:**

```http
POST /api/RoleSpecificUser/technician
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "tech.mohamed",
  "email": "mohamed@hospital.com",
  "password": "SecurePass123!",
  "firstName": "Mohamed",
  "lastName": "Ali",
  "department": "Radiology",
  "hospitalId": "HOSP001",
  "departmentId": 2
}
```

**Example Response:**

```json
{
	"userId": "c6d3e293-9f2g-644c-d094-dg4bdf97eg45",
	"userName": "tech.mohamed",
	"email": "mohamed@hospital.com",
	"role": "Technician",
	"departmentName": "Medical",
	"createdAt": "2025-09-07T16:30:00Z",
	"technicianId": 1,
	"department": "Radiology",
	"hospitalName": "General Hospital",
	"message": "Technician 'Mohamed Ali' created successfully and assigned to hospital 'General Hospital'"
}
```

---

## 4. Create Admin

**Endpoint:** `POST /api/RoleSpecificUser/admin`

**Required Fields:**

- userName, email, password (base fields)
- accessLevel (optional)

**Authorization:** SuperAdmin only

**Example Request:**

```http
POST /api/RoleSpecificUser/admin
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "admin.sara",
  "email": "sara@company.com",
  "password": "SecurePass123!",
  "firstName": "Sara",
  "lastName": "Mahmoud",
  "accessLevel": "Full Access",
  "departmentId": 1
}
```

**Example Response:**

```json
{
	"userId": "e7f4g405-0h3i-755d-e205-ei5cgf08fi56",
	"userName": "admin.sara",
	"email": "sara@company.com",
	"role": "Admin",
	"departmentName": "Administration",
	"createdAt": "2025-09-07T16:30:00Z",
	"message": "Admin 'admin.sara' created successfully"
}
```

---

## 5. Create Finance Manager

**Endpoint:** `POST /api/RoleSpecificUser/finance-manager`

**Required Fields:**

- userName, email, password (base fields)
- budgetAuthority (optional)

**Example Request:**

```http
POST /api/RoleSpecificUser/finance-manager
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "finance.omar",
  "email": "omar@company.com",
  "password": "SecurePass123!",
  "firstName": "Omar",
  "lastName": "Khaled",
  "budgetAuthority": "Up to 1M EGP",
  "departmentId": 5
}
```

**Example Response:**

```json
{
	"userId": "f8g5h516-1i4j-866e-f316-fj6dih19gj67",
	"userName": "finance.omar",
	"email": "omar@company.com",
	"role": "FinanceManager",
	"departmentName": "Finance",
	"createdAt": "2025-09-07T16:30:00Z",
	"message": "Finance Manager 'finance.omar' created successfully"
}
```

---

## 6. Create Legal Manager

**Endpoint:** `POST /api/RoleSpecificUser/legal-manager`

**Required Fields:**

- userName, email, password (base fields)
- legalSpecialty (optional)

**Example Request:**

```http
POST /api/RoleSpecificUser/legal-manager
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "legal.fatma",
  "email": "fatma@company.com",
  "password": "SecurePass123!",
  "firstName": "Fatma",
  "lastName": "Youssef",
  "legalSpecialty": "Healthcare Law",
  "departmentId": 6
}
```

**Example Response:**

```json
{
	"userId": "g9h6i627-2j5k-977f-g427-gk7ejk20hk78",
	"userName": "legal.fatma",
	"email": "fatma@company.com",
	"role": "LegalManager",
	"departmentName": "Legal",
	"createdAt": "2025-09-07T16:30:00Z",
	"message": "Legal Manager 'legal.fatma' created successfully"
}
```

---

## 7. Create Salesman

**Endpoint:** `POST /api/RoleSpecificUser/salesman`

**Required Fields:**

- userName, email, password (base fields)
- territory (optional)
- salesTarget (optional)

**Example Request:**

```http
POST /api/RoleSpecificUser/salesman
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "sales.hassan",
  "email": "hassan@company.com",
  "password": "SecurePass123!",
  "firstName": "Hassan",
  "lastName": "Ibrahim",
  "territory": "Cairo Region",
  "salesTarget": "500K EGP/month",
  "departmentId": 3
}
```

**Example Response:**

```json
{
	"userId": "h0i7j738-3k6l-088g-h538-hl8fkl31il89",
	"userName": "sales.hassan",
	"email": "hassan@company.com",
	"role": "Salesman",
	"departmentName": "Sales",
	"createdAt": "2025-09-07T16:30:00Z",
	"message": "Salesman 'sales.hassan' created successfully"
}
```

---

## 8. Create Finance Employee

**Endpoint:** `POST /api/RoleSpecificUser/finance-employee`

**Required Fields:**

- userName, email, password (base fields only)

**Example Request:**

```http
POST /api/RoleSpecificUser/finance-employee
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "fin.employee",
  "email": "employee@company.com",
  "password": "SecurePass123!",
  "firstName": "Nour",
  "lastName": "Ahmed",
  "departmentId": 5
}
```

---

## 9. Create Legal Employee

**Endpoint:** `POST /api/RoleSpecificUser/legal-employee`

**Required Fields:**

- userName, email, password (base fields only)

**Example Request:**

```http
POST /api/RoleSpecificUser/legal-employee
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "userName": "legal.employee",
  "email": "legalemployee@company.com",
  "password": "SecurePass123!",
  "firstName": "Yara",
  "lastName": "Mostafa",
  "departmentId": 6
}
```

---

## Prerequisites for Testing

### 1. Get Hospitals (for Doctor/Technician creation)

```http
GET /api/Hospital
Authorization: Bearer {token}
```

### 2. Get Governorates (for Engineer creation)

```http
GET /api/Governorate
Authorization: Bearer {token}
```

### 3. Get Departments (optional for all roles)

```http
GET /api/Department
Authorization: Bearer {token}
```

---

## Testing with cURL

### Example cURL for Doctor Creation:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/doctor" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "dr.test",
    "email": "test@hospital.com",
    "password": "SecurePass123!",
    "firstName": "Test",
    "lastName": "Doctor",
    "specialty": "General Medicine",
    "hospitalId": "HOSP001"
  }'
```

### Example cURL for Engineer Creation:

```bash
curl -X POST "http://localhost:5117/api/RoleSpecificUser/engineer" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "eng.test",
    "email": "test@company.com",
    "password": "SecurePass123!",
    "firstName": "Test",
    "lastName": "Engineer",
    "specialty": "Mechanical Engineering",
    "governorateIds": [1, 2]
  }'
```

---

## Error Handling

### Common Error Responses:

**400 Bad Request - Missing Required Fields:**

```json
{
	"error": "Role field is required.",
	"timestamp": "2025-09-07T16:30:00Z"
}
```

**400 Bad Request - Invalid Hospital:**

```json
{
	"error": "Hospital with ID HOSP999 not found",
	"timestamp": "2025-09-07T16:30:00Z"
}
```

**400 Bad Request - Invalid Governorates:**

```json
{
	"error": "Governorates with IDs [99, 100] not found",
	"timestamp": "2025-09-07T16:30:00Z"
}
```

**401 Unauthorized:**

```json
{
	"error": "Unauthorized access",
	"timestamp": "2025-09-07T16:30:00Z"
}
```

---

## Notes

- All passwords should meet security requirements
- Department IDs are optional - will auto-assign based on role if not provided
- governorateIds for engineers must be valid Egypt governorate IDs (1-27)
- hospitalId must exist in the hospitals table
- SuperAdmin role can only be created by existing SuperAdmin users
- All other roles can be created by SuperAdmin or Admin users


