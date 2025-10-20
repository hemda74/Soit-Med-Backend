# React Sales Module Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the sales module into your existing React web application. The integration includes all business logic, API integration, state management, and user interface components for the sales management system.

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

Add these dependencies to your existing React project:

```bash
npm install @microsoft/signalr
npm install axios
npm install react-hot-toast
npm install react-hook-form
npm install @headlessui/react
npm install @heroicons/react
npm install tailwindcss
npm install @types/node
```

### 2. Environment Configuration

Add these environment variables to your existing `.env` file:

```env
# Add these to your existing .env file
REACT_APP_SALES_API_URL=https://your-api-url.com/api
REACT_APP_SIGNALR_URL=https://your-api-url.com/notificationHub
REACT_APP_SALES_ENVIRONMENT=development
```

### 3. Directory Structure

Add these directories to your existing React project:

```
src/
├── services/
│   ├── salesApiService.js
│   └── signalRService.js
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
│       ├── NotificationBell.jsx
│       └── ToastContainer.jsx
└── pages/
    ├── SalesSupportDashboard.jsx
    └── SalesManagerDashboard.jsx
```

## API Service Layer Integration

### 1. Base API Service

Create `src/services/salesApiService.js` (or integrate with your existing API service):

```javascript
import axios from 'axios';

class ApiService {
	constructor() {
		this.api = axios.create({
			baseURL: process.env.REACT_APP_API_URL,
			timeout: 10000,
		});

		this.setupInterceptors();
	}

	setupInterceptors() {
		// Request interceptor
		this.api.interceptors.request.use(
			(config) => {
				const token = localStorage.getItem('authToken');
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
			(error) => {
				if (error.response?.status === 401) {
					localStorage.removeItem('authToken');
					window.location.href = '/login';
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

export default new ApiService();
```

### 2. Sales API Service

Create `src/services/salesApiService.js` (integrate with your existing API structure):

