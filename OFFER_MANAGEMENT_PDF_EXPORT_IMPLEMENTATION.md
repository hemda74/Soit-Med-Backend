# Offer Management & PDF Export Implementation

## Overview
This document describes the complete implementation of the offer management system across all three projects (Backend, Dashboard, and Mobile Engineer App), including PDF export functionality.

## Implementation Summary

### 1. Mobile Engineer App (React Native)

#### New Screens Created
1. **OffersListScreen.tsx** (`Soit-Med-Mobile-Engineer/screens/common/OffersListScreen.tsx`)
   - Displays all offers assigned to the engineer/salesman
   - Features:
     - Filter offers by status (All, Draft, Sent, Accepted, Rejected)
     - View offer summary cards with client name, amount, status
     - Pull-to-refresh functionality
     - Navigation to offer details
     - Beautiful UI with status badges and icons

2. **OfferDetailsScreen.tsx** (`Soit-Med-Mobile-Engineer/screens/common/OfferDetailsScreen.tsx`)
   - Shows complete offer information
   - Features:
     - Financial details (Total Amount, Final Price, Payment Type)
     - Important dates (Created, Valid Until, Sent to Client)
     - Equipment list with details (Name, Model, Provider, Country, Price, Stock status)
     - Terms & Conditions (Payment Terms, Delivery Terms)
     - Client Response (if available)
     - **PDF Export** functionality
     - Beautiful card-based layout

#### PDF Export Implementation
- Uses `expo-file-system` to save PDF files
- Uses `expo-sharing` to share/export PDFs
- Downloads PDF from backend API endpoint
- Converts blob to base64 and saves to device
- Allows user to share or save the PDF

#### Navigation Updates
- Added `offers` and `OfferDetails` screen routes to navigation config
- Updated bottom navigation for both Engineer and Salesman roles
- Added "Offers" tab in bottom navigation (replaces "History" in bottom nav for space)

#### Localization
- Added translations in both English and Arabic:
  - `navigation.offers`: "Offers" / "العروض"
  - `offers.title`: "Offers" / "العروض"
  - `offers.offerDetails`: "Offer Details" / "تفاصيل العرض"
  - `offers.noOffers`: "No offers available" / "لا توجد عروض متاحة"
  - `offers.loadingOffers`: "Loading offers..." / "جاري تحميل العروض..."

#### Type Definitions
Updated `types/sales.ts` to include:
- Enhanced `SalesmanOffer` interface with optional fields
- Enhanced `SalesmanOfferDetails` interface
- Updated `OfferEquipmentItem` interface to match backend structure

### 2. Dashboard (React + TypeScript)

#### PDF Export Functionality
- **Already Implemented** in `OfferCreationPage.tsx`
- Export button in the "Finalize Offer" section
- Uses `salesApi.exportOfferPdf()` method

#### API Service Updates
Added new method to `salesApi.ts`:
```typescript
async exportOfferPdf(offerId: string): Promise<Blob>
```
- Fetches PDF from backend endpoint
- Returns blob for download
- Handles authentication via Bearer token

#### Usage
Sales Support users can:
1. Create an offer
2. View offer details
3. Click "Export PDF" button to download offer as PDF
4. Send offer to salesman

### 3. Backend (.NET Core)

#### PDF Export Endpoint
**Endpoint:** `GET /api/Offer/{offerId}/export-pdf`
- **Authorization:** SalesSupport, SalesManager, Salesman
- **Response:** PDF file (application/pdf)
- **Implementation:** Uses `PdfExportService.GenerateOfferPdfAsync()`

#### PDF Generation Service
**File:** `SoitMed/Services/PdfExportService.cs`
- Uses iTextSharp library for PDF generation
- Includes letterhead template
- Generates comprehensive offer document with:
  - Offer header (Client, Dates, Status)
  - Equipment list table
  - Terms and conditions
  - Installment plan (if applicable)
  - Payment summary

#### Offer Retrieval Endpoints

##### For Salesmen
1. **Get Assigned Offers**
   - **Endpoint:** `GET /api/Offer/assigned-to-me`
   - **Authorization:** Salesman role only
   - **Returns:** List of offers assigned to the authenticated salesman

2. **Get Offer Details**
   - **Endpoint:** `GET /api/Offer/{id}`
   - **Authorization:** SalesSupport, SalesManager, SuperAdmin, Salesman
   - **Returns:** Complete offer details including equipment, terms, installments

3. **Get Offer Equipment**
   - **Endpoint:** `GET /api/Offer/{offerId}/equipment`
   - **Authorization:** SalesSupport, SalesManager, Salesman
   - **Returns:** List of equipment items for the offer

