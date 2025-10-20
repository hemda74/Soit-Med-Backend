# React Native Mobile Implementation Guide

## Overview

This guide provides complete implementation instructions for the React Native mobile team to integrate the new salesman workflow system with real-time notifications and push notifications that work even when the app is sleeping.

## Prerequisites

- React Native app with navigation setup
- Authentication system with JWT tokens
- State management (Redux/Context/Zustand)
- HTTP client (Axios/Fetch)

## Installation

### 1. Install Required Packages

```bash
npm install @microsoft/signalr
npm install @react-native-async-storage/async-storage
npm install react-native-push-notification
npm install @react-native-community/push-notification-ios
npm install react-native-background-job
npm install @react-native-community/netinfo
```

### 2. iOS Setup

Add to `ios/Podfile`:

```ruby
pod 'RNPushNotificationIOS', :path => '../node_modules/@react-native-community/push-notification-ios'
```

Run:

```bash
cd ios && pod install
```

### 3. Android Setup

Add to `android/app/src/main/AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.WAKE_LOCK" />
<uses-permission android:name="android.permission.VIBRATE" />
<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED"/>
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

## Core Services Implementation

### 1. SignalR Service

Create `src/services/SignalRService.js`:

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import AsyncStorage from '@react-native-async-storage/async-storage';
import PushNotification from 'react-native-push-notification';
import NetInfo from '@react-native-community/netinfo';

class SignalRService {
	constructor() {
		this.connection = null;
		this.isConnected = false;
		this.reconnectAttempts = 0;
		this.maxReconnectAttempts = 5;
		this.reconnectTimer = null;
		this.isReconnecting = false;
	}

	async connect() {
		try {
			if (this.isReconnecting) return;

			const token = await AsyncStorage.getItem('authToken');
			if (!token) {
				throw new Error(
					'No authentication token found'
				);
			}

			// Check network connectivity
			const netInfo = await NetInfo.fetch();
			if (!netInfo.isConnected) {
				throw new Error('No network connection');
			}

			this.connection = new HubConnectionBuilder()
				.withUrl(
					'https://your-api-url.com/notificationHub',
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

			// Send connection success notification
			PushNotification.localNotification({
				title: 'Connected',
				message: 'Real-time notifications are now active',
				playSound: false,
				priority: 'low',
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

		// Handle connection events
		this.connection.onclose((error) => {
			console.log('SignalR Connection Closed:', error);
			this.isConnected = false;
			this.isReconnecting = false;
			this.handleReconnect();
		});

		this.connection.onreconnecting((error) => {
			console.log('SignalR Reconnecting:', error);
			this.isReconnecting = true;
		});

		this.connection.onreconnected((connectionId) => {
			console.log('SignalR Reconnected:', connectionId);
			this.isConnected = true;
			this.isReconnecting = false;
			this.reconnectAttempts = 0;
		});
	}

	handleNotification(message, link, data) {
		console.log('Received Notification:', { message, link, data });

		// Show push notification even when app is in background
		PushNotification.localNotification({
			title: 'New Notification',
			message: message,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			vibrate: true,
			data: {
				type: 'notification',
				link: link,
				...data,
			},
			actions: ['View', 'Dismiss'],
		});

		// Update app state if needed
		// You can dispatch to Redux store or update context
		this.updateAppState('notification', { message, link, data });
	}

	handleNewRequest(request) {
		console.log('New Request:', request);

		// Show push notification for new requests
		PushNotification.localNotification({
			title: 'New Request',
			message: `${request.requestType} request from ${request.clientName}`,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			vibrate: true,
			data: {
				type: 'newRequest',
				requestId: request.requestId,
				requestType: request.requestType,
				clientName: request.clientName,
			},
			actions: ['View Request', 'Dismiss'],
		});

		this.updateAppState('newRequest', request);
	}

	handleRequestAssignment(assignment) {
		console.log('Request Assigned:', assignment);

		PushNotification.localNotification({
			title: 'Request Assigned',
			message: `You have been assigned a ${assignment.requestType} request`,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			vibrate: true,
			data: {
				type: 'assignment',
				assignmentId: assignment.assignmentId,
				requestId: assignment.requestId,
			},
			actions: ['View Assignment', 'Dismiss'],
		});

		this.updateAppState('assignment', assignment);
	}

	handleRequestStatusUpdate(update) {
		console.log('Request Status Updated:', update);

		PushNotification.localNotification({
			title: 'Request Updated',
			message: `Your request status has been updated to ${update.newStatus}`,
			playSound: true,
			soundName: 'default',
			priority: 'medium',
			importance: 'medium',
			vibrate: true,
			data: {
				type: 'statusUpdate',
				requestId: update.requestId,
				newStatus: update.newStatus,
			},
			actions: ['View Update', 'Dismiss'],
		});

		this.updateAppState('statusUpdate', update);
	}

	updateAppState(type, data) {
		// This method should be implemented based on your state management solution
		// Example for Redux:
		// store.dispatch({ type: 'NOTIFICATION_RECEIVED', payload: { type, data } });

		// Example for Context:
		// notificationContext.addNotification({ type, data });

		console.log('App state updated:', { type, data });
	}

	async handleReconnect() {
		if (this.reconnectAttempts >= this.maxReconnectAttempts) {
			console.error('Max reconnection attempts reached');
			this.isReconnecting = false;
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
}

export default new SignalRService();
```

