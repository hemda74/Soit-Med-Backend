# POST /api/WeeklyPlan - Complete Example Guide

## Endpoint Information

**URL:** `POST /api/WeeklyPlan`

**Authorization:** `Bearer {token}` (All authenticated users - Salesmen can create their own plans)

**Content-Type:** `application/json`

---

## Request Examples

### Example 1: Basic Weekly Plan (Minimum Required Fields)

```http
POST /api/WeeklyPlan
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
	"weekStartDate": "2025-11-04T00:00:00Z",
	"weekEndDate": "2025-11-10T23:59:59Z",
	"title": "Week 45 - November 2025",
	"description": null
}
```

### Example 2: Complete Weekly Plan (All Fields)

```json
{
	"weekStartDate": "2025-11-04T00:00:00Z",
	"weekEndDate": "2025-11-10T23:59:59Z",
	"title": "Week 45 - November 2025 Sales Plan",
	"description": "Focus on following up with hospitals in Cairo and preparing quotes for interested clients. Priority: Complete pending offer requests."
}
```

### Example 3: Week with Detailed Description

```json
{
	"weekStartDate": "2025-11-11T00:00:00Z",
	"weekEndDate": "2025-11-17T23:59:59Z",
	"title": "Week 46 - November 2025",
	"description": "Monday: Visit Cairo Medical Center - discuss X-ray equipment requirements. Tuesday: Follow up on pending offers. Wednesday: Client presentation for ultrasound systems. Thursday: Contract negotiations. Friday: Weekly review and planning."
}
```

### Example 4: Short Title

```json
{
	"weekStartDate": "2025-12-01T00:00:00Z",
	"weekEndDate": "2025-12-07T23:59:59Z",
	"title": "December Week 1",
	"description": "Year-end push for deal closures"
}
```

### Example 5: Create Plan with Tasks (NEW FEATURE)

```json
{
	"weekStartDate": "2025-12-01T00:00:00Z",
	"weekEndDate": "2025-12-07T23:59:59Z",
	"title": "Week 49 - December 2025",
	"description": "End of year planning with multiple client visits",
	"tasks": [
		{
			"title": "Visit Client Hospital A",
			"taskType": "Visit",
			"clientStatus": "Old",
			"clientId": 25,
			"plannedDate": "2025-12-02T10:00:00Z",
			"priority": "High",
			"status": "Planned"
		},
		{
			"title": "Visit New Hospital B",
			"taskType": "Visit",
			"clientStatus": "New",
			"clientName": "New Hospital Name",
			"placeType": "Hospital",
			"plannedDate": "2025-12-03T14:00:00Z",
			"priority": "Medium",
			"status": "Planned"
		},
		{
			"title": "Follow up on pending offers",
			"taskType": "FollowUp",
			"clientStatus": "Old",
			"clientId": 30,
			"plannedDate": "2025-12-04T11:00:00Z",
			"priority": "High",
			"status": "Planned",
			"purpose": "Check status of offer sent last week"
		}
	]
}
```

**Note:** When creating tasks during plan creation, the `weeklyPlanId` in each task will be automatically set to the created plan's ID. You don't need to provide it in the task DTOs.

---

## Success Response (201 Created)

### Response Structure

```json
{
	"success": true,
	"data": {
		"id": 117,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employee": null,
		"weekStartDate": "2025-11-04T00:00:00Z",
		"weekEndDate": "2025-11-10T23:59:59Z",
		"title": "Week 45 - November 2025 Sales Plan",
		"description": "Focus on following up with hospitals in Cairo and preparing quotes for interested clients.",
		"isActive": true,
		"rating": null,
		"managerComment": null,
		"managerReviewedAt": null,
		"managerViewedAt": null,
		"viewedBy": null,
		"isViewed": false,
		"createdAt": "2025-11-02T10:20:00Z",
		"updatedAt": "2025-11-02T10:20:00Z",
		"tasks": [
			{
				"id": 200,
				"title": "Visit Client Hospital A",
				"taskType": "Visit",
				"clientName": "New Hospital Name",
				"plannedDate": "2025-12-02T10:00:00Z",
				"priority": "High",
				"status": "Planned",
				"progressCount": 0
			}
		]
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-11-02T10:20:00Z"
}
```

### Response Fields Explained

