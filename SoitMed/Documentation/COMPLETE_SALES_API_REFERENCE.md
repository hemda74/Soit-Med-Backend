# Complete Sales Module API Reference - React Native Integration

## ⚠️ IMPORTANT: Enum Values Must Be Numeric

**CRITICAL:** All enum values must be sent as **numbers**, not strings!

- `interactionType`: 1 = Visit, 2 = FollowUp (NOT "Visit", "FollowUp")
- `clientType`: 1 = A, 2 = B, 3 = C, 4 = D (NOT "A", "B", "C", "D")
- `result`: 1 = Interested, 2 = NotInterested (NOT "Interested", "NotInterested")
- `reason`: 1 = Cash, 2 = Price, 3 = Need, 4 = Other (NOT "Cash", "Price", etc.)

## Base Information

- **Authentication**: JWT Bearer Token
- **Content-Type**: `application/json`
- **Base URL**: `http://localhost:5117` (Development) / `https://api.soitmed.com` (Production)

## Authentication Header

```
Authorization: Bearer YOUR_JWT_TOKEN
```

## React Native Setup

### Installation

```bash
npm install axios @react-native-async-storage/async-storage
# or
yarn add axios @react-native-async-storage/async-storage
```

### API Client Configuration

```javascript
// apiClient.js
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

const API_BASE_URL = __DEV__
	? 'http://localhost:5117'
	: 'https://api.soitmed.com';

const apiClient = axios.create({
	baseURL: API_BASE_URL,
	timeout: 10000,
	headers: {
		'Content-Type': 'application/json',
	},
});

// Request interceptor to add auth token
apiClient.interceptors.request.use(
	async (config) => {
		try {
			const token = await AsyncStorage.getItem('jwt_token');
			if (token) {
				config.headers.Authorization = `Bearer ${token}`;
			}
		} catch (error) {
			console.error('Error getting token:', error);
		}
		return config;
	},
	(error) => {
		return Promise.reject(error);
	}
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
	(response) => response,
	async (error) => {
		if (error.response?.status === 401) {
			// Token expired or invalid
			await AsyncStorage.removeItem('jwt_token');
			// Navigate to login screen
		}
		return Promise.reject(error);
	}
);

export default apiClient;
```

---

## Activities Management APIs

### 1. Create Activity

**POST** `/api/activities/tasks/{taskId}/activities`

**Authorization**: `Salesman`

**Path Parameters**:

- `taskId` (int): ID of the weekly plan task

**Request Body**:

```json
{
	"interactionType": 1,
	"clientType": 1,
	"result": 1,
	"reason": null,
	"comment": "Client showed strong interest in MRI machine",
	"dealInfo": {
		"dealValue": 750000.0,
		"expectedCloseDate": "2024-02-15T00:00:00Z"
	},
	"offerInfo": null
}
```

**Enum Values**:

- `interactionType`: 1 = Visit, 2 = FollowUp
- `clientType`: 1 = A, 2 = B, 3 = C, 4 = D
- `result`: 1 = Interested, 2 = NotInterested
- `reason`: 1 = Cash, 2 = Price, 3 = Need, 4 = Other (required if result = 2)

**Response** (200 OK):

```json
{
	"success": true,
	"message": "Activity created successfully",
	"data": {
		"id": 1,
		"planTaskId": 123,
		"userId": "user123",
		"userName": "Ahmed Mohamed",
		"interactionType": 1,
		"interactionTypeName": "Visit",
		"clientType": 1,
		"clientTypeName": "A",
		"result": 1,
		"resultName": "Interested",
		"reason": null,
		"reasonName": null,
		"comment": "Client showed strong interest in MRI machine",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z",
		"deal": {
			"id": 1,
			"activityLogId": 1,
			"userId": "user123",
			"dealValue": 750000.0,
			"status": 1,
			"statusName": "Pending",
			"expectedCloseDate": "2024-02-15T00:00:00Z",
			"createdAt": "2024-01-15T10:30:00Z",
			"updatedAt": "2024-01-15T10:30:00Z"
		},
		"offer": null
	}
}
```

**React Native Implementation**:

```javascript
// services/ActivityService.js
import apiClient from '../apiClient';

export const ActivityService = {
	// Create a new activity
	async createActivity(taskId, activityData) {
		try {
			const response = await apiClient.post(
				`/api/activities/tasks/${taskId}/activities`,
				activityData
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	// Get all activities for current user
	async getActivities(startDate = null, endDate = null) {
		try {
			const params = new URLSearchParams();
			if (startDate)
				params.append(
					'startDate',
					startDate.toISOString()
				);
			if (endDate)
				params.append('endDate', endDate.toISOString());

			const response = await apiClient.get(
				`/api/activities?${params}`
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	// Get single activity by ID
	async getActivity(activityId) {
		try {
			const response = await apiClient.get(
				`/api/activities/${activityId}`
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	handleError(error) {
		if (error.response) {
			return new Error(
				error.response.data.message || 'API Error'
			);
		} else if (error.request) {
			return new Error(
				'Network Error - Please check your connection'
			);
		} else {
			return new Error('An unexpected error occurred');
		}
	},
};
```

**React Native Component Example**:

```javascript
// components/CreateActivityForm.js
import React, { useState } from 'react';
import {
	View,
	Text,
	TextInput,
	TouchableOpacity,
	Alert,
	StyleSheet,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { ActivityService } from '../services/ActivityService';

const CreateActivityForm = ({ taskId, onActivityCreated }) => {
	const [formData, setFormData] = useState({
		interactionType: 1,
		clientType: 1,
		result: 1,
		reason: null,
		comment: '',
		dealValue: '',
		expectedCloseDate: '',
		offerDetails: '',
		documentUrl: '',
	});
	const [loading, setLoading] = useState(false);

	const handleSubmit = async () => {
		try {
			setLoading(true);

			// Validate required fields
			if (!formData.comment.trim()) {
				Alert.alert('Error', 'Comment is required');
				return;
			}

			if (
				formData.result === 1 &&
				!formData.dealValue &&
				!formData.offerDetails
			) {
				Alert.alert(
					'Error',
					'Either deal value or offer details is required when client is interested'
				);
				return;
			}

			if (formData.result === 2 && !formData.reason) {
				Alert.alert(
					'Error',
					'Reason is required when client is not interested'
				);
				return;
			}

			// Prepare request data - CRITICAL: Use numeric enum values, NOT strings!
			const requestData = {
				interactionType: formData.interactionType, // Must be 1 or 2, NOT "Visit" or "FollowUp"
				clientType: formData.clientType, // Must be 1, 2, 3, or 4, NOT "A", "B", "C", "D"
				result: formData.result, // Must be 1 or 2, NOT "Interested" or "NotInterested"
				reason: formData.reason, // Must be 1, 2, 3, or 4, NOT "Cash", "Price", etc.
				comment: formData.comment,
				dealInfo: formData.dealValue
					? {
							dealValue: parseFloat(
								formData.dealValue
							),
							expectedCloseDate:
								formData.expectedCloseDate
									? new Date(
											formData.expectedCloseDate
									  ).toISOString()
									: null,
					  }
					: null,
				offerInfo: formData.offerDetails
					? {
							offerDetails:
								formData.offerDetails,
							documentUrl:
								formData.documentUrl ||
								null,
					  }
					: null,
			};

			const result = await ActivityService.createActivity(
				taskId,
				requestData
			);

			Alert.alert('Success', 'Activity created successfully');
			onActivityCreated?.(result.data);

			// Reset form
			setFormData({
				interactionType: 1,
				clientType: 1,
				result: 1,
				reason: null,
				comment: '',
				dealValue: '',
				expectedCloseDate: '',
				offerDetails: '',
				documentUrl: '',
			});
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	return (
		<View style={styles.container}>
			<Text style={styles.title}>Create New Activity</Text>

			{/* Interaction Type */}
			<View style={styles.inputGroup}>
				<Text style={styles.label}>
					Interaction Type
				</Text>
				<Picker
					selectedValue={formData.interactionType}
					onValueChange={(value) =>
						setFormData({
							...formData,
							interactionType: value,
						})
					}
					style={styles.picker}
				>
					<Picker.Item
						label="Visit"
						value={1}
					/>
					<Picker.Item
						label="Follow Up"
						value={2}
					/>
				</Picker>
			</View>

			{/* Client Type */}
			<View style={styles.inputGroup}>
				<Text style={styles.label}>Client Type</Text>
				<Picker
					selectedValue={formData.clientType}
					onValueChange={(value) =>
						setFormData({
							...formData,
							clientType: value,
						})
					}
					style={styles.picker}
				>
					<Picker.Item
						label="A (High Value)"
						value={1}
					/>
					<Picker.Item
						label="B (Medium Value)"
						value={2}
					/>
					<Picker.Item
						label="C (Low Value)"
						value={3}
					/>
					<Picker.Item
						label="D (Very Low Value)"
						value={4}
					/>
				</Picker>
			</View>

			{/* Result */}
			<View style={styles.inputGroup}>
				<Text style={styles.label}>Result</Text>
				<Picker
					selectedValue={formData.result}
					onValueChange={(value) =>
						setFormData({
							...formData,
							result: value,
							reason: null,
						})
					}
					style={styles.picker}
				>
					<Picker.Item
						label="Interested"
						value={1}
					/>
					<Picker.Item
						label="Not Interested"
						value={2}
					/>
				</Picker>
			</View>

			{/* Reason (only if Not Interested) */}
			{formData.result === 2 && (
				<View style={styles.inputGroup}>
					<Text style={styles.label}>
						Reason for Not Interested
					</Text>
					<Picker
						selectedValue={formData.reason}
						onValueChange={(value) =>
							setFormData({
								...formData,
								reason: value,
							})
						}
						style={styles.picker}
					>
						<Picker.Item
							label="Cash/Budget Issues"
							value={1}
						/>
						<Picker.Item
							label="Price Too High"
							value={2}
						/>
						<Picker.Item
							label="No Current Need"
							value={3}
						/>
						<Picker.Item
							label="Other"
							value={4}
						/>
					</Picker>
				</View>
			)}

			{/* Comment */}
			<View style={styles.inputGroup}>
				<Text style={styles.label}>Comment *</Text>
				<TextInput
					style={styles.textArea}
					value={formData.comment}
					onChangeText={(text) =>
						setFormData({
							...formData,
							comment: text,
						})
					}
					placeholder="Enter activity details..."
					multiline
					numberOfLines={4}
				/>
			</View>

			{/* Deal Info (only if Interested) */}
			{formData.result === 1 && (
				<>
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Deal Value (Optional)
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.dealValue
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									dealValue: text,
								})
							}
							placeholder="Enter deal value"
							keyboardType="numeric"
						/>
					</View>

					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Expected Close Date
							(Optional)
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.expectedCloseDate
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									expectedCloseDate:
										text,
								})
							}
							placeholder="YYYY-MM-DD"
						/>
					</View>

					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Offer Details (Optional)
						</Text>
						<TextInput
							style={styles.textArea}
							value={
								formData.offerDetails
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									offerDetails:
										text,
								})
							}
							placeholder="Enter offer details..."
							multiline
							numberOfLines={3}
						/>
					</View>

					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Document URL (Optional)
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.documentUrl
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									documentUrl:
										text,
								})
							}
							placeholder="https://example.com/document.pdf"
							keyboardType="url"
						/>
					</View>
				</>
			)}

			<TouchableOpacity
				style={[
					styles.button,
					loading && styles.buttonDisabled,
				]}
				onPress={handleSubmit}
				disabled={loading}
			>
				<Text style={styles.buttonText}>
					{loading
						? 'Creating...'
						: 'Create Activity'}
				</Text>
			</TouchableOpacity>
		</View>
	);
};

const styles = StyleSheet.create({
	container: {
		padding: 20,
		backgroundColor: '#fff',
	},
	title: {
		fontSize: 24,
		fontWeight: 'bold',
		marginBottom: 20,
		textAlign: 'center',
	},
	inputGroup: {
		marginBottom: 15,
	},
	label: {
		fontSize: 16,
		fontWeight: '600',
		marginBottom: 5,
		color: '#333',
	},
	input: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		fontSize: 16,
	},
	textArea: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		fontSize: 16,
		textAlignVertical: 'top',
	},
	picker: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
	},
	button: {
		backgroundColor: '#007AFF',
		padding: 15,
		borderRadius: 8,
		alignItems: 'center',
		marginTop: 20,
	},
	buttonDisabled: {
		backgroundColor: '#ccc',
	},
	buttonText: {
		color: '#fff',
		fontSize: 16,
		fontWeight: '600',
	},
});

export default CreateActivityForm;
```

