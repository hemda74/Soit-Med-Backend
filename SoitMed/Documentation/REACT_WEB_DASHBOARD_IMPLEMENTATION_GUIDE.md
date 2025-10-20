# React Web Dashboard Implementation Guide - Sales Module

## Overview

This guide provides comprehensive **sales business logic and data flow** for all sales-related roles in the React web application, including Sales Manager, Sales Support, and Super Admin dashboards.

## Prerequisites

- React application with routing setup
- State management (Redux/Context/Zustand)
- HTTP client (Axios/Fetch)
- SignalR client for real-time communication

## Installation

### Required Packages

```bash
npm install @microsoft/signalr
npm install axios
npm install react-router-dom
npm install @reduxjs/toolkit react-redux
npm install react-hot-toast
```

## Core Services Implementation

### 1. API Service - Sales Module

Create `src/services/salesApiService.js`:

```javascript
import axios from 'axios';

const API_BASE_URL =
	process.env.REACT_APP_API_URL || 'https://your-api-url.com/api';

class SalesApiService {
	constructor() {
		this.api = axios.create({
			baseURL: API_BASE_URL,
			timeout: 10000,
		});

		// Request interceptor to add auth token
		this.api.interceptors.request.use(
			(config) => {
				const token = localStorage.getItem('authToken');
				if (token) {
					config.headers.Authorization = `Bearer ${token}`;
				}
				return config;
			},
			(error) => {
				return Promise.reject(error);
			}
		);

		// Response interceptor to handle errors
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

	// ==================== SALES SUPPORT ENDPOINTS ====================

	async getSalesSupportRequests(status = null, page = 1, pageSize = 20) {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString(),
		});

		if (status) {
			params.append('status', status);
		}

		const response = await this.api.get(
			`/RequestWorkflow/sales-support?${params}`
		);
		return response.data;
	}

	async updateRequestStatus(requestId, status, comment = null) {
		const response = await this.api.put(
			`/RequestWorkflow/${requestId}/status`,
			{
				status,
				comment,
			}
		);
		return response.data;
	}

	async assignRequest(requestId, assignToUserId) {
		const response = await this.api.put(
			`/RequestWorkflow/${requestId}/assign`,
			{
				assignToUserId,
			}
		);
		return response.data;
	}

	async getRequestDetails(requestId) {
		const response = await this.api.get(
			`/RequestWorkflow/${requestId}`
		);
		return response.data;
	}

	// ==================== SALES MANAGER ENDPOINTS ====================

	async getSalesManagerDashboard() {
		const response = await this.api.get('/SalesReport/dashboard');
		return response.data;
	}

	async getSalesReports(filters = {}) {
		const params = new URLSearchParams();

		if (filters.startDate)
			params.append('startDate', filters.startDate);
		if (filters.endDate) params.append('endDate', filters.endDate);
		if (filters.salesmanId)
			params.append('salesmanId', filters.salesmanId);
		if (filters.status) params.append('status', filters.status);
		if (filters.page) params.append('page', filters.page);
		if (filters.pageSize)
			params.append('pageSize', filters.pageSize);

		const response = await this.api.get(`/SalesReport?${params}`);
		return response.data;
	}

	async getSalesmanPerformance(salesmanId, period = 'monthly') {
		const response = await this.api.get(
			`/SalesReport/salesman/${salesmanId}/performance?period=${period}`
		);
		return response.data;
	}

	async getSalesTrends(period = 'monthly') {
		const response = await this.api.get(
			`/SalesReport/trends?period=${period}`
		);
		return response.data;
	}

	async getTopPerformers(limit = 10) {
		const response = await this.api.get(
			`/SalesReport/top-performers?limit=${limit}`
		);
		return response.data;
	}

	async getSalesmanList() {
		const response = await this.api.get('/Users/by-role/Salesman');
		return response.data;
	}

	async getSalesSupportList() {
		const response = await this.api.get(
			'/Users/by-role/SalesSupport'
		);
		return response.data;
	}

	async createSalesReport(reportData) {
		const response = await this.api.post(
			'/SalesReport',
			reportData
		);
		return response.data;
	}

	async updateSalesReport(reportId, reportData) {
		const response = await this.api.put(
			`/SalesReport/${reportId}`,
			reportData
		);
		return response.data;
	}

	async deleteSalesReport(reportId) {
		const response = await this.api.delete(
			`/SalesReport/${reportId}`
		);
		return response.data;
	}

	// ==================== SUPER ADMIN ENDPOINTS ====================

	async getSuperAdminDashboard() {
		const response = await this.api.get('/Admin/dashboard');
		return response.data;
	}

	async getAllSalesData(filters = {}) {
		const params = new URLSearchParams();

		if (filters.startDate)
			params.append('startDate', filters.startDate);
		if (filters.endDate) params.append('endDate', filters.endDate);
		if (filters.role) params.append('role', filters.role);
		if (filters.department)
			params.append('department', filters.department);
		if (filters.page) params.append('page', filters.page);
		if (filters.pageSize)
			params.append('pageSize', filters.pageSize);

		const response = await this.api.get(
			`/Admin/sales-data?${params}`
		);
		return response.data;
	}

	async getSystemAnalytics() {
		const response = await this.api.get('/Admin/analytics');
		return response.data;
	}

	async getRolePerformance(role) {
		const response = await this.api.get(
			`/Admin/role-performance/${role}`
		);
		return response.data;
	}

	async getDepartmentPerformance(departmentId) {
		const response = await this.api.get(
			`/Admin/department-performance/${departmentId}`
		);
		return response.data;
	}

	async getAllUsers() {
		const response = await this.api.get('/Users');
		return response.data;
	}

	async createUser(userData) {
		const response = await this.api.post('/Users', userData);
		return response.data;
	}

	async updateUser(userId, userData) {
		const response = await this.api.put(
			`/Users/${userId}`,
			userData
		);
		return response.data;
	}

	async deleteUser(userId) {
		const response = await this.api.delete(`/Users/${userId}`);
		return response.data;
	}

	async getSystemSettings() {
		const response = await this.api.get('/Admin/settings');
		return response.data;
	}

	async updateSystemSettings(settings) {
		const response = await this.api.put(
			'/Admin/settings',
			settings
		);
		return response.data;
	}

	// ==================== LEGAL MANAGER ENDPOINTS ====================

	async getLegalManagerRequests(status = null, page = 1, pageSize = 20) {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString(),
		});

		if (status) {
			params.append('status', status);
		}

		const response = await this.api.get(
			`/RequestWorkflow/legal-manager?${params}`
		);
		return response.data;
	}

	async assignToLegalEmployee(requestId, legalEmployeeId) {
		const response = await this.api.put(
			`/RequestWorkflow/${requestId}/assign-legal`,
			{
				legalEmployeeId,
			}
		);
		return response.data;
	}

	async getLegalEmployees() {
		const response = await this.api.get(
			'/Users/by-role/LegalEmployee'
		);
		return response.data;
	}

	// ==================== NOTIFICATION ENDPOINTS ====================

	async getNotifications(unreadOnly = false) {
		const response = await this.api.get(
			`/Notification?unreadOnly=${unreadOnly}`
		);
		return response.data;
	}

	async markNotificationAsRead(notificationId) {
		const response = await this.api.put(
			`/Notification/${notificationId}/read`
		);
		return response.data;
	}

	async markAllNotificationsAsRead() {
		const response = await this.api.put(
			'/Notification/mark-all-read'
		);
		return response.data;
	}

	// ==================== CLIENT TRACKING ENDPOINTS ====================

	async getClientHistory(clientId, page = 1, pageSize = 20) {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString(),
		});

		const response = await this.api.get(
			`/ClientTracking/${clientId}/history?${params}`
		);
		return response.data;
	}

	async addClientVisit(visitData) {
		const response = await this.api.post(
			'/ClientTracking/visit',
			visitData
		);
		return response.data;
	}

	async updateClientVisit(visitId, visitData) {
		const response = await this.api.put(
			`/ClientTracking/visit/${visitId}`,
			visitData
		);
		return response.data;
	}

	async deleteClientVisit(visitId) {
		const response = await this.api.delete(
			`/ClientTracking/visit/${visitId}`
		);
		return response.data;
	}

	async getClientVisits(clientId, filters = {}) {
		const params = new URLSearchParams();

		if (filters.startDate)
			params.append('startDate', filters.startDate);
		if (filters.endDate) params.append('endDate', filters.endDate);
		if (filters.visitType)
			params.append('visitType', filters.visitType);
		if (filters.salesmanId)
			params.append('salesmanId', filters.salesmanId);
		if (filters.status) params.append('status', filters.status);
		if (filters.page) params.append('page', filters.page);
		if (filters.pageSize)
			params.append('pageSize', filters.pageSize);

		const response = await this.api.get(
			`/ClientTracking/${clientId}/visits?${params}`
		);
		return response.data;
	}

	async getClientInteractionHistory(clientId, page = 1, pageSize = 20) {
		const params = new URLSearchParams({
			page: page.toString(),
			pageSize: pageSize.toString(),
		});

		const response = await this.api.get(
			`/ClientTracking/${clientId}/interactions?${params}`
		);
		return response.data;
	}

	async addClientInteraction(interactionData) {
		const response = await this.api.post(
			'/ClientTracking/interaction',
			interactionData
		);
		return response.data;
	}

	async getClientSalesHistory(clientId, filters = {}) {
		const params = new URLSearchParams();

		if (filters.startDate)
			params.append('startDate', filters.startDate);
		if (filters.endDate) params.append('endDate', filters.endDate);
		if (filters.salesmanId)
			params.append('salesmanId', filters.salesmanId);
		if (filters.status) params.append('status', filters.status);
		if (filters.page) params.append('page', filters.page);
		if (filters.pageSize)
			params.append('pageSize', filters.pageSize);

		const response = await this.api.get(
			`/ClientTracking/${clientId}/sales?${params}`
		);
		return response.data;
	}

	async getClientAnalytics(clientId, period = 'monthly') {
		const response = await this.api.get(
			`/ClientTracking/${clientId}/analytics?period=${period}`
		);
		return response.data;
	}

	async getClientSummary(clientId) {
		const response = await this.api.get(
			`/ClientTracking/${clientId}/summary`
		);
		return response.data;
	}

	async searchClients(query, filters = {}) {
		const params = new URLSearchParams();
		params.append('query', query);

		if (filters.role) params.append('role', filters.role);
		if (filters.department)
			params.append('department', filters.department);
		if (filters.page) params.append('page', filters.page);
		if (filters.pageSize)
			params.append('pageSize', filters.pageSize);

		const response = await this.api.get(
			`/ClientTracking/search?${params}`
		);
		return response.data;
	}

	async getClientTimeline(clientId, limit = 50) {
		const response = await this.api.get(
			`/ClientTracking/${clientId}/timeline?limit=${limit}`
		);
		return response.data;
	}

	async exportClientHistory(clientId, format = 'pdf', filters = {}) {
		const params = new URLSearchParams();
		params.append('format', format);

		if (filters.startDate)
			params.append('startDate', filters.startDate);
		if (filters.endDate) params.append('endDate', filters.endDate);
		if (filters.includeVisits)
			params.append('includeVisits', filters.includeVisits);
		if (filters.includeSales)
			params.append('includeSales', filters.includeSales);
		if (filters.includeInteractions)
			params.append(
				'includeInteractions',
				filters.includeInteractions
			);

		const response = await this.api.get(
			`/ClientTracking/${clientId}/export?${params}`,
			{ responseType: 'blob' }
		);
		return response.data;
	}

	// ==================== USER MANAGEMENT ENDPOINTS ====================

	async getCurrentUser() {
		const response = await this.api.get('/Users/me');
		return response.data;
	}

	async getUsersByRole(role) {
		const response = await this.api.get(`/Users/by-role/${role}`);
		return response.data;
	}

	async getDepartments() {
		const response = await this.api.get('/Department');
		return response.data;
	}

	async getRoles() {
		const response = await this.api.get('/Role');
		return response.data;
	}
}

export default new SalesApiService();
```

