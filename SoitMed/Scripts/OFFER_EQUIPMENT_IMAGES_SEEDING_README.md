# Offer Equipment Images Seeding Guide

## Overview

This guide explains how to seed offer equipment with image paths for Sales Support offers.

## Changes Made

1. **Image Path Structure**: Images are now saved to `/wwwroot/offers/{offerId}/` instead of the previous path
2. **Image Naming**: Images are named as `equipment-{equipmentId}-{guid}.jpg`
3. **Auto-Update**: When uploading images via the API, the equipment's `ImagePath` is automatically updated in the database

## Database Seeding

### Option 1: SQL Script (Recommended for Existing Data)

Run the SQL script `SeedOfferEquipmentImages.sql` to update existing offer equipment with image paths:

```sql
-- The script will:
-- 1. Update all OfferEquipment records with NULL ImagePath
-- 2. Set ImagePath in format: offers/{offerId}/equipment-{equipmentId}-{guid}.jpg
-- 3. Only update equipment that belongs to existing offers
```

**Steps:**

1. Open SQL Server Management Studio
2. Connect to your database
3. Open `SeedOfferEquipmentImages.sql`
4. Update the database name in the script (or comment out the USE statement)
5. Execute the script

### Option 2: Manual Update via Entity Framework

If you have seeded offers, you can manually set image paths:

```csharp
var equipment = await context.OfferEquipment
    .FirstOrDefaultAsync(e => e.OfferId == offerId && e.Id == equipmentId);

if (equipment != null)
{
    equipment.ImagePath = $"offers/{offerId}/equipment-{equipmentId}-{Guid.NewGuid()}.jpg";
    await context.SaveChangesAsync();
}
```

## Adding Actual Image Files

After running the SQL script, you need to add actual image files:

1. **Directory Structure**: Create directories in `/wwwroot/offers/{offerId}/` for each offer
2. **Image Files**: Place image files matching the naming pattern:
      - `equipment-{equipmentId}-{guid}.jpg`
      - Example: `equipment-1-a1b2c3d4-e5f6-7890.jpg`

### Sample Images Directory Structure:

```
wwwroot/
  offers/
    1/
      equipment-1-a1b2c3d4-e5f6-7890.jpg
      equipment-2-b2c3d4e5-f6g7-8901.jpg
    2/
      equipment-3-c3d4e5f6-g7h8-9012.jpg
```

## API Usage

### Upload Image for Equipment

```http
POST /api/Offer/{offerId}/equipment/{equipmentId}/upload-image
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [image file]
```

**Response:**

```json
{
	"success": true,
	"data": {
		"id": 1,
		"offerId": 1,
		"name": "X-Ray Machine",
		"model": "Model XYZ",
		"imagePath": "offers/1/equipment-1-a1b2c3d4-e5f6-7890.jpg",
		"price": 50000.0
	},
	"message": "Image uploaded and equipment updated successfully"
}
```

## Seeding Sample Data with Images

If you want to seed offers with actual images during initialization:

### Example C# Seeding Code:

```csharp
// In your DbInitializer or Seed method
var offer = await context.SalesOffers.FirstOrDefaultAsync(o => o.Id == 1);
if (offer != null)
{
    var equipment = new OfferEquipment
    {
        OfferId = offer.Id,
        Name = "X-Ray Machine",
        Model = "Model XYZ",
        Provider = "MedicalTech Inc",
        Country = "Germany",
        Price = 50000.0,
        Description = "High-quality digital X-Ray machine",
        InStock = true,
        ImagePath = "offers/1/equipment-1-seed.jpg" // Set image path
    };

    await context.OfferEquipment.AddAsync(equipment);
    await context.SaveChangesAsync();

    // Then copy actual image file to: wwwroot/offers/1/equipment-1-seed.jpg
}
```

## Image Path Format

- **Pattern**: `offers/{offerId}/equipment-{equipmentId}-{guid}.{extension}`
- **Example**: `offers/5/equipment-12-a1b2c3d4-e5f6-7890-abcd.jpg`
- **Full Path**: `/wwwroot/offers/{offerId}/equipment-{equipmentId}-{guid}.jpg`

## Notes

- Images are stored in `/wwwroot/offers/{offerId}/` directory
- Each equipment in an offer can have one image
- Image path is automatically updated in database after upload
- Image files must be manually placed in the correct directory after SQL seeding
- Supported formats: JPG, JPEG, PNG, GIF (max 5MB)

## Troubleshooting

### Images not showing?

1. Check that image files exist in `/wwwroot/offers/{offerId}/`
2. Verify ImagePath in database matches actual file location
3. Ensure file names match exactly (case-sensitive on Linux)
4. Check file permissions

### Upload fails?

1. Verify offer and equipment exist
2. Check file size (max 5MB)
3. Ensure file format is supported (JPG, JPEG, PNG, GIF)
4. Check directory permissions for `/wwwroot/offers/`