### 2. Push Notification Service

Create `src/services/PushNotificationService.js`:

```javascript
import PushNotification from 'react-native-push-notification';
import { Platform } from 'react-native';

class PushNotificationService {
	constructor() {
		this.configure();
	}

	configure() {
		PushNotification.configure({
			// Called when Token is generated (iOS and Android)
			onRegister: function (token) {
				console.log('TOKEN:', token);
				// Send token to your server
				this.sendTokenToServer(token);
			}.bind(this),

			// Called when a remote or local notification is opened or received
			onNotification: function (notification) {
				console.log('NOTIFICATION:', notification);

				// Handle notification based on type
				if (notification.data) {
					this.handleNotificationData(
						notification.data
					);
				}

				// Required: Called when a remote is opened or received
				notification.finish(
					'UIBackgroundFetchResultNoData'
				);
			}.bind(this),

			// Called when Registered Action is pressed
			onAction: function (notification) {
				console.log('ACTION:', notification.action);
				console.log('NOTIFICATION:', notification);

				if (
					notification.action === 'View' ||
					notification.action ===
						'View Request' ||
					notification.action ===
						'View Assignment' ||
					notification.action === 'View Update'
				) {
					this.handleNotificationTap(
						notification.data
					);
				}
			}.bind(this),

			// Called when the user fails to register for remote notifications
			onRegistrationError: function (err) {
				console.error(err.message, err);
			},

			// Permissions to register
			permissions: {
				alert: true,
				badge: true,
				sound: true,
			},

			// Should the initial notification be popped automatically
			popInitialNotification: true,

			// Request permissions on iOS
			requestPermissions: Platform.OS === 'ios',
		});

		// Create notification channels for Android
		this.createNotificationChannels();
	}

	createNotificationChannels() {
		// High priority channel for important notifications
		PushNotification.createChannel(
			{
				channelId: 'high-priority',
				channelName: 'High Priority Notifications',
				channelDescription:
					'Important notifications that require immediate attention',
				playSound: true,
				soundName: 'default',
				importance: 4,
				vibrate: true,
			},
			(created) =>
				console.log(
					`High priority channel created: ${created}`
				)
		);

		// Medium priority channel for regular notifications
		PushNotification.createChannel(
			{
				channelId: 'medium-priority',
				channelName: 'Medium Priority Notifications',
				channelDescription:
					'Regular notifications and updates',
				playSound: true,
				soundName: 'default',
				importance: 3,
				vibrate: true,
			},
			(created) =>
				console.log(
					`Medium priority channel created: ${created}`
				)
		);

		// Low priority channel for informational notifications
		PushNotification.createChannel(
			{
				channelId: 'low-priority',
				channelName: 'Low Priority Notifications',
				channelDescription:
					'Informational notifications',
				playSound: false,
				soundName: 'default',
				importance: 2,
				vibrate: false,
			},
			(created) =>
				console.log(
					`Low priority channel created: ${created}`
				)
		);
	}

	sendTokenToServer(token) {
		// Send token to your backend
		fetch(
			'https://your-api-url.com/api/notifications/register-device',
			{
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${this.getAuthToken()}`,
				},
				body: JSON.stringify({
					deviceToken: token.token,
					platform: Platform.OS,
					appVersion: '1.0.0', // Get from your app config
				}),
			}
		)
			.then((response) => response.json())
			.then((data) => console.log('Token registered:', data))
			.catch((error) =>
				console.error('Error registering token:', error)
			);
	}

	getAuthToken() {
		// Get auth token from your storage
		// This should be implemented based on your auth system
		return 'your-auth-token';
	}

	handleNotificationData(data) {
		// Handle different notification types
		switch (data.type) {
			case 'notification':
				this.navigateToScreen('Notifications');
				break;
			case 'newRequest':
				this.navigateToScreen('Requests', {
					requestId: data.requestId,
				});
				break;
			case 'assignment':
				this.navigateToScreen('Assignments', {
					assignmentId: data.assignmentId,
				});
				break;
			case 'statusUpdate':
				this.navigateToScreen('Requests', {
					requestId: data.requestId,
				});
				break;
			default:
				this.navigateToScreen('Home');
				break;
		}
	}

	handleNotificationTap(data) {
		// Handle notification tap based on data
		this.handleNotificationData(data);
	}

	navigateToScreen(screenName, params = {}) {
		// Implement navigation based on your navigation system
		// Example for React Navigation:
		// navigation.navigate(screenName, params);
		console.log(`Navigate to ${screenName} with params:`, params);
	}

	// Schedule local notification
	scheduleNotification(title, message, data = {}, delay = 0) {
		PushNotification.localNotificationSchedule({
			title: title,
			message: message,
			date: new Date(Date.now() + delay),
			data: data,
			channelId: 'medium-priority',
		});
	}

	// Send immediate notification
	sendNotification(title, message, data = {}, priority = 'medium') {
		const channelId =
			priority === 'high'
				? 'high-priority'
				: priority === 'medium'
				? 'medium-priority'
				: 'low-priority';

		PushNotification.localNotification({
			title: title,
			message: message,
			data: data,
			channelId: channelId,
			playSound: priority !== 'low',
			vibrate: priority !== 'low',
		});
	}

	// Cancel all notifications
	cancelAll() {
		PushNotification.cancelAllLocalNotifications();
	}

	// Cancel specific notification
	cancelNotification(id) {
		PushNotification.cancelLocalNotifications({ id: id });
	}
}