## Business Logic Implementation

### 1. Sales Support Dashboard Logic

**Core State Management:**

```javascript
const [requests, setRequests] = useState([]);
const [loading, setLoading] = useState(true);
const [statusFilter, setStatusFilter] = useState('');
const [selectedRequest, setSelectedRequest] = useState(null);
const [actionDialog, setActionDialog] = useState({
	open: false,
	action: '',
	requestId: null,
});
const [actionData, setActionData] = useState({
	status: '',
	comment: '',
});

// Client Tracking for Sales Support
const [clientHistory, setClientHistory] = useState([]);
const [clientVisits, setClientVisits] = useState([]);
const [selectedClient, setSelectedClient] = useState(null);
const [visitDialog, setVisitDialog] = useState({
	open: false,
	clientId: null,
	visitId: null,
});
```

**Key Business Functions:**

1. **Load Requests Logic:**

      ```javascript
      const loadRequests = async () => {
      	try {
      		setLoading(true);
      		const response =
      			await salesApiService.getSalesSupportRequests(
      				statusFilter || null
      			);
      		setRequests(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load requests');
      		console.error('Error loading requests:', error);
      	} finally {
      		setLoading(false);
      	}
      };
      ```

2. **Status Change Logic:**

      ```javascript
      const handleStatusChange = async (requestId, newStatus, comment = '') => {
      	try {
      		await salesApiService.updateRequestStatus(
      			requestId,
      			newStatus,
      			comment
      		);
      		toast.success('Request status updated successfully');
      		loadRequests(); // Refresh the list
      	} catch (error) {
      		toast.error('Failed to update request status');
      		console.error('Error updating status:', error);
      	}
      };
      ```

