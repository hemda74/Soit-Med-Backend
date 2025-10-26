# Enhanced Offer System - Frontend Implementation Guide

## Overview

This guide provides step-by-step instructions for implementing the Enhanced Offer System in React and React Native applications.

---

## React Implementation

### 1. Create Equipment Form Component

**File:** `components/EquipmentForm.jsx`

```jsx
import React, { useState } from 'react';

const EquipmentForm = ({ offerId, onAddEquipment }) => {
	const [formData, setFormData] = useState({
		name: '',
		model: '',
		provider: '',
		country: '',
		price: '',
		description: '',
	});

	const handleSubmit = async (e) => {
		e.preventDefault();

		const equipmentData = {
			...formData,
			price: parseFloat(formData.price),
		};

		try {
			const response = await fetch(
				`/api/Offer/${offerId}/equipment`,
				{
					method: 'POST',
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
					body: JSON.stringify(equipmentData),
				}
			);

			const data = await response.json();
			if (data.success) {
				onAddEquipment(data.data);
				setFormData({
					name: '',
					model: '',
					provider: '',
					country: '',
					price: '',
					description: '',
				});
			}
		} catch (error) {
			console.error('Error adding equipment:', error);
		}
	};

	return (
		<form
			onSubmit={handleSubmit}
			className="equipment-form"
		>
			<input
				type="text"
				placeholder="Equipment Name"
				value={formData.name}
				onChange={(e) =>
					setFormData({
						...formData,
						name: e.target.value,
					})
				}
				required
			/>

			<input
				type="text"
				placeholder="Model"
				value={formData.model}
				onChange={(e) =>
					setFormData({
						...formData,
						model: e.target.value,
					})
				}
			/>

			<input
				type="text"
				placeholder="Provider"
				value={formData.provider}
				onChange={(e) =>
					setFormData({
						...formData,
						provider: e.target.value,
					})
				}
			/>

			<input
				type="text"
				placeholder="Country"
				value={formData.country}
				onChange={(e) =>
					setFormData({
						...formData,
						country: e.target.value,
					})
				}
			/>

			<input
				type="number"
				placeholder="Price"
				value={formData.price}
				onChange={(e) =>
					setFormData({
						...formData,
						price: e.target.value,
					})
				}
				required
				min="0"
				step="0.01"
			/>

			<textarea
				placeholder="Description"
				value={formData.description}
				onChange={(e) =>
					setFormData({
						...formData,
						description: e.target.value,
					})
				}
				rows={3}
			/>

			<button type="submit">Add Equipment</button>
		</form>
	);
};

export default EquipmentForm;
```

---

### 2. Create Equipment List Component

**File:** `components/EquipmentList.jsx`

```jsx
import React, { useState, useEffect } from 'react';
import axios from 'axios';

const EquipmentList = ({ offerId }) => {
	const [equipment, setEquipment] = useState([]);
	const [loading, setLoading] = useState(true);

	useEffect(() => {
		fetchEquipment();
	}, [offerId]);

	const fetchEquipment = async () => {
		try {
			const response = await axios.get(
				`/api/Offer/${offerId}/equipment`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setEquipment(response.data.data);
		} catch (error) {
			console.error('Error fetching equipment:', error);
		} finally {
			setLoading(false);
		}
	};

	const handleDelete = async (equipmentId) => {
		if (
			!window.confirm(
				'Are you sure you want to delete this equipment?'
			)
		) {
			return;
		}

		try {
			await axios.delete(
				`/api/Offer/${offerId}/equipment/${equipmentId}`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);
			setEquipment(
				equipment.filter(
					(item) => item.id !== equipmentId
				)
			);
		} catch (error) {
			console.error('Error deleting equipment:', error);
		}
	};

	if (loading) return <div>Loading...</div>;

	return (
		<div className="equipment-list">
			<h3>Equipment List</h3>
			{equipment.length === 0 ? (
				<p>No equipment added yet</p>
			) : (
				<table>
					<thead>
						<tr>
							<th>Name</th>
							<th>Model</th>
							<th>Provider</th>
							<th>Country</th>
							<th>Price</th>
							<th>Actions</th>
						</tr>
					</thead>
					<tbody>
						{equipment.map((item) => (
							<tr key={item.id}>
								<td>
									{
										item.name
									}
								</td>
								<td>
									{item.model ||
										'N/A'}
								</td>
								<td>
									{item.provider ||
										'N/A'}
								</td>
								<td>
									{item.country ||
										'N/A'}
								</td>
								<td>
									$
									{item.price.toFixed(
										2
									)}
								</td>
								<td>
									<button
										onClick={() =>
											handleDelete(
												item.id
											)
										}
									>
										Delete
									</button>
								</td>
							</tr>
						))}
					</tbody>
				</table>
			)}
		</div>
	);
};

export default EquipmentList;
```

