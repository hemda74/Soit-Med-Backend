# Products Catalog API Documentation

## Overview

The Products Catalog system allows SalesSupport and Sales Managers to manage a catalog of products/equipment that can be retrieved when creating offers. This replaces the manual entry system with a centralized catalog.

---

## 📋 Table Structure

The `Products` table contains:

- **Id** (BIGINT, Identity) - Primary Key
- **Name** (NVARCHAR(200), Required)
- **Model** (NVARCHAR(100), Optional)
- **Provider** (NVARCHAR(100), Optional) - Manufacturer/Provider
- **Country** (NVARCHAR(100), Optional)
- **Category** (NVARCHAR(100), Optional) - e.g., "X-Ray", "Ultrasound", "CT Scanner"
- **BasePrice** (DECIMAL(18,2), Required)
- **Description** (NVARCHAR(2000), Optional)
- **ImagePath** (NVARCHAR(500), Optional)
- **Year** (INT, Optional)
- **InStock** (BIT, Default: 1)
- **IsActive** (BIT, Default: 1)
- **CreatedBy** (NVARCHAR(450), Optional) - User ID
- **CreatedAt** (DATETIME2, Default: GETUTCDATE())
- **UpdatedAt** (DATETIME2, Default: GETUTCDATE())

---

## 🔐 Authorization

| Endpoint                       | Roles Allowed                                    |
| ------------------------------ | ------------------------------------------------ |
| GET (All, Search, By Category) | SalesSupport, SalesManager, Salesman, SuperAdmin |
| GET (ById)                     | SalesSupport, SalesManager, Salesman, SuperAdmin |
| POST (Create)                  | SalesSupport, SalesManager, SuperAdmin           |
| PUT (Update)                   | SalesSupport, SalesManager, SuperAdmin           |
| DELETE                         | SalesManager, SuperAdmin                         |
| POST (Upload Image)            | SalesSupport, SalesManager, SuperAdmin           |

---

## 📡 API Endpoints

### 1. Get All Products

**GET** `/api/Product`

**Query Parameters:**

- `category` (optional) - Filter by category
- `inStock` (optional, boolean) - Filter by stock status

**Example:**

```http
GET /api/Product?category=X-Ray&inStock=true
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": [
		{
			"id": 1,
			"name": "جهاز أشعة رقمي",
			"model": "XR-200",
			"provider": "MedTech",
			"country": "ألمانيا",
			"category": "X-Ray",
			"basePrice": 350000.0,
			"description": "جهاز أشعة رقمي عالي الجودة",
			"imagePath": null,
			"year": 2024,
			"inStock": true,
			"isActive": true,
			"createdBy": "user-id-123",
			"createdByName": "John Doe",
			"createdAt": "2025-01-02T10:00:00Z",
			"updatedAt": "2025-01-02T10:00:00Z"
		}
	],
	"message": "Products retrieved successfully"
}
```

---

### 2. Get Product by ID

**GET** `/api/Product/{id}`

**Example:**

```http
GET /api/Product/1
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"name": "جهاز أشعة رقمي",
		"model": "XR-200",
		"provider": "MedTech",
		"country": "ألمانيا",
		"category": "X-Ray",
		"basePrice": 350000.0,
		"description": "جهاز أشعة رقمي عالي الجودة",
		"imagePath": "products/product-1.jpg",
		"year": 2024,
		"inStock": true,
		"isActive": true,
		"createdBy": "user-id-123",
		"createdByName": "John Doe",
		"createdAt": "2025-01-02T10:00:00Z",
		"updatedAt": "2025-01-02T10:00:00Z"
	},
	"message": "Product retrieved successfully"
}
```

---

### 3. Get Products by Category

**GET** `/api/Product/category/{category}`

**Example:**

```http
GET /api/Product/category/Ultrasound
Authorization: Bearer {token}
```

---

### 4. Search Products

**GET** `/api/Product/search?q={searchTerm}`

Searches in: Name, Model, Provider, Category, Description

**Example:**

```http
GET /api/Product/search?q=ultrasound
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": [
		{
			"id": 2,
			"name": "جهاز الموجات فوق الصوتية",
			"model": "US-500",
			"provider": "Sonix",
			"category": "Ultrasound",
			"basePrice": 250000.0,
			"inStock": true,
			"isActive": true
		}
	],
	"message": "Products retrieved successfully"
}
```

---

### 5. Create Product

**POST** `/api/Product`

**Request Body:**

```json
{
	"name": "جهاز أشعة رقمي",
	"model": "XR-200",
	"provider": "MedTech",
	"country": "ألمانيا",
	"category": "X-Ray",
	"basePrice": 350000.0,
	"description": "جهاز أشعة رقمي عالي الجودة مع ضمان شامل",
	"year": 2024,
	"inStock": true
}
```

**Example:**

```http
POST /api/Product
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "جهاز أشعة رقمي",
  "model": "XR-200",
  "provider": "MedTech",
  "country": "ألمانيا",
  "category": "X-Ray",
  "basePrice": 350000.00,
  "description": "جهاز أشعة رقمي عالي الجودة",
  "year": 2024,
  "inStock": true
}
```