### 2. Get User Activities

**GET** `/api/activities?startDate={date}&endDate={date}`

**Authorization**: `Salesman`

**Query Parameters**:

- `startDate` (optional): Start date in ISO format
- `endDate` (optional): End date in ISO format

**Response** (200 OK):

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"planTaskId": 123,
			"userId": "user123",
			"userName": "Ahmed Mohamed",
			"interactionType": 1,
			"interactionTypeName": "Visit",
			"clientType": 1,
			"clientTypeName": "A",
			"result": 1,
			"resultName": "Interested",
			"reason": null,
			"reasonName": null,
			"comment": "Client showed strong interest in MRI machine",
			"createdAt": "2024-01-15T10:30:00Z",
			"updatedAt": "2024-01-15T10:30:00Z",
			"deal": {
				"id": 1,
				"activityLogId": 1,
				"userId": "user123",
				"dealValue": 750000.0,
				"status": 1,
				"statusName": "Pending",
				"expectedCloseDate": "2024-02-15T00:00:00Z",
				"createdAt": "2024-01-15T10:30:00Z",
				"updatedAt": "2024-01-15T10:30:00Z"
			},
			"offer": null
		}
	]
}
```

**React Native Component Example**:

```javascript
// components/ActivitiesList.js
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
import { ActivityService } from '../services/ActivityService';

const ActivitiesList = ({ startDate, endDate }) => {
	const [activities, setActivities] = useState([]);
	const [loading, setLoading] = useState(true);
	const [refreshing, setRefreshing] = useState(false);

	useEffect(() => {
		loadActivities();
	}, [startDate, endDate]);

	const loadActivities = async () => {
		try {
			setLoading(true);
			const response = await ActivityService.getActivities(
				startDate,
				endDate
			);
			setActivities(response.data || []);
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	const onRefresh = async () => {
		setRefreshing(true);
		await loadActivities();
		setRefreshing(false);
	};

	const renderActivity = ({ item }) => (
		<TouchableOpacity style={styles.activityCard}>
			<View style={styles.activityHeader}>
				<Text style={styles.activityType}>
					{item.interactionTypeName} -{' '}
					{item.clientTypeName}
				</Text>
				<Text style={styles.activityDate}>
					{new Date(
						item.createdAt
					).toLocaleDateString()}
				</Text>
			</View>

			<Text style={styles.activityComment}>
				{item.comment}
			</Text>

			<View style={styles.activityResult}>
				<Text
					style={[
						styles.resultText,
						item.result === 1
							? styles.interested
							: styles.notInterested,
					]}
				>
					{item.resultName}
				</Text>

				{item.reason && (
					<Text style={styles.reasonText}>
						Reason: {item.reasonName}
					</Text>
				)}
			</View>

			{item.deal && (
				<View style={styles.dealInfo}>
					<Text style={styles.dealValue}>
						Deal: $
						{item.deal.dealValue.toLocaleString()}
					</Text>
					<Text style={styles.dealStatus}>
						Status: {item.deal.statusName}
					</Text>
				</View>
			)}

			{item.offer && (
				<View style={styles.offerInfo}>
					<Text style={styles.offerDetails}>
						Offer: {item.offer.offerDetails}
					</Text>
					<Text style={styles.offerStatus}>
						Status: {item.offer.statusName}
					</Text>
				</View>
			)}
		</TouchableOpacity>
	);

	if (loading) {
		return (
			<View style={styles.centerContainer}>
				<Text>Loading activities...</Text>
			</View>
		);
	}

	return (
		<View style={styles.container}>
			<FlatList
				data={activities}
				renderItem={renderActivity}
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
							No activities found
						</Text>
					</View>
				}
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
	activityCard: {
		backgroundColor: '#fff',
		margin: 10,
		padding: 15,
		borderRadius: 10,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
		elevation: 3,
	},
	activityHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 10,
	},
	activityType: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#333',
	},
	activityDate: {
		fontSize: 14,
		color: '#666',
	},
	activityComment: {
		fontSize: 14,
		color: '#555',
		marginBottom: 10,
	},
	activityResult: {
		marginBottom: 10,
	},
	resultText: {
		fontSize: 16,
		fontWeight: 'bold',
	},
	interested: {
		color: '#4CAF50',
	},
	notInterested: {
		color: '#F44336',
	},
	reasonText: {
		fontSize: 14,
		color: '#666',
		marginTop: 5,
	},
	dealInfo: {
		backgroundColor: '#E8F5E8',
		padding: 10,
		borderRadius: 5,
		marginTop: 5,
	},
	dealValue: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#2E7D32',
	},
	dealStatus: {
		fontSize: 14,
		color: '#388E3C',
		marginTop: 2,
	},
	offerInfo: {
		backgroundColor: '#E3F2FD',
		padding: 10,
		borderRadius: 5,
		marginTop: 5,
	},
	offerDetails: {
		fontSize: 14,
		color: '#1976D2',
	},
	offerStatus: {
		fontSize: 14,
		color: '#1565C0',
		marginTop: 2,
	},
	emptyContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
		padding: 50,
	},
	emptyText: {
		fontSize: 16,
		color: '#666',
	},
});

export default ActivitiesList;
```

### 3. Get Single Activity

**GET** `/api/activities/{id}`

**Authorization**: `Salesman`

**Path Parameters**:

- `id` (long): Activity ID

**Response** (200 OK):

```json
{
	"success": true,
	"data": {
		"id": 1,
		"planTaskId": 123,
		"userId": "user123",
		"userName": "Ahmed Mohamed",
		"interactionType": 1,
		"interactionTypeName": "Visit",
		"clientType": 1,
		"clientTypeName": "A",
		"result": 1,
		"resultName": "Interested",
		"reason": null,
		"reasonName": null,
		"comment": "Client showed strong interest in MRI machine",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T10:30:00Z",
		"deal": {
			"id": 1,
			"activityLogId": 1,
			"userId": "user123",
			"dealValue": 750000.0,
			"status": 1,
			"statusName": "Pending",
			"expectedCloseDate": "2024-02-15T00:00:00Z",
			"createdAt": "2024-01-15T10:30:00Z",
			"updatedAt": "2024-01-15T10:30:00Z"
		},
		"offer": null
	}
}
```

**React Native Hook Example**:

```javascript
// hooks/useActivity.js
import { useState, useEffect } from 'react';
import { ActivityService } from '../services/ActivityService';

