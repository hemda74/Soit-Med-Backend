# Weekly Plan API Guide for React Native Team

## 🎯 Overview

This guide provides everything the React Native team needs to implement the **Weekly Plan** system for salesmen. This replaces the old Sales Report system with a more structured approach.

## 📱 What Salesmen Can Do

- ✅ **Create weekly plans** with multiple tasks
- ✅ **Add daily progress** updates throughout the week
- ✅ **Manage tasks** (add, edit, delete, mark complete)
- ✅ **View their plans** and progress
- ❌ **Cannot see other salesmen's plans** (only managers can)

## 🔗 Base URL

```
http://YOUR_COMPUTER_IP:5117/api/WeeklyPlan
```

**Important:** Replace `YOUR_COMPUTER_IP` with your actual computer's IP address (e.g., `192.168.1.182`)

---

## 🔐 Authentication

All requests require a JWT token in the header:

```javascript
headers: {
  'Authorization': `Bearer ${userToken}`,
  'Content-Type': 'application/json'
}
```

---

## 📋 API Endpoints

### 1. Create Weekly Plan

**POST** `/api/WeeklyPlan`

**Role:** Salesman only

**Purpose:** Create a new weekly plan with tasks

```javascript
const createWeeklyPlan = async (planData) => {
	const response = await fetch('http://YOUR_IP:5117/api/WeeklyPlan', {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'Content-Type': 'application/json',
		},
		body: JSON.stringify({
			title: 'Week 1 October Plan',
			description: 'Sales visits to Cairo hospitals',
			weekStartDate: '2024-10-01',
			weekEndDate: '2024-10-07',
			tasks: [
				{
					title: 'Visit Hospital 57357',
					description:
						'Present new medical equipment',
					displayOrder: 1,
				},
				{
					title: 'Follow up with Dar Al Fouad',
					description:
						"Follow up on last week's proposal",
					displayOrder: 2,
				},
			],
		}),
	});

	return await response.json();
};
```

**Response:**

```javascript
{
  "success": true,
  "message": "Weekly plan created successfully",
  "data": {
    "id": 1,
    "title": "Week 1 October Plan",
    "description": "Sales visits to Cairo hospitals",
    "weekStartDate": "2024-10-01",
    "weekEndDate": "2024-10-07",
    "employeeId": "emp123",
    "employeeName": "Ahmed Mohamed",
    "rating": null,
    "managerComment": null,
    "managerReviewedAt": null,
    "createdAt": "2024-10-01T08:00:00Z",
    "updatedAt": "2024-10-01T08:00:00Z",
    "isActive": true,
    "tasks": [
      {
        "id": 1,
        "weeklyPlanId": 1,
        "title": "Visit Hospital 57357",
        "description": "Present new medical equipment",
        "isCompleted": false,
        "displayOrder": 1,
        "createdAt": "2024-10-01T08:00:00Z",
        "updatedAt": "2024-10-01T08:00:00Z"
      }
    ],
    "dailyProgresses": [],
    "totalTasks": 2,
    "completedTasks": 0,
    "completionPercentage": 0.0
  }
}
```

---

### 2. Get All Weekly Plans

**GET** `/api/WeeklyPlan`

**Role:** Salesman (own plans only)

**Purpose:** Get all weekly plans with optional filtering

```javascript
const getWeeklyPlans = async (filters = {}) => {
	const queryParams = new URLSearchParams(filters);
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan?${queryParams}`,
		{
			method: 'GET',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		}
	);

	return await response.json();
};

// Examples:
// Get all plans: getWeeklyPlans()
// Get plans for specific week: getWeeklyPlans({ startDate: '2024-10-01', endDate: '2024-10-07' })
// Pagination: getWeeklyPlans({ page: 1, pageSize: 10 })
```

**Query Parameters:**

- `startDate` (optional): Filter from date (YYYY-MM-DD)
- `endDate` (optional): Filter to date (YYYY-MM-DD)
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 100)

---

### 3. Get Single Weekly Plan

**GET** `/api/WeeklyPlan/{id}`

**Role:** Salesman (own plans only)

**Purpose:** Get detailed view of a specific plan

```javascript
const getWeeklyPlan = async (planId) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}`,
		{
			method: 'GET',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		}
	);

	return await response.json();
};
```

---

### 4. Update Weekly Plan

**PUT** `/api/WeeklyPlan/{id}`

**Role:** Salesman (own plans only)

