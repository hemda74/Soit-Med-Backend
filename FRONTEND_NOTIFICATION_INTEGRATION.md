# Frontend Notification Integration Guide

## 🎯 Overview

When a **Salesman** creates a `TaskProgress` with an offer request, the system automatically sends notifications to all **SalesSupport** users. This guide explains how to implement the notification system correctly on the frontend.

---

## ⚠️ Critical Information

### How Notifications Are Triggered

1. **Salesman** creates TaskProgress with offer request:

      ```
      POST /api/TaskProgress/with-offer-request
      ```

2. **Backend automatically:**

      - Creates an `OfferRequest` in the database
      - Finds all active `SalesSupport` users
      - Saves notification to database for each SalesSupport user
      - Sends real-time notification via SignalR (if user is connected)

3. **SalesSupport users receive:**
      - Real-time notification via SignalR (if connected)
      - Notification saved in database (always, even if SignalR fails)

---

## 🔌 SignalR Connection Requirements

### Hub Endpoint

```
Hub URL: {baseUrl}/notificationHub
Example: http://localhost:5117/notificationHub
```

### Authentication

SignalR requires **JWT authentication**. The token must be included in the connection.

**Option 1: Authorization Header (Recommended)**

- Include `Authorization: Bearer {token}` in connection headers

**Option 2: Query String (Fallback)**

- Append `?access_token={token}` to hub URL (for WebSocket compatibility)

---

## 📡 SignalR Events to Listen For

### 1. `ReceiveNotification` (Personal Notifications)

**Triggered when:** A notification is sent directly to your user account

**Event Name:** `ReceiveNotification`

**Data Structure:**

```typescript
{
  id: number;                    // Notification ID
  title: string;                  // e.g., "New Offer Request"
  message: string;                // e.g., "Ahmed Mohamed requested an offer for client: Cairo Hospital"
  type: string;                   // e.g., "OfferRequest"
  priority: string;               // "Low" | "Medium" | "High"
  isRead: boolean;
  createdAt: string;              // ISO 8601 date string
  requestWorkflowId?: number | null;
  activityLogId?: number | null;
}
```

**Example (JavaScript/TypeScript):**

```javascript
connection.on('ReceiveNotification', function (notification) {
	console.log('New notification received:', notification);

	// Show notification to user
	showNotificationToast(notification);

	// Update notification count
	updateNotificationBadge();

	// Refresh notifications list
	loadNotifications();
});
```

### 2. `NewRequest` (Role-Based Broadcast)

**Triggered when:** A new request is broadcasted to your role group

**Event Name:** `NewRequest`

**Data Structure:**

```typescript
{
	requestId: number;
	requestType: string; // "Offer" | "Deal"
	clientName: string;
	clientAddress: string;
	equipmentDetails: string;
	createdAt: string; // ISO 8601 date string
}
```

**Example (JavaScript/TypeScript):**

```javascript
connection.on('NewRequest', function (request) {
	console.log('New request broadcast:', request);

	// Show alert for new request
	showRequestAlert(request);
});
```

---

## 💻 Complete Frontend Implementation

### React Example