export const useActivity = (activityId) => {
	const [activity, setActivity] = useState(null);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);

	useEffect(() => {
		if (activityId) {
			loadActivity();
		}
	}, [activityId]);

	const loadActivity = async () => {
		try {
			setLoading(true);
			setError(null);
			const response = await ActivityService.getActivity(
				activityId
			);
			setActivity(response.data);
		} catch (err) {
			setError(err.message);
		} finally {
			setLoading(false);
		}
	};

	const refresh = () => {
		loadActivity();
	};

	return { activity, loading, error, refresh };
};
```

---

## 💼 Deal Management APIs

### 4. Update Deal

**PUT** `/api/deals/{id}`

**Authorization**: `Salesman`

**Path Parameters**:

- `id` (long): Deal ID

**Request Body**:

```json
{
	"status": 2,
	"dealValue": 800000.0,
	"expectedCloseDate": "2024-02-20T00:00:00Z"
}
```

**Deal Status Values**:

- `status`: 1 = Pending, 2 = Won, 3 = Lost

**Response** (200 OK):

```json
{
	"success": true,
	"message": "Deal updated successfully",
	"data": {
		"id": 1,
		"activityLogId": 1,
		"userId": "user123",
		"dealValue": 800000.0,
		"status": 2,
		"statusName": "Won",
		"expectedCloseDate": "2024-02-20T00:00:00Z",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T11:45:00Z"
	}
}
```

**React Native Service Implementation**:

```javascript
// services/DealService.js
import apiClient from '../apiClient';

export const DealService = {
	// Update a deal
	async updateDeal(dealId, updateData) {
		try {
			const response = await apiClient.put(
				`/api/deals/${dealId}`,
				updateData
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	handleError(error) {
		if (error.response) {
			return new Error(
				error.response.data.message || 'API Error'
			);
		} else if (error.request) {
			return new Error(
				'Network Error - Please check your connection'
			);
		} else {
			return new Error('An unexpected error occurred');
		}
	},
};
```

**React Native Component Example**:

```javascript
// components/DealUpdateForm.js
import React, { useState } from 'react';
import {
	View,
	Text,
	TextInput,
	TouchableOpacity,
	Alert,
	StyleSheet,
	Modal,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { DealService } from '../services/DealService';

const DealUpdateForm = ({ deal, visible, onClose, onDealUpdated }) => {
	const [formData, setFormData] = useState({
		status: deal?.status || 1,
		dealValue: deal?.dealValue?.toString() || '',
		expectedCloseDate: deal?.expectedCloseDate
			? new Date(deal.expectedCloseDate)
					.toISOString()
					.split('T')[0]
			: '',
	});
	const [loading, setLoading] = useState(false);

	const handleSubmit = async () => {
		try {
			setLoading(true);

			// Validate required fields
			if (
				!formData.dealValue ||
				parseFloat(formData.dealValue) <= 0
			) {
				Alert.alert(
					'Error',
					'Deal value must be greater than 0'
				);
				return;
			}

			// Prepare request data
			const requestData = {
				status: formData.status,
				dealValue: parseFloat(formData.dealValue),
				expectedCloseDate: formData.expectedCloseDate
					? new Date(
							formData.expectedCloseDate
					  ).toISOString()
					: null,
			};

			const result = await DealService.updateDeal(
				deal.id,
				requestData
			);

			Alert.alert('Success', 'Deal updated successfully');
			onDealUpdated?.(result.data);
			onClose();
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	if (!deal) return null;

	return (
		<Modal
			visible={visible}
			animationType="slide"
			presentationStyle="pageSheet"
		>
			<View style={styles.container}>
				<View style={styles.header}>
					<Text style={styles.title}>
						Update Deal
					</Text>
					<TouchableOpacity
						onPress={onClose}
						style={styles.closeButton}
					>
						<Text
							style={
								styles.closeButtonText
							}
						>
							✕
						</Text>
					</TouchableOpacity>
				</View>

				<View style={styles.form}>
					{/* Deal Value */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Deal Value *
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.dealValue
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									dealValue: text,
								})
							}
							placeholder="Enter deal value"
							keyboardType="numeric"
						/>
					</View>

					{/* Status */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Status
						</Text>
						<Picker
							selectedValue={
								formData.status
							}
							onValueChange={(
								value
							) =>
								setFormData({
									...formData,
									status: value,
								})
							}
							style={styles.picker}
						>
							<Picker.Item
								label="Pending"
								value={1}
							/>
							<Picker.Item
								label="Won"
								value={2}
							/>
							<Picker.Item
								label="Lost"
								value={3}
							/>
						</Picker>
					</View>

					{/* Expected Close Date */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Expected Close Date
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.expectedCloseDate
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									expectedCloseDate:
										text,
								})
							}
							placeholder="YYYY-MM-DD"
						/>
					</View>

					<TouchableOpacity
						style={[
							styles.button,
							loading &&
								styles.buttonDisabled,
						]}
						onPress={handleSubmit}
						disabled={loading}
					>
						<Text style={styles.buttonText}>
							{loading
								? 'Updating...'
								: 'Update Deal'}
						</Text>
					</TouchableOpacity>
				</View>
			</View>
		</Modal>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#fff',
	},
	header: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: 20,
		borderBottomWidth: 1,
		borderBottomColor: '#eee',
	},
	title: {
		fontSize: 20,
		fontWeight: 'bold',
	},
	closeButton: {
		padding: 5,
	},
	closeButtonText: {
		fontSize: 20,
		color: '#666',
	},
	form: {
		padding: 20,
	},
	inputGroup: {
		marginBottom: 20,
	},
	label: {
		fontSize: 16,
		fontWeight: '600',
		marginBottom: 8,
		color: '#333',
	},
	input: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		fontSize: 16,
	},
	picker: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
	},
	button: {
		backgroundColor: '#007AFF',
		padding: 15,
		borderRadius: 8,
		alignItems: 'center',
		marginTop: 20,
	},
	buttonDisabled: {
		backgroundColor: '#ccc',
	},
	buttonText: {
		color: '#fff',
		fontSize: 16,
		fontWeight: '600',
	},
});

export default DealUpdateForm;
```

---

## 📋 Offer Management APIs

### 5. Update Offer

**PUT** `/api/offers/{id}`

**Authorization**: `Salesman`

**Path Parameters**:

- `id` (long): Offer ID

**Request Body**:

```json
{
	"status": 3,
	"offerDetails": "Offer accepted with minor modifications",
	"documentUrl": "https://example.com/updated-proposal.pdf"
}
```

**Offer Status Values**:

- `status`: 1 = Draft, 2 = Sent, 3 = Accepted, 4 = Rejected

**Response** (200 OK):

```json
{
	"success": true,
	"message": "Offer updated successfully",
	"data": {
		"id": 1,
		"activityLogId": 1,
		"userId": "user123",
		"offerDetails": "Offer accepted with minor modifications",
		"status": 3,
		"statusName": "Accepted",
		"documentUrl": "https://example.com/updated-proposal.pdf",
		"createdAt": "2024-01-15T10:30:00Z",
		"updatedAt": "2024-01-15T11:45:00Z"
	}
}
```

**React Native Service Implementation**:

```javascript
// services/OfferService.js
import apiClient from '../apiClient';