**Purpose:** Update plan title and description

```javascript
const updateWeeklyPlan = async (planId, updateData) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}`,
		{
			method: 'PUT',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				title: 'Updated Week 1 October Plan',
				description: 'Updated description',
			}),
		}
	);

	return await response.json();
};
```

---

### 5. Delete Weekly Plan

**DELETE** `/api/WeeklyPlan/{id}`

**Role:** Salesman (own plans only)

**Purpose:** Delete a weekly plan

```javascript
const deleteWeeklyPlan = async (planId) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}`,
		{
			method: 'DELETE',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		}
	);

	return await response.json();
};
```

---

## 📝 Task Management

### 6. Add Task to Plan

**POST** `/api/WeeklyPlan/{planId}/tasks`

**Role:** Salesman (own plans only)

```javascript
const addTask = async (planId, taskData) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/tasks`,
		{
			method: 'POST',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				title: 'Call Al Galaa Hospital',
				description: 'Follow up on urgent request',
				displayOrder: 3,
			}),
		}
	);

	return await response.json();
};
```

### 7. Update Task

**PUT** `/api/WeeklyPlan/{planId}/tasks/{taskId}`

**Role:** Salesman (own plans only)

```javascript
const updateTask = async (planId, taskId, taskData) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/tasks/${taskId}`,
		{
			method: 'PUT',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				title: 'Call Al Galaa Hospital - URGENT',
				description: 'Follow up on urgent request',
				isCompleted: true,
				displayOrder: 1,
			}),
		}
	);

	return await response.json();
};
```

### 8. Delete Task

**DELETE** `/api/WeeklyPlan/{planId}/tasks/{taskId}`

**Role:** Salesman (own plans only)

```javascript
const deleteTask = async (planId, taskId) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/tasks/${taskId}`,
		{
			method: 'DELETE',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		}
	);

	return await response.json();
};
```

---

## 📊 Daily Progress Management

### 9. Add Daily Progress

**POST** `/api/WeeklyPlan/{planId}/progress`

**Role:** Salesman (own plans only)

**Purpose:** Add daily progress update

```javascript
const addDailyProgress = async (planId, progressData) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/progress`,
		{
			method: 'POST',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				progressDate: '2024-10-01',
				notes: 'Today I visited Hospital 57357 and presented all new products. We agreed on a second meeting next week to discuss the proposal. I also called Dar Al Fouad Hospital and confirmed the appointment for Wednesday.',
				tasksWorkedOn: [1, 2], // Array of task IDs
			}),
		}
	);

	return await response.json();
};
```

### 10. Update Daily Progress

**PUT** `/api/WeeklyPlan/{planId}/progress/{progressId}`

**Role:** Salesman (own plans only)

```javascript
const updateDailyProgress = async (planId, progressId, progressData) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/progress/${progressId}`,
		{
			method: 'PUT',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				notes: 'Updated: Today I visited Hospital 57357 and presented all products. We agreed on a second meeting. I also called Dar Al Fouad and confirmed. Additionally, I started preparing the monthly report.',
				tasksWorkedOn: [1, 2, 3],
			}),
		}
	);

	return await response.json();
};
```

### 11. Delete Daily Progress

**DELETE** `/api/WeeklyPlan/{planId}/progress/{progressId}`

**Role:** Salesman (own plans only)

```javascript
const deleteDailyProgress = async (planId, progressId) => {
	const response = await fetch(
		`http://YOUR_IP:5117/api/WeeklyPlan/${planId}/progress/${progressId}`,
		{
			method: 'DELETE',
			headers: {
				Authorization: `Bearer ${token}`,
				'Content-Type': 'application/json',
			},
		}
	);

	return await response.json();
};
```

---

## 🎨 React Native Implementation Examples

### API Service Class

```javascript
// services/WeeklyPlanService.js
class WeeklyPlanService {
	constructor(baseURL, token) {
		this.baseURL = baseURL;
		this.token = token;
	}

	async request(endpoint, options = {}) {
		const url = `${this.baseURL}${endpoint}`;
		const config = {
			headers: {
				Authorization: `Bearer ${this.token}`,
				'Content-Type': 'application/json',
				...options.headers,
			},
			...options,
		};

		const response = await fetch(url, config);
		const data = await response.json();

		if (!response.ok) {
			throw new Error(data.message || 'Request failed');
		}

		return data;
	}