```javascript
import apiService from './apiService';

class SalesApiService {
	// ==================== CLIENT MANAGEMENT ====================

	async searchClients(query, filters = {}) {
		const params = { query, ...filters };
		return await apiService.get('/Client/search', params);
	}

	async createClient(clientData) {
		return await apiService.post('/Client', clientData);
	}

	async getClient(clientId) {
		return await apiService.get(`/Client/${clientId}`);
	}

	async updateClient(clientId, clientData) {
		return await apiService.put(`/Client/${clientId}`, clientData);
	}

	async findOrCreateClient(clientData) {
		return await apiService.post(
			'/Client/find-or-create',
			clientData
		);
	}

	async getMyClients(filters = {}) {
		return await apiService.get('/Client/my-clients', filters);
	}

	async getClientsNeedingFollowUp() {
		return await apiService.get('/Client/follow-up-needed');
	}

	async getClientStatistics() {
		return await apiService.get('/Client/statistics');
	}

	// ==================== WEEKLY PLANNING ====================

	async createWeeklyPlan(planData) {
		return await apiService.post('/WeeklyPlan', planData);
	}

	async getWeeklyPlans(filters = {}) {
		return await apiService.get('/WeeklyPlan', filters);
	}

	async getWeeklyPlan(planId) {
		return await apiService.get(`/WeeklyPlan/${planId}`);
	}

	async updateWeeklyPlan(planId, planData) {
		return await apiService.put(`/WeeklyPlan/${planId}`, planData);
	}

	async submitWeeklyPlan(planId) {
		return await apiService.post(`/WeeklyPlan/${planId}/submit`);
	}

	async approveWeeklyPlan(planId, notes = '') {
		return await apiService.post(`/WeeklyPlan/${planId}/approve`, {
			notes,
		});
	}

	async rejectWeeklyPlan(planId, reason = '') {
		return await apiService.post(`/WeeklyPlan/${planId}/reject`, {
			reason,
		});
	}

	async getCurrentWeeklyPlan() {
		return await apiService.get('/WeeklyPlan/current');
	}

	// ==================== PLAN ITEMS ====================

	async createPlanItem(itemData) {
		return await apiService.post('/WeeklyPlanItem', itemData);
	}

	async getPlanItems(planId) {
		return await apiService.get(`/WeeklyPlanItem/plan/${planId}`);
	}

	async updatePlanItem(itemId, itemData) {
		return await apiService.put(
			`/WeeklyPlanItem/${itemId}`,
			itemData
		);
	}

	async completePlanItem(itemId, completionData) {
		return await apiService.post(
			`/WeeklyPlanItem/${itemId}/complete`,
			completionData
		);
	}

	async cancelPlanItem(itemId, reason) {
		return await apiService.post(
			`/WeeklyPlanItem/${itemId}/cancel`,
			{ reason }
		);
	}

	async postponePlanItem(itemId, newDate, reason) {
		return await apiService.post(
			`/WeeklyPlanItem/${itemId}/postpone`,
			{ newDate, reason }
		);
	}

	async getOverdueItems() {
		return await apiService.get('/WeeklyPlanItem/overdue');
	}

	async getUpcomingItems(days = 7) {
		return await apiService.get('/WeeklyPlanItem/upcoming', {
			days,
		});
	}

	// ==================== CLIENT TRACKING ====================

	async getClientVisits(clientId, filters = {}) {
		return await apiService.get(
			`/ClientTracking/${clientId}/visits`,
			filters
		);
	}

	async addClientVisit(visitData) {
		return await apiService.post(
			'/ClientTracking/visit',
			visitData
		);
	}

	async updateClientVisit(visitId, visitData) {
		return await apiService.put(
			`/ClientTracking/visit/${visitId}`,
			visitData
		);
	}

	async deleteClientVisit(visitId) {
		return await apiService.delete(
			`/ClientTracking/visit/${visitId}`
		);
	}

	async getClientInteractions(clientId, filters = {}) {
		return await apiService.get(
			`/ClientTracking/${clientId}/interactions`,
			filters
		);
	}

	async addClientInteraction(interactionData) {
		return await apiService.post(
			'/ClientTracking/interaction',
			interactionData
		);
	}

	async getClientAnalytics(clientId, period = 'monthly') {
		return await apiService.get(
			`/ClientTracking/${clientId}/analytics`,
			{ period }
		);
	}

	async getClientSummary(clientId) {
		return await apiService.get(
			`/ClientTracking/${clientId}/summary`
		);
	}

	async getClientTimeline(clientId, limit = 50) {
		return await apiService.get(
			`/ClientTracking/${clientId}/timeline`,
			{ limit }
		);
	}

	async exportClientHistory(clientId, format = 'pdf', filters = {}) {
		const response = await apiService.api.get(
			`/ClientTracking/${clientId}/export`,
			{
				params: { format, ...filters },
				responseType: 'blob',
			}
		);
		return response.data;
	}

	// ==================== SALES REPORTS ====================

	async getSalesManagerDashboard() {
		return await apiService.get('/SalesReport/dashboard');
	}

	async getSalesReports(filters = {}) {
		return await apiService.get('/SalesReport', filters);
	}

	async getSalesmanPerformance(salesmanId, period = 'monthly') {
		return await apiService.get(
			`/SalesReport/salesman/${salesmanId}/performance`,
			{ period }
		);
	}

	async getSalesTrends(period = 'monthly') {
		return await apiService.get('/SalesReport/trends', { period });
	}

	async getTopPerformers(limit = 10) {
		return await apiService.get('/SalesReport/top-performers', {
			limit,
		});
	}

	// ==================== REQUEST WORKFLOW ====================

	async createRequestWorkflow(requestData) {
		return await apiService.post('/RequestWorkflows', requestData);
	}

	async getMyRequests(filters = {}) {
		return await apiService.get('/RequestWorkflows/sent', filters);
	}

	async getAssignedRequests(filters = {}) {
		return await apiService.get(
			'/RequestWorkflows/assigned',
			filters
		);
	}

	async updateRequestStatus(requestId, status, comment = '') {
		return await apiService.put(
			`/RequestWorkflows/${requestId}/status`,
			{ status, comment }
		);
	}

	async assignRequest(requestId, assignToUserId) {
		return await apiService.put(
			`/RequestWorkflows/${requestId}/assign`,
			{ assignToUserId }
		);
	}

	// ==================== DELIVERY & PAYMENT TERMS ====================

	async createDeliveryTerms(termsData) {
		return await apiService.post('/DeliveryTerms', termsData);
	}

	async createPaymentTerms(termsData) {
		return await apiService.post('/PaymentTerms', termsData);
	}

	async getDeliveryTerms(termsId) {
		return await apiService.get(`/DeliveryTerms/${termsId}`);
	}

	async getPaymentTerms(termsId) {
		return await apiService.get(`/PaymentTerms/${termsId}`);
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

### 3. Weekly Plan Slice

Add this slice to your existing Redux store in `src/store/slices/weeklyPlanSlice.js`:

```javascript
import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import salesApiService from '../../services/salesApiService';