export default new PushNotificationService();
```

### 3. API Service

Create `src/services/ApiService.js`:

```javascript
import AsyncStorage from '@react-native-async-storage/async-storage';

const BASE_URL = 'https://your-api-url.com/api';

class ApiService {
	constructor() {
		this.baseURL = BASE_URL;
	}

	async getAuthToken() {
		return await AsyncStorage.getItem('authToken');
	}

	async getHeaders() {
		const token = await this.getAuthToken();
		return {
			'Content-Type': 'application/json',
			Authorization: `Bearer ${token}`,
		};
	}

	async request(endpoint, options = {}) {
		const url = `${this.baseURL}${endpoint}`;
		const headers = await this.getHeaders();

		const config = {
			...options,
			headers: {
				...headers,
				...options.headers,
			},
		};

		try {
			const response = await fetch(url, config);
			const data = await response.json();

			if (!response.ok) {
				throw new Error(
					data.message || 'Request failed'
				);
			}

			return data;
		} catch (error) {
			console.error('API Request Error:', error);
			throw error;
		}
	}

	// Salesman API methods
	async sendOfferRequest(requestData) {
		return this.request('/RequestWorkflows', {
			method: 'POST',
			body: JSON.stringify(requestData),
		});
	}

	async sendDealRequest(requestData) {
		return this.request('/RequestWorkflows', {
			method: 'POST',
			body: JSON.stringify(requestData),
		});
	}

	async getMyRequests(status = null, page = 1, pageSize = 20) {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString(),
		});

		if (status) {
			params.append('status', status);
		}

		return this.request(`/RequestWorkflows/sent?${params}`);
	}

	async getRequestDetails(requestId) {
		return this.request(`/RequestWorkflows/${requestId}`);
	}

	// Notification API methods
	async getNotifications(unreadOnly = false) {
		return this.request(`/Notifications?unreadOnly=${unreadOnly}`);
	}

	async markNotificationAsRead(notificationId) {
		return this.request(`/Notifications/${notificationId}/read`, {
			method: 'PUT',
		});
	}

	// User API methods
	async getUsersByRole(role) {
		return this.request(`/Users/by-role/${role}`);
	}

	async getCurrentUser() {
		return this.request('/Users/me');
	}
}

export default new ApiService();
```

## Screen Components

### 1. Send Offer Request Screen

Create `src/screens/SendOfferRequestScreen.js`:

```javascript
import React, { useState, useEffect } from 'react';
import {
	View,
	Text,
	TextInput,
	TouchableOpacity,
	ScrollView,
	Alert,
	StyleSheet,
} from 'react-native';
import ApiService from '../services/ApiService';

