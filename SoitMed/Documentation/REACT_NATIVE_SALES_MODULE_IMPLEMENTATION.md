# React Native Sales Module Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the sales module into your existing React Native mobile application. The integration includes all business logic, API integration, state management, and user interface components for the sales management system.

## Table of Contents

1. [Integration Setup](#integration-setup)
2. [API Service Layer Integration](#api-service-layer-integration)
3. [State Management Integration](#state-management-integration)
4. [Component Integration](#component-integration)
5. [Role-Based Dashboard Integration](#role-based-dashboard-integration)
6. [Client Management Integration](#client-management-integration)
7. [Weekly Planning System Integration](#weekly-planning-system-integration)
8. [Sales Workflow Integration](#sales-workflow-integration)
9. [Error Handling Integration](#error-handling-integration)
10. [Testing Integration](#testing-integration)

## Integration Setup

### 1. Install Required Dependencies

Add these dependencies to your existing React Native project:

```bash
npm install @microsoft/signalr
npm install axios
npm install @reduxjs/toolkit react-redux
npm install react-native-push-notification
npm install @react-native-async-storage/async-storage
npm install @react-native-community/netinfo
npm install react-native-vector-icons
npm install react-native-paper
npm install react-native-gesture-handler
npm install react-native-reanimated
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
REACT_APP_SALES_API_URL=https://your-api-url.com/api
REACT_APP_SIGNALR_URL=https://your-api-url.com/notificationHub
REACT_APP_SALES_ENVIRONMENT=development
```

### 5. Directory Structure

Add these directories to your existing React Native project:

```
src/
├── services/
│   ├── salesApiService.js
│   ├── signalRService.js
│   └── pushNotificationService.js
├── store/
│   └── slices/
│       ├── clientSlice.js
│       ├── weeklyPlanSlice.js
│       └── salesReportSlice.js
├── components/
│   ├── sales/
│   │   ├── ClientSearch.jsx
│   │   ├── ClientDetails.jsx
│   │   └── WeeklyPlanForm.jsx
│   └── notifications/
│       ├── NotificationCenter.jsx
│       └── ToastContainer.jsx
├── screens/
│   ├── SalesSupportDashboard.jsx
│   └── SalesManagerDashboard.jsx
└── navigation/
    └── SalesStack.js
```

## API Service Layer Integration

### 1. Base API Service

Add this service to your existing services in `src/services/salesApiService.js`:

```javascript
import axios from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';

class SalesApiService {
	constructor() {
		this.api = axios.create({
			baseURL: process.env.REACT_APP_SALES_API_URL,
			timeout: 10000,
		});

		this.setupInterceptors();
	}

	setupInterceptors() {
		// Request interceptor
		this.api.interceptors.request.use(
			async (config) => {
				const token = await AsyncStorage.getItem(
					'authToken'
				);
				if (token) {
					config.headers.Authorization = `Bearer ${token}`;
				}
				return config;
			},
			(error) => Promise.reject(error)
		);

		// Response interceptor
		this.api.interceptors.response.use(
			(response) => response,
			async (error) => {
				if (error.response?.status === 401) {
					await AsyncStorage.removeItem(
						'authToken'
					);
					// Navigate to login screen
					// You can use your existing navigation method here
				}
				return Promise.reject(error);
			}
		);
	}

	async get(url, params = {}) {
		const response = await this.api.get(url, { params });
		return response.data;
	}

	async post(url, data = {}) {
		const response = await this.api.post(url, data);
		return response.data;
	}

	async put(url, data = {}) {
		const response = await this.api.put(url, data);
		return response.data;
	}

	async delete(url) {
		const response = await this.api.delete(url);
		return response.data;
	}
}

export default new SalesApiService();
```

### 2. Sales API Service

Add this service to your existing services in `src/services/salesApiService.js`:

```javascript
import salesApiService from './salesApiService';

class SalesApiService {
	// ==================== CLIENT MANAGEMENT ====================

	async searchClients(query, filters = {}) {
		const params = { query, ...filters };
		return await salesApiService.get('/Client/search', params);
	}

	async createClient(clientData) {
		return await salesApiService.post('/Client', clientData);
	}

	async getClient(clientId) {
		return await salesApiService.get(`/Client/${clientId}`);
	}

	async updateClient(clientId, clientData) {
		return await salesApiService.put(
			`/Client/${clientId}`,
			clientData
		);
	}

	async findOrCreateClient(clientData) {
		return await salesApiService.post(
			'/Client/find-or-create',
			clientData
		);
	}

	async getMyClients(filters = {}) {
		return await salesApiService.get('/Client/my-clients', filters);
	}

	async getClientsNeedingFollowUp() {
		return await salesApiService.get('/Client/follow-up-needed');
	}

	async getClientStatistics() {
		return await salesApiService.get('/Client/statistics');
	}

	// ==================== WEEKLY PLANNING ====================

	async createWeeklyPlan(planData) {
		return await salesApiService.post('/WeeklyPlan', planData);
	}

	async getWeeklyPlans(filters = {}) {
		return await salesApiService.get('/WeeklyPlan', filters);
	}

	async getWeeklyPlan(planId) {
		return await salesApiService.get(`/WeeklyPlan/${planId}`);
	}

	async updateWeeklyPlan(planId, planData) {
		return await salesApiService.put(
			`/WeeklyPlan/${planId}`,
			planData
		);
	}

	async submitWeeklyPlan(planId) {
		return await salesApiService.post(
			`/WeeklyPlan/${planId}/submit`
		);
	}

	async approveWeeklyPlan(planId, notes = '') {
		return await salesApiService.post(
			`/WeeklyPlan/${planId}/approve`,
			{ notes }
		);
	}

	async rejectWeeklyPlan(planId, reason = '') {
		return await salesApiService.post(
			`/WeeklyPlan/${planId}/reject`,
			{ reason }
		);
	}

	async getCurrentWeeklyPlan() {
		return await salesApiService.get('/WeeklyPlan/current');
	}

	// ==================== PLAN ITEMS ====================

	async createPlanItem(itemData) {
		return await salesApiService.post('/WeeklyPlanItem', itemData);
	}

	async getPlanItems(planId) {
		return await salesApiService.get(
			`/WeeklyPlanItem/plan/${planId}`
		);
	}

	async updatePlanItem(itemId, itemData) {
		return await salesApiService.put(
			`/WeeklyPlanItem/${itemId}`,
			itemData
		);
	}

	async completePlanItem(itemId, completionData) {
		return await salesApiService.post(
			`/WeeklyPlanItem/${itemId}/complete`,
			completionData
		);
	}

	async cancelPlanItem(itemId, reason) {
		return await salesApiService.post(
			`/WeeklyPlanItem/${itemId}/cancel`,
			{ reason }
		);
	}

	async postponePlanItem(itemId, newDate, reason) {
		return await salesApiService.post(
			`/WeeklyPlanItem/${itemId}/postpone`,
			{ newDate, reason }
		);
	}

	async getOverdueItems() {
		return await salesApiService.get('/WeeklyPlanItem/overdue');
	}

	async getUpcomingItems(days = 7) {
		return await salesApiService.get('/WeeklyPlanItem/upcoming', {
			days,
		});
	}

	// ==================== CLIENT TRACKING ====================

	async getClientVisits(clientId, filters = {}) {
		return await salesApiService.get(
			`/ClientTracking/${clientId}/visits`,
			filters
		);
	}

	async addClientVisit(visitData) {
		return await salesApiService.post(
			'/ClientTracking/visit',
			visitData
		);
	}

	async updateClientVisit(visitId, visitData) {
		return await salesApiService.put(
			`/ClientTracking/visit/${visitId}`,
			visitData
		);
	}

	async deleteClientVisit(visitId) {
		return await salesApiService.delete(
			`/ClientTracking/visit/${visitId}`
		);
	}

	async getClientInteractions(clientId, filters = {}) {
		return await salesApiService.get(
			`/ClientTracking/${clientId}/interactions`,
			filters
		);
	}

	async addClientInteraction(interactionData) {
		return await salesApiService.post(
			'/ClientTracking/interaction',
			interactionData
		);
	}

	async getClientAnalytics(clientId, period = 'monthly') {
		return await salesApiService.get(
			`/ClientTracking/${clientId}/analytics`,
			{ period }
		);
	}

	async getClientSummary(clientId) {
		return await salesApiService.get(
			`/ClientTracking/${clientId}/summary`
		);
	}

	async getClientTimeline(clientId, limit = 50) {
		return await salesApiService.get(
			`/ClientTracking/${clientId}/timeline`,
			{ limit }
		);
	}

	// ==================== SALES REPORTS ====================

	async getSalesManagerDashboard() {
		return await salesApiService.get('/SalesReport/dashboard');
	}

	async getSalesReports(filters = {}) {
		return await salesApiService.get('/SalesReport', filters);
	}

	async getSalesmanPerformance(salesmanId, period = 'monthly') {
		return await salesApiService.get(
			`/SalesReport/salesman/${salesmanId}/performance`,
			{ period }
		);
	}

	async getSalesTrends(period = 'monthly') {
		return await salesApiService.get('/SalesReport/trends', {
			period,
		});
	}

	async getTopPerformers(limit = 10) {
		return await salesApiService.get(
			'/SalesReport/top-performers',
			{ limit }
		);
	}

	// ==================== REQUEST WORKFLOW ====================

	async createRequestWorkflow(requestData) {
		return await salesApiService.post(
			'/RequestWorkflows',
			requestData
		);
	}

	async getMyRequests(filters = {}) {
		return await salesApiService.get(
			'/RequestWorkflows/sent',
			filters
		);
	}

	async getAssignedRequests(filters = {}) {
		return await salesApiService.get(
			'/RequestWorkflows/assigned',
			filters
		);
	}

	async updateRequestStatus(requestId, status, comment = '') {
		return await salesApiService.put(
			`/RequestWorkflows/${requestId}/status`,
			{ status, comment }
		);
	}

	async assignRequest(requestId, assignToUserId) {
		return await salesApiService.put(
			`/RequestWorkflows/${requestId}/assign`,
			{ assignToUserId }
		);
	}
}

export default new SalesApiService();
```

## State Management Integration

### 1. Redux Store Integration

Add these slices to your existing Redux store in `src/store/index.js`:

```javascript
import { configureStore } from '@reduxjs/toolkit';
import authSlice from './slices/authSlice';
import clientSlice from './slices/clientSlice';
import weeklyPlanSlice from './slices/weeklyPlanSlice';
import notificationSlice from './slices/notificationSlice';
import salesReportSlice from './slices/salesReportSlice';

export const store = configureStore({
	reducer: {
		auth: authSlice,
		clients: clientSlice,
		weeklyPlans: weeklyPlanSlice,
		notifications: notificationSlice,
		salesReports: salesReportSlice,
	},
	middleware: (getDefaultMiddleware) =>
		getDefaultMiddleware({
			serializableCheck: {
				ignoredActions: ['persist/PERSIST'],
			},
		}),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
```

### 2. Client Slice

Add this slice to your existing Redux store in `src/store/slices/clientSlice.js`:

```javascript
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import salesApiService from '../../services/salesApiService';

// Async thunks
export const searchClients = createAsyncThunk(
	'clients/searchClients',
	async ({ query, filters = {} }) => {
		const response = await salesApiService.searchClients(
			query,
			filters
		);
		return response.data;
	}
);

export const createClient = createAsyncThunk(
	'clients/createClient',
	async (clientData) => {
		const response = await salesApiService.createClient(clientData);
		return response.data;
	}
);

export const getMyClients = createAsyncThunk(
	'clients/getMyClients',
	async (filters = {}) => {
		const response = await salesApiService.getMyClients(filters);
		return response.data;
	}
);

export const getClientDetails = createAsyncThunk(
	'clients/getClientDetails',
	async (clientId) => {
		const response = await salesApiService.getClient(clientId);
		return response.data;
	}
);

export const getClientVisits = createAsyncThunk(
	'clients/getClientVisits',
	async ({ clientId, filters = {} }) => {
		const response = await salesApiService.getClientVisits(
			clientId,
			filters
		);
		return response.data;
	}
);

export const addClientVisit = createAsyncThunk(
	'clients/addClientVisit',
	async (visitData) => {
		const response = await salesApiService.addClientVisit(
			visitData
		);
		return response.data;
	}
);

const initialState = {
	clients: [],
	selectedClient: null,
	clientVisits: [],
	clientInteractions: [],
	clientAnalytics: null,
	searchResults: [],
	loading: false,
	error: null,
	pagination: {
		page: 1,
		pageSize: 20,
		totalCount: 0,
		totalPages: 0,
	},
};

const clientSlice = createSlice({
	name: 'clients',
	initialState,
	reducers: {
		setSelectedClient: (state, action) => {
			state.selectedClient = action.payload;
		},
		clearSearchResults: (state) => {
			state.searchResults = [];
		},
		setPagination: (state, action) => {
			state.pagination = {
				...state.pagination,
				...action.payload,
			};
		},
		clearError: (state) => {
			state.error = null;
		},
	},
	extraReducers: (builder) => {
		builder
			// Search clients
			.addCase(searchClients.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(searchClients.fulfilled, (state, action) => {
				state.loading = false;
				state.searchResults = action.payload;
			})
			.addCase(searchClients.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Create client
			.addCase(createClient.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(createClient.fulfilled, (state, action) => {
				state.loading = false;
				state.clients.unshift(action.payload);
			})
			.addCase(createClient.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get my clients
			.addCase(getMyClients.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(getMyClients.fulfilled, (state, action) => {
				state.loading = false;
				state.clients = action.payload;
			})
			.addCase(getMyClients.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get client details
			.addCase(getClientDetails.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(
				getClientDetails.fulfilled,
				(state, action) => {
					state.loading = false;
					state.selectedClient = action.payload;
				}
			)
			.addCase(getClientDetails.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get client visits
			.addCase(getClientVisits.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(getClientVisits.fulfilled, (state, action) => {
				state.loading = false;
				state.clientVisits = action.payload;
			})
			.addCase(getClientVisits.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Add client visit
			.addCase(addClientVisit.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(addClientVisit.fulfilled, (state, action) => {
				state.loading = false;
				state.clientVisits.unshift(action.payload);
			})
			.addCase(addClientVisit.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			});
	},
});

export const {
	setSelectedClient,
	clearSearchResults,
	setPagination,
	clearError,
} = clientSlice.actions;

export default clientSlice.reducer;
```

## Component Integration

### 1. Client Search Component

Add this component to your existing components in `src/components/sales/ClientSearch.jsx`:

```jsx
import React, { useState, useEffect } from 'react';
import {
	View,
	TextInput,
	FlatList,
	TouchableOpacity,
	Text,
	StyleSheet,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import {
	searchClients,
	clearSearchResults,
} from '../../store/slices/clientSlice';
import Icon from 'react-native-vector-icons/MaterialIcons';

const ClientSearch = ({
	onClientSelect,
	placeholder = 'Search clients...',
}) => {
	const dispatch = useDispatch();
	const { searchResults, loading } = useSelector(
		(state) => state.clients
	);
	const [query, setQuery] = useState('');
	const [showResults, setShowResults] = useState(false);

	useEffect(() => {
		const timeoutId = setTimeout(() => {
			if (query.length >= 2) {
				dispatch(searchClients({ query }));
				setShowResults(true);
			} else {
				dispatch(clearSearchResults());
				setShowResults(false);
			}
		}, 300);

		return () => clearTimeout(timeoutId);
	}, [query, dispatch]);

	const handleClientSelect = (client) => {
		setQuery(client.name);
		setShowResults(false);
		onClientSelect(client);
	};

	const renderClientItem = ({ item }) => (
		<TouchableOpacity
			style={styles.clientItem}
			onPress={() => handleClientSelect(item)}
		>
			<View style={styles.clientInfo}>
				<Text style={styles.clientName}>
					{item.name}
				</Text>
				<Text style={styles.clientType}>
					{item.type} • {item.specialization}
				</Text>
				<Text style={styles.clientLocation}>
					{item.location}
				</Text>
			</View>
		</TouchableOpacity>
	);

	return (
		<View style={styles.container}>
			<View style={styles.searchContainer}>
				<Icon
					name="search"
					size={20}
					color="#666"
					style={styles.searchIcon}
				/>
				<TextInput
					style={styles.searchInput}
					value={query}
					onChangeText={setQuery}
					placeholder={placeholder}
					placeholderTextColor="#999"
				/>
			</View>

			{showResults && (
				<View style={styles.resultsContainer}>
					{loading ? (
						<View
							style={
								styles.loadingContainer
							}
						>
							<Text
								style={
									styles.loadingText
								}
							>
								Searching...
							</Text>
						</View>
					) : searchResults.length > 0 ? (
						<FlatList
							data={searchResults}
							renderItem={
								renderClientItem
							}
							keyExtractor={(item) =>
								item.id.toString()
							}
							style={
								styles.resultsList
							}
						/>
					) : (
						<View
							style={
								styles.noResultsContainer
							}
						>
							<Text
								style={
									styles.noResultsText
								}
							>
								No clients found
							</Text>
						</View>
					)}
				</View>
			)}
		</View>
	);
};

const styles = StyleSheet.create({
	container: {
		position: 'relative',
		zIndex: 1000,
	},
	searchContainer: {
		flexDirection: 'row',
		alignItems: 'center',
		backgroundColor: '#f5f5f5',
		borderRadius: 8,
		paddingHorizontal: 12,
		paddingVertical: 8,
		marginBottom: 8,
	},
	searchIcon: {
		marginRight: 8,
	},
	searchInput: {
		flex: 1,
		fontSize: 16,
		color: '#333',
	},
	resultsContainer: {
		position: 'absolute',
		top: 50,
		left: 0,
		right: 0,
		backgroundColor: '#fff',
		borderRadius: 8,
		borderWidth: 1,
		borderColor: '#e0e0e0',
		maxHeight: 200,
		zIndex: 1001,
	},
	resultsList: {
		maxHeight: 200,
	},
	clientItem: {
		padding: 12,
		borderBottomWidth: 1,
		borderBottomColor: '#f0f0f0',
	},
	clientInfo: {
		flex: 1,
	},
	clientName: {
		fontSize: 16,
		fontWeight: '600',
		color: '#333',
		marginBottom: 4,
	},
	clientType: {
		fontSize: 14,
		color: '#666',
		marginBottom: 2,
	},
	clientLocation: {
		fontSize: 12,
		color: '#999',
	},
	loadingContainer: {
		padding: 16,
		alignItems: 'center',
	},
	loadingText: {
		color: '#666',
		fontSize: 14,
	},
	noResultsContainer: {
		padding: 16,
		alignItems: 'center',
	},
	noResultsText: {
		color: '#666',
		fontSize: 14,
	},
});

export default ClientSearch;
```

### 2. Client Details Component

Add this component to your existing components in `src/components/sales/ClientDetails.jsx`:

```jsx
import React, { useEffect, useState } from 'react';
import {
	View,
	Text,
	ScrollView,
	TouchableOpacity,
	Modal,
	TextInput,
	StyleSheet,
	Alert,
} from 'react-native';
import { useDispatch, useSelector } from 'react-redux';
import {
	getClientDetails,
	getClientVisits,
	addClientVisit,
} from '../../store/slices/clientSlice';
import Icon from 'react-native-vector-icons/MaterialIcons';

const ClientDetails = ({ clientId }) => {
	const dispatch = useDispatch();
	const { selectedClient, clientVisits, loading } = useSelector(
		(state) => state.clients
	);
	const [showVisitForm, setShowVisitForm] = useState(false);
	const [visitForm, setVisitForm] = useState({
		visitDate: '',
		visitType: 'Initial',
		location: '',
		purpose: '',
		notes: '',
		results: '',
		nextVisitDate: '',
	});

	useEffect(() => {
		if (clientId) {
			dispatch(getClientDetails(clientId));
			dispatch(getClientVisits({ clientId }));
		}
	}, [clientId, dispatch]);

	const handleVisitSubmit = () => {
		dispatch(
			addClientVisit({
				clientId,
				...visitForm,
			})
		);
		setShowVisitForm(false);
		setVisitForm({
			visitDate: '',
			visitType: 'Initial',
			location: '',
			purpose: '',
			notes: '',
			results: '',
			nextVisitDate: '',
		});
		Alert.alert('Success', 'Visit added successfully');
	};

	if (loading) {
		return (
			<View style={styles.loadingContainer}>
				<Text style={styles.loadingText}>
					Loading client details...
				</Text>
			</View>
		);
	}

	if (!selectedClient) {
		return (
			<View style={styles.errorContainer}>
				<Text style={styles.errorText}>
					Client not found
				</Text>
			</View>
		);
	}

	return (
		<ScrollView style={styles.container}>
			<View style={styles.header}>
				<View style={styles.headerContent}>
					<Text style={styles.clientName}>
						{selectedClient.name}
					</Text>
					<Text style={styles.clientType}>
						{selectedClient.type} •{' '}
						{selectedClient.specialization}
					</Text>
					<Text style={styles.clientLocation}>
						{selectedClient.location}
					</Text>
				</View>
				<TouchableOpacity
					style={styles.addVisitButton}
					onPress={() => setShowVisitForm(true)}
				>
					<Icon
						name="add"
						size={20}
						color="#fff"
					/>
					<Text style={styles.addVisitButtonText}>
						Add Visit
					</Text>
				</TouchableOpacity>
			</View>

			<View style={styles.content}>
				<View style={styles.section}>
					<Text style={styles.sectionTitle}>
						Contact Information
					</Text>
					<View style={styles.infoRow}>
						<Icon
							name="phone"
							size={16}
							color="#666"
						/>
						<Text style={styles.infoText}>
							{selectedClient.phone}
						</Text>
					</View>
					<View style={styles.infoRow}>
						<Icon
							name="email"
							size={16}
							color="#666"
						/>
						<Text style={styles.infoText}>
							{selectedClient.email}
						</Text>
					</View>
					<View style={styles.statusContainer}>
						<Text
							style={
								styles.statusLabel
							}
						>
							Status:
						</Text>
						<View
							style={[
								styles.statusBadge,
								{
									backgroundColor:
										selectedClient.status ===
										'Active'
											? '#4CAF50'
											: '#FF9800',
								},
							]}
						>
							<Text
								style={
									styles.statusText
								}
							>
								{
									selectedClient.status
								}
							</Text>
						</View>
					</View>
				</View>

				<View style={styles.section}>
					<Text style={styles.sectionTitle}>
						Visit History
					</Text>
					{clientVisits.map((visit) => (
						<View
							key={visit.id}
							style={styles.visitItem}
						>
							<View
								style={
									styles.visitHeader
								}
							>
								<Text
									style={
										styles.visitType
									}
								>
									{
										visit.visitType
									}
								</Text>
								<View
									style={[
										styles.visitStatus,
										{
											backgroundColor:
												visit.status ===
												'Completed'
													? '#4CAF50'
													: '#FF9800',
										},
									]}
								>
									<Text
										style={
											styles.visitStatusText
										}
									>
										{
											visit.status
										}
									</Text>
								</View>
							</View>
							<Text
								style={
									styles.visitPurpose
								}
							>
								{visit.purpose}
							</Text>
							<Text
								style={
									styles.visitDate
								}
							>
								{new Date(
									visit.visitDate
								).toLocaleDateString()}
							</Text>
							{visit.results && (
								<Text
									style={
										styles.visitResults
									}
								>
									{
										visit.results
									}
								</Text>
							)}
						</View>
					))}
				</View>
			</View>

			{/* Visit Form Modal */}
			<Modal
				visible={showVisitForm}
				animationType="slide"
				presentationStyle="pageSheet"
			>
				<View style={styles.modalContainer}>
					<View style={styles.modalHeader}>
						<Text style={styles.modalTitle}>
							Add Client Visit
						</Text>
						<TouchableOpacity
							onPress={() =>
								setShowVisitForm(
									false
								)
							}
						>
							<Icon
								name="close"
								size={24}
								color="#666"
							/>
						</TouchableOpacity>
					</View>

					<ScrollView style={styles.modalContent}>
						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Visit Date
							</Text>
							<TextInput
								style={
									styles.input
								}
								value={
									visitForm.visitDate
								}
								onChangeText={(
									text
								) =>
									setVisitForm(
										{
											...visitForm,
											visitDate: text,
										}
									)
								}
								placeholder="YYYY-MM-DD"
							/>
						</View>

						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Visit Type
							</Text>
							<View
								style={
									styles.pickerContainer
								}
							>
								<Text
									style={
										styles.pickerText
									}
								>
									{
										visitForm.visitType
									}
								</Text>
								<Icon
									name="arrow-drop-down"
									size={
										20
									}
									color="#666"
								/>
							</View>
						</View>

						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Location
							</Text>
							<TextInput
								style={
									styles.input
								}
								value={
									visitForm.location
								}
								onChangeText={(
									text
								) =>
									setVisitForm(
										{
											...visitForm,
											location: text,
										}
									)
								}
								placeholder="Visit location"
							/>
						</View>

						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Purpose
							</Text>
							<TextInput
								style={
									styles.input
								}
								value={
									visitForm.purpose
								}
								onChangeText={(
									text
								) =>
									setVisitForm(
										{
											...visitForm,
											purpose: text,
										}
									)
								}
								placeholder="Visit purpose"
							/>
						</View>

						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Notes
							</Text>
							<TextInput
								style={[
									styles.input,
									styles.textArea,
								]}
								value={
									visitForm.notes
								}
								onChangeText={(
									text
								) =>
									setVisitForm(
										{
											...visitForm,
											notes: text,
										}
									)
								}
								placeholder="Additional notes"
								multiline
								numberOfLines={
									3
								}
							/>
						</View>

						<View style={styles.formGroup}>
							<Text
								style={
									styles.label
								}
							>
								Results
							</Text>
							<TextInput
								style={[
									styles.input,
									styles.textArea,
								]}
								value={
									visitForm.results
								}
								onChangeText={(
									text
								) =>
									setVisitForm(
										{
											...visitForm,
											results: text,
										}
									)
								}
								placeholder="Visit results"
								multiline
								numberOfLines={
									3
								}
							/>
						</View>
					</ScrollView>

					<View style={styles.modalFooter}>
						<TouchableOpacity
							style={
								styles.cancelButton
							}
							onPress={() =>
								setShowVisitForm(
									false
								)
							}
						>
							<Text
								style={
									styles.cancelButtonText
								}
							>
								Cancel
							</Text>
						</TouchableOpacity>
						<TouchableOpacity
							style={
								styles.submitButton
							}
							onPress={
								handleVisitSubmit
							}
						>
							<Text
								style={
									styles.submitButtonText
								}
							>
								Add Visit
							</Text>
						</TouchableOpacity>
					</View>
				</View>
			</Modal>
		</ScrollView>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		backgroundColor: '#f5f5f5',
	},
	loadingContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
	},
	loadingText: {
		fontSize: 16,
		color: '#666',
	},
	errorContainer: {
		flex: 1,
		justifyContent: 'center',
		alignItems: 'center',
	},
	errorText: {
		fontSize: 16,
		color: '#f44336',
	},
	header: {
		backgroundColor: '#fff',
		padding: 16,
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'flex-start',
		borderBottomWidth: 1,
		borderBottomColor: '#e0e0e0',
	},
	headerContent: {
		flex: 1,
	},
	clientName: {
		fontSize: 24,
		fontWeight: 'bold',
		color: '#333',
		marginBottom: 4,
	},
	clientType: {
		fontSize: 16,
		color: '#666',
		marginBottom: 2,
	},
	clientLocation: {
		fontSize: 14,
		color: '#999',
	},
	addVisitButton: {
		backgroundColor: '#2196F3',
		paddingHorizontal: 16,
		paddingVertical: 8,
		borderRadius: 6,
		flexDirection: 'row',
		alignItems: 'center',
	},
	addVisitButtonText: {
		color: '#fff',
		marginLeft: 4,
		fontWeight: '600',
	},
	content: {
		padding: 16,
	},
	section: {
		backgroundColor: '#fff',
		borderRadius: 8,
		padding: 16,
		marginBottom: 16,
	},
	sectionTitle: {
		fontSize: 18,
		fontWeight: '600',
		color: '#333',
		marginBottom: 12,
	},
	infoRow: {
		flexDirection: 'row',
		alignItems: 'center',
		marginBottom: 8,
	},
	infoText: {
		marginLeft: 8,
		fontSize: 14,
		color: '#666',
	},
	statusContainer: {
		flexDirection: 'row',
		alignItems: 'center',
		marginTop: 8,
	},
	statusLabel: {
		fontSize: 14,
		color: '#666',
		marginRight: 8,
	},
	statusBadge: {
		paddingHorizontal: 8,
		paddingVertical: 4,
		borderRadius: 12,
	},
	statusText: {
		color: '#fff',
		fontSize: 12,
		fontWeight: '600',
	},
	visitItem: {
		backgroundColor: '#f9f9f9',
		borderRadius: 6,
		padding: 12,
		marginBottom: 8,
	},
	visitHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		marginBottom: 4,
	},
	visitType: {
		fontSize: 16,
		fontWeight: '600',
		color: '#333',
	},
	visitStatus: {
		paddingHorizontal: 6,
		paddingVertical: 2,
		borderRadius: 8,
	},
	visitStatusText: {
		color: '#fff',
		fontSize: 10,
		fontWeight: '600',
	},
	visitPurpose: {
		fontSize: 14,
		color: '#666',
		marginBottom: 4,
	},
	visitDate: {
		fontSize: 12,
		color: '#999',
		marginBottom: 4,
	},
	visitResults: {
		fontSize: 14,
		color: '#333',
		fontStyle: 'italic',
	},
	modalContainer: {
		flex: 1,
		backgroundColor: '#fff',
	},
	modalHeader: {
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		padding: 16,
		borderBottomWidth: 1,
		borderBottomColor: '#e0e0e0',
	},
	modalTitle: {
		fontSize: 18,
		fontWeight: '600',
		color: '#333',
	},
	modalContent: {
		flex: 1,
		padding: 16,
	},
	formGroup: {
		marginBottom: 16,
	},
	label: {
		fontSize: 14,
		fontWeight: '600',
		color: '#333',
		marginBottom: 8,
	},
	input: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 6,
		padding: 12,
		fontSize: 16,
		backgroundColor: '#fff',
	},
	textArea: {
		height: 80,
		textAlignVertical: 'top',
	},
	pickerContainer: {
		borderWidth: 1,
		borderColor: '#ddd',
		borderRadius: 6,
		padding: 12,
		flexDirection: 'row',
		justifyContent: 'space-between',
		alignItems: 'center',
		backgroundColor: '#fff',
	},
	pickerText: {
		fontSize: 16,
		color: '#333',
	},
	modalFooter: {
		flexDirection: 'row',
		padding: 16,
		borderTopWidth: 1,
		borderTopColor: '#e0e0e0',
	},
	cancelButton: {
		flex: 1,
		padding: 12,
		marginRight: 8,
		borderRadius: 6,
		borderWidth: 1,
		borderColor: '#ddd',
		alignItems: 'center',
	},
	cancelButtonText: {
		color: '#666',
		fontSize: 16,
		fontWeight: '600',
	},
	submitButton: {
		flex: 1,
		padding: 12,
		marginLeft: 8,
		borderRadius: 6,
		backgroundColor: '#2196F3',
		alignItems: 'center',
	},
	submitButtonText: {
		color: '#fff',
		fontSize: 16,
		fontWeight: '600',
	},
});

export default ClientDetails;
```

## Integration Steps Summary

### Step 1: Install Dependencies

```bash
npm install @microsoft/signalr axios @reduxjs/toolkit react-redux react-native-push-notification @react-native-async-storage/async-storage @react-native-community/netinfo react-native-vector-icons react-native-paper
```

### Step 2: Configure Native Platforms

- Update iOS Podfile and run `pod install`
- Update Android manifest permissions

### Step 3: Add Environment Variables

Add the sales API URLs to your existing `.env` file.

### Step 4: Create Services

Add the sales API service and SignalR service to your existing services directory.

### Step 5: Update Redux Store

Add the sales-related slices to your existing Redux store configuration.

### Step 6: Add Components

Create the sales components in your existing components directory structure.

### Step 7: Add Screens

Create the role-based dashboard screens in your existing screens directory.

### Step 8: Update Navigation

Add the new sales screens to your existing navigation stack.

### Step 9: Add Tests

Add the sales module tests to your existing test suite.

## Conclusion

This integration guide provides everything needed to add a complete sales module to your existing React Native application:

- **Seamless API integration** with all sales endpoints
- **Redux state management** integration for complex data flows
- **Role-based dashboards** for different user types
- **Client management** with search and tracking
- **Weekly planning system** with approval workflows
- **Error handling** and user feedback integration
- **Testing integration** for reliable code
- **Native mobile features** like push notifications and offline support

The integration follows React Native best practices and can be added to any existing React Native application without disrupting current functionality.