3. **Assignment Logic:**

      ```javascript
      const handleAssignRequest = async (requestId, assignToUserId) => {
      	try {
      		await salesApiService.assignRequest(
      			requestId,
      			assignToUserId
      		);
      		toast.success('Request assigned successfully');
      		loadRequests(); // Refresh the list
      	} catch (error) {
      		toast.error('Failed to assign request');
      		console.error('Error assigning request:', error);
      	}
      };
      ```

4. **Client Visit Management:**

      ```javascript
      const loadClientVisits = async (clientId) => {
      	try {
      		const response = await salesApiService.getClientVisits(
      			clientId
      		);
      		setClientVisits(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load client visits');
      		console.error('Error loading visits:', error);
      	}
      };

      const addClientVisit = async (visitData) => {
      	try {
      		const response = await salesApiService.addClientVisit(
      			visitData
      		);
      		toast.success('Visit recorded successfully');
      		loadClientVisits(visitData.clientId);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to record visit');
      		console.error('Error recording visit:', error);
      	}
      };

      const updateClientVisit = async (visitId, visitData) => {
      	try {
      		const response = await salesApiService.updateClientVisit(
      			visitId,
      			visitData
      		);
      		toast.success('Visit updated successfully');
      		loadClientVisits(visitData.clientId);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to update visit');
      		console.error('Error updating visit:', error);
      	}
      };
      ```