**Response:**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"name": "جهاز أشعة رقمي",
		"model": "XR-200",
		"provider": "MedTech",
		"country": "ألمانيا",
		"category": "X-Ray",
		"basePrice": 350000.0,
		"description": "جهاز أشعة رقمي عالي الجودة",
		"imagePath": null,
		"year": 2024,
		"inStock": true,
		"isActive": true,
		"createdBy": "user-id-123",
		"createdByName": "John Doe",
		"createdAt": "2025-01-02T10:00:00Z",
		"updatedAt": "2025-01-02T10:00:00Z"
	},
	"message": "Product created successfully"
}
```

---

### 6. Update Product

**PUT** `/api/Product/{id}`

**Request Body:**

```json
{
	"name": "جهاز أشعة رقمي - محدث",
	"model": "XR-200 Pro",
	"basePrice": 375000.0,
	"inStock": false,
	"description": "إصدار محدث مع مميزات جديدة"
}
```

**Example:**

```http
PUT /api/Product/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "جهاز أشعة رقمي - محدث",
  "basePrice": 375000.00,
  "inStock": false
}
```

**Note:** All fields are optional in the request. Only provided fields will be updated.

---

### 7. Delete Product (Soft Delete)

**DELETE** `/api/Product/{id}`

**Authorization:** SalesManager, SuperAdmin only

**Example:**

```http
DELETE /api/Product/1
Authorization: Bearer {token}
```

**Response:**

```json
{
	"success": true,
	"data": null,
	"message": "Product deleted successfully"
}
```

**Note:** This is a soft delete - sets `IsActive = false`. The product remains in the database but won't appear in active product lists.

---

### 8. Upload Product Image

**POST** `/api/Product/{id}/upload-image`

**Content-Type:** `multipart/form-data`

**Request:**

- `file` (Form Data) - Image file (JPG, PNG, GIF, max 5MB)

**Example:**

```http
POST /api/Product/1/upload-image
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [binary image data]
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "جهاز أشعة رقمي",
    "imagePath": "products/product-1.jpg",
    ...
  },
  "message": "Product image uploaded successfully"
}
```

---

## 💡 Usage Examples

### Scenario 1: SalesSupport Retrieving Products for Offer Creation

```http
# Step 1: Get all X-Ray products in stock
GET /api/Product?category=X-Ray&inStock=true
Authorization: Bearer {salesSupport_token}

# Step 2: Search for specific product
GET /api/Product/search?q=XR-200
Authorization: Bearer {salesSupport_token}

# Step 3: Get full details of selected product
GET /api/Product/1
Authorization: Bearer {salesSupport_token}
```

### Scenario 2: SalesSupport Adding New Product to Catalog

```http
POST /api/Product
Authorization: Bearer {salesSupport_token}
Content-Type: application/json

{
  "name": "جهاز CT Scanner",
  "model": "CT-3000",
  "provider": "MedImaging",
  "country": "الولايات المتحدة",
  "category": "CT Scanner",
  "basePrice": 2500000.00,
  "description": "جهاز CT Scanner متقدم مع دقة عالية",
  "year": 2024,
  "inStock": true
}

# Then upload image
POST /api/Product/2/upload-image
Authorization: Bearer {salesSupport_token}
Content-Type: multipart/form-data
file: [image file]
```

### Scenario 3: SalesManager Managing Catalog

```http
# Update product price
PUT /api/Product/1
Authorization: Bearer {salesManager_token}
Content-Type: application/json

{
  "basePrice": 400000.00
}

# Mark product as out of stock
PUT /api/Product/1
Authorization: Bearer {salesManager_token}
Content-Type: application/json

{
  "inStock": false
}

# Deactivate product (soft delete)
DELETE /api/Product/1
Authorization: Bearer {salesManager_token}
```

---

## 💻 Frontend Integration Guide

### TypeScript Interfaces

Create these types/interfaces in your Angular/React project:

```typescript
// product.interface.ts
export interface Product {
	id: number;
	name: string;
	model?: string;
	provider?: string;
	country?: string;
	category?: string;
	basePrice: number;
	description?: string;
	imagePath?: string;
	year?: number;
	inStock: boolean;
	isActive: boolean;
	createdBy?: string;
	createdByName?: string;
	createdAt: string;
	updatedAt: string;
}

export interface CreateProductDTO {
	name: string;
	model?: string;
	provider?: string;
	country?: string;
	category?: string;
	basePrice: number;
	description?: string;
	year?: number;
	inStock: boolean;
}

export interface UpdateProductDTO {
	name?: string;
	model?: string;
	provider?: string;
	country?: string;
	category?: string;
	basePrice?: number;
	description?: string;
	year?: number;
	inStock?: boolean;
	isActive?: boolean;
}

export interface ProductSearchParams {
	category?: string;
	inStock?: boolean;
	searchTerm?: string;
}

export interface ApiResponse<T> {
	success: boolean;
	data: T;
	message: string;
	timestamp?: string;
	errors?: any;
}
```

---

### Angular Service Example

```typescript
// product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import {
	Product,
	CreateProductDTO,
	UpdateProductDTO,
	ApiResponse,
} from '../models/product.interface';

@Injectable({
	providedIn: 'root',
})
export class ProductService {
	private readonly apiUrl = '/api/Product';

	constructor(private http: HttpClient) {}

	/**
	 * Get all products with optional filters
	 */
	getAllProducts(params?: {
		category?: string;
		inStock?: boolean;
	}): Observable<Product[]> {
		let httpParams = new HttpParams();

		if (params?.category) {
			httpParams = httpParams.set(
				'category',
				params.category
			);
		}

		if (params?.inStock !== undefined) {
			httpParams = httpParams.set(
				'inStock',
				params.inStock.toString()
			);
		}

		return this.http
			.get<ApiResponse<Product[]>>(this.apiUrl, {
				params: httpParams,
			})
			.pipe(
				map((response) => response.data || []),
				catchError(this.handleError)
			);
	}