const SendOfferRequestScreen = ({ navigation, route }) => {
	const { activityLogId, offerId } = route.params;

	const [formData, setFormData] = useState({
		activityLogId,
		offerId,
		clientName: '',
		clientAddress: '',
		equipmentDetails: '',
		deliveryTerms: {
			deliveryMethod: '',
			deliveryAddress: '',
			city: '',
			state: '',
			postalCode: '',
			country: 'Egypt',
			estimatedDeliveryDays: '',
			specialInstructions: '',
			isUrgent: false,
			preferredDeliveryDate: '',
			contactPerson: '',
			contactPhone: '',
			contactEmail: '',
		},
		paymentTerms: {
			paymentMethod: '',
			totalAmount: '',
			downPayment: '',
			installmentCount: '',
			installmentAmount: '',
			paymentDueDays: '',
			bankName: '',
			accountNumber: '',
			IBAN: '',
			swiftCode: '',
			paymentInstructions: '',
			requiresAdvancePayment: false,
			advancePaymentPercentage: '',
			currency: 'EGP',
		},
		comments: '',
	});

	const [loading, setLoading] = useState(false);
	const [users, setUsers] = useState([]);

	useEffect(() => {
		loadUsers();
	}, []);

	const loadUsers = async () => {
		try {
			const response = await ApiService.getUsersByRole(
				'SalesSupport'
			);
			setUsers(response.data || []);
		} catch (error) {
			console.error('Error loading users:', error);
		}
	};

	const handleInputChange = (field, value) => {
		if (field.includes('.')) {
			const [parent, child] = field.split('.');
			setFormData((prev) => ({
				...prev,
				[parent]: {
					...prev[parent],
					[child]: value,
				},
			}));
		} else {
			setFormData((prev) => ({
				...prev,
				[field]: value,
			}));
		}
	};

	const validateForm = () => {
		if (!formData.clientName.trim()) {
			Alert.alert('Error', 'Client name is required');
			return false;
		}
		if (!formData.clientAddress.trim()) {
			Alert.alert('Error', 'Client address is required');
			return false;
		}
		if (!formData.equipmentDetails.trim()) {
			Alert.alert('Error', 'Equipment details are required');
			return false;
		}
		if (!formData.deliveryTerms.deliveryMethod.trim()) {
			Alert.alert('Error', 'Delivery method is required');
			return false;
		}
		if (!formData.deliveryTerms.deliveryAddress.trim()) {
			Alert.alert('Error', 'Delivery address is required');
			return false;
		}
		if (!formData.paymentTerms.paymentMethod.trim()) {
			Alert.alert('Error', 'Payment method is required');
			return false;
		}
		if (
			!formData.paymentTerms.totalAmount ||
			parseFloat(formData.paymentTerms.totalAmount) <= 0
		) {
			Alert.alert(
				'Error',
				'Total amount must be greater than 0'
			);
			return false;
		}
		return true;
	};

	const handleSubmit = async () => {
		if (!validateForm()) return;

		setLoading(true);
		try {
			// Convert string values to appropriate types
			const submitData = {
				...formData,
				deliveryTerms: {
					...formData.deliveryTerms,
					estimatedDeliveryDays: formData
						.deliveryTerms
						.estimatedDeliveryDays
						? parseInt(
								formData
									.deliveryTerms
									.estimatedDeliveryDays
						  )
						: null,
					isUrgent: formData.deliveryTerms
						.isUrgent,
				},
				paymentTerms: {
					...formData.paymentTerms,
					totalAmount: parseFloat(
						formData.paymentTerms
							.totalAmount
					),
					downPayment: formData.paymentTerms
						.downPayment
						? parseFloat(
								formData
									.paymentTerms
									.downPayment
						  )
						: null,
					installmentCount: formData.paymentTerms
						.installmentCount
						? parseInt(
								formData
									.paymentTerms
									.installmentCount
						  )
						: null,
					installmentAmount: formData.paymentTerms
						.installmentAmount
						? parseFloat(
								formData
									.paymentTerms
									.installmentAmount
						  )
						: null,
					paymentDueDays: formData.paymentTerms
						.paymentDueDays
						? parseInt(
								formData
									.paymentTerms
									.paymentDueDays
						  )
						: null,
					requiresAdvancePayment:
						formData.paymentTerms
							.requiresAdvancePayment,
					advancePaymentPercentage: formData
						.paymentTerms
						.advancePaymentPercentage
						? parseFloat(
								formData
									.paymentTerms
									.advancePaymentPercentage
						  )
						: null,
				},
			};

			await ApiService.sendOfferRequest(submitData);

			Alert.alert(
				'Success',
				'Offer request sent successfully',
				[
					{
						text: 'OK',
						onPress: () =>
							navigation.goBack(),
					},
				]
			);
		} catch (error) {
			Alert.alert(
				'Error',
				error.message || 'Failed to send offer request'
			);
		} finally {
			setLoading(false);
		}
	};

	return (
		<ScrollView style={styles.container}>
			<Text style={styles.title}>Send Offer Request</Text>

			{/* Client Information */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Client Information
				</Text>

				<TextInput
					style={styles.input}
					placeholder="Client Name *"
					value={formData.clientName}
					onChangeText={(value) =>
						handleInputChange(
							'clientName',
							value
						)
					}
				/>

				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Client Address *"
					value={formData.clientAddress}
					onChangeText={(value) =>
						handleInputChange(
							'clientAddress',
							value
						)
					}
					multiline
					numberOfLines={3}
				/>

				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Equipment Details *"
					value={formData.equipmentDetails}
					onChangeText={(value) =>
						handleInputChange(
							'equipmentDetails',
							value
						)
					}
					multiline
					numberOfLines={3}
				/>
			</View>

			{/* Delivery Terms */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Delivery Terms
				</Text>

				<TextInput
					style={styles.input}
					placeholder="Delivery Method *"
					value={
						formData.deliveryTerms
							.deliveryMethod
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.deliveryMethod',
							value
						)
					}
				/>

				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Delivery Address *"
					value={
						formData.deliveryTerms
							.deliveryAddress
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.deliveryAddress',
							value
						)
					}
					multiline
					numberOfLines={2}
				/>

				<View style={styles.row}>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="City"
						value={
							formData.deliveryTerms
								.city
						}
						onChangeText={(value) =>
							handleInputChange(
								'deliveryTerms.city',
								value
							)
						}
					/>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="State"
						value={
							formData.deliveryTerms
								.state
						}
						onChangeText={(value) =>
							handleInputChange(
								'deliveryTerms.state',
								value
							)
						}
					/>
				</View>

				<View style={styles.row}>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="Postal Code"
						value={
							formData.deliveryTerms
								.postalCode
						}
						onChangeText={(value) =>
							handleInputChange(
								'deliveryTerms.postalCode',
								value
							)
						}
					/>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="Country"
						value={
							formData.deliveryTerms
								.country
						}
						onChangeText={(value) =>
							handleInputChange(
								'deliveryTerms.country',
								value
							)
						}
					/>
				</View>

				<TextInput
					style={styles.input}
					placeholder="Estimated Delivery Days"
					value={
						formData.deliveryTerms
							.estimatedDeliveryDays
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.estimatedDeliveryDays',
							value
						)
					}
					keyboardType="numeric"
				/>

				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Special Instructions"
					value={
						formData.deliveryTerms
							.specialInstructions
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.specialInstructions',
							value
						)
					}
					multiline
					numberOfLines={2}
				/>

				<TextInput
					style={styles.input}
					placeholder="Contact Person"
					value={
						formData.deliveryTerms
							.contactPerson
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.contactPerson',
							value
						)
					}
				/>

				<TextInput
					style={styles.input}
					placeholder="Contact Phone"
					value={
						formData.deliveryTerms
							.contactPhone
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.contactPhone',
							value
						)
					}
					keyboardType="phone-pad"
				/>

				<TextInput
					style={styles.input}
					placeholder="Contact Email"
					value={
						formData.deliveryTerms
							.contactEmail
					}
					onChangeText={(value) =>
						handleInputChange(
							'deliveryTerms.contactEmail',
							value
						)
					}
					keyboardType="email-address"
					autoCapitalize="none"
				/>
			</View>

			{/* Payment Terms */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Payment Terms
				</Text>

				<TextInput
					style={styles.input}
					placeholder="Payment Method *"
					value={
						formData.paymentTerms
							.paymentMethod
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.paymentMethod',
							value
						)
					}
				/>

				<TextInput
					style={styles.input}
					placeholder="Total Amount *"
					value={
						formData.paymentTerms
							.totalAmount
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.totalAmount',
							value
						)
					}
					keyboardType="numeric"
				/>

				<TextInput
					style={styles.input}
					placeholder="Down Payment"
					value={
						formData.paymentTerms
							.downPayment
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.downPayment',
							value
						)
					}
					keyboardType="numeric"
				/>

				<View style={styles.row}>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="Installment Count"
						value={
							formData.paymentTerms
								.installmentCount
						}
						onChangeText={(value) =>
							handleInputChange(
								'paymentTerms.installmentCount',
								value
							)
						}
						keyboardType="numeric"
					/>
					<TextInput
						style={[
							styles.input,
							styles.halfInput,
						]}
						placeholder="Installment Amount"
						value={
							formData.paymentTerms
								.installmentAmount
						}
						onChangeText={(value) =>
							handleInputChange(
								'paymentTerms.installmentAmount',
								value
							)
						}
						keyboardType="numeric"
					/>
				</View>

				<TextInput
					style={styles.input}
					placeholder="Payment Due Days"
					value={
						formData.paymentTerms
							.paymentDueDays
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.paymentDueDays',
							value
						)
					}
					keyboardType="numeric"
				/>

				<TextInput
					style={styles.input}
					placeholder="Bank Name"
					value={formData.paymentTerms.bankName}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.bankName',
							value
						)
					}
				/>

				<TextInput
					style={styles.input}
					placeholder="Account Number"
					value={
						formData.paymentTerms
							.accountNumber
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.accountNumber',
							value
						)
					}
				/>

				<TextInput
					style={styles.input}
					placeholder="IBAN"
					value={formData.paymentTerms.IBAN}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.IBAN',
							value
						)
					}
				/>

				<TextInput
					style={styles.input}
					placeholder="SWIFT Code"
					value={formData.paymentTerms.swiftCode}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.swiftCode',
							value
						)
					}
				/>

				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Payment Instructions"
					value={
						formData.paymentTerms
							.paymentInstructions
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.paymentInstructions',
							value
						)
					}
					multiline
					numberOfLines={2}
				/>

				<TextInput
					style={styles.input}
					placeholder="Advance Payment Percentage"
					value={
						formData.paymentTerms
							.advancePaymentPercentage
					}
					onChangeText={(value) =>
						handleInputChange(
							'paymentTerms.advancePaymentPercentage',
							value
						)
					}
					keyboardType="numeric"
				/>
			</View>

			{/* Comments */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Additional Comments
				</Text>
				<TextInput
					style={[styles.input, styles.textArea]}
					placeholder="Comments"
					value={formData.comments}
					onChangeText={(value) =>
						handleInputChange(
							'comments',
							value
						)
					}
					multiline
					numberOfLines={3}
				/>
			</View>

			<TouchableOpacity
				style={[
					styles.submitButton,
					loading && styles.disabledButton,
				]}
				onPress={handleSubmit}
				disabled={loading}
			>
				<Text style={styles.submitButtonText}>
					{loading
						? 'Sending...'
						: 'Send Offer Request'}
				</Text>
			</TouchableOpacity>
		</ScrollView>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		padding: 16,
		backgroundColor: '#f5f5f5',
	},
	title: {
		fontSize: 24,
		fontWeight: 'bold',
		marginBottom: 20,
		textAlign: 'center',
	},
	section: {
		backgroundColor: 'white',
		padding: 16,
		marginBottom: 16,
		borderRadius: 8,
		elevation: 2,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
	},
	sectionTitle: {
		fontSize: 18,
		fontWeight: 'bold',
		marginBottom: 12,
		color: '#333',
	},
	input: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		marginBottom: 12,
		fontSize: 16,
		backgroundColor: 'white',
	},
	textArea: {
		height: 80,
		textAlignVertical: 'top',
	},
	row: {
		flexDirection: 'row',
		justifyContent: 'space-between',
	},
	halfInput: {
		width: '48%',
	},
	submitButton: {
		backgroundColor: '#007bff',
		padding: 16,
		borderRadius: 8,
		alignItems: 'center',
		marginTop: 20,
	},
	disabledButton: {
		backgroundColor: '#ccc',
	},
	submitButtonText: {
		color: 'white',
		fontSize: 18,
		fontWeight: 'bold',
	},
});

