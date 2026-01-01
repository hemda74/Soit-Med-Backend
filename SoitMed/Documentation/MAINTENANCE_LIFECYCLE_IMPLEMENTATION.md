# Maintenance Request & Lifecycle Module - Implementation Summary

## Overview
This document outlines the complete implementation of the Maintenance Request & Lifecycle module, including database schema, backend APIs, payment processing, and frontend integration guidelines.

## Database Schema

### Updated Entities

#### 1. MaintenanceRequest
- **New Fields for Future Installments** (Reserved, not implemented):
  - `PaymentPlan?` (enum: OneTime, Installment)
  - `InstallmentMonths?` (int)
  - `CollectionDelegateId?` (FK to ApplicationUser)
  - `CollectionDelegate` (Navigation property)
- **New Fields for Cost Calculation**:
  - `LaborFees` (decimal) - Service fees charged by engineer

#### 2. SparePartRequest
- **New Fields for Warehouse Approval**:
  - `ApprovedByWarehouseKeeperId` (FK to ApplicationUser)
  - `ApprovedByWarehouseKeeper` (Navigation property)
  - `WarehouseApprovedAt` (DateTime?)
  - `WarehouseRejectionReason` (string?)
  - `WarehouseApproved` (bool?) - null = pending, true = approved, false = rejected

#### 3. Invoice (New Entity)
- `InvoiceNumber` (string, unique)
- `MaintenanceRequestId` (FK)
- `LaborFees` (decimal)
- `SparePartsTotal` (decimal)
- `TotalAmount` (decimal)
- `PaidAmount` (decimal)
- `RemainingAmount` (decimal)
- `Status` (PaymentStatus enum)
- `Method` (PaymentMethod enum)
- **Future-proofing fields** (Reserved):
  - `PaymentPlan?`
  - `InstallmentMonths?`
  - `CollectionDelegateId?`

### Enums

#### PaymentMethod (Updated)
- Added `Installment = 14` (reserved for future)

#### PaymentPlan (New)
- `OneTime = 1`
- `Installment = 2` (reserved for future)

## Backend API Endpoints

### 1. Create Maintenance Request
**Endpoint:** `POST /api/MaintenanceRequest`
**Roles:** Doctor, Technician, Manager
**Description:** Customer or Call Center creates a maintenance ticket

### 2. Assign Engineer
**Endpoint:** `POST /api/MaintenanceRequest/{id}/assign`
**Roles:** Admin, MaintenanceSupport, MaintenanceManager
**Description:** Admin/Coordinator assigns an engineer to the request

### 3. Request Spare Parts (Engineer)
**Endpoint:** `POST /api/SparePartRequest?maintenanceVisitId={visitId}`
**Roles:** Engineer
**Description:** Engineer requests spare parts during diagnosis

### 4. Warehouse Approval/Rejection
**Endpoint:** `POST /api/SparePartRequest/{id}/warehouse-approval`
**Roles:** WarehouseKeeper
**Request Body:**
```json
{
  "approved": true,
  "rejectionReason": "string (optional)"
}
```
**Description:** Warehouse Keeper approves or rejects spare part requests in the Dashboard

### 5. Finalize Job and Process Payment
**Endpoint:** `POST /api/MaintenanceRequest/{id}/finalize-job`
**Roles:** Engineer
**Request Body:**
```json
{
  "maintenanceRequestId": 123,
  "paymentMethod": "Cash" | "CreditCard" | "Paymob" | "Gateway",
  "laborFees": 500.00,
  "notes": "Job completed successfully",
  "paymentToken": "string (for gateway payments)",
  "additionalPaymentData": {}
}
```
**Description:** 
- Calculates total cost (Labor Fees + Approved Spare Parts)
- Creates invoice
- Processes payment using Strategy Pattern
- Closes the ticket

## Payment Strategy Pattern

### Architecture
The payment processing uses the **Strategy Pattern** to allow easy extension for installment payments in the future.

### Implemented Strategies

1. **CashPaymentStrategy**
   - Handles `PaymentMethod.Cash`
   - Marks payment as `Pending` (requires manual confirmation from Accounts)

2. **VisaPaymentStrategy**
   - Handles `PaymentMethod.CreditCard`, `DebitCard`, `Paymob`, `Gateway`
   - Integrates with payment gateways (Paymob, Stripe, etc.)
   - Returns redirect URL for 3D Secure if needed

