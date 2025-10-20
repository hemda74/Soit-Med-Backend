# React Notification System Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the notification system into your existing React web application using SignalR for real-time communication and browser notifications for user alerts.

## Table of Contents

1. [Integration Setup](#integration-setup)
2. [SignalR Service Integration](#signalr-service-integration)
3. [Notification Service Integration](#notification-service-integration)
4. [State Management Integration](#state-management-integration)
5. [UI Components Integration](#ui-components-integration)
6. [Integration Examples](#integration-examples)
7. [Error Handling Integration](#error-handling-integration)
8. [Testing Integration](#testing-integration)
9. [Performance Optimization](#performance-optimization)

## Integration Setup

### 1. Install Required Dependencies

Add these dependencies to your existing React project:

```bash
npm install @microsoft/signalr
npm install react-hot-toast
npm install @reduxjs/toolkit react-redux
npm install @headlessui/react
npm install @heroicons/react
```

## SignalR Service Integration

### 1. SignalR Service Class

Add this service to your existing services in `src/services/signalRService.js`:

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
					`${process.env.REACT_APP_SIGNALR_URL}`,
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

### 1. Notification Service Class

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

## State Management

### 1. Notification Slice

Create `src/store/slices/notificationSlice.js`:

```javascript
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
	notifications: [],
	unreadCount: 0,
	connectionStatus: {
		isConnected: false,
		isReconnecting: false,
	},
	loading: false,
	error: null,
};

const notificationSlice = createSlice({
	name: 'notifications',
	initialState,
	reducers: {
		setNotifications: (state, action) => {
			state.notifications = action.payload;
		},
		addNotification: (state, action) => {
			state.notifications.unshift(action.payload);
			if (!action.payload.isRead) {
				state.unreadCount++;
			}
		},
		markNotificationAsRead: (state, action) => {
			const notification = state.notifications.find(
				(n) => n.id === action.payload
			);
			if (notification && !notification.isRead) {
				notification.isRead = true;
				state.unreadCount--;
			}
		},
		markAllAsRead: (state) => {
			state.notifications.forEach((notification) => {
				notification.isRead = true;
			});
			state.unreadCount = 0;
		},
		setUnreadCount: (state, action) => {
			state.unreadCount = action.payload;
		},
		setConnectionStatus: (state, action) => {
			state.connectionStatus = {
				...state.connectionStatus,
				...action.payload,
			};
		},
		clearOldNotifications: (state, action) => {
			const daysOld = action.payload || 7;
			const cutoffDate = new Date();
			cutoffDate.setDate(cutoffDate.getDate() - daysOld);

			state.notifications = state.notifications.filter(
				(n) => new Date(n.timestamp) > cutoffDate
			);

			state.unreadCount = state.notifications.filter(
				(n) => !n.isRead
			).length;
		},
		setLoading: (state, action) => {
			state.loading = action.payload;
		},
		setError: (state, action) => {
			state.error = action.payload;
		},
		clearError: (state) => {
			state.error = null;
		},
	},
});

export const {
	setNotifications,
	addNotification,
	markNotificationAsRead,
	markAllAsRead,
	setUnreadCount,
	setConnectionStatus,
	clearOldNotifications,
	setLoading,
	setError,
	clearError,
} = notificationSlice.actions;

export default notificationSlice.reducer;
```

### 2. Notification Middleware

Create `src/store/middleware/notificationMiddleware.js`:

```javascript
import notificationService from '../../services/notificationService';
import signalRService from '../../services/signalRService';
import {
	addNotification,
	setUnreadCount,
	setConnectionStatus,
} from '../slices/notificationSlice';

const notificationMiddleware = (store) => (next) => (action) => {
	// Initialize notification service when app starts
	if (action.type === 'auth/loginSuccess') {
		notificationService.initialize();
		signalRService.connect();
	}

	// Set up listeners for notification events
	if (action.type === 'notifications/initialize') {
		// Listen for new notifications
		notificationService.addEventListener(
			'notificationAdded',
			(notification) => {
				store.dispatch(addNotification(notification));
			}
		);

		// Listen for unread count changes
		notificationService.addEventListener(
			'unreadCountChanged',
			(count) => {
				store.dispatch(setUnreadCount(count));
			}
		);

		// Listen for connection status changes
		signalRService.addEventListener(
			'connectionStatus',
			(status) => {
				store.dispatch(setConnectionStatus(status));
			}
		);
	}

	return next(action);
};

export default notificationMiddleware;
```

## UI Components

### 1. Notification Bell Component

Create `src/components/notifications/NotificationBell.jsx`:

```jsx
import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Menu, Transition } from '@headlessui/react';
import { BellIcon } from '@heroicons/react/24/outline';
import {
	markNotificationAsRead,
	markAllAsRead,
	clearOldNotifications,
} from '../../store/slices/notificationSlice';
import { format } from 'date-fns';

const NotificationBell = () => {
	const dispatch = useDispatch();
	const { notifications, unreadCount, connectionStatus } = useSelector(
		(state) => state.notifications
	);
	const [isOpen, setIsOpen] = useState(false);

	useEffect(() => {
		// Clear old notifications on component mount
		dispatch(clearOldNotifications(7));
	}, [dispatch]);

	const handleNotificationClick = (notification) => {
		if (!notification.isRead) {
			dispatch(markNotificationAsRead(notification.id));
		}

		// Navigate to relevant page if link exists
		if (notification.link) {
			window.location.href = notification.link;
		}
	};

	const handleMarkAllAsRead = () => {
		dispatch(markAllAsRead());
	};

	const getNotificationIcon = (type) => {
		switch (type) {
			case 'newRequest':
				return '🔔';
			case 'assignment':
				return '📋';
			case 'statusUpdate':
				return '🔄';
			case 'salesReport':
				return '📊';
			case 'userRoleChange':
				return '👤';
			default:
				return '🔔';
		}
	};

	const getNotificationColor = (type) => {
		switch (type) {
			case 'newRequest':
				return 'text-blue-600 bg-blue-50';
			case 'assignment':
				return 'text-green-600 bg-green-50';
			case 'statusUpdate':
				return 'text-yellow-600 bg-yellow-50';
			case 'salesReport':
				return 'text-purple-600 bg-purple-50';
			case 'userRoleChange':
				return 'text-gray-600 bg-gray-50';
			default:
				return 'text-gray-600 bg-gray-50';
		}
	};

	return (
		<div className="relative">
			<Menu
				as="div"
				className="relative"
			>
				<Menu.Button
					className="relative p-2 text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 rounded-full"
					onClick={() => setIsOpen(!isOpen)}
				>
					<span className="sr-only">
						View notifications
					</span>
					<BellIcon className="h-6 w-6" />
					{unreadCount > 0 && (
						<span className="absolute -top-1 -right-1 h-5 w-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
							{unreadCount > 99
								? '99+'
								: unreadCount}
						</span>
					)}
				</Menu.Button>

				<Transition
					show={isOpen}
					as={React.Fragment}
					enter="transition ease-out duration-100"
					enterFrom="transform opacity-0 scale-95"
					enterTo="transform opacity-100 scale-100"
					leave="transition ease-in duration-75"
					leaveFrom="transform opacity-100 scale-100"
					leaveTo="transform opacity-0 scale-95"
				>
					<Menu.Items className="absolute right-0 z-10 mt-2 w-80 origin-top-right rounded-md bg-white py-1 shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
						<div className="px-4 py-3 border-b border-gray-200">
							<div className="flex items-center justify-between">
								<h3 className="text-lg font-medium text-gray-900">
									Notifications
								</h3>
								<div className="flex items-center space-x-2">
									{connectionStatus.isConnected ? (
										<div className="flex items-center text-green-600">
											<div className="w-2 h-2 bg-green-500 rounded-full mr-2"></div>
											<span className="text-sm">
												Connected
											</span>
										</div>
									) : connectionStatus.isReconnecting ? (
										<div className="flex items-center text-yellow-600">
											<div className="w-2 h-2 bg-yellow-500 rounded-full mr-2 animate-pulse"></div>
											<span className="text-sm">
												Reconnecting...
											</span>
										</div>
									) : (
										<div className="flex items-center text-red-600">
											<div className="w-2 h-2 bg-red-500 rounded-full mr-2"></div>
											<span className="text-sm">
												Disconnected
											</span>
										</div>
									)}
									{unreadCount >
										0 && (
										<button
											onClick={
												handleMarkAllAsRead
											}
											className="text-sm text-blue-600 hover:text-blue-800"
										>
											Mark
											all
											as
											read
										</button>
									)}
								</div>
							</div>
						</div>

						<div className="max-h-96 overflow-y-auto">
							{notifications.length ===
							0 ? (
								<div className="px-4 py-8 text-center text-gray-500">
									<BellIcon className="h-12 w-12 mx-auto mb-4 text-gray-300" />
									<p>
										No
										notifications
										yet
									</p>
								</div>
							) : (
								notifications.map(
									(
										notification
									) => (
										<Menu.Item
											key={
												notification.id
											}
										>
											{({
												active,
											}) => (
												<div
													className={`px-4 py-3 cursor-pointer ${
														active
															? 'bg-gray-50'
															: ''
													} ${
														!notification.isRead
															? 'bg-blue-50'
															: ''
													}`}
													onClick={() =>
														handleNotificationClick(
															notification
														)
													}
												>
													<div className="flex items-start space-x-3">
														<div className="flex-shrink-0">
															<span className="text-lg">
																{getNotificationIcon(
																	notification.type
																)}
															</span>
														</div>
														<div className="flex-1 min-w-0">
															<div className="flex items-center justify-between">
																<p
																	className={`text-sm font-medium ${
																		!notification.isRead
																			? 'text-gray-900'
																			: 'text-gray-700'
																	}`}
																>
																	{
																		notification.title
																	}
																</p>
																<div className="flex items-center space-x-2">
																	<span
																		className={`px-2 py-1 text-xs rounded-full ${getNotificationColor(
																			notification.type
																		)}`}
																	>
																		{
																			notification.type
																		}
																	</span>
																	{!notification.isRead && (
																		<div className="w-2 h-2 bg-blue-500 rounded-full"></div>
																	)}
																</div>
															</div>
															<p className="text-sm text-gray-600 mt-1">
																{
																	notification.message
																}
															</p>
															<p className="text-xs text-gray-400 mt-1">
																{format(
																	new Date(
																		notification.timestamp
																	),
																	'MMM dd, HH:mm'
																)}
															</p>
														</div>
													</div>
												</div>
											)}
										</Menu.Item>
									)
								)
							)}
						</div>

						{notifications.length > 0 && (
							<div className="px-4 py-3 border-t border-gray-200">
								<button
									onClick={() => {
										// Navigate to full notifications page
										window.location.href =
											'/notifications';
									}}
									className="w-full text-center text-sm text-blue-600 hover:text-blue-800"
								>
									View all
									notifications
								</button>
							</div>
						)}
					</Menu.Items>
				</Transition>
			</Menu>
		</div>
	);
};

export default NotificationBell;
```

### 2. Toast Container Component

Create `src/components/notifications/ToastContainer.jsx`:

```jsx
import React from 'react';
import toast, { Toaster } from 'react-hot-toast';

export const showSuccess = (message) => toast.success(message);
export const showError = (message) => toast.error(message);
export const showInfo = (message) => toast(message);
export const showLoading = (message) => toast.loading(message);

export const ToastContainer = () => (
	<Toaster
		position="top-right"
		toastOptions={{
			duration: 4000,
			style: {
				background: '#363636',
				color: '#fff',
				borderRadius: '8px',
				padding: '12px 16px',
				fontSize: '14px',
				fontWeight: '500',
			},
			success: {
				duration: 3000,
				iconTheme: {
					primary: '#4ade80',
					secondary: '#fff',
				},
				style: {
					background: '#10b981',
					color: '#fff',
				},
			},
			error: {
				duration: 5000,
				iconTheme: {
					primary: '#ef4444',
					secondary: '#fff',
				},
				style: {
					background: '#ef4444',
					color: '#fff',
				},
			},
			loading: {
				duration: Infinity,
				iconTheme: {
					primary: '#3b82f6',
					secondary: '#fff',
				},
				style: {
					background: '#3b82f6',
					color: '#fff',
				},
			},
		}}
	/>
);

export default ToastContainer;
```

### 3. Connection Status Indicator

Create `src/components/notifications/ConnectionStatus.jsx`:

```jsx
import React from 'react';
import { useSelector } from 'react-redux';
import {
	WifiIcon,
	ExclamationTriangleIcon,
	CheckCircleIcon,
} from '@heroicons/react/24/outline';

const ConnectionStatus = () => {
	const { connectionStatus } = useSelector(
		(state) => state.notifications
	);

	const getStatusConfig = () => {
		if (connectionStatus.isConnected) {
			return {
				icon: CheckCircleIcon,
				color: 'text-green-600',
				bgColor: 'bg-green-50',
				text: 'Connected',
				description: 'Real-time notifications active',
			};
		} else if (connectionStatus.isReconnecting) {
			return {
				icon: WifiIcon,
				color: 'text-yellow-600',
				bgColor: 'bg-yellow-50',
				text: 'Reconnecting...',
				description: 'Attempting to reconnect',
			};
		} else {
			return {
				icon: ExclamationTriangleIcon,
				color: 'text-red-600',
				bgColor: 'bg-red-50',
				text: 'Disconnected',
				description: 'Notifications may be delayed',
			};
		}
	};

	const config = getStatusConfig();
	const Icon = config.icon;

	return (
		<div
			className={`inline-flex items-center px-3 py-1 rounded-full text-sm ${config.bgColor}`}
		>
			<Icon className={`h-4 w-4 mr-2 ${config.color}`} />
			<span className={config.color}>{config.text}</span>
		</div>
	);
};

export default ConnectionStatus;
```

## Integration Examples

### 1. App Integration

Update your main `App.js`:

```jsx
import React, { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Provider } from 'react-redux';
import { store } from './store';
import { ToastContainer } from './components/notifications/ToastContainer';
import notificationService from './services/notificationService';
import signalRService from './services/signalRService';
import DashboardLayout from './components/layout/DashboardLayout';
import SalesSupportDashboard from './pages/SalesSupportDashboard';
import SalesManagerDashboard from './pages/SalesManagerDashboard';

const App = () => {
	const dispatch = useDispatch();

	useEffect(() => {
		const initializeNotifications = async () => {
			try {
				await notificationService.initialize();
				await signalRService.connect();
			} catch (error) {
				console.error(
					'Failed to initialize notifications:',
					error
				);
			}
		};

		initializeNotifications();

		return () => {
			signalRService.disconnect();
		};
	}, []);

	return (
		<Provider store={store}>
			<Router>
				<div className="App">
					<Routes>
						<Route
							path="/"
							element={
								<DashboardLayout />
							}
						>
							<Route
								path="sales-support"
								element={
									<SalesSupportDashboard />
								}
							/>
							<Route
								path="sales-manager"
								element={
									<SalesManagerDashboard />
								}
							/>
						</Route>
					</Routes>
					<ToastContainer />
				</div>
			</Router>
		</Provider>
	);
};

export default App;
```

### 2. Header Integration

Update your header component:

```jsx
import React from 'react';
import NotificationBell from '../notifications/NotificationBell';
import ConnectionStatus from '../notifications/ConnectionStatus';

const Header = () => {
	return (
		<header className="bg-white shadow-sm border-b border-gray-200">
			<div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
				<div className="flex justify-between items-center h-16">
					<div className="flex items-center">
						<h1 className="text-xl font-semibold text-gray-900">
							Sales Management System
						</h1>
					</div>

					<div className="flex items-center space-x-4">
						<ConnectionStatus />
						<NotificationBell />
					</div>
				</div>
			</div>
		</header>
	);
};

export default Header;
```

### 3. Custom Hook for Notifications

Create `src/hooks/useNotifications.js`:

```javascript
import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import notificationService from '../services/notificationService';
import {
	addNotification,
	markNotificationAsRead,
	markAllAsRead,
} from '../store/slices/notificationSlice';

export const useNotifications = () => {
	const dispatch = useDispatch();
	const { notifications, unreadCount } = useSelector(
		(state) => state.notifications
	);
	const [isInitialized, setIsInitialized] = useState(false);

	useEffect(() => {
		if (!isInitialized) {
			// Set up listeners
			notificationService.addEventListener(
				'notificationAdded',
				(notification) => {
					dispatch(addNotification(notification));
				}
			);

			notificationService.addEventListener(
				'unreadCountChanged',
				(count) => {
					// This is handled by the middleware
				}
			);

			setIsInitialized(true);
		}
	}, [dispatch, isInitialized]);

	const markAsRead = (notificationId) => {
		dispatch(markNotificationAsRead(notificationId));
	};

	const markAllAsRead = () => {
		dispatch(markAllAsRead());
	};

	const getNotificationsByType = (type) => {
		return notifications.filter((n) => n.type === type);
	};

	const getUnreadNotifications = () => {
		return notifications.filter((n) => !n.isRead);
	};

	return {
		notifications,
		unreadCount,
		markAsRead,
		markAllAsRead,
		getNotificationsByType,
		getUnreadNotifications,
	};
};
```

## Error Handling

### 1. Error Boundary for Notifications

Create `src/components/notifications/NotificationErrorBoundary.jsx`:

```jsx
import React from 'react';

class NotificationErrorBoundary extends React.Component {
	constructor(props) {
		super(props);
		this.state = { hasError: false, error: null };
	}

	static getDerivedStateFromError(error) {
		return { hasError: true, error };
	}

	componentDidCatch(error, errorInfo) {
		console.error('Notification Error:', error, errorInfo);
	}

	render() {
		if (this.state.hasError) {
			return (
				<div className="bg-red-50 border border-red-200 rounded-md p-4">
					<div className="flex">
						<div className="flex-shrink-0">
							<svg
								className="h-5 w-5 text-red-400"
								viewBox="0 0 20 20"
								fill="currentColor"
							>
								<path
									fillRule="evenodd"
									d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
									clipRule="evenodd"
								/>
							</svg>
						</div>
						<div className="ml-3">
							<h3 className="text-sm font-medium text-red-800">
								Notification
								Error
							</h3>
							<div className="mt-2 text-sm text-red-700">
								<p>
									There
									was an
									error
									loading
									notifications.
									Please
									refresh
									the
									page.
								</p>
							</div>
						</div>
					</div>
				</div>
			);
		}

		return this.props.children;
	}
}

export default NotificationErrorBoundary;
```

### 2. Connection Error Handling

Create `src/services/connectionErrorHandler.js`:

```javascript
import {
	showError,
	showInfo,
} from '../components/notifications/ToastContainer';

class ConnectionErrorHandler {
	static handleConnectionError(error) {
		console.error('Connection Error:', error);

		if (error.message.includes('401')) {
			showError(
				'Authentication failed. Please log in again.'
			);
			// Redirect to login
			window.location.href = '/login';
		} else if (error.message.includes('403')) {
			showError(
				'Access denied. You do not have permission to receive notifications.'
			);
		} else if (error.message.includes('404')) {
			showError(
				'Notification service not found. Please contact support.'
			);
		} else if (error.message.includes('500')) {
			showError(
				'Server error. Notifications may be delayed.'
			);
		} else {
			showError(
				'Connection error. Attempting to reconnect...'
			);
		}
	}

	static handleReconnectionAttempt(attemptNumber, maxAttempts) {
		if (attemptNumber === 1) {
			showInfo('Connection lost. Attempting to reconnect...');
		} else if (attemptNumber === maxAttempts) {
			showError(
				'Failed to reconnect. Please refresh the page.'
			);
		}
	}

	static handleReconnectionSuccess() {
		showInfo('Connection restored. Notifications are now active.');
	}
}

export default ConnectionErrorHandler;
```

## Testing

### 1. SignalR Service Tests

Create `src/services/__tests__/signalRService.test.js`:

```javascript
import signalRService from '../signalRService';

// Mock SignalR
jest.mock('@microsoft/signalr', () => ({
	HubConnectionBuilder: jest.fn(() => ({
		withUrl: jest.fn().mockReturnThis(),
		configureLogging: jest.fn().mockReturnThis(),
		withAutomaticReconnect: jest.fn().mockReturnThis(),
		build: jest.fn(() => ({
			start: jest.fn(),
			stop: jest.fn(),
			on: jest.fn(),
			onclose: jest.fn(),
			onreconnecting: jest.fn(),
			onreconnected: jest.fn(),
			invoke: jest.fn(),
		})),
	})),
	LogLevel: {
		Information: 1,
	},
}));

describe('SignalRService', () => {
	beforeEach(() => {
		jest.clearAllMocks();
		localStorage.clear();
	});

	describe('Connection Management', () => {
		it('should connect with valid token', async () => {
			localStorage.setItem('authToken', 'test-token');

			await signalRService.connect();

			expect(signalRService.isConnected).toBe(true);
		});

		it('should not connect without token', async () => {
			await expect(signalRService.connect()).rejects.toThrow(
				'No authentication token found'
			);
		});

		it('should handle connection errors', async () => {
			localStorage.setItem('authToken', 'test-token');

			// Mock connection failure
			const mockConnection = {
				start: jest
					.fn()
					.mockRejectedValue(
						new Error('Connection failed')
					),
				stop: jest.fn(),
				on: jest.fn(),
				onclose: jest.fn(),
				onreconnecting: jest.fn(),
				onreconnected: jest.fn(),
				invoke: jest.fn(),
			};

			jest.mocked(
				require('@microsoft/signalr')
					.HubConnectionBuilder
			).mockReturnValue({
				withUrl: jest.fn().mockReturnThis(),
				configureLogging: jest.fn().mockReturnThis(),
				withAutomaticReconnect: jest
					.fn()
					.mockReturnThis(),
				build: jest
					.fn()
					.mockReturnValue(mockConnection),
			});

			await signalRService.connect();

			expect(signalRService.isReconnecting).toBe(true);
		});
	});

	describe('Event Handling', () => {
		it('should add event listeners', () => {
			const callback = jest.fn();
			signalRService.addEventListener('testEvent', callback);

			expect(signalRService.listeners.has('testEvent')).toBe(
				true
			);
			expect(
				signalRService.listeners.get('testEvent')
			).toContain(callback);
		});

		it('should remove event listeners', () => {
			const callback = jest.fn();
			signalRService.addEventListener('testEvent', callback);
			signalRService.removeEventListener(
				'testEvent',
				callback
			);

			expect(
				signalRService.listeners.get('testEvent')
			).not.toContain(callback);
		});

		it('should notify listeners', () => {
			const callback = jest.fn();
			signalRService.addEventListener('testEvent', callback);
			signalRService.notifyListeners('testEvent', {
				data: 'test',
			});

			expect(callback).toHaveBeenCalledWith({ data: 'test' });
		});
	});
});
```

### 2. Notification Service Tests

Create `src/services/__tests__/notificationService.test.js`:

```javascript
import notificationService from '../notificationService';

// Mock SignalR service
jest.mock('../signalRService', () => ({
	requestNotificationPermission: jest.fn().mockResolvedValue(true),
	addEventListener: jest.fn(),
}));

describe('NotificationService', () => {
	beforeEach(() => {
		notificationService.notifications = [];
		notificationService.unreadCount = 0;
		jest.clearAllMocks();
	});

	describe('Notification Management', () => {
		it('should add notification', () => {
			const notification = {
				id: 1,
				type: 'test',
				title: 'Test',
				message: 'Test message',
				isRead: false,
			};

			notificationService.addNotification(notification);

			expect(notificationService.notifications).toContain(
				notification
			);
			expect(notificationService.unreadCount).toBe(1);
		});

		it('should mark notification as read', () => {
			const notification = {
				id: 1,
				type: 'test',
				title: 'Test',
				message: 'Test message',
				isRead: false,
			};

			notificationService.addNotification(notification);
			notificationService.markAsRead(1);

			expect(
				notificationService.notifications[0].isRead
			).toBe(true);
			expect(notificationService.unreadCount).toBe(0);
		});

		it('should mark all notifications as read', () => {
			const notifications = [
				{ id: 1, isRead: false },
				{ id: 2, isRead: false },
			];

			notifications.forEach((n) =>
				notificationService.addNotification(n)
			);
			notificationService.markAllAsRead();

			expect(notificationService.unreadCount).toBe(0);
			expect(
				notificationService.notifications.every(
					(n) => n.isRead
				)
			).toBe(true);
		});
	});

	describe('Filtering', () => {
		it('should filter notifications by type', () => {
			const notifications = [
				{ id: 1, type: 'request', isRead: false },
				{ id: 2, type: 'assignment', isRead: false },
				{ id: 3, type: 'request', isRead: false },
			];

			notifications.forEach((n) =>
				notificationService.addNotification(n)
			);
			const requestNotifications =
				notificationService.getNotificationsByType(
					'request'
				);

			expect(requestNotifications).toHaveLength(2);
			expect(
				requestNotifications.every(
					(n) => n.type === 'request'
				)
			).toBe(true);
		});

		it('should get unread notifications', () => {
			const notifications = [
				{ id: 1, isRead: false },
				{ id: 2, isRead: true },
				{ id: 3, isRead: false },
			];

			notifications.forEach((n) =>
				notificationService.addNotification(n)
			);
			const unreadNotifications =
				notificationService.getUnreadNotifications();

			expect(unreadNotifications).toHaveLength(2);
			expect(
				unreadNotifications.every((n) => !n.isRead)
			).toBe(true);
		});
	});
});
```

## Performance Optimization

### 1. Notification Throttling

Create `src/utils/notificationThrottler.js`:

```javascript
class NotificationThrottler {
	constructor(throttleMs = 1000) {
		this.throttleMs = throttleMs;
		this.lastNotificationTime = 0;
		this.pendingNotifications = [];
	}

	throttle(notification) {
		const now = Date.now();

		if (now - this.lastNotificationTime >= this.throttleMs) {
			this.lastNotificationTime = now;
			this.processPendingNotifications();
			return notification;
		} else {
			this.pendingNotifications.push(notification);
			return null;
		}
	}

	processPendingNotifications() {
		if (this.pendingNotifications.length > 0) {
			// Process the most recent notification
			const latestNotification =
				this.pendingNotifications.pop();
			this.pendingNotifications = [];
			return latestNotification;
		}
		return null;
	}
}

export default NotificationThrottler;
```

### 2. Memory Management

Create `src/utils/notificationMemoryManager.js`:

```javascript
class NotificationMemoryManager {
	constructor(maxNotifications = 100, maxAgeDays = 7) {
		this.maxNotifications = maxNotifications;
		this.maxAgeDays = maxAgeDays;
	}

	cleanup(notifications) {
		const now = new Date();
		const maxAge = this.maxAgeDays * 24 * 60 * 60 * 1000; // Convert to milliseconds

		// Remove old notifications
		const filteredNotifications = notifications.filter(
			(notification) => {
				const notificationAge =
					now - new Date(notification.timestamp);
				return notificationAge <= maxAge;
			}
		);

		// Limit number of notifications
		const limitedNotifications = filteredNotifications.slice(
			0,
			this.maxNotifications
		);

		return limitedNotifications;
	}

	getMemoryUsage(notifications) {
		const sizeInBytes = JSON.stringify(notifications).length;
		const sizeInKB = (sizeInBytes / 1024).toFixed(2);

		return {
			count: notifications.length,
			sizeInBytes,
			sizeInKB,
			maxNotifications: this.maxNotifications,
			isOverLimit:
				notifications.length > this.maxNotifications,
		};
	}
}

export default NotificationMemoryManager;
```

## Conclusion

This comprehensive React notification system implementation provides:

- **Real-time communication** with SignalR
- **Browser notifications** for important alerts
- **Toast notifications** for immediate feedback
- **State management** with Redux
- **Error handling** and reconnection logic
- **Performance optimization** with throttling and memory management
- **Comprehensive testing** for reliability

The system is designed to work seamlessly with the sales module and provides a robust foundation for real-time notifications in a React web application.