5. **Client History Tracking:**

      ```javascript
      const loadClientHistory = async (clientId) => {
      	try {
      		const response = await salesApiService.getClientHistory(
      			clientId
      		);
      		setClientHistory(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load client history');
      		console.error('Error loading history:', error);
      	}
      };

      const addClientInteraction = async (interactionData) => {
      	try {
      		const response =
      			await salesApiService.addClientInteraction(
      				interactionData
      			);
      		toast.success('Interaction recorded successfully');
      		loadClientHistory(interactionData.clientId);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to record interaction');
      		console.error('Error recording interaction:', error);
      	}
      };
      ```

### 2. Sales Manager Dashboard Logic

**Core State Management:**

```javascript
const [dashboardData, setDashboardData] = useState(null);
const [salesReports, setSalesReports] = useState([]);
const [salesmanList, setSalesmanList] = useState([]);
const [filters, setFilters] = useState({
	startDate: '',
	endDate: '',
	salesmanId: '',
	status: '',
});
const [loading, setLoading] = useState(true);

// Client Tracking for Sales Manager
const [clientAnalytics, setClientAnalytics] = useState([]);
const [clientPerformance, setClientPerformance] = useState([]);
const [selectedClient, setSelectedClient] = useState(null);
const [clientSearchQuery, setClientSearchQuery] = useState('');
const [clientSearchResults, setClientSearchResults] = useState([]);
```

**Key Business Functions:**

1. **Load Dashboard Data:**

      ```javascript
      const loadDashboardData = async () => {
      	try {
      		setLoading(true);
      		const [dashboard, reports, salesmen] = await Promise.all([
      			salesApiService.getSalesManagerDashboard(),
      			salesApiService.getSalesReports(filters),
      			salesApiService.getSalesmanList(),
      		]);

      		setDashboardData(dashboard.data);
      		setSalesReports(reports.data || []);
      		setSalesmanList(salesmen.data || []);
      	} catch (error) {
      		toast.error('Failed to load dashboard data');
      		console.error('Error loading dashboard:', error);
      	} finally {
      		setLoading(false);
      	}
      };
      ```

2. **Sales Performance Analysis:**

      ```javascript
      const getSalesmanPerformance = async (salesmanId, period = 'monthly') => {
      	try {
      		const response =
      			await salesApiService.getSalesmanPerformance(
      				salesmanId,
      				period
      			);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load performance data');
      		console.error('Error loading performance:', error);
      	}
      };
      ```

3. **Sales Trends Analysis:**

      ```javascript
      const getSalesTrends = async (period = 'monthly') => {
      	try {
      		const response = await salesApiService.getSalesTrends(
      			period
      		);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load trends data');
      		console.error('Error loading trends:', error);
      	}
      };
      ```

4. **Top Performers:**

      ```javascript
      const getTopPerformers = async (limit = 10) => {
      	try {
      		const response = await salesApiService.getTopPerformers(
      			limit
      		);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load top performers');
      		console.error('Error loading top performers:', error);
      	}
      };
      ```

5. **Client Analytics and Tracking:**

      ```javascript
      const loadClientAnalytics = async () => {
      	try {
      		const response = await salesApiService.getClientAnalytics(
      			'monthly'
      		);
      		setClientAnalytics(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load client analytics');
      		console.error('Error loading client analytics:', error);
      	}
      };

      const searchClients = async (query) => {
      	try {
      		const response = await salesApiService.searchClients(
      			query,
      			{ role: 'Client' }
      		);
      		setClientSearchResults(response.data || []);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to search clients');
      		console.error('Error searching clients:', error);
      	}
      };

      const getClientPerformance = async (clientId) => {
      	try {
      		const response = await salesApiService.getClientAnalytics(
      			clientId,
      			'monthly'
      		);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load client performance');
      		console.error('Error loading client performance:', error);
      	}
      };

      const exportClientReport = async (clientId, format = 'pdf') => {
      	try {
      		const response =
      			await salesApiService.exportClientHistory(
      				clientId,
      				format
      			);

      		// Create download link
      		const url = window.URL.createObjectURL(
      			new Blob([response])
      		);
      		const link = document.createElement('a');
      		link.href = url;
      		link.setAttribute(
      			'download',
      			`client-report-${clientId}.${format}`
      		);
      		document.body.appendChild(link);
      		link.click();
      		link.remove();
      		window.URL.revokeObjectURL(url);

      		toast.success('Client report exported successfully');
      	} catch (error) {
      		toast.error('Failed to export client report');
      		console.error('Error exporting report:', error);
      	}
      };
      ```

