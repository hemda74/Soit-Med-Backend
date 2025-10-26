# Weekly Plan API - Complete Testing Guide

**Version**: 1.0  
**Date**: 2025-10-26  
**Base URL**: `http://localhost:5117/api`  
**For**: Frontend Development Teams

---

## Overview

This document provides complete testing instructions for all Weekly Plan endpoints. Use this guide to test the API and integrate with React/React Native applications.

---

## Table of Contents

1. [Authentication](#authentication)
2. [Test Credentials](#test-credentials)
3. [API Endpoints Reference](#api-endpoints-reference)
4. [Testing with cURL](#testing-with-curl)
5. [Testing with Postman](#testing-with-postman)
6. [Testing with React](#testing-with-react)
7. [Testing with React Native](#testing-with-react-native)
8. [Expected Responses](#expected-responses)
9. [Error Handling](#error-handling)

---

## Authentication

All Weekly Plan endpoints require JWT Bearer Token authentication.

### Headers Required

```http
Authorization: Bearer {your_jwt_token}
Content-Type: application/json
```

---

## Test Credentials

### Sales Manager

- **Email**: salesmanager@soitmed.com
- **Password**: 356120Ahmed@shraf2

### Sales Support

- **Email**: salessupport@soitmed.com
- **Password**: 356120Ahmed@shraf2

### Salesman

- **Email**: ahmed@soitmed.com
- **Password**: 356120Ahmed@shraf2

---

## API Endpoints Reference

### 1. Get All Weekly Plans (Paginated)

**Endpoint**: `GET /api/weeklyplan`  
**Authorization**: Required (All authenticated users)

**Query Parameters**:

- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 20, max: 100) - Items per page

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": {
		"plans": [
			{
				"id": 42,
				"employeeId": "Ahmed_Ashraf_Sales_001",
				"weekStartDate": "2025-10-27T00:00:00",
				"weekEndDate": "2025-11-02T23:59:59",
				"title": "Week 44 Sales Plan",
				"description": "Current week active plan",
				"isActive": true,
				"rating": 4,
				"managerComment": "Good work!",
				"createdAt": "2025-10-26T12:00:00",
				"updatedAt": "2025-10-26T12:00:00",
				"tasks": []
			}
		],
		"pagination": {
			"page": 1,
			"pageSize": 20,
			"totalCount": 13,
			"totalPages": 1
		}
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

---

### 2. Get Current Active Weekly Plan

**Endpoint**: `GET /api/weeklyplan/current`  
**Authorization**: Required (All authenticated users)

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": {
		"id": 42,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"weekStartDate": "2025-10-27T00:00:00",
		"weekEndDate": "2025-11-02T23:59:59",
		"title": "Week 44 Sales Plan",
		"description": "Current week's sales activities",
		"isActive": true,
		"createdAt": "2025-10-26T12:00:00",
		"updatedAt": "2025-10-26T12:00:00",
		"tasks": [
			{
				"id": 80,
				"taskType": "Visit",
				"clientId": 24,
				"title": "Visit Cairo Hospital",
				"priority": "High",
				"status": "Planned"
			}
		]
	},
	"message": "Operation completed successfully"
}
```

**Expected Response (404 Not Found)** - No current plan:

```json
{
	"success": false,
	"message": "No current weekly plan found"
}
```

---

### 3. Get Specific Weekly Plan by ID

**Endpoint**: `GET /api/weeklyplan/{id}`  
**Authorization**: Required (All authenticated users)

**Path Parameters**:

- `id` - Weekly plan ID (long)

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": {
		"id": 42,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"weekStartDate": "2025-10-27T00:00:00",
		"weekEndDate": "2025-11-02T23:59:59",
		"title": "Week 44 Sales Plan",
		"description": "Focus on client follow-ups",
		"isActive": false,
		"rating": 4,
		"managerComment": "Good plan, well structured",
		"managerReviewedAt": "2025-10-26T12:00:00",
		"createdAt": "2025-10-26T12:00:00",
		"updatedAt": "2025-10-26T12:00:00",
		"tasks": [
			{
				"id": 80,
				"taskType": "Visit",
				"clientId": 24,
				"title": "Visit Cairo Hospital",
				"status": "Completed",
				"priority": "High"
			}
		]
	},
	"message": "Operation completed successfully"
}
```

---

### 4. Create Weekly Plan

**Endpoint**: `POST /api/weeklyplan`  
**Authorization**: Required (All authenticated users)

**Request Body**:

```json
{
	"weekStartDate": "2025-11-03T00:00:00",
	"weekEndDate": "2025-11-09T23:59:59",
	"title": "Week 45 Sales Plan",
	"description": "Focus on deal closures and follow-ups"
}
```

**Request Body Schema**:

```typescript
{
  weekStartDate: string; // DateTime, required
  weekEndDate: string;   // DateTime, required
  title: string;          // max 200 chars, required
  description?: string;   // max 1000 chars, optional
}
```

**Validation Rules**:

- `weekStartDate`: Required, must be a valid date
- `weekEndDate`: Required, must be after `weekStartDate`
- `title`: Required, max 200 characters
- `description`: Optional, max 1000 characters

**Expected Response (201 Created)**:

```json
{
	"success": true,
	"data": {
		"id": 43,
		"employeeId": "Ahmed_Ashraf_Sales_001",
		"weekStartDate": "2025-11-03T00:00:00",
		"weekEndDate": "2025-11-09T23:59:59",
		"title": "Week 45 Sales Plan",
		"description": "Focus on deal closures and follow-ups",
		"isActive": true,
		"rating": null,
		"managerComment": null,
		"managerReviewedAt": null,
		"createdAt": "2025-10-26T12:00:00",
		"updatedAt": "2025-10-26T12:00:00",
		"tasks": []
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-10-26T12:00:00Z"
}
```

**Error Response (400 Bad Request)**:

```json
{
	"success": false,
	"message": "Week end date must be after start date"
}
```

---

### 5. Update Weekly Plan

**Endpoint**: `PUT /api/weeklyplan/{id}`  
**Authorization**: Required (Only own plans)

**Path Parameters**:

- `id` - Weekly plan ID (long)

**Request Body** (all fields optional):

```json
{
	"title": "Updated Week 45 Sales Plan",
	"description": "Updated: Focus on high-priority clients"
}
```

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": {
		"id": 43,
		"title": "Updated Week 45 Sales Plan",
		"description": "Updated: Focus on high-priority clients",
		"updatedAt": "2025-10-26T12:30:00"
	},
	"message": "Operation completed successfully"
}
```

---

### 6. Submit Weekly Plan for Review

**Endpoint**: `POST /api/weeklyplan/{id}/submit`  
**Authorization**: Required (Only own plans)

**Path Parameters**:

- `id` - Weekly plan ID (long)

**Request Body**: None required

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": "Weekly plan submitted successfully",
	"message": "Operation completed successfully"
}
```

**Error Response (400 Bad Request)**:

```json
{
	"success": false,
	"message": "Plan has already been submitted"
}
```

---

### 7. Review Weekly Plan (Manager Only)

**Endpoint**: `POST /api/weeklyplan/{id}/review`  
**Authorization**: Required (Admin, SalesManager only)

**Path Parameters**:

- `id` - Weekly plan ID (long)

**Request Body**:

```json
{
	"rating": 4,
	"comment": "Good plan, well structured. Keep up the good work!"
}
```

**Request Body Schema**:

```typescript
{
  rating?: number;      // 1-5, optional
  comment?: string;     // max 1000 chars, optional
}
```

**Validation Rules**:

- `rating`: Optional, must be between 1 and 5
- `comment`: Optional, max 1000 characters

**Expected Response (200 OK)**:

```json
{
	"success": true,
	"data": "Weekly plan reviewed successfully",
	"message": "Operation completed successfully"
}
```

**Error Response (403 Forbidden)**:

```json
{
	"success": false,
	"message": "Forbidden - Manager access required"
}
```

---

## Testing with cURL

### 1. Get Authentication Token

```bash
curl -X POST "http://localhost:5117/api/account/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "salesmanager@soitmed.com",
    "password": "356120Ahmed@shraf2"
  }'
```

**Response**:

```json
{
	"success": true,
	"data": {
		"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
		"expiresIn": 3600,
		"user": {
			"id": "Ahmed_Hemdan_Sales_002",
			"email": "salesmanager@soitmed.com",
			"roles": ["SalesManager"]
		}
	}
}
```

Save the `token` value for subsequent requests.

---

### 2. Get All Weekly Plans

```bash
curl -X GET "http://localhost:5117/api/weeklyplan?page=1&pageSize=20" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### 3. Get Current Weekly Plan

```bash
curl -X GET "http://localhost:5117/api/weeklyplan/current" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### 4. Get Specific Plan by ID

```bash
curl -X GET "http://localhost:5117/api/weeklyplan/42" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### 5. Create New Weekly Plan

```bash
curl -X POST "http://localhost:5117/api/weeklyplan" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "weekStartDate": "2025-11-03T00:00:00",
    "weekEndDate": "2025-11-09T23:59:59",
    "title": "Week 45 Sales Plan",
    "description": "Focus on deal closures and follow-ups"
  }'
```

---

### 6. Update Weekly Plan

```bash
curl -X PUT "http://localhost:5117/api/weeklyplan/43" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Week 45 Sales Plan",
    "description": "Updated description"
  }'
```

---

### 7. Submit Weekly Plan

```bash
curl -X POST "http://localhost:5117/api/weeklyplan/43/submit" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

### 8. Review Weekly Plan (Manager Only)

```bash
curl -X POST "http://localhost:5117/api/weeklyplan/43/review" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": 4,
    "comment": "Good work! Excellent planning."
  }'
```

---

## Testing with Postman

### Collection Setup

1. **Import Collection**: Create a new Postman collection named "Weekly Plans API"
2. **Set Variables**:

      - `baseUrl`: `http://localhost:5117/api`
      - `token`: Your JWT token (set after login)

3. **Add Authorization to Collection**:
      - Type: Bearer Token
      - Token: `{{token}}`

### Request Examples

Create these requests in Postman:

1. **Login** (POST)

      - URL: `{{baseUrl}}/account/login`
      - Body:
           ```json
           {
           	"email": "salesmanager@soitmed.com",
           	"password": "356120Ahmed@shraf2"
           }
           ```
      - Tests: Save token to environment

2. **Get All Plans** (GET)

      - URL: `{{baseUrl}}/weeklyplan?page=1&pageSize=20`

3. **Get Current Plan** (GET)

      - URL: `{{baseUrl}}/weeklyplan/current`

4. **Get Plan by ID** (GET)

      - URL: `{{baseUrl}}/weeklyplan/42`

5. **Create Plan** (POST)

      - URL: `{{baseUrl}}/weeklyplan`
      - Body:
           ```json
           {
           	"weekStartDate": "2025-11-03T00:00:00",
           	"weekEndDate": "2025-11-09T23:59:59",
           	"title": "Week 45 Sales Plan",
           	"description": "Test plan"
           }
           ```

6. **Update Plan** (PUT)

      - URL: `{{baseUrl}}/weeklyplan/43`
      - Body:
           ```json
           {
           	"title": "Updated Title"
           }
           ```

7. **Submit Plan** (POST)

      - URL: `{{baseUrl}}/weeklyplan/43/submit`

8. **Review Plan** (POST) - Manager Only
      - URL: `{{baseUrl}}/weeklyplan/43/review`
      - Body:
           ```json
           {
           	"rating": 4,
           	"comment": "Good work!"
           }
           ```

---

## Testing with React

```jsx
import React, { useState, useEffect } from 'react';
import axios from 'axios';

const WeeklyPlanTesting = () => {
	const [token, setToken] = useState('');
	const [plans, setPlans] = useState([]);
	const [currentPlan, setCurrentPlan] = useState(null);
	const [loading, setLoading] = useState(false);

	// Login
	const login = async () => {
		try {
			const response = await axios.post(
				'http://localhost:5117/api/account/login',
				{
					email: 'salesmanager@soitmed.com',
					password: '356120Ahmed@shraf2',
				}
			);
			setToken(response.data.data.token);
		} catch (error) {
			console.error('Login failed:', error);
		}
	};

	// Get All Plans
	const fetchPlans = async () => {
		setLoading(true);
		try {
			const response = await axios.get(
				'http://localhost:5117/api/weeklyplan',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
					params: { page: 1, pageSize: 20 },
				}
			);
			setPlans(response.data.data.plans);
		} catch (error) {
			console.error('Error fetching plans:', error);
		} finally {
			setLoading(false);
		}
	};

	// Get Current Plan
	const fetchCurrentPlan = async () => {
		try {
			const response = await axios.get(
				'http://localhost:5117/api/weeklyplan/current',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setCurrentPlan(response.data.data);
		} catch (error) {
			console.error('Error fetching current plan:', error);
		}
	};

	// Create Plan
	const createPlan = async () => {
		try {
			const response = await axios.post(
				'http://localhost:5117/api/weeklyplan',
				{
					weekStartDate: '2025-11-03T00:00:00',
					weekEndDate: '2025-11-09T23:59:59',
					title: 'Test Plan',
					description: 'Test description',
				},
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			alert('Plan created!');
			fetchPlans(); // Refresh list
		} catch (error) {
			console.error('Error creating plan:', error);
			alert('Failed to create plan');
		}
	};

	// Submit Plan
	const submitPlan = async (planId) => {
		try {
			await axios.post(
				`http://localhost:5117/api/weeklyplan/${planId}/submit`,
				{},
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			alert('Plan submitted successfully!');
			fetchPlans();
		} catch (error) {
			console.error('Error submitting plan:', error);
			alert('Failed to submit plan');
		}
	};

	useEffect(() => {
		if (token) {
			fetchPlans();
		}
	}, [token]);

	if (!token) {
		return (
			<div>
				<button onClick={login}>Login</button>
			</div>
		);
	}

	return (
		<div>
			<h2>Weekly Plans Testing</h2>

			<button onClick={fetchPlans}>Refresh Plans</button>
			<button onClick={fetchCurrentPlan}>
				Get Current Plan
			</button>
			<button onClick={createPlan}>Create Plan</button>

			<h3>Current Plan</h3>
			{currentPlan && (
				<div>
					<p>
						<strong>Title:</strong>{' '}
						{currentPlan.title}
					</p>
					<p>
						<strong>Status:</strong>{' '}
						{currentPlan.isActive
							? 'Active'
							: 'Inactive'}
					</p>
				</div>
			)}

			<h3>All Plans ({plans.length})</h3>
			{loading && <p>Loading...</p>}
			{plans.map((plan) => (
				<div
					key={plan.id}
					style={{
						border: '1px solid #ccc',
						padding: '10px',
						margin: '10px',
					}}
				>
					<p>
						<strong>ID:</strong> {plan.id}
					</p>
					<p>
						<strong>Title:</strong>{' '}
						{plan.title}
					</p>
					<p>
						<strong>Active:</strong>{' '}
						{plan.isActive ? 'Yes' : 'No'}
					</p>
					<p>
						<strong>Rating:</strong>{' '}
						{plan.rating || 'Not rated'}
					</p>
					<button
						onClick={() =>
							submitPlan(plan.id)
						}
					>
						Submit
					</button>
				</div>
			))}
		</div>
	);
};

