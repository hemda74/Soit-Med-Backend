# Products Catalog Seeding Guide

## Overview

This guide explains how to seed the Products catalog table with initial data including product images.

## Files

- **`SeedProductsData.sql`**: SQL script to insert 7 products (4 Portable X-Ray, 3 Mobile X-Ray)

## Products Included

### Portable X-Ray Products (4)

1. **Flat Panel Detector Wireless solutions**

      - Model: Wireless
      - Provider: شركة كوريا
      - Country: كوريا
      - Price: 250,000 EGP
      - Image: `images/pic6.png`

2. **portable X-ray unit + Holder**

      - Model: Portable
      - Provider: شركة الصين
      - Country: الصين
      - Price: 180,000 EGP
      - Image: `images/pic5.png`

3. **High powered portable X-ray unit 60 mA**

      - Model: 60 mA
      - Provider: شركة كوريا
      - Country: كوريا
      - Price: 320,000 EGP
      - Image: `images/Picture4.png`

4. **High powered portable X-ray unit 100 mA**
      - Model: 100 mA
      - Provider: شركة كوريا
      - Country: كوريا
      - Price: 380,000 EGP
      - Image: `images/Picture3.jpg`

### Mobile X-Ray Products (3)

5. **ANALOG MOBILE RADIOGRAPHIC UNIT**

      - Model: ALMA4 - 100 mA
      - Provider: شركة ايطاليا
      - Country: ايطاليا
      - Price: 450,000 EGP
      - Image: `images/Picture2.png`

6. **ANALOG MOBILE RADIOGRAPHIC**

      - Model: MAC 32 KW - 400 mA
      - Provider: شركة ايطاليا
      - Country: ايطاليا
      - Price: 550,000 EGP
      - Image: `images/Picture1.jpg`

7. **Analog Mobile Radiographic unit**
      - Model: TMS 320 - 32kW - 400mA
      - Provider: شركة ايطاليا
      - Country: ايطاليا
      - Price: 580,000 EGP
      - Image: `images/pic6.png`

## How to Run

### Step 1: Ensure Images Exist

Make sure the following images are in `/wwwroot/images/`:

- `pic6.png`
- `pic5.png`
- `Picture4.png`
- `Picture3.jpg`
- `Picture2.png`
- `Picture1.jpg`

### Step 2: Run SQL Script

**Option A: Using SQL Server Management Studio (SSMS)**

1. Open SQL Server Management Studio
2. Connect to your database server
3. Open `SeedProductsData.sql`
4. **Important**: Update the database name on line 20:
      ```sql
      USE [ITIWebApi44]; -- Change to your database name
      ```
5. Execute the script (F5 or Execute button)

**Option B: Using Command Line**

```powershell
# Using sqlcmd
sqlcmd -S "DESKTOP-DTSVDRG\SQLEXPRESS" -d "ITIWebApi44" -i "SoitMed\Scripts\SeedProductsData.sql"
```

### Step 3: Verify

After running the script, verify the data:

```sql
-- Check total products
SELECT COUNT(*) AS TotalProducts FROM Products WHERE IsActive = 1;

-- Check by category
SELECT Category, COUNT(*) AS Count
FROM Products
WHERE IsActive = 1
GROUP BY Category;

-- View all products
SELECT Id, Name, Model, Category, Provider, Country, BasePrice, ImagePath
FROM Products
WHERE IsActive = 1
ORDER BY Category, Name;
```

## Image Access

Images are served via static files middleware. Once seeded, images can be accessed at:

```
http://localhost:5117/images/pic6.png
http://localhost:5117/images/pic5.png
http://localhost:5117/images/Picture4.png
http://localhost:5117/images/Picture3.jpg
http://localhost:5117/images/Picture2.png
http://localhost:5117/images/Picture1.jpg
```

## Features

- **Idempotent**: Script can be run multiple times safely (won't create duplicates)
- **Safe**: Uses `IF NOT EXISTS` to prevent duplicate inserts
- **Complete**: Includes all product fields (name, model, provider, country, category, price, description, image, year)
- **Summary**: Displays summary statistics after seeding

## Expected Output

When you run the script, you should see:

```
============================================================================
Seeding Products Catalog Data
============================================================================

✓ Inserted: Flat Panel Detector Wireless solutions
✓ Inserted: portable X-ray unit + Holder
✓ Inserted: High powered portable X-ray unit 60 mA
✓ Inserted: High powered portable X-ray unit 100 mA
✓ Inserted: ANALOG MOBILE RADIOGRAPHIC UNIT - ALMA4
✓ Inserted: ANALOG MOBILE RADIOGRAPHIC - MAC 32 KW
✓ Inserted: Analog Mobile Radiographic unit - TMS 320

============================================================================
Seeding Summary
============================================================================
Total Active Products: 7
Portable X-Ray Products: 4
Mobile X-Ray Products: 3

============================================================================
Sample Products List:
============================================================================
[Results showing all products]

============================================================================
Seeding completed successfully!
============================================================================
```

## Notes

- Images must already exist in `/wwwroot/images/` before running
- Product names are in Arabic/English mix as provided
- Prices are in EGP (Egyptian Pounds)
- All products are set as `InStock = 1` and `IsActive = 1`
- Year is set to 2024 for all products

## Troubleshooting

### Error: "Invalid object name 'Products'"

- **Cause**: Table doesn't exist
- **Solution**: Run `CreateProductCatalogTable.sql` first

### Error: "Invalid column name"

- **Cause**: Table structure doesn't match
- **Solution**: Verify table structure matches the script expectations

### Images not displaying

- **Cause**: Images not in correct location or static files not configured
- **Solution**:
     1. Verify images are in `/wwwroot/images/`
     2. Check `app.UseStaticFiles()` is in `Program.cs`
     3. Verify image paths in database match actual filenames

### Duplicate key error

- **Cause**: Products already exist
- **Solution**: This shouldn't happen due to `IF NOT EXISTS`, but you can delete existing products first if needed

---

**Last Updated:** 2025-01-02