export const OfferService = {
	// Update an offer
	async updateOffer(offerId, updateData) {
		try {
			const response = await apiClient.put(
				`/api/offers/${offerId}`,
				updateData
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	handleError(error) {
		if (error.response) {
			return new Error(
				error.response.data.message || 'API Error'
			);
		} else if (error.request) {
			return new Error(
				'Network Error - Please check your connection'
			);
		} else {
			return new Error('An unexpected error occurred');
		}
	},
};
```

**React Native Component Example**:

```javascript
// components/OfferUpdateForm.js
import React, { useState } from 'react';
import {
	View,
	Text,
	TextInput,
	TouchableOpacity,
	Alert,
	StyleSheet,
	Modal,
	ScrollView,
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { OfferService } from '../services/OfferService';

const OfferUpdateForm = ({ offer, visible, onClose, onOfferUpdated }) => {
	const [formData, setFormData] = useState({
		status: offer?.status || 1,
		offerDetails: offer?.offerDetails || '',
		documentUrl: offer?.documentUrl || '',
	});
	const [loading, setLoading] = useState(false);

	const handleSubmit = async () => {
		try {
			setLoading(true);

			// Validate required fields
			if (!formData.offerDetails.trim()) {
				Alert.alert(
					'Error',
					'Offer details are required'
				);
				return;
			}

			// Prepare request data
			const requestData = {
				status: formData.status,
				offerDetails: formData.offerDetails,
				documentUrl: formData.documentUrl || null,
			};

			const result = await OfferService.updateOffer(
				offer.id,
				requestData
			);

			Alert.alert('Success', 'Offer updated successfully');
			onOfferUpdated?.(result.data);
			onClose();
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	if (!offer) return null;

	return (
		<Modal
			visible={visible}
			animationType="slide"
			presentationStyle="pageSheet"
		>
			<View style={styles.container}>
				<View style={styles.header}>
					<Text style={styles.title}>
						Update Offer
					</Text>
					<TouchableOpacity
						onPress={onClose}
						style={styles.closeButton}
					>
						<Text
							style={
								styles.closeButtonText
							}
						>
							✕
						</Text>
					</TouchableOpacity>
				</View>

				<ScrollView style={styles.form}>
					{/* Status */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Status
						</Text>
						<Picker
							selectedValue={
								formData.status
							}
							onValueChange={(
								value
							) =>
								setFormData({
									...formData,
									status: value,
								})
							}
							style={styles.picker}
						>
							<Picker.Item
								label="Draft"
								value={1}
							/>
							<Picker.Item
								label="Sent"
								value={2}
							/>
							<Picker.Item
								label="Accepted"
								value={3}
							/>
							<Picker.Item
								label="Rejected"
								value={4}
							/>
						</Picker>
					</View>

					{/* Offer Details */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Offer Details *
						</Text>
						<TextInput
							style={styles.textArea}
							value={
								formData.offerDetails
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									offerDetails:
										text,
								})
							}
							placeholder="Enter offer details..."
							multiline
							numberOfLines={6}
						/>
					</View>

					{/* Document URL */}
					<View style={styles.inputGroup}>
						<Text style={styles.label}>
							Document URL
						</Text>
						<TextInput
							style={styles.input}
							value={
								formData.documentUrl
							}
							onChangeText={(text) =>
								setFormData({
									...formData,
									documentUrl:
										text,
								})
							}
							placeholder="https://example.com/document.pdf"
							keyboardType="url"
						/>
					</View>

					<TouchableOpacity
						style={[
							styles.button,
							loading &&
								styles.buttonDisabled,
						]}
						onPress={handleSubmit}
						disabled={loading}
					>
						<Text style={styles.buttonText}>
							{loading
								? 'Updating...'
								: 'Update Offer'}
						</Text>
					</TouchableOpacity>
				</ScrollView>
			</View>
		</Modal>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#fff',
	},
	header: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: 20,
		borderBottomWidth: 1,
		borderBottomColor: '#eee',
	},
	title: {
		fontSize: 20,
		fontWeight: 'bold',
	},
	closeButton: {
		padding: 5,
	},
	closeButtonText: {
		fontSize: 20,
		color: '#666',
	},
	form: {
		flex: 1,
		padding: 20,
	},
	inputGroup: {
		marginBottom: 20,
	},
	label: {
		fontSize: 16,
		fontWeight: '600',
		marginBottom: 8,
		color: '#333',
	},
	input: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		fontSize: 16,
	},
	textArea: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
		padding: 12,
		fontSize: 16,
		textAlignVertical: 'top',
		minHeight: 120,
	},
	picker: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 8,
	},
	button: {
		backgroundColor: '#007AFF',
		padding: 15,
		borderRadius: 8,
		alignItems: 'center',
		marginTop: 20,
		marginBottom: 40,
	},
	buttonDisabled: {
		backgroundColor: '#ccc',
	},
	buttonText: {
		color: '#fff',
		fontSize: 16,
		fontWeight: '600',
	},
});

export default OfferUpdateForm;
```

---

## 📊 Salesman Statistics APIs

### 7. Get Salesman Statistics

**GET** `/api/salesman/stats?startDate={date}&endDate={date}`

**Authorization**: `Salesman`

**Query Parameters**:

- `startDate` (required): Start date in ISO format
- `endDate` (required): End date in ISO format

**Response** (200 OK):

```json
{
	"success": true,
	"data": {
		"startDate": "2024-01-01T00:00:00Z",
		"endDate": "2024-01-31T23:59:59Z",
		"userId": "user123",
		"userName": "Ahmed Mohamed",
		"totalActivities": 45,
		"totalVisits": 30,
		"totalFollowUps": 15,
		"interestedActivities": 25,
		"notInterestedActivities": 20,
		"totalDeals": 8,
		"wonDeals": 5,
		"lostDeals": 2,
		"pendingDeals": 1,
		"totalDealValue": 1200000.0,
		"wonDealValue": 800000.0,
		"averageDealValue": 150000.0,
		"totalOffers": 12,
		"acceptedOffers": 8,
		"rejectedOffers": 3,
		"draftOffers": 1,
		"sentOffers": 0,
		"conversionRate": 17.78,
		"offerAcceptanceRate": 66.67,
		"interestRate": 55.56,
		"clientTypeStats": [
			{
				"clientType": 1,
				"clientTypeName": "A",
				"count": 20,
				"percentage": 44.44,
				"totalValue": 800000.0
			},
			{
				"clientType": 2,
				"clientTypeName": "B",
				"count": 15,
				"percentage": 33.33,
				"totalValue": 300000.0
			},
			{
				"clientType": 3,
				"clientTypeName": "C",
				"count": 10,
				"percentage": 22.22,
				"totalValue": 100000.0
			}
		],
		"recentActivities": [
			{
				"id": 1,
				"userName": "Ahmed Mohamed",
				"interactionType": 1,
				"interactionTypeName": "Visit",
				"clientType": 1,
				"clientTypeName": "A",
				"result": 1,
				"resultName": "Interested",
				"comment": "Successful visit to 57357 Hospital",
				"createdAt": "2024-01-30T10:00:00Z",
				"dealValue": 500000.0,
				"offerDetails": null
			}
		],
		"weeklyTrends": [
			{
				"weekStart": "2024-01-01T00:00:00Z",
				"weekEnd": "2024-01-07T23:59:59Z",
				"activities": 12,
				"visits": 8,
				"followUps": 4,
				"deals": 2,
				"wonDeals": 1,
				"dealValue": 200000.0,
				"offers": 3,
				"acceptedOffers": 2
			}
		]
	}
}
```

### 8. Get Current Week Statistics

**GET** `/api/salesman/stats/current-week`

**Authorization**: `Salesman`

**Response**: Same as above but for current week only.

### 9. Get Current Month Statistics

**GET** `/api/salesman/stats/current-month`

**Authorization**: `Salesman`

**Response**: Same as above but for current month only.

**React Native Service Implementation**:

```javascript
// services/SalesmanStatsService.js
import apiClient from '../apiClient';

export const SalesmanStatsService = {
	// Get statistics for date range
	async getSalesmanStats(startDate, endDate) {
		try {
			const params = new URLSearchParams();
			params.append('startDate', startDate.toISOString());
			params.append('endDate', endDate.toISOString());

			const response = await apiClient.get(
				`/api/salesman/stats?${params}`
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	// Get current week statistics
	async getCurrentWeekStats() {
		try {
			const response = await apiClient.get(
				'/api/salesman/stats/current-week'
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	// Get current month statistics
	async getCurrentMonthStats() {
		try {
			const response = await apiClient.get(
				'/api/salesman/stats/current-month'
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	handleError(error) {
		if (error.response) {
			return new Error(
				error.response.data.message || 'API Error'
			);
		} else if (error.request) {
			return new Error(
				'Network Error - Please check your connection'
			);
		} else {
			return new Error('An unexpected error occurred');
		}
	},
};
```

**React Native Component Example**:

```javascript
// components/SalesmanDashboard.js
import React, { useState, useEffect } from 'react';
import {
	View,
	Text,
	ScrollView,
	StyleSheet,
	RefreshControl,
	TouchableOpacity,
	Alert,
	Dimensions,
} from 'react-native';
import { SalesmanStatsService } from '../services/SalesmanStatsService';

const { width } = Dimensions.get('window');

const SalesmanDashboard = () => {
	const [stats, setStats] = useState(null);
	const [loading, setLoading] = useState(true);
	const [refreshing, setRefreshing] = useState(false);
	const [selectedPeriod, setSelectedPeriod] = useState('week'); // 'week', 'month', 'custom'

	useEffect(() => {
		loadStats();
	}, [selectedPeriod]);

	const loadStats = async () => {
		try {
			setLoading(true);
			let response;

			switch (selectedPeriod) {
				case 'week':
					response =
						await SalesmanStatsService.getCurrentWeekStats();
					break;
				case 'month':
					response =
						await SalesmanStatsService.getCurrentMonthStats();
					break;
				default:
					// Custom date range
					const startDate = new Date();
					startDate.setMonth(
						startDate.getMonth() - 1
					);
					const endDate = new Date();
					response =
						await SalesmanStatsService.getSalesmanStats(
							startDate,
							endDate
						);
			}

			setStats(response.data);
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	const onRefresh = async () => {
		setRefreshing(true);
		await loadStats();
		setRefreshing(false);
	};

	const formatCurrency = (amount) => {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: 'USD',
			minimumFractionDigits: 0,
			maximumFractionDigits: 0,
		}).format(amount);
	};

	const formatPercentage = (value) => {
		return `${value.toFixed(1)}%`;
	};

	if (loading) {
		return (
			<View style={styles.centerContainer}>
				<Text>Loading your statistics...</Text>
			</View>
		);
	}

	if (!stats) {
		return (
			<View style={styles.centerContainer}>
				<Text>No data available</Text>
			</View>
		);
	}

	return (
		<ScrollView
			style={styles.container}
			refreshControl={
				<RefreshControl
					refreshing={refreshing}
					onRefresh={onRefresh}
				/>
			}
		>
			{/* Period Selector */}
			<View style={styles.periodSelector}>
				<TouchableOpacity
					style={[
						styles.periodButton,
						selectedPeriod === 'week' &&
							styles.periodButtonActive,
					]}
					onPress={() =>
						setSelectedPeriod('week')
					}
				>
					<Text
						style={[
							styles.periodButtonText,
							selectedPeriod ===
								'week' &&
								styles.periodButtonTextActive,
						]}
					>
						This Week
					</Text>
				</TouchableOpacity>
				<TouchableOpacity
					style={[
						styles.periodButton,
						selectedPeriod === 'month' &&
							styles.periodButtonActive,
					]}
					onPress={() =>
						setSelectedPeriod('month')
					}
				>
					<Text
						style={[
							styles.periodButtonText,
							selectedPeriod ===
								'month' &&
								styles.periodButtonTextActive,
						]}
					>
						This Month
					</Text>
				</TouchableOpacity>
			</View>

			{/* Overview Cards */}
			<View style={styles.overviewContainer}>
				<View style={styles.cardRow}>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#4CAF50',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalActivities}
						</Text>
						<Text style={styles.statLabel}>
							Total Activities
						</Text>
					</View>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#2196F3',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalDeals}
						</Text>
						<Text style={styles.statLabel}>
							Total Deals
						</Text>
					</View>
				</View>

				<View style={styles.cardRow}>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#FF9800',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.wonDeals}
						</Text>
						<Text style={styles.statLabel}>
							Won Deals
						</Text>
					</View>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#9C27B0',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalOffers}
						</Text>
						<Text style={styles.statLabel}>
							Total Offers
						</Text>
					</View>
				</View>
			</View>

			{/* Performance Metrics */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Performance Metrics
				</Text>
				<View style={styles.metricsContainer}>
					<View style={styles.metricItem}>
						<Text
							style={
								styles.metricValue
							}
						>
							{formatPercentage(
								stats.conversionRate
							)}
						</Text>
						<Text
							style={
								styles.metricLabel
							}
						>
							Conversion Rate
						</Text>
					</View>
					<View style={styles.metricItem}>
						<Text
							style={
								styles.metricValue
							}
						>
							{formatPercentage(
								stats.interestRate
							)}
						</Text>
						<Text
							style={
								styles.metricLabel
							}
						>
							Interest Rate
						</Text>
					</View>
					<View style={styles.metricItem}>
						<Text
							style={
								styles.metricValue
							}
						>
							{formatPercentage(
								stats.offerAcceptanceRate
							)}
						</Text>
						<Text
							style={
								styles.metricLabel
							}
						>
							Offer Acceptance
						</Text>
					</View>
				</View>
			</View>

			{/* Financial Summary */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Financial Summary
				</Text>
				<View style={styles.financialContainer}>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.totalDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Total Deal Value
						</Text>
					</View>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.wonDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Won Deal Value
						</Text>
					</View>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.averageDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Average Deal Value
						</Text>
					</View>
				</View>
			</View>

			{/* Activity Breakdown */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Activity Breakdown
				</Text>
				<View style={styles.activityBreakdown}>
					<View style={styles.activityItem}>
						<Text
							style={
								styles.activityLabel
							}
						>
							Visits
						</Text>
						<Text
							style={
								styles.activityValue
							}
						>
							{stats.totalVisits}
						</Text>
					</View>
					<View style={styles.activityItem}>
						<Text
							style={
								styles.activityLabel
							}
						>
							Follow-ups
						</Text>
						<Text
							style={
								styles.activityValue
							}
						>
							{stats.totalFollowUps}
						</Text>
					</View>
					<View style={styles.activityItem}>
						<Text
							style={
								styles.activityLabel
							}
						>
							Interested
						</Text>
						<Text
							style={
								styles.activityValue
							}
						>
							{
								stats.interestedActivities
							}
						</Text>
					</View>
					<View style={styles.activityItem}>
						<Text
							style={
								styles.activityLabel
							}
						>
							Not Interested
						</Text>
						<Text
							style={
								styles.activityValue
							}
						>
							{
								stats.notInterestedActivities
							}
						</Text>
					</View>
				</View>
			</View>

			{/* Client Type Distribution */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Client Type Distribution
				</Text>
				{stats.clientTypeStats.map(
					(clientType, index) => (
						<View
							key={index}
							style={
								styles.clientTypeItem
							}
						>
							<View
								style={
									styles.clientTypeHeader
								}
							>
								<Text
									style={
										styles.clientTypeName
									}
								>
									Type{' '}
									{
										clientType.clientTypeName
									}
								</Text>
								<Text
									style={
										styles.clientTypeCount
									}
								>
									{
										clientType.count
									}{' '}
									(
									{formatPercentage(
										clientType.percentage
									)}
									)
								</Text>
							</View>
							<View
								style={
									styles.progressBar
								}
							>
								<View
									style={[
										styles.progressFill,
										{
											width: `${clientType.percentage}%`,
										},
									]}
								/>
							</View>
							<Text
								style={
									styles.clientTypeValue
								}
							>
								{formatCurrency(
									clientType.totalValue
								)}
							</Text>
						</View>
					)
				)}
			</View>

			{/* Recent Activities */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Recent Activities
				</Text>
				{stats.recentActivities.map(
					(activity, index) => (
						<View
							key={index}
							style={
								styles.activityItem
							}
						>
							<View
								style={
									styles.activityHeader
								}
							>
								<Text
									style={
										styles.activityType
									}
								>
									{
										activity.interactionTypeName
									}{' '}
									- Type{' '}
									{
										activity.clientTypeName
									}
								</Text>
								<Text
									style={
										styles.activityDate
									}
								>
									{new Date(
										activity.createdAt
									).toLocaleDateString()}
								</Text>
							</View>
							<Text
								style={
									styles.activityComment
								}
							>
								{
									activity.comment
								}
							</Text>
							{activity.dealValue && (
								<Text
									style={
										styles.activityValue
									}
								>
									Deal:{' '}
									{formatCurrency(
										activity.dealValue
									)}
								</Text>
							)}
							{activity.offerDetails && (
								<Text
									style={
										styles.activityOffer
									}
								>
									Offer:{' '}
									{
										activity.offerDetails
									}
								</Text>
							)}
						</View>
					)
				)}
			</View>
		</ScrollView>
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
	periodSelector: {
		flexDirection: 'row',
		margin: 15,
		backgroundColor: '#fff',
		borderRadius: 10,
		padding: 5,
	},
	periodButton: {
		flex: 1,
		padding: 10,
		borderRadius: 8,
		alignItems: 'center',
	},
	periodButtonActive: {
		backgroundColor: '#007AFF',
	},
	periodButtonText: {
		fontSize: 14,
		color: '#666',
	},
	periodButtonTextActive: {
		color: '#fff',
		fontWeight: 'bold',
	},
	overviewContainer: {
		padding: 15,
	},
	cardRow: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 15,
	},
	statCard: {
		flex: 1,
		padding: 20,
		borderRadius: 10,
		marginHorizontal: 5,
		alignItems: 'center',
	},
	statNumber: {
		fontSize: 24,
		fontWeight: 'bold',
		color: '#fff',
	},
	statLabel: {
		fontSize: 14,
		color: '#fff',
		marginTop: 5,
	},
	section: {
		backgroundColor: '#fff',
		margin: 15,
		padding: 20,
		borderRadius: 10,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
		elevation: 3,
	},
	sectionTitle: {
		fontSize: 18,
		fontWeight: 'bold',
		marginBottom: 15,
		color: '#333',
	},
	metricsContainer: {
		flexDirection: 'row',
		justifyContent: 'space-around',
	},
	metricItem: {
		alignItems: 'center',
	},
	metricValue: {
		fontSize: 20,
		fontWeight: 'bold',
		color: '#1976D2',
	},
	metricLabel: {
		fontSize: 12,
		color: '#666',
		marginTop: 5,
	},
	financialContainer: {
		flexDirection: 'row',
		justifyContent: 'space-around',
	},
	financialItem: {
		alignItems: 'center',
	},
	financialValue: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#2E7D32',
	},
	financialLabel: {
		fontSize: 12,
		color: '#666',
		marginTop: 5,
	},
	activityBreakdown: {
		flexDirection: 'row',
		flexWrap: 'wrap',
		justifyContent: 'space-between',
	},
	activityItem: {
		width: '48%',
		backgroundColor: '#f8f9fa',
		padding: 15,
		borderRadius: 8,
		marginBottom: 10,
		alignItems: 'center',
	},
	activityLabel: {
		fontSize: 14,
		color: '#666',
		marginBottom: 5,
	},
	activityValue: {
		fontSize: 20,
		fontWeight: 'bold',
		color: '#333',
	},
	clientTypeItem: {
		marginBottom: 15,
	},
	clientTypeHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 5,
	},
	clientTypeName: {
		fontSize: 16,
		fontWeight: '600',
	},
	clientTypeCount: {
		fontSize: 14,
		color: '#666',
	},
	progressBar: {
		height: 8,
		backgroundColor: '#E0E0E0',
		borderRadius: 4,
		marginBottom: 5,
	},
	progressFill: {
		height: '100%',
		backgroundColor: '#4CAF50',
		borderRadius: 4,
	},
	clientTypeValue: {
		fontSize: 14,
		color: '#2E7D32',
		fontWeight: '600',
	},
	activityHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 5,
	},
	activityType: {
		fontSize: 14,
		color: '#1976D2',
		fontWeight: '600',
	},
	activityDate: {
		fontSize: 12,
		color: '#666',
	},
	activityComment: {
		fontSize: 14,
		color: '#555',
		marginBottom: 5,
	},
	activityValue: {
		fontSize: 14,
		color: '#2E7D32',
		fontWeight: '600',
	},
	activityOffer: {
		fontSize: 14,
		color: '#FF9800',
		fontStyle: 'italic',
	},
});

export default SalesmanDashboard;
```

---

## 📊 Manager Dashboard APIs

### 6. Get Dashboard Statistics

**GET** `/api/manager/dashboard-stats?startDate={date}&endDate={date}`

**Authorization**: `SalesManager,SuperAdmin`

**Query Parameters**:

- `startDate` (required): Start date in ISO format
- `endDate` (required): End date in ISO format

**Response** (200 OK):

```json
{
	"success": true,
	"data": {
		"startDate": "2024-01-01T00:00:00Z",
		"endDate": "2024-01-31T23:59:59Z",
		"totalActivities": 150,
		"totalVisits": 100,
		"totalFollowUps": 50,
		"totalDeals": 25,
		"totalOffers": 30,
		"wonDeals": 15,
		"lostDeals": 8,
		"pendingDeals": 2,
		"acceptedOffers": 20,
		"rejectedOffers": 8,
		"draftOffers": 2,
		"sentOffers": 0,
		"totalDealValue": 5000000.0,
		"wonDealValue": 3000000.0,
		"averageDealValue": 200000.0,
		"conversionRate": 16.67,
		"offerAcceptanceRate": 66.67,
		"clientTypeStats": [
			{
				"clientType": 1,
				"clientTypeName": "A",
				"count": 60,
				"percentage": 40.0,
				"totalValue": 3000000.0
			},
			{
				"clientType": 2,
				"clientTypeName": "B",
				"count": 45,
				"percentage": 30.0,
				"totalValue": 1500000.0
			},
			{
				"clientType": 3,
				"clientTypeName": "C",
				"count": 30,
				"percentage": 20.0,
				"totalValue": 400000.0
			},
			{
				"clientType": 4,
				"clientTypeName": "D",
				"count": 15,
				"percentage": 10.0,
				"totalValue": 100000.0
			}
		],
		"salespersonStats": [
			{
				"userId": "user1",
				"userName": "Ahmed Mohamed",
				"totalActivities": 75,
				"totalDeals": 12,
				"wonDeals": 8,
				"totalValue": 1500000.0,
				"wonValue": 1000000.0,
				"conversionRate": 16.0
			},
			{
				"userId": "user2",
				"userName": "Sara Ahmed",
				"totalActivities": 50,
				"totalDeals": 8,
				"wonDeals": 5,
				"totalValue": 1200000.0,
				"wonValue": 800000.0,
				"conversionRate": 16.0
			}
		],
		"recentActivities": [
			{
				"id": 1,
				"userName": "Ahmed Mohamed",
				"interactionType": 1,
				"interactionTypeName": "Visit",
				"clientType": 1,
				"clientTypeName": "A",
				"result": 1,
				"resultName": "Interested",
				"comment": "Successful visit to 57357 Hospital",
				"createdAt": "2024-01-30T10:00:00Z",
				"dealValue": 500000.0,
				"offerDetails": null
			},
			{
				"id": 2,
				"userName": "Sara Ahmed",
				"interactionType": 2,
				"interactionTypeName": "FollowUp",
				"clientType": 2,
				"clientTypeName": "B",
				"result": 1,
				"resultName": "Interested",
				"comment": "Follow-up call with client",
				"createdAt": "2024-01-29T14:30:00Z",
				"dealValue": null,
				"offerDetails": "Comprehensive medical equipment proposal"
			}
		]
	}
}
```

**React Native Service Implementation**:

```javascript
// services/ManagerDashboardService.js
import apiClient from '../apiClient';

export const ManagerDashboardService = {
	// Get dashboard statistics
	async getDashboardStats(startDate, endDate) {
		try {
			const params = new URLSearchParams();
			params.append('startDate', startDate.toISOString());
			params.append('endDate', endDate.toISOString());

			const response = await apiClient.get(
				`/api/manager/dashboard-stats?${params}`
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},

	handleError(error) {
		if (error.response) {
			return new Error(
				error.response.data.message || 'API Error'
			);
		} else if (error.request) {
			return new Error(
				'Network Error - Please check your connection'
			);
		} else {
			return new Error('An unexpected error occurred');
		}
	},
};
```

**React Native Component Example**:

```javascript
// components/ManagerDashboard.js
import React, { useState, useEffect } from 'react';
import {
	View,
	Text,
	ScrollView,
	StyleSheet,
	RefreshControl,
	TouchableOpacity,
	Alert,
	Dimensions,
} from 'react-native';
import { ManagerDashboardService } from '../services/ManagerDashboardService';

const { width } = Dimensions.get('window');

const ManagerDashboard = () => {
	const [stats, setStats] = useState(null);
	const [loading, setLoading] = useState(true);
	const [refreshing, setRefreshing] = useState(false);
	const [dateRange, setDateRange] = useState({
		startDate: new Date(
			new Date().getFullYear(),
			new Date().getMonth(),
			1
		),
		endDate: new Date(),
	});

	useEffect(() => {
		loadDashboardStats();
	}, [dateRange]);

	const loadDashboardStats = async () => {
		try {
			setLoading(true);
			const response =
				await ManagerDashboardService.getDashboardStats(
					dateRange.startDate,
					dateRange.endDate
				);
			setStats(response.data);
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	const onRefresh = async () => {
		setRefreshing(true);
		await loadDashboardStats();
		setRefreshing(false);
	};

	const formatCurrency = (amount) => {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency: 'USD',
			minimumFractionDigits: 0,
			maximumFractionDigits: 0,
		}).format(amount);
	};

	const formatPercentage = (value) => {
		return `${value.toFixed(1)}%`;
	};

	if (loading) {
		return (
			<View style={styles.centerContainer}>
				<Text>Loading dashboard...</Text>
			</View>
		);
	}

	if (!stats) {
		return (
			<View style={styles.centerContainer}>
				<Text>No data available</Text>
			</View>
		);
	}

	return (
		<ScrollView
			style={styles.container}
			refreshControl={
				<RefreshControl
					refreshing={refreshing}
					onRefresh={onRefresh}
				/>
			}
		>
			{/* Overview Cards */}
			<View style={styles.overviewContainer}>
				<View style={styles.cardRow}>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#4CAF50',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalActivities}
						</Text>
						<Text style={styles.statLabel}>
							Total Activities
						</Text>
					</View>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#2196F3',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalDeals}
						</Text>
						<Text style={styles.statLabel}>
							Total Deals
						</Text>
					</View>
				</View>

				<View style={styles.cardRow}>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#FF9800',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.wonDeals}
						</Text>
						<Text style={styles.statLabel}>
							Won Deals
						</Text>
					</View>
					<View
						style={[
							styles.statCard,
							{
								backgroundColor:
									'#9C27B0',
							},
						]}
					>
						<Text style={styles.statNumber}>
							{stats.totalOffers}
						</Text>
						<Text style={styles.statLabel}>
							Total Offers
						</Text>
					</View>
				</View>
			</View>

			{/* Financial Summary */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Financial Summary
				</Text>
				<View style={styles.financialContainer}>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.totalDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Total Deal Value
						</Text>
					</View>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.wonDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Won Deal Value
						</Text>
					</View>
					<View style={styles.financialItem}>
						<Text
							style={
								styles.financialValue
							}
						>
							{formatCurrency(
								stats.averageDealValue
							)}
						</Text>
						<Text
							style={
								styles.financialLabel
							}
						>
							Average Deal Value
						</Text>
					</View>
				</View>
			</View>

			{/* Performance Metrics */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Performance Metrics
				</Text>
				<View style={styles.metricsContainer}>
					<View style={styles.metricItem}>
						<Text
							style={
								styles.metricValue
							}
						>
							{formatPercentage(
								stats.conversionRate
							)}
						</Text>
						<Text
							style={
								styles.metricLabel
							}
						>
							Conversion Rate
						</Text>
					</View>
					<View style={styles.metricItem}>
						<Text
							style={
								styles.metricValue
							}
						>
							{formatPercentage(
								stats.offerAcceptanceRate
							)}
						</Text>
						<Text
							style={
								styles.metricLabel
							}
						>
							Offer Acceptance Rate
						</Text>
					</View>
				</View>
			</View>

			{/* Client Type Distribution */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Client Type Distribution
				</Text>
				{stats.clientTypeStats.map(
					(clientType, index) => (
						<View
							key={index}
							style={
								styles.clientTypeItem
							}
						>
							<View
								style={
									styles.clientTypeHeader
								}
							>
								<Text
									style={
										styles.clientTypeName
									}
								>
									Type{' '}
									{
										clientType.clientTypeName
									}
								</Text>
								<Text
									style={
										styles.clientTypeCount
									}
								>
									{
										clientType.count
									}{' '}
									(
									{formatPercentage(
										clientType.percentage
									)}
									)
								</Text>
							</View>
							<View
								style={
									styles.progressBar
								}
							>
								<View
									style={[
										styles.progressFill,
										{
											width: `${clientType.percentage}%`,
										},
									]}
								/>
							</View>
							<Text
								style={
									styles.clientTypeValue
								}
							>
								{formatCurrency(
									clientType.totalValue
								)}
							</Text>
						</View>
					)
				)}
			</View>

			{/* Salesperson Performance */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Salesperson Performance
				</Text>
				{stats.salespersonStats.map(
					(salesperson, index) => (
						<View
							key={index}
							style={
								styles.salespersonCard
							}
						>
							<Text
								style={
									styles.salespersonName
								}
							>
								{
									salesperson.userName
								}
							</Text>
							<View
								style={
									styles.salespersonStats
								}
							>
								<View
									style={
										styles.salespersonStat
									}
								>
									<Text
										style={
											styles.salespersonStatValue
										}
									>
										{
											salesperson.totalActivities
										}
									</Text>
									<Text
										style={
											styles.salespersonStatLabel
										}
									>
										Activities
									</Text>
								</View>
								<View
									style={
										styles.salespersonStat
									}
								>
									<Text
										style={
											styles.salespersonStatValue
										}
									>
										{
											salesperson.wonDeals
										}
									</Text>
									<Text
										style={
											styles.salespersonStatLabel
										}
									>
										Won
										Deals
									</Text>
								</View>
								<View
									style={
										styles.salespersonStat
									}
								>
									<Text
										style={
											styles.salespersonStatValue
										}
									>
										{formatPercentage(
											salesperson.conversionRate
										)}
									</Text>
									<Text
										style={
											styles.salespersonStatLabel
										}
									>
										Conversion
									</Text>
								</View>
							</View>
							<Text
								style={
									styles.salespersonValue
								}
							>
								{formatCurrency(
									salesperson.wonValue
								)}
							</Text>
						</View>
					)
				)}
			</View>

			{/* Recent Activities */}
			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Recent Activities
				</Text>
				{stats.recentActivities.map(
					(activity, index) => (
						<View
							key={index}
							style={
								styles.activityItem
							}
						>
							<View
								style={
									styles.activityHeader
								}
							>
								<Text
									style={
										styles.activityUser
									}
								>
									{
										activity.userName
									}
								</Text>
								<Text
									style={
										styles.activityDate
									}
								>
									{new Date(
										activity.createdAt
									).toLocaleDateString()}
								</Text>
							</View>
							<Text
								style={
									styles.activityType
								}
							>
								{
									activity.interactionTypeName
								}{' '}
								- Type{' '}
								{
									activity.clientTypeName
								}
							</Text>
							<Text
								style={
									styles.activityComment
								}
							>
								{
									activity.comment
								}
							</Text>
							{activity.dealValue && (
								<Text
									style={
										styles.activityValue
									}
								>
									Deal:{' '}
									{formatCurrency(
										activity.dealValue
									)}
								</Text>
							)}
							{activity.offerDetails && (
								<Text
									style={
										styles.activityOffer
									}
								>
									Offer:{' '}
									{
										activity.offerDetails
									}
								</Text>
							)}
						</View>
					)
				)}
			</View>
		</ScrollView>
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
	overviewContainer: {
		padding: 15,
	},
	cardRow: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 15,
	},
	statCard: {
		flex: 1,
		padding: 20,
		borderRadius: 10,
		marginHorizontal: 5,
		alignItems: 'center',
	},
	statNumber: {
		fontSize: 24,
		fontWeight: 'bold',
		color: '#fff',
	},
	statLabel: {
		fontSize: 14,
		color: '#fff',
		marginTop: 5,
	},
	section: {
		backgroundColor: '#fff',
		margin: 15,
		padding: 20,
		borderRadius: 10,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
		elevation: 3,
	},
	sectionTitle: {
		fontSize: 18,
		fontWeight: 'bold',
		marginBottom: 15,
		color: '#333',
	},
	financialContainer: {
		flexDirection: 'row',
		justifyContent: 'space-around',
	},
	financialItem: {
		alignItems: 'center',
	},
	financialValue: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#2E7D32',
	},
	financialLabel: {
		fontSize: 12,
		color: '#666',
		marginTop: 5,
	},
	metricsContainer: {
		flexDirection: 'row',
		justifyContent: 'space-around',
	},
	metricItem: {
		alignItems: 'center',
	},
	metricValue: {
		fontSize: 20,
		fontWeight: 'bold',
		color: '#1976D2',
	},
	metricLabel: {
		fontSize: 12,
		color: '#666',
		marginTop: 5,
	},
	clientTypeItem: {
		marginBottom: 15,
	},
	clientTypeHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 5,
	},
	clientTypeName: {
		fontSize: 16,
		fontWeight: '600',
	},
	clientTypeCount: {
		fontSize: 14,
		color: '#666',
	},
	progressBar: {
		height: 8,
		backgroundColor: '#E0E0E0',
		borderRadius: 4,
		marginBottom: 5,
	},
	progressFill: {
		height: '100%',
		backgroundColor: '#4CAF50',
		borderRadius: 4,
	},
	clientTypeValue: {
		fontSize: 14,
		color: '#2E7D32',
		fontWeight: '600',
	},
	salespersonCard: {
		backgroundColor: '#f8f9fa',
		padding: 15,
		borderRadius: 8,
		marginBottom: 10,
	},
	salespersonName: {
		fontSize: 16,
		fontWeight: 'bold',
		marginBottom: 10,
	},
	salespersonStats: {
		flexDirection: 'row',
		justifyContent: 'space-around',
		marginBottom: 10,
	},
	salespersonStat: {
		alignItems: 'center',
	},
	salespersonStatValue: {
		fontSize: 18,
		fontWeight: 'bold',
		color: '#1976D2',
	},
	salespersonStatLabel: {
		fontSize: 12,
		color: '#666',
	},
	salespersonValue: {
		fontSize: 16,
		fontWeight: 'bold',
		color: '#2E7D32',
		textAlign: 'center',
	},
	activityItem: {
		backgroundColor: '#f8f9fa',
		padding: 15,
		borderRadius: 8,
		marginBottom: 10,
	},
	activityHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		marginBottom: 5,
	},
	activityUser: {
		fontSize: 16,
		fontWeight: 'bold',
	},
	activityDate: {
		fontSize: 12,
		color: '#666',
	},
	activityType: {
		fontSize: 14,
		color: '#1976D2',
		marginBottom: 5,
	},
	activityComment: {
		fontSize: 14,
		color: '#555',
		marginBottom: 5,
	},
	activityValue: {
		fontSize: 14,
		color: '#2E7D32',
		fontWeight: '600',
	},
	activityOffer: {
		fontSize: 14,
		color: '#FF9800',
		fontStyle: 'italic',
	},
});

export default ManagerDashboard;
```

---

## 🔧 Error Responses

### Standard Error Format

```json
{
	"success": false,
	"message": "Error description",
	"data": null
}
```

### Common HTTP Status Codes

- `200` - Success
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/missing token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `500` - Internal Server Error

### Example Error Responses

**400 Bad Request**:

```json
{
	"success": false,
	"message": "Either DealInfo or OfferInfo must be provided when result is Interested",
	"data": null
}
```

**401 Unauthorized**:

```json
{
	"success": false,
	"message": "Unauthorized access",
	"data": null
}
```

**403 Forbidden**:

```json
{
	"success": false,
	"message": "You don't have permission to add activities to this task",
	"data": null
}
```

**404 Not Found**:

```json
{
	"success": false,
	"message": "Activity not found",
	"data": null
}
```

### React Native Error Handling

```javascript
// utils/errorHandler.js
export const handleApiError = (error) => {
	if (error.response) {
		// Server responded with error status
		const { status, data } = error.response;

		switch (status) {
			case 400:
				return {
					type: 'validation',
					message:
						data.message ||
						'Invalid request data',
					details: data.errors || null,
				};
			case 401:
				return {
					type: 'auth',
					message: 'Session expired. Please login again.',
					action: 'logout',
				};
			case 403:
				return {
					type: 'permission',
					message: "You don't have permission to perform this action",
					action: 'navigate_back',
				};
			case 404:
				return {
					type: 'not_found',
					message: 'The requested resource was not found',
					action: 'navigate_back',
				};
			case 500:
				return {
					type: 'server',
					message: 'Server error. Please try again later.',
					action: 'retry',
				};
			default:
				return {
					type: 'unknown',
					message:
						data.message ||
						'An unexpected error occurred',
					action: 'retry',
				};
		}
	} else if (error.request) {
		// Network error
		return {
			type: 'network',
			message: 'Network error. Please check your connection.',
			action: 'retry',
		};
	} else {
		// Other error
		return {
			type: 'unknown',
			message: 'An unexpected error occurred',
			action: 'retry',
		};
	}
};

// Usage in components
const handleError = (error) => {
	const errorInfo = handleApiError(error);

	switch (errorInfo.action) {
		case 'logout':
			// Navigate to login screen
			navigation.navigate('Login');
			break;
		case 'navigate_back':
			navigation.goBack();
			break;
		case 'retry':
			// Show retry option
			Alert.alert('Error', errorInfo.message, [
				{ text: 'Cancel', style: 'cancel' },
				{ text: 'Retry', onPress: () => retryAction() },
			]);
			break;
		default:
			Alert.alert('Error', errorInfo.message);
	}
};
```

---

## 📝 Data Validation Rules

### Activity Request Validation

- `interactionType`: Must be 1 (Visit) or 2 (FollowUp)
- `clientType`: Must be 1 (A), 2 (B), 3 (C), or 4 (D)
- `result`: Must be 1 (Interested) or 2 (NotInterested)
- `reason`: Required if result is 2 (NotInterested)
- `dealInfo` or `offerInfo`: Required if result is 1 (Interested)

### Deal Update Validation

- `status`: Must be 1 (Pending), 2 (Won), or 3 (Lost)
- `dealValue`: Must be positive decimal number
- `expectedCloseDate`: Must be valid ISO date string

### Offer Update Validation

- `status`: Must be 1 (Draft), 2 (Sent), 3 (Accepted), or 4 (Rejected)
- `offerDetails`: Required string
- `documentUrl`: Optional valid URL

---

## 🚀 Complete React Native Integration Guide

### Project Setup

```bash
# Create new React Native project
npx react-native init SoitMedApp --template react-native-template-typescript

# Install required dependencies
cd SoitMedApp
npm install axios @react-native-async-storage/async-storage @react-native-picker/picker
npm install --save-dev @types/react-native-vector-icons

# For iOS
cd ios && pod install && cd ..
```

### Project Structure

```
src/
├── api/
│   ├── apiClient.js
│   └── endpoints.js
├── services/
│   ├── ActivityService.js
│   ├── DealService.js
│   ├── OfferService.js
│   └── ManagerDashboardService.js
├── components/
│   ├── CreateActivityForm.js
│   ├── ActivitiesList.js
│   ├── DealUpdateForm.js
│   ├── OfferUpdateForm.js
│   └── ManagerDashboard.js
├── hooks/
│   └── useActivity.js
├── utils/
│   ├── errorHandler.js
│   └── constants.js
├── screens/
│   ├── Salesman/
│   │   ├── ActivitiesScreen.js
│   │   ├── CreateActivityScreen.js
│   │   └── DealsScreen.js
│   └── Manager/
│       └── DashboardScreen.js
└── navigation/
    └── AppNavigator.js
```

### Constants and Configuration

```javascript
// utils/constants.js
export const API_CONFIG = {
	BASE_URL: __DEV__ ? 'http://localhost:5117' : 'https://api.soitmed.com',
	TIMEOUT: 10000,
};

export const ENUMS = {
	INTERACTION_TYPE: {
		VISIT: 1,
		FOLLOW_UP: 2,
	},
	CLIENT_TYPE: {
		A: 1,
		B: 2,
		C: 3,
		D: 4,
	},
	ACTIVITY_RESULT: {
		INTERESTED: 1,
		NOT_INTERESTED: 2,
	},
	REJECTION_REASON: {
		CASH: 1,
		PRICE: 2,
		NEED: 3,
		OTHER: 4,
	},
	DEAL_STATUS: {
		PENDING: 1,
		WON: 2,
		LOST: 3,
	},
	OFFER_STATUS: {
		DRAFT: 1,
		SENT: 2,
		ACCEPTED: 3,
		REJECTED: 4,
	},
};

export const COLORS = {
	PRIMARY: '#007AFF',
	SUCCESS: '#4CAF50',
	ERROR: '#F44336',
	WARNING: '#FF9800',
	INFO: '#2196F3',
	BACKGROUND: '#f5f5f5',
	CARD: '#ffffff',
	TEXT: '#333333',
	TEXT_SECONDARY: '#666666',
};
```

### Navigation Setup

```javascript
// navigation/AppNavigator.js
import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';

// Import screens
import ActivitiesScreen from '../screens/Salesman/ActivitiesScreen';
import CreateActivityScreen from '../screens/Salesman/CreateActivityScreen';
import DealsScreen from '../screens/Salesman/DealsScreen';
import DashboardScreen from '../screens/Manager/DashboardScreen';

const Stack = createStackNavigator();
const Tab = createBottomTabNavigator();

const SalesmanStack = () => (
	<Stack.Navigator>
		<Stack.Screen
			name="Activities"
			component={ActivitiesScreen}
			options={{ title: 'My Activities' }}
		/>
		<Stack.Screen
			name="CreateActivity"
			component={CreateActivityScreen}
			options={{ title: 'Create Activity' }}
		/>
		<Stack.Screen
			name="Deals"
			component={DealsScreen}
			options={{ title: 'My Deals' }}
		/>
	</Stack.Navigator>
);

const ManagerStack = () => (
	<Stack.Navigator>
		<Stack.Screen
			name="Dashboard"
			component={DashboardScreen}
			options={{ title: 'Manager Dashboard' }}
		/>
	</Stack.Navigator>
);

const AppNavigator = ({ userRole }) => {
	return (
		<NavigationContainer>
			<Tab.Navigator>
				<Tab.Screen
					name="Activities"
					component={SalesmanStack}
					options={{ title: 'Activities' }}
				/>
				{userRole === 'SalesManager' ||
				userRole === 'SuperAdmin' ? (
					<Tab.Screen
						name="Dashboard"
						component={ManagerStack}
						options={{ title: 'Dashboard' }}
					/>
				) : null}
			</Tab.Navigator>
		</NavigationContainer>
	);
};

export default AppNavigator;
```

### Main App Component

```javascript
// App.js
import React, { useState, useEffect } from 'react';
import { View, Text, ActivityIndicator, StyleSheet } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';
import AppNavigator from './src/navigation/AppNavigator';

const App = () => {
	const [isLoading, setIsLoading] = useState(true);
	const [userRole, setUserRole] = useState(null);
	const [isAuthenticated, setIsAuthenticated] = useState(false);

	useEffect(() => {
		checkAuthStatus();
	}, []);

	const checkAuthStatus = async () => {
		try {
			const token = await AsyncStorage.getItem('jwt_token');
			const role = await AsyncStorage.getItem('user_role');

			if (token && role) {
				setIsAuthenticated(true);
				setUserRole(role);
			}
		} catch (error) {
			console.error('Error checking auth status:', error);
		} finally {
			setIsLoading(false);
		}
	};

	if (isLoading) {
		return (
			<View style={styles.loadingContainer}>
				<ActivityIndicator
					size="large"
					color="#007AFF"
				/>
				<Text style={styles.loadingText}>
					Loading...
				</Text>
			</View>
		);
	}

	if (!isAuthenticated) {
		// Navigate to login screen
		return <LoginScreen onLogin={checkAuthStatus} />;
	}

	return <AppNavigator userRole={userRole} />;
};

const styles = StyleSheet.create({
	loadingContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
		backgroundColor: '#f5f5f5',
	},
	loadingText: {
		marginTop: 10,
		fontSize: 16,
		color: '#666',
	},
});

export default App;
```

### Testing the APIs

```javascript
// utils/apiTester.js
import { ActivityService } from '../services/ActivityService';
import { DealService } from '../services/DealService';
import { OfferService } from '../services/OfferService';
import { ManagerDashboardService } from '../services/ManagerDashboardService';

export const testAllAPIs = async () => {
	console.log('🧪 Testing SoitMed APIs...');

	try {
		// Test 1: Get Activities
		console.log('📋 Testing Get Activities...');
		const activities = await ActivityService.getActivities();
		console.log(
			'✅ Activities loaded:',
			activities.data?.length || 0,
			'items'
		);

		// Test 2: Create Activity (if you have a task ID)
		console.log('➕ Testing Create Activity...');
		const newActivity = await ActivityService.createActivity(1, {
			interactionType: 1,
			clientType: 1,
			result: 1,
			comment: 'Test activity from React Native',
			dealInfo: {
				dealValue: 100000,
				expectedCloseDate: new Date(
					Date.now() + 30 * 24 * 60 * 60 * 1000
				).toISOString(),
			},
		});
		console.log('✅ Activity created:', newActivity.data?.id);

		// Test 3: Update Deal (if you have a deal ID)
		if (newActivity.data?.deal?.id) {
			console.log('💼 Testing Update Deal...');
			const updatedDeal = await DealService.updateDeal(
				newActivity.data.deal.id,
				{
					status: 2,
					dealValue: 120000,
				}
			);
			console.log('✅ Deal updated:', updatedDeal.data?.id);
		}

		// Test 4: Get Dashboard Stats (if manager)
		console.log('📊 Testing Dashboard Stats...');
		const startDate = new Date();
		startDate.setMonth(startDate.getMonth() - 1);
		const endDate = new Date();

		const dashboardStats =
			await ManagerDashboardService.getDashboardStats(
				startDate,
				endDate
			);
		console.log('✅ Dashboard stats loaded:', {
			totalActivities: dashboardStats.data?.totalActivities,
			totalDeals: dashboardStats.data?.totalDeals,
			wonDeals: dashboardStats.data?.wonDeals,
		});

		console.log('🎉 All API tests completed successfully!');
	} catch (error) {
		console.error('❌ API test failed:', error.message);
	}
};
```

---

## 🔐 Role-Based Access Summary

| Role             | Permissions                                                                                  |
| ---------------- | -------------------------------------------------------------------------------------------- |
| **Salesman**     | • Create activities<br>• Update own deals/offers<br>• View own activities                    |
| **SalesManager** | • All Salesman permissions<br>• View team dashboard statistics<br>• Access manager analytics |
| **SuperAdmin**   | • All permissions<br>• View team dashboard statistics<br>• Full system access                |

---

## 🚨 Common Issues and Troubleshooting

### Issue 1: Enum Value Conversion Error

**Error Message:**

```
The JSON value could not be converted to SoitMed.Models.Enums.InteractionType
```

**Cause:** Sending string values instead of numeric enum values.

**❌ Wrong (String Values):**

```javascript
const requestData = {
	interactionType: 'Visit', // ❌ Wrong - string
	clientType: 'A', // ❌ Wrong - string
	result: 'Interested', // ❌ Wrong - string
	reason: 'Price', // ❌ Wrong - string
};
```

**✅ Correct (Numeric Values):**

```javascript
const requestData = {
	interactionType: 1, // ✅ Correct - Visit = 1
	clientType: 1, // ✅ Correct - A = 1
	result: 1, // ✅ Correct - Interested = 1
	reason: 2, // ✅ Correct - Price = 2
};
```

### Issue 2: "Request field is required" Error

**Cause:** Missing or incorrect request body structure.

**Solution:** Ensure the request body matches the `CreateActivityRequestDto` structure exactly.

### Issue 3: Network Request Failed

**Cause:** Incorrect API URL or network connectivity issues.

**Solutions:**

1. Check if the API server is running on the correct port (5117)
2. Verify the base URL in your API client
3. Check network connectivity
4. Ensure proper CORS configuration

### Debug Helper Function

```javascript
// utils/debugHelper.js
export const debugApiRequest = (endpoint, data) => {
	console.log('🔍 API Debug Info:');
	console.log('Endpoint:', endpoint);
	console.log('Request Data:', JSON.stringify(data, null, 2));
	console.log('Data Types:', {
		interactionType: typeof data.interactionType,
		clientType: typeof data.clientType,
		result: typeof data.result,
		reason: typeof data.reason,
	});

	// Validate enum values
	const errors = [];
	if (
		typeof data.interactionType !== 'number' ||
		![1, 2].includes(data.interactionType)
	) {
		errors.push(
			'interactionType must be 1 (Visit) or 2 (FollowUp)'
		);
	}
	if (
		typeof data.clientType !== 'number' ||
		![1, 2, 3, 4].includes(data.clientType)
	) {
		errors.push('clientType must be 1 (A), 2 (B), 3 (C), or 4 (D)');
	}
	if (typeof data.result !== 'number' || ![1, 2].includes(data.result)) {
		errors.push(
			'result must be 1 (Interested) or 2 (NotInterested)'
		);
	}
	if (
		data.result === 2 &&
		(!data.reason || ![1, 2, 3, 4].includes(data.reason))
	) {
		errors.push(
			'reason must be 1 (Cash), 2 (Price), 3 (Need), or 4 (Other) when result is 2'
		);
	}

	if (errors.length > 0) {
		console.error('❌ Validation Errors:', errors);
	} else {
		console.log('✅ Request data is valid');
	}
};

// Usage in your service
import { debugApiRequest } from '../utils/debugHelper';

export const ActivityService = {
	async createActivity(taskId, activityData) {
		debugApiRequest(
			`/api/activities/tasks/${taskId}/activities`,
			activityData
		);

		try {
			const response = await apiClient.post(
				`/api/activities/tasks/${taskId}/activities`,
				activityData
			);
			return response.data;
		} catch (error) {
			throw this.handleError(error);
		}
	},
};
```

---

## 📱 React Native Best Practices

### 1. State Management

- Use React hooks for local state
- Consider Redux or Context API for global state
- Implement proper loading and error states

### 2. Performance

- Use FlatList for large lists
- Implement pagination for activities
- Use React.memo for expensive components

### 3. Security

- Store JWT tokens securely using AsyncStorage
- Implement token refresh logic
- Validate all user inputs

### 4. Offline Support

- Cache data locally
- Implement sync when connection is restored
- Show offline indicators

### 5. Testing

- Write unit tests for services
- Test API integration
- Use React Native Testing Library

---

**For technical support, contact: support@soitmed.com**