export default WeeklyPlanTesting;
```

---

## Testing with React Native

```jsx
import React, { useState, useEffect } from 'react';
import { View, Text, Button, FlatList, Alert } from 'react-native';
import axios from 'axios';

const WeeklyPlanTesting = () => {
	const [token, setToken] = useState('');
	const [plans, setPlans] = useState([]);

	const login = async () => {
		try {
			const response = await axios.post(
				'http://localhost:5117/api/account/login',
				{
					email: 'salesmanager@soitmed.com',
					password: '356120Ahmed@shraf2',
				}
			);
			setToken(response.data.data.token);
			Alert.alert('Success', 'Logged in successfully');
		} catch (error) {
			Alert.alert('Error', 'Login failed');
		}
	};

	const fetchPlans = async () => {
		try {
			const response = await axios.get(
				'http://localhost:5117/api/weeklyplan',
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setPlans(response.data.data.plans);
		} catch (error) {
			Alert.alert('Error', 'Failed to fetch plans');
		}
	};

	const submitPlan = async (planId) => {
		try {
			await axios.post(
				`http://localhost:5117/api/weeklyplan/${planId}/submit`,
				{},
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			Alert.alert('Success', 'Plan submitted');
			fetchPlans();
		} catch (error) {
			Alert.alert('Error', 'Failed to submit plan');
		}
	};

	return (
		<View style={{ padding: 20 }}>
			<Text style={{ fontSize: 24, fontWeight: 'bold' }}>
				Weekly Plans Testing
			</Text>

			{!token ? (
				<Button
					title="Login"
					onPress={login}
				/>
			) : (
				<View>
					<Button
						title="Refresh Plans"
						onPress={fetchPlans}
					/>
					<FlatList
						data={plans}
						keyExtractor={(item) =>
							item.id.toString()
						}
						renderItem={({ item }) => (
							<View
								style={{
									padding: 10,
									borderWidth: 1,
									marginVertical: 5,
								}}
							>
								<Text>
									<strong>
										ID:
									</strong>{' '}
									{
										item.id
									}
								</Text>
								<Text>
									<strong>
										Title:
									</strong>{' '}
									{
										item.title
									}
								</Text>
								<Text>
									<strong>
										Active:
									</strong>{' '}
									{item.isActive
										? 'Yes'
										: 'No'}
								</Text>
								<Button
									title="Submit"
									onPress={() =>
										submitPlan(
											item.id
										)
									}
								/>
							</View>
						)}
					/>
				</View>
			)}
		</View>
	);
};

export default WeeklyPlanTesting;
```

---

## Current Database Status

- **Total Plans**: 13
- **Active Plans**: 2
- **Reviewed Plans**: 4
- **Tasks**: 37 total tasks across all plans

---

## Error Handling

### Common Errors

**401 Unauthorized**:

```json
{
	"success": false,
	"message": "Unauthorized access"
}
```

Solution: Add valid Bearer token to headers.

**403 Forbidden**:

```json
{
	"success": false,
	"message": "Forbidden - Manager access required"
}
```

Solution: Use SalesManager or Admin role token for review endpoint.

**404 Not Found**:

```json
{
	"success": false,
	"message": "Weekly plan not found"
}
```

Solution: Verify plan ID exists and belongs to current user.

**400 Bad Request**:

```json
{
	"success": false,
	"message": "Week end date must be after start date"
}
```

Solution: Ensure `weekEndDate` is after `weekStartDate`.

---

## Rate Limiting

- **Global**: 100 requests per minute
- **API Endpoints**: 200 requests per minute
- **Auth Endpoints**: 10 requests per minute

If you exceed rate limits, you'll receive a `429 Too Many Requests` response.

---

**End of Weekly Plan Testing Guide**
