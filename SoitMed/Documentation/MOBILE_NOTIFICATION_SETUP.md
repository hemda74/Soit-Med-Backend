# Mobile Notification Setup Guide

## Overview

This guide explains how to set up real-time notifications in your React Native mobile app using SignalR and push notifications for the Sales Support workflow system. This includes notifications for offer requests, deal requests, and status updates.

## Prerequisites

- React Native app with navigation setup
- SignalR client library
- Push notification library (react-native-push-notification or @react-native-async-storage/async-storage)

## Installation

### 1. Install Required Packages

```bash
npm install @microsoft/signalr
npm install @react-native-async-storage/async-storage
npm install react-native-push-notification
npm install @react-native-community/push-notification-ios
npm install @react-native-community/netinfo
```

### 2. iOS Setup (if targeting iOS)

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
```

## Implementation

### 1. SignalR Service

Create `src/services/SignalRService.js`:

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
	}

	async connect() {
		try {
			const token = await AsyncStorage.getItem('authToken');
			if (!token) {
				throw new Error(
					'No authentication token found'
				);
			}

			this.connection = new HubConnectionBuilder()
				.withUrl(
					'https://your-api-url.com/notificationHub',
					{
						accessTokenFactory: () => token,
					}
				)
				.configureLogging(LogLevel.Information)
				.withAutomaticReconnect()
				.build();

			// Set up event handlers
			this.setupEventHandlers();

			// Start connection
			await this.connection.start();
			this.isConnected = true;
			this.reconnectAttempts = 0;

			console.log('SignalR Connected');
		} catch (error) {
			console.error('SignalR Connection Error:', error);
			this.handleReconnect();
		}
	}

	setupEventHandlers() {
		// Handle new notifications
		this.connection.on('ReceiveNotification', (notification) => {
			this.handleNotification(notification);
		});

		// Handle new requests
		this.connection.on('NewRequest', (request) => {
			this.handleNewRequest(request);
		});

		// Handle connection events
		this.connection.onclose((error) => {
			console.log('SignalR Connection Closed:', error);
			this.isConnected = false;
			this.handleReconnect();
		});

		this.connection.onreconnecting((error) => {
			console.log('SignalR Reconnecting:', error);
		});

		this.connection.onreconnected((connectionId) => {
			console.log('SignalR Reconnected:', connectionId);
			this.isConnected = true;
			this.reconnectAttempts = 0;
		});
	}

	handleNotification(notification) {
		console.log('Received Notification:', notification);

		// Show local notification
		PushNotification.localNotification({
			title: notification.title,
			message: notification.message,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			data: {
				type: notification.type,
				requestWorkflowId:
					notification.requestWorkflowId,
				activityLogId: notification.activityLogId,
			},
		});

		// Update app state if needed
		// You can use Redux, Context, or any state management solution
	}

	handleNewRequest(request) {
		console.log('New Request:', request);

		// Show local notification for new requests
		PushNotification.localNotification({
			title: 'New Request',
			message: `${request.requestType} request from ${request.clientName}`,
			playSound: true,
			soundName: 'default',
			priority: 'high',
			importance: 'high',
			data: {
				type: 'NewRequest',
				requestId: request.requestId,
				requestType: request.requestType,
			},
		});
	}

	async handleReconnect() {
		if (this.reconnectAttempts < this.maxReconnectAttempts) {
			this.reconnectAttempts++;
			const delay =
				Math.pow(2, this.reconnectAttempts) * 1000; // Exponential backoff

			console.log(
				`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts})`
			);

			setTimeout(() => {
				this.connect();
			}, delay);
		} else {
			console.error('Max reconnection attempts reached');
		}
	}

	async disconnect() {
		if (this.connection) {
			await this.connection.stop();
			this.isConnected = false;
		}
	}

	// Send message to hub (if needed)
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
}

export default new SignalRService();
```

### 2. Push Notification Setup

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

	sendTokenToServer(token) {
		// Send token to your backend
		// This should be called when the user logs in
		fetch(
			'https://your-api-url.com/api/notifications/register-device',
			{
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${yourAuthToken}`,
				},
				body: JSON.stringify({
					deviceToken: token.token,
					platform: Platform.OS,
				}),
			}
		)
			.then((response) => response.json())
			.then((data) => console.log('Token registered:', data))
			.catch((error) =>
				console.error('Error registering token:', error)
			);
	}

	handleNotificationData(data) {
		// Handle different notification types
		switch (data.type) {
			case 'Request':
				// Navigate to requests screen
				break;
			case 'Assignment':
				// Navigate to assignments screen
				break;
			case 'Update':
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
}

export default new PushNotificationService();
```

### 3. App Integration

Update your main App.js:

```javascript
import React, { useEffect } from 'react';
import { AppState } from 'react-native';
import SignalRService from './src/services/SignalRService';
import PushNotificationService from './src/services/PushNotificationService';

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
      }
    };

    const subscription = AppState.addEventListener('change', handleAppStateChange);

    return () => {
      subscription?.remove();
      SignalRService.disconnect();
    };
  }, []);

  // Your app content
  return (
    // Your app components
  );
};

export default App;
```

### 4. Background Task (iOS)

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
```

## Testing

### 1. Test SignalR Connection

```javascript
// In your component
import SignalRService from './src/services/SignalRService';

const TestConnection = () => {
	const testConnection = () => {
		console.log('Connection status:', SignalRService.isConnected);
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
PushNotificationService.scheduleNotification(
	'Test Notification',
	'This is a test notification',
	{ type: 'test' }
);
```

## Troubleshooting

### Common Issues

1. **Connection fails**: Check authentication token and network connectivity
2. **Notifications not received**: Verify device token registration
3. **Background notifications not working**: Check iOS/Android permissions

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

1. **Token Management**: Store authentication tokens securely
2. **Connection Security**: Use HTTPS for SignalR connections
3. **Data Validation**: Validate all notification data on the client side

## Performance Optimization

1. **Connection Management**: Reconnect only when necessary
2. **Notification Batching**: Batch multiple notifications when possible
3. **Memory Management**: Clean up event listeners properly

## Conclusion

This setup provides a robust notification system that works both when the app is active and in the background. The SignalR connection ensures real-time updates, while push notifications ensure users are notified even when the app is not running.