---

### 3. Create Installment Plan Component

**File:** `components/InstallmentPlan.jsx`

```jsx
import React, { useState } from 'react';
import axios from 'axios';

const InstallmentPlan = ({ offerId, totalAmount }) => {
	const [numberOfInstallments, setNumberOfInstallments] = useState(6);
	const [startDate, setStartDate] = useState('');
	const [paymentFrequency, setPaymentFrequency] = useState('Monthly');
	const [installments, setInstallments] = useState([]);

	const handleCreate = async (e) => {
		e.preventDefault();

		const installmentData = {
			numberOfInstallments,
			startDate: new Date(startDate).toISOString(),
			paymentFrequency,
		};

		try {
			const response = await axios.post(
				`/api/Offer/${offerId}/installments`,
				installmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);

			if (response.data.success) {
				setInstallments(response.data.data);
			}
		} catch (error) {
			console.error('Error creating installments:', error);
		}
	};

	return (
		<div className="installment-plan">
			<h3>Create Installment Plan</h3>
			<form onSubmit={handleCreate}>
				<label>
					Number of Installments:
					<input
						type="number"
						value={numberOfInstallments}
						onChange={(e) =>
							setNumberOfInstallments(
								parseInt(
									e.target
										.value
								)
							)
						}
						min="1"
						max="24"
						required
					/>
				</label>

				<label>
					Start Date:
					<input
						type="date"
						value={startDate}
						onChange={(e) =>
							setStartDate(
								e.target.value
							)
						}
						required
					/>
				</label>

				<label>
					Payment Frequency:
					<select
						value={paymentFrequency}
						onChange={(e) =>
							setPaymentFrequency(
								e.target.value
							)
						}
					>
						<option value="Monthly">
							Monthly
						</option>
						<option value="Quarterly">
							Quarterly
						</option>
						<option value="Weekly">
							Weekly
						</option>
					</select>
				</label>

				<button type="submit">
					Create Installment Plan
				</button>
			</form>

			{installments.length > 0 && (
				<div className="installment-schedule">
					<h4>Installment Schedule</h4>
					<table>
						<thead>
							<tr>
								<th>#</th>
								<th>Amount</th>
								<th>
									Due Date
								</th>
								<th>Status</th>
							</tr>
						</thead>
						<tbody>
							{installments.map(
								(
									installment
								) => (
									<tr
										key={
											installment.id
										}
									>
										<td>
											{
												installment.installmentNumber
											}
										</td>
										<td>
											$
											{installment.amount.toFixed(
												2
											)}
										</td>
										<td>
											{new Date(
												installment.dueDate
											).toLocaleDateString()}
										</td>
										<td>
											{
												installment.status
											}
										</td>
									</tr>
								)
							)}
						</tbody>
					</table>
				</div>
			)}
		</div>
	);
};

export default InstallmentPlan;
```

---

### 4. Create PDF Export Component

**File:** `components/PdfExport.jsx`

```jsx
import React from 'react';
import axios from 'axios';

const PdfExport = ({ offerId }) => {
	const handleExport = async () => {
		try {
			const response = await axios.get(
				`/api/Offer/${offerId}/export-pdf`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
					responseType: 'blob',
				}
			);

			// Create blob URL
			const blob = new Blob([response.data], {
				type: 'application/pdf',
			});
			const url = window.URL.createObjectURL(blob);

			// Create download link
			const a = document.createElement('a');
			a.href = url;
			a.download = `offer-${offerId}.pdf`;
			document.body.appendChild(a);
			a.click();

			// Cleanup
			document.body.removeChild(a);
			window.URL.revokeObjectURL(url);
		} catch (error) {
			console.error('Error exporting PDF:', error);
			alert('Failed to export PDF');
		}
	};

	return (
		<button
			onClick={handleExport}
			className="pdf-export-btn"
		>
			📄 Export to PDF
		</button>
	);
};

export default PdfExport;
```

---

## React Native Implementation

### 1. Create Equipment Form (React Native)

**File:** `components/EquipmentForm.native.jsx`

