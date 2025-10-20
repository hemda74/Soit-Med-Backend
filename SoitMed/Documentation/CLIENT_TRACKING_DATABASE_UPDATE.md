# Client Tracking Database Update

## Overview

This document outlines the database changes made to support the new Client Tracking and Weekly Planning system.

## New Tables Added

### 1. Clients Table

Stores client information including contact details, status, and priority.

**Key Fields:**

- `Id` (Primary Key)
- `Name` - Client name
- `Type` - Client type (Doctor, Hospital, Clinic, etc.)
- `Specialization` - Client specialization
- `Location` - Client location
- `Phone`, `Email` - Contact information
- `Status` - Client status (Potential, Active, Inactive, Lost)
- `Priority` - Client priority (Low, Medium, High)
- `PotentialValue` - Estimated potential value
- `ContactPerson` - Primary contact person details
- `LastContactDate`, `NextContactDate` - Contact tracking
- `SatisfactionRating` - Client satisfaction (1-5 scale)
- `CreatedBy`, `AssignedTo` - User tracking

### 2. ClientVisits Table

Tracks all client visits including purpose, results, and follow-up.

**Key Fields:**

- `Id` (Primary Key)
- `ClientId` (Foreign Key to Clients)
- `VisitDate` - Date of visit
- `VisitType` - Type of visit (Initial, Follow-up, Maintenance, Support)
- `Location` - Visit location
- `Purpose` - Visit purpose
- `Attendees` - JSON array of attendee names
- `Notes`, `Results` - Visit details
- `NextVisitDate` - Scheduled next visit
- `Status` - Visit status (Completed, Scheduled, Cancelled)
- `SalesmanId` - Assigned salesman
- `Attachments` - JSON array of file paths

### 3. ClientInteractions Table

Tracks all client interactions including calls, emails, and meetings.

**Key Fields:**

- `Id` (Primary Key)
- `ClientId` (Foreign Key to Clients)
- `InteractionDate` - Date of interaction
- `InteractionType` - Type (Call, Email, Meeting, Visit)
- `Subject` - Interaction subject
- `Description` - Interaction details
- `Participants` - JSON array of participant names
- `Outcome` - Interaction outcome
- `FollowUpRequired` - Boolean flag
- `FollowUpDate` - Scheduled follow-up date
- `Priority` - Interaction priority
- `Status` - Interaction status (Open, Closed, Pending)

### 4. ClientAnalytics Table

Stores calculated analytics and metrics for clients.

**Key Fields:**

- `Id` (Primary Key)
- `ClientId` (Foreign Key to Clients)
- `Period` - Analytics period (daily, weekly, monthly, yearly)
- `PeriodStart`, `PeriodEnd` - Period boundaries
- `TotalVisits`, `TotalInteractions`, `TotalSales` - Counts
- `AverageVisitDuration` - Average duration
- `LastVisitDate`, `NextScheduledVisit` - Visit tracking
- `ClientSatisfactionScore` - Satisfaction rating
- `ConversionRate` - Conversion percentage
- `Revenue` - Revenue generated
- `GrowthRate` - Growth percentage
- `TopProducts` - JSON array of top products
- `KeyMetrics` - JSON object with additional metrics

### 5. WeeklyPlans Table

Stores weekly planning information for employees.

**Key Fields:**

- `Id` (Primary Key)
- `EmployeeId` - Employee who created the plan
- `WeekStartDate`, `WeekEndDate` - Week boundaries
- `PlanTitle`, `PlanDescription` - Plan details
- `Status` - Plan status (Draft, Submitted, Approved, Rejected)
- `ApprovalNotes`, `RejectionReason` - Approval details
- `SubmittedAt`, `ApprovedAt`, `RejectedAt` - Timestamps
- `ApprovedBy`, `RejectedBy` - User tracking

### 6. WeeklyPlanItems Table

Stores individual items within weekly plans.

**Key Fields:**

- `Id` (Primary Key)
- `WeeklyPlanId` (Foreign Key to WeeklyPlans)
- `ClientId` (Foreign Key to Clients, nullable)
- `ClientName` - Client name (for new clients)
- `ClientType`, `ClientSpecialization` - Client details
- `PlannedVisitDate` - Scheduled visit date
- `VisitPurpose` - Visit purpose
- `Priority` - Item priority
- `Status` - Item status (Planned, Completed, Cancelled, Postponed)
- `IsNewClient` - Boolean flag for new clients
- `ActualVisitDate` - Actual visit date
- `Results`, `Feedback` - Visit results
- `SatisfactionRating` - Satisfaction rating
- `NextVisitDate` - Follow-up visit date
- `CancellationReason`, `PostponementReason` - Status reasons

## Indexes Created

### Performance Indexes

- `IX_Clients_Name` - Client name lookup
- `IX_Clients_Type` - Client type filtering
- `IX_Clients_Status` - Status filtering
- `IX_Clients_CreatedBy` - User's clients
- `IX_Clients_AssignedTo` - Assigned clients