	// Create weekly plan
	async createPlan(planData) {
		return this.request('/api/WeeklyPlan', {
			method: 'POST',
			body: JSON.stringify(planData),
		});
	}

	// Get all plans
	async getPlans(filters = {}) {
		const queryParams = new URLSearchParams(filters);
		return this.request(`/api/WeeklyPlan?${queryParams}`);
	}

	// Get single plan
	async getPlan(planId) {
		return this.request(`/api/WeeklyPlan/${planId}`);
	}

	// Update plan
	async updatePlan(planId, updateData) {
		return this.request(`/api/WeeklyPlan/${planId}`, {
			method: 'PUT',
			body: JSON.stringify(updateData),
		});
	}

	// Delete plan
	async deletePlan(planId) {
		return this.request(`/api/WeeklyPlan/${planId}`, {
			method: 'DELETE',
		});
	}

	// Add task
	async addTask(planId, taskData) {
		return this.request(`/api/WeeklyPlan/${planId}/tasks`, {
			method: 'POST',
			body: JSON.stringify(taskData),
		});
	}

	// Update task
	async updateTask(planId, taskId, taskData) {
		return this.request(
			`/api/WeeklyPlan/${planId}/tasks/${taskId}`,
			{
				method: 'PUT',
				body: JSON.stringify(taskData),
			}
		);
	}

	// Delete task
	async deleteTask(planId, taskId) {
		return this.request(
			`/api/WeeklyPlan/${planId}/tasks/${taskId}`,
			{
				method: 'DELETE',
			}
		);
	}

	// Add daily progress
	async addDailyProgress(planId, progressData) {
		return this.request(`/api/WeeklyPlan/${planId}/progress`, {
			method: 'POST',
			body: JSON.stringify(progressData),
		});
	}

	// Update daily progress
	async updateDailyProgress(planId, progressId, progressData) {
		return this.request(
			`/api/WeeklyPlan/${planId}/progress/${progressId}`,
			{
				method: 'PUT',
				body: JSON.stringify(progressData),
			}
		);
	}

	// Delete daily progress
	async deleteDailyProgress(planId, progressId) {
		return this.request(
			`/api/WeeklyPlan/${planId}/progress/${progressId}`,
			{
				method: 'DELETE',
			}
		);
	}
}

export default WeeklyPlanService;
```

### Usage in Components

```javascript
// components/WeeklyPlanScreen.js
import React, { useState, useEffect } from 'react';
import { View, Text, FlatList, TouchableOpacity, Alert } from 'react-native';
import WeeklyPlanService from '../services/WeeklyPlanService';