```jsx
import React, { useState } from 'react';
import { View, TextInput, Button, StyleSheet, Alert } from 'react-native';
import axios from 'axios';

const EquipmentForm = ({ offerId, onAddEquipment }) => {
	const [formData, setFormData] = useState({
		name: '',
		model: '',
		provider: '',
		country: '',
		price: '',
		description: '',
	});

	const handleSubmit = async () => {
		const equipmentData = {
			...formData,
			price: parseFloat(formData.price),
		};

		try {
			const response = await axios.post(
				`${API_BASE_URL}/Offer/${offerId}/equipment`,
				equipmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);

			if (response.data.success) {
				onAddEquipment(response.data.data);
				setFormData({
					name: '',
					model: '',
					provider: '',
					country: '',
					price: '',
					description: '',
				});
				Alert.alert(
					'Success',
					'Equipment added successfully'
				);
			}
		} catch (error) {
			console.error('Error adding equipment:', error);
			Alert.alert('Error', 'Failed to add equipment');
		}
	};

	return (
		<View style={styles.form}>
			<TextInput
				style={styles.input}
				placeholder="Equipment Name"
				value={formData.name}
				onChangeText={(text) =>
					setFormData({ ...formData, name: text })
				}
			/>

			<TextInput
				style={styles.input}
				placeholder="Model"
				value={formData.model}
				onChangeText={(text) =>
					setFormData({
						...formData,
						model: text,
					})
				}
			/>

			<TextInput
				style={styles.input}
				placeholder="Provider"
				value={formData.provider}
				onChangeText={(text) =>
					setFormData({
						...formData,
						provider: text,
					})
				}
			/>

			<TextInput
				style={styles.input}
				placeholder="Country"
				value={formData.country}
				onChangeText={(text) =>
					setFormData({
						...formData,
						country: text,
					})
				}
			/>

			<TextInput
				style={styles.input}
				placeholder="Price"
				value={formData.price}
				onChangeText={(text) =>
					setFormData({
						...formData,
						price: text,
					})
				}
				keyboardType="numeric"
			/>

			<TextInput
				style={[styles.input, styles.textArea]}
				placeholder="Description"
				value={formData.description}
				onChangeText={(text) =>
					setFormData({
						...formData,
						description: text,
					})
				}
				multiline
				numberOfLines={4}
			/>

			<Button
				title="Add Equipment"
				onPress={handleSubmit}
			/>
		</View>
	);
};

const styles = StyleSheet.create({
	form: {
		padding: 20,
	},
	input: {
		borderWidth: 1,
		borderColor: '#ccc',
		borderRadius: 5,
		padding: 10,
		marginBottom: 10,
	},
	textArea: {
		height: 100,
		textAlignVertical: 'top',
	},
});

export default EquipmentForm;
```

---

### 2. Create Custom Hook for Enhanced Offer

**File:** `hooks/useEnhancedOffer.js`

```js
import { useState, useEffect } from 'react';
import axios from 'axios';

const API_BASE_URL = 'your-api-base-url';

const useEnhancedOffer = (offerId, token) => {
	const [offer, setOffer] = useState(null);
	const [equipment, setEquipment] = useState([]);
	const [terms, setTerms] = useState(null);
	const [installments, setInstallments] = useState([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState(null);

	const fetchOffer = async () => {
		try {
			const response = await axios.get(
				`${API_BASE_URL}/Offer/${offerId}`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				}
			);

			const data = response.data.data;
			setOffer(data);
			setEquipment(data.equipment || []);
			setTerms(data.terms);
			setInstallments(data.installments || []);
		} catch (err) {
			setError(err.message);
		} finally {
			setLoading(false);
		}
	};

	useEffect(() => {
		if (offerId) {
			fetchOffer();
		}
	}, [offerId]);

	const addEquipment = async (equipmentData) => {
		try {
			const response = await axios.post(
				`${API_BASE_URL}/Offer/${offerId}/equipment`,
				equipmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);

			if (response.data.success) {
				setEquipment([
					...equipment,
					response.data.data,
				]);
				return response.data;
			}
		} catch (err) {
			throw err;
		}
	};

	const updateTerms = async (termsData) => {
		try {
			const response = await axios.post(
				`${API_BASE_URL}/Offer/${offerId}/terms`,
				termsData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);

			if (response.data.success) {
				setTerms(response.data.data);
			}
			return response.data;
		} catch (err) {
			throw err;
		}
	};

	const createInstallments = async (installmentData) => {
		try {
			const response = await axios.post(
				`${API_BASE_URL}/Offer/${offerId}/installments`,
				installmentData,
				{
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type':
							'application/json',
					},
				}
			);

			if (response.data.success) {
				setInstallments(response.data.data);
			}
			return response.data;
		} catch (err) {
			throw err;
		}
	};

	const exportPdf = async () => {
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

			return response.data;
		} catch (err) {
			throw err;
		}
	};

	return {
		offer,
		equipment,
		terms,
		installments,
		loading,
		error,
		addEquipment,
		updateTerms,
		createInstallments,
		exportPdf,
		refetch: fetchOffer,
	};
};

export default useEnhancedOffer;
```