### 3. Super Admin Dashboard Logic

**Core State Management:**

```javascript
const [adminDashboard, setAdminDashboard] = useState(null);
const [allSalesData, setAllSalesData] = useState([]);
const [systemAnalytics, setSystemAnalytics] = useState(null);
const [users, setUsers] = useState([]);
const [departments, setDepartments] = useState([]);
const [roles, setRoles] = useState([]);
const [filters, setFilters] = useState({
	startDate: '',
	endDate: '',
	role: '',
	department: '',
});
const [loading, setLoading] = useState(true);

// Client Tracking for Super Admin
const [allClientAnalytics, setAllClientAnalytics] = useState([]);
const [clientTrackingStats, setClientTrackingStats] = useState(null);
const [clientSearchQuery, setClientSearchQuery] = useState('');
const [clientSearchResults, setClientSearchResults] = useState([]);
const [selectedClientForTracking, setSelectedClientForTracking] =
	useState(null);
```

**Key Business Functions:**

1. **Load Admin Dashboard:**

      ```javascript
      const loadAdminDashboard = async () => {
      	try {
      		setLoading(true);
      		const [
      			dashboard,
      			analytics,
      			usersData,
      			departmentsData,
      			rolesData,
      		] = await Promise.all([
      			salesApiService.getSuperAdminDashboard(),
      			salesApiService.getSystemAnalytics(),
      			salesApiService.getAllUsers(),
      			salesApiService.getDepartments(),
      			salesApiService.getRoles(),
      		]);

      		setAdminDashboard(dashboard.data);
      		setSystemAnalytics(analytics.data);
      		setUsers(usersData.data || []);
      		setDepartments(departmentsData.data || []);
      		setRoles(rolesData.data || []);
      	} catch (error) {
      		toast.error('Failed to load admin dashboard');
      		console.error('Error loading admin dashboard:', error);
      	} finally {
      		setLoading(false);
      	}
      };
      ```

2. **Get All Sales Data:**

      ```javascript
      const loadAllSalesData = async () => {
      	try {
      		const response = await salesApiService.getAllSalesData(
      			filters
      		);
      		setAllSalesData(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load sales data');
      		console.error('Error loading sales data:', error);
      	}
      };
      ```

3. **Role Performance Analysis:**

      ```javascript
      const getRolePerformance = async (role) => {
      	try {
      		const response = await salesApiService.getRolePerformance(
      			role
      		);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load role performance');
      		console.error('Error loading role performance:', error);
      	}
      };
      ```

4. **Department Performance:**

      ```javascript
      const getDepartmentPerformance = async (departmentId) => {
      	try {
      		const response =
      			await salesApiService.getDepartmentPerformance(
      				departmentId
      			);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load department performance');
      		console.error(
      			'Error loading department performance:',
      			error
      		);
      	}
      };
      ```

5. **User Management:**

      ```javascript
      const createUser = async (userData) => {
      	try {
      		const response = await salesApiService.createUser(
      			userData
      		);
      		toast.success('User created successfully');
      		loadAdminDashboard(); // Refresh the list
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to create user');
      		console.error('Error creating user:', error);
      	}
      };

      const updateUser = async (userId, userData) => {
      	try {
      		const response = await salesApiService.updateUser(
      			userId,
      			userData
      		);
      		toast.success('User updated successfully');
      		loadAdminDashboard(); // Refresh the list
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to update user');
      		console.error('Error updating user:', error);
      	}
      };

      const deleteUser = async (userId) => {
      	try {
      		await salesApiService.deleteUser(userId);
      		toast.success('User deleted successfully');
      		loadAdminDashboard(); // Refresh the list
      	} catch (error) {
      		toast.error('Failed to delete user');
      		console.error('Error deleting user:', error);
      	}
      };
      ```

