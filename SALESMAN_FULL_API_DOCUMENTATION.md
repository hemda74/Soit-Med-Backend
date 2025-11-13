## Salesman Full API Documentation

Auth

- All endpoints require: `Authorization: Bearer {token}`
- Standard success:

```json
{ "success": true, "data": { ... }, "message": "..." }
```

- Standard error:

```json
{ "success": false, "message": "...", "errors": { ... } }
```

---

### Weekly Plans & Tasks

List my weekly plans

- GET `/api/WeeklyPlan?page=1&pageSize=10&weekStartDate=YYYY-MM-DDT00:00:00&weekEndDate=YYYY-MM-DDT23:59:59`
- Do NOT send `employeeId` or `isViewed` as Salesman

Response (truncated)

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 115,
				"weekStartDate": "2025-03-03T00:00:00",
				"weekEndDate": "2025-03-09T00:00:00",
				"title": "Week 1 - March 2025",
				"description": "Deal closures",
				"tasks": [
					{
						"id": 135,
						"taskType": "Visit",
						"clientId": 161,
						"clientName": "Heliopolis Healthcare",
						"plannedDate": "2025-03-04T00:00:00",
						"priority": "High",
						"status": "Pending",
						"progressCount": 0
					}
				]
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 10,
			"totalCount": 5,
			"totalPages": 1
		}
	},
	"message": "Operation completed successfully"
}
```

Get weekly plan (with tasks)

- GET `/api/WeeklyPlan/{id}`

Get current week plan

- GET `/api/WeeklyPlan/current`

---

### Task Progress

Create task progress

- POST `/api/TaskProgress`

```json
{
	"taskId": 135,
	"progressDate": "2025-03-04T10:30:00Z",
	"progressType": "Visit",
	"description": "Client meeting completed",
	"visitResult": "Interested",
	"nextStep": "SendOffer"
}
```

Create progress + offer request

- POST `/api/TaskProgress/with-offer-request`

```json
{
	"taskId": 135,
	"progressDate": "2025-03-04T10:30:00Z",
	"progressType": "Visit",
	"description": "Interested; needs formal quote",
	"clientId": 161,
	"requestedProducts": "X-Ray Machine, Ultrasound",
	"specialNotes": "Urgent delivery"
}
```

List my progress

- GET `/api/TaskProgress`

By task

- GET `/api/TaskProgress/task/{taskId}`

By client

- GET `/api/TaskProgress/by-client/{clientId}`

Update progress

- PUT `/api/TaskProgress/{id}`

```json
{
	"description": "Adjusted next steps",
	"visitResult": "Interested",
	"nextStep": "ScheduleDemo"
}
```

---

### Offer Requests

Create offer request

- POST `/api/OfferRequest`

```json
{
	"clientId": 161,
	"taskProgressId": 200,
	"requestedProducts": "X-Ray Machine, Ultrasound",
	"specialNotes": "Urgent delivery"
}
```

My offer requests

- GET `/api/OfferRequest/my-requests`

Get offer request

- GET `/api/OfferRequest/{id}`

Notes

- New requests auto-assign to SalesSupport if available; status becomes `Assigned`. Otherwise status `Requested`.

---

### Offers (Salesman consumption)

Offers assigned to me

- GET `/api/Offer/assigned-to-me`

Offer detail (includes equipment)

- GET `/api/Offer/{id}`

Export offer PDF

- GET `/api/Offer/{offerId}/export-pdf`

Offer object (key fields)

```json
{
	"id": 45,
	"clientId": 161,
	"clientName": "Heliopolis Healthcare",
	"assignedTo": "Ahmed_Ashraf_Sales_001",
	"products": "X-Ray Machine, Ultrasound",
	"totalAmount": 370000.0,
	"paymentType": "Installments",
	"finalPrice": 370000.0,
	"validUntil": "2025-12-31T00:00:00Z",
	"status": "Draft",
	"equipment": [
		{
			"id": 1,
			"name": "X-Ray Machine",
			"model": "XR-200",
			"provider": "MedTech Co.",
			"country": "Germany",
			"year": 2023,
			"price": 250000.0,
			"imagePath": "https://cdn.example.com/xray.jpg"
		}
	]
}
```

---

### Deals (Salesman)

Create deal

- POST `/api/Deal`

```json
{
	"offerId": 45,
	"clientId": 161,
	"dealValue": 370000.0,
	"paymentTerms": "30% upfront, 70% on delivery",
	"deliveryTerms": "4-6 weeks",
	"notes": "Client accepted",
	"expectedDeliveryDate": "2026-01-15T00:00:00Z"
}
```

List my deals

- GET `/api/Deal`

Deal detail

- GET `/api/Deal/{id}`

Deals by client

- GET `/api/Deal/by-client/{clientId}`

---

### Common Errors

- 400 Bad Request: validation failure (missing required fields)
- 401 Unauthorized: missing/invalid token
- 403 Forbidden: role not allowed (e.g., Salesman calling manager-only endpoint)
- 404 Not Found: resource not found
- 500 Server Error: unexpected backend error

### Tips

- Always check `result.success` before using `result.data`.
- For Salesman, avoid manager-only params (`employeeId`, `isViewed`).
- Use embedded `tasks` in weekly plan for dashboard rendering.
- Use `with-offer-request` to log and spawn an offer request in one call.