3. **InstallmentPaymentStrategy** (Reserved)
   - Handles `PaymentMethod.Installment`
   - **NOT IMPLEMENTED** - Returns error message
   - Reserved for future development

### PaymentStrategyFactory
- Automatically selects the appropriate strategy based on payment method
- Registered in DI container

## Frontend Integration Guidelines

### React Dashboard (Web)

#### State Management
```typescript
// Use React Query for data fetching
const { data: sparePartRequests } = useQuery({
  queryKey: ['sparePartRequests', 'pending'],
  queryFn: () => fetchPendingSparePartRequests()
});

// Warehouse Approval Mutation
const approveMutation = useMutation({
  mutationFn: (data: { id: number, approved: boolean, reason?: string }) =>
    api.post(`/api/SparePartRequest/${data.id}/warehouse-approval`, data),
  onSuccess: () => {
    queryClient.invalidateQueries(['sparePartRequests']);
  }
});
```

#### Components Needed
1. **SparePartApprovalQueue** - Lists pending spare part requests
2. **WarehouseApprovalCard** - Shows part details, pricing, approve/reject actions
3. **PaymentProcessingModal** - For viewing payment status

### React Native (Mobile App)

#### State Management
```typescript
// Engineer: Request Spare Parts
const requestPartMutation = useMutation({
  mutationFn: (data: CreateSparePartRequestDTO) =>
    api.post('/api/SparePartRequest', data, {
      params: { maintenanceVisitId: visitId }
    })
});

// Engineer: Finalize Job
const finalizeJobMutation = useMutation({
  mutationFn: (data: FinalizeJobDTO) =>
    api.post(`/api/MaintenanceRequest/${requestId}/finalize-job`, data),
  onSuccess: (response) => {
    // Navigate to payment screen or success screen
    navigation.navigate('PaymentSuccess');
  }
});
```

#### Screens Needed
1. **RequestSparePartsScreen** - Engineer requests parts during diagnosis
2. **FinalizeJobScreen** - Engineer enters labor fees, selects payment method
3. **PaymentScreen** - Handles Cash or Gateway payment
4. **JobCompletedScreen** - Success confirmation

#### Payment Method Selection
- **Current Implementation:** Only show `Cash` and `CreditCard`/`Gateway` options
- **Future:** When installments are implemented, add `Installment` option with month selector

## Workflow Summary

1. **Request Creation** → Customer/Call Center creates ticket
2. **Assignment** → Admin assigns Engineer
3. **QR Verification** → Engineer scans QR code to verify arrival
4. **Diagnosis** → Engineer diagnoses issue
5. **Spare Parts Request** → Engineer requests parts if needed
6. **Warehouse Approval** → Warehouse Keeper approves/rejects in Dashboard
7. **Customer Approval** → Customer approves part pricing (if required)
8. **Cost Calculation** → System calculates: Labor Fees + Approved Parts
9. **Payment Processing** → Engineer collects payment (Cash or Gateway)
10. **Closure** → Ticket marked as completed

## Future Installment Support

### Database Fields (Already Added)
- `MaintenanceRequest.PaymentPlan`
- `MaintenanceRequest.InstallmentMonths`
- `MaintenanceRequest.CollectionDelegateId`
- `Invoice.PaymentPlan`
- `Invoice.InstallmentMonths`
- `Invoice.CollectionDelegateId`

### Code Structure (Ready)
- `InstallmentPaymentStrategy` class exists (returns error)
- `PaymentMethod.Installment` enum value exists
- Strategy Pattern allows easy integration

### To Implement Installments (Future)
1. Implement `InstallmentPaymentStrategy.ProcessPaymentAsync()`
2. Create `InstallmentSchedule` entity
3. Add monthly payment tracking
4. Add collection delegate assignment logic
5. Add reminder notifications
6. Update UI to show installment option

## Testing Checklist

- [ ] Create maintenance request
- [ ] Assign engineer
- [ ] Request spare parts
- [ ] Warehouse approves parts
- [ ] Warehouse rejects parts
- [ ] Customer approves parts
- [ ] Finalize job with Cash payment
- [ ] Finalize job with Gateway payment
- [ ] Verify invoice creation
- [ ] Verify payment transaction recording
- [ ] Verify cost calculation (Labor + Parts)

## Notes

- All installment-related fields are nullable and unused
- Payment UI should only show Cash and Visa/Gateway options
- InstallmentPaymentStrategy returns error if called (by design)
- Warehouse approval is required before engineer can proceed
- Cost calculation includes only approved spare parts

