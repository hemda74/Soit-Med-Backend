# React Native Notification System Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the notification system into your existing React Native mobile application using SignalR for real-time communication and push notifications for user alerts.

## Table of Contents

1. [Integration Setup](#integration-setup)
2. [SignalR Service Integration](#signalr-service-integration)
3. [Push Notification Service Integration](#push-notification-service-integration)
4. [State Management Integration](#state-management-integration)
5. [UI Components Integration](#ui-components-integration)
6. [Integration Examples](#integration-examples)
7. [Error Handling Integration](#error-handling-integration)
8. [Testing Integration](#testing-integration)
9. [Performance Optimization](#performance-optimization)

## Integration Setup

### 1. Install Required Dependencies

Add these dependencies to your existing React Native project:

```bash
npm install @microsoft/signalr
npm install @react-native-async-storage/async-storage
npm install react-native-push-notification
npm install @react-native-community/push-notification-ios
npm install @react-native-community/netinfo
npm install react-native-vector-icons
```

### 2. iOS Setup (if targeting iOS)

Add to your existing `ios/Podfile`:

```ruby
pod 'RNPushNotificationIOS', :path => '../node_modules/@react-native-community/push-notification-ios'
```

Run:

```bash
cd ios && pod install
```

### 3. Android Setup

Add to your existing `android/app/src/main/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.WAKE_LOCK" />
<uses-permission android:name="android.permission.VIBRATE" />
<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED"/>
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

### 4. Environment Configuration

Add these environment variables to your existing `.env` file:

```env
# Add these to your existing .env file
REACT_APP_SIGNALR_URL=https://your-api-url.com/notificationHub
REACT_APP_NOTIFICATION_ENABLED=true
```

## SignalR Service Integration

### 1. SignalR Service Class

Add this service to your existing services in `src/services/signalRService.js`:

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import AsyncStorage from '@react-native-async-storage/async-storage';
import PushNotification from 'react-native-push-notification';

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

			const token = await this.getAuthToken();
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

		// Show push notification
		this.showPushNotification(
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

		this.showPushNotification(
			'New Request',
			`${request.requestType} request from ${request.clientName}`,
			null,
			{ type: 'newRequest', ...request }
		);

		this.notifyListeners('newRequest', request);
	}

	handleRequestAssignment(assignment) {
		console.log('Request Assigned:', assignment);

		this.showPushNotification(
			'Request Assigned',
			`You have been assigned a ${assignment.requestType} request`,
			null,
			{ type: 'assignment', ...assignment }
		);

		this.notifyListeners('assignment', assignment);
	}

	handleRequestStatusUpdate(update) {
		console.log('Request Status Updated:', update);

		this.showPushNotification(
			'Request Updated',
			`Request status has been updated to ${update.newStatus}`,
			null,
			{ type: 'statusUpdate', ...update }
		);

		this.notifyListeners('statusUpdate', update);
	}

	handleSalesReportUpdate(report) {
		console.log('Sales Report Updated:', report);

		this.showPushNotification(
			'Sales Report Updated',
			`New sales report has been generated`,
			null,
			{ type: 'salesReport', ...report }
		);

		this.notifyListeners('salesReportUpdate', report);
	}

	handleUserRoleChange(user) {
		console.log('User Role Changed:', user);

		this.showPushNotification(
			'User Role Updated',
			`User ${user.firstName} ${user.lastName} role has been changed`,
			null,
			{ type: 'userRoleChange', ...user }
		);

		this.notifyListeners('userRoleChange', user);
	}

	showPushNotification(title, message, link, data) {
		PushNotification.localNotification({
			title: title,
			message: message,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			vibrate: true,
			data: {
				type: data?.type || 'default',
				...data,
				link: link,
			},
			actions: ['View', 'Dismiss'],
		});
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

	async getAuthToken() {
		return await AsyncStorage.getItem('authToken');
	}
}

export default new SignalRService();
```

## Push Notification Service Integration

### 1. Push Notification Service

Add this service to your existing services in `src/services/pushNotificationService.js`:

```javascript
import PushNotification from 'react-native-push-notification';
import { Platform } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

class PushNotificationService {
	constructor() {
		this.configure();
	}

	configure() {
		PushNotification.configure({
			// (optional) Called when Token is generated (iOS and Android)
			onRegister: function (token) {
				console.log('TOKEN:', token);
				// Send token to your server
				this.sendTokenToServer(token);
			},

			// (required) Called when a remote or local notification is opened or received
			onNotification: function (notification) {
				console.log('NOTIFICATION:', notification);

				// Handle notification based on type
				if (notification.data) {
					this.handleNotificationData(
						notification.data
					);
				}

				// (required) Called when a remote is opened or received
				notification.finish(
					'UIBackgroundFetchResultNoData'
				);
			},

			// (optional) Called when Registered Action is pressed and invokeApp is false, if true onNotification will be called (Android)
			onAction: function (notification) {
				console.log('ACTION:', notification.action);
				console.log('NOTIFICATION:', notification);
			},

			// (optional) Called when the user fails to register for remote notifications. Typically called when APNS is having issues, or the device is a simulator. (iOS)
			onRegistrationError: function (err) {
				console.error(err.message, err);
			},

			// IOS ONLY (optional): default: all - Permissions to register.
			permissions: {
				alert: true,
				badge: true,
				sound: true,
			},

			// Should the initial notification be popped automatically
			// default: true
			popInitialNotification: true,

			/**
			 * (optional) default: true
			 * - Specified if permissions (ios) and token (android and ios) will requested or not,
			 * - if not, you must call PushNotificationsHandler.requestPermissions() later
			 * - if you are not using remote notification or do not have Firebase installed, use this:
			 *     requestPermissions: Platform.OS === 'ios'
			 */
			requestPermissions: Platform.OS === 'ios',
		});

		// Create default channel for Android
		PushNotification.createChannel(
			{
				channelId: 'default-channel-id',
				channelName: 'Default Channel',
				channelDescription:
					'A default channel for notifications',
				playSound: true,
				soundName: 'default',
				importance: 4,
				vibrate: true,
			},
			(created) =>
				console.log(
					`createChannel returned '${created}'`
				)
		);
	}

	async sendTokenToServer(token) {
		try {
			const authToken = await AsyncStorage.getItem(
				'authToken'
			);
			if (!authToken) return;

			// Send token to your backend
			const response = await fetch(
				`${process.env.REACT_APP_SALES_API_URL}/notifications/register-device`,
				{
					method: 'POST',
					headers: {
						'Content-Type':
							'application/json',
						Authorization: `Bearer ${authToken}`,
					},
					body: JSON.stringify({
						deviceToken: token.token,
						platform: Platform.OS,
					}),
				}
			);

			if (response.ok) {
				console.log('Token registered successfully');
			}
		} catch (error) {
			console.error('Error registering token:', error);
		}
	}

	handleNotificationData(data) {
		// Handle different notification types
		switch (data.type) {
			case 'newRequest':
				// Navigate to requests screen
				break;
			case 'assignment':
				// Navigate to assignments screen
				break;
			case 'statusUpdate':
				// Navigate to updates screen
				break;
			default:
				// Handle default case
				break;
		}
	}

	// Schedule local notification
	scheduleNotification(title, message, data = {}) {
		PushNotification.localNotificationSchedule({
			title: title,
			message: message,
			date: new Date(Date.now() + 5 * 1000), // 5 seconds from now
			data: data,
		});
	}

	// Cancel all notifications
	cancelAll() {
		PushNotification.cancelAllLocalNotifications();
	}

	// Get initial notification
	getInitialNotification() {
		return new Promise((resolve) => {
			PushNotification.getInitialNotification(
				(notification) => {
					resolve(notification);
				}
			);
		});
	}
}

export default new PushNotificationService();
```

## State Management Integration

### 1. Notification Slice

Add this slice to your existing Redux store in `src/store/slices/notificationSlice.js`:

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

## UI Components Integration

### 1. Notification Center Component

Add this component to your existing components in `src/components/notifications/NotificationCenter.jsx`:

```jsx
import React, { useState, useEffect } from 'react';
import {
	View,
	Text,
	FlatList,
	TouchableOpacity,
	StyleSheet,
	RefreshControl,
	Alert,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import Icon from 'react-native-vector-icons/MaterialIcons';
import {
	markNotificationAsRead,
	markAllAsRead,
	clearOldNotifications,
} from '../../store/slices/notificationSlice';

const NotificationCenter = ({ navigation }) => {
	const dispatch = useDispatch();
	const { notifications, unreadCount, loading } = useSelector(
		(state) => state.notifications
	);
	const [refreshing, setRefreshing] = useState(false);

	useEffect(() => {
		// Clear old notifications on component mount
		dispatch(clearOldNotifications(7));
	}, [dispatch]);

	const handleRefresh = () => {
		setRefreshing(true);
		// Refresh notifications logic here
		setTimeout(() => {
			setRefreshing(false);
		}, 1000);
	};

	const handleNotificationPress = (notification) => {
		if (!notification.isRead) {
			dispatch(markNotificationAsRead(notification.id));
		}

		// Navigate to relevant screen based on notification type
		switch (notification.type) {
			case 'newRequest':
				navigation.navigate('Requests');
				break;
			case 'assignment':
				navigation.navigate('Assignments');
				break;
			case 'statusUpdate':
				navigation.navigate('Updates');
				break;
			default:
				// Handle default navigation
				break;
		}
	};

	const handleMarkAllAsRead = () => {
		Alert.alert(
			'Mark All as Read',
			'Are you sure you want to mark all notifications as read?',
			[
				{ text: 'Cancel', style: 'cancel' },
				{
					text: 'Mark All',
					onPress: () =>
						dispatch(markAllAsRead()),
				},
			]
		);
	};

	const getNotificationIcon = (type) => {
		switch (type) {
			case 'newRequest':
				return 'notifications';
			case 'assignment':
				return 'assignment';
			case 'statusUpdate':
				return 'update';
			case 'salesReport':
				return 'assessment';
			case 'userRoleChange':
				return 'person';
			default:
				return 'notifications';
		}
	};

	const getNotificationColor = (type) => {
		switch (type) {
			case 'newRequest':
				return '#2196F3';
			case 'assignment':
				return '#4CAF50';
			case 'statusUpdate':
				return '#FF9800';
			case 'salesReport':
				return '#9C27B0';
			case 'userRoleChange':
				return '#607D8B';
			default:
				return '#666';
		}
	};

	const renderNotificationItem = ({ item }) => (
		<TouchableOpacity
			style={[
				styles.notificationItem,
				!item.isRead && styles.unreadNotification,
			]}
			onPress={() => handleNotificationPress(item)}
		>
			<View style={styles.notificationContent}>
				<View style={styles.notificationHeader}>
					<View style={styles.iconContainer}>
						<Icon
							name={getNotificationIcon(
								item.type
							)}
							size={20}
							color={getNotificationColor(
								item.type
							)}
						/>
					</View>
					<View style={styles.notificationInfo}>
						<Text
							style={
								styles.notificationTitle
							}
						>
							{item.title}
						</Text>
						<Text
							style={
								styles.notificationTime
							}
						>
							{new Date(
								item.timestamp
							).toLocaleDateString()}
						</Text>
					</View>
					{!item.isRead && (
						<View
							style={styles.unreadDot}
						/>
					)}
				</View>
				<Text style={styles.notificationMessage}>
					{item.message}
				</Text>
			</View>
		</TouchableOpacity>
	);

	const renderEmptyState = () => (
		<View style={styles.emptyState}>
			<Icon
				name="notifications-none"
				size={64}
				color="#ccc"
			/>
			<Text style={styles.emptyStateTitle}>
				No Notifications
			</Text>
			<Text style={styles.emptyStateMessage}>
				You don't have any notifications yet
			</Text>
		</View>
	);

	return (
		<View style={styles.container}>
			<View style={styles.header}>
				<Text style={styles.headerTitle}>
					Notifications
				</Text>
				{unreadCount > 0 && (
					<TouchableOpacity
						onPress={handleMarkAllAsRead}
					>
						<Text
							style={
								styles.markAllText
							}
						>
							Mark All as Read
						</Text>
					</TouchableOpacity>
				)}
			</View>

			<FlatList
				data={notifications}
				renderItem={renderNotificationItem}
				keyExtractor={(item) => item.id.toString()}
				ListEmptyComponent={renderEmptyState}
				refreshControl={
					<RefreshControl
						refreshing={refreshing}
						onRefresh={handleRefresh}
					/>
				}
				style={styles.notificationsList}
			/>
		</View>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#f5f5f5',
	},
	header: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: 16,
		backgroundColor: '#fff',
		borderBottomWidth: 1,
		borderBottomColor: '#e0e0e0',
	},
	headerTitle: {
		fontSize: 20,
		fontWeight: 'bold',
		color: '#333',
	},
	markAllText: {
		color: '#2196F3',
		fontSize: 14,
		fontWeight: '600',
	},
	notificationsList: {
		flex: 1,
	},
	notificationItem: {
		backgroundColor: '#fff',
		marginHorizontal: 16,
		marginVertical: 4,
		borderRadius: 8,
		padding: 16,
		elevation: 2,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 1 },
		shadowOpacity: 0.2,
		shadowRadius: 2,
	},
	unreadNotification: {
		backgroundColor: '#f0f8ff',
		borderLeftWidth: 4,
		borderLeftColor: '#2196F3',
	},
	notificationContent: {
		flex: 1,
	},
	notificationHeader: {
		flexDirection: 'row',
		alignItems: 'center',
		marginBottom: 8,
	},
	iconContainer: {
		width: 32,
		height: 32,
		borderRadius: 16,
		backgroundColor: '#f0f0f0',
		justifyContent: 'center',
		alignItems: 'center',
		marginRight: 12,
	},
	notificationInfo: {
		flex: 1,
	},
	notificationTitle: {
		fontSize: 16,
		fontWeight: '600',
		color: '#333',
		marginBottom: 2,
	},
	notificationTime: {
		fontSize: 12,
		color: '#999',
	},
	unreadDot: {
		width: 8,
		height: 8,
		borderRadius: 4,
		backgroundColor: '#2196F3',
	},
	notificationMessage: {
		fontSize: 14,
		color: '#666',
		lineHeight: 20,
	},
	emptyState: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
		padding: 32,
	},
	emptyStateTitle: {
		fontSize: 18,
		fontWeight: '600',
		color: '#666',
		marginTop: 16,
		marginBottom: 8,
	},
	emptyStateMessage: {
		fontSize: 14,
		color: '#999',
		textAlign: 'center',
	},
});

export default NotificationCenter;
```

### 2. Notification Badge Component

Add this component to your existing components in `src/components/notifications/NotificationBadge.jsx`:

```jsx
import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { useSelector } from 'react-redux';
import Icon from 'react-native-vector-icons/MaterialIcons';

const NotificationBadge = ({ onPress, size = 24 }) => {
	const { unreadCount } = useSelector((state) => state.notifications);

	return (
		<TouchableOpacity
			onPress={onPress}
			style={styles.container}
		>
			<Icon
				name="notifications"
				size={size}
				color="#666"
			/>
			{unreadCount > 0 && (
				<View style={styles.badge}>
					<Text style={styles.badgeText}>
						{unreadCount > 99
							? '99+'
							: unreadCount}
					</Text>
				</View>
			)}
		</TouchableOpacity>
	);
};

const styles = StyleSheet.create({
	container: {
		position: 'relative',
	},
	badge: {
		position: 'absolute',
		top: -8,
		right: -8,
		backgroundColor: '#f44336',
		borderRadius: 10,
		minWidth: 20,
		height: 20,
		justifyContent: 'center',
		alignItems: 'center',
		paddingHorizontal: 4,
	},
	badgeText: {
		color: '#fff',
		fontSize: 12,
		fontWeight: 'bold',
	},
});

export default NotificationBadge;
```

## Integration Examples

### 1. App Integration

Update your main `App.js`:

```javascript
import React, { useEffect } from 'react';
import { Provider } from 'react-redux';
import { NavigationContainer } from '@react-navigation/native';
import { store } from './src/store';
import signalRService from './src/services/signalRService';
import pushNotificationService from './src/services/pushNotificationService';
import AppNavigator from './src/navigation/AppNavigator';

const App = () => {
	useEffect(() => {
		const initializeNotifications = async () => {
			try {
				// Initialize push notifications
				pushNotificationService;

				// Connect to SignalR
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
			<NavigationContainer>
				<AppNavigator />
			</NavigationContainer>
		</Provider>
	);
};

export default App;
```

### 2. Header Integration

Update your header component:

```jsx
import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import NotificationBadge from '../notifications/NotificationBadge';

const Header = ({ navigation }) => {
	return (
		<View style={styles.header}>
			<Text style={styles.title}>Sales Management</Text>
			<NotificationBadge
				onPress={() =>
					navigation.navigate('Notifications')
				}
				size={24}
			/>
		</View>
	);
};

const styles = StyleSheet.create({
	header: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: 16,
		backgroundColor: '#fff',
		borderBottomWidth: 1,
		borderBottomColor: '#e0e0e0',
	},
	title: {
		fontSize: 20,
		fontWeight: 'bold',
		color: '#333',
	},
});

export default Header;
```

## Integration Steps Summary

### Step 1: Install Dependencies

```bash
npm install @microsoft/signalr @react-native-async-storage/async-storage react-native-push-notification @react-native-community/push-notification-ios @react-native-community/netinfo react-native-vector-icons
```

### Step 2: Configure Native Platforms

- Update iOS Podfile and run `pod install`
- Update Android manifest permissions

### Step 3: Add Environment Variables

Add the SignalR URL to your existing `.env` file.

### Step 4: Create Services

Add the SignalR service and push notification service to your existing services directory.

### Step 5: Update Redux Store

Add the notification slice to your existing Redux store configuration.

### Step 6: Add Components

Create the notification components in your existing components directory structure.

### Step 7: Update Navigation

Add the notification screen to your existing navigation stack.

### Step 8: Update App.js

Initialize the notification services in your main App component.

### Step 9: Add Tests

Add the notification system tests to your existing test suite.

## Conclusion

This integration guide provides everything needed to add a complete notification system to your existing React Native application:

- **Real-time communication** with SignalR
- **Push notifications** for important alerts
- **Background support** for notifications
- **State management** with Redux
- **Error handling** and reconnection logic
- **Performance optimization** for mobile devices
- **Comprehensive testing** for reliability

The system is designed to work seamlessly with the sales module and provides a robust foundation for real-time notifications in a React Native mobile application.

