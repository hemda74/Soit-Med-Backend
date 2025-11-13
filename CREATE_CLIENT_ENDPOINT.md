# Create Client Endpoint

## Endpoint

```
POST /api/Client
```

## Authorization

**Required:** Salesman, SalesManager, or SuperAdmin role

**Header:**

```
Authorization: Bearer {your_jwt_token}
Content-Type: application/json
```

---

## Request Body

### ⚠️ Important Note

**Only `name` is required. All other fields, including `type`, are optional.**

### Required Fields

| Field  | Type   | Max Length | Description                          |
| ------ | ------ | ---------- | ------------------------------------ |
| `name` | string | 200        | Client name (e.g., "Ahmed Hospital") |

### Optional Fields

| Field                   | Type    | Max Length | Description                                                             |
| ----------------------- | ------- | ---------- | ----------------------------------------------------------------------- |
| `type`                  | string  | 50         | Client type (e.g., "Hospital", "Clinic", "Pharmacy")                    |
| `specialization`        | string  | 100        | Medical specialization                                                  |
| `location`              | string  | 100        | City or location                                                        |
| `phone`                 | string  | 20         | Phone number                                                            |
| `email`                 | string  | -          | Email address                                                           |
| `website`               | string  | -          | Website URL                                                             |
| `address`               | string  | 500        | Street address                                                          |
| `city`                  | string  | 100        | City name                                                               |
| `governorate`           | string  | 100        | Governorate/State                                                       |
| `postalCode`            | string  | 20         | Postal/ZIP code                                                         |
| `notes`                 | string  | 2000       | Additional notes                                                        |
| `status`                | string  | -          | `"Potential"`, `"Active"`, `"Inactive"`, `"Lost"` (default: `"Active"`) |
| `priority`              | string  | -          | `"Low"`, `"Medium"`, `"High"` (default: `"Medium"`)                     |
| `classification`        | string  | 10         | Client classification                                                   |
| `rating`                | integer | -          | Client rating (1-5)                                                     |
| `potentialValue`        | decimal | -          | Estimated potential value                                               |
| `contactPerson`         | string  | 200        | Contact person name                                                     |
| `contactPersonPhone`    | string  | 20         | Contact person phone                                                    |
| `contactPersonEmail`    | string  | 100        | Contact person email                                                    |
| `contactPersonPosition` | string  | 100        | Contact person position                                                 |
| `assignedTo`            | string  | -          | User ID to assign client to (auto-assigned to creator if not provided)  |

---

## Request Examples

### Example 1: Minimal Request (Only Name Required)

```http
POST /api/Client
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

```json
{
	"name": "New Hospital"
}
```

**Note:** This is the absolute minimum - only `name` is required. All other fields are optional.

---

### Example 2: Complete Request (All Fields)

```http
POST /api/Client
Authorization: Bearer {your_token}
Content-Type: application/json
```

```json
{
	"name": "Cairo Medical Center",
	"type": "Hospital",
	"specialization": "Cardiology",
	"location": "Cairo",
	"phone": "+20123456789",
	"email": "contact@cairomedical.com",
	"website": "https://cairomedical.com",
	"address": "123 Main Street, Nasr City",
	"city": "Cairo",
	"governorate": "Cairo",
	"postalCode": "12345",
	"notes": "High priority client, interested in X-Ray equipment",
	"status": "Potential",
	"priority": "High",
	"potentialValue": 1000000,
	"contactPerson": "Dr. Ahmed Mohamed",
	"contactPersonPhone": "+20198887766",
	"contactPersonEmail": "ahmed@cairomedical.com",
	"contactPersonPosition": "Medical Director"
}
```

---

### Example 3: Hospital with Contact Person

```json
{
	"name": "Alexandria General Hospital",
	"type": "Hospital",
	"specialization": "General Medicine",
	"location": "Alexandria",
	"phone": "+20312345678",
	"email": "info@alexhospital.com",
	"address": "456 Corniche Street",
	"city": "Alexandria",
	"governorate": "Alexandria",
	"status": "Active",
	"priority": "High",
	"contactPerson": "Dr. Mohamed Ali",
	"contactPersonPhone": "+20197776655",
	"contactPersonEmail": "mohamed@alexhospital.com",
	"contactPersonPosition": "Procurement Manager"
}
```

---

### Example 4: Clinic

```json
{
	"name": "Al-Salam Clinic",
	"type": "Clinic",
	"specialization": "Pediatrics",
	"location": "Giza",
	"phone": "+20212345678",
	"email": "info@alsalamclinic.com",
	"address": "789 Pyramid Street",
	"city": "Giza",
	"governorate": "Giza",
	"status": "Potential",
	"priority": "Medium",
	"contactPerson": "Dr. Fatima Hassan",
	"contactPersonPhone": "+20196665544"
}
```

---

## Success Response (201 Created)

```json
{
	"success": true,
	"data": {
		"id": 150,
		"name": "Cairo Medical Center",
		"type": "Hospital",
		"specialization": "Cardiology",
		"location": "Cairo",
		"phone": "+20123456789",
		"email": "contact@cairomedical.com",
		"website": "https://cairomedical.com",
		"address": "123 Main Street, Nasr City",
		"city": "Cairo",
		"governorate": "Cairo",
		"postalCode": "12345",
		"status": "Potential",
		"priority": "High",
		"assignedTo": "Ahmed_Ashraf_Sales_001",
		"contactPerson": "Dr. Ahmed Mohamed",
		"contactPersonPhone": "+20198887766",
		"contactPersonEmail": "ahmed@cairomedical.com",
		"contactPersonPosition": "Medical Director",
		"potentialValue": 1000000,
		"createdAt": "2025-11-02T13:00:00Z",
		"updatedAt": "2025-11-02T13:00:00Z"
	},
	"message": null,
	"timestamp": "2025-11-02T13:00:00Z"
}
```

**Note:** The client will be automatically assigned to you (`assignedTo` field contains your user ID).

---

## Error Responses

### 400 Bad Request - Validation Errors

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"name": ["اسم العميل مطلوب"],
		"type": ["نوع العميل مطلوب"]
	},
	"timestamp": "2025-11-02T13:00:00Z"
}
```