// Async thunks
export const createWeeklyPlan = createAsyncThunk(
	'weeklyPlans/createWeeklyPlan',
	async (planData) => {
		const response = await salesApiService.createWeeklyPlan(
			planData
		);
		return response.data;
	}
);

export const getWeeklyPlans = createAsyncThunk(
	'weeklyPlans/getWeeklyPlans',
	async (filters = {}) => {
		const response = await salesApiService.getWeeklyPlans(filters);
		return response.data;
	}
);

export const getCurrentWeeklyPlan = createAsyncThunk(
	'weeklyPlans/getCurrentWeeklyPlan',
	async () => {
		const response = await salesApiService.getCurrentWeeklyPlan();
		return response.data;
	}
);

export const createPlanItem = createAsyncThunk(
	'weeklyPlans/createPlanItem',
	async (itemData) => {
		const response = await salesApiService.createPlanItem(itemData);
		return response.data;
	}
);

export const getPlanItems = createAsyncThunk(
	'weeklyPlans/getPlanItems',
	async (planId) => {
		const response = await salesApiService.getPlanItems(planId);
		return response.data;
	}
);

export const completePlanItem = createAsyncThunk(
	'weeklyPlans/completePlanItem',
	async ({ itemId, completionData }) => {
		const response = await salesApiService.completePlanItem(
			itemId,
			completionData
		);
		return response.data;
	}
);

const initialState = {
	plans: [],
	currentPlan: null,
	planItems: [],
	overdueItems: [],
	upcomingItems: [],
	loading: false,
	error: null,
	pagination: {
		page: 1,
		pageSize: 20,
		totalCount: 0,
		totalPages: 0,
	},
};

