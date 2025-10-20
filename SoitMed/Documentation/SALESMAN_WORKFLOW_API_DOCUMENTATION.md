# Salesman Workflow API Documentation

## Overview

This documentation covers the enhanced salesman workflow APIs that include sending offers to sales support and deals to legal managers with delivery and payment terms.

## New Features

- **Sales Support Role**: New role for handling offer requests
- **Delivery Terms**: Comprehensive delivery information and conditions
- **Payment Terms**: Detailed payment methods and conditions
- **Real-time Notifications**: SignalR integration for instant updates
- **Mobile Push Notifications**: Background notifications for mobile apps

## API Endpoints

### 1. Create Request Workflow (Offer/Deal)

**Endpoint:** `POST /api/RequestWorkflows`

**Authorization:** Salesman role required

**Description:** Creates a new request workflow for either an offer (sent to Sales Support) or deal (sent to Legal Manager) with client details, equipment information, and terms.

**Request Body (Offer Request):**

```json
{
	"activityLogId": 123,
	"offerId": 456,
	"toUserId": "sales-support-user-id",
	"comment": "Client requires installation support and training",
	"deliveryTermsId": 1,
	"paymentTermsId": 1
}
```

**Request Body (Deal Request):**

```json
{
	"activityLogId": 123,
	"dealId": 789,
	"toUserId": "legal-manager-user-id",
	"comment": "Client requires extended warranty and maintenance contract",
	"deliveryTermsId": 2,
	"paymentTermsId": 2
}
```

**Note:** The system automatically determines whether to send to Sales Support or Legal Manager based on whether `offerId` or `dealId` is provided. Delivery and payment terms are created separately and referenced by ID.

**Response:**

```json
{
	"success": true,
	"message": "Request workflow created successfully",
	"data": {
		"id": 1,
		"activityLogId": 123,
		"offerId": 456,
		"dealId": null,
		"fromUserId": "salesman-user-id",
		"fromUserName": "John Doe",
		"toUserId": "sales-support-user-id",
		"toUserName": "Jane Smith",
		"status": "Pending",
		"statusName": "Pending",
		"comment": "Client requires installation support and training",
		"createdAt": "2025-10-15T10:30:00Z",
		"updatedAt": "2025-10-15T10:30:00Z"
	},
	"errors": null,
	"timestamp": "2025-10-15T10:30:00Z"
}
```

### 2. Get My Requests

**Endpoint:** `GET /api/RequestWorkflows/sent`

**Authorization:** Salesman role required

**Description:** Retrieves all requests sent by the current salesman.

**Query Parameters:**

- `status` (optional): Filter by request status (Pending, Assigned, InProgress, Completed, Rejected, Cancelled)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20)

**Response:**