6. **System-wide Client Tracking:**

      ```javascript
      const loadAllClientAnalytics = async () => {
      	try {
      		const response = await salesApiService.getAllSalesData({
      			...filters,
      			includeClientTracking: true,
      		});
      		setAllClientAnalytics(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load client analytics');
      		console.error('Error loading client analytics:', error);
      	}
      };

      const searchAllClients = async (query) => {
      	try {
      		const response = await salesApiService.searchClients(
      			query,
      			{
      				role: 'Client',
      				includeTracking: true,
      			}
      		);
      		setClientSearchResults(response.data || []);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to search clients');
      		console.error('Error searching clients:', error);
      	}
      };

      const getClientTrackingStats = async () => {
      	try {
      		const response =
      			await salesApiService.getSystemAnalytics();
      		setClientTrackingStats(
      			response.data?.clientTracking || null
      		);
      	} catch (error) {
      		toast.error('Failed to load client tracking stats');
      		console.error('Error loading tracking stats:', error);
      	}
      };

      const exportAllClientData = async (
      	format = 'pdf',
      	exportFilters = {}
      ) => {
      	try {
      		const response =
      			await salesApiService.exportClientHistory(
      				'all',
      				format,
      				exportFilters
      			);

      		// Create download link
      		const url = window.URL.createObjectURL(
      			new Blob([response])
      		);
      		const link = document.createElement('a');
      		link.href = url;
      		link.setAttribute(
      			'download',
      			`all-client-data.${format}`
      		);
      		document.body.appendChild(link);
      		link.click();
      		link.remove();
      		window.URL.revokeObjectURL(url);

      		toast.success('All client data exported successfully');
      	} catch (error) {
      		toast.error('Failed to export client data');
      		console.error('Error exporting data:', error);
      	}
      };
      ```

### 4. Client Tracking Dashboard Logic

**Core State Management:**

```javascript
const [clientHistory, setClientHistory] = useState([]);
const [clientVisits, setClientVisits] = useState([]);
const [clientInteractions, setClientInteractions] = useState([]);
const [clientSales, setClientSales] = useState([]);
const [clientSummary, setClientSummary] = useState(null);
const [clientAnalytics, setClientAnalytics] = useState(null);
const [selectedClient, setSelectedClient] = useState(null);
const [searchQuery, setSearchQuery] = useState('');
const [searchResults, setSearchResults] = useState([]);
const [filters, setFilters] = useState({
	startDate: '',
	endDate: '',
	visitType: '',
	salesmanId: '',
	status: '',
});
const [loading, setLoading] = useState(true);
const [timeline, setTimeline] = useState([]);
```

**Key Business Functions:**

1. **Load Client History:**

      ```javascript
      const loadClientHistory = async (clientId) => {
      	try {
      		setLoading(true);
      		const [
      			history,
      			visits,
      			interactions,
      			sales,
      			summary,
      			analytics,
      		] = await Promise.all([
      			salesApiService.getClientHistory(clientId),
      			salesApiService.getClientVisits(
      				clientId,
      				filters
      			),
      			salesApiService.getClientInteractionHistory(
      				clientId
      			),
      			salesApiService.getClientSalesHistory(
      				clientId,
      				filters
      			),
      			salesApiService.getClientSummary(clientId),
      			salesApiService.getClientAnalytics(
      				clientId,
      				'monthly'
      			),
      		]);

      		setClientHistory(history.data || []);
      		setClientVisits(visits.data || []);
      		setClientInteractions(interactions.data || []);
      		setClientSales(sales.data || []);
      		setClientSummary(summary.data);
      		setClientAnalytics(analytics.data);
      	} catch (error) {
      		toast.error('Failed to load client history');
      		console.error('Error loading client history:', error);
      	} finally {
      		setLoading(false);
      	}
      };
      ```

2. **Add Client Visit:**

      ```javascript
      const addClientVisit = async (visitData) => {
      	try {
      		const response = await salesApiService.addClientVisit(
      			visitData
      		);
      		toast.success('Client visit added successfully');
      		loadClientHistory(selectedClient.id); // Refresh history
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to add client visit');
      		console.error('Error adding visit:', error);
      	}
      };
      ```

3. **Update Client Visit:**

      ```javascript
      const updateClientVisit = async (visitId, visitData) => {
      	try {
      		const response = await salesApiService.updateClientVisit(
      			visitId,
      			visitData
      		);
      		toast.success('Client visit updated successfully');
      		loadClientHistory(selectedClient.id); // Refresh history
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to update client visit');
      		console.error('Error updating visit:', error);
      	}
      };
      ```

4. **Add Client Interaction:**

      ```javascript
      const addClientInteraction = async (interactionData) => {
      	try {
      		const response =
      			await salesApiService.addClientInteraction(
      				interactionData
      			);
      		toast.success('Client interaction recorded successfully');
      		loadClientHistory(selectedClient.id); // Refresh history
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to record interaction');
      		console.error('Error recording interaction:', error);
      	}
      };
      ```

5. **Search Clients:**

      ```javascript
      const searchClients = async (query, roleFilters = {}) => {
      	try {
      		const response = await salesApiService.searchClients(
      			query,
      			roleFilters
      		);
      		setSearchResults(response.data || []);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to search clients');
      		console.error('Error searching clients:', error);
      	}
      };
      ```

