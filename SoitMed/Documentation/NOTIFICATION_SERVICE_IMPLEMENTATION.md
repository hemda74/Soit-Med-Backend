# Notification Service Implementation Guide

## Overview

This guide provides a **standalone notification service** for handling real-time notifications in the React web application using SignalR.

## Installation

### Required Packages

```bash
npm install @microsoft/signalr
npm install react-hot-toast
```

## SignalR Service Implementation

Create `src/services/signalRService.js`:

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
	constructor() {
		this.connection = null;
		this.isConnected = false;
		this.reconnectAttempts = 0;
		this.maxReconnectAttempts = 5;
		this.reconnectTimer = null;
		this.isReconnecting = false;
		this.listeners = new Map();
	}

	async connect() {
		try {
			if (this.isReconnecting) return;

			const token = this.getAuthToken();
			if (!token) {
				throw new Error(
					'No authentication token found'
				);
			}

			this.connection = new HubConnectionBuilder()
				.withUrl(
					`${process.env.REACT_APP_API_URL}/notificationHub`,
					{
						accessTokenFactory: () => token,
						transport: [
							'WebSockets',
							'ServerSentEvents',
							'LongPolling',
						],
					}
				)
				.configureLogging(LogLevel.Information)
				.withAutomaticReconnect({
					nextRetryDelayInMilliseconds: (
						retryContext
					) => {
						return Math.min(
							1000 *
								Math.pow(
									2,
									retryContext.previousRetryCount
								),
							30000
						);
					},
				})
				.build();

			// Set up event handlers
			this.setupEventHandlers();

			// Start connection
			await this.connection.start();
			this.isConnected = true;
			this.reconnectAttempts = 0;
			this.isReconnecting = false;

			console.log('SignalR Connected');

			// Notify listeners of connection
			this.notifyListeners('connectionStatus', {
				isConnected: true,
			});
		} catch (error) {
			console.error('SignalR Connection Error:', error);
			this.isReconnecting = true;
			this.handleReconnect();
		}
	}

	setupEventHandlers() {
		// Handle new notifications
		this.connection.on(
			'ReceiveNotification',
			(message, link, data) => {
				this.handleNotification(message, link, data);
			}
		);

		// Handle new requests
		this.connection.on('NewRequest', (request) => {
			this.handleNewRequest(request);
		});

		// Handle request assignments
		this.connection.on('RequestAssigned', (assignment) => {
			this.handleRequestAssignment(assignment);
		});

		// Handle request status updates
		this.connection.on('RequestStatusUpdated', (update) => {
			this.handleRequestStatusUpdate(update);
		});

		// Handle sales report updates
		this.connection.on('SalesReportUpdated', (report) => {
			this.handleSalesReportUpdate(report);
		});

		// Handle user role changes
		this.connection.on('UserRoleChanged', (user) => {
			this.handleUserRoleChange(user);
		});

		// Handle connection events
		this.connection.onclose((error) => {
			console.log('SignalR Connection Closed:', error);
			this.isConnected = false;
			this.isReconnecting = false;
			this.notifyListeners('connectionStatus', {
				isConnected: false,
			});
			this.handleReconnect();
		});

		this.connection.onreconnecting((error) => {
			console.log('SignalR Reconnecting:', error);
			this.isReconnecting = true;
			this.notifyListeners('connectionStatus', {
				isReconnecting: true,
			});
		});

		this.connection.onreconnected((connectionId) => {
			console.log('SignalR Reconnected:', connectionId);
			this.isConnected = true;
			this.isReconnecting = false;
			this.reconnectAttempts = 0;
			this.notifyListeners('connectionStatus', {
				isConnected: true,
				isReconnecting: false,
			});
		});
	}

	handleNotification(message, link, data) {
		console.log('Received Notification:', { message, link, data });

		// Show browser notification if permission is granted
		this.showBrowserNotification(
			'New Notification',
			message,
			link,
			data
		);

		// Notify listeners
		this.notifyListeners('notification', { message, link, data });
	}

	handleNewRequest(request) {
		console.log('New Request:', request);

		// Show browser notification for new requests
		this.showBrowserNotification(
			'New Request',
			`${request.requestType} request from ${request.clientName}`,
			null,
			{ type: 'newRequest', ...request }
		);

		this.notifyListeners('newRequest', request);
	}

	handleRequestAssignment(assignment) {
		console.log('Request Assigned:', assignment);

		this.showBrowserNotification(
			'Request Assigned',
			`You have been assigned a ${assignment.requestType} request`,
			null,
			{ type: 'assignment', ...assignment }
		);

		this.notifyListeners('assignment', assignment);
	}

	handleRequestStatusUpdate(update) {
		console.log('Request Status Updated:', update);

		this.showBrowserNotification(
			'Request Updated',
			`Request status has been updated to ${update.newStatus}`,
			null,
			{ type: 'statusUpdate', ...update }
		);

		this.notifyListeners('statusUpdate', update);
	}

	handleSalesReportUpdate(report) {
		console.log('Sales Report Updated:', report);

		this.showBrowserNotification(
			'Sales Report Updated',
			`New sales report has been generated`,
			null,
			{ type: 'salesReport', ...report }
		);

		this.notifyListeners('salesReportUpdate', report);
	}

	handleUserRoleChange(user) {
		console.log('User Role Changed:', user);

		this.showBrowserNotification(
			'User Role Updated',
			`User ${user.firstName} ${user.lastName} role has been changed`,
			null,
			{ type: 'userRoleChange', ...user }
		);

		this.notifyListeners('userRoleChange', user);
	}

	showBrowserNotification(title, message, link, data) {
		if (
			'Notification' in window &&
			Notification.permission === 'granted'
		) {
			const notification = new Notification(title, {
				body: message,
				icon: '/favicon.ico',
				badge: '/favicon.ico',
				tag: data?.type || 'default',
				data: data,
			});

			notification.onclick = () => {
				window.focus();
				if (link) {
					window.location.href = link;
				}
				notification.close();
			};

			// Auto close after 5 seconds
			setTimeout(() => {
				notification.close();
			}, 5000);
		}
	}

	// Request notification permission
	async requestNotificationPermission() {
		if ('Notification' in window) {
			const permission =
				await Notification.requestPermission();
			return permission === 'granted';
		}
		return false;
	}

	// Add event listener
	addEventListener(event, callback) {
		if (!this.listeners.has(event)) {
			this.listeners.set(event, []);
		}
		this.listeners.get(event).push(callback);
	}

	// Remove event listener
	removeEventListener(event, callback) {
		if (this.listeners.has(event)) {
			const callbacks = this.listeners.get(event);
			const index = callbacks.indexOf(callback);
			if (index > -1) {
				callbacks.splice(index, 1);
			}
		}
	}

	// Notify all listeners of an event
	notifyListeners(event, data) {
		if (this.listeners.has(event)) {
			this.listeners.get(event).forEach((callback) => {
				try {
					callback(data);
				} catch (error) {
					console.error(
						'Error in event listener:',
						error
					);
				}
			});
		}
	}

	async handleReconnect() {
		if (this.reconnectAttempts >= this.maxReconnectAttempts) {
			console.error('Max reconnection attempts reached');
			this.isReconnecting = false;
			this.notifyListeners('connectionStatus', {
				isConnected: false,
				isReconnecting: false,
			});
			return;
		}

		this.reconnectAttempts++;
		const delay = Math.min(
			1000 * Math.pow(2, this.reconnectAttempts),
			30000
		);

		console.log(
			`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts})`
		);

		this.reconnectTimer = setTimeout(() => {
			this.connect();
		}, delay);
	}

	async disconnect() {
		if (this.reconnectTimer) {
			clearTimeout(this.reconnectTimer);
			this.reconnectTimer = null;
		}

		if (this.connection) {
			await this.connection.stop();
			this.isConnected = false;
			this.isReconnecting = false;
		}
	}

	// Send message to hub
	async sendMessage(methodName, ...args) {
		if (this.connection && this.isConnected) {
			try {
				await this.connection.invoke(
					methodName,
					...args
				);
			} catch (error) {
				console.error('Error sending message:', error);
			}
		}
	}

	// Get connection status
	getConnectionStatus() {
		return {
			isConnected: this.isConnected,
			isReconnecting: this.isReconnecting,
			reconnectAttempts: this.reconnectAttempts,
		};
	}

	getAuthToken() {
		return localStorage.getItem('authToken');
	}
}