export default SendOfferRequestScreen;
```

### 2. My Requests Screen

Create `src/screens/MyRequestsScreen.js`:

```javascript
import React, { useState, useEffect, useCallback } from 'react';
import {
	View,
	Text,
	FlatList,
	TouchableOpacity,
	RefreshControl,
	StyleSheet,
	Alert,
} from 'react-native';
import ApiService from '../services/ApiService';

const MyRequestsScreen = ({ navigation }) => {
	const [requests, setRequests] = useState([]);
	const [loading, setLoading] = useState(true);
	const [refreshing, setRefreshing] = useState(false);
	const [statusFilter, setStatusFilter] = useState(null);

	const statusOptions = [
		{ label: 'All', value: null },
		{ label: 'Pending', value: 'Pending' },
		{ label: 'Assigned', value: 'Assigned' },
		{ label: 'In Progress', value: 'InProgress' },
		{ label: 'Completed', value: 'Completed' },
		{ label: 'Rejected', value: 'Rejected' },
		{ label: 'Cancelled', value: 'Cancelled' },
	];

	useEffect(() => {
		loadRequests();
	}, [statusFilter]);

	const loadRequests = async () => {
		try {
			setLoading(true);
			const response = await ApiService.getMyRequests(
				statusFilter
			);
			setRequests(response.data || []);
		} catch (error) {
			Alert.alert('Error', 'Failed to load requests');
			console.error('Error loading requests:', error);
		} finally {
			setLoading(false);
		}
	};

	const onRefresh = useCallback(async () => {
		setRefreshing(true);
		await loadRequests();
		setRefreshing(false);
	}, [statusFilter]);

	const getStatusColor = (status) => {
		switch (status) {
			case 'Pending':
				return '#ffc107';
			case 'Assigned':
				return '#17a2b8';
			case 'InProgress':
				return '#007bff';
			case 'Completed':
				return '#28a745';
			case 'Rejected':
				return '#dc3545';
			case 'Cancelled':
				return '#6c757d';
			default:
				return '#6c757d';
		}
	};

	const formatDate = (dateString) => {
		const date = new Date(dateString);
		return (
			date.toLocaleDateString() +
			' ' +
			date.toLocaleTimeString()
		);
	};

	const renderRequest = ({ item }) => (
		<TouchableOpacity
			style={styles.requestCard}
			onPress={() =>
				navigation.navigate('RequestDetails', {
					requestId: item.id,
				})
			}
		>
			<View style={styles.requestHeader}>
				<Text style={styles.requestType}>
					{item.requestType}
				</Text>
				<View
					style={[
						styles.statusBadge,
						{
							backgroundColor:
								getStatusColor(
									item.status
								),
						},
					]}
				>
					<Text style={styles.statusText}>
						{item.statusName}
					</Text>
				</View>
			</View>

			<Text style={styles.clientName}>{item.clientName}</Text>
			<Text style={styles.clientAddress}>
				{item.clientAddress}
			</Text>

			{item.equipmentDetails && (
				<Text
					style={styles.equipmentDetails}
					numberOfLines={2}
				>
					{item.equipmentDetails}
				</Text>
			)}

			<View style={styles.requestFooter}>
				<Text style={styles.createdDate}>
					Created: {formatDate(item.createdAt)}
				</Text>
				{item.assignedAt && (
					<Text style={styles.assignedDate}>
						Assigned:{' '}
						{formatDate(item.assignedAt)}
					</Text>
				)}
			</View>
		</TouchableOpacity>
	);

	const renderStatusFilter = () => (
		<View style={styles.filterContainer}>
			<Text style={styles.filterTitle}>
				Filter by Status:
			</Text>
			<View style={styles.filterButtons}>
				{statusOptions.map((option) => (
					<TouchableOpacity
						key={option.value}
						style={[
							styles.filterButton,
							statusFilter ===
								option.value &&
								styles.activeFilterButton,
						]}
						onPress={() =>
							setStatusFilter(
								option.value
							)
						}
					>
						<Text
							style={[
								styles.filterButtonText,
								statusFilter ===
									option.value &&
									styles.activeFilterButtonText,
							]}
						>
							{option.label}
						</Text>
					</TouchableOpacity>
				))}
			</View>
		</View>
	);

	if (loading) {
		return (
			<View style={styles.centerContainer}>
				<Text>Loading requests...</Text>
			</View>
		);
	}

	return (
		<View style={styles.container}>
			{renderStatusFilter()}

			<FlatList
				data={requests}
				renderItem={renderRequest}
				keyExtractor={(item) => item.id.toString()}
				refreshControl={
					<RefreshControl
						refreshing={refreshing}
						onRefresh={onRefresh}
					/>
				}
				ListEmptyComponent={
					<View style={styles.emptyContainer}>
						<Text style={styles.emptyText}>
							No requests found
						</Text>
					</View>
				}
				contentContainerStyle={styles.listContainer}
			/>
		</View>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#f5f5f5',
	},
	centerContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
	},
	filterContainer: {
		backgroundColor: 'white',
		padding: 16,
		marginBottom: 8,
	},
	filterTitle: {
		fontSize: 16,
		fontWeight: 'bold',
		marginBottom: 8,
	},
	filterButtons: {
		flexDirection: 'row',
		flexWrap: 'wrap',
	},
	filterButton: {
		paddingHorizontal: 12,
		paddingVertical: 6,
		marginRight: 8,
		marginBottom: 8,
		borderRadius: 16,
		borderWidth: 1,
		borderColor: '#ddd',
		backgroundColor: 'white',
	},
	activeFilterButton: {
		backgroundColor: '#007bff',
		borderColor: '#007bff',
	},
	filterButtonText: {
		fontSize: 14,
		color: '#333',
	},
	activeFilterButtonText: {
		color: 'white',
	},
	listContainer: {
		padding: 16,
	},
	requestCard: {
		backgroundColor: 'white',
		padding: 16,
		marginBottom: 12,
		borderRadius: 8,
		elevation: 2,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
	},
	requestHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		marginBottom: 8,
	},
	requestType: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#333',
	},
	statusBadge: {
		paddingHorizontal: 8,
		paddingVertical: 4,
		borderRadius: 12,
	},
	statusText: {
		color: 'white',
		fontSize: 12,
		fontWeight: 'bold',
	},
	clientName: {
		fontSize: 14,
		fontWeight: '600',
		color: '#333',
		marginBottom: 4,
	},
	clientAddress: {
		fontSize: 12,
		color: '#666',
		marginBottom: 8,
	},
	equipmentDetails: {
		fontSize: 12,
		color: '#666',
		marginBottom: 8,
	},
	requestFooter: {
		flexDirection: 'row',
		justifyContent: 'space-between',
	},
	createdDate: {
		fontSize: 10,
		color: '#999',
	},
	assignedDate: {
		fontSize: 10,
		color: '#999',
	},
	emptyContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
		paddingVertical: 40,
	},
	emptyText: {
		fontSize: 16,
		color: '#666',
	},
});

