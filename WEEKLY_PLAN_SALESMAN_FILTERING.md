# Weekly Plan Filtering for Salesmen

## Overview

Salesmen can now filter their own weekly plans by date range, just like SalesManager and SuperAdmin. This allows salesmen to retrieve specific weekly plans for their records.

## Endpoint

### Get Weekly Plans (with Date Filters for Salesmen)

**Endpoint:** `GET /api/WeeklyPlan`

**Authorization:** All authenticated users

---

## Query Parameters

| Parameter       | Type          | Required | Default | Description                                                 |
| --------------- | ------------- | -------- | ------- | ----------------------------------------------------------- |
| `page`          | number        | No       | 1       | Page number for pagination                                  |
| `pageSize`      | number        | No       | 20      | Number of items per page                                    |
| `weekStartDate` | string (date) | No       | -       | Filter plans where week start date >= this date             |
| `weekEndDate`   | string (date) | No       | -       | Filter plans where week end date <= this date               |

**For SalesManager/SuperAdmin Only:**
- `employeeId` - Filter by specific salesman's ID
- `isViewed` - Filter by viewed status (true = viewed, false = not viewed)

---

## Salesmen Filtering Rules

### What Salesmen Can Do

✅ **Filter by date range** - Get plans within specific dates  
✅ **See only their own plans** - Automatically restricted  
✅ **Use pagination** - Works with all date filters  

### What Salesmen Cannot Do

❌ **Filter by employee ID** - Salesmen can only see their own plans  
❌ **Filter by viewed status** - This is a manager-only feature  
❌ **See other salesmen's plans** - Even if they provide an employee ID  

---

## Usage Examples

### Example 1: Get All My Plans

**Salesman Request:**
```http
GET /api/WeeklyPlan?page=1&pageSize=20
Authorization: Bearer SALESMAN_TOKEN
```

**Response:**
```json
{
  "success": true,
  "data": {
    "plans": [
      {
        "id": 41,
        "employeeId": "Ahmed_Ashraf_Sales_001",
        "weekStartDate": "2025-11-03T00:00:00",
        "weekEndDate": "2025-11-09T00:00:00",
        "title": "Week 45 Sales Plan",
        "description": "My weekly plan",
        "isActive": true,
        "createdAt": "2025-10-26T12:45:22.72",
        "updatedAt": "2025-10-26T12:45:22.72",
        "tasks": [...]
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 10,
      "totalPages": 1
    }
  }
}
```

---

### Example 2: Get Plans for November 2025

**Salesman Request:**
```http
GET /api/WeeklyPlan?weekStartDate=2025-11-01&weekEndDate=2025-11-30&page=1&pageSize=20
Authorization: Bearer SALESMAN_TOKEN
```

**Response:**
Returns only plans from November 2025 for this salesman.

---

### Example 3: Get Plans After Specific Date

**Salesman Request:**
```http
GET /api/WeeklyPlan?weekStartDate=2025-11-15&page=1&pageSize=20
Authorization: Bearer SALESMAN_TOKEN
```

**Response:**
Returns all plans where week start date is November 15, 2025 or later.

---

### Example 4: Attempt to Filter by Employee ID (Will Fail)

**Salesman Request:**
```http
GET /api/WeeklyPlan?employeeId=Other_Salesman_ID&page=1&pageSize=20
Authorization: Bearer SALESMAN_TOKEN
```

**Response:**
```json
{
  "success": false,
  "message": "Salesmen can only filter by date range",
  "timestamp": "2025-10-27T10:00:00Z"
}
```

**Status Code:** 403 Forbidden

---

### Example 5: Attempt to Filter by Viewed Status (Will Fail)

**Salesman Request:**
```http
GET /api/WeeklyPlan?isViewed=false&page=1&pageSize=20
Authorization: Bearer SALESMAN_TOKEN
```

**Response:**
```json
{
  "success": false,
  "message": "Salesmen can only filter by date range",
  "timestamp": "2025-10-27T10:00:00Z"
}
```

**Status Code:** 403 Forbidden

---

## Frontend Implementation Example

### TypeScript Service Method