	/**
	 * Get product by ID
	 */
	getProductById(id: number): Observable<Product> {
		return this.http
			.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`)
			.pipe(
				map((response) => response.data),
				catchError(this.handleError)
			);
	}

	/**
	 * Get products by category
	 */
	getProductsByCategory(category: string): Observable<Product[]> {
		return this.http
			.get<ApiResponse<Product[]>>(
				`${this.apiUrl}/category/${encodeURIComponent(
					category
				)}`
			)
			.pipe(
				map((response) => response.data || []),
				catchError(this.handleError)
			);
	}

	/**
	 * Search products
	 */
	searchProducts(searchTerm: string): Observable<Product[]> {
		if (!searchTerm || searchTerm.trim() === '') {
			return this.getAllProducts();
		}

		const params = new HttpParams().set('q', searchTerm.trim());

		return this.http
			.get<ApiResponse<Product[]>>(`${this.apiUrl}/search`, {
				params,
			})
			.pipe(
				map((response) => response.data || []),
				catchError(this.handleError)
			);
	}

	/**
	 * Create new product
	 */
	createProduct(product: CreateProductDTO): Observable<Product> {
		return this.http
			.post<ApiResponse<Product>>(this.apiUrl, product)
			.pipe(
				map((response) => response.data),
				catchError(this.handleError)
			);
	}

	/**
	 * Update product
	 */
	updateProduct(
		id: number,
		product: UpdateProductDTO
	): Observable<Product> {
		return this.http
			.put<ApiResponse<Product>>(
				`${this.apiUrl}/${id}`,
				product
			)
			.pipe(
				map((response) => response.data),
				catchError(this.handleError)
			);
	}

	/**
	 * Delete product (soft delete)
	 */
	deleteProduct(id: number): Observable<boolean> {
		return this.http
			.delete<ApiResponse<null>>(`${this.apiUrl}/${id}`)
			.pipe(
				map((response) => response.success),
				catchError(this.handleError)
			);
	}

	/**
	 * Upload product image
	 */
	uploadProductImage(id: number, file: File): Observable<Product> {
		const formData = new FormData();
		formData.append('file', file);

		return this.http
			.post<ApiResponse<Product>>(
				`${this.apiUrl}/${id}/upload-image`,
				formData
			)
			.pipe(
				map((response) => response.data),
				catchError(this.handleError)
			);
	}

	/**
	 * Get image URL (helper method)
	 */
	getImageUrl(imagePath: string | null | undefined): string {
		if (!imagePath) {
			return '/assets/images/product-placeholder.png'; // Fallback image
		}
		// Assuming images are served from /uploads/ or similar
		return `/uploads/${imagePath}`;
	}

	/**
	 * Error handler
	 */
	private handleError(error: any): Observable<never> {
		let errorMessage = 'حدث خطأ غير متوقع';

		if (error.error?.message) {
			errorMessage = error.error.message;
		} else if (error.message) {
			errorMessage = error.message;
		}

		console.error('Product Service Error:', error);
		return throwError(() => new Error(errorMessage));
	}
}
```

---

### Angular Component Examples

#### 1. Product List Component

```typescript
// product-list.component.ts
import { Component, OnInit } from '@angular/core';
import { ProductService } from '../services/product.service';
import { Product } from '../models/product.interface';

@Component({
	selector: 'app-product-list',
	templateUrl: './product-list.component.html',
	styleUrls: ['./product-list.component.css'],
})
export class ProductListComponent implements OnInit {
	products: Product[] = [];
	filteredProducts: Product[] = [];
	loading = false;
	error: string | null = null;

	// Filters
	selectedCategory: string = '';
	inStockFilter: boolean | null = null;
	searchTerm: string = '';

	// Categories dropdown
	categories: string[] = [
		'X-Ray',
		'Ultrasound',
		'CT Scanner',
		'MRI',
		'Other',
	];

	constructor(private productService: ProductService) {}

	ngOnInit(): void {
		this.loadProducts();
	}

	loadProducts(): void {
		this.loading = true;
		this.error = null;

		const params: any = {};
		if (this.selectedCategory) {
			params.category = this.selectedCategory;
		}
		if (this.inStockFilter !== null) {
			params.inStock = this.inStockFilter;
		}

		this.productService.getAllProducts(params).subscribe({
			next: (products) => {
				this.products = products;
				this.filteredProducts = products;
				this.loading = false;
			},
			error: (err) => {
				this.error =
					err.message || 'فشل تحميل المنتجات';
				this.loading = false;
			},
		});
	}

	onCategoryChange(): void {
		this.loadProducts();
	}

	onStockFilterChange(): void {
		this.loadProducts();
	}

	onSearch(): void {
		if (this.searchTerm.trim()) {
			this.loading = true;
			this.productService
				.searchProducts(this.searchTerm)
				.subscribe({
					next: (products) => {
						this.filteredProducts =
							products;
						this.loading = false;
					},
					error: (err) => {
						this.error = err.message;
						this.loading = false;
					},
				});
		} else {
			this.filteredProducts = this.products;
		}
	}

	clearFilters(): void {
		this.selectedCategory = '';
		this.inStockFilter = null;
		this.searchTerm = '';
		this.loadProducts();
	}

	getImageUrl(imagePath: string | null | undefined): string {
		return this.productService.getImageUrl(imagePath);
	}

	formatPrice(price: number): string {
		return new Intl.NumberFormat('ar-EG', {
			style: 'currency',
			currency: 'EGP',
		}).format(price);
	}
}
```

```html
<!-- product-list.component.html -->
<div class="product-list-container">
	<div class="filters-section">
		<div class="filter-group">
			<label>البحث:</label>
			<input
				type="text"
				[(ngModel)]="searchTerm"
				(keyup.enter)="onSearch()"
				placeholder="ابحث عن منتج..."
			/>
			<button (click)="onSearch()">بحث</button>
		</div>

		<div class="filter-group">
			<label>الفئة:</label>
			<select
				[(ngModel)]="selectedCategory"
				(change)="onCategoryChange()"
			>
				<option value="">الكل</option>
				<option
					*ngFor="let cat of categories"
					[value]="cat"
				>
					{{ cat }}
				</option>
			</select>
		</div>

		<div class="filter-group">
			<label>الحالة:</label>
			<select
				[(ngModel)]="inStockFilter"
				(change)="onStockFilterChange()"
			>
				<option [ngValue]="null">الكل</option>
				<option [ngValue]="true">متوفر</option>
				<option [ngValue]="false">غير متوفر</option>
			</select>
		</div>

		<button (click)="clearFilters()">مسح الفلاتر</button>
	</div>

	<div
		*ngIf="loading"
		class="loading"
	>
		جاري التحميل...
	</div>
	<div
		*ngIf="error"
		class="error"
	>
		{{ error }}
	</div>

	<div
		class="products-grid"
		*ngIf="!loading && !error"
	>
		<div
			*ngFor="let product of filteredProducts"
			class="product-card"
		>
			<img
				[src]="getImageUrl(product.imagePath)"
				[alt]="product.name"
				class="product-image"
			/>
			<div class="product-info">
				<h3>{{ product.name }}</h3>
				<p *ngIf="product.model">
					النوع: {{ product.model }}
				</p>
				<p *ngIf="product.provider">
					المورد: {{ product.provider }}
				</p>
				<p *ngIf="product.category">
					الفئة: {{ product.category }}
				</p>
				<p class="price">
					{{ formatPrice(product.basePrice) }}
				</p>
				<span
					[class]="product.inStock ? 'in-stock' : 'out-of-stock'"
				>
					{{ product.inStock ? 'متوفر' : 'غير
					متوفر' }}
				</span>
			</div>
		</div>
	</div>

	<div
		*ngIf="!loading && !error && filteredProducts.length === 0"
		class="empty-state"
	>
		لا توجد منتجات
	</div>
</div>
```

---

#### 2. Product Form Component (Create/Edit)

```typescript
// product-form.component.ts
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../services/product.service';
import {
	Product,
	CreateProductDTO,
	UpdateProductDTO,
} from '../models/product.interface';

@Component({
	selector: 'app-product-form',
	templateUrl: './product-form.component.html',
})
export class ProductFormComponent implements OnInit {
	@Input() product?: Product; // For edit mode
	@Output() saved = new EventEmitter<Product>();
	@Output() cancelled = new EventEmitter<void>();

	productForm: FormGroup;
	loading = false;
	error: string | null = null;
	selectedFile: File | null = null;
	imagePreview: string | null = null;

	categories = ['X-Ray', 'Ultrasound', 'CT Scanner', 'MRI', 'Other'];

	constructor(
		private fb: FormBuilder,
		private productService: ProductService
	) {
		this.productForm = this.fb.group({
			name: [
				'',
				[
					Validators.required,
					Validators.maxLength(200),
				],
			],
			model: ['', Validators.maxLength(100)],
			provider: ['', Validators.maxLength(100)],
			country: ['', Validators.maxLength(100)],
			category: [''],
			basePrice: [
				0,
				[Validators.required, Validators.min(0.01)],
			],
			description: ['', Validators.maxLength(2000)],
			year: [null],
			inStock: [true],
		});
	}

	ngOnInit(): void {
		if (this.product) {
			// Edit mode - populate form
			this.productForm.patchValue({
				name: this.product.name,
				model: this.product.model || '',
				provider: this.product.provider || '',
				country: this.product.country || '',
				category: this.product.category || '',
				basePrice: this.product.basePrice,
				description: this.product.description || '',
				year: this.product.year || null,
				inStock: this.product.inStock,
			});

			if (this.product.imagePath) {
				this.imagePreview =
					this.productService.getImageUrl(
						this.product.imagePath
					);
			}
		}
	}

	onFileSelected(event: Event): void {
		const input = event.target as HTMLInputElement;
		if (input.files && input.files[0]) {
			const file = input.files[0];

			// Validate file type
			const allowedTypes = [
				'image/jpeg',
				'image/jpg',
				'image/png',
				'image/gif',
			];
			if (!allowedTypes.includes(file.type)) {
				this.error =
					'نوع الملف غير مدعوم. يرجى اختيار صورة (JPG, PNG, GIF)';
				return;
			}

			// Validate file size (5MB)
			if (file.size > 5 * 1024 * 1024) {
				this.error = 'حجم الملف أكبر من 5 ميجابايت';
				return;
			}

			this.selectedFile = file;
			this.error = null;

			// Preview image
			const reader = new FileReader();
			reader.onload = (e) => {
				this.imagePreview = e.target?.result as string;
			};
			reader.readAsDataURL(file);
		}
	}

	onSubmit(): void {
		if (this.productForm.invalid) {
			this.markFormGroupTouched(this.productForm);
			return;
		}

		this.loading = true;
		this.error = null;

		const formValue = this.productForm.value;

		if (this.product) {
			// Update existing product
			const updateData: UpdateProductDTO = {
				...formValue,
				year: formValue.year || undefined,
			};

			this.productService
				.updateProduct(this.product.id, updateData)
				.subscribe({
					next: (updatedProduct) => {
						// Upload image if selected
						if (this.selectedFile) {
							this.uploadImage(
								updatedProduct.id
							);
						} else {
							this.loading = false;
							this.saved.emit(
								updatedProduct
							);
						}
					},
					error: (err) => {
						this.error =
							err.message ||
							'فشل تحديث المنتج';
						this.loading = false;
					},
				});
		} else {
			// Create new product
			const createData: CreateProductDTO = {
				...formValue,
				year: formValue.year || undefined,
			};

			this.productService
				.createProduct(createData)
				.subscribe({
					next: (newProduct) => {
						// Upload image if selected
						if (this.selectedFile) {
							this.uploadImage(
								newProduct.id
							);
						} else {
							this.loading = false;
							this.saved.emit(
								newProduct
							);
						}
					},
					error: (err) => {
						this.error =
							err.message ||
							'فشل إنشاء المنتج';
						this.loading = false;
					},
				});
		}
	}

	private uploadImage(productId: number): void {
		if (!this.selectedFile) {
			this.loading = false;
			return;
		}

		this.productService
			.uploadProductImage(productId, this.selectedFile)
			.subscribe({
				next: (updatedProduct) => {
					this.loading = false;
					this.saved.emit(updatedProduct);
				},
				error: (err) => {
					this.error =
						err.message || 'فشل رفع الصورة';
					this.loading = false;
				},
			});
	}

	onCancel(): void {
		this.cancelled.emit();
	}

	markFormGroupTouched(formGroup: FormGroup): void {
		Object.keys(formGroup.controls).forEach((key) => {
			const control = formGroup.get(key);
			control?.markAsTouched();
		});
	}

	isFieldInvalid(fieldName: string): boolean {
		const field = this.productForm.get(fieldName);
		return !!(field && field.invalid && field.touched);
	}
}
```

```html
<!-- product-form.component.html -->
<form
	[formGroup]="productForm"
	(ngSubmit)="onSubmit()"
>
	<div class="form-section">
		<h2>{{ product ? 'تعديل المنتج' : 'إضافة منتج جديد' }}</h2>

		<div
			class="form-group"
			[class.invalid]="isFieldInvalid('name')"
		>
			<label>الاسم *</label>
			<input
				type="text"
				formControlName="name"
			/>
			<span
				class="error"
				*ngIf="isFieldInvalid('name')"
			>
				الاسم مطلوب
			</span>
		</div>

		<div class="form-row">
			<div
				class="form-group"
				[class.invalid]="isFieldInvalid('model')"
			>
				<label>الموديل</label>
				<input
					type="text"
					formControlName="model"
				/>
			</div>

			<div
				class="form-group"
				[class.invalid]="isFieldInvalid('provider')"
			>
				<label>المورد</label>
				<input
					type="text"
					formControlName="provider"
				/>
			</div>
		</div>

		<div class="form-row">
			<div
				class="form-group"
				[class.invalid]="isFieldInvalid('category')"
			>
				<label>الفئة</label>
				<select formControlName="category">
					<option value="">اختر الفئة</option>
					<option
						*ngFor="let cat of categories"
						[value]="cat"
					>
						{{ cat }}
					</option>
				</select>
			</div>

			<div
				class="form-group"
				[class.invalid]="isFieldInvalid('country')"
			>
				<label>البلد</label>
				<input
					type="text"
					formControlName="country"
				/>
			</div>
		</div>

		<div
			class="form-group"
			[class.invalid]="isFieldInvalid('basePrice')"
		>
			<label>السعر الأساسي *</label>
			<input
				type="number"
				formControlName="basePrice"
				step="0.01"
				min="0.01"
			/>
			<span
				class="error"
				*ngIf="isFieldInvalid('basePrice')"
			>
				السعر مطلوب ويجب أن يكون أكبر من 0
			</span>
		</div>

		<div
			class="form-group"
			[class.invalid]="isFieldInvalid('year')"
		>
			<label>السنة</label>
			<input
				type="number"
				formControlName="year"
			/>
		</div>

		<div
			class="form-group"
			[class.invalid]="isFieldInvalid('description')"
		>
			<label>الوصف</label>
			<textarea
				formControlName="description"
				rows="4"
			></textarea>
		</div>

		<div class="form-group">
			<label>
				<input
					type="checkbox"
					formControlName="inStock"
				/>
				متوفر في المخزن
			</label>
		</div>

		<div class="form-group">
			<label>صورة المنتج</label>
			<input
				type="file"
				accept="image/*"
				(change)="onFileSelected($event)"
			/>
			<div
				*ngIf="imagePreview"
				class="image-preview"
			>
				<img
					[src]="imagePreview"
					alt="Preview"
				/>
			</div>
		</div>

		<div
			*ngIf="error"
			class="error-message"
		>
			{{ error }}
		</div>

		<div class="form-actions">
			<button
				type="submit"
				[disabled]="loading || productForm.invalid"
			>
				{{ loading ? 'جاري الحفظ...' : (product ?
				'تحديث' : 'إضافة') }}
			</button>
			<button
				type="button"
				(click)="onCancel()"
			>
				إلغاء
			</button>
		</div>
	</div>
</form>
```

---

#### 3. Product Selection Component (for Offer Creation)

```typescript
// product-selection.component.ts
import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { ProductService } from '../services/product.service';
import { Product } from '../models/product.interface';

@Component({
	selector: 'app-product-selection',
	templateUrl: './product-selection.component.html',
})
export class ProductSelectionComponent implements OnInit {
	@Output() productSelected = new EventEmitter<Product>();

	products: Product[] = [];
	filteredProducts: Product[] = [];
	selectedProducts: Product[] = [];
	loading = false;
	searchTerm = '';
	selectedCategory = '';

	categories = ['X-Ray', 'Ultrasound', 'CT Scanner', 'MRI', 'Other'];

	constructor(private productService: ProductService) {}

	ngOnInit(): void {
		this.loadProducts();
	}

	loadProducts(): void {
		this.loading = true;
		const params = this.selectedCategory
			? { category: this.selectedCategory, inStock: true }
			: { inStock: true };

		this.productService.getAllProducts(params).subscribe({
			next: (products) => {
				this.products = products;
				this.filteredProducts = products;
				this.loading = false;
			},
			error: (err) => {
				console.error('Error loading products:', err);
				this.loading = false;
			},
		});
	}

	onSearch(): void {
		if (this.searchTerm.trim()) {
			this.loading = true;
			this.productService
				.searchProducts(this.searchTerm)
				.subscribe({
					next: (products) => {
						this.filteredProducts =
							products.filter(
								(p) =>
									p.inStock &&
									p.isActive
							);
						this.loading = false;
					},
					error: (err) => {
						console.error(
							'Error searching products:',
							err
						);
						this.loading = false;
					},
				});
		} else {
			this.filteredProducts = this.products;
		}
	}

	onCategoryChange(): void {
		this.loadProducts();
	}

	selectProduct(product: Product): void {
		this.productSelected.emit(product);
		// Optionally close modal/dialog
	}

	isSelected(product: Product): boolean {
		return this.selectedProducts.some((p) => p.id === product.id);
	}

	formatPrice(price: number): string {
		return new Intl.NumberFormat('ar-EG', {
			style: 'currency',
			currency: 'EGP',
		}).format(price);
	}
}
```

```html
<!-- product-selection.component.html -->
<div class="product-selection-modal">
	<div class="modal-header">
		<h3>اختر منتج من الكتالوج</h3>
		<button (click)="close()">✕</button>
	</div>

	<div class="search-section">
		<input
			type="text"
			[(ngModel)]="searchTerm"
			(keyup.enter)="onSearch()"
			placeholder="ابحث عن منتج..."
		/>
		<select
			[(ngModel)]="selectedCategory"
			(change)="onCategoryChange()"
		>
			<option value="">كل الفئات</option>
			<option
				*ngFor="let cat of categories"
				[value]="cat"
			>
				{{ cat }}
			</option>
		</select>
	</div>

	<div
		*ngIf="loading"
		class="loading"
	>
		جاري التحميل...
	</div>

	<div class="products-list">
		<div
			*ngFor="let product of filteredProducts"
			class="product-item"
			[class.selected]="isSelected(product)"
			(click)="selectProduct(product)"
		>
			<img
				[src]="getImageUrl(product.imagePath)"
				[alt]="product.name"
			/>
			<div class="product-details">
				<h4>{{ product.name }}</h4>
				<p *ngIf="product.model">{{ product.model }}</p>
				<p class="price">
					{{ formatPrice(product.basePrice) }}
				</p>
			</div>
		</div>
	</div>
</div>
```

---

#### 4. Integration with Offer Creation Flow

```typescript
// offer-creation.component.ts (Partial)
import { Component } from '@angular/core';
import { ProductService } from '../services/product.service';
import { OfferService } from '../services/offer.service';
import { Product } from '../models/product.interface';

@Component({
	selector: 'app-offer-creation',
	templateUrl: './offer-creation.component.html',
})
export class OfferCreationComponent {
	selectedProducts: Product[] = [];
	customEquipment: any[] = [];
	showProductSelection = false;

	constructor(
		private productService: ProductService,
		private offerService: OfferService
	) {}

	openProductSelection(): void {
		this.showProductSelection = true;
	}

	onProductSelected(product: Product): void {
		// Convert Product to OfferEquipment format
		const equipment = {
			name: product.name,
			model: product.model,
			provider: product.provider,
			country: product.country,
			price: product.basePrice,
			description: product.description,
			imagePath: product.imagePath,
			year: product.year,
			inStock: product.inStock,
		};

		this.selectedProducts.push(product);
		this.customEquipment.push(equipment);
		this.showProductSelection = false;
	}

	removeProduct(product: Product): void {
		this.selectedProducts = this.selectedProducts.filter(
			(p) => p.id !== product.id
		);
		this.customEquipment = this.customEquipment.filter(
			(e) => e.name !== product.name
		);
	}

	createOffer(): void {
		// Create offer with selected products
		const offerData = {
			// ... other offer data
			equipment: this.customEquipment,
		};

		this.offerService.createOfferWithItems(offerData).subscribe({
			next: (offer) => {
				console.log('Offer created:', offer);
				// Navigate or show success message
			},
			error: (err) => {
				console.error('Error creating offer:', err);
			},
		});
	}
}
```

---

### React Example (Alternative)

```typescript
// useProducts.ts (React Hook)
import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';

const API_BASE_URL = '/api/Product';

interface Product {
	id: number;
	name: string;
	model?: string;
	provider?: string;
	category?: string;
	basePrice: number;
	description?: string;
	imagePath?: string;
	year?: number;
	inStock: boolean;
	isActive: boolean;
}

export const useProducts = (filters?: {
	category?: string;
	inStock?: boolean;
}) => {
	const [products, setProducts] = useState<Product[]>([]);
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const fetchProducts = useCallback(async () => {
		setLoading(true);
		setError(null);

		try {
			const params = new URLSearchParams();
			if (filters?.category)
				params.append('category', filters.category);
			if (filters?.inStock !== undefined)
				params.append(
					'inStock',
					filters.inStock.toString()
				);

			const response = await axios.get(
				`${API_BASE_URL}?${params.toString()}`,
				{
					headers: {
						Authorization: `Bearer ${localStorage.getItem(
							'token'
						)}`,
					},
				}
			);

			setProducts(response.data.data || []);
		} catch (err: any) {
			setError(
				err.response?.data?.message ||
					'فشل تحميل المنتجات'
			);
		} finally {
			setLoading(false);
		}
	}, [filters]);

	useEffect(() => {
		fetchProducts();
	}, [fetchProducts]);

	return { products, loading, error, refetch: fetchProducts };
};

export const useProductSearch = (searchTerm: string) => {
	const [products, setProducts] = useState<Product[]>([]);
	const [loading, setLoading] = useState(false);

	useEffect(() => {
		if (!searchTerm.trim()) {
			setProducts([]);
			return;
		}

		const search = async () => {
			setLoading(true);
			try {
				const response = await axios.get(
					`${API_BASE_URL}/search?q=${encodeURIComponent(
						searchTerm
					)}`,
					{
						headers: {
							Authorization: `Bearer ${localStorage.getItem(
								'token'
							)}`,
						},
					}
				);
				setProducts(response.data.data || []);
			} catch (err) {
				console.error('Search error:', err);
				setProducts([]);
			} finally {
				setLoading(false);
			}
		};

		const timeoutId = setTimeout(search, 300); // Debounce
		return () => clearTimeout(timeoutId);
	}, [searchTerm]);

	return { products, loading };
};
```

---

## 🔄 Integration with Offer Creation

When SalesSupport creates an offer, they can:

1. **Select products from catalog:**

      - Use `GET /api/Product` to browse available products
      - Get product details with `GET /api/Product/{id}`
      - Convert `Product` to `OfferEquipment` format when adding to offer

2. **Add custom products:**

      - Create products directly as `OfferEquipment` (existing flow)
      - Or add new products to catalog first, then use them

3. **Mix approach:**
      - Select some products from catalog
      - Add some custom equipment manually

### Recommended Flow:

```typescript
// When creating offer:
1. User clicks "Add Product from Catalog"
2. Open Product Selection Modal/Dialog
3. User searches/browses products
4. User selects product(s)
5. Convert Product to OfferEquipment:
   {
     name: product.name,
     model: product.model,
     provider: product.provider,
     price: product.basePrice, // or allow price override
     imagePath: product.imagePath,
     ...
   }
6. Add to offer's equipment array
7. User can still add custom equipment manually
8. Submit offer with all equipment
```

---

## 🛡️ Error Handling Best Practices

### 1. HTTP Interceptor for Authentication

```typescript
// auth.interceptor.ts
import { Injectable } from '@angular/core';
import {
	HttpInterceptor,
	HttpRequest,
	HttpHandler,
	HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
	constructor(private router: Router) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<any> {
		const token = localStorage.getItem('token');

		if (token) {
			req = req.clone({
				setHeaders: {
					Authorization: `Bearer ${token}`,
				},
			});
		}

		return next.handle(req).pipe(
			catchError((error: HttpErrorResponse) => {
				if (error.status === 401) {
					// Unauthorized - redirect to login
					localStorage.removeItem('token');
					this.router.navigate(['/login']);
				} else if (error.status === 403) {
					// Forbidden - show error message
					alert(
						'ليس لديك صلاحية للقيام بهذا الإجراء'
					);
				}
				return throwError(() => error);
			})
		);
	}
}
```

### 2. Global Error Handler

```typescript
// error-handler.service.ts
import { Injectable, ErrorHandler } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
	handleError(error: any): void {
		if (error instanceof HttpErrorResponse) {
			// Server error
			console.error('Server Error:', error);

			if (error.error?.message) {
				// Show user-friendly message
				console.error(
					'Error Message:',
					error.error.message
				);
			}
		} else {
			// Client error
			console.error('Client Error:', error);
		}
	}
}
```

### 3. Error Handling in Components

```typescript
// Example: Safe error handling pattern
this.productService.getAllProducts().subscribe({
	next: (products) => {
		this.products = products;
		this.loading = false;
		this.error = null;
	},
	error: (err) => {
		// Extract error message
		const errorMessage =
			err.error?.message ||
			err.message ||
			'حدث خطأ غير متوقع';

		this.error = errorMessage;
		this.loading = false;

		// Log for debugging
		console.error('Product loading error:', err);

		// Show user notification (optional)
		// this.notificationService.showError(errorMessage);
	},
});
```

---

## ⚡ Performance Optimization Tips

### 1. Debounce Search Input

```typescript
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

export class ProductListComponent {
	private searchSubject = new Subject<string>();

	ngOnInit(): void {
		// Debounce search input
		this.searchSubject
			.pipe(
				debounceTime(300), // Wait 300ms after user stops typing
				distinctUntilChanged() // Only search if value changed
			)
			.subscribe((searchTerm) => {
				if (searchTerm.trim()) {
					this.performSearch(searchTerm);
				} else {
					this.loadProducts();
				}
			});
	}

	onSearchInput(value: string): void {
		this.searchSubject.next(value);
	}
}
```

### 2. Caching Products

```typescript
// product.service.ts - Add caching
private productsCache: Map<string, { data: Product[]; timestamp: number }> = new Map();
private readonly CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

getAllProducts(params?: { category?: string; inStock?: boolean }): Observable<Product[]> {
  const cacheKey = JSON.stringify(params || {});
  const cached = this.productsCache.get(cacheKey);

  if (cached && Date.now() - cached.timestamp < this.CACHE_DURATION) {
    return of(cached.data);
  }

  // Fetch from API
  return this.http.get<ApiResponse<Product[]>>(this.apiUrl, { params: httpParams })
    .pipe(
      map(response => {
        const products = response.data || [];
        this.productsCache.set(cacheKey, {
          data: products,
          timestamp: Date.now()
        });
        return products;
      }),
      catchError(this.handleError)
    );
}

clearCache(): void {
  this.productsCache.clear();
}
```

### 3. Virtual Scrolling (for large lists)

```html
<!-- Use Angular CDK Virtual Scrolling -->
<cdk-virtual-scroll-viewport
	itemSize="200"
	class="products-viewport"
>
	<div
		*cdkVirtualFor="let product of products"
		class="product-card"
	>
		<!-- Product content -->
	</div>
</cdk-virtual-scroll-viewport>
```

---

## 🎨 UI/UX Recommendations

### 1. Loading States

```typescript
// Show skeleton loaders while loading
// Use Angular Material progress spinner
// Or custom loading component
```

```html
<div
	*ngIf="loading"
	class="skeleton-loader"
>
	<!-- Skeleton cards -->
</div>
```

### 2. Empty States

```html
<div
	*ngIf="!loading && products.length === 0"
	class="empty-state"
>
	<img
		src="/assets/images/empty-products.svg"
		alt="No products"
	/>
	<h3>لا توجد منتجات</h3>
	<p>ابدأ بإضافة منتج جديد إلى الكتالوج</p>
	<button (click)="openAddProductDialog()">إضافة منتج</button>
</div>
```

### 3. Image Lazy Loading

```html
<img
	[src]="getImageUrl(product.imagePath)"
	[alt]="product.name"
	loading="lazy"
	(error)="handleImageError($event)"
/>
```

```typescript
handleImageError(event: Event): void {
  const img = event.target as HTMLImageElement;
  img.src = '/assets/images/product-placeholder.png';
}
```

---

## 🔗 Integration Checklist

### When integrating Products Catalog in Offer Creation:

- [ ] Create `ProductService` with all CRUD methods
- [ ] Create `Product` interface/type matching API response
- [ ] Implement product selection modal/dialog
- [ ] Add "Browse Catalog" button in offer creation form
- [ ] Handle product-to-equipment conversion
- [ ] Allow price override when selecting from catalog
- [ ] Show product images in selection modal
- [ ] Implement search and filter in selection modal
- [ ] Handle loading and error states
- [ ] Cache products for better performance
- [ ] Add validation for required fields
- [ ] Test with empty catalog
- [ ] Test with large number of products
- [ ] Test image upload functionality
- [ ] Test category filtering
- [ ] Test search functionality

---

## 📱 Mobile Responsive Considerations

```typescript
// Check screen size for mobile view
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';

export class ProductListComponent {
	isMobile = false;

	constructor(private breakpointObserver: BreakpointObserver) {
		this.breakpointObserver
			.observe([Breakpoints.Handset])
			.subscribe((result) => {
				this.isMobile = result.matches;
			});
	}
}
```

```html
<!-- Responsive grid -->
<div
	class="products-grid"
	[class.mobile]="isMobile"
>
	<!-- Products -->
</div>
```

```css
.products-grid {
	display: grid;
	grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
	gap: 1rem;
}

.products-grid.mobile {
	grid-template-columns: 1fr;
}
```

---

## 🧪 Testing Examples

### Unit Test Example

```typescript
// product.service.spec.ts
import { TestBed } from '@angular/core/testing';
import {
	HttpClientTestingModule,
	HttpTestingController,
} from '@angular/common/http/testing';
import { ProductService } from './product.service';

describe('ProductService', () => {
	let service: ProductService;
	let httpMock: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule],
			providers: [ProductService],
		});
		service = TestBed.inject(ProductService);
		httpMock = TestBed.inject(HttpTestingController);
	});

	it('should fetch products', () => {
		const mockProducts = [
			{
				id: 1,
				name: 'Test Product',
				basePrice: 1000,
				inStock: true,
				isActive: true,
			},
		];

		service.getAllProducts().subscribe((products) => {
			expect(products.length).toBe(1);
			expect(products[0].name).toBe('Test Product');
		});

		const req = httpMock.expectOne('/api/Product');
		expect(req.request.method).toBe('GET');
		req.flush({ success: true, data: mockProducts });
	});

	afterEach(() => {
		httpMock.verify();
	});
});
```

---

## 📊 Database Indexes

The following indexes are created for performance:

1. **IX_Products_Category_IsActive_InStock** - For filtering by category and status
2. **IX_Products_IsActive** - For filtering active products
3. **IX_Products_Name_Model_Provider** - For search operations

---

## ✅ Status Codes

| Code | Meaning                              |
| ---- | ------------------------------------ |
| 200  | Success                              |
| 201  | Created (for POST)                   |
| 400  | Bad Request (validation errors)      |
| 401  | Unauthorized                         |
| 403  | Forbidden (insufficient permissions) |
| 404  | Not Found                            |
| 500  | Internal Server Error                |

---

## 🎯 Best Practices

1. **Always use categories** - Makes filtering easier
2. **Upload images** - Visual reference helps salesmen
3. **Keep prices updated** - BasePrice should reflect current market price
4. **Use soft delete** - Don't hard delete products, just set IsActive = false
5. **Search before creating** - Check if product already exists before adding new one

---

## 🚀 Quick Test

Test the API with these curl commands (replace `{token}` with actual JWT token):

```bash
# Get all products
curl -X GET "http://localhost:5117/api/Product" \
  -H "Authorization: Bearer {token}"

# Create product
curl -X POST "http://localhost:5117/api/Product" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "category": "X-Ray",
    "basePrice": 100000.00,
    "inStock": true
  }'

# Search products
curl -X GET "http://localhost:5117/api/Product/search?q=test" \
  -H "Authorization: Bearer {token}"
```

---

**Last Updated:** 2025-01-02
**Version:** 1.0.0
