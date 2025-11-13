# Task and Task Progress Endpoint Calls

Complete, ready-to-use endpoint call examples for creating tasks and task progress.

---

## 📋 1. Create Task (WeeklyPlanTask)

### Endpoint

```
POST /api/WeeklyPlanTask
```

### Headers

```
Authorization: Bearer {your_token}
Content-Type: application/json
```

### Example 1: Create Task for Existing Client (Old)

```http
POST /api/WeeklyPlanTask
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

```json
{
	"weeklyPlanId": 116,
	"title": "Visit Ahmed Hospital",
	"taskType": "Visit",
	"clientId": 25,
	"clientStatus": "Old",
	"plannedDate": "2025-01-15T10:00:00Z",
	"plannedTime": "10:00 AM",
	"purpose": "Discuss new equipment requirements",
	"notes": "Focus on X-Ray machines",
	"priority": "High",
	"status": "Planned",
	"clientClassification": "A"
}
```

### Example 2: Create Task for New Client

```json
{
	"weeklyPlanId": 116,
	"title": "Visit New Hospital",
	"taskType": "Visit",
	"clientStatus": "New",
	"clientName": "New Hospital Name",
	"placeName": "Main Building",
	"placeType": "Hospital",
	"clientPhone": "+20 1234567890",
	"clientAddress": "123 Main Street",
	"clientLocation": "Cairo",
	"plannedDate": "2025-01-16T14:00:00Z",
	"plannedTime": "2:00 PM",
	"purpose": "Initial visit to introduce our services",
	"notes": "New potential client",
	"priority": "Medium",
	"status": "Planned",
	"clientClassification": "B"
}
```

### Example 3: Minimal Task (Only Required Fields)

```json
{
	"weeklyPlanId": 116,
	"title": "Follow up call",
	"taskType": "FollowUp",
	"clientStatus": "Old",
	"clientId": 25,
	"priority": "Medium",
	"status": "Planned"
}
```

### Required Fields

- `weeklyPlanId` (long) - The weekly plan ID
- `title` (string, max 500) - Task title
- `taskType` (string) - Must be "Visit" or "FollowUp"
- If `clientStatus = "Old"`: `clientId` is required
- If `clientStatus = "New"`: `clientName` is required

### Optional Fields

- `plannedDate` (DateTime)
- `plannedTime` (string, max 20)
- `purpose` (string, max 500)
- `notes` (string, max 1000)
- `priority` (string) - "High", "Medium", "Low" (default: "Medium")
- `status` (string) - "Planned", "InProgress", "Completed", "Cancelled" (default: "Planned")
- `clientClassification` (string) - "A", "B", "C", "D"
- For new clients: `placeName`, `placeType`, `clientPhone`, `clientAddress`, `clientLocation`

### Success Response (201 Created)

```json
{
	"success": true,
	"data": {
		"id": 200,
		"weeklyPlanId": 116,
		"title": "Visit Ahmed Hospital",
		"taskType": "Visit",
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"clientStatus": "Old",
		"plannedDate": "2025-01-15T10:00:00Z",
		"plannedTime": "10:00 AM",
		"purpose": "Discuss new equipment requirements",
		"notes": "Focus on X-Ray machines",
		"priority": "High",
		"status": "Planned",
		"progressCount": 0,
		"createdAt": "2025-01-13T08:00:00Z",
		"updatedAt": "2025-01-13T08:00:00Z",
		"progresses": [],
		"offerRequests": [],
		"offers": [],
		"deals": []
	},
	"message": "Task created successfully"
}
```

---

## 📊 2. Create Task Progress

### Endpoint

```
POST /api/TaskProgress
```

### Headers

```
Authorization: Bearer {your_token}
Content-Type: application/json
```

### Example 1: Basic Visit Progress

```json
{
	"taskId": 200,
	"progressDate": "2025-01-15T10:30:00Z",
	"progressType": "Visit",
	"description": "Met with procurement manager",
	"notes": "Client showed interest in X-Ray equipment",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"satisfactionRating": 4
}
```

### Example 2: Complete Progress with Follow-up

```json
{
	"taskId": 200,
	"progressDate": "2025-01-15T10:30:00Z",
	"progressType": "Visit",
	"description": "Initial visit to discuss equipment needs",
	"notes": "Client has budget approved for Q1 2025",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"nextFollowUpDate": "2025-01-22T10:00:00Z",
	"followUpNotes": "Send formal quote by next week",
	"satisfactionRating": 5,
	"feedback": "Very positive meeting, client is ready to move forward"
}
```

### Example 3: Not Interested Progress

```json
{
	"taskId": 200,
	"progressDate": "2025-01-15T10:30:00Z",
	"progressType": "Visit",
	"description": "Met with client",
	"visitResult": "NotInterested",
	"notInterestedComment": "Budget constraints, will revisit next quarter"
}
```

### Example 4: Call Progress

```json
{
	"taskId": 200,
	"progressDate": "2025-01-16T14:00:00Z",
	"progressType": "Call",
	"description": "Follow-up call to check on offer status",
	"notes": "Client received offer, reviewing internally",
	"visitResult": "Interested",
	"nextStep": "NeedsDeal",
	"nextFollowUpDate": "2025-01-20T10:00:00Z",
	"followUpNotes": "Call back Monday for decision"
}
```

### Required Fields

- `taskId` (long) - The task ID
- `progressDate` (DateTime) - Date and time of the progress
- `progressType` (string) - Must be "Visit", "Call", "Meeting", or "Email"

### Optional Fields

- `description` (string, max 2000)
- `notes` (string, max 2000)
- `visitResult` (string) - "Interested" or "NotInterested"
- `notInterestedComment` (string, max 2000) - Required if `visitResult = "NotInterested"`
- `nextStep` (string) - "NeedsDeal" or "NeedsOffer"
- `nextFollowUpDate` (DateTime)
- `followUpNotes` (string, max 1000)
- `satisfactionRating` (int) - 1 to 5
- `feedback` (string, max 2000)

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"id": 500,
		"taskId": 200,
		"clientId": 25,
		"clientName": "Ahmed Hospital",
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"employeeName": "Ahmed Ashraf",
		"progressDate": "2025-01-15T10:30:00Z",
		"progressType": "Visit",
		"description": "Met with procurement manager",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"offerRequestId": null,
		"offerId": null,
		"dealId": null,
		"nextFollowUpDate": "2025-01-22T10:00:00Z",
		"satisfactionRating": 4,
		"createdAt": "2025-01-15T10:35:00Z"
	},
	"message": "Task progress created successfully"
}
```

