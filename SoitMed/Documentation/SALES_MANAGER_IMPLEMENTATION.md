# 🎯 Sales Manager Role Implementation

## **Overview**

The Sales Manager role has been successfully added to the SoitMed Backend system. This role provides management capabilities within the Sales department and follows the same patterns as other manager roles in the system.

---

## **🔧 Implementation Details**

### **1. Role Definition**

- **Role Name**: `SalesManager`
- **Department**: Sales
- **Type**: Manager Role
- **Permissions**: Can register other users (Admin capability)

### **2. Files Created/Modified**

#### **New Files:**

- `DTO/CreateSalesManagerWithImageDTO.cs` - DTOs for Sales Manager creation
- `test-sales-manager.ps1` - Test script for Sales Manager functionality
- `SALES_MANAGER_IMPLEMENTATION.md` - This documentation

#### **Modified Files:**

- `Models/Core/UserRoles.cs` - Added SalesManager role constant and updated role lists
- `Controllers/RoleSpecificUserController.cs` - Added Sales Manager creation endpoint

---

## **🚀 API Endpoint**

### **Create Sales Manager**

```http
POST /api/RoleSpecificUser/sales-manager
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

#### **Request Body (Form Data):**

```json
{
	"Email": "sales.manager@soitmed.com",
	"Password": "SecurePassword123!",
	"FirstName": "John",
	"LastName": "Smith",
	"DepartmentId": 3, // Optional - will auto-assign to Sales department
	"AltText": "Profile image description", // Optional
	"SalesTerritory": "North Region", // Optional
	"SalesTeam": "Team Alpha", // Optional
	"SalesTarget": 100000.0 // Optional
}
```

#### **Response:**

```json
{
	"userId": "SM-JOHN-SMITH-001",
	"email": "sales.manager@soitmed.com",
	"role": "SalesManager",
	"departmentName": "Sales",
	"createdAt": "2025-01-21T10:30:00Z",
	"profileImage": {
		"id": 1,
		"fileName": "profile.jpg",
		"filePath": "/uploads/sales-manager/profile.jpg",
		"contentType": "image/jpeg",
		"fileSize": 1024000,
		"altText": "Profile image description",
		"isProfileImage": true,
		"uploadedAt": "2025-01-21T10:30:00Z"
	},
	"message": "Sales Manager 'sales.manager@soitmed.com' created successfully with profile image",
	"salesTerritory": "North Region",
	"salesTeam": "Team Alpha",
	"salesTarget": 100000.0
}
```

---

## **👥 Role Hierarchy**

### **Sales Department Structure:**

```
Sales Department
├── SalesManager (Manager Role)
│   ├── Can create/manage Salesman accounts
│   ├── Can view sales reports
│   ├── Can manage sales territories
│   └── Has admin privileges within Sales department
└── Salesman (Employee Role)
    ├── Basic sales operations
    └── Reports to Sales Manager
```

### **Manager Capabilities:**

- ✅ Create and manage Salesman accounts
- ✅ View and manage sales reports
- ✅ Access to sales analytics
- ✅ Territory and team management
- ✅ User registration permissions

---

## **🔐 Authorization**

### **Who Can Create Sales Managers:**

- `SuperAdmin` - Full system access
- `Admin` - Administrative access

### **Sales Manager Permissions:**

- Can create `Salesman` accounts
- Can view sales-related data
- Can manage sales territories and teams
- Has department-level admin privileges

---

## **📊 Database Integration**

### **Migration Applied:**

- **Migration**: `20250921210759_AddSalesManagerRole`
- **Status**: Applied successfully
- **Changes**: Role added to Identity system (no schema changes needed)

### **Role Storage:**

- Sales Manager role is stored in ASP.NET Identity `AspNetRoles` table
- User assignments stored in `AspNetUserRoles` table
- No additional database tables required

---

## **🧪 Testing**

### **Test the Implementation:**

```powershell
# Run the test script
.\test-sales-manager.ps1
```

### **Manual Testing:**

1. **Start the application:**

      ```bash
      dotnet run --urls "http://localhost:5117"
      ```

2. **Access Swagger UI:**

      - Navigate to: `http://localhost:5117/swagger`
      - Find the `RoleSpecificUser` section
      - Look for `POST /api/RoleSpecificUser/sales-manager`

3. **Test with Postman/curl:**
      ```bash
      curl -X POST "http://localhost:5117/api/RoleSpecificUser/sales-manager" \
           -H "Authorization: Bearer YOUR_TOKEN" \
           -F "Email=sales.manager@test.com" \
           -F "Password=Test123!" \
           -F "FirstName=Test" \
           -F "LastName=Manager" \
           -F "SalesTerritory=Test Region"
      ```

---

## **📋 Usage Examples**

### **1. Create a Sales Manager via Swagger:**

1. Open `http://localhost:5117/swagger`
2. Authorize with a SuperAdmin or Admin token
3. Find `POST /api/RoleSpecificUser/sales-manager`
4. Click "Try it out"
5. Fill in the required fields
6. Execute the request

### **2. Create a Sales Manager via Code:**

```csharp
var salesManagerDTO = new CreateSalesManagerWithImageDTO
{
    Email = "manager@company.com",
    Password = "SecurePass123!",
    FirstName = "Jane",
    LastName = "Doe",
    SalesTerritory = "West Coast",
    SalesTeam = "Team Bravo",
    SalesTarget = 150000.00m
};

// Call the endpoint with proper authorization
```

---

## **🔍 Validation Rules**

### **Required Fields:**

- `Email` - Must be valid email format
- `Password` - Minimum 6 characters

### **Optional Fields:**

- `FirstName` - Max 100 characters
- `LastName` - Max 100 characters
- `DepartmentId` - Will auto-assign to Sales department
- `AltText` - Max 500 characters
- `SalesTerritory` - Max 200 characters
- `SalesTeam` - Max 100 characters
- `SalesTarget` - Must be positive decimal

---

## **🎉 Success Indicators**

### **What You Should See:**

- ✅ Sales Manager role appears in role lists
- ✅ Sales department includes both SalesManager and Salesman
- ✅ Sales Manager is included in manager roles
- ✅ Sales Manager has admin privileges
- ✅ API endpoint responds successfully
- ✅ User can be created with SalesManager role
- ✅ Profile image upload works (if provided)

### **Verification Steps:**

1. Check `UserRoles.GetAllRoles()` includes "SalesManager"
2. Check `UserRoles.GetRolesByDepartment()["Sales"]` includes "SalesManager"
3. Check `UserRoles.GetManagerRoles()` includes "SalesManager"
4. Check `UserRoles.GetAdminRoles()` includes "SalesManager"
5. Test the API endpoint with valid data

---

## **🚀 Next Steps**

### **Immediate Actions:**

1. Test the implementation using the provided test script
2. Create your first Sales Manager account
3. Verify the role works as expected

### **Future Enhancements:**

- Add Sales Manager specific dashboard
- Implement sales territory management
- Add sales team hierarchy features
- Create sales performance tracking

---

## **📞 Support**

If you encounter any issues:

1. Check the application logs
2. Verify the database connection
3. Ensure proper authorization tokens
4. Run the test script for diagnostics

---

**🎯 Sales Manager role implementation is complete and ready for use!**