### 401 Unauthorized - Not Authenticated

```json
{
	"success": false,
	"message": "غير مصرح لك",
	"timestamp": "2025-11-02T13:00:00Z"
}
```

### 500 Internal Server Error

```json
{
	"success": false,
	"message": "حدث خطأ في إنشاء العميل",
	"timestamp": "2025-11-02T13:00:00Z"
}
```

---

## cURL Examples

### Minimal Request

```bash
curl -X POST "http://localhost:5117/api/Client" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Hospital",
    "type": "Hospital"
  }'
```

### Complete Request

```bash
curl -X POST "http://localhost:5117/api/Client" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Cairo Medical Center",
    "type": "Hospital",
    "specialization": "Cardiology",
    "location": "Cairo",
    "phone": "+20123456789",
    "email": "contact@cairomedical.com",
    "status": "Potential",
    "priority": "High",
    "contactPerson": "Dr. Ahmed",
    "contactPersonPhone": "+20198887766"
  }'
```

---

## JavaScript/Fetch Example

```javascript
const createClient = async (token, clientData) => {
	const response = await fetch('http://localhost:5117/api/Client', {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'Content-Type': 'application/json',
		},
		body: JSON.stringify(clientData),
	});

	if (!response.ok) {
		const error = await response.json();
		throw new Error(error.message || 'Failed to create client');
	}

	return await response.json();
};

// Usage
const clientData = {
	name: 'New Hospital',
	type: 'Hospital',
	location: 'Cairo',
	phone: '+20123456789',
	priority: 'High',
};

const result = await createClient(yourToken, clientData);
console.log('Client created:', result.data);
```

---

## Python Example

```python
import requests

url = "http://localhost:5117/api/Client"
headers = {
    "Authorization": "Bearer YOUR_TOKEN",
    "Content-Type": "application/json"
}

client_data = {
    "name": "New Hospital",
    "type": "Hospital",
    "location": "Cairo",
    "phone": "+20123456789",
    "priority": "High"
}

response = requests.post(url, json=client_data, headers=headers)

if response.status_code == 201:
    result = response.json()
    print(f"Client created: {result['data']['name']} (ID: {result['data']['id']})")
else:
    print(f"Error: {response.json()}")
```

---

## Important Notes

1. ✅ **Auto-Assignment:** New clients are automatically assigned to you
2. ✅ **Status Default:** If not provided, status defaults to `"Potential"`
3. ✅ **Priority Default:** If not provided, priority defaults to `"Medium"`
4. ⚠️ **Required Fields:** **Only `name` is required.** All other fields, including `type`, are optional
5. ✅ **Email Validation:** Email format is validated if provided
6. ✅ **Phone Validation:** Phone format is validated if provided
7. ✅ **Flexible Creation:** You can create a client with just a name and add details later via update

---

## Client Type Values

Common client types:

- `"Hospital"`
- `"Clinic"`
- `"Pharmacy"`
- `"Lab"`
- `"Medical Center"`
- `"Diagnostic Center"`

## Status Values

- `"Potential"` - Potential/new client (default)
- `"Active"` - Active client
- `"Inactive"` - Inactive client
- `"Lost"` - Lost client

## Priority Values

- `"Low"` - Low priority
- `"Medium"` - Medium priority (default)
- `"High"` - High priority

---

## Related Endpoints

After creating a client, you can:

- `GET /api/Client/{id}` - Get the created client details
- `PUT /api/Client/{id}` - Update the client
- `POST /api/WeeklyPlanTask` - Create a task for this client
- `POST /api/TaskProgress` - Record a visit for this client
