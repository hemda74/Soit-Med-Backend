## Frontend Integration - Offer Requests and Offers

### List/Search Salesmen for Offer Assignment

- Method: GET
- Path: `/api/Offer/salesmen`
- Roles: `SalesSupport, SalesManager, SuperAdmin`
- Query params:
     - `q` (optional): case-insensitive search term applied to `firstName`, `lastName`, or `userName`.

Request examples:

```bash
curl -H "Authorization: Bearer <token>" \
  https://<api-base>/api/Offer/salesmen

curl -H "Authorization: Bearer <token>" \
  "https://<api-base>/api/Offer/salesmen?q=ahmed"
```

Response shape:

```json
{
	"success": true,
	"data": [
		{
			"id": "string",
			"firstName": "string",
			"lastName": "string",
			"email": "string",
			"phoneNumber": "string",
			"userName": "string",
			"isActive": true
		}
	]
}
```

TypeScript types (frontend helper):

```ts
export type SalesmanLite = {
	id: string;
	firstName: string;
	lastName: string;
	email: string;
	phoneNumber: string;
	userName: string;
	isActive: boolean;
};

export async function fetchSalesmen(
	token: string,
	q?: string
): Promise<SalesmanLite[]> {
	const url = new URL(
		'/api/Offer/salesmen',
		process.env.NEXT_PUBLIC_API_BASE!
	);
	if (q && q.trim()) url.searchParams.set('q', q.trim());
	const res = await fetch(url.toString(), {
		headers: { Authorization: `Bearer ${token}` },
	});
	if (!res.ok) throw new Error('Failed to load salesmen');
	const body = await res.json();
	return body.data as SalesmanLite[];
}
```

UI suggestion (debounced search):

```ts
import { useEffect, useMemo, useState } from 'react';

export function useDebouncedValue<T>(value: T, delay = 300): T {
	const [debounced, setDebounced] = useState(value);
	useEffect(() => {
		const id = setTimeout(() => setDebounced(value), delay);
		return () => clearTimeout(id);
	}, [value, delay]);
	return debounced;
}

// Usage
// const [query, setQuery] = useState("");
// const dq = useDebouncedValue(query, 300);
// useEffect(() => { fetchSalesmen(token, dq) }, [dq]);
```

- Base URL: `/api`
- Auth: Bearer JWT in `Authorization` header
- Content types:
     - JSON for standard endpoints
     - `multipart/form-data` for image upload

### 1) Create Offer Request (auto-assign to SalesSupport)

- POST `/OfferRequest`
- Roles: Salesman, SalesManager

Request

```json
{
	"clientId": 1,
	"taskProgressId": 123, // optional
	"requestedProducts": "X-Ray Machine, Ultrasound",
	"specialNotes": "Urgent"
}
```

Axios example

```ts
await api.post('/OfferRequest', body);
```

Response (important fields)

```json
{
	"data": {
		"id": 91,
		"status": "Assigned", // or "Requested" if no support exists
		"assignedTo": "<supportUserId>",
		"assignedToName": "<support name>"
	}
}
```

UI notes

- If `status === "Assigned"`, show assigned support.
- If `Requested`, show banner: "Pending assignment".

---

### 2) List Offer Requests

- GET `/OfferRequest?status=Assigned|Requested|...` (optional filters)
- Role views:
     - SalesSupport: only requests assigned to me
     - Salesman: only my created requests
     - SalesManager/SuperAdmin: all

Axios example

```ts
const { data } = await api.get('/OfferRequest', {
	params: { status: 'Assigned' },
});
```

---

### 3) Reassign Offer Request (support → support)

- PUT `/OfferRequest/{id}/assign`
- Roles: SalesManager, SalesSupport, SuperAdmin

Request

```json
{ "assignedTo": "Ahmed_Hemdan_Engineering_001" }
```

---

### 4) Create Offer (auto-creates equipment from products)

- POST `/Offer`
- Roles: SalesSupport, SalesManager

Request

```json
{
	"offerRequestId": 91, // optional; if present, request becomes Ready
	"clientId": 1,
	"assignedTo": "<salesmanId>",
	"products": "X-Ray Machine, CT Scanner",
	"totalAmount": 500000,
	"validUntil": "2025-12-31T00:00:00Z",
	"notes": "..."
}
```

Behavior

- Parses `products` by comma/newline/semicolon.
- Creates equipment items with placeholder `imagePath: offers/{offerId}/equipment-placeholder.png`.

---

### 5) Assign/Reassign Offer to Salesman

- PUT `/Offer/{offerId}/assign-to-salesman`
- Roles: SalesSupport, SalesManager, SuperAdmin

Request

```json
{ "salesmanId": "<salesmanId>" }
```

---

### 6) Get Offer Details (includes equipment)

- GET `/Offer/{id}`
- Roles: SalesSupport, SalesManager, Salesman, SuperAdmin

Axios example

```ts
const { data } = await api.get(`/Offer/${offerId}`);
const offer = data.data;
// offer.equipment: [{ id, name, imagePath, price, ... }]
```

---

### 7) Upload Equipment Image

- POST `/Offer/{offerId}/equipment/{equipmentId}/upload-image`
- Roles: SalesSupport, SalesManager
- FormData

Axios example

```ts
const form = new FormData();
form.append('file', file);
await api.post(
	`/Offer/${offerId}/equipment/${equipmentId}/upload-image`,
	form,
	{
		headers: { 'Content-Type': 'multipart/form-data' },
	}
);
```

On success, `imagePath` becomes `offers/{offerId}/equipment-{equipmentId}-{guid}.ext`.

---

### 8) Export Offer PDF (optional in UI)

- GET `/Offer/{offerId}/export-pdf`
- Roles: SalesSupport, SalesManager, Salesman

```ts
const res = await api.get(`/Offer/${offerId}/export-pdf`, {
	responseType: 'blob',
});
```

---

## Minimal Client Helpers

```ts
import axios from 'axios';

export const api = axios.create({ baseURL: '/api' });
api.interceptors.request.use((cfg) => {
	const token = localStorage.getItem('token');
	if (token) cfg.headers.Authorization = `Bearer ${token}`;
	return cfg;
});
```

Common types

```ts
type OfferRequest = {
	id: number;
	status:
		| 'Requested'
		| 'Assigned'
		| 'InProgress'
		| 'Ready'
		| 'Sent'
		| 'Cancelled';
	assignedTo?: string;
	assignedToName?: string;
};

type Offer = {
	id: number;
	assignedTo: string;
	products: string;
	equipment: {
		id: number;
		name: string;
		imagePath?: string;
		price: number;
	}[];
};
```

## UI Flow Recommendations

- Salesman
     - Create Offer Request → read status and assigned support
     - View "My Requests": GET `/OfferRequest`
- SalesSupport
     - View assigned requests: GET `/OfferRequest?status=Assigned`
     - Create Offer → auto equipment created
     - Upload images for each equipment
     - Assign offer to salesman: PUT `/Offer/{id}/assign-to-salesman`
- SalesManager
     - Monitor requests/offers, reassign when needed

## Error States to Handle

- Assign to non-Salesman → 400 with message "User must have Salesman role"
- No SalesSupport users on creation → status "Requested" (show manual-assignment UI)
- Image upload validation: only JPG/JPEG/PNG/GIF and max 5MB
