# Enhanced Offer System - Frontend API Documentation

## Overview

This document provides complete API documentation for the Enhanced Offer System, designed for React and React Native frontend developers.

---

## Base URL

```
Development: http://localhost:5000/api
Production: https://your-production-url.com/api
```

---

## Authentication

All endpoints require JWT Bearer token authentication.

```javascript
const headers = {
	Authorization: `Bearer ${token}`,
	'Content-Type': 'application/json',
};
```

---

## Table of Contents

1. [Equipment Management](#equipment-management)
2. [Terms Management](#terms-management)
3. [Installment Plans](#installment-plans)
4. [Enhanced Offer Operations](#enhanced-offer-operations)
5. [PDF Export](#pdf-export)
6. [Data Models](#data-models)
7. [Error Handling](#error-handling)

---

## Equipment Management

### 1. Add Equipment to Offer

**POST** `/api/Offer/{offerId}/equipment`

**Description:** Adds new equipment items to an existing offer.

**Request Body:**

```json
{
	"name": "Ultrasound Machine",
	"model": "US-2000",
	"provider": "MedTech Inc",
	"country": "USA",
	"price": 25000.0,
	"description": "High-resolution ultrasound system"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 5,
		"name": "Ultrasound Machine",
		"model": "US-2000",
		"provider": "MedTech Inc",
		"country": "USA",
		"imagePath": null,
		"price": 25000.0,
		"description": "High-resolution ultrasound system"
	},
	"message": "Equipment added successfully"
}
```

**React Example:**

```jsx
const addEquipment = async (offerId, equipmentData) => {
	const response = await fetch(`/api/Offer/${offerId}/equipment`, {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'Content-Type': 'application/json',
		},
		body: JSON.stringify(equipmentData),
	});

	return await response.json();
};

// Usage
const newEquipment = {
	name: 'CT Scanner',
	model: 'CT-3000',
	provider: 'ScanTech',
	country: 'Germany',
	price: 150000.0,
	description: '64-slice CT scanner',
};

const result = await addEquipment(offerId, newEquipment);
```

**React Native Example:**

```jsx
import axios from 'axios';

const addEquipment = async (offerId, equipmentData) => {
	try {
		const response = await axios.post(
			`${API_BASE_URL}/Offer/${offerId}/equipment`,
			equipmentData,
			{
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'application/json',
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error adding equipment:', error);
		throw error;
	}
};
```

---

### 2. Get Offer Equipment List

**GET** `/api/Offer/{offerId}/equipment`

**Description:** Retrieves all equipment items for an offer.

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"offerId": 5,
			"name": "Ultrasound Machine",
			"model": "US-2000",
			"provider": "MedTech Inc",
			"country": "USA",
			"imagePath": "uploads/offer-equipment/5/1/image.jpg",
			"price": 25000.0,
			"description": "High-resolution ultrasound system"
		},
		{
			"id": 2,
			"offerId": 5,
			"name": "CT Scanner",
			"model": "CT-3000",
			"provider": "ScanTech",
			"country": "Germany",
			"imagePath": null,
			"price": 150000.0,
			"description": "64-slice CT scanner"
		}
	],
	"message": "Equipment list retrieved successfully"
}
```

**React Example:**

```jsx
const [equipment, setEquipment] = useState([]);

useEffect(() => {
	const fetchEquipment = async () => {
		const response = await fetch(
			`/api/Offer/${offerId}/equipment`,
			{
				headers: {
					Authorization: `Bearer ${token}`,
				},
			}
		);
		const data = await response.json();
		setEquipment(data.data);
	};

	fetchEquipment();
}, [offerId]);
```

---

### 3. Upload Equipment Image

**POST** `/api/Offer/{offerId}/equipment/{equipmentId}/upload-image`

**Description:** Uploads an image for a specific equipment item.

**Request:** `multipart/form-data`

**Form Data:**

- `file`: Image file (JPG, PNG, GIF, max 5MB)

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"relativePath": "uploads/offer-equipment/5/1/abc123.jpg",
		"fileName": "abc123.jpg",
		"fileSize": 245678,
		"contentType": "image/jpeg"
	},
	"message": "Image uploaded successfully"
}
```

**React Example with File Upload:**

```jsx
const uploadEquipmentImage = async (offerId, equipmentId, file) => {
	const formData = new FormData();
	formData.append('file', file);

	const response = await fetch(
		`/api/Offer/${offerId}/equipment/${equipmentId}/upload-image`,
		{
			method: 'POST',
			headers: {
				Authorization: `Bearer ${token}`,
			},
			body: formData,
		}
	);

	return await response.json();
};

// Usage with file input
const handleImageUpload = async (event) => {
	const file = event.target.files[0];
	if (file) {
		const result = await uploadEquipmentImage(
			offerId,
			equipmentId,
			file
		);
		console.log('Image uploaded:', result.data.relativePath);
	}
};
```

**React Native Example:**

```jsx
import * as ImagePicker from 'expo-image-picker';
import axios from 'axios';

const uploadEquipmentImage = async (offerId, equipmentId, uri) => {
	const formData = new FormData();

	formData.append('file', {
		uri,
		type: 'image/jpeg',
		name: 'equipment.jpg',
	});

	try {
		const response = await axios.post(
			`${API_BASE_URL}/Offer/${offerId}/equipment/${equipmentId}/upload-image`,
			formData,
			{
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'multipart/form-data',
				},
			}
		);
		return response.data;
	} catch (error) {
		console.error('Error uploading image:', error);
		throw error;
	}
};

// Usage with image picker
const pickAndUploadImage = async (offerId, equipmentId) => {
	const result = await ImagePicker.launchImageLibraryAsync({
		mediaTypes: ImagePicker.MediaTypeOptions.Images,
		allowsEditing: true,
		quality: 0.8,
	});

	if (!result.cancelled) {
		await uploadEquipmentImage(offerId, equipmentId, result.uri);
	}
};
```

---

### 4. Delete Equipment

**DELETE** `/api/Offer/{offerId}/equipment/{equipmentId}`

**Description:** Removes an equipment item from an offer.

**Response (200 OK):**

```json
{
	"success": true,
	"message": "Equipment deleted successfully"
}
```

---

## Terms Management

### 1. Add/Update Terms

**POST** `/api/Offer/{offerId}/terms`

**Description:** Adds or updates general terms and conditions for an offer.

**Request Body:**

```json
{
	"warrantyPeriod": "2 years comprehensive warranty",
	"deliveryTime": "30-45 days from order confirmation",
	"maintenanceTerms": "Free maintenance for first year, then annual service contract",
	"otherTerms": "Installation and training included"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 5,
		"warrantyPeriod": "2 years comprehensive warranty",
		"deliveryTime": "30-45 days from order confirmation",
		"maintenanceTerms": "Free maintenance for first year, then annual service contract",
		"otherTerms": "Installation and training included"
	},
	"message": "Terms added successfully"
}
```

**React Example:**

```jsx
const updateTerms = async (offerId, termsData) => {
	const response = await fetch(`/api/Offer/${offerId}/terms`, {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'Content-Type': 'application/json',
		},
		body: JSON.stringify(termsData),
	});

	return await response.json();
};
```

---

## Installment Plans

### 1. Create Installment Plan

**POST** `/api/Offer/{offerId}/installments`

**Description:** Creates an installment payment plan for an offer.

**Request Body:**

```json
{
	"numberOfInstallments": 6,
	"startDate": "2025-11-01T00:00:00Z",
	"paymentFrequency": "Monthly"
}
```

**Response (200 OK):**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"offerId": 5,
			"installmentNumber": 1,
			"amount": 29166.67,
			"dueDate": "2025-11-01T00:00:00Z",
			"status": "Pending"
		},
		{
			"id": 2,
			"offerId": 5,
			"installmentNumber": 2,
			"amount": 29166.67,
			"dueDate": "2025-12-01T00:00:00Z",
			"status": "Pending"
		}
		// ... more installments
	],
	"message": "Installment plan created successfully"
}
```

**React Example:**

```jsx
const createInstallments = async (offerId, installmentData) => {
	const response = await fetch(`/api/Offer/${offerId}/installments`, {
		method: 'POST',
		headers: {
			Authorization: `Bearer ${token}`,
			'Content-Type': 'application/json',
		},
		body: JSON.stringify(installmentData),
	});

	return await response.json();
};

// Usage
const installmentPlan = {
	numberOfInstallments: 6,
	startDate: new Date('2025-11-01').toISOString(),
	paymentFrequency: 'Monthly',
};

const result = await createInstallments(offerId, installmentPlan);
```

---

## Enhanced Offer Operations

### 1. Get Enhanced Offer Details

**GET** `/api/Offer/{offerId}`

**Description:** Retrieves complete offer details including equipment, terms, and installments.

**Response (200 OK):**

```json
{
	"success": true,
	"data": {
		"id": 5,
		"offerRequestId": 3,
		"clientId": 12,
		"clientName": "Cairo Hospital",
		"createdBy": "user123",
		"createdByName": "Ahmed Mohamed",
		"assignedTo": "salesman456",
		"assignedToName": "Omar Hassan",
		"products": "Medical equipment package",
		"totalAmount": 175000.0,
		"paymentTerms": "30% down payment, balance in installments",
		"deliveryTerms": "FOB port",
		"validUntil": "2025-12-31T00:00:00Z",
		"status": "Draft",
		"sentToClientAt": null,
		"clientResponse": null,
		"createdAt": "2025-10-26T10:30:00Z",
		"paymentType": "Installments",
		"finalPrice": 175000.0,
		"offerDuration": "90 days",
		"equipment": [
			{
				"id": 1,
				"name": "Ultrasound Machine",
				"model": "US-2000",
				"provider": "MedTech Inc",
				"country": "USA",
				"price": 25000.0
			}
		],
		"terms": {
			"warrantyPeriod": "2 years",
			"deliveryTime": "30 days",
			"maintenanceTerms": "1 year free maintenance"
		},
		"installments": [
			{
				"installmentNumber": 1,
				"amount": 29166.67,
				"dueDate": "2025-11-01T00:00:00Z",
				"status": "Pending"
			}
		]
	},
	"message": "Offer retrieved successfully"
}
```

---

## PDF Export

### 1. Export Offer as PDF

**GET** `/api/Offer/{offerId}/export-pdf`

**Description:** Generates and downloads the offer as a professional PDF with company letterhead.

**Response (200 OK):** Binary PDF file

**Headers:**

```
Content-Type: application/pdf
Content-Disposition: attachment; filename="offer-5.pdf"
```

**React Example:**

```jsx
const exportOfferPdf = async (offerId) => {
	const response = await fetch(`/api/Offer/${offerId}/export-pdf`, {
		method: 'GET',
		headers: {
			Authorization: `Bearer ${token}`,
		},
	});

	const blob = await response.blob();
	const url = window.URL.createObjectURL(blob);
	const a = document.createElement('a');
	a.href = url;
	a.download = `offer-${offerId}.pdf`;
	document.body.appendChild(a);
	a.click();
	document.body.removeChild(a);
	window.URL.revokeObjectURL(url);
};

// Usage
<button onClick={() => exportOfferPdf(offerId)}>Export PDF</button>;
```

**React Native Example:**

```jsx
import * as FileSystem from 'expo-file-system';
import * as Sharing from 'expo-sharing';

const exportOfferPdf = async (offerId) => {
	try {
		const response = await axios.get(
			`${API_BASE_URL}/Offer/${offerId}/export-pdf`,
			{
				headers: {
					Authorization: `Bearer ${token}`,
				},
				responseType: 'blob',
			}
		);

		const fileUri = `${FileSystem.documentDirectory}offer-${offerId}.pdf`;
		await FileSystem.writeAsStringAsync(fileUri, response.data, {
			encoding: FileSystem.EncodingType.Base64,
		});

		await Sharing.shareAsync(fileUri);
	} catch (error) {
		console.error('Error exporting PDF:', error);
	}
};
```

---

## Data Models

### OfferEquipment

```typescript
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
```

### OfferTerms

```typescript
interface OfferTerms {
	id: number;
	offerId: number;
	warrantyPeriod?: string;
	deliveryTime?: string;
	maintenanceTerms?: string;
	otherTerms?: string;
}
```

### InstallmentPlan

```typescript
interface InstallmentPlan {
	id: number;
	offerId: number;
	installmentNumber: number;
	amount: number;
	dueDate: string; // ISO date string
	status: 'Pending' | 'Paid' | 'Overdue';
	notes?: string;
}
```

### EnhancedOfferResponse

```typescript
interface EnhancedOfferResponse {
	id: number;
	offerRequestId?: number;
	clientId: number;
	clientName: string;
	createdBy: string;
	createdByName: string;
	assignedTo: string;
	assignedToName: string;
	products: string;
	totalAmount: number;
	paymentTerms?: string;
	deliveryTerms?: string;
	validUntil: string;
	status: string;
	sentToClientAt?: string;
	clientResponse?: string;
	createdAt: string;
	paymentType?: 'Cash' | 'Installments' | 'Other';
	finalPrice?: number;
	offerDuration?: string;
	equipment: OfferEquipment[];
	terms?: OfferTerms;
	installments: InstallmentPlan[];
}
```

---

## Error Handling

### Standard Error Response

```json
{
	"success": false,
	"message": "Error description",
	"errors": ["Specific error detail 1", "Specific error detail 2"]
}
```

### HTTP Status Codes

- `200 OK` - Request succeeded
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Error Handling Example

```jsx
const fetchOffer = async (offerId) => {
	try {
		const response = await fetch(`/api/Offer/${offerId}`, {
			headers: {
				Authorization: `Bearer ${token}`,
			},
		});

		if (!response.ok) {
			const error = await response.json();
			throw new Error(
				error.message || 'Failed to fetch offer'
			);
		}

		const data = await response.json();
		return data.data;
	} catch (error) {
		console.error('Error:', error);
		// Handle error in UI
		setError(error.message);
	}
};
```

---

## Complete React Hook Example

```jsx
import { useState, useEffect } from 'react';

const useEnhancedOffer = (offerId) => {
	const [offer, setOffer] = useState(null);
	const [equipment, setEquipment] = useState([]);
	const [terms, setTerms] = useState(null);
	const [installments, setInstallments] = useState([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);

	useEffect(() => {
		const fetchOffer = async () => {
			try {
				const response = await fetch(
					`/api/Offer/${offerId}`,
					{
						headers: {
							Authorization: `Bearer ${token}`,
						},
					}
				);

				if (!response.ok)
					throw new Error(
						'Failed to fetch offer'
					);

				const data = await response.json();
				setOffer(data.data);
				setEquipment(data.data.equipment || []);
				setTerms(data.data.terms);
				setInstallments(data.data.installments || []);
			} catch (err) {
				setError(err.message);
			} finally {
				setLoading(false);
			}
		};

		fetchOffer();
	}, [offerId]);

	const addEquipment = async (equipmentData) => {
		const response = await fetch(
			`/api/Offer/${offerId}/equipment`,
			{
				method: 'POST',
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'application/json',
				},
				body: JSON.stringify(equipmentData),
			}
		);

		const data = await response.json();
		if (data.success) {
			setEquipment([...equipment, data.data]);
		}
		return data;
	};

	const exportPdf = async () => {
		const response = await fetch(
			`/api/Offer/${offerId}/export-pdf`,
			{
				headers: {
					Authorization: `Bearer ${token}`,
				},
			}
		);

		const blob = await response.blob();
		const url = window.URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = `offer-${offerId}.pdf`;
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);
		window.URL.revokeObjectURL(url);
	};

	return {
		offer,
		equipment,
		terms,
		installments,
		loading,
		error,
		addEquipment,
		exportPdf,
	};
};

export default useEnhancedOffer;
```

---

## Testing

You can test all endpoints using Postman or curl:

```bash
# Get offer equipment
curl -X GET \
  'http://localhost:5000/api/Offer/5/equipment' \
  -H 'Authorization: Bearer YOUR_TOKEN'

# Add equipment
curl -X POST \
  'http://localhost:5000/api/Offer/5/equipment' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
    "name": "MRI Machine",
    "model": "MRI-4000",
    "provider": "MedScan",
    "country": "Germany",
    "price": 500000.00
  }'

# Export PDF
curl -X GET \
  'http://localhost:5000/api/Offer/5/export-pdf' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  --output offer.pdf
```

---

## Support

For any issues or questions:

- Check the implementation status document
- Review the SQL migration script (if needed)
- Contact the backend team

**Version:** 1.0.0  
**Last Updated:** October 26, 2025