6. **Get Client Timeline:**

      ```javascript
      const loadClientTimeline = async (clientId, limit = 50) => {
      	try {
      		const response = await salesApiService.getClientTimeline(
      			clientId,
      			limit
      		);
      		setTimeline(response.data || []);
      		return response.data;
      	} catch (error) {
      		toast.error('Failed to load client timeline');
      		console.error('Error loading timeline:', error);
      	}
      };
      ```

7. **Export Client History:**

      ```javascript
      const exportClientHistory = async (
      	clientId,
      	format = 'pdf',
      	exportFilters = {}
      ) => {
      	try {
      		const response =
      			await salesApiService.exportClientHistory(
      				clientId,
      				format,
      				exportFilters
      			);

      		// Create download link
      		const url = window.URL.createObjectURL(
      			new Blob([response])
      		);
      		const link = document.createElement('a');
      		link.href = url;
      		link.setAttribute(
      			'download',
      			`client-history-${clientId}.${format}`
      		);
      		document.body.appendChild(link);
      		link.click();
      		link.remove();
      		window.URL.revokeObjectURL(url);

      		toast.success('Client history exported successfully');
      	} catch (error) {
      		toast.error('Failed to export client history');
      		console.error('Error exporting history:', error);
      	}
      };
      ```

### 5. Legal Manager Dashboard Logic

**Core State Management:**

```javascript
const [requests, setRequests] = useState([]);
const [legalEmployees, setLegalEmployees] = useState([]);
const [loading, setLoading] = useState(true);
const [statusFilter, setStatusFilter] = useState('');
const [selectedRequest, setSelectedRequest] = useState(null);
const [assignDialog, setAssignDialog] = useState({
	open: false,
	requestId: null,
});
const [assignData, setAssignData] = useState({ legalEmployeeId: '' });
```

**Key Business Functions:**

1. **Load Legal Requests:**

      ```javascript
      const loadRequests = async () => {
      	try {
      		setLoading(true);
      		const response =
      			await salesApiService.getLegalManagerRequests(
      				statusFilter || null
      			);
      		setRequests(response.data || []);
      	} catch (error) {
      		toast.error('Failed to load requests');
      		console.error('Error loading requests:', error);
      	} finally {
      		setLoading(false);
      	}
      };
      ```

2. **Load Legal Employees:**

      ```javascript
      const loadLegalEmployees = async () => {
      	try {
      		const response =
      			await salesApiService.getLegalEmployees();
      		setLegalEmployees(response.data || []);
      	} catch (error) {
      		console.error('Error loading legal employees:', error);
      	}
      };
      ```

3. **Assign to Legal Employee:**
      ```javascript
      const handleAssignToLegal = async (requestId, legalEmployeeId) => {
      	try {
      		await salesApiService.assignToLegalEmployee(
      			requestId,
      			legalEmployeeId
      		);
      		toast.success(
      			'Request assigned to legal employee successfully'
      		);
      		loadRequests(); // Refresh the list
      	} catch (error) {
      		toast.error('Failed to assign request');
      		console.error('Error assigning request:', error);
      	}
      };
      ```

## Data Flow Architecture

### 1. Request Workflow States

**Status Transitions:**

- `Pending` → `Assigned` (Sales Support accepts)
- `Assigned` → `InProgress` (Work starts)
- `InProgress` → `Completed` (Work finished)
- `Pending` → `Rejected` (Sales Support rejects)
- Any state → `Cancelled` (Request cancelled)

### 2. Role-Based Access Control

**Sales Support:**

- View and manage assigned requests
- Update request status
- Assign requests to other users
- View request details

**Sales Manager:**

- View all sales reports and analytics
- Monitor salesman performance
- Generate sales reports
- View sales trends and top performers

**Super Admin:**

- Full system access
- User management
- System analytics
- Department and role performance
- System settings management

**Legal Manager:**

- View legal requests
- Assign requests to legal employees
- Monitor legal process status

### 3. Client Tracking Data Structures

**Client Visit Data:**

```javascript
const visitData = {
	clientId: 'string',
	visitDate: 'datetime',
	visitType: 'string', // 'Initial', 'Follow-up', 'Maintenance', 'Support'
	location: 'string',
	purpose: 'string',
	attendees: ['string'], // Array of attendee names
	notes: 'string',
	results: 'string',
	nextVisitDate: 'datetime',
	status: 'string', // 'Completed', 'Scheduled', 'Cancelled'
	salesmanId: 'string',
	attachments: ['string'], // Array of file paths
	createdBy: 'string',
	createdAt: 'datetime',
};
```

**Client Interaction Data:**