export default MyRequestsScreen;
```

## App Integration

### 1. Main App Component

Update your main `App.js`:

```javascript
import React, { useEffect } from 'react';
import { AppState, Platform } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import SignalRService from './src/services/SignalRService';
import PushNotificationService from './src/services/PushNotificationService';

// Import your screens
import HomeScreen from './src/screens/HomeScreen';
import SendOfferRequestScreen from './src/screens/SendOfferRequestScreen';
import MyRequestsScreen from './src/screens/MyRequestsScreen';
import RequestDetailsScreen from './src/screens/RequestDetailsScreen';

const Stack = createStackNavigator();

const App = () => {
	useEffect(() => {
		// Initialize push notifications
		PushNotificationService;

		// Connect to SignalR when app starts
		SignalRService.connect();

		// Handle app state changes
		const handleAppStateChange = (nextAppState) => {
			if (nextAppState === 'active') {
				// App has come to the foreground
				if (!SignalRService.isConnected) {
					SignalRService.connect();
				}
			} else if (nextAppState === 'background') {
				// App has gone to the background
				// Keep connection alive for notifications
				console.log(
					'App went to background, keeping SignalR connection alive'
				);
			}
		};

		const subscription = AppState.addEventListener(
			'change',
			handleAppStateChange
		);

		return () => {
			subscription?.remove();
			SignalRService.disconnect();
		};
	}, []);

	return (
		<NavigationContainer>
			<Stack.Navigator initialRouteName="Home">
				<Stack.Screen
					name="Home"
					component={HomeScreen}
					options={{ title: 'Sales Dashboard' }}
				/>
				<Stack.Screen
					name="SendOfferRequest"
					component={SendOfferRequestScreen}
					options={{
						title: 'Send Offer Request',
					}}
				/>
				<Stack.Screen
					name="MyRequests"
					component={MyRequestsScreen}
					options={{ title: 'My Requests' }}
				/>
				<Stack.Screen
					name="RequestDetails"
					component={RequestDetailsScreen}
					options={{ title: 'Request Details' }}
				/>
			</Stack.Navigator>
		</NavigationContainer>
	);
};