##### For Sales Support
1. **Get My Offers**
   - **Endpoint:** `GET /api/Offer/my-offers`
   - **Authorization:** SalesSupport, SalesManager, SuperAdmin
   - **Returns:** Offers created by the authenticated user
   - **Filters:** status, startDate, endDate

2. **Get All Offers**
   - **Endpoint:** `GET /api/Offer`
   - **Authorization:** SalesSupport, SalesManager, SuperAdmin
   - **Returns:** All offers (with optional filters)

#### Authorization Summary
- **PDF Export:** Accessible to SalesSupport, SalesManager, and Salesman
- **View Offers:** Salesman can view offers assigned to them
- **Create/Manage Offers:** SalesSupport and SalesManager only
- **Equipment/Terms:** Accessible to all authorized roles

## How It Works

### Salesman/Engineer Flow
1. Salesman logs into mobile app
2. Navigates to "Offers" tab in bottom navigation
3. Sees list of all offers assigned to them
4. Taps on an offer to view details
5. Reviews offer information (equipment, terms, prices)
6. Taps "Export as PDF" button
7. PDF is downloaded and shared/saved to device

### Sales Support Flow
1. Sales Support logs into dashboard
2. Creates an offer for a client
3. Assigns offer to a salesman
4. Adds equipment, terms, and installment plans
5. Clicks "Export PDF" to download a copy
6. Clicks "Send to Salesman" to notify the salesman
7. Salesman receives the offer in their mobile app

## Technical Details

### Mobile App Dependencies
Required packages (already in package.json):
- `expo-file-system`: For file operations
- `expo-sharing`: For sharing files
- `@react-navigation/native`: For navigation

### Backend Dependencies
Required packages:
- `iTextSharp`: For PDF generation (already included)

### API Integration
The mobile app uses `SalesmanService` to communicate with the backend:
- `listOffersAssignedToMe()`: Fetches assigned offers
- `getOffer()`: Fetches offer details
- `getOfferEquipment()`: Fetches equipment list
- `exportOfferPdf()`: Downloads PDF

## Database Models

### SalesOffer
Main offer model with fields:
- Basic info: ClientId, CreatedBy, AssignedTo
- Financial: TotalAmount, FinalPrice, PaymentType
- Dates: ValidUntil, SentToClientAt, CreatedAt
- Status tracking
- Product details

### OfferEquipment
Equipment items associated with offers:
- Name, Model, Provider, Country
- Price, InStock status
- Description, ImagePath

### OfferTerms
Terms and conditions:
- WarrantyPeriod
- DeliveryTime
- MaintenanceTerms
- OtherTerms

### InstallmentPlan
Payment installments:
- InstallmentNumber
- Amount, DueDate
- Status, Notes

## Testing Checklist

### Mobile App
- [ ] View offers list
- [ ] Filter offers by status
- [ ] View offer details
- [ ] Export PDF (verify PDF is downloadable)
- [ ] Verify all UI elements display correctly
- [ ] Test on both iOS and Android
- [ ] Test in both English and Arabic

### Dashboard
- [ ] Create new offer
- [ ] View offer details
- [ ] Export PDF from offer creation page
- [ ] Verify PDF contains all offer information
- [ ] Send offer to salesman

### Backend
- [ ] Verify `/api/Offer/assigned-to-me` returns correct offers
- [ ] Verify `/api/Offer/{id}` returns complete details
- [ ] Verify `/api/Offer/{id}/export-pdf` generates valid PDF
- [ ] Test authorization (Salesman can only see assigned offers)
- [ ] Verify PDF includes letterhead
- [ ] Test with offers that have equipment, terms, and installments

## Security Considerations

1. **Authorization**: All endpoints require authentication
2. **Role-Based Access**: Salesmen can only access offers assigned to them
3. **Token Security**: Bearer tokens used for API authentication
4. **Data Validation**: Input validation on all endpoints

## Future Enhancements

Potential improvements:
1. Add signature capability to PDFs
2. Email PDF directly to client from mobile app
3. Add offer approval workflow
4. Track PDF download/view history
5. Add offer templates
6. Support for multiple currencies
7. Add client feedback directly in mobile app

## Support & Maintenance

For issues or questions:
1. Check API logs in backend
2. Verify endpoint permissions in `OfferController.cs`
3. Check mobile app console for network errors
4. Verify PDF service configuration in backend

## Conclusion

The complete offer management system is now implemented across all three projects with full PDF export functionality. Salesmen can view and export offers from their mobile app, while Sales Support can manage and export offers from the dashboard.