```javascript
const interactionData = {
	clientId: 'string',
	interactionDate: 'datetime',
	interactionType: 'string', // 'Call', 'Email', 'Meeting', 'Visit'
	subject: 'string',
	description: 'string',
	participants: ['string'], // Array of participant names
	outcome: 'string',
	followUpRequired: 'boolean',
	followUpDate: 'datetime',
	priority: 'string', // 'Low', 'Medium', 'High'
	status: 'string', // 'Open', 'Closed', 'Pending'
	createdBy: 'string',
	createdAt: 'datetime',
};
```

**Client Analytics Data:**

```javascript
const clientAnalytics = {
	clientId: 'string',
	period: 'string', // 'daily', 'weekly', 'monthly', 'yearly'
	totalVisits: 'number',
	totalInteractions: 'number',
	totalSales: 'number',
	averageVisitDuration: 'number',
	lastVisitDate: 'datetime',
	nextScheduledVisit: 'datetime',
	clientSatisfactionScore: 'number',
	conversionRate: 'number',
	revenue: 'number',
	growthRate: 'number',
	topProducts: ['string'],
	keyMetrics: {
		visitFrequency: 'number',
		responseTime: 'number',
		issueResolutionRate: 'number',
	},
};
```

### 4. Dashboard Data Flow

**Sales Support Dashboard:**

1. Load user's assigned requests
2. Filter by status
3. Handle status updates
4. Assign requests to others
5. Track client visits and interactions

**Sales Manager Dashboard:**

1. Load dashboard analytics
2. Load sales reports
3. Load salesman performance data
4. Generate filtered reports
5. Track client performance and analytics
6. Export client reports

**Super Admin Dashboard:**

1. Load system-wide analytics
2. Load all users and roles
3. Load department performance
4. Manage system settings
5. Monitor all client tracking data
6. Export comprehensive client reports

**Legal Manager Dashboard:**

1. Load legal requests
2. Load legal employees
3. Assign requests to legal team

**Client Tracking Dashboard:**

1. Search and select clients
2. Load client history and timeline
3. Record visits and interactions
4. View client analytics and performance
5. Export client reports

## Error Handling

### 1. API Error Handling

```javascript
const handleApiError = (error) => {
	if (error.response?.status === 401) {
		// Redirect to login
		localStorage.removeItem('authToken');
		window.location.href = '/login';
	} else if (error.response?.status === 403) {
		toast.error('Access denied');
	} else if (error.response?.status === 404) {
		toast.error('Resource not found');
	} else if (error.response?.status >= 500) {
		toast.error('Server error. Please try again later.');
	} else {
		toast.error('An error occurred. Please try again.');
	}
	console.error('API Error:', error);
};
```

### 2. Loading States

```javascript
const [loading, setLoading] = useState(true);
const [error, setError] = useState(null);

const loadData = async () => {
	try {
		setLoading(true);
		setError(null);
		// API call
	} catch (error) {
		setError(error.message);
		handleApiError(error);
	} finally {
		setLoading(false);
	}
};
```

## Security Considerations

1. **Role-Based Access**: Each role has specific permissions
2. **Token Management**: Secure token storage and refresh
3. **Data Validation**: Client-side validation before API calls
4. **Error Sanitization**: Don't expose sensitive error details

## Performance Optimization

1. **Lazy Loading**: Load data only when needed
2. **Caching**: Cache frequently accessed data
3. **Pagination**: Implement pagination for large datasets
4. **Debouncing**: Debounce search and filter inputs

## Testing

### 1. Test API Calls

```javascript
const testApiConnection = async () => {
	try {
		const response = await salesApiService.getCurrentUser();
		console.log('API Connection successful:', response);
	} catch (error) {
		console.error('API Connection failed:', error);
	}
};
```

### 2. Test Role Permissions

```javascript
const testRoleAccess = async (role) => {
	try {
		const response = await salesApiService.getUsersByRole(role);
		console.log(`${role} access successful:`, response);
	} catch (error) {
		console.error(`${role} access failed:`, error);
	}
};
```

## Environment Configuration

Create `.env` file:

```env
REACT_APP_API_URL=https://your-api-url.com/api
REACT_APP_SIGNALR_URL=https://your-api-url.com/notificationHub
```

## Conclusion

This implementation provides comprehensive sales business logic for all roles in the sales module, with proper API endpoints, state management, and error handling for a complete sales management system. The client tracking functionality allows managers, users, and super admins to:

- **Track client history** including visits, interactions, and sales
- **Monitor client performance** with analytics and reporting
- **Record detailed interactions** with visit management and interaction logging
- **Export comprehensive reports** for client analysis
- **Search and filter clients** across all roles with appropriate permissions
- **Generate client timelines** for complete activity tracking

The system provides role-based access to client tracking features, ensuring that each user type has appropriate visibility and control over client data while maintaining security and data integrity.