### Visit Indexes

- `IX_ClientVisits_ClientId` - Client visits
- `IX_ClientVisits_VisitDate` - Date range queries
- `IX_ClientVisits_SalesmanId` - Salesman visits
- `IX_ClientVisits_Status` - Status filtering

### Interaction Indexes

- `IX_ClientInteractions_ClientId` - Client interactions
- `IX_ClientInteractions_InteractionDate` - Date range queries
- `IX_ClientInteractions_InteractionType` - Type filtering
- `IX_ClientInteractions_Status` - Status filtering

### Analytics Indexes

- `IX_ClientAnalytics_ClientId` - Client analytics
- `IX_ClientAnalytics_Period` - Period filtering
- `IX_ClientAnalytics_PeriodStart` - Date range queries

### Weekly Plan Indexes

- `IX_WeeklyPlans_EmployeeId` - Employee plans
- `IX_WeeklyPlans_WeekStartDate` - Week queries
- `IX_WeeklyPlans_Status` - Status filtering

### Plan Item Indexes

- `IX_WeeklyPlanItems_WeeklyPlanId` - Plan items
- `IX_WeeklyPlanItems_ClientId` - Client items
- `IX_WeeklyPlanItems_PlannedVisitDate` - Date queries
- `IX_WeeklyPlanItems_Status` - Status filtering
- `IX_WeeklyPlanItems_Priority` - Priority filtering

## Constraints Added

### Check Constraints

- `CK_ClientVisits_SatisfactionRating` - Rating between 1-5
- `CK_WeeklyPlanItems_SatisfactionRating` - Rating between 1-5
- `CK_Clients_SatisfactionRating` - Rating between 1-5

### Unique Constraints

- `UQ_WeeklyPlans_EmployeeId_WeekStartDate` - One plan per employee per week

## Foreign Key Relationships

### Client Relationships

- `Clients` → `ClientVisits` (One-to-Many)
- `Clients` → `ClientInteractions` (One-to-Many)
- `Clients` → `ClientAnalytics` (One-to-Many)
- `Clients` → `WeeklyPlanItems` (One-to-Many, nullable)

### Weekly Plan Relationships

- `WeeklyPlans` → `WeeklyPlanItems` (One-to-Many)

## Data Types and Precision

### Decimal Fields

- `PotentialValue` - decimal(18,2)
- `AverageVisitDuration` - decimal(18,2)
- `ClientSatisfactionScore` - decimal(18,2)
- `ConversionRate` - decimal(18,2)
- `Revenue` - decimal(18,2)
- `GrowthRate` - decimal(18,2)

### String Fields

- `Name` - nvarchar(200)
- `Type` - nvarchar(50)
- `Specialization` - nvarchar(100)
- `Location` - nvarchar(100)
- `Phone` - nvarchar(20)
- `Email` - nvarchar(100)
- `Website` - nvarchar(200)
- `Address` - nvarchar(500)
- `Notes` - nvarchar(1000)
- `Status` - nvarchar(50)
- `Priority` - nvarchar(50)

## Migration Script

The complete migration script is available in:
`SoitMed/Migrations/20250115000000_AddClientTrackingTables.sql`

## Repository Updates

### New Repository Interfaces

- `IClientRepository`
- `IClientVisitRepository`
- `IClientInteractionRepository`
- `IClientAnalyticsRepository`
- `IWeeklyPlanRepository`
- `IWeeklyPlanItemRepository`

### UnitOfWork Updates

Added new repository properties to `IUnitOfWork` and `UnitOfWork`:

- `Clients`
- `ClientVisits`
- `ClientInteractions`
- `ClientAnalytics`
- `WeeklyPlans`
- `WeeklyPlanItems`

## Context Updates

### DbSet Properties Added

```csharp
public DbSet<Client> Clients { get; set; }
public DbSet<ClientVisit> ClientVisits { get; set; }
public DbSet<ClientInteraction> ClientInteractions { get; set; }
public DbSet<ClientAnalytics> ClientAnalytics { get; set; }
public DbSet<WeeklyPlan> WeeklyPlans { get; set; }
public DbSet<WeeklyPlanItem> WeeklyPlanItems { get; set; }
```

## Testing

Unit tests have been created for the new repositories:

- `ClientVisitRepositoryTests`
- `ClientInteractionRepositoryTests`
- `ClientAnalyticsRepositoryTests`

## Rollback Instructions

To rollback these changes:

1. Drop the new tables in reverse order
2. Remove the new DbSet properties from Context
3. Remove the new repository properties from UnitOfWork
4. Remove the new repository classes and interfaces

## Notes

- All timestamps use UTC timezone
- JSON fields are stored as nvarchar and parsed in the application
- Soft deletes are not implemented for these tables
- All foreign keys have appropriate cascade/set null behavior
- Indexes are optimized for common query patterns