```typescript
import {
	HubConnection,
	HubConnectionBuilder,
	LogLevel,
} from '@microsoft/signalr';
import { useEffect, useState } from 'react';

interface Notification {
	id: number;
	title: string;
	message: string;
	type: string;
	priority: string;
	isRead: boolean;
	createdAt: string;
	requestWorkflowId?: number | null;
	activityLogId?: number | null;
}

const NotificationService = () => {
	const [connection, setConnection] = useState<HubConnection | null>(
		null
	);
	const [notifications, setNotifications] = useState<Notification[]>([]);
	const [unreadCount, setUnreadCount] = useState(0);

	useEffect(() => {
		const token = localStorage.getItem('authToken'); // Your token storage
		const baseUrl = 'http://localhost:5117'; // Your API base URL

		// Create SignalR connection
		const newConnection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/notificationHub`, {
				accessTokenFactory: () => token || '',
				headers: {
					Authorization: `Bearer ${token}`,
				},
			})
			.withAutomaticReconnect() // Auto-reconnect on disconnect
			.configureLogging(LogLevel.Information)
			.build();

		// Set up event handlers
		newConnection.on(
			'ReceiveNotification',
			(notification: Notification) => {
				console.log(
					'📬 New notification:',
					notification
				);

				// Add to notifications list
				setNotifications((prev) => [
					notification,
					...prev,
				]);

				// Update unread count
				setUnreadCount((prev) => prev + 1);

				// Show toast/alert to user
				showNotificationToast(notification);
			}
		);

		newConnection.on('NewRequest', (request: any) => {
			console.log('🔔 New request broadcast:', request);
			// Handle role-based broadcast
		});

		// Handle connection events
		newConnection.onclose(() => {
			console.warn(
				'⚠️ SignalR connection closed. Will attempt to reconnect...'
			);
		});

		newConnection.onreconnecting(() => {
			console.log('🔄 SignalR reconnecting...');
		});

		newConnection.onreconnected(() => {
			console.log('✅ SignalR reconnected successfully');
			// Reload notifications after reconnect
			loadNotifications();
		});

		// Start connection
		newConnection
			.start()
			.then(() => {
				console.log('✅ Connected to SignalR hub');
				setConnection(newConnection);
				// Load existing notifications
				loadNotifications();
			})
			.catch((error) => {
				console.error(
					'❌ Error connecting to SignalR:',
					error
				);
				// Fallback: Use polling instead
				startPolling();
			});

		// Cleanup on unmount
		return () => {
			if (newConnection) {
				newConnection.stop();
			}
		};
	}, []);

	// Load notifications from API
	const loadNotifications = async () => {
		try {
			const token = localStorage.getItem('authToken');
			const response = await fetch(
				'http://localhost:5117/api/Notification',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			if (response.ok) {
				const data = await response.json();
				if (data.success && data.data) {
					setNotifications(data.data);
					setUnreadCount(
						data.data.filter(
							(n: Notification) =>
								!n.isRead
						).length
					);
				}
			}
		} catch (error) {
			console.error('Error loading notifications:', error);
		}
	};

	// Polling fallback (if SignalR fails)
	const [pollingInterval, setPollingInterval] =
		useState<NodeJS.Timeout | null>(null);

	const startPolling = () => {
		console.log('🔄 Starting notification polling (fallback mode)');
		const interval = setInterval(() => {
			loadNotifications();
		}, 10000); // Poll every 10 seconds

		setPollingInterval(interval);
	};

	useEffect(() => {
		if (connection && pollingInterval) {
			clearInterval(pollingInterval);
			setPollingInterval(null);
		}
	}, [connection]);

	// Show notification toast
	const showNotificationToast = (notification: Notification) => {
		// Use your toast/notification library (e.g., react-toastify, react-hot-toast)
		// Example:
		toast.info(notification.message, {
			title: notification.title,
			duration: 5000,
			onClick: () => {
				// Navigate to relevant page
				if (notification.type === 'OfferRequest') {
					// Navigate to offer request details
					router.push(
						`/offer-requests/${notification.requestWorkflowId}`
					);
				}
			},
		});
	};

	return { connection, notifications, unreadCount, loadNotifications };
};
```

---

## 🔄 Angular Example

```typescript
import { Injectable } from '@angular/core';
import {
	HubConnection,
	HubConnectionBuilder,
	LogLevel,
} from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
	providedIn: 'root',
})
export class NotificationService {
	private hubConnection: HubConnection | null = null;
	private notificationsSubject = new BehaviorSubject<any[]>([]);
	public notifications$ = this.notificationsSubject.asObservable();

	private unreadCountSubject = new BehaviorSubject<number>(0);
	public unreadCount$ = this.unreadCountSubject.asObservable();

	constructor(private authService: AuthService) {}