```json
{
	"success": true,
	"message": "Requests retrieved successfully",
	"data": [
		{
			"id": 1,
			"activityLogId": 123,
			"offerId": 456,
			"dealId": null,
			"requestType": "Offer",
			"fromRole": "Salesman",
			"toRole": "SalesSupport",
			"fromUserId": "user123",
			"toUserId": "support456",
			"status": "Assigned",
			"statusName": "Assigned",
			"comments": "Client requires installation support",
			"clientName": "Cairo General Hospital",
			"clientAddress": "123 Tahrir Square, Cairo, Egypt",
			"equipmentDetails": "MRI Machine - Model XYZ",
			"deliveryTerms": {
				"id": 1,
				"deliveryMethod": "Express",
				"deliveryAddress": "123 Tahrir Square, Cairo, Egypt",
				"city": "Cairo",
				"state": "Cairo",
				"postalCode": "11511",
				"country": "Egypt",
				"estimatedDeliveryDays": 7,
				"specialInstructions": "Deliver to main entrance",
				"isUrgent": false,
				"preferredDeliveryDate": "2025-11-01T10:00:00Z",
				"contactPerson": "Dr. Ahmed Hassan",
				"contactPhone": "+201234567890",
				"contactEmail": "ahmed.hassan@hospital.com",
				"createdAt": "2025-10-15T10:30:00Z",
				"updatedAt": "2025-10-15T10:30:00Z"
			},
			"paymentTerms": {
				"id": 1,
				"paymentMethod": "Bank Transfer",
				"totalAmount": 1500000.0,
				"downPayment": 300000.0,
				"installmentCount": 12,
				"installmentAmount": 100000.0,
				"paymentDueDays": 30,
				"bankName": "National Bank of Egypt",
				"accountNumber": "1234567890",
				"IBAN": "EG12345678901234567890123456",
				"swiftCode": "NBELEGCX",
				"paymentInstructions": "Payment within 30 days of delivery",
				"requiresAdvancePayment": true,
				"advancePaymentPercentage": 20.0,
				"currency": "EGP",
				"createdAt": "2025-10-15T10:30:00Z",
				"updatedAt": "2025-10-15T10:30:00Z"
			},
			"createdAt": "2025-10-15T10:30:00Z",
			"updatedAt": "2025-10-15T10:30:00Z",
			"assignedAt": "2025-10-15T11:00:00Z",
			"completedAt": null
		}
	],
	"errors": null,
	"timestamp": "2025-10-15T10:30:00Z"
}
```

## Real-time Notifications

### SignalR Connection

**Hub URL:** `wss://your-api-url.com/notificationHub`

**Authentication:** Include JWT token in connection headers

**Events:**

- `ReceiveNotification`: New notification received
- `NewRequest`: New request created (for managers)
- `RequestAssigned`: Request assigned to user
- `RequestStatusUpdated`: Request status changed

### Notification Types

1. **Request**: New offer/deal request created
2. **Assignment**: Request assigned to user
3. **Update**: Request status updated
4. **Reminder**: Request reminder notification

### Mobile Push Notifications

The system supports background push notifications for mobile apps. When a request is sent or status is updated, push notifications are automatically sent to relevant users.

## Error Handling

### Common Error Responses

**400 Bad Request:**

```json
{
	"success": false,
	"message": "Validation failed",
	"errors": {
		"clientName": ["Client name is required"],
		"deliveryTerms.deliveryAddress": [
			"Delivery address is required"
		]
	},
	"timestamp": "2025-10-15T10:30:00Z"
}
```

**401 Unauthorized:**

```json
{
	"success": false,
	"message": "Unauthorized access",
	"errors": null,
	"timestamp": "2025-10-15T10:30:00Z"
}
```

**403 Forbidden:**

```json
{
	"success": false,
	"message": "Insufficient permissions",
	"errors": null,
	"timestamp": "2025-10-15T10:30:00Z"
}
```

**500 Internal Server Error:**

```json
{
	"success": false,
	"message": "An error occurred while processing the request",
	"errors": null,
	"timestamp": "2025-10-15T10:30:00Z"
}
```

## Frontend Integration Examples

### React Native Example

```javascript
// Send offer request
const sendOfferRequest = async (requestData) => {
	try {
		const response = await fetch(
			'https://your-api-url.com/api/RequestWorkflows',
			{
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${authToken}`,
				},
				body: JSON.stringify(requestData),
			}
		);

		const result = await response.json();

		if (result.success) {
			Alert.alert('Success', 'Request sent successfully');
		} else {
			Alert.alert('Error', result.message);
		}
	} catch (error) {
		Alert.alert('Error', 'Failed to send request');
	}
};

