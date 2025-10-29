# 📊 Sales Activity and Performance Tracking System - English Documentation

## Overview

A comprehensive sales activity and performance tracking system designed specifically for SoitMed. The system provides advanced capabilities for recording activities, managing deals and offers, and detailed analytics for managers.

## 🏗️ Architecture

### 1. Data Models

#### Enums

```csharp
public enum InteractionType { Visit, FollowUp }           // Type of interaction
public enum ClientType { A, B, C, D }                    // Client classification
public enum ActivityResult { Interested, NotInterested }  // Activity result
public enum RejectionReason { Cash, Price, Need, Other }  // Rejection reason
public enum DealStatus { Pending, Won, Lost }             // Deal status
public enum OfferStatus { Draft, Sent, Accepted, Rejected } // Offer status
```

#### Core Entities

- **ActivityLog**: Daily activity records
- **Deal**: Commercial deals
- **Offer**: Commercial offers

### 2. Service Layer

#### ActivityService

- Create activities with secure transactions
- Manage deals and offers
- Permission validation

#### ManagerDashboardService

- Comprehensive manager statistics
- Performance analysis
- Detailed reporting

### 3. API Endpoints

#### Sales Activities

- `POST /api/activities/tasks/{taskId}/activities` - Create new activity
- `GET /api/activities` - Get user activities
- `GET /api/activities/{id}` - Get specific activity

#### Deal Management

- `PUT /api/deals/{id}` - Update deal

#### Offer Management

- `PUT /api/offers/{id}` - Update offer

#### Manager Dashboard

- `GET /api/manager/dashboard-stats` - Manager statistics

## 📋 Usage Guide

### 1. Creating a New Activity

#### Example: Client Visit with Deal

```json
POST /api/activities/tasks/1/activities
{
    "interactionType": "Visit",
    "clientType": "A",
    "result": "Interested",
    "comment": "Client showed strong interest in MRI machine",
    "dealInfo": {
        "dealValue": 750000.00,
        "expectedCloseDate": "2024-02-15"
    }
}
```

#### Example: Follow-up with Offer

```json
POST /api/activities/tasks/1/activities
{
    "interactionType": "FollowUp",
    "clientType": "B",
    "result": "Interested",
    "comment": "Client requested detailed proposal for medical equipment",
    "offerInfo": {
        "offerDetails": "Comprehensive medical equipment proposal",
        "documentUrl": "https://example.com/proposal.pdf"
    }
}
```

#### Example: Client Rejection

```json
POST /api/activities/tasks/1/activities
{
    "interactionType": "Visit",
    "clientType": "C",
    "result": "NotInterested",
    "reason": "Price",
    "comment": "Client found price too high"
}
```

### 2. Updating a Deal

```json
PUT /api/deals/1
{
    "status": "Won",
    "dealValue": 800000.00,
    "expectedCloseDate": "2024-02-20"
}
```

### 3. Updating an Offer

```json
PUT /api/offers/1
{
    "status": "Accepted",
    "offerDetails": "Offer accepted with minor modifications"
}
```

### 4. Manager Dashboard Statistics

```json
GET /api/manager/dashboard-stats?startDate=2024-01-01&endDate=2024-01-31
```

#### Expected Response:

```json
{
	"success": true,
	"data": {
		"startDate": "2024-01-01",
		"endDate": "2024-01-31",
		"totalActivities": 150,
		"totalVisits": 100,
		"totalFollowUps": 50,
		"totalDeals": 25,
		"totalOffers": 30,
		"wonDeals": 15,
		"lostDeals": 8,
		"pendingDeals": 2,
		"acceptedOffers": 20,
		"rejectedOffers": 8,
		"draftOffers": 2,
		"totalDealValue": 5000000.0,
		"wonDealValue": 3000000.0,
		"averageDealValue": 200000.0,
		"conversionRate": 16.67,
		"offerAcceptanceRate": 66.67,
		"clientTypeStats": [
			{
				"clientType": "A",
				"clientTypeName": "A",
				"count": 60,
				"percentage": 40.0,
				"totalValue": 3000000.0
			}
		],
		"salespersonStats": [
			{
				"userId": "user1",
				"userName": "Ahmed Mohamed",
				"totalActivities": 75,
				"totalDeals": 12,
				"wonDeals": 8,
				"totalValue": 1500000.0,
				"wonValue": 1000000.0,
				"conversionRate": 16.0
			}
		],
		"recentActivities": [
			{
				"id": 1,
				"userName": "Ahmed Mohamed",
				"interactionType": "Visit",
				"interactionTypeName": "Visit",
				"clientType": "A",
				"clientTypeName": "A",
				"result": "Interested",
				"resultName": "Interested",
				"comment": "Successful visit to 57357 Hospital",
				"createdAt": "2024-01-30T10:00:00Z",
				"dealValue": 500000.0
			}
		]
	}
}
```

## 🔐 Authorization System

### Required Roles

- **Salesman**: Can create and update activities, deals, and offers
- **SalesManager**: Can access manager statistics
- **SuperAdmin**: Full permissions

### Permission Validation

- Each user can only access their own activities
- Managers can view their team's statistics
- Task ownership validation before creating activities

## ✅ Testing

### Unit Tests

- Comprehensive tests for all services
- Error handling tests
- Permission validation tests
- Secure transaction tests

### Integration Tests

- API endpoint tests
- Database tests
- Performance tests

## 🚀 Implementation and Setup

### 1. Create Database

```bash
dotnet ef migrations add AddSalesFunnelEntities
dotnet ef database update
```

### 2. Run Tests

```bash
dotnet test SoitMed.Tests
```

### 3. Run Application

```bash
dotnet run
```

## 📊 Key Performance Indicators (KPIs)

### For Sales Representatives

- Daily activity count
- Conversion rate
- Won deal values
- Accepted offer count

### For Managers

- Team overall performance
- Client distribution by classification
- Monthly trend analysis
- Salesperson performance comparison

## 🔧 Troubleshooting

### Common Issues

1. **Permission Error**: Ensure user has correct role
2. **Task Error**: Ensure task belongs to user
3. **Validation Error**: Ensure sent data is valid

### Error Logs

- All errors are logged in the logging system
- Errors can be tracked using activity ID
- Immediate notifications for critical errors

## 📞 Technical Support

For technical support or to report issues:

- Email: support@soitmed.com
- Phone: +20-123-456-7890
- Working Hours: 9:00 AM - 6:00 PM (Cairo Time)

---

**Developed by SoitMed Development Team**  
**Last Updated: January 2024**