export default App;
```

### 2. Background Task Setup (iOS)

For iOS background notifications, add to `ios/YourApp/AppDelegate.m`:

```objc
#import <UserNotifications/UserNotifications.h>

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
  // ... existing code ...

  // Request notification permissions
  UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
  center.delegate = self;

  [center requestAuthorizationWithOptions:(UNAuthorizationOptionSound | UNAuthorizationOptionAlert | UNAuthorizationOptionBadge) completionHandler:^(BOOL granted, NSError * _Nullable error) {
    if (granted) {
      dispatch_async(dispatch_get_main_queue(), ^{
        [[UIApplication sharedApplication] registerForRemoteNotifications];
      });
    }
  }];

  return YES;
}

// Handle notification when app is in background
- (void)userNotificationCenter:(UNUserNotificationCenter *)center didReceiveNotificationResponse:(UNNotificationResponse *)response withCompletionHandler:(void (^)(void))completionHandler {
  // Handle notification tap
  completionHandler();
}

// Handle notification when app is in foreground
- (void)userNotificationCenter:(UNUserNotificationCenter *)center willPresentNotification:(UNNotification *)notification withCompletionHandler:(void (^)(UNNotificationPresentationOptions))completionHandler {
  // Show notification even when app is in foreground
  completionHandler(UNNotificationPresentationOptionAlert | UNNotificationPresentationOptionSound);
}
```

## Testing

### 1. Test SignalR Connection

```javascript
// In your component
import SignalRService from './src/services/SignalRService';