---

### 3. Complete Example Screen

**File:** `screens/EnhancedOfferScreen.jsx`

```jsx
import React from 'react';
import { View, Text, ScrollView, StyleSheet } from 'react-native';
import { useRoute } from '@react-navigation/native';
import EquipmentList from '../components/EquipmentList.native';
import EquipmentForm from '../components/EquipmentForm.native';
import PdfExport from '../components/PdfExport.native';
import useEnhancedOffer from '../hooks/useEnhancedOffer';

const EnhancedOfferScreen = () => {
	const route = useRoute();
	const { offerId, token } = route.params;

	const { offer, equipment, loading, addEquipment, exportPdf } =
		useEnhancedOffer(offerId, token);

	const handleAddEquipment = (newEquipment) => {
		addEquipment(newEquipment);
	};

	const handleExportPdf = async () => {
		try {
			const blob = await exportPdf();
			// Handle PDF export in React Native
			// You may need to use libraries like react-native-fs or expo-file-system
		} catch (error) {
			console.error('Error exporting PDF:', error);
		}
	};

	if (loading) {
		return (
			<View style={styles.container}>
				<Text>Loading...</Text>
			</View>
		);
	}

	return (
		<ScrollView style={styles.container}>
			<Text style={styles.title}>Offer #{offerId}</Text>

			<View style={styles.section}>
				<Text style={styles.subtitle}>
					Total Amount: $
					{offer?.totalAmount?.toFixed(2)}
				</Text>
				<Text>Status: {offer?.status}</Text>
				<Text>Client: {offer?.clientName}</Text>
			</View>

			<View style={styles.section}>
				<Text style={styles.sectionTitle}>
					Equipment
				</Text>
				<EquipmentForm
					offerId={offerId}
					onAddEquipment={handleAddEquipment}
				/>
				<EquipmentList
					offerId={offerId}
					equipment={equipment}
				/>
			</View>

			<View style={styles.section}>
				<PdfExport
					offerId={offerId}
					onExport={handleExportPdf}
				/>
			</View>
		</ScrollView>
	);
};

const styles = StyleSheet.create({
	container: {
		flex: 1,
		padding: 20,
	},
	title: {
		fontSize: 24,
		fontWeight: 'bold',
		marginBottom: 20,
	},
	subtitle: {
		fontSize: 18,
		fontWeight: '600',
		marginBottom: 10,
	},
	section: {
		marginBottom: 30,
	},
	sectionTitle: {
		fontSize: 18,
		fontWeight: '600',
		marginBottom: 10,
	},
});

export default EnhancedOfferScreen;
```

---

## Quick Start Checklist

### React Setup:

1. ✅ Install axios: `npm install axios`
2. ✅ Create equipment form component
3. ✅ Create equipment list component
4. ✅ Create installment plan component
5. ✅ Create PDF export component
6. ✅ Integrate with your existing offer pages

### React Native Setup:

1. ✅ Install axios: `npm install axios`
2. ✅ Install navigation: `npm install @react-navigation/native`
3. ✅ Install file system: `npm install expo-file-system`
4. ✅ Install sharing: `npm install expo-sharing`
5. ✅ Create components in native directory
6. ✅ Create custom hooks
7. ✅ Integrate into your navigation

---

## API Endpoints Summary

| Method | Endpoint                                        | Description            |
| ------ | ----------------------------------------------- | ---------------------- |
| POST   | `/api/Offer/{id}/equipment`                     | Add equipment          |
| GET    | `/api/Offer/{id}/equipment`                     | Get equipment list     |
| DELETE | `/api/Offer/{id}/equipment/{eqId}`              | Delete equipment       |
| POST   | `/api/Offer/{id}/equipment/{eqId}/upload-image` | Upload image           |
| POST   | `/api/Offer/{id}/terms`                         | Add/update terms       |
| POST   | `/api/Offer/{id}/installments`                  | Create installments    |
| GET    | `/api/Offer/{id}`                               | Get full offer details |
| GET    | `/api/Offer/{id}/export-pdf`                    | Export PDF             |

---

**Need Help?** Check `FRONTEND_API_DOCUMENTATION.md` for detailed API documentation.