const weeklyPlanSlice = createSlice({
	name: 'weeklyPlans',
	initialState,
	reducers: {
		setCurrentPlan: (state, action) => {
			state.currentPlan = action.payload;
		},
		updatePlanItem: (state, action) => {
			const index = state.planItems.findIndex(
				(item) => item.id === action.payload.id
			);
			if (index !== -1) {
				state.planItems[index] = action.payload;
			}
		},
		clearError: (state) => {
			state.error = null;
		},
	},
	extraReducers: (builder) => {
		builder
			// Create weekly plan
			.addCase(createWeeklyPlan.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(
				createWeeklyPlan.fulfilled,
				(state, action) => {
					state.loading = false;
					state.plans.unshift(action.payload);
					state.currentPlan = action.payload;
				}
			)
			.addCase(createWeeklyPlan.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get weekly plans
			.addCase(getWeeklyPlans.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(getWeeklyPlans.fulfilled, (state, action) => {
				state.loading = false;
				state.plans = action.payload.plans || [];
				state.pagination =
					action.payload.pagination ||
					state.pagination;
			})
			.addCase(getWeeklyPlans.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get current weekly plan
			.addCase(getCurrentWeeklyPlan.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(
				getCurrentWeeklyPlan.fulfilled,
				(state, action) => {
					state.loading = false;
					state.currentPlan = action.payload;
				}
			)
			.addCase(
				getCurrentWeeklyPlan.rejected,
				(state, action) => {
					state.loading = false;
					state.error = action.error.message;
				}
			)
			// Create plan item
			.addCase(createPlanItem.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(createPlanItem.fulfilled, (state, action) => {
				state.loading = false;
				state.planItems.push(action.payload);
			})
			.addCase(createPlanItem.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Get plan items
			.addCase(getPlanItems.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(getPlanItems.fulfilled, (state, action) => {
				state.loading = false;
				state.planItems = action.payload;
			})
			.addCase(getPlanItems.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			})
			// Complete plan item
			.addCase(completePlanItem.pending, (state) => {
				state.loading = true;
				state.error = null;
			})
			.addCase(
				completePlanItem.fulfilled,
				(state, action) => {
					state.loading = false;
					const index = state.planItems.findIndex(
						(item) =>
							item.id ===
							action.payload.id
					);
					if (index !== -1) {
						state.planItems[index] =
							action.payload;
					}
				}
			)
			.addCase(completePlanItem.rejected, (state, action) => {
				state.loading = false;
				state.error = action.error.message;
			});
	},
});

export const { setCurrentPlan, updatePlanItem, clearError } =
	weeklyPlanSlice.actions;

export default weeklyPlanSlice.reducer;
```

## Component Integration

### 1. Dashboard Layout Integration

Update your existing dashboard layout or create `src/components/layout/DashboardLayout.jsx`:

```jsx
import React from 'react';
import { useSelector } from 'react-redux';
import Sidebar from './Sidebar';
import Header from './Header';
import NotificationBell from './NotificationBell';

const DashboardLayout = ({ children }) => {
	const { user } = useSelector((state) => state.auth);

	return (
		<div className="min-h-screen bg-gray-50">
			<Sidebar />
			<div className="lg:pl-64">
				<Header />
				<main className="py-6">
					<div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
						{children}
					</div>
				</main>
			</div>
		</div>
	);
};

export default DashboardLayout;
```

### 2. Client Search Component

Add this component to your existing components in `src/components/sales/ClientSearch.jsx`:

```jsx
import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
	searchClients,
	clearSearchResults,
} from '../../store/slices/clientSlice';
import { MagnifyingGlassIcon } from '@heroicons/react/24/outline';

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

	return (
		<div className="relative">
			<div className="relative">
				<MagnifyingGlassIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
				<input
					type="text"
					value={query}
					onChange={(e) =>
						setQuery(e.target.value)
					}
					placeholder={placeholder}
					className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
				/>
			</div>

			{showResults && (
				<div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
					{loading ? (
						<div className="p-4 text-center text-gray-500">
							Searching...
						</div>
					) : searchResults.length > 0 ? (
						searchResults.map((client) => (
							<div
								key={client.id}
								onClick={() =>
									handleClientSelect(
										client
									)
								}
								className="p-3 hover:bg-gray-50 cursor-pointer border-b border-gray-100 last:border-b-0"
							>
								<div className="font-medium text-gray-900">
									{
										client.name
									}
								</div>
								<div className="text-sm text-gray-500">
									{
										client.type
									}{' '}
									•{' '}
									{
										client.specialization
									}
								</div>
								<div className="text-sm text-gray-400">
									{
										client.location
									}
								</div>
							</div>
						))
					) : (
						<div className="p-4 text-center text-gray-500">
							No clients found
						</div>
					)}
				</div>
			)}
		</div>
	);
};

export default ClientSearch;
```

### 3. Client Details Component

Add this component to your existing components in `src/components/sales/ClientDetails.jsx`:

```jsx
import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
	getClientDetails,
	getClientVisits,
	addClientVisit,
} from '../../store/slices/clientSlice';
import { format } from 'date-fns';

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

	const handleVisitSubmit = (e) => {
		e.preventDefault();
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
	};

	if (loading) {
		return (
			<div className="text-center py-8">
				Loading client details...
			</div>
		);
	}

	if (!selectedClient) {
		return <div className="text-center py-8">Client not found</div>;
	}

	return (
		<div className="bg-white rounded-lg shadow">
			<div className="px-6 py-4 border-b border-gray-200">
				<div className="flex justify-between items-start">
					<div>
						<h2 className="text-2xl font-bold text-gray-900">
							{selectedClient.name}
						</h2>
						<p className="text-gray-600">
							{selectedClient.type} •{' '}
							{
								selectedClient.specialization
							}
						</p>
						<p className="text-sm text-gray-500">
							{
								selectedClient.location
							}
						</p>
					</div>
					<button
						onClick={() =>
							setShowVisitForm(true)
						}
						className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
					>
						Add Visit
					</button>
				</div>
			</div>

			<div className="p-6">
				<div className="grid grid-cols-1 md:grid-cols-2 gap-6">
					<div>
						<h3 className="text-lg font-semibold mb-4">
							Contact Information
						</h3>
						<div className="space-y-2">
							<p>
								<span className="font-medium">
									Phone:
								</span>{' '}
								{
									selectedClient.phone
								}
							</p>
							<p>
								<span className="font-medium">
									Email:
								</span>{' '}
								{
									selectedClient.email
								}
							</p>
							<p>
								<span className="font-medium">
									Status:
								</span>
								<span
									className={`ml-2 px-2 py-1 rounded-full text-xs ${
										selectedClient.status ===
										'Active'
											? 'bg-green-100 text-green-800'
											: 'bg-yellow-100 text-yellow-800'
									}`}
								>
									{
										selectedClient.status
									}
								</span>
							</p>
						</div>
					</div>

					<div>
						<h3 className="text-lg font-semibold mb-4">
							Visit History
						</h3>
						<div className="space-y-3 max-h-64 overflow-y-auto">
							{clientVisits.map(
								(visit) => (
									<div
										key={
											visit.id
										}
										className="border border-gray-200 rounded-lg p-3"
									>
										<div className="flex justify-between items-start">
											<div>
												<p className="font-medium">
													{
														visit.visitType
													}
												</p>
												<p className="text-sm text-gray-600">
													{
														visit.purpose
													}
												</p>
												<p className="text-xs text-gray-500">
													{format(
														new Date(
															visit.visitDate
														),
														'MMM dd, yyyy HH:mm'
													)}
												</p>
											</div>
											<span
												className={`px-2 py-1 rounded-full text-xs ${
													visit.status ===
													'Completed'
														? 'bg-green-100 text-green-800'
														: 'bg-yellow-100 text-yellow-800'
												}`}
											>
												{
													visit.status
												}
											</span>
										</div>
										{visit.results && (
											<p className="text-sm text-gray-700 mt-2">
												{
													visit.results
												}
											</p>
										)}
									</div>
								)
							)}
						</div>
					</div>
				</div>
			</div>

			{/* Visit Form Modal */}
			{showVisitForm && (
				<div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
					<div className="bg-white rounded-lg p-6 w-full max-w-md">
						<h3 className="text-lg font-semibold mb-4">
							Add Client Visit
						</h3>
						<form
							onSubmit={
								handleVisitSubmit
							}
							className="space-y-4"
						>
							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Visit
									Date
								</label>
								<input
									type="datetime-local"
									value={
										visitForm.visitDate
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												visitDate: e
													.target
													.value,
											}
										)
									}
									required
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								/>
							</div>

							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Visit
									Type
								</label>
								<select
									value={
										visitForm.visitType
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												visitType: e
													.target
													.value,
											}
										)
									}
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								>
									<option value="Initial">
										Initial
									</option>
									<option value="Follow-up">
										Follow-up
									</option>
									<option value="Maintenance">
										Maintenance
									</option>
									<option value="Support">
										Support
									</option>
								</select>
							</div>

							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Location
								</label>
								<input
									type="text"
									value={
										visitForm.location
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												location: e
													.target
													.value,
											}
										)
									}
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								/>
							</div>

							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Purpose
								</label>
								<input
									type="text"
									value={
										visitForm.purpose
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												purpose: e
													.target
													.value,
											}
										)
									}
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								/>
							</div>

							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Notes
								</label>
								<textarea
									value={
										visitForm.notes
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												notes: e
													.target
													.value,
											}
										)
									}
									rows={3}
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								/>
							</div>

							<div>
								<label className="block text-sm font-medium text-gray-700 mb-1">
									Results
								</label>
								<textarea
									value={
										visitForm.results
									}
									onChange={(
										e
									) =>
										setVisitForm(
											{
												...visitForm,
												results: e
													.target
													.value,
											}
										)
									}
									rows={3}
									className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
								/>
							</div>

							<div className="flex justify-end space-x-3">
								<button
									type="button"
									onClick={() =>
										setShowVisitForm(
											false
										)
									}
									className="px-4 py-2 text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
								>
									Cancel
								</button>
								<button
									type="submit"
									className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
								>
									Add
									Visit
								</button>
							</div>
						</form>
					</div>
				</div>
			)}
		</div>
	);
};

