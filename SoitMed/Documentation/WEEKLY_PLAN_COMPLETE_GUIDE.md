# Weekly Plan - Complete Guide for Frontend Teams

**Version**: 1.0  
**Date**: 2025-10-26  
**For**: React & React Native Development Teams  
**Base URL**: `http://localhost:5117/api`

---

## Overview

The Weekly Plan module is the foundation of the sales workflow. Sales employees create weekly plans with tasks, execute them, and submit for manager review. This generates offers and deals in the sales pipeline.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Database Schema](#database-schema)
3. [API Endpoints](#api-endpoints)
4. [Integration with Sales Module](#integration-with-sales-module)
5. [Frontend Implementation](#frontend-implementation)
6. [Sample Data](#sample-data)
7. [Testing Guide](#testing-guide)

---

## Architecture Overview

### Workflow

```
[Weekly Plan Created]
    ↓
[Tasks Added to Plan]
    ↓
[Task Progress Recorded]
    ↓
[Offer Request Created]
    ↓
[Sales Offer Generated]
    ↓
[Deal Closed]
```

### User Roles

- **Salesman** (ahmed@soitmed.com): Creates plans, executes tasks, submits plans
- **SalesSupport** (salessupport@soitmed.com): Creates offers from requests
- **SalesManager** (salesmanager@soitmed.com): Reviews plans, approves deals

---

## Database Schema

### WeeklyPlans Table

| Column              | Type           | Description                      |
| ------------------- | -------------- | -------------------------------- |
| `Id`                | bigint         | Auto-generated plan ID           |
| `EmployeeId`        | nvarchar(450)  | Sales employee who created plan  |
| `WeekStartDate`     | date           | Week start date                  |
| `WeekEndDate`       | date           | Week end date                    |
| `Title`             | nvarchar(200)  | Plan title                       |
| `Description`       | nvarchar(1000) | Plan description                 |
| `IsActive`          | bit            | Whether plan is currently active |
| `Rating`            | int            | Manager rating (1-5)             |
| `ManagerComment`    | nvarchar(1000) | Manager review comment           |
| `ManagerReviewedAt` | datetime2      | Review timestamp                 |
| `ReviewedBy`        | nvarchar(450)  | Manager who reviewed             |
| `CreatedAt`         | datetime2      | Creation timestamp               |
| `UpdatedAt`         | datetime2      | Update timestamp                 |

### WeeklyPlanTasks Table

| Column         | Type          | Description                               |
| -------------- | ------------- | ----------------------------------------- |
| `Id`           | bigint        | Auto-generated task ID                    |
| `WeeklyPlanId` | bigint        | Parent weekly plan                        |
| `Title`        | nvarchar(500) | Task title                                |
| `TaskType`     | nvarchar(50)  | Visit, FollowUp, Call, Email, Meeting     |
| `ClientId`     | bigint        | Link to existing client                   |
| `ClientStatus` | nvarchar(20)  | "Old" or "New"                            |
| `ClientName`   | nvarchar(200) | For new clients                           |
| `PlannedDate`  | datetime2     | When task is planned                      |
| `PlannedTime`  | nvarchar(20)  | Time (e.g., "10:00")                      |
| `Purpose`      | nvarchar(500) | Task purpose                              |
| `Priority`     | nvarchar(50)  | High, Medium, Low                         |
| `Status`       | nvarchar(50)  | Planned, InProgress, Completed, Cancelled |

---

## API Endpoints

### 1. Get All Weekly Plans

```http
GET /api/weeklyplan?page=1&pageSize=20
Authorization: Bearer {token}
```

**Returns**:

```json
{
  "success": true,
  "data": {
    "plans": [
      {
        "id": 42,
        "title": "Week 44 Sales Plan",
        "weekStartDate": "2025-10-27T00:00:00",
        "weekEndDate": "2025-11-02T23:59:59",
        "isActive": true,
        "rating": 4,
        "tasks": [...]
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 5,
      "totalPages": 1
    }
  }
}
```

### 2. Get Current Weekly Plan

```http
GET /api/weeklyplan/current
Authorization: Bearer {token}
```

**Returns**: Currently active plan (IsActive = true)

### 3. Get Weekly Plan by ID

```http
GET /api/weeklyplan/42
Authorization: Bearer {token}
```

### 4. Create Weekly Plan

```http
POST /api/weeklyplan
Authorization: Bearer {token}
Content-Type: application/json

{
  "weekStartDate": "2025-10-27T00:00:00",
  "weekEndDate": "2025-11-02T23:59:59",
  "title": "Week 44 Sales Plan",
  "description": "Focus on client follow-ups"
}
```

### 5. Update Weekly Plan

```http
PUT /api/weeklyplan/42
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Updated Plan Title",
  "description": "Updated description"
}
```

### 6. Submit Weekly Plan

```http
POST /api/weeklyplan/42/submit
Authorization: Bearer {token}
```

### 7. Review Weekly Plan (Manager Only)

```http
POST /api/weeklyplan/42/review
Authorization: Bearer {token}
Content-Type: application/json

{
  "rating": 4,
  "comment": "Good work!"
}
```

---

## Integration with Sales Module

### Data Flow

```
Weekly Plan
    ├── Weekly Plan Task
    │       └── Task Progress
    │               └── Offer Request (if client interested)
    │                       └── Sales Offer (by SalesSupport)
    │                               └── Sales Deal (by Salesman)
    │                                       └── Manager Approval
    │                                               └── SuperAdmin Approval
```

### Relationships

1. **Weekly Plan** has many **Tasks**
2. **Task** has many **Task Progress** entries
3. **Task Progress** can create **Offer Request**
4. **Offer Request** generates **Sales Offer**
5. **Sales Offer** becomes **Sales Deal**

---

## Frontend Implementation

### React & React Native Components

#### 1. WeeklyPlanList Component

```typescript
interface WeeklyPlan {
	id: number;
	title: string;
	weekStartDate: string;
	weekEndDate: string;
	isActive: boolean;
	rating?: number;
	tasks: WeeklyPlanTask[];
}

interface WeeklyPlanTask {
	id: number;
	title: string;
	taskType: string;
	clientName?: string;
	status: string;
	priority: string;
	plannedDate?: string;
}

// Fetch weekly plans
const fetchWeeklyPlans = async (page = 1, pageSize = 20) => {
	const response = await fetch(
		`/api/weeklyplan?page=${page}&pageSize=${pageSize}`,
		{
			headers: {
				Authorization: `Bearer ${token}`,
			},
		}
	);
	return response.json();
};
```

#### 2. WeeklyPlanForm Component (Create/Edit)

```typescript
const WeeklyPlanForm = ({ onSubmit, initialData }) => {
  const [formData, setFormData] = useState({
    weekStartDate: '',
    weekEndDate: '',
    title: '',
    description: ''
  });

  const handleSubmit = async () => {
    const response = await fetch('/api/weeklyplan', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`
      },
      body: JSON.stringify(formData)
    });

    if (response.ok) {
      onSubmit(await response.json());
    }
  };

  return (
    <Form>
      <Input label="Title" value={formData.title} onChange={...} />
      <DatePicker label="Start Date" value={formData.weekStartDate} />
      <DatePicker label="End Date" value={formData.weekEndDate} />
      <TextArea label="Description" value={formData.description} />
      <Button onClick={handleSubmit}>Create Plan</Button>
    </Form>
  );
};
```

#### 3. TaskProgress Component

```typescript
const TaskProgressList = ({ tasks }) => {
	return (
		<List>
			{tasks.map((task) => (
				<TaskCard
					key={task.id}
					task={task}
				/>
			))}
		</List>
	);
};

const TaskCard = ({ task }) => {
	return (
		<Card>
			<Title>{task.title}</Title>
			<Badge status={task.status}>{task.status}</Badge>
			<Badge priority={task.priority}>{task.priority}</Badge>
			<Text>Client: {task.clientName || 'New Client'}</Text>
			<Text>Date: {task.plannedDate}</Text>
			<Button onClick={() => recordProgress(task.id)}>
				Record Progress
			</Button>
		</Card>
	);
};
```

---

## Sample Data

### Test Users

- **Salesman**: ahmed@soitmed.com (Password: 356120Ahmed@shraf2)
- **SalesManager**: salesmanager@soitmed.com (Password: 356120Ahmed@shraf2)
- **SalesSupport**: salessupport@soitmed.com (Password: 356120Ahmed@shraf2)

### Available Data

**Current Database Status:**

- 13 Weekly Plans (various states: Active, Submitted, Reviewed)
- Multiple Tasks per plan (40+ total tasks)
- Task Progress entries with client interactions
- 13 Offer Requests (various statuses)
- 7 Sales Offers (Draft, Sent, UnderReview, Accepted)
- 3 Sales Deals (Pending, Approved, Completed)

**Sample Plans:**

- Plan #41: Active plan for next week (3 tasks)
- Plan #34: Active plan with 2 tasks (1 completed, 1 pending)
- Plan #38: Current week plan (active)
- 4 Plans with manager ratings (2, 4, 5 stars)

---

## Testing Guide

### 1. Get All Plans

```bash
curl -X GET "http://localhost:5117/api/weeklyplan" \
  -H "Authorization: Bearer {token}"
```

### 2. Get Current Plan

```bash
curl -X GET "http://localhost:5117/api/weeklyplan/current" \
  -H "Authorization: Bearer {token}"
```

### 3. Create New Plan

```bash
curl -X POST "http://localhost:5117/api/weeklyplan" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "weekStartDate": "2025-11-03T00:00:00",
    "weekEndDate": "2025-11-09T23:59:59",
    "title": "Week 45 Sales Plan",
    "description": "Focus on deal closures"
  }'
```

---

## Complete Endpoint Summary

| Endpoint                      | Method | Auth    | Description               |
| ----------------------------- | ------ | ------- | ------------------------- |
| `/api/weeklyplan`             | GET    | All     | Get all plans (paginated) |
| `/api/weeklyplan`             | POST   | All     | Create new plan           |
| `/api/weeklyplan/{id}`        | GET    | All     | Get specific plan         |
| `/api/weeklyplan/{id}`        | PUT    | All     | Update plan               |
| `/api/weeklyplan/{id}/submit` | POST   | All     | Submit for review         |
| `/api/weeklyplan/{id}/review` | POST   | Manager | Review and rate           |
| `/api/weeklyplan/current`     | GET    | All     | Get current active plan   |

---

## Error Handling

### 401 Unauthorized

```json
{
	"success": false,
	"message": "Unauthorized access"
}
```

### 404 Not Found

```json
{
	"success": false,
	"message": "Weekly plan not found"
}
```

### 400 Bad Request

```json
{
	"success": false,
	"message": "Week end date must be after start date"
}
```

---

## Rate Limiting

- Global: 100 requests/minute
- API Endpoints: 200 requests/minute
- Auth Endpoints: 10 requests/minute

---

**End of Weekly Plan Complete Guide**
