# Fix Categories Not Showing Issue

## Problem
1. **Backend returns 0 categories** - API shows "Total categories in database: 0"
2. **Products have CategoryId = NULL** - Migration script hasn't been run yet
3. **Frontend shows empty categories** - No categories appear in home screen

## Solution Steps

### Step 1: Check ProductCategories Table State
Run this in SSMS:
```sql
USE [ITIWebApi44];
SELECT COUNT(*) AS TotalCategories FROM ProductCategories;
SELECT COUNT(*) AS ActiveCategories FROM ProductCategories WHERE IsActive = 1;
SELECT COUNT(*) AS MainActiveCategories FROM ProductCategories WHERE IsActive = 1 AND ParentCategoryId IS NULL;
```

**Expected Result:** Should show categories exist. If all are 0, the table might be empty.

### Step 2: Fix Categories (If Needed)
Run: `URGENT_FIX_CATEGORIES.sql` in SSMS
- This ensures main categories are active and have no parent
- Commits the changes

### Step 3: Run Product Migration
Run: `migrate_products_to_categoryid_SSMS.sql` in SSMS
- This maps Products.Category (text) to Products.CategoryId (foreign key)
- **IMPORTANT:** Review results before committing!

### Step 4: Verify
After running both scripts, check:
```sql
-- Should return > 0
SELECT COUNT(*) FROM ProductCategories WHERE IsActive = 1 AND ParentCategoryId IS NULL;

-- Should return > 0 (products with CategoryId)
SELECT COUNT(*) FROM Products WHERE CategoryId IS NOT NULL;
```

## Files to Run (in order):
1. `check_categories_state.sql` - Diagnostic (optional)
2. `URGENT_FIX_CATEGORIES.sql` - Fix categories (if needed)
3. `migrate_products_to_categoryid_SSMS.sql` - Migrate products (REQUIRED)