---

## 🎯 3. Create Task Progress with Offer Request

### Endpoint

```
POST /api/TaskProgress/with-offer-request
```

### Headers

```
Authorization: Bearer {your_token}
Content-Type: application/json
```

### Description

Creates task progress AND automatically creates an offer request when the client is interested. This is useful when you want to record a visit and immediately request an offer in one call.

### Example: Visit Progress with Offer Request

```json
{
	"taskId": 200,
	"progressDate": "2025-01-15T10:30:00Z",
	"progressType": "Visit",
	"description": "Client meeting completed, client is interested",
	"visitResult": "Interested",
	"nextStep": "NeedsOffer",
	"clientId": 25,
	"requestedProducts": "X-Ray Machine Model XYZ-2000, Ultrasound System",
	"specialNotes": "Client needs fast delivery - urgent requirement"
}
```

### Required Fields

- `taskId` (long)
- `progressDate` (DateTime)
- `progressType` (string)
- `clientId` (long) - **Required for offer request**
- `requestedProducts` (string, max 2000) - **Required for offer request**

### Optional Fields

- `description` (string, max 2000)
- `notes` (string, max 2000)
- `visitResult` (string)
- `nextStep` (string)
- `specialNotes` (string, max 2000) - Additional notes for the offer request

### Success Response (200 OK)

```json
{
	"success": true,
	"data": {
		"progressId": 500,
		"offerRequestId": 25,
		"message": "Progress and offer request created"
	},
	"message": "Task progress created and offer request triggered successfully"
}
```

---

## 📝 Field Reference

### Task Type Values

- `Visit` - Physical visit to client location
- `FollowUp` - Follow-up on previous interaction

### Priority Values

- `High`
- `Medium` (default)
- `Low`

### Status Values

- `Planned` (default)
- `InProgress`
- `Completed`
- `Cancelled`

### Progress Type Values

- `Visit`
- `Call`
- `Meeting`
- `Email`

### Visit Result Values

- `Interested`
- `NotInterested`

### Next Step Values

- `NeedsOffer` - Client needs an offer/quote
- `NeedsDeal` - Client is ready for deal/contract

### Client Status Values

- `Old` - Existing client (requires `clientId`)
- `New` - New client (requires `clientName`)

### Client Classification Values

- `A` - Highest priority
- `B` - High priority
- `C` - Medium priority
- `D` - Low priority

---

## 🔧 cURL Examples

### Create Task (Existing Client)

```bash
curl -X POST "http://localhost:5117/api/WeeklyPlanTask" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "weeklyPlanId": 116,
    "title": "Visit Ahmed Hospital",
    "taskType": "Visit",
    "clientId": 25,
    "clientStatus": "Old",
    "plannedDate": "2025-01-15T10:00:00Z",
    "priority": "High",
    "status": "Planned"
  }'
```

### Create Task Progress

```bash
curl -X POST "http://localhost:5117/api/TaskProgress" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "taskId": 200,
    "progressDate": "2025-01-15T10:30:00Z",
    "progressType": "Visit",
    "description": "Met with procurement manager",
    "visitResult": "Interested",
    "nextStep": "NeedsOffer"
  }'
```

### Create Progress with Offer Request

```bash
curl -X POST "http://localhost:5117/api/TaskProgress/with-offer-request" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "taskId": 200,
    "progressDate": "2025-01-15T10:30:00Z",
    "progressType": "Visit",
    "description": "Client interested in equipment",
    "clientId": 25,
    "requestedProducts": "X-Ray Machine Model XYZ-2000",
    "specialNotes": "Urgent delivery required"
  }'
```

---

## ⚠️ Important Notes

1. **Client Validation:**

      - When `clientStatus = "Old"`: `clientId` must exist in the database
      - When `clientStatus = "New"`: `clientName` is required, `clientId` is not validated

2. **Task Ownership:**

      - Salesmen can only create tasks in their own weekly plans
      - Salesmen can only create progress for their own tasks

3. **Task Progress:**

      - One task can have multiple progress records
      - Progress records track the history of interactions

4. **Offer Request:**
      - The endpoint `/with-offer-request` requires `clientId` (cannot be used with new clients)
      - Offer request will be automatically created and assigned to SalesSupport

---

## 🔗 Related Endpoints

- `GET /api/WeeklyPlanTask/{id}` - Get task details
- `GET /api/WeeklyPlanTask/by-plan/{weeklyPlanId}` - Get all tasks for a plan
- `PUT /api/WeeklyPlanTask/{id}` - Update task
- `DELETE /api/WeeklyPlanTask/{id}` - Delete task
- `GET /api/TaskProgress/task/{taskId}` - Get all progress for a task
- `GET /api/TaskProgress/by-client/{clientId}` - Get client visit history
- `PUT /api/TaskProgress/{id}` - Update progress


