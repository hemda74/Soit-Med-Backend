# Products Catalog API Testing Guide

This guide provides instructions for testing all Products Catalog API endpoints.

---

## 📋 Prerequisites

1. **API Running**: Ensure your API is running on `http://localhost:5117`
2. **Valid Token**: You need a JWT token from a user with one of these roles:
      - `SalesSupport` (can create, update, view)
      - `SalesManager` (full access including delete)
      - `SuperAdmin` (full access)
      - `Salesman` (read-only access)

---

## 🔐 Getting a Token

### Option 1: Login via API

```powershell
# Login and get token
$loginBody = @{
    UserName = "ahmed@soitmed.com"  # Your username/email
    Password = "YourPassword"         # Your password
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5117/api/Account/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json"

$TOKEN = $response.token
Write-Host "Token: $TOKEN"
```

### Option 2: Use Existing Token

If you already have a valid token, use it directly:

```powershell
$TOKEN = "your-jwt-token-here"
```

---

## 🧪 Running the Test Script

### Step 1: Open PowerShell

```powershell
cd D:\Soit-Med\Soit-Med-Backend
```

### Step 2: Edit the Script (if needed)

Open `test_product_endpoints.ps1` and either:

- Set `$TOKEN = "your-token-here"` at the top, OR
- The script will prompt you to login

### Step 3: Run the Script

```powershell
.\test_product_endpoints.ps1
```

---

## 📡 Manual Testing with PowerShell

### 1. Get All Products

```powershell
$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "application/json"
}

Invoke-RestMethod -Uri "http://localhost:5117/api/Product" `
    -Method GET `
    -Headers $headers | ConvertTo-Json -Depth 5
```

### 2. Get Products with Filters

```powershell
Invoke-RestMethod -Uri "http://localhost:5117/api/Product?category=X-Ray&inStock=true" `
    -Method GET `
    -Headers $headers | ConvertTo-Json -Depth 5
```

### 3. Search Products

```powershell
Invoke-RestMethod -Uri "http://localhost:5117/api/Product/search?q=ultrasound" `
    -Method GET `
    -Headers $headers | ConvertTo-Json -Depth 5
```

### 4. Get Products by Category

```powershell
Invoke-RestMethod -Uri "http://localhost:5117/api/Product/category/Ultrasound" `
    -Method GET `
    -Headers $headers | ConvertTo-Json -Depth 5
```

### 5. Create Product

```powershell
$productData = @{
    name = "جهاز أشعة رقمي"
    model = "XR-200"
    provider = "MedTech"
    country = "ألمانيا"
    category = "X-Ray"
    basePrice = 350000.00
    description = "جهاز أشعة رقمي عالي الجودة"
    year = 2024
    inStock = $true
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5117/api/Product" `
    -Method POST `
    -Headers $headers `
    -Body $productData

$productId = $response.data.id
Write-Host "Created product with ID: $productId"
```

### 6. Get Product by ID

```powershell
$productId = 1  # Replace with actual ID

Invoke-RestMethod -Uri "http://localhost:5117/api/Product/$productId" `
    -Method GET `
    -Headers $headers | ConvertTo-Json -Depth 5
```

### 7. Update Product

```powershell
$productId = 1  # Replace with actual ID