export default ClientDetails;
```

## Role-Based Dashboard Integration

### 1. Sales Support Dashboard

Add this page to your existing pages in `src/pages/SalesSupportDashboard.jsx`:

```jsx
import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
	getAssignedRequests,
	updateRequestStatus,
} from '../../store/slices/requestSlice';
import { getMyClients } from '../../store/slices/clientSlice';
import ClientSearch from '../../components/clients/ClientSearch';
import ClientDetails from '../../components/clients/ClientDetails';

const SalesSupportDashboard = () => {
	const dispatch = useDispatch();
	const { assignedRequests, loading } = useSelector(
		(state) => state.requests
	);
	const { clients } = useSelector((state) => state.clients);
	const [selectedClient, setSelectedClient] = useState(null);

	useEffect(() => {
		dispatch(getAssignedRequests());
		dispatch(getMyClients());
	}, [dispatch]);

	const handleStatusUpdate = (requestId, status, comment = '') => {
		dispatch(updateRequestStatus({ requestId, status, comment }));
	};

	return (
		<div className="space-y-6">
			<div className="bg-white rounded-lg shadow p-6">
				<h1 className="text-2xl font-bold text-gray-900 mb-6">
					Sales Support Dashboard
				</h1>

				<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
					<div>
						<h2 className="text-lg font-semibold mb-4">
							Client Search
						</h2>
						<ClientSearch
							onClientSelect={
								setSelectedClient
							}
						/>
					</div>

					<div>
						<h2 className="text-lg font-semibold mb-4">
							Assigned Requests
						</h2>
						<div className="space-y-3">
							{assignedRequests.map(
								(request) => (
									<div
										key={
											request.id
										}
										className="border border-gray-200 rounded-lg p-4"
									>
										<div className="flex justify-between items-start">
											<div>
												<h3 className="font-medium">
													{
														request.requestType
													}{' '}
													Request
												</h3>
												<p className="text-sm text-gray-600">
													{
														request.clientName
													}
												</p>
												<p className="text-xs text-gray-500">
													{
														request.comment
													}
												</p>
											</div>
											<div className="flex space-x-2">
												<button
													onClick={() =>
														handleStatusUpdate(
															request.id,
															'InProgress',
															'Started working on request'
														)
													}
													className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700"
												>
													Start
												</button>
												<button
													onClick={() =>
														handleStatusUpdate(
															request.id,
															'Completed',
															'Request completed successfully'
														)
													}
													className="px-3 py-1 bg-green-600 text-white text-sm rounded hover:bg-green-700"
												>
													Complete
												</button>
											</div>
										</div>
									</div>
								)
							)}
						</div>
					</div>
				</div>
			</div>

			{selectedClient && (
				<ClientDetails clientId={selectedClient.id} />
			)}
		</div>
	);
};