	startConnection(): void {
		const token = this.authService.getToken();
		const baseUrl = 'http://localhost:5117';

		this.hubConnection = new HubConnectionBuilder()
			.withUrl(`${baseUrl}/notificationHub`, {
				accessTokenFactory: () => token || '',
				headers: {
					Authorization: `Bearer ${token}`,
				},
			})
			.withAutomaticReconnect()
			.configureLogging(LogLevel.Information)
			.build();

		// Listen for notifications
		this.hubConnection.on('ReceiveNotification', (notification) => {
			console.log('📬 New notification:', notification);
			this.addNotification(notification);
			this.updateUnreadCount();
			this.showToast(notification);
		});

		this.hubConnection.on('NewRequest', (request) => {
			console.log('🔔 New request:', request);
			// Handle role-based broadcast
		});

		// Start connection
		this.hubConnection
			.start()
			.then(() => {
				console.log('✅ SignalR connected');
				this.loadNotifications();
			})
			.catch((error) => {
				console.error(
					'❌ SignalR connection failed:',
					error
				);
				this.startPolling();
			});
	}

	private addNotification(notification: any): void {
		const current = this.notificationsSubject.value;
		this.notificationsSubject.next([notification, ...current]);
	}

	private updateUnreadCount(): void {
		const unread = this.notificationsSubject.value.filter(
			(n) => !n.isRead
		).length;
		this.unreadCountSubject.next(unread);
	}

	private async loadNotifications(): Promise<void> {
		try {
			const token = this.authService.getToken();
			const response = await fetch(
				'http://localhost:5117/api/Notification',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			if (response.ok) {
				const data = await response.json();
				if (data.success) {
					this.notificationsSubject.next(
						data.data
					);
					this.updateUnreadCount();
				}
			}
		} catch (error) {
			console.error('Error loading notifications:', error);
		}
	}

	private startPolling(): void {
		setInterval(() => {
			this.loadNotifications();
		}, 10000); // Poll every 10 seconds
	}

	private showToast(notification: any): void {
		// Use your toast service (e.g., Angular Material Snackbar, ngx-toastr)
	}

	stopConnection(): void {
		if (this.hubConnection) {
			this.hubConnection.stop();
		}
	}
}
```

---

## 📋 API Endpoints (Fallback & Initial Load)

### 1. Get Notifications

```http
GET /api/Notification?page=1&pageSize=20&unreadOnly=false
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": [
		{
			"id": 123,
			"title": "New Offer Request",
			"message": "Ahmed Mohamed requested an offer for client: Cairo Hospital",
			"type": "OfferRequest",
			"priority": "High",
			"isRead": false,
			"createdAt": "2025-01-15T10:00:00Z",
			"requestWorkflowId": 45,
			"activityLogId": null
		}
	],
	"message": null,
	"timestamp": "2025-01-15T10:00:00Z"
}
```

### 2. Get Unread Count

```http
GET /api/Notification/unread-count
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"UnreadCount": 5
	}
}
```

### 3. Mark as Read

```http
PUT /api/Notification/{notificationId}/read
Authorization: Bearer {token}
```

### 4. Mark All as Read

```http
PUT /api/Notification/mark-all-read
Authorization: Bearer {token}
```

---

## ⚠️ Common Implementation Mistakes

### ❌ Mistake 1: Not Connecting to SignalR

**Wrong:**

```javascript
// Only using API polling - missing real-time updates
setInterval(() => {
  fetch('/api/Notification').then(...);
}, 5000);
```

**✅ Correct:**

```javascript
// Use SignalR for real-time, API for initial load and fallback
connection.on('ReceiveNotification', handleNotification);
```

---

### ❌ Mistake 2: Not Handling Connection Failures

**Wrong:**

```javascript
connection.start(); // No error handling
```

**✅ Correct:**

```javascript
connection
	.start()
	.then(() => console.log('Connected'))
	.catch((error) => {
		console.error('Connection failed:', error);
		// Fallback to polling
		startPolling();
	});
```

---

### ❌ Mistake 3: Not Including Authorization Token

**Wrong:**

```javascript
const connection = new HubConnectionBuilder()
	.withUrl('/notificationHub') // Missing auth
	.build();
```

**✅ Correct:**

```javascript
const connection = new HubConnectionBuilder()
	.withUrl('/notificationHub', {
		accessTokenFactory: () => token,
		headers: {
			Authorization: `Bearer ${token}`,
		},
	})
	.build();