export default new SignalRService();
```

## Notification Service Implementation

Create `src/services/notificationService.js`:

```javascript
import toast from 'react-hot-toast';
import signalRService from './signalRService';

class NotificationService {
	constructor() {
		this.notifications = [];
		this.unreadCount = 0;
		this.listeners = new Map();
	}

	// Initialize notification service
	async initialize() {
		// Request notification permission
		await signalRService.requestNotificationPermission();

		// Set up SignalR listeners
		signalRService.addEventListener(
			'notification',
			this.handleNotification.bind(this)
		);
		signalRService.addEventListener(
			'newRequest',
			this.handleNewRequest.bind(this)
		);
		signalRService.addEventListener(
			'assignment',
			this.handleAssignment.bind(this)
		);
		signalRService.addEventListener(
			'statusUpdate',
			this.handleStatusUpdate.bind(this)
		);
		signalRService.addEventListener(
			'salesReportUpdate',
			this.handleSalesReportUpdate.bind(this)
		);
		signalRService.addEventListener(
			'userRoleChange',
			this.handleUserRoleChange.bind(this)
		);
	}

	handleNotification(data) {
		this.addNotification({
			id: Date.now(),
			type: 'notification',
			title: 'New Notification',
			message: data.message,
			link: data.link,
			data: data.data,
			timestamp: new Date(),
			isRead: false,
		});
	}

