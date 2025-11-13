# React Native Guide - Salesman App

## 🎯 Overview

This guide is for **Salesman** users who use the **React Native mobile app** to:

- Create TaskProgress entries
- Initiate offer requests automatically
- Track their tasks and progress

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Installation](#installation)
3. [Creating TaskProgress (Auto-Create OfferRequest)](#creating-taskprogress-auto-create-offerrequest)
4. [API Endpoints](#api-endpoints)
5. [Example Components](#example-components)
6. [Error Handling](#error-handling)

---

## 📦 Prerequisites

- React Native 0.60+
- `@react-native-community/async-storage` for token storage
- `react-native-axios` or `fetch` API
- JWT authentication token

---

## 📥 Installation

```bash
npm install @react-native-community/async-storage axios
# or
yarn add @react-native-community/async-storage axios
```

---

## 📝 Creating TaskProgress (Auto-Create OfferRequest)

### How It Works

When a Salesman creates a TaskProgress with:

- `visitResult: "Interested"`
- `nextStep: "NeedsOffer"`

The backend **automatically**:

1. Creates an `OfferRequest` in the database
2. Assigns it to a SalesSupport user
3. Sends notifications to all SalesSupport users

### Endpoint

```http
POST /api/TaskProgress
Authorization: Bearer {token}
```

### Request Body

```typescript
interface CreateTaskProgressRequest {
	taskId: number; // Required: Task ID from WeeklyPlan
	progressDate: string; // Required: ISO 8601 date string
	progressType: string; // Required: "Visit", "Call", "Meeting", "Email"
	visitResult: 'Interested'; // ✅ Required: Triggers auto-create
	nextStep: 'NeedsOffer'; // ✅ Required: Triggers auto-create
	description?: string; // Optional: Used as RequestedProducts
	notes?: string; // Optional: Used as SpecialNotes
	notInterestedComment?: string;
	nextFollowUpDate?: string;
	followUpNotes?: string;
	satisfactionRating?: number; // 1-5
	feedback?: string;
}
```

### Complete Example

```typescript
// services/TaskProgressService.ts
import AsyncStorage from '@react-native-community/async-storage';
import axios from 'axios';

const API_BASE_URL = 'http://your-api-url.com'; // Replace with your API URL

export interface CreateTaskProgressDTO {
	taskId: number;
	progressDate: string;
	progressType: string;
	visitResult?: string;
	nextStep?: string;
	description?: string;
	notes?: string;
	notInterestedComment?: string;
	nextFollowUpDate?: string;
	followUpNotes?: string;
	satisfactionRating?: number;
	feedback?: string;
}

export interface TaskProgressResponse {
	success: boolean;
	data: {
		id: number;
		taskId: number;
		clientId: number | null;
		clientName: string;
		progressDate: string;
		progressType: string;
		visitResult: string | null;
		nextStep: string | null;
		offerRequestId: number | null; // ✅ Set if OfferRequest was created
		createdAt: string;
	};
	message: string;
}

export const createTaskProgress = async (
	progressData: CreateTaskProgressDTO
): Promise<TaskProgressResponse> => {
	try {
		const token = await AsyncStorage.getItem('authToken');

		if (!token) {
			throw new Error('No authentication token found');
		}

		const response = await axios.post(
			`${API_BASE_URL}/api/TaskProgress`,
			progressData,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`,
				},
			}
		);

		return response.data;
	} catch (error: any) {
		console.error('Error creating task progress:', error);
		throw error;
	}
};
```

### Usage in Component

```typescript
// components/TaskProgressForm.tsx
import React, { useState } from 'react';
import { View, TextInput, Button, Alert } from 'react-native';
import { createTaskProgress } from '../services/TaskProgressService';

interface TaskProgressFormProps {
	taskId: number;
	onSuccess?: () => void;
}