| Field               | Type          | Description                                                                      |
| ------------------- | ------------- | -------------------------------------------------------------------------------- |
| `id`                | number        | Unique identifier for the weekly plan                                            |
| `employeeId`        | string        | ID of the employee who created the plan (automatically set to current user)      |
| `employee`          | object/null   | Employee details (null for Salesman, populated for Managers)                     |
| `weekStartDate`     | datetime      | Start date of the week (ISO 8601 format)                                         |
| `weekEndDate`       | datetime      | End date of the week (ISO 8601 format)                                           |
| `title`             | string        | Title of the weekly plan (max 200 characters)                                    |
| `description`       | string/null   | Detailed description (max 1000 characters, optional)                             |
| `isActive`          | boolean       | Whether the plan is active (always `true` for new plans)                         |
| `rating`            | number/null   | Manager rating (1-5, null until reviewed)                                        |
| `managerComment`    | string/null   | Manager's review comment (null until reviewed)                                   |
| `managerReviewedAt` | datetime/null | When manager reviewed the plan (null until reviewed)                             |
| `managerViewedAt`   | datetime/null | When manager viewed the plan (null until viewed)                                 |
| `viewedBy`          | string/null   | ID of manager who viewed (null until viewed)                                     |
| `isViewed`          | boolean       | Whether manager has viewed the plan                                              |
| `createdAt`         | datetime      | When the plan was created                                                        |
| `updatedAt`         | datetime      | When the plan was last updated                                                   |
| `tasks`             | array         | List of tasks in the plan (populated if tasks were created during plan creation) |

---

## Error Responses

### 400 Bad Request - Plan Already Exists for This Week

**Scenario:** Trying to create a plan for a week when one already exists

```json
{
	"success": false,
	"message": "يوجد خطة أسبوعية بالفعل لهذا الأسبوع",
	"errors": null,
	"timestamp": "2025-11-02T10:20:00Z"
}
```

**English Translation:** "A weekly plan already exists for this week"

**Solution:**

- Check existing plans: `GET /api/WeeklyPlan`
- Update existing plan: `PUT /api/WeeklyPlan/{id}`
- Or create plan for a different week

---

### 400 Bad Request - Validation Errors

#### Missing Required Fields

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"WeekStartDate": ["WeekStartDate is required"],
		"WeekEndDate": ["WeekEndDate is required"],
		"Title": ["Title is required"]
	},
	"timestamp": "2025-11-02T10:20:00Z"
}
```

#### Title Too Long

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"Title": ["Title cannot exceed 200 characters"]
	},
	"timestamp": "2025-11-02T10:20:00Z"
}
```

#### Description Too Long

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"Description": ["Description cannot exceed 1000 characters"]
	},
	"timestamp": "2025-11-02T10:20:00Z"
}
```

#### Invalid Date Format

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"WeekStartDate": [
			"The WeekStartDate field must be a valid date"
		],
		"WeekEndDate": ["The WeekEndDate field must be a valid date"]
	},
	"timestamp": "2025-11-02T10:20:00Z"
}
```

#### Multiple Validation Errors

```json
{
	"success": false,
	"message": "Validation failed. Please check the following fields:",
	"errors": {
		"WeekStartDate": ["WeekStartDate is required"],
		"Title": [
			"Title is required",
			"Title cannot exceed 200 characters"
		],
		"Description": ["Description cannot exceed 1000 characters"]
	},
	"timestamp": "2025-11-02T10:20:00Z"
}
```

---

### 401 Unauthorized - Missing or Invalid Token

**Scenario:** Request without token or with expired/invalid token

```json
{
	"success": false,
	"message": "Unauthorized access",
	"errors": null,
	"timestamp": "2025-11-02T10:20:00Z"
}
```

**Solution:**

- Ensure `Authorization: Bearer {token}` header is included
- Verify token is valid and not expired
- Login again to get a new token

---

### 500 Internal Server Error

**Scenario:** Server-side error (database issue, unexpected exception)

```json
{
	"success": false,
	"message": "An error occurred while creating the weekly plan",
	"errors": null,
	"timestamp": "2025-11-02T10:20:00Z"
}
```

**Solution:**

- Retry the request
- Contact support if issue persists
- Check server logs for details

---

## Field Requirements and Constraints

### weekStartDate (Required)

- **Type:** `datetime` (ISO 8601 format)
- **Required:** Yes
- **Example:** `"2025-11-04T00:00:00Z"`
- **Format:** `YYYY-MM-DDTHH:mm:ssZ`
- **Note:** Should be the start of the week (typically Monday)

### weekEndDate (Required)

- **Type:** `datetime` (ISO 8601 format)
- **Required:** Yes
- **Example:** `"2025-11-10T23:59:59Z"`
- **Format:** `YYYY-MM-DDTHH:mm:ssZ`
- **Note:** Should be the end of the week (typically Sunday)

### title (Required)

- **Type:** `string`
- **Required:** Yes
- **Max Length:** 200 characters
- **Examples:**
     - `"Week 45 - November 2025"`
     - `"Week 3 - January 2025 Sales Plan"`
     - `"December Week 1"`

### description (Optional)