```

---

### ❌ Mistake 4: Not Listening to Both Events

**Wrong:**

```javascript
// Only listening to personal notifications
connection.on('ReceiveNotification', handleNotification);
// Missing: NewRequest event
```

**✅ Correct:**

```javascript
// Listen to both events
connection.on('ReceiveNotification', handlePersonalNotification);
connection.on('NewRequest', handleRoleBroadcast);
```

---

### ❌ Mistake 5: Not Loading Initial Notifications

**Wrong:**

```javascript
// Only listening to new notifications - missing existing ones
connection.on('ReceiveNotification', handleNotification);
```

**✅ Correct:**

```javascript
connection.start().then(() => {
	// Load existing notifications from API
	loadNotifications();
	// Also listen for new ones
	connection.on('ReceiveNotification', handleNotification);
});
```

---

### ❌ Mistake 6: Not Handling Reconnection

**Wrong:**

```javascript
// No reconnection handling - notifications stop if connection drops
const connection = new HubConnectionBuilder()
	.withUrl('/notificationHub')
	.build();
```

**✅ Correct:**

```javascript
const connection = new HubConnectionBuilder()
	.withUrl('/notificationHub')
	.withAutomaticReconnect() // ✅ Enables auto-reconnect
	.build();

connection.onreconnected(() => {
	// Reload notifications after reconnection
	loadNotifications();
});
```

---

## 🔍 Testing Your Implementation

### 1. Check SignalR Connection

Open browser console and verify:

```
✅ Connected to SignalR hub
```

### 2. Test Notification Flow

1. **Login as SalesSupport user** (e.g., `Ahmed_Hemdan_Engineering_001`)
2. **Verify SignalR connection** in console
3. **Create TaskProgress with offer request** as Salesman:
      ```http
      POST /api/TaskProgress/with-offer-request
      {
        "taskId": 123,
        "visitResult": "Interested",
        "nextStep": "NeedsOffer",
        "clientId": 456,
        "requestedProducts": "X-Ray Machine"
      }
      ```
4. **Check console** - should see:
      ```
      📬 New notification: { title: "New Offer Request", ... }
      ```

### 3. Verify Database Fallback

1. **Disconnect from SignalR** (close browser tab or disconnect)
2. **Create TaskProgress with offer request** as Salesman
3. **Reconnect to SignalR** and check `/api/Notification`
4. **Should see notification** in the list (even though real-time delivery failed)

---

## 📊 Notification Flow Diagram

```
┌─────────────────┐
│  Salesman       │
│  Creates Task   │
│  Progress with  │
│  Offer Request  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Backend:       │
│  1. Create      │
│     OfferRequest│
│  2. Find        │
│     SalesSupport│
│     users       │
│  3. Save        │
│     notification│
│     to DB       │
└────────┬────────┘
         │
         ├──────────────────┬──────────────────┐
         ▼                  ▼                  ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  SignalR Send   │  │  SignalR Send   │  │  SignalR Send   │
│  to User_001    │  │  to User_002    │  │  to User_003    │
└────────┬────────┘  └────────┬────────┘  └────────┬────────┘
         │                     │                     │
         ▼                     ▼                     ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  If Connected:  │  │  If Connected:  │  │  If Not:        │
│  Real-time      │  │  Real-time      │  │  Saved in DB    │
│  Delivery ✅    │  │  Delivery ✅    │  │  (poll via API) │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

---

## 🔑 Key Points to Remember

1. ✅ **Notifications are ALWAYS saved to database** - even if SignalR fails
2. ✅ **Real-time delivery requires active SignalR connection**
3. ✅ **Always implement API polling as fallback**
4. ✅ **Load existing notifications on connection start**
5. ✅ **Handle reconnection events** to reload notifications
6. ✅ **Include JWT token in SignalR connection**
7. ✅ **Listen to both `ReceiveNotification` and `NewRequest` events**

---

## 📞 Support

If notifications aren't working:

1. **Check browser console** for SignalR connection errors
2. **Verify JWT token** is valid and included
3. **Check backend logs** for notification creation
4. **Test API endpoint** directly: `GET /api/Notification`
5. **Verify user has SalesSupport role** (for offer request notifications)

---

**Last Updated:** 2025-01-15
**Backend Version:** Latest
**SignalR Hub:** `/notificationHub`