```typescript
class WeeklyPlanService {
  constructor(private token: string, private userRole: string) {}

  /**
   * Get weekly plans with date filters
   * Salesmen can only filter by date, not by employee or viewed status
   */
  async getMyPlansWithDateFilter(
    startDate?: string,
    endDate?: string,
    page: number = 1,
    pageSize: number = 20
  ) {
    const params = new URLSearchParams();
    
    // Add pagination
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    
    // Add date filters
    if (startDate) params.append('weekStartDate', startDate);
    if (endDate) params.append('weekEndDate', endDate);

    const url = `${API_BASE_URL}?${params.toString()}`;
    
    const response = await fetch(url, {
      headers: {
        Authorization: `Bearer ${this.token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      if (response.status === 403) {
        throw new Error('You can only filter by date range');
      }
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
  }
}
```

---

### React Component Example

```tsx
const MyWeeklyPlans: React.FC = ({ token }) => {
  const [plans, setPlans] = useState([]);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);

  const service = new WeeklyPlanService(token, 'Salesman');

  const fetchPlans = async () => {
    setLoading(true);
    try {
      const result = await service.getMyPlansWithDateFilter(startDate, endDate, page, 20);
      setPlans(result.data.plans);
    } catch (error) {
      console.error('Failed to fetch plans:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPlans();
  }, [startDate, endDate, page]);

  return (
    <div>
      <h2>My Weekly Plans</h2>

      {/* Date Filters */}
      <div className="filters">
        <label>Start Date:</label>
        <input
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
        />

        <label>End Date:</label>
        <input
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
        />

        <button onClick={() => { setStartDate(''); setEndDate(''); setPage(1); }}>
          Clear Filters
        </button>
      </div>

      {/* Plans List */}
      {loading ? (
        <div>Loading...</div>
      ) : (
        <div>
          {plans.map((plan) => (
            <div key={plan.id}>
              <h3>{plan.title}</h3>
              <p>Week: {new Date(plan.weekStartDate).toLocaleDateString()} - {new Date(plan.weekEndDate).toLocaleDateString()}</p>
              <p>Tasks: {plan.tasks.length}</p>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      <button onClick={() => setPage(page - 1)} disabled={page === 1}>
        Previous
      </button>
      <span>Page {page}</span>
      <button onClick={() => setPage(page + 1)}>
        Next
      </button>
    </div>
  );
};
```

---

## Common Use Cases for Salesmen

### 1. View Current Month's Plans

```typescript
const getCurrentMonthPlans = async () => {
  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth() + 1;
  const startDate = `${year}-${String(month).padStart(2, '0')}-01`;
  const endDate = `${year}-${String(month).padStart(2, '0')}-${new Date(year, month, 0).getDate()}`;
  
  return service.getMyPlansWithDateFilter(startDate, endDate);
};
```

### 2. View Plans for Specific Quarter

```typescript
const getQuarterPlans = async (year: number, quarter: number) => {
  const startMonth = (quarter - 1) * 3 + 1;
  const endMonth = quarter * 3;
  const startDate = `${year}-${String(startMonth).padStart(2, '0')}-01`;
  const endDate = `${year}-${String(endMonth).padStart(2, '0')}-${new Date(year, endMonth, 0).getDate()}`;
  
  return service.getMyPlansWithDateFilter(startDate, endDate);
};
```

### 3. View Recent Plans (Last 30 Days)

```typescript
const getRecentPlans = async () => {
  const thirtyDaysAgo = new Date();
  thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
  const startDate = thirtyDaysAgo.toISOString().split('T')[0];
  
  return service.getMyPlansWithDateFilter(startDate);
};
```

---

## API Response Format

### Success Response

```json
{
  "success": true,
  "data": {
    "plans": [
      {
        "id": 41,
        "employeeId": "Ahmed_Ashraf_Sales_001",
        "weekStartDate": "2025-11-03T00:00:00",
        "weekEndDate": "2025-11-09T00:00:00",
        "title": "Week 45 Sales Plan",
        "description": "My weekly activities",
        "isActive": false,
        "rating": 4,
        "managerComment": "Good work!",
        "managerReviewedAt": "2025-10-26T12:41:56Z",
        "createdAt": "2025-10-26T12:45:22.72",
        "updatedAt": "2025-10-26T12:45:22.72",
        "tasks": [...]
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalCount": 5,
      "totalPages": 1
    }
  },
  "message": "Operation completed successfully",
  "timestamp": "2025-10-27T10:00:00Z"
}
```

---

## Error Responses

### 403 Forbidden - Attempted to Filter by Employee/Viewed Status

```json
{
  "success": false,
  "message": "Salesmen can only filter by date range",
  "timestamp": "2025-10-27T10:00:00Z"
}
```

### 403 Forbidden - No Permission for Filters

```json
{
  "success": false,
  "message": "You do not have permission to use filters",
  "timestamp": "2025-10-27T10:00:00Z"
}
```

---

## Summary

**Salesmen can now:**
- ✅ Filter their own weekly plans by date range
- ✅ View plans for specific months or date ranges
- ✅ Use pagination with date filters
- ✅ Access all the same data fields as before

**Salesmen restrictions:**
- ❌ Can only see their own plans (automatic restriction)
- ❌ Cannot filter by employee ID
- ❌ Cannot filter by viewed status
- ❌ Cannot access other salesmen's plans

This enhancement allows salesmen to efficiently retrieve and review their weekly plans while maintaining proper data isolation and access control.

