# 🚀 Sales Module API Documentation for React/React Native Teams

## Base URL

```
Development: http://localhost:58868
Production: https://api.soitmed.com
```

## Authentication

All API endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer YOUR_JWT_TOKEN
```

## 📱 API Endpoints

### 1. Activities Management

#### Create Activity

**POST** `/api/activities/tasks/{taskId}/activities`

**Headers:**

```
Content-Type: application/json
Authorization: Bearer {token}
```

**Request Body:**

```typescript
interface CreateActivityRequest {
	interactionType: 'Visit' | 'FollowUp';
	clientType: 'A' | 'B' | 'C' | 'D';
	result: 'Interested' | 'NotInterested';
	reason?: 'Cash' | 'Price' | 'Need' | 'Other'; // Required if result is NotInterested
	comment?: string;
	dealInfo?: {
		dealValue: number;
		expectedCloseDate?: string; // ISO date string
	};
	offerInfo?: {
		offerDetails: string;
		documentUrl?: string;
	};
}
```

**Response:**

```typescript
interface ApiResponse<T> {
	success: boolean;
	message: string;
	data: T;
}

interface ActivityResponse {
	id: number;
	planTaskId: number;
	userId: string;
	userName: string;
	interactionType: string;
	interactionTypeName: string;
	clientType: string;
	clientTypeName: string;
	result: string;
	resultName: string;
	reason?: string;
	reasonName?: string;
	comment?: string;
	createdAt: string; // ISO date string
	updatedAt: string; // ISO date string
	deal?: DealResponse;
	offer?: OfferResponse;
}
```

**Example Request:**

```javascript
const createActivity = async (taskId, activityData) => {
	const response = await fetch(
		`/api/activities/tasks/${taskId}/activities`,
		{
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`,
			},
			body: JSON.stringify(activityData),
		}
	);
	return response.json();
};

// Usage
const activityData = {
	interactionType: 'Visit',
	clientType: 'A',
	result: 'Interested',
	comment: 'Client showed strong interest',
	dealInfo: {
		dealValue: 750000,
		expectedCloseDate: '2024-02-15',
	},
};

const result = await createActivity(1, activityData);
```

#### Get User Activities

**GET** `/api/activities?startDate={date}&endDate={date}`

**Query Parameters:**

- `startDate` (optional): Start date in YYYY-MM-DD format
- `endDate` (optional): End date in YYYY-MM-DD format

**Response:**

```typescript
interface ActivitiesResponse {
	success: boolean;
	data: ActivityResponse[];
}
```

**Example:**

```javascript
const getActivities = async (startDate, endDate) => {
	const params = new URLSearchParams();
	if (startDate) params.append('startDate', startDate);
	if (endDate) params.append('endDate', endDate);

	const response = await fetch(`/api/activities?${params}`, {
		headers: {
			Authorization: `Bearer ${token}`,
		},
	});
	return response.json();
};
```

#### Get Single Activity

**GET** `/api/activities/{id}`

**Response:**

```typescript
interface SingleActivityResponse {
	success: boolean;
	data: ActivityResponse;
}
```

### 2. Deal Management

#### Update Deal

**PUT** `/api/deals/{id}`

**Request Body:**

```typescript
interface UpdateDealRequest {
	status?: 'Pending' | 'Won' | 'Lost';
	dealValue?: number;
	expectedCloseDate?: string; // ISO date string
}
```

**Response:**

```typescript
interface DealResponse {
	id: number;
	activityLogId: number;
	userId: string;
	dealValue: number;
	status: string;
	statusName: string;
	expectedCloseDate?: string;
	createdAt: string;
	updatedAt: string;
}
```

**Example:**

```javascript
const updateDeal = async (dealId, updateData) => {
	const response = await fetch(`/api/deals/${dealId}`, {
		method: 'PUT',
		headers: {
			'Content-Type': 'application/json',
			Authorization: `Bearer ${token}`,
		},
		body: JSON.stringify(updateData),
	});
	return response.json();
};

// Usage
const updateData = {
	status: 'Won',
	dealValue: 800000,
	expectedCloseDate: '2024-02-20',
};

const result = await updateDeal(1, updateData);
```

### 3. Offer Management

#### Update Offer

**PUT** `/api/offers/{id}`

**Request Body:**

```typescript
interface UpdateOfferRequest {
	status?: 'Draft' | 'Sent' | 'Accepted' | 'Rejected';
	offerDetails?: string;
	documentUrl?: string;
}
```

**Response:**

```typescript
interface OfferResponse {
	id: number;
	activityLogId: number;
	userId: string;
	offerDetails: string;
	status: string;
	statusName: string;
	documentUrl?: string;
	createdAt: string;
	updatedAt: string;
}
```

### 4. Manager Dashboard

#### Get Dashboard Statistics

**GET** `/api/manager/dashboard-stats?startDate={date}&endDate={date}`

**Required Role:** SalesManager or SuperAdmin

**Query Parameters:**

- `startDate` (required): Start date in YYYY-MM-DD format
- `endDate` (required): End date in YYYY-MM-DD format

**Response:**

```typescript
interface DashboardStatsResponse {
	success: boolean;
	data: {
		startDate: string;
		endDate: string;
		totalActivities: number;
		totalVisits: number;
		totalFollowUps: number;
		totalDeals: number;
		totalOffers: number;
		wonDeals: number;
		lostDeals: number;
		pendingDeals: number;
		acceptedOffers: number;
		rejectedOffers: number;
		draftOffers: number;
		totalDealValue: number;
		wonDealValue: number;
		averageDealValue: number;
		conversionRate: number;
		offerAcceptanceRate: number;
		clientTypeStats: ClientTypeStats[];
		salespersonStats: SalespersonStats[];
		recentActivities: RecentActivity[];
	};
}

interface ClientTypeStats {
	clientType: string;
	clientTypeName: string;
	count: number;
	percentage: number;
	totalValue: number;
}

interface SalespersonStats {
	userId: string;
	userName: string;
	totalActivities: number;
	totalDeals: number;
	wonDeals: number;
	totalValue: number;
	wonValue: number;
	conversionRate: number;
}

interface RecentActivity {
	id: number;
	userName: string;
	interactionType: string;
	interactionTypeName: string;
	clientType: string;
	clientTypeName: string;
	result: string;
	resultName: string;
	comment?: string;
	createdAt: string;
	dealValue?: number;
	offerDetails?: string;
}
```

**Example:**

```javascript
const getDashboardStats = async (startDate, endDate) => {
	const response = await fetch(
		`/api/manager/dashboard-stats?startDate=${startDate}&endDate=${endDate}`,
		{
			headers: {
				Authorization: `Bearer ${token}`,
			},
		}
	);
	return response.json();
};

// Usage
const stats = await getDashboardStats('2024-01-01', '2024-01-31');
console.log('Total Activities:', stats.data.totalActivities);
console.log('Conversion Rate:', stats.data.conversionRate);
```

## 🔧 Error Handling

### Standard Error Response

```typescript
interface ErrorResponse {
	success: false;
	message: string;
	data: null;
}
```

### HTTP Status Codes

- `200` - Success
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/missing token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `500` - Internal Server Error

### Example Error Handling

```javascript
const handleApiCall = async (apiCall) => {
	try {
		const response = await apiCall();

		if (!response.success) {
			throw new Error(response.message);
		}

		return response.data;
	} catch (error) {
		console.error('API Error:', error.message);
		// Handle error (show toast, redirect, etc.)
		throw error;
	}
};
```

## 📱 React/React Native Examples

### React Hook Example

```javascript
import { useState, useEffect } from 'react';

const useActivities = (startDate, endDate) => {
	const [activities, setActivities] = useState([]);
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState(null);

	const fetchActivities = async () => {
		setLoading(true);
		setError(null);

		try {
			const response = await getActivities(
				startDate,
				endDate
			);
			setActivities(response.data);
		} catch (err) {
			setError(err.message);
		} finally {
			setLoading(false);
		}
	};

	useEffect(() => {
		fetchActivities();
	}, [startDate, endDate]);

	return { activities, loading, error, refetch: fetchActivities };
};
```

### React Native Component Example

```javascript
import React, { useState } from 'react';
import { View, Text, TouchableOpacity, Alert } from 'react-native';

const ActivityForm = ({ taskId, onSuccess }) => {
	const [formData, setFormData] = useState({
		interactionType: 'Visit',
		clientType: 'A',
		result: 'Interested',
		comment: '',
	});

	const handleSubmit = async () => {
		try {
			const response = await createActivity(taskId, formData);
			if (response.success) {
				Alert.alert(
					'Success',
					'Activity created successfully'
				);
				onSuccess?.(response.data);
			}
		} catch (error) {
			Alert.alert('Error', error.message);
		}
	};

	return (
		<View>
			{/* Form fields */}
			<TouchableOpacity onPress={handleSubmit}>
				<Text>Create Activity</Text>
			</TouchableOpacity>
		</View>
	);
};
```

## 🔐 Role-Based Access

### Salesman Permissions

- Create activities
- Update own deals and offers
- View own activities

### SalesManager Permissions

- All Salesman permissions
- View team dashboard statistics
- Access manager analytics

### SuperAdmin Permissions

- All permissions
- Full system access

## 📊 Data Validation

### Required Fields

- `interactionType`: Must be 'Visit' or 'FollowUp'
- `clientType`: Must be 'A', 'B', 'C', or 'D'
- `result`: Must be 'Interested' or 'NotInterested'
- `reason`: Required if result is 'NotInterested'
- `dealInfo` or `offerInfo`: Required if result is 'Interested'

### Date Format

- All dates should be in ISO 8601 format: `YYYY-MM-DD`
- Example: `2024-01-15`

### Number Format

- `dealValue`: Must be positive decimal number
- Example: `750000.50`

## 🚀 Getting Started

1. **Install Dependencies**

      ```bash
      npm install axios
      # or
      yarn add axios
      ```

2. **Setup API Client**

      ```javascript
      import axios from 'axios';

      const apiClient = axios.create({
      	baseURL: 'http://localhost:58868',
      	headers: {
      		'Content-Type': 'application/json',
      	},
      });

      // Add token to requests
      apiClient.interceptors.request.use((config) => {
      	const token = localStorage.getItem('token'); // or AsyncStorage for React Native
      	if (token) {
      		config.headers.Authorization = `Bearer ${token}`;
      	}
      	return config;
      });
      ```

3. **Start Using the APIs**

      ```javascript
      // Get activities
      const activities = await apiClient.get('/api/activities');

      // Create activity
      const newActivity = await apiClient.post(
      	'/api/activities/tasks/1/activities',
      	activityData
      );

      // Get dashboard stats
      const stats = await apiClient.get(
      	'/api/manager/dashboard-stats?startDate=2024-01-01&endDate=2024-01-31'
      );
      ```

---

**For technical support, contact: support@soitmed.com**