export default SalesSupportDashboard;
```

### 2. Sales Manager Dashboard

Add this page to your existing pages in `src/pages/SalesManagerDashboard.jsx`:

```jsx
import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
	getSalesManagerDashboard,
	getSalesReports,
} from '../../store/slices/salesReportSlice';
import { getWeeklyPlans } from '../../store/slices/weeklyPlanSlice';

const SalesManagerDashboard = () => {
	const dispatch = useDispatch();
	const { dashboardData, salesReports, loading } = useSelector(
		(state) => state.salesReports
	);
	const { plans } = useSelector((state) => state.weeklyPlans);

	useEffect(() => {
		dispatch(getSalesManagerDashboard());
		dispatch(getSalesReports());
		dispatch(getWeeklyPlans());
	}, [dispatch]);

	if (loading) {
		return (
			<div className="text-center py-8">
				Loading dashboard...
			</div>
		);
	}

	return (
		<div className="space-y-6">
			<div className="bg-white rounded-lg shadow p-6">
				<h1 className="text-2xl font-bold text-gray-900 mb-6">
					Sales Manager Dashboard
				</h1>

				{/* Key Metrics */}
				<div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
					<div className="bg-blue-50 rounded-lg p-4">
						<h3 className="text-sm font-medium text-blue-600">
							Total Sales
						</h3>
						<p className="text-2xl font-bold text-blue-900">
							$
							{dashboardData?.totalSales?.toLocaleString() ||
								0}
						</p>
					</div>
					<div className="bg-green-50 rounded-lg p-4">
						<h3 className="text-sm font-medium text-green-600">
							Active Clients
						</h3>
						<p className="text-2xl font-bold text-green-900">
							{dashboardData?.activeClients ||
								0}
						</p>
					</div>
					<div className="bg-yellow-50 rounded-lg p-4">
						<h3 className="text-sm font-medium text-yellow-600">
							Pending Requests
						</h3>
						<p className="text-2xl font-bold text-yellow-900">
							{dashboardData?.pendingRequests ||
								0}
						</p>
					</div>
					<div className="bg-purple-50 rounded-lg p-4">
						<h3 className="text-sm font-medium text-purple-600">
							Team Performance
						</h3>
						<p className="text-2xl font-bold text-purple-900">
							{dashboardData?.teamPerformance ||
								0}
							%
						</p>
					</div>
				</div>

				{/* Weekly Plans Approval */}
				<div className="mb-8">
					<h2 className="text-lg font-semibold mb-4">
						Weekly Plans Pending Approval
					</h2>
					<div className="space-y-3">
						{plans
							.filter(
								(plan) =>
									plan.status ===
									'Submitted'
							)
							.map((plan) => (
								<div
									key={
										plan.id
									}
									className="border border-gray-200 rounded-lg p-4"
								>
									<div className="flex justify-between items-start">
										<div>
											<h3 className="font-medium">
												{
													plan.planTitle
												}
											</h3>
											<p className="text-sm text-gray-600">
												{
													plan.employeeName
												}
											</p>
											<p className="text-xs text-gray-500">
												{
													plan.weekStartDate
												}{' '}
												-{' '}
												{
													plan.weekEndDate
												}
											</p>
										</div>
										<div className="flex space-x-2">
											<button
												onClick={() =>
													dispatch(
														approveWeeklyPlan(
															{
																planId: plan.id,
																notes: 'Approved by manager',
															}
														)
													)
												}
												className="px-3 py-1 bg-green-600 text-white text-sm rounded hover:bg-green-700"
											>
												Approve
											</button>
											<button
												onClick={() =>
													dispatch(
														rejectWeeklyPlan(
															{
																planId: plan.id,
																reason: 'Please revise the plan',
															}
														)
													)
												}
												className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700"
											>
												Reject
											</button>
										</div>
									</div>
								</div>
							))}
					</div>
				</div>

				{/* Sales Reports */}
				<div>
					<h2 className="text-lg font-semibold mb-4">
						Recent Sales Reports
					</h2>
					<div className="overflow-x-auto">
						<table className="min-w-full divide-y divide-gray-200">
							<thead className="bg-gray-50">
								<tr>
									<th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
										Date
									</th>
									<th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
										Salesman
									</th>
									<th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
										Amount
									</th>
									<th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
										Status
									</th>
								</tr>
							</thead>
							<tbody className="bg-white divide-y divide-gray-200">
								{salesReports.map(
									(
										report
									) => (
										<tr
											key={
												report.id
											}
										>
											<td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
												{new Date(
													report.date
												).toLocaleDateString()}
											</td>
											<td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
												{
													report.salesmanName
												}
											</td>
											<td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
												$
												{report.amount?.toLocaleString()}
											</td>
											<td className="px-6 py-4 whitespace-nowrap">
												<span
													className={`px-2 py-1 rounded-full text-xs ${
														report.status ===
														'Completed'
															? 'bg-green-100 text-green-800'
															: 'bg-yellow-100 text-yellow-800'
													}`}
												>
													{
														report.status
													}
												</span>
											</td>
										</tr>
									)
								)}
							</tbody>
						</table>
					</div>
				</div>
			</div>
		</div>
	);
};