$updateData = @{
    name = "جهاز أشعة رقمي - محدث"
    basePrice = 375000.00
    inStock = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5117/api/Product/$productId" `
    -Method PUT `
    -Headers $headers `
    -Body $updateData | ConvertTo-Json -Depth 5
```

### 8. Upload Product Image

```powershell
$productId = 1  # Replace with actual ID
$imagePath = "C:\path\to\image.jpg"

# Create multipart form data
$boundary = [System.Guid]::NewGuid().ToString()
$fileBytes = [System.IO.File]::ReadAllBytes($imagePath)
$fileName = Split-Path $imagePath -Leaf

$bodyLines = (
    "--$boundary",
    "Content-Disposition: form-data; name=`"file`"; filename=`"$fileName`"",
    "Content-Type: image/jpeg",
    "",
    [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($fileBytes),
    "--$boundary--"
) -join "`r`n"

$headers = @{
    "Authorization" = "Bearer $TOKEN"
    "Content-Type" = "multipart/form-data; boundary=$boundary"
}

Invoke-RestMethod -Uri "http://localhost:5117/api/Product/$productId/upload-image" `
    -Method POST `
    -Headers $headers `
    -Body ([System.Text.Encoding]::GetEncoding('iso-8859-1').GetBytes($bodyLines)) | ConvertTo-Json
```

### 9. Delete Product (SalesManager/SuperAdmin only)

```powershell
$productId = 1  # Replace with actual ID

Invoke-RestMethod -Uri "http://localhost:5117/api/Product/$productId" `
    -Method DELETE `
    -Headers $headers | ConvertTo-Json
```

---

## 🧪 Testing with Postman

### Import Collection

1. Open Postman
2. Create new Collection: "Products Catalog API"
3. Set Collection Variable:
      - `base_url`: `http://localhost:5117`
      - `token`: `your-jwt-token-here`

### Request Examples

#### GET All Products

```
GET {{base_url}}/api/Product
Authorization: Bearer {{token}}
```

#### GET with Filters

```
GET {{base_url}}/api/Product?category=X-Ray&inStock=true
Authorization: Bearer {{token}}
```

#### Search Products

```
GET {{base_url}}/api/Product/search?q=ultrasound
Authorization: Bearer {{token}}
```

#### Create Product

```
POST {{base_url}}/api/Product
Authorization: Bearer {{token}}
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

#### Update Product

```
PUT {{base_url}}/api/Product/1
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "basePrice": 400000.00,
  "inStock": false
}
```

#### Upload Image

```
POST {{base_url}}/api/Product/1/upload-image
Authorization: Bearer {{token}}
Content-Type: multipart/form-data

file: [Select File]
```

#### Delete Product

```
DELETE {{base_url}}/api/Product/1
Authorization: Bearer {{token}}
```

---

## 🧪 Testing with cURL

### Get All Products

```bash
curl -X GET "http://localhost:5117/api/Product" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json"
```

### Create Product

```bash
curl -X POST "http://localhost:5117/api/Product" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "جهاز أشعة رقمي",
    "category": "X-Ray",
    "basePrice": 350000.00,
    "inStock": true
  }'
```

### Search Products

```bash
curl -X GET "http://localhost:5117/api/Product/search?q=ultrasound" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Update Product

```bash
curl -X PUT "http://localhost:5117/api/Product/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "basePrice": 400000.00
  }'
```

### Upload Image

```bash
curl -X POST "http://localhost:5117/api/Product/1/upload-image" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -F "file=@/path/to/image.jpg"
```

### Delete Product

```bash
curl -X DELETE "http://localhost:5117/api/Product/1" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## ✅ Expected Results

### Successful Response Format

```json
{
	"success": true,
	"data": {
		// Product data or array of products
	},
	"message": "Operation completed successfully",
	"timestamp": "2025-01-02T10:00:00Z"
}
```

### Error Response Format

```json
{
	"success": false,
	"message": "Error message here",
	"errors": {
		// Validation errors if any
	},
	"timestamp": "2025-01-02T10:00:00Z"
}
```

---

## 🔍 Testing Checklist

- [ ] Get all products (no filters)
- [ ] Get products with category filter
- [ ] Get products with inStock filter
- [ ] Get products with both filters
- [ ] Search products by name
- [ ] Search products by model
- [ ] Search products by provider
- [ ] Get products by category endpoint
- [ ] Create product with all fields
- [ ] Create product with minimal fields
- [ ] Get product by ID
- [ ] Update product (partial update)
- [ ] Update product (full update)
- [ ] Upload product image
- [ ] Delete product (requires SalesManager/SuperAdmin)
- [ ] Test error: Get non-existent product (404)
- [ ] Test error: Create with invalid data (400)
- [ ] Test error: Unauthorized access (401)
- [ ] Test error: Forbidden access (403)

---

## 🐛 Troubleshooting

### Error: "401 Unauthorized"

- **Cause**: Invalid or missing token
- **Solution**: Login again to get a fresh token

### Error: "403 Forbidden"

- **Cause**: User role doesn't have permission
- **Solution**: Use a token from SalesSupport, SalesManager, or SuperAdmin

### Error: "404 Not Found"

- **Cause**: Product ID doesn't exist
- **Solution**: Use a valid product ID from the GET all products response

### Error: "400 Bad Request"

- **Cause**: Validation errors in request data
- **Solution**: Check error message for validation details

### Error: Connection Failed

- **Cause**: API is not running
- **Solution**: Start the API server and ensure it's running on port 5117

---

## 📝 Notes

- The test script creates a test product that remains in the database
- You may want to delete test products manually after testing
- Image upload requires actual image files (JPG, PNG, GIF, max 5MB)
- All timestamps are in UTC
- Prices are in EGP (Egyptian Pounds)

---

**Last Updated:** 2025-01-02


