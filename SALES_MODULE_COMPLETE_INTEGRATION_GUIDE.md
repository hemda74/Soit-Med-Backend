# Sales Module - Complete Integration Guide for Frontend Teams

**Version**: 1.0  
**Date**: 2025-10-26  
**For**: React & React Native Development Teams  
**Base URL**: `http://localhost:5117/api`

---

## Overview

This document provides a complete integration guide for the entire sales module, including weekly plans, offers with enhanced features (equipment, terms, installments), and the complete sales workflow.

---

## Table of Contents

1. [Module Architecture](#module-architecture)
2. [Database Schema](#database-schema)
3. [API Endpoints Complete Reference](#api-endpoints-complete-reference)
4. [Weekly Plan Module](#weekly-plan-module)
5. [Offer Module with Enhanced Features](#offer-module-with-enhanced-features)
6. [Deal Module](#deal-module)
7. [Task Progress Module](#task-progress-module)
8. [Offer Request Module](#offer-request-module)
9. [Sales Report Module](#sales-report-module)
10. [Frontend Implementation Examples](#frontend-implementation-examples)
11. [Integration Workflow](#integration-workflow)
12. [Sample Data](#sample-data)

---

## Module Architecture

### Complete Sales Workflow

```
Weekly Plan Created
    ↓
Tasks Added to Plan
    ↓
Task Progress Recorded
    ↓
Client Shows Interest
    ↓
Offer Request Created
    ↓
Sales Support Creates Offer with Equipment/Terms/Installments
    ↓
Sales Offer Sent to Client
    ↓
Client Accepts Offer
    ↓
Sales Deal Created
    ↓
Manager Approval
    ↓
SuperAdmin Approval
    ↓
Deal Completed/Delivered
```

### User Roles

| Role             | Description          | Permissions                                                        |
| ---------------- | -------------------- | ------------------------------------------------------------------ |
| **Salesman**     | Field sales employee | Create plans, record progress, create offer requests, create deals |
| **SalesSupport** | Sales support staff  | Create offers, manage equipment/terms/installments                 |
| **SalesManager** | Sales manager        | Review plans, approve deals, view all data                         |
| **SuperAdmin**   | System administrator | Full access to all features                                        |

---

## Database Schema

### Core Tables

#### 1. WeeklyPlans

```sql
Id (bigint) - Primary Key
EmployeeId (nvarchar(450)) - FK to AspNetUsers
WeekStartDate (date)
WeekEndDate (date)
Title (nvarchar(200))
Description (nvarchar(1000))
IsActive (bit)
Rating (int) - 1-5
ManagerComment (nvarchar(1000))
ReviewedBy (nvarchar(450))
```

#### 2. WeeklyPlanTasks

```sql
Id (bigint) - Primary Key
WeeklyPlanId (bigint) - FK to WeeklyPlans
Title (nvarchar(500))
TaskType (nvarchar(50)) - Visit, FollowUp, Call, Email, Meeting
ClientId (bigint) - FK to Clients
Status (nvarchar(50)) - Planned, InProgress, Completed, Cancelled
Priority (nvarchar(50)) - High, Medium, Low
PlannedDate (datetime2)
```

#### 3. SalesOffers

```sql
Id (bigint) - Primary Key
OfferRequestId (bigint) - FK to OfferRequests
ClientId (bigint) - FK to Clients
CreatedBy (nvarchar(450)) - SalesSupport ID
AssignedTo (nvarchar(450)) - Salesman ID
Products (nvarchar(2000))
TotalAmount (decimal(18,2))
PaymentTerms (nvarchar(2000))
DeliveryTerms (nvarchar(2000))
PaymentType (nvarchar(50)) - Cash, Installments, Other
FinalPrice (decimal(18,2))
OfferDuration (nvarchar(200))
Status (nvarchar(50)) - Draft, Sent, Accepted, etc.
ValidUntil (datetime2)
```

#### 4. OfferEquipment

```sql
Id (bigint) - Primary Key
OfferId (bigint) - FK to SalesOffers
Name (nvarchar(200))
Model (nvarchar(100))
Provider (nvarchar(100))
Country (nvarchar(100))
ImagePath (nvarchar(500))
Price (decimal(18,2))
Description (nvarchar(500))
```

#### 5. OfferTerms

```sql
Id (bigint) - Primary Key
OfferId (bigint) - FK to SalesOffers
WarrantyPeriod (nvarchar(500))
DeliveryTime (nvarchar(500))
MaintenanceTerms (nvarchar(2000))
OtherTerms (nvarchar(2000))
```

#### 6. InstallmentPlan

```sql
Id (bigint) - Primary Key
OfferId (bigint) - FK to SalesOffers
InstallmentNumber (int)
Amount (decimal(18,2))
DueDate (datetime2)
Status (nvarchar(50)) - Pending, Paid, Overdue
Notes (nvarchar(500))
```

#### 7. TaskProgresses

```sql
Id (bigint) - Primary Key
TaskId (bigint) - FK to WeeklyPlanTasks
ClientId (bigint) - FK to Clients
EmployeeId (nvarchar(450)) - FK to AspNetUsers
ProgressType (nvarchar(50)) - Visit, Call, Email, etc.
Description (nvarchar(2000))
VisitResult (nvarchar(50)) - Interested, NotInterested, etc.
NextStep (nvarchar(100))
NextFollowUpDate (datetime2)
```

#### 8. OfferRequests

```sql
Id (bigint) - Primary Key
ClientId (bigint) - FK to Clients
RequestedBy (nvarchar(450)) - FK to AspNetUsers
AssignedTo (nvarchar(450)) - FK to AspNetUsers
RequestedProducts (nvarchar(2000))
SpecialNotes (nvarchar(2000))
Status (nvarchar(50)) - Requested, InProgress, Completed
```

#### 9. SalesDeals

```sql
Id (bigint) - Primary Key
OfferId (bigint) - FK to SalesOffers
ClientId (bigint) - FK to Clients
SalesmanId (nvarchar(450)) - FK to AspNetUsers
DealValue (decimal(18,2))
Status (nvarchar(50)) - PendingManagerApproval, Approved, etc.
ManagerApprovedBy (nvarchar(450))
ManagerApprovedAt (datetime2)
SuperAdminApprovedBy (nvarchar(450))
SuperAdminApprovedAt (datetime2)
```

---

## API Endpoints Complete Reference

### Weekly Plan Endpoints

| Endpoint                      | Method | Auth    | Description               |
| ----------------------------- | ------ | ------- | ------------------------- |
| `/api/weeklyplan`             | GET    | All     | Get all plans (paginated) |
| `/api/weeklyplan`             | POST   | All     | Create new plan           |
| `/api/weeklyplan/{id}`        | GET    | All     | Get specific plan         |
| `/api/weeklyplan/{id}`        | PUT    | All     | Update plan               |
| `/api/weeklyplan/{id}/submit` | POST   | All     | Submit for review         |
| `/api/weeklyplan/{id}/review` | POST   | Manager | Review and rate plan      |
| `/api/weeklyplan/current`     | GET    | All     | Get current active plan   |

### Offer Endpoints (Enhanced)

| Endpoint                                               | Method | Auth                                 | Description                 |
| ------------------------------------------------------ | ------ | ------------------------------------ | --------------------------- |
| `/api/offer`                                           | GET    | SalesSupport,SalesManager,SuperAdmin | Get all offers with filters |
| `/api/offer/my-offers`                                 | GET    | SalesSupport,SalesManager            | Get my created offers       |
| `/api/offer/{id}`                                      | GET    | All roles                            | Get specific offer          |
| `/api/offer`                                           | POST   | SalesSupport,SalesManager            | Create new offer            |
| `/api/offer/{id}/equipment`                            | GET    | All roles                            | Get offer equipment list    |
| `/api/offer/{id}/equipment`                            | POST   | SalesSupport,SalesManager            | Add equipment to offer      |
| `/api/offer/{id}/equipment/{equipmentId}`              | DELETE | SalesSupport,SalesManager            | Delete equipment            |
| `/api/offer/{id}/equipment/{equipmentId}/upload-image` | POST   | SalesSupport,SalesManager            | Upload equipment image      |
| `/api/offer/{id}/terms`                                | POST   | SalesSupport,SalesManager            | Add/update terms            |
| `/api/offer/{id}/terms`                                | GET    | All roles                            | Get offer terms             |
| `/api/offer/{id}/installments`                         | POST   | SalesSupport,SalesManager            | Create installment plan     |
| `/api/offer/{id}/installments`                         | GET    | All roles                            | Get installment plan        |
| `/api/offer/{id}/export-pdf`                           | GET    | All roles                            | Export offer as PDF         |

### Deal Endpoints

| Endpoint                              | Method | Auth                    | Description           |
| ------------------------------------- | ------ | ----------------------- | --------------------- |
| `/api/deal`                           | GET    | Salesman,SalesManager   | Get all deals         |
| `/api/deal`                           | POST   | Salesman,SalesManager   | Create new deal       |
| `/api/deal/{id}`                      | GET    | Salesman,SalesManager   | Get specific deal     |
| `/api/deal/{id}/manager-approval`     | POST   | SalesManager,SuperAdmin | Approve/reject deal   |
| `/api/deal/pending-manager-approvals` | GET    | SalesManager            | Get pending approvals |
| `/api/deal/by-salesman/{salesmanId}`  | GET    | SalesManager            | Get deals by salesman |

### Task Progress Endpoints

| Endpoint                                  | Method | Auth                  | Description                        |
| ----------------------------------------- | ------ | --------------------- | ---------------------------------- |
| `/api/taskprogress`                       | GET    | Salesman,SalesManager | Get all task progress              |
| `/api/taskprogress`                       | POST   | Salesman,SalesManager | Create task progress               |
| `/api/taskprogress/with-offer-request`    | POST   | Salesman,SalesManager | Create progress with offer request |
| `/api/taskprogress/employee/{employeeId}` | GET    | SalesManager          | Get progress by employee           |

### Offer Request Endpoints

| Endpoint                        | Method | Auth                  | Description          |
| ------------------------------- | ------ | --------------------- | -------------------- |
| `/api/offerrequest`             | GET    | All roles             | Get all requests     |
| `/api/offerrequest`             | POST   | Salesman,SalesManager | Create request       |
| `/api/offerrequest/{id}`        | GET    | All roles             | Get specific request |
| `/api/offerrequest/{id}/assign` | PUT    | Manager               | Assign request       |
| `/api/offerrequest/{id}/status` | PUT    | Manager               | Update status        |

---

## Weekly Plan Module

### React Example: Weekly Plan Component

```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

const WeeklyPlanManager = () => {
	const [plans, setPlans] = useState([]);
	const [currentPlan, setCurrentPlan] = useState(null);
	const [loading, setLoading] = useState(true);

	// Fetch all plans
	useEffect(() => {
		const fetchPlans = async () => {
			try {
				const response = await axios.get(
					'/api/weeklyplan',
					{
						headers: {
							Authorization: `Bearer ${token}`,
						},
					}
				);
				setPlans(response.data.data.plans);
			} catch (error) {
				console.error('Error fetching plans:', error);
			} finally {
				setLoading(false);
			}
		};

		fetchPlans();
	}, []);

	// Get current active plan
	const fetchCurrentPlan = async () => {
		try {
			const response = await axios.get(
				'/api/weeklyplan/current',
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

	// Create new plan
	const createPlan = async (planData) => {
		try {
			const response = await axios.post(
				'/api/weeklyplan',
				planData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setPlans([...plans, response.data.data]);
			return response.data;
		} catch (error) {
			console.error('Error creating plan:', error);
			throw error;
		}
	};

	// Submit plan
	const submitPlan = async (planId) => {
		try {
			const response = await axios.post(
				`/api/weeklyplan/${planId}/submit`,
				{},
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			return response.data;
		} catch (error) {
			console.error('Error submitting plan:', error);
			throw error;
		}
	};

	if (loading) return <Spinner />;

	return (
		<div>
			<h2>Weekly Plans</h2>
			<button onClick={createPlan}>Create New Plan</button>
			{plans.map((plan) => (
				<div key={plan.id}>
					<h3>{plan.title}</h3>
					<p>
						Status:{' '}
						{plan.isActive
							? 'Active'
							: 'Inactive'}
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
```

### React Native Example

```jsx
import React, { useState, useEffect } from 'react';
import { View, Text, FlatList, TouchableOpacity } from 'react-native';
import axios from 'axios';

const WeeklyPlanManager = ({ token }) => {
	const [plans, setPlans] = useState([]);
	const [loading, setLoading] = useState(true);

	useEffect(() => {
		fetchPlans();
	}, []);

	const fetchPlans = async () => {
		try {
			const response = await axios.get('/api/weeklyplan', {
				headers: { Authorization: `Bearer ${token}` },
			});
			setPlans(response.data.data.plans);
		} catch (error) {
			Alert.alert('Error', 'Failed to fetch plans');
		} finally {
			setLoading(false);
		}
	};

	const submitPlan = async (planId) => {
		try {
			await axios.post(
				`/api/weeklyplan/${planId}/submit`,
				{},
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			Alert.alert('Success', 'Plan submitted successfully');
		} catch (error) {
			Alert.alert('Error', 'Failed to submit plan');
		}
	};

	return (
		<FlatList
			data={plans}
			keyExtractor={(item) => item.id.toString()}
			renderItem={({ item }) => (
				<View>
					<Text>{item.title}</Text>
					<TouchableOpacity
						onPress={() =>
							submitPlan(item.id)
						}
					>
						<Text>Submit</Text>
					</TouchableOpacity>
				</View>
			)}
		/>
	);
};
```

---

## Offer Module with Enhanced Features

### Enhanced Offer Structure

```typescript
interface EnhancedOffer {
	id: number;
	clientId: number;
	clientName: string;
	products: string;
	totalAmount: number;
	paymentTerms?: string;
	deliveryTerms?: string;
	paymentType?: 'Cash' | 'Installments' | 'Other';
	finalPrice?: number;
	offerDuration?: string;
	status: string;
	equipment: OfferEquipment[];
	terms?: OfferTerms;
	installments: InstallmentPlan[];
	validUntil: string;
	createdAt: string;
}

interface OfferEquipment {
	id: number;
	offerId: number;
	name: string;
	model?: string;
	provider?: string;
	country?: string;
	imagePath?: string;
	price: number;
	description?: string;
}

interface OfferTerms {
	id: number;
	offerId: number;
	warrantyPeriod?: string;
	deliveryTime?: string;
	maintenanceTerms?: string;
	otherTerms?: string;
}

interface InstallmentPlan {
	id: number;
	offerId: number;
	installmentNumber: number;
	amount: number;
	dueDate: string;
	status: 'Pending' | 'Paid' | 'Overdue';
	notes?: string;
}
```

### React Example: Enhanced Offer Component

```jsx
import { useState, useEffect } from 'react';
import axios from 'axios';

const EnhancedOfferManager = ({ offerId, token }) => {
	const [offer, setOffer] = useState(null);
	const [equipment, setEquipment] = useState([]);
	const [terms, setTerms] = useState(null);
	const [installments, setInstallments] = useState([]);

	useEffect(() => {
		fetchOfferDetails();
	}, [offerId]);

	const fetchOfferDetails = async () => {
		try {
			// Get offer
			const offerResponse = await axios.get(
				`/api/offer/${offerId}`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setOffer(offerResponse.data.data);

			// Get equipment
			const equipmentResponse = await axios.get(
				`/api/offer/${offerId}/equipment`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setEquipment(equipmentResponse.data.data);

			// Get terms
			const termsResponse = await axios.get(
				`/api/offer/${offerId}/terms`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setTerms(termsResponse.data.data);

			// Get installments
			const installmentsResponse = await axios.get(
				`/api/offer/${offerId}/installments`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setInstallments(installmentsResponse.data.data);
		} catch (error) {
			console.error('Error fetching offer details:', error);
		}
	};

	// Add equipment
	const addEquipment = async (equipmentData) => {
		try {
			const response = await axios.post(
				`/api/offer/${offerId}/equipment`,
				equipmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			setEquipment([...equipment, response.data.data]);
			return response.data;
		} catch (error) {
			console.error('Error adding equipment:', error);
			throw error;
		}
	};

	// Add/Update terms
	const updateTerms = async (termsData) => {
		try {
			const response = await axios.post(
				`/api/offer/${offerId}/terms`,
				termsData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			setTerms(response.data.data);
			return response.data;
		} catch (error) {
			console.error('Error updating terms:', error);
			throw error;
		}
	};

	// Create installments
	const createInstallments = async (installmentData) => {
		try {
			const response = await axios.post(
				`/api/offer/${offerId}/installments`,
				installmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			setInstallments(response.data.data);
			return response.data;
		} catch (error) {
			console.error('Error creating installments:', error);
			throw error;
		}
	};

	// Upload equipment image
	const uploadEquipmentImage = async (equipmentId, file) => {
		const formData = new FormData();
		formData.append('file', file);

		try {
			const response = await axios.post(
				`/api/offer/${offerId}/equipment/${equipmentId}/upload-image`,
				formData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'multipart/form-data',
					},
				}
			);
			return response.data;
		} catch (error) {
			console.error('Error uploading image:', error);
			throw error;
		}
	};

	// Export PDF
	const exportPdf = async () => {
		try {
			const response = await axios.get(
				`/api/offer/${offerId}/export-pdf`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
					responseType: 'blob',
				}
			);

			// Download PDF
			const url = window.URL.createObjectURL(
				new Blob([response.data])
			);
			const link = document.createElement('a');
			link.href = url;
			link.setAttribute('download', `offer-${offerId}.pdf`);
			document.body.appendChild(link);
			link.click();
			link.parentNode.removeChild(link);
		} catch (error) {
			console.error('Error exporting PDF:', error);
		}
	};

	if (!offer) return <div>Loading...</div>;

	return (
		<div className="offer-details">
			<h2>Offer #{offer.id}</h2>
			<p>Client: {offer.clientName}</p>
			<p>
				Total Amount: $
				{offer.totalAmount.toLocaleString()}
			</p>
			<p>Status: {offer.status}</p>

			{/* Equipment Section */}
			<section className="equipment-section">
				<h3>Equipment ({equipment.length})</h3>
				<button onClick={() => openAddEquipmentModal()}>
					Add Equipment
				</button>
				{equipment.map((item) => (
					<div
						key={item.id}
						className="equipment-card"
					>
						<h4>{item.name}</h4>
						<p>Model: {item.model}</p>
						<p>
							Price: $
							{item.price.toLocaleString()}
						</p>
						{item.imagePath && (
							<img
								src={
									item.imagePath
								}
								alt={item.name}
							/>
						)}
						<button
							onClick={() =>
								uploadEquipmentImage(
									item.id,
									file
								)
							}
						>
							Upload Image
						</button>
					</div>
				))}
			</section>

			{/* Terms Section */}
			<section className="terms-section">
				<h3>Terms & Conditions</h3>
				{terms && (
					<div>
						<p>
							<strong>
								Warranty:
							</strong>{' '}
							{terms.warrantyPeriod}
						</p>
						<p>
							<strong>
								Delivery:
							</strong>{' '}
							{terms.deliveryTime}
						</p>
						<p>
							<strong>
								Maintenance:
							</strong>{' '}
							{terms.maintenanceTerms}
						</p>
					</div>
				)}
				<button onClick={() => openTermsModal()}>
					Update Terms
				</button>
			</section>

			{/* Installments Section */}
			{offer.paymentType === 'Installments' && (
				<section className="installments-section">
					<h3>
						Installment Plan (
						{installments.length}{' '}
						installments)
					</h3>
					<button
						onClick={() =>
							openInstallmentsModal()
						}
					>
						Create Installments
					</button>
					{installments.map((item) => (
						<div key={item.id}>
							<p>
								Installment #
								{
									item.installmentNumber
								}
								: ${item.amount}{' '}
								- Due:{' '}
								{item.dueDate}
							</p>
							<p>
								Status:{' '}
								{item.status}
							</p>
						</div>
					))}
				</section>
			)}

			{/* Export PDF */}
			<button onClick={exportPdf}>Export as PDF</button>
		</div>
	);
};
```

### React Native Example

```jsx
import React, { useState, useEffect } from 'react';
import { View, Text, ScrollView, TouchableOpacity } from 'react-native';
import axios from 'axios';

const EnhancedOfferManager = ({ offerId, token }) => {
	const [offer, setOffer] = useState(null);
	const [equipment, setEquipment] = useState([]);

	useEffect(() => {
		fetchOfferDetails();
	}, [offerId]);

	const fetchOfferDetails = async () => {
		try {
			const offerResponse = await axios.get(
				`/api/offer/${offerId}`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setOffer(offerResponse.data.data);

			const equipmentResponse = await axios.get(
				`/api/offer/${offerId}/equipment`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setEquipment(equipmentResponse.data.data);
		} catch (error) {
			Alert.alert('Error', 'Failed to fetch offer details');
		}
	};

	const addEquipment = async (equipmentData) => {
		try {
			await axios.post(
				`/api/offer/${offerId}/equipment`,
				equipmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			fetchOfferDetails();
			Alert.alert('Success', 'Equipment added successfully');
		} catch (error) {
			Alert.alert('Error', 'Failed to add equipment');
		}
	};

	return (
		<ScrollView>
			<Text>Offer #{offer?.id}</Text>
			<Text>Client: {offer?.clientName}</Text>
			<Text>Total: ${offer?.totalAmount}</Text>

			<View>
				<Text>Equipment ({equipment.length})</Text>
				{equipment.map((item) => (
					<View key={item.id}>
						<Text>{item.name}</Text>
						<Text>
							Price: ${item.price}
						</Text>
					</View>
				))}
			</View>
		</ScrollView>
	);
};
```

---

## Deal Module

### React Example: Deal Creation

```jsx
import { useState } from 'react';
import axios from 'axios';

const CreateDeal = ({ offerId, token }) => {
	const [dealData, setDealData] = useState({
		offerId: offerId,
		clientId: null,
		dealValue: 0,
		paymentTerms: '',
		deliveryTerms: '',
		notes: '',
	});

	const createDeal = async () => {
		try {
			const response = await axios.post(
				'/api/deal',
				dealData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			Alert.alert('Success', 'Deal created successfully');
			return response.data;
		} catch (error) {
			console.error('Error creating deal:', error);
			throw error;
		}
	};

	return (
		<form onSubmit={createDeal}>
			<input
				type="number"
				value={dealData.dealValue}
				onChange={(e) =>
					setDealData({
						...dealData,
						dealValue: e.target.value,
					})
				}
				placeholder="Deal Value"
			/>
			<button type="submit">Create Deal</button>
		</form>
	);
};
```

---

## Task Progress Module

### React Example: Record Task Progress

```jsx
import { useState } from 'react';
import axios from 'axios';

const TaskProgressForm = ({ taskId, token }) => {
	const [progress, setProgress] = useState({
		taskId: taskId,
		clientId: null,
		progressType: 'Visit',
		description: '',
		visitResult: 'Interested',
		nextStep: 'NeedsOffer',
	});

	const recordProgress = async () => {
		try {
			const response = await axios.post(
				'/api/taskprogress',
				progress,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			return response.data;
		} catch (error) {
			console.error('Error recording progress:', error);
			throw error;
		}
	};

	return (
		<form onSubmit={recordProgress}>
			<select
				value={progress.progressType}
				onChange={(e) =>
					setProgress({
						...progress,
						progressType: e.target.value,
					})
				}
			>
				<option value="Visit">Visit</option>
				<option value="Call">Call</option>
				<option value="Email">Email</option>
				<option value="Meeting">Meeting</option>
			</select>
			<textarea
				value={progress.description}
				onChange={(e) =>
					setProgress({
						...progress,
						description: e.target.value,
					})
				}
				placeholder="Description"
			/>
			<button type="submit">Record Progress</button>
		</form>
	);
};
```

---

## Offer Request Module

### React Example: Create Offer Request

```jsx
import { useState } from 'react';
import axios from 'axios';

const CreateOfferRequest = ({ token, taskProgressId }) => {
	const [request, setRequest] = useState({
		taskProgressId: taskProgressId,
		requestedProducts: '',
		specialNotes: '',
	});

	const createRequest = async () => {
		try {
			const response = await axios.post(
				'/api/taskprogress/with-offer-request',
				request,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);
			Alert.alert(
				'Success',
				'Offer request created successfully'
			);
			return response.data;
		} catch (error) {
			console.error('Error creating request:', error);
			throw error;
		}
	};

	return (
		<form onSubmit={createRequest}>
			<textarea
				value={request.requestedProducts}
				onChange={(e) =>
					setRequest({
						...request,
						requestedProducts:
							e.target.value,
					})
				}
				placeholder="Requested Products"
			/>
			<button type="submit">Create Request</button>
		</form>
	);
};
```

---

## Integration Workflow

### Complete Sales Workflow Implementation

```jsx
import React, { useState, useEffect } from 'react';
import { WeeklyPlanManager } from './WeeklyPlanManager';
import { TaskProgressForm } from './TaskProgressForm';
import { EnhancedOfferManager } from './EnhancedOfferManager';
import { CreateDeal } from './CreateDeal';

const SalesWorkflow = ({ userId, userRole, token }) => {
	const [currentStep, setCurrentStep] = useState('weekly-plan');
	const [selectedPlan, setSelectedPlan] = useState(null);
	const [selectedTask, setSelectedTask] = useState(null);
	const [selectedOffer, setSelectedOffer] = useState(null);

	return (
		<div className="sales-workflow">
			{/* Step 1: Weekly Planning */}
			{currentStep === 'weekly-plan' && (
				<WeeklyPlanManager
					token={token}
					onSelectPlan={(plan) => {
						setSelectedPlan(plan);
						setCurrentStep('task-progress');
					}}
				/>
			)}

			{/* Step 2: Task Progress */}
			{currentStep === 'task-progress' && (
				<div>
					<h3>
						Tasks for Plan #
						{selectedPlan.id}
					</h3>
					{selectedPlan.tasks.map((task) => (
						<div key={task.id}>
							<h4>{task.title}</h4>
							<TaskProgressForm
								taskId={task.id}
								token={token}
								onCreateOfferRequest={(
									taskProgress
								) => {
									setCurrentStep(
										'offer-request'
									);
								}}
							/>
						</div>
					))}
				</div>
			)}

			{/* Step 3: Offer Creation (SalesSupport) */}
			{(userRole === 'SalesSupport' ||
				userRole === 'SalesManager') &&
				currentStep === 'offer-creation' && (
					<EnhancedOfferManager
						offerId={selectedOffer?.id}
						token={token}
					/>
				)}

			{/* Step 4: Deal Creation (Salesman) */}
			{(userRole === 'Salesman' ||
				userRole === 'SalesManager') &&
				currentStep === 'deal-creation' && (
					<CreateDeal
						offerId={selectedOffer?.id}
						token={token}
						onDealCreated={() => {
							setCurrentStep(
								'weekly-plan'
							);
						}}
					/>
				)}
		</div>
	);
};
```

---

## Sample Data

### Test Users

- **Salesman**: ahmed@soitmed.com (Password: 356120Ahmed@shraf2)
- **SalesManager**: salesmanager@soitmed.com (Password: 356120Ahmed@shraf2)
- **SalesSupport**: salessupport@soitmed.com (Password: 356120Ahmed@shraf2)

### Current Database Status

- **Weekly Plans**: 13 plans
- **Tasks**: 37 tasks
- **Task Progress**: 7 entries
- **Offer Requests**: 13 requests
- **Sales Offers**: 7 offers
- **Sales Deals**: 3 deals

### Sample Plan IDs

- Plan #41: Active plan for next week (3 tasks)
- Plan #34: Active plan with 2 tasks (1 completed)
- Plan #38: Current week plan

---

## Complete API Reference Summary

### Weekly Plan

- 7 endpoints (Create, Read, Update, Submit, Review, Current)

### Offers (Enhanced)

- 14 endpoints (CRUD + Equipment + Terms + Installments + PDF Export)

### Deals

- 6 endpoints (Create, Read, Approve, Pending, By Salesman)

### Task Progress

- 4 endpoints (Create, Read, By Employee, With Offer Request)

### Offer Requests

- 5 endpoints (Create, Read, Assign, Update Status, By Salesman)

---

## Error Handling

### Standard Error Response

```json
{
	"success": false,
	"message": "Error description",
	"errors": ["Detail 1", "Detail 2"]
}
```

### HTTP Status Codes

- 200 OK - Request succeeded
- 201 Created - Resource created
- 400 Bad Request - Invalid data
- 401 Unauthorized - Auth required
- 403 Forbidden - Insufficient permissions
- 404 Not Found - Resource not found
- 429 Too Many Requests - Rate limit exceeded
- 500 Internal Server Error - Server error

### Rate Limiting

- Global: 100 requests per minute
- API Endpoints: 200 requests per minute
- Auth Endpoints: 10 requests per minute

---

## Testing Credentials

```javascript
const testCredentials = {
	salesman: {
		email: 'ahmed@soitmed.com',
		password: '356120Ahmed@shraf2',
	},
	salesManager: {
		email: 'salesmanager@soitmed.com',
		password: '356120Ahmed@shraf2',
	},
	salesSupport: {
		email: 'salessupport@soitmed.com',
		password: '356120Ahmed@shraf2',
	},
};
```

---

**End of Complete Integration Guide**