export default SalesManagerDashboard;
```

## Error Handling Integration

### 1. Error Boundary Component

Add this component to your existing error handling in `src/components/ErrorBoundary.jsx`:

```jsx
import React from 'react';

class ErrorBoundary extends React.Component {
	constructor(props) {
		super(props);
		this.state = { hasError: false, error: null };
	}

	static getDerivedStateFromError(error) {
		return { hasError: true, error };
	}

	componentDidCatch(error, errorInfo) {
		console.error('Error caught by boundary:', error, errorInfo);
	}

	render() {
		if (this.state.hasError) {
			return (
				<div className="min-h-screen flex items-center justify-center bg-gray-50">
					<div className="max-w-md w-full bg-white shadow-lg rounded-lg p-6">
						<div className="flex items-center mb-4">
							<div className="flex-shrink-0">
								<svg
									className="h-8 w-8 text-red-400"
									fill="none"
									viewBox="0 0 24 24"
									stroke="currentColor"
								>
									<path
										strokeLinecap="round"
										strokeLinejoin="round"
										strokeWidth={
											2
										}
										d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
									/>
								</svg>
							</div>
							<div className="ml-3">
								<h3 className="text-lg font-medium text-gray-900">
									Something
									went
									wrong
								</h3>
							</div>
						</div>
						<div className="mt-2">
							<p className="text-sm text-gray-500">
								We're sorry, but
								something
								unexpected
								happened. Please
								try refreshing
								the page.
							</p>
						</div>
						<div className="mt-4">
							<button
								onClick={() =>
									window.location.reload()
								}
								className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
							>
								Refresh Page
							</button>
						</div>
					</div>
				</div>
			);
		}

		return this.props.children;
	}
}

export default ErrorBoundary;
```

### 2. Toast Notifications

Add this component to your existing notification system in `src/components/Toast.jsx`:

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
			},
			success: {
				duration: 3000,
				iconTheme: {
					primary: '#4ade80',
					secondary: '#fff',
				},
			},
			error: {
				duration: 5000,
				iconTheme: {
					primary: '#ef4444',
					secondary: '#fff',
				},
			},
		}}
	/>
);

export default ToastContainer;
```

## Testing Integration