- **Type:** `string` or `null`
- **Required:** No
- **Max Length:** 1000 characters
- **Can be:** `null` or empty string
- **Examples:**
     - `null`
     - `"Focus on hospital visits"`
     - Detailed multi-line description

---

## Business Rules

1. **One Plan Per Week:** Each employee can only have one weekly plan per week (based on `weekStartDate`)
2. **Automatic Assignment:** The plan is automatically assigned to the logged-in user (cannot create plans for others)
3. **Active Status:** New plans are automatically set to `isActive: true`
4. **Empty Tasks:** New plans start with an empty `tasks` array
5. **No Manager Fields:** Manager-related fields (`rating`, `managerComment`, etc.) are `null` until reviewed

## ⚠️ Important: Tasks Cannot Be Created During Plan Creation

**Note:** The `POST /api/WeeklyPlan` endpoint does **NOT** support creating tasks when creating the plan. Tasks must be added separately after the plan is created.

**Current Limitation:**

- The `CreateWeeklyPlanDTO` only includes: `weekStartDate`, `weekEndDate`, `title`, and `description`
- There is no `tasks` field in the request body
- New plans are created with an empty `tasks` array

**Workflow:**

1. Create the weekly plan using `POST /api/WeeklyPlan`
2. Get the plan ID from the response
3. Add tasks separately (if a task creation endpoint exists)
4. View the plan with tasks using `GET /api/WeeklyPlan/{id}`

**Note:** Currently, there doesn't appear to be a dedicated endpoint to add tasks to a weekly plan. This functionality may need to be implemented separately.

---

## Complete cURL Example

```bash
curl -X POST "http://localhost:5117/api/WeeklyPlan" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "weekStartDate": "2025-11-04T00:00:00Z",
    "weekEndDate": "2025-11-10T23:59:59Z",
    "title": "Week 45 - November 2025 Sales Plan",
    "description": "Focus on following up with hospitals and preparing quotes"
  }'
```

---

## Complete PowerShell Example

```powershell
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}
$body = @{
    weekStartDate = "2025-11-04T00:00:00Z"
    weekEndDate = "2025-11-10T23:59:59Z"
    title = "Week 45 - November 2025 Sales Plan"
    description = "Focus on following up with hospitals and preparing quotes"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5117/api/WeeklyPlan" `
    -Method Post -Headers $headers -Body $body

$response | ConvertTo-Json -Depth 10
```

---

## Complete JavaScript/Fetch Example

```javascript
const token = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...';
const url = 'http://localhost:5117/api/WeeklyPlan';

const requestBody = {
	weekStartDate: '2025-11-04T00:00:00Z',
	weekEndDate: '2025-11-10T23:59:59Z',
	title: 'Week 45 - November 2025 Sales Plan',
	description:
		'Focus on following up with hospitals and preparing quotes',
};

fetch(url, {
	method: 'POST',
	headers: {
		Authorization: `Bearer ${token}`,
		'Content-Type': 'application/json',
	},
	body: JSON.stringify(requestBody),
})
	.then((response) => response.json())
	.then((data) => {
		console.log('Success:', data);
		if (data.success) {
			console.log('Plan created with ID:', data.data.id);
		} else {
			console.error('Error:', data.message);
		}
	})
	.catch((error) => {
		console.error('Request failed:', error);
	});
```

---

## Tips and Best Practices

1. **Week Dates:** Use consistent week boundaries (e.g., Monday to Sunday)
2. **Title Format:** Use a consistent naming convention like `"Week {number} - {Month} {Year}"`
3. **Description:** Be specific and actionable - helps with planning and review
4. **Check Existing Plans:** Before creating, check if a plan already exists for that week
5. **Time Zones:** All dates should be in UTC (Z suffix) or include timezone offset
6. **Error Handling:** Always check the `success` field in the response before proceeding

---

## Common Issues and Solutions

### Issue: "Plan already exists for this week"

**Solution:**

- Get existing plans: `GET /api/WeeklyPlan?weekStartDate={date}`
- Update existing plan instead of creating new one
- Or choose a different week

### Issue: "Validation failed"

**Solution:**

- Check that all required fields are present
- Verify field lengths (title ≤ 200, description ≤ 1000)
- Ensure dates are in valid ISO 8601 format

### Issue: "Unauthorized access"

**Solution:**

- Verify token is included in Authorization header
- Check token is not expired
- Ensure user is authenticated

---

## Related Endpoints

- **GET /api/WeeklyPlan** - View all your weekly plans
- **GET /api/WeeklyPlan/{id}** - View specific weekly plan details
- **PUT /api/WeeklyPlan/{id}** - Update an existing weekly plan
- **POST /api/WeeklyPlan/{id}/submit** - Submit plan for manager review
- **GET /api/WeeklyPlan/current** - Get current week's plan