	handleNewRequest(data) {
		this.addNotification({
			id: Date.now(),
			type: 'newRequest',
			title: 'New Request',
			message: `${data.requestType} request from ${data.clientName}`,
			data: data,
			timestamp: new Date(),
			isRead: false,
		});

		// Show toast notification
		toast.success(`New ${data.requestType} request received`);
	}

	handleAssignment(data) {
		this.addNotification({
			id: Date.now(),
			type: 'assignment',
			title: 'Request Assigned',
			message: `You have been assigned a ${data.requestType} request`,
			data: data,
			timestamp: new Date(),
			isRead: false,
		});

		toast.info('You have been assigned a new request');
	}

	handleStatusUpdate(data) {
		this.addNotification({
			id: Date.now(),
			type: 'statusUpdate',
			title: 'Request Updated',
			message: `Request status updated to ${data.newStatus}`,
			data: data,
			timestamp: new Date(),
			isRead: false,
		});

		toast.info('Request status has been updated');
	}

	handleSalesReportUpdate(data) {
		this.addNotification({
			id: Date.now(),
			type: 'salesReport',
			title: 'Sales Report Updated',
			message: `New sales report has been generated`,
			data: data,
			timestamp: new Date(),
			isRead: false,
		});

		toast.success('New sales report available');
	}

	handleUserRoleChange(data) {
		this.addNotification({
			id: Date.now(),
			type: 'userRoleChange',
			title: 'User Role Updated',
			message: `User ${data.firstName} ${data.lastName} role has been changed`,
			data: data,
			timestamp: new Date(),
			isRead: false,
		});

		toast.info('User role has been updated');
	}

	addNotification(notification) {
		this.notifications.unshift(notification);
		if (!notification.isRead) {
			this.unreadCount++;
		}

		// Keep only last 100 notifications
		if (this.notifications.length > 100) {
			this.notifications = this.notifications.slice(0, 100);
		}

		this.notifyListeners('notificationAdded', notification);
		this.notifyListeners('unreadCountChanged', this.unreadCount);
	}

	markAsRead(notificationId) {
		const notification = this.notifications.find(
			(n) => n.id === notificationId
		);
		if (notification && !notification.isRead) {
			notification.isRead = true;
			this.unreadCount--;
			this.notifyListeners('notificationRead', notification);
			this.notifyListeners(
				'unreadCountChanged',
				this.unreadCount
			);
		}
	}

	markAllAsRead() {
		this.notifications.forEach((notification) => {
			notification.isRead = true;
		});
		this.unreadCount = 0;
		this.notifyListeners('allNotificationsRead');
		this.notifyListeners('unreadCountChanged', this.unreadCount);
	}

	getNotifications() {
		return this.notifications;
	}

	getUnreadCount() {
		return this.unreadCount;
	}

	getUnreadNotifications() {
		return this.notifications.filter((n) => !n.isRead);
	}

	// Filter notifications by type
	getNotificationsByType(type) {
		return this.notifications.filter((n) => n.type === type);
	}

	// Clear old notifications
	clearOldNotifications(daysOld = 7) {
		const cutoffDate = new Date();
		cutoffDate.setDate(cutoffDate.getDate() - daysOld);

		this.notifications = this.notifications.filter(
			(n) => new Date(n.timestamp) > cutoffDate
		);

		// Recalculate unread count
		this.unreadCount = this.notifications.filter(
			(n) => !n.isRead
		).length;
		this.notifyListeners('unreadCountChanged', this.unreadCount);
	}

	// Add event listener
	addEventListener(event, callback) {
		if (!this.listeners.has(event)) {
			this.listeners.set(event, []);
		}
		this.listeners.get(event).push(callback);
	}

	// Remove event listener
	removeEventListener(event, callback) {
		if (this.listeners.has(event)) {
			const callbacks = this.listeners.get(event);
			const index = callbacks.indexOf(callback);
			if (index > -1) {
				callbacks.splice(index, 1);
			}
		}
	}