### 1. API Service Tests

Add these tests to your existing test suite in `src/services/__tests__/salesApiService.test.js`:

```javascript
import salesApiService from '../salesApiService';
import apiService from '../apiService';

// Mock the apiService
jest.mock('../apiService');

describe('SalesApiService', () => {
	beforeEach(() => {
		jest.clearAllMocks();
	});

	describe('Client Management', () => {
		it('should search clients', async () => {
			const mockResponse = {
				data: [{ id: 1, name: 'Test Client' }],
			};
			apiService.get.mockResolvedValue(mockResponse);

			const result = await salesApiService.searchClients(
				'test'
			);

			expect(apiService.get).toHaveBeenCalledWith(
				'/Client/search',
				{ query: 'test' }
			);
			expect(result).toEqual(mockResponse);
		});

		it('should create client', async () => {
			const clientData = {
				name: 'New Client',
				type: 'Doctor',
			};
			const mockResponse = { data: { id: 1, ...clientData } };
			apiService.post.mockResolvedValue(mockResponse);

			const result = await salesApiService.createClient(
				clientData
			);

			expect(apiService.post).toHaveBeenCalledWith(
				'/Client',
				clientData
			);
			expect(result).toEqual(mockResponse);
		});
	});

	describe('Weekly Planning', () => {
		it('should create weekly plan', async () => {
			const planData = { planTitle: 'Test Plan' };
			const mockResponse = { data: { id: 1, ...planData } };
			apiService.post.mockResolvedValue(mockResponse);

			const result = await salesApiService.createWeeklyPlan(
				planData
			);

			expect(apiService.post).toHaveBeenCalledWith(
				'/WeeklyPlan',
				planData
			);
			expect(result).toEqual(mockResponse);
		});
	});
});
```

### 2. Component Tests

Add these tests to your existing test suite in `src/components/__tests__/ClientSearch.test.jsx`:

```javascript
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import ClientSearch from '../ClientSearch';
import clientSlice from '../../store/slices/clientSlice';

const createMockStore = (initialState = {}) => {
	return configureStore({
		reducer: {
			clients: clientSlice,
		},
		preloadedState: {
			clients: {
				searchResults: [],
				loading: false,
				error: null,
				...initialState.clients,
			},
		},
	});
};

describe('ClientSearch', () => {
	it('renders search input', () => {
		const store = createMockStore();
		render(
			<Provider store={store}>
				<ClientSearch onClientSelect={jest.fn()} />
			</Provider>
		);

		expect(
			screen.getByPlaceholderText('Search clients...')
		).toBeInTheDocument();
	});

	it('shows search results when typing', async () => {
		const mockClients = [
			{
				id: 1,
				name: 'Test Client',
				type: 'Doctor',
				specialization: 'Cardiology',
			},
		];
		const store = createMockStore({
			clients: { searchResults: mockClients, loading: false },
		});

		render(
			<Provider store={store}>
				<ClientSearch onClientSelect={jest.fn()} />
			</Provider>
		);

		const input = screen.getByPlaceholderText('Search clients...');
		fireEvent.change(input, { target: { value: 'test' } });

		await waitFor(() => {
			expect(
				screen.getByText('Test Client')
			).toBeInTheDocument();
		});
	});
});
```

## Integration Steps Summary

### Step 1: Install Dependencies

```bash
npm install @microsoft/signalr axios @reduxjs/toolkit react-redux react-hot-toast react-hook-form @headlessui/react @heroicons/react
```

### Step 2: Add Environment Variables

Add the sales API URLs to your existing `.env` file.

### Step 3: Create Services

Add the sales API service and SignalR service to your existing services directory.

### Step 4: Update Redux Store

Add the sales-related slices to your existing Redux store configuration.

### Step 5: Add Components

Create the sales components in your existing components directory structure.

### Step 6: Add Pages

Create the role-based dashboard pages in your existing pages directory.

### Step 7: Update Routing

Add the new sales routes to your existing React Router configuration.

### Step 8: Add Tests

Add the sales module tests to your existing test suite.

## Conclusion

This integration guide provides everything needed to add a complete sales module to your existing React application:

- **Seamless API integration** with all sales endpoints
- **Redux state management** integration for complex data flows
- **Role-based dashboards** for different user types
- **Client management** with search and tracking
- **Weekly planning system** with approval workflows
- **Error handling** and user feedback integration
- **Testing integration** for reliable code

The integration follows React best practices and can be added to any existing React application without disrupting current functionality.