export const TaskProgressForm: React.FC<TaskProgressFormProps> = ({
	taskId,
	onSuccess,
}) => {
	const [description, setDescription] = useState('');
	const [notes, setNotes] = useState('');
	const [visitResult, setVisitResult] = useState<string>('');
	const [nextStep, setNextStep] = useState<string>('');
	const [loading, setLoading] = useState(false);

	const handleSubmit = async () => {
		// Validate required fields
		if (!visitResult || !nextStep) {
			Alert.alert(
				'Error',
				'Please select visit result and next step'
			);
			return;
		}

		// ✅ Check if this will trigger offer request
		const willCreateOfferRequest =
			visitResult === 'Interested' &&
			nextStep === 'NeedsOffer';

		setLoading(true);

		try {
			const response = await createTaskProgress({
				taskId: taskId,
				progressDate: new Date().toISOString(),
				progressType: 'Visit',
				visitResult: visitResult,
				nextStep: nextStep,
				description: description,
				notes: notes,
			});

			if (response.success) {
				// ✅ Check if OfferRequest was created
				if (response.data.offerRequestId) {
					Alert.alert(
						'Success',
						'Task progress created! Offer request has been automatically created and sent to Sales Support.',
						[
							{
								text: 'OK',
								onPress: () =>
									onSuccess &&
									onSuccess(),
							},
						]
					);
				} else {
					Alert.alert(
						'Success',
						'Task progress created successfully!',
						[
							{
								text: 'OK',
								onPress: () =>
									onSuccess &&
									onSuccess(),
							},
						]
					);
				}
			}
		} catch (error: any) {
			Alert.alert(
				'Error',
				error.response?.data?.message ||
					'Failed to create task progress'
			);
		} finally {
			setLoading(false);
		}
	};

	return (
		<View>
			<TextInput
				placeholder="Description"
				value={description}
				onChangeText={setDescription}
				multiline
			/>

			<TextInput
				placeholder="Notes"
				value={notes}
				onChangeText={setNotes}
				multiline
			/>

			{/* Visit Result Picker */}
			<Picker
				selectedValue={visitResult}
				onValueChange={setVisitResult}
				placeholder="Select Visit Result"
			>
				<Picker.Item
					label="Interested"
					value="Interested"
				/>
				<Picker.Item
					label="Not Interested"
					value="NotInterested"
				/>
			</Picker>

			{/* Next Step Picker */}
			{visitResult === 'Interested' && (
				<Picker
					selectedValue={nextStep}
					onValueChange={setNextStep}
					placeholder="Select Next Step"
				>
					<Picker.Item
						label="Needs Offer"
						value="NeedsOffer"
					/>
					<Picker.Item
						label="Needs Deal"
						value="NeedsDeal"
					/>
				</Picker>
			)}

			{/* Info message */}
			{visitResult === 'Interested' &&
				nextStep === 'NeedsOffer' && (
					<View
						style={{
							padding: 10,
							backgroundColor:
								'#e3f2fd',
						}}
					>
						<Text
							style={{
								color: '#1976d2',
							}}
						>
							ℹ️ This will
							automatically create an
							offer request and notify
							Sales Support
						</Text>
					</View>
				)}

			<Button
				title={
					loading
						? 'Creating...'
						: 'Create Progress'
				}
				onPress={handleSubmit}
				disabled={loading || !visitResult || !nextStep}
			/>
		</View>
	);
};
```

---

## 📚 API Endpoints Reference

### 1. Create TaskProgress (Auto-Create OfferRequest)

```http
POST /api/TaskProgress
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 150,
  "progressDate": "2024-01-15T10:00:00Z",
  "progressType": "Visit",
  "visitResult": "Interested",
  "nextStep": "NeedsOffer",
  "description": "Client needs Portable X-Ray equipment",
  "notes": "Client prefers delivery within 2 weeks"
}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"id": 123,
		"taskId": 150,
		"clientId": 456,
		"clientName": "Cairo Hospital",
		"progressDate": "2024-01-15T10:00:00Z",
		"progressType": "Visit",
		"visitResult": "Interested",
		"nextStep": "NeedsOffer",
		"offerRequestId": 789, // ✅ Set if OfferRequest was created
		"createdAt": "2024-01-15T10:00:00Z"
	},
	"message": "Task progress created successfully"
}
```

### 2. Create TaskProgress with Explicit OfferRequest

```http
POST /api/TaskProgress/with-offer-request
Authorization: Bearer {token}
Content-Type: application/json

{
  "taskId": 150,
  "clientId": 456,
  "progressDate": "2024-01-15T10:00:00Z",
  "progressType": "Visit",
  "visitResult": "Interested",
  "nextStep": "NeedsOffer",
  "requestedProducts": "Portable X-Ray unit Model XYZ",
  "specialNotes": "Delivery to Cairo location within 2 weeks"
}
```

### 3. Get My Tasks

```http
GET /api/WeeklyPlan/current
Authorization: Bearer {token}
```

### 4. Get Task Progresses

```http
GET /api/TaskProgress/task/{taskId}
Authorization: Bearer {token}
```

---

## 🔍 Important Notes

### Auto-Create Conditions

An OfferRequest is **automatically created** when:

1. ✅ `visitResult` = `"Interested"`
2. ✅ `nextStep` = `"NeedsOffer"`
3. ✅ Task has a `clientId` (not null)

If any of these conditions are not met, the TaskProgress is created but **no OfferRequest** is created.

### What Happens When OfferRequest is Created

1. **OfferRequest** is created in database
2. **Automatically assigned** to a SalesSupport user
3. **Notifications** are sent to all SalesSupport users via SignalR
4. **Response includes** `offerRequestId` so you know it was created

### Error Handling

```typescript
try {
	const response = await createTaskProgress(data);
	// Handle success
} catch (error: any) {
	if (error.response) {
		// Server responded with error
		const status = error.response.status;
		const message = error.response.data?.message;

		if (status === 401) {
			// Unauthorized - token expired
			Alert.alert('Session Expired', 'Please login again');
			// Navigate to login
		} else if (status === 403) {
			// Forbidden - no permission
			Alert.alert(
				'Permission Denied',
				'You do not have permission'
			);
		} else {
			Alert.alert('Error', message || 'Something went wrong');
		}
	} else if (error.request) {
		// Request made but no response
		Alert.alert(
			'Network Error',
			'Please check your internet connection'
		);
	} else {
		// Other error
		Alert.alert('Error', 'An unexpected error occurred');
	}
}
```

---

## ✅ Checklist

Before implementation:

- [ ] Install required packages (`@react-native-community/async-storage`, `axios`)
- [ ] Configure API base URL
- [ ] Store JWT token in AsyncStorage
- [ ] Include token in all API requests
- [ ] Handle token expiration (401 errors)
- [ ] Validate `visitResult` and `nextStep` before submission
- [ ] Show user feedback when OfferRequest is auto-created
- [ ] Handle network errors gracefully

---

## 📱 Complete Example App Structure

```
src/
├── services/
│   ├── api.ts                 # Axios instance with token
│   ├── TaskProgressService.ts # TaskProgress API calls
│   └── WeeklyPlanService.ts   # WeeklyPlan API calls
├── components/
│   ├── TaskProgressForm.tsx   # Form to create progress
│   └── TaskList.tsx           # List of tasks
├── screens/
│   ├── TaskDetailScreen.tsx   # Task detail with progress form
│   └── WeeklyPlanScreen.tsx   # Weekly plan view
└── utils/
    └── storage.ts              # AsyncStorage helpers
```

---

**Last Updated:** 2024-01-15  
**Target Platform:** React Native (iOS & Android)  
**User Role:** Salesman