// Connect to SignalR
const connectToSignalR = async () => {
	const connection = new HubConnectionBuilder()
		.withUrl('https://your-api-url.com/notificationHub', {
			accessTokenFactory: () => authToken,
		})
		.build();

	connection.on('ReceiveNotification', (notification) => {
		// Handle notification
		showNotification(notification);
	});

	await connection.start();
};
```

### React Web Example

```javascript
// Send request (offer or deal)
const sendRequest = async (requestData) => {
	try {
		const response = await fetch('/api/RequestWorkflows', {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${authToken}`,
			},
			body: JSON.stringify(requestData),
		});

		const result = await response.json();
		return result;
	} catch (error) {
		console.error('Error sending request:', error);
		throw error;
	}
};
```

### 3. Create Delivery Terms

**Endpoint:** `POST /api/DeliveryTerms`

**Authorization:** Salesman role required

**Description:** Creates delivery terms for a request.

**Request Body:**

```json
{
	"deliveryMethod": "Express",
	"deliveryAddress": "123 Tahrir Square, Cairo, Egypt",
	"city": "Cairo",
	"state": "Cairo",
	"postalCode": "11511",
	"country": "Egypt",
	"estimatedDeliveryDays": 7,
	"specialInstructions": "Deliver to main entrance, call contact person 30 minutes before arrival",
	"isUrgent": false,
	"preferredDeliveryDate": "2025-11-01T10:00:00Z",
	"contactPerson": "Dr. Ahmed Hassan",
	"contactPhone": "+201234567890",
	"contactEmail": "ahmed.hassan@hospital.com"
}
```

### 4. Create Payment Terms

**Endpoint:** `POST /api/PaymentTerms`

**Authorization:** Salesman role required

**Description:** Creates payment terms for a request.

**Request Body:**

```json
{
	"paymentMethod": "Bank Transfer",
	"totalAmount": 1500000.0,
	"downPayment": 300000.0,
	"installmentCount": 12,
	"installmentAmount": 100000.0,
	"paymentDueDays": 30,
	"bankName": "National Bank of Egypt",
	"accountNumber": "1234567890",
	"IBAN": "EG12345678901234567890123456",
	"swiftCode": "NBELEGCX",
	"paymentInstructions": "Payment to be made within 30 days of delivery"
}
```

## Validation Rules

### Delivery Terms Validation

- `deliveryMethod`: Required, max 200 characters
- `deliveryAddress`: Required, max 500 characters
- `city`: Optional, max 100 characters
- `state`: Optional, max 100 characters
- `postalCode`: Optional, max 20 characters
- `country`: Optional, max 100 characters
- `estimatedDeliveryDays`: Optional, must be positive integer
- `specialInstructions`: Optional, max 1000 characters
- `contactPerson`: Optional, max 200 characters
- `contactPhone`: Optional, max 20 characters
- `contactEmail`: Optional, max 100 characters, must be valid email format

### Payment Terms Validation

- `paymentMethod`: Required, max 100 characters
- `totalAmount`: Required, must be greater than 0
- `downPayment`: Optional, must be positive
- `installmentCount`: Optional, must be positive integer
- `installmentAmount`: Optional, must be positive
- `paymentDueDays`: Optional, must be positive integer
- `bankName`: Optional, max 200 characters
- `accountNumber`: Optional, max 50 characters
- `IBAN`: Optional, max 50 characters
- `swiftCode`: Optional, max 100 characters
- `paymentInstructions`: Optional, max 1000 characters
- `advancePaymentPercentage`: Optional, must be between 0 and 100
- `currency`: Optional, max 200 characters, defaults to "EGP"

## Best Practices

1. **Always validate data** on the frontend before sending requests
2. **Handle errors gracefully** and provide user-friendly messages
3. **Implement retry logic** for network failures
4. **Use SignalR for real-time updates** instead of polling
5. **Store authentication tokens securely** and refresh them when needed
6. **Implement proper loading states** for better user experience
7. **Test notification delivery** on both foreground and background states

## Security Considerations

1. **Authentication**: All endpoints require valid JWT token
2. **Authorization**: Role-based access control enforced
3. **Data Validation**: Server-side validation for all inputs
4. **HTTPS**: All communications must use HTTPS
5. **Token Security**: Store tokens securely and implement proper refresh logic

## Support

For technical support or questions about the API, please contact the development team or refer to the main API documentation.
