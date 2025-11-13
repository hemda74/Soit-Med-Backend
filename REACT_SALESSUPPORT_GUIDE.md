# React Guide - SalesSupport Web App

## 🎯 Overview

This guide is for **SalesSupport** users who use the **React web application** to:

- Receive real-time notifications when Salesmen create offer requests
- View and manage offer requests
- Track progress on assigned offers

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [SignalR Connection Setup](#signalr-connection-setup)
4. [Receiving Notifications](#receiving-notifications)
5. [Complete React Hook Example](#complete-react-hook-example)
6. [Complete Component Examples](#complete-component-examples)
7. [Troubleshooting](#troubleshooting)
8. [API Endpoints Reference](#api-endpoints-reference)

---

## 📦 Prerequisites

- React 16.8+ (with Hooks)
- `@microsoft/signalr` package
- JWT authentication token stored in localStorage/sessionStorage
- API base URL configured

---

## 📥 Installation

```bash
npm install @microsoft/signalr
# or
yarn add @microsoft/signalr
```

---

## 🔌 SignalR Connection Setup

### Hub Configuration

**Hub URL:** `{baseUrl}/notificationHub`

**Example:**

```javascript
const baseUrl = 'http://localhost:5117'; // Your API URL
const hubUrl = `${baseUrl}/notificationHub`;
```

### Authentication

SignalR requires **JWT token** in the connection:

```typescript
import * as signalR from '@microsoft/signalr';

const token = localStorage.getItem('authToken'); // Your token storage

const connection = new signalR.HubConnectionBuilder()
	.withUrl(`${baseUrl}/notificationHub`, {
		accessTokenFactory: () => token || '',
		headers: {
			Authorization: `Bearer ${token}`,
		},
	})
	.withAutomaticReconnect() // Auto-reconnect on disconnect
	.configureLogging(signalR.LogLevel.Information) // Enable logging
	.build();
```

---

## 📡 Receiving Notifications

### SignalR Event: `ReceiveNotification`

**Triggered when:** A Salesman creates a TaskProgress with offer request, and the system sends you a notification.

**Event Handler:**

```typescript
connection.on('ReceiveNotification', (notification) => {
	console.log('📬 New notification received:', notification);

	// Add to notifications list
	setNotifications((prev) => [notification, ...prev]);

	// Update unread count
	setUnreadCount((prev) => prev + 1);

	// Show toast/alert to user
	showNotificationToast(notification);
});
```

**Notification Data Structure:**

```typescript
interface Notification {
	id: number;
	title: string; // e.g., "New Offer Request"
	message: string; // e.g., "Ahmed Mohamed requested an offer for client: Cairo Hospital"
	type: string; // e.g., "OfferRequest"
	priority: string; // "Low" | "Medium" | "High"
	isRead: boolean;
	createdAt: string; // ISO 8601 date string
	requestWorkflowId?: number | null;
	activityLogId?: number | null;
}
```

---

## 🎣 Complete React Hook Example

### `useNotification.ts` Hook

```typescript
import { useEffect, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

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

interface UseNotificationReturn {
	connection: signalR.HubConnection | null;
	notifications: Notification[];
	unreadCount: number;
	isConnected: boolean;
	loadNotifications: () => Promise<void>;
	markAsRead: (notificationId: number) => Promise<void>;
	markAllAsRead: () => Promise<void>;
	getUnreadCount: () => Promise<number>;
}

export const useNotification = (
	baseUrl: string = 'http://localhost:5117'
): UseNotificationReturn => {
	const [connection, setConnection] =
		useState<signalR.HubConnection | null>(null);
	const [notifications, setNotifications] = useState<Notification[]>([]);
	const [unreadCount, setUnreadCount] = useState(0);
	const [isConnected, setIsConnected] = useState(false);

	// Load notifications from API
	const loadNotifications = useCallback(async () => {
		try {
			const token = localStorage.getItem('authToken');
			const response = await fetch(
				`${baseUrl}/api/Notification?page=1&pageSize=50`,
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
	}, [baseUrl]);

	// Get unread count
	const getUnreadCount = useCallback(async () => {
		try {
			const token = localStorage.getItem('authToken');
			const response = await fetch(
				`${baseUrl}/api/Notification/unread-count`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			if (response.ok) {
				const data = await response.json();
				if (
					data.success &&
					typeof data.data === 'number'
				) {
					setUnreadCount(data.data);
					return data.data;
				}
			}
		} catch (error) {
			console.error('Error getting unread count:', error);
		}
		return 0;
	}, [baseUrl]);

	// Mark notification as read
	const markAsRead = useCallback(
		async (notificationId: number) => {
			try {
				const token = localStorage.getItem('authToken');
				const response = await fetch(
					`${baseUrl}/api/Notification/${notificationId}/read`,
					{
						method: 'PUT',
						headers: {
							Authorization: `Bearer ${token}`,
						},
					}
				);

				if (response.ok) {
					setNotifications((prev) =>
						prev.map((n) =>
							n.id === notificationId
								? {
										...n,
										isRead: true,
								  }
								: n
						)
					);
					setUnreadCount((prev) =>
						Math.max(0, prev - 1)
					);
				}
			} catch (error) {
				console.error(
					'Error marking notification as read:',
					error
				);
			}
		},
		[baseUrl]
	);

	// Mark all notifications as read
	const markAllAsRead = useCallback(async () => {
		try {
			const token = localStorage.getItem('authToken');
			const response = await fetch(
				`${baseUrl}/api/Notification/read-all`,
				{
					method: 'PUT',
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			if (response.ok) {
				setNotifications((prev) =>
					prev.map((n) => ({
						...n,
						isRead: true,
					}))
				);
				setUnreadCount(0);
			}
		} catch (error) {
			console.error(
				'Error marking all notifications as read:',
				error
			);
		}
	}, [baseUrl]);

	// Initialize SignalR connection
	useEffect(() => {
		const token = localStorage.getItem('authToken');
		if (!token) {
			console.warn(
				'No auth token found. SignalR connection skipped.'
			);
			return;
		}

		// Create SignalR connection
		const newConnection = new signalR.HubConnectionBuilder()
			.withUrl(`${baseUrl}/notificationHub`, {
				accessTokenFactory: () => token || '',
				headers: {
					Authorization: `Bearer ${token}`,
				},
			})
			.withAutomaticReconnect({
				nextRetryDelayInMilliseconds: (
					retryContext
				) => {
					// Exponential backoff: 0s, 2s, 10s, 30s, then 30s intervals
					if (
						retryContext.previousRetryCount ===
						0
					)
						return 0;
					if (
						retryContext.previousRetryCount ===
						1
					)
						return 2000;
					if (
						retryContext.previousRetryCount ===
						2
					)
						return 10000;
					return 30000;
				},
			})
			.configureLogging(signalR.LogLevel.Information)
			.build();

		// Set up event handlers
		newConnection.on(
			'ReceiveNotification',
			(notification: Notification) => {
				console.log(
					'📬 New notification received:',
					notification
				);

				// Add to notifications list (at the beginning)
				setNotifications((prev) => [
					notification,
					...prev,
				]);

				// Update unread count
				setUnreadCount((prev) => prev + 1);

				// Show browser notification (optional)
				if (
					'Notification' in window &&
					Notification.permission === 'granted'
				) {
					new Notification(notification.title, {
						body: notification.message,
						icon: '/notification-icon.png',
					});
				}

				// Show toast notification (implement your toast system)
				// toast.success(notification.title, { description: notification.message });
			}
		);

		newConnection.on('NewRequest', (request: any) => {
			console.log('🔔 New request broadcast:', request);
			// Handle role-based broadcast if needed
			// This is sent to all SalesSupport users in the role group
		});

		// Handle connection events
		newConnection.onclose((error) => {
			console.warn('⚠️ SignalR connection closed:', error);
			setIsConnected(false);
		});

		newConnection.onreconnecting((error) => {
			console.log('🔄 SignalR reconnecting...', error);
			setIsConnected(false);
		});

		newConnection.onreconnected((connectionId) => {
			console.log(
				'✅ SignalR reconnected. Connection ID:',
				connectionId
			);
			setIsConnected(true);
			// Reload notifications after reconnect
			loadNotifications();
			getUnreadCount();
		});

		// Start connection
		newConnection
			.start()
			.then(() => {
				console.log('✅ Connected to SignalR hub');
				setIsConnected(true);
				setConnection(newConnection);
				// Load existing notifications
				loadNotifications();
				getUnreadCount();
			})
			.catch((error) => {
				console.error(
					'❌ Error connecting to SignalR:',
					error
				);
				setIsConnected(false);
				// Fallback: Use polling instead
				const pollingInterval = setInterval(() => {
					loadNotifications();
					getUnreadCount();
				}, 10000); // Poll every 10 seconds

				// Clear polling when connection is established
				newConnection.onreconnected(() => {
					clearInterval(pollingInterval);
				});
			});

		// Cleanup on unmount
		return () => {
			if (newConnection) {
				newConnection.stop();
			}
		};
	}, [baseUrl, loadNotifications, getUnreadCount]);

	return {
		connection,
		notifications,
		unreadCount,
		isConnected,
		loadNotifications,
		markAsRead,
		markAllAsRead,
		getUnreadCount,
	};
};
```

---

## 🧩 Complete Component Examples

### 1. NotificationBell Component

```typescript
// components/NotificationBell.tsx
import React, { useState } from 'react';
import { useNotification } from '../hooks/useNotification';
import './NotificationBell.css'; // Your styles

export const NotificationBell: React.FC = () => {
	const {
		notifications,
		unreadCount,
		isConnected,
		markAsRead,
		markAllAsRead,
	} = useNotification();

	const [isOpen, setIsOpen] = useState(false);

	return (
		<div className="notification-bell">
			<button
				onClick={() => setIsOpen(!isOpen)}
				className="bell-button"
				aria-label="Notifications"
			>
				<svg
					xmlns="http://www.w3.org/2000/svg"
					width="24"
					height="24"
					viewBox="0 0 24 24"
					fill="none"
					stroke="currentColor"
					strokeWidth="2"
				>
					<path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
					<path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
				</svg>
				{unreadCount > 0 && (
					<span className="badge">
						{unreadCount}
					</span>
				)}
				{!isConnected && (
					<span
						className="connection-status"
						title="Disconnected"
					>
						⚠️
					</span>
				)}
			</button>

			{isOpen && (
				<div className="notification-dropdown">
					<div className="notification-header">
						<h3>Notifications</h3>
						{unreadCount > 0 && (
							<button
								onClick={
									markAllAsRead
								}
								className="mark-all-read"
							>
								Mark all as read
							</button>
						)}
					</div>

					<div className="notification-list">
						{notifications.length === 0 ? (
							<div className="empty-state">
								No notifications
							</div>
						) : (
							notifications.map(
								(
									notification
								) => (
									<div
										key={
											notification.id
										}
										className={`notification-item ${
											!notification.isRead
												? 'unread'
												: ''
										}`}
										onClick={() =>
											!notification.isRead &&
											markAsRead(
												notification.id
											)
										}
									>
										<div className="notification-content">
											<div className="notification-title">
												{
													notification.title
												}
											</div>
											<div className="notification-message">
												{
													notification.message
												}
											</div>
											<div className="notification-meta">
												<span className="notification-type">
													{
														notification.type
													}
												</span>
												<span className="notification-date">
													{new Date(
														notification.createdAt
													).toLocaleString()}
												</span>
											</div>
										</div>
										{!notification.isRead && (
											<div className="unread-indicator"></div>
										)}
									</div>
								)
							)
						)}
					</div>
				</div>
			)}
		</div>
	);
};
```

### 2. OfferRequestList Component

```typescript
// components/OfferRequestList.tsx
import React, { useEffect, useState } from 'react';
import { useNotification } from '../hooks/useNotification';
import './OfferRequestList.css';

interface OfferRequest {
	id: number;
	clientName: string;
	requestedProducts: string;
	status: string;
	requestDate: string;
	createdOfferId?: number | null;
}

export const OfferRequestList: React.FC = () => {
	const { notifications } = useNotification();
	const [offerRequests, setOfferRequests] = useState<OfferRequest[]>([]);
	const [loading, setLoading] = useState(true);

	useEffect(() => {
		loadOfferRequests();
	}, [notifications]); // Reload when new notifications arrive

	const loadOfferRequests = async () => {
		try {
			const token = localStorage.getItem('authToken');
			const response = await fetch(
				'http://localhost:5117/api/OfferRequest?status=Assigned',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			if (response.ok) {
				const data = await response.json();
				if (data.success && data.data) {
					setOfferRequests(data.data);
				}
			}
		} catch (error) {
			console.error('Error loading offer requests:', error);
		} finally {
			setLoading(false);
		}
	};

	if (loading) {
		return <div>Loading offer requests...</div>;
	}

	return (
		<div className="offer-request-list">
			<h2>Assigned Offer Requests</h2>
			{offerRequests.length === 0 ? (
				<p>No offer requests assigned to you</p>
			) : (
				<ul>
					{offerRequests.map((request) => (
						<li
							key={request.id}
							className="offer-request-item"
						>
							<div className="request-header">
								<h3>
									{
										request.clientName
									}
								</h3>
								<span
									className={`status ${request.status.toLowerCase()}`}
								>
									{
										request.status
									}
								</span>
							</div>
							<p className="request-products">
								{
									request.requestedProducts
								}
							</p>
							<p className="request-date">
								Requested:{' '}
								{new Date(
									request.requestDate
								).toLocaleDateString()}
							</p>
							{request.createdOfferId && (
								<a
									href={`/offers/${request.createdOfferId}`}
								>
									View
									Created
									Offer →
								</a>
							)}
						</li>
					))}
				</ul>
			)}
		</div>
	);
};
```

---

## 🛠️ Troubleshooting

### ❌ Issue 1: No Notifications Received

**Check:**

1. **SignalR Connection Status**

      ```typescript
      console.log('Connection state:', connection?.state);
      // Should be: Connected
      ```

2. **User Role**

      - You must have `SalesSupport` role
      - Check with your admin

3. **TaskProgress Creation**

      - Salesman must create with `visitResult: "Interested"` and `nextStep: "NeedsOffer"`
      - Task must have a `clientId`

4. **Token Validity**
      - Token might be expired
      - Check token in localStorage
      - Try logging out and back in

### ❌ Issue 2: Connection Keeps Dropping

**Solution:**

```typescript
.withAutomaticReconnect({
  nextRetryDelayInMilliseconds: (retryContext) => {
    if (retryContext.previousRetryCount === 0) return 0;
    if (retryContext.previousRetryCount === 1) return 2000;
    if (retryContext.previousRetryCount === 2) return 10000;
    return 30000;
  },
})
```

### ❌ Issue 3: Notifications in DB but Not Received

**Solution:**

- Check browser console for SignalR connection errors
- Verify token is valid and not expired
- Check network tab for WebSocket connection
- Verify you have `SalesSupport` role

---

## 📚 API Endpoints Reference

### Get Notifications

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
			"id": 1,
			"title": "New Offer Request",
			"message": "Ahmed Mohamed requested an offer for client: Cairo Hospital",
			"type": "OfferRequest",
			"priority": "High",
			"isRead": false,
			"createdAt": "2024-01-15T10:30:00Z"
		}
	]
}
```

### Get Unread Count

```http
GET /api/Notification/unread-count
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": 5
}
```

### Mark Notification as Read

```http
PUT /api/Notification/{notificationId}/read
Authorization: Bearer {token}
```

### Mark All Notifications as Read

```http
PUT /api/Notification/read-all
Authorization: Bearer {token}
```

### Get Offer Requests (Assigned to Me)

```http
GET /api/OfferRequest?status=Assigned
Authorization: Bearer {token}
```

---

## ✅ Checklist

Before going to production:

- [ ] Install `@microsoft/signalr`
- [ ] SignalR connection is established on app startup
- [ ] Token is included in SignalR connection headers
- [ ] `ReceiveNotification` event handler is registered
- [ ] Notifications are loaded from API on connection start
- [ ] Reconnection is handled (auto-reconnect enabled)
- [ ] Polling fallback is implemented for connection failures
- [ ] Browser notification permissions are requested (optional)
- [ ] Unread count badge is displayed
- [ ] Notifications can be marked as read

---

## 🎯 Summary

1. **Install** `@microsoft/signalr`
2. **Connect** to `/notificationHub` with JWT token
3. **Listen** for `ReceiveNotification` event
4. **Receive** notifications when Salesmen create offer requests
5. **Handle** reconnection and errors gracefully
6. **Display** notifications in your UI

---

**Last Updated:** 2024-01-15  
**Target Platform:** React Web Application  
**User Role:** SalesSupport