	// Notify all listeners of an event
	notifyListeners(event, data) {
		if (this.listeners.has(event)) {
			this.listeners.get(event).forEach((callback) => {
				try {
					callback(data);
				} catch (error) {
					console.error(
						'Error in notification listener:',
						error
					);
				}
			});
		}
	}
}

export default new NotificationService();
```

## Usage Examples

### 1. Initialize Notifications in App

```javascript
import React, { useEffect } from 'react';
import signalRService from './services/signalRService';
import notificationService from './services/notificationService';

const App = () => {
  useEffect(() => {
    const initializeNotifications = async () => {
      try {
        await notificationService.initialize();
        await signalRService.connect();
      } catch (error) {
        console.error('Failed to initialize notifications:', error);
      }
    };

    initializeNotifications();

    return () => {
      signalRService.disconnect();
    };
  }, []);

  return (
    // Your app components
  );
};
```

### 2. Use Notifications in Components

```javascript
import React, { useState, useEffect } from 'react';
import notificationService from '../services/notificationService';

const NotificationComponent = () => {
	const [notifications, setNotifications] = useState([]);
	const [unreadCount, setUnreadCount] = useState(0);

	useEffect(() => {
		// Set up notification listeners
		notificationService.addEventListener(
			'notificationAdded',
			handleNotificationAdded
		);
		notificationService.addEventListener(
			'unreadCountChanged',
			handleUnreadCountChanged
		);

		// Load initial notifications
		setNotifications(notificationService.getNotifications());
		setUnreadCount(notificationService.getUnreadCount());

		return () => {
			notificationService.removeEventListener(
				'notificationAdded',
				handleNotificationAdded
			);
			notificationService.removeEventListener(
				'unreadCountChanged',
				handleUnreadCountChanged
			);
		};
	}, []);

	const handleNotificationAdded = (notification) => {
		setNotifications(notificationService.getNotifications());
	};

	const handleUnreadCountChanged = (count) => {
		setUnreadCount(count);
	};

	const handleMarkAsRead = (notificationId) => {
		notificationService.markAsRead(notificationId);
	};

	const handleMarkAllAsRead = () => {
		notificationService.markAllAsRead();
	};

	return (
		<div>
			<h3>Notifications ({unreadCount})</h3>
			<button onClick={handleMarkAllAsRead}>
				Mark All as Read
			</button>
			{notifications.map((notification) => (
				<div
					key={notification.id}
					onClick={() =>
						handleMarkAsRead(
							notification.id
						)
					}
				>
					<h4>{notification.title}</h4>
					<p>{notification.message}</p>
					<span>
						{notification.timestamp.toLocaleString()}
					</span>
					{!notification.isRead && <span>●</span>}
				</div>
			))}
		</div>
	);
};
```

### 3. Test Notifications

```javascript
// Test browser notification
const testNotification = () => {
	notificationService.addNotification({
		id: Date.now(),
		type: 'test',
		title: 'Test Notification',
		message: 'This is a test notification',
		timestamp: new Date(),
		isRead: false,
	});
};

// Test SignalR connection
const testConnection = () => {
	const status = signalRService.getConnectionStatus();
	console.log('Connection status:', status);
};
```

## Environment Configuration

Create `.env` file:

```env
REACT_APP_API_URL=https://your-api-url.com/api
REACT_APP_SIGNALR_URL=https://your-api-url.com/notificationHub
```

## Troubleshooting

### Common Issues

1. **Connection fails**: Check authentication token and network connectivity
2. **Notifications not received**: Verify browser notification permissions
3. **SignalR reconnection issues**: Check network stability and token validity
4. **CORS issues**: Ensure proper CORS configuration on the backend

### Debug Mode

Enable debug logging:

```javascript
// In signalRService.js
.withUrl(`${process.env.REACT_APP_API_URL}/notificationHub`, {
  accessTokenFactory: () => token,
  logLevel: LogLevel.Debug, // Add this for debugging
})
```

## Security Considerations

1. **Token Management**: Store authentication tokens securely
2. **Connection Security**: Use HTTPS for SignalR connections
3. **Data Validation**: Validate all notification data on the client side
4. **XSS Prevention**: Sanitize all user input before displaying

## Performance Optimization

1. **Connection Management**: Reconnect only when necessary
2. **Notification Batching**: Batch multiple notifications when possible
3. **Memory Management**: Clean up event listeners properly
4. **Notification Cleanup**: Clear old notifications regularly

## Conclusion

This notification service provides a complete solution for real-time notifications in your React application, with proper error handling, reconnection logic, and browser notification support.