const WeeklyPlanScreen = ({ route, navigation }) => {
	const [plans, setPlans] = useState([]);
	const [loading, setLoading] = useState(true);
	const [apiService] = useState(
		new WeeklyPlanService('http://YOUR_IP:5117', userToken)
	);

	useEffect(() => {
		loadPlans();
	}, []);

	const loadPlans = async () => {
		try {
			setLoading(true);
			const response = await apiService.getPlans();
			setPlans(response.data.data);
		} catch (error) {
			Alert.alert('Error', error.message);
		} finally {
			setLoading(false);
		}
	};

	const createNewPlan = async () => {
		try {
			const newPlan = {
				title: 'New Weekly Plan',
				description: 'Plan description',
				weekStartDate: '2024-10-01',
				weekEndDate: '2024-10-07',
				tasks: [],
			};

			const response = await apiService.createPlan(newPlan);
			Alert.alert('Success', 'Plan created successfully');
			loadPlans(); // Refresh the list
		} catch (error) {
			Alert.alert('Error', error.message);
		}
	};

	const addTaskToPlan = async (planId) => {
		try {
			const newTask = {
				title: 'New Task',
				description: 'Task description',
				displayOrder: 1,
			};

			await apiService.addTask(planId, newTask);
			Alert.alert('Success', 'Task added successfully');
			loadPlans(); // Refresh the list
		} catch (error) {
			Alert.alert('Error', error.message);
		}
	};

	const addDailyProgress = async (planId) => {
		try {
			const progress = {
				progressDate: new Date()
					.toISOString()
					.split('T')[0], // Today's date
				notes: "Today's progress notes...",
				tasksWorkedOn: [1, 2], // Task IDs
			};

			await apiService.addDailyProgress(planId, progress);
			Alert.alert(
				'Success',
				'Daily progress added successfully'
			);
			loadPlans(); // Refresh the list
		} catch (error) {
			Alert.alert('Error', error.message);
		}
	};

	const renderPlan = ({ item }) => (
		<View style={styles.planCard}>
			<Text style={styles.planTitle}>{item.title}</Text>
			<Text style={styles.planDescription}>
				{item.description}
			</Text>
			<Text style={styles.progressText}>
				Progress: {item.completedTasks}/
				{item.totalTasks} tasks (
				{item.completionPercentage}%)
			</Text>

			<View style={styles.buttonRow}>
				<TouchableOpacity
					style={styles.button}
					onPress={() => addTaskToPlan(item.id)}
				>
					<Text style={styles.buttonText}>
						Add Task
					</Text>
				</TouchableOpacity>

				<TouchableOpacity
					style={styles.button}
					onPress={() =>
						addDailyProgress(item.id)
					}
				>
					<Text style={styles.buttonText}>
						Add Progress
					</Text>
				</TouchableOpacity>
			</View>
		</View>
	);

	if (loading) {
		return (
			<View style={styles.center}>
				<Text>Loading plans...</Text>
			</View>
		);
	}

	return (
		<View style={styles.container}>
			<TouchableOpacity
				style={styles.createButton}
				onPress={createNewPlan}
			>
				<Text style={styles.createButtonText}>
					Create New Plan
				</Text>
			</TouchableOpacity>

			<FlatList
				data={plans}
				renderItem={renderPlan}
				keyExtractor={(item) => item.id.toString()}
				style={styles.list}
			/>
		</View>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		padding: 16,
	},
	center: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
	},
	createButton: {
		backgroundColor: '#007AFF',
		padding: 16,
		borderRadius: 8,
		marginBottom: 16,
	},
	createButtonText: {
		color: 'white',
		textAlign: 'center',
		fontWeight: 'bold',
	},
	planCard: {
		backgroundColor: 'white',
		padding: 16,
		borderRadius: 8,
		marginBottom: 12,
		shadowColor: '#000',
		shadowOffset: { width: 0, height: 2 },
		shadowOpacity: 0.1,
		shadowRadius: 4,
		elevation: 3,
	},
	planTitle: {
		fontSize: 18,
		fontWeight: 'bold',
		marginBottom: 8,
	},
	planDescription: {
		fontSize: 14,
		color: '#666',
		marginBottom: 8,
	},
	progressText: {
		fontSize: 12,
		color: '#007AFF',
		marginBottom: 12,
	},
	buttonRow: {
		flexDirection: 'row',
		justifyContent: 'space-between',
	},
	button: {
		backgroundColor: '#34C759',
		padding: 8,
		borderRadius: 4,
		flex: 0.48,
	},
	buttonText: {
		color: 'white',
		textAlign: 'center',
		fontSize: 12,
	},
	list: {
		flex: 1,
	},
});

export default WeeklyPlanScreen;
```

---

## ⚠️ Important Notes

### 1. **Date Format**

- All dates must be in `YYYY-MM-DD` format
- Use `new Date().toISOString().split('T')[0]` to get today's date

### 2. **Error Handling**

- Always wrap API calls in try-catch blocks
- Check response status before processing data
- Show user-friendly error messages

### 3. **Authentication**

- Store JWT token securely (AsyncStorage)
- Refresh token when it expires
- Handle 401 Unauthorized responses

### 4. **Network Configuration**

- Use your computer's IP address, not localhost
- Ensure both devices are on the same Wi-Fi
- For Android, add `usesCleartextTraffic: true` in app.json

### 5. **Validation Rules**

- `title`: Required, max 200 characters
- `description`: Optional, max 1000 characters
- `weekStartDate`: Required, must be valid date
- `weekEndDate`: Required, must be after start date
- `notes` (daily progress): Required, max 2000 characters

---

## 🚀 Quick Start Checklist

- [ ] Replace `YOUR_IP` with actual computer IP address
- [ ] Implement authentication with JWT token
- [ ] Create WeeklyPlanService class
- [ ] Implement basic CRUD operations
- [ ] Add error handling
- [ ] Test with real API endpoints
- [ ] Add loading states and user feedback

---

## 📞 Support

If you encounter any issues:

1. Check the API is running: `http://YOUR_IP:5117/swagger`
2. Verify authentication token is valid
3. Check network connectivity
4. Review error messages in console

**Last Updated:** October 6, 2025