const TestConnection = () => {
	const testConnection = () => {
		const status = SignalRService.getConnectionStatus();
		console.log('Connection status:', status);
		Alert.alert(
			'Connection Status',
			JSON.stringify(status, null, 2)
		);
	};

	return (
		<Button
			title="Test Connection"
			onPress={testConnection}
		/>
	);
};
```

### 2. Test Push Notifications

```javascript
// Test local notification
PushNotificationService.sendNotification(
	'Test Notification',
	'This is a test notification',
	{ type: 'test' },
	'high'
);
```

## Troubleshooting

### Common Issues

1. **Connection fails**: Check authentication token and network connectivity
2. **Notifications not received**: Verify device token registration and permissions
3. **Background notifications not working**: Check iOS/Android permissions and background modes
4. **SignalR reconnection issues**: Check network stability and token validity

### Debug Mode

Enable debug logging:

```javascript
// In SignalRService.js
.withUrl('https://your-api-url.com/notificationHub', {
  accessTokenFactory: () => token,
  logLevel: LogLevel.Debug, // Add this for debugging
})
```

## Security Considerations

1. **Token Management**: Store authentication tokens securely using Keychain (iOS) or Keystore (Android)
2. **Connection Security**: Use HTTPS for SignalR connections
3. **Data Validation**: Validate all notification data on the client side
4. **Background Security**: Ensure sensitive data is not logged in background tasks

## Performance Optimization

1. **Connection Management**: Reconnect only when necessary
2. **Notification Batching**: Batch multiple notifications when possible
3. **Memory Management**: Clean up event listeners properly
4. **Background Efficiency**: Minimize background processing to preserve battery

## Conclusion

This implementation provides a robust notification system that works both when the app is active and in the background. The SignalR connection ensures real-time updates, while push notifications ensure users are notified even when the app is not running, similar to WhatsApp's behavior.
