# SoitMed Backend - Main Branch

## Business Overview

SoitMed is a comprehensive medical equipment management and sales system designed to streamline operations for medical equipment suppliers, hospitals, clinics, and healthcare professionals.

### Core Business Functions

#### 1. Sales Module
The sales module manages the complete sales lifecycle from initial client contact to deal closure:

- **Client Management**: Track and manage client information including hospitals, clinics, doctors, and technicians
- **Offer Management**: Create, manage, and track sales offers with detailed equipment specifications, pricing, and terms
- **Deal Processing**: Handle deal creation, approval workflows, and deal closure
- **Salesman Management**: Track salesman activities, weekly plans, tasks, and performance statistics
- **Sales Reports**: Generate comprehensive sales reports and analytics

#### 2. Maintenance Module
The maintenance module handles the complete lifecycle of equipment maintenance requests:

- **Maintenance Requests**: Customers can submit maintenance requests for their equipment with descriptions, symptoms, and multimedia attachments (images, videos, audio)
- **Visit Management**: Engineers can create maintenance visits, scan QR codes or enter serial numbers to load equipment data, and create detailed visit reports
- **Spare Parts Management**: Handle spare part requests with local/global availability checks, pricing by maintenance managers, and customer approval workflows
- **Engineer Assignment**: Automatic assignment of engineers based on location and workload, with manual assignment options
- **Customer Rating**: Customers can rate engineers after service completion
- **Status Tracking**: Real-time status tracking similar to delivery systems, allowing customers to see the current state of their maintenance requests

#### 3. Payment Module
The payment module manages all financial transactions:

- **Multiple Payment Methods**: Support for various payment methods including payment gateways (Stripe, PayPal, local gateways), cash, and bank transfers
- **Payment Processing**: Secure payment processing with transaction tracking
- **Accounting Management**: Accounting roles can confirm or reject payments with detailed notes
- **Payment Tracking**: Track payment status, amounts, and transaction history

#### 4. Equipment Management
Comprehensive equipment tracking and management:

- **Equipment Registration**: Register equipment with QR codes, serial numbers, and detailed specifications
- **Equipment Linking**: Equipment can be linked to hospitals or directly to customers (doctors)
- **Maintenance History**: Track maintenance history and repair visits for each equipment
- **Equipment Status**: Monitor equipment operational status

#### 5. User Management
Role-based access control with multiple user types:

- **Doctors**: Can submit maintenance requests, view equipment, and manage their profile
- **Technicians**: Similar to doctors with technician-specific permissions
- **Engineers**: Handle maintenance visits, create reports, and manage spare part requests
- **Salesmen**: Manage clients, create offers, track deals, and manage weekly plans
- **Sales Support**: Support sales operations, manage offer requests, and handle client interactions
- **Maintenance Support**: Coordinate maintenance requests, assign engineers, and manage workflows
- **Maintenance Manager**: Set spare part prices, approve global purchases, and manage maintenance operations
- **Spare Parts Coordinator**: Check spare part availability (local/global) and coordinate with inventory managers
- **Inventory Manager**: Prepare spare parts for engineers when available locally
- **Finance Manager & Finance Employee**: Handle payment confirmations, rejections, and accounting operations
- **Super Admin**: Full system access and oversight
- **Admin**: Administrative access with tracking capabilities

#### 6. Notification System
Real-time notifications for all system activities:

- **Request Notifications**: Notify relevant users when maintenance requests are created, assigned, or updated
- **Payment Notifications**: Notify customers and accounting staff about payment status changes
- **Spare Part Notifications**: Notify coordinators, managers, and engineers about spare part availability and pricing
- **Visit Notifications**: Notify maintenance support when visits are completed
- **Push Notifications**: Mobile push notifications for critical updates

### Business Workflows

#### Maintenance Request Workflow
1. Customer submits maintenance request with equipment selection, description, and attachments
2. Request appears in maintenance support dashboard
3. Maintenance support reviews and assigns to engineer (or auto-assignment based on location)
4. Engineer receives notification and creates maintenance visit
5. Engineer scans QR code or enters serial number to load equipment data
6. Engineer creates visit report with outcome (completed, needs second visit, needs spare part)
7. If spare part needed: Coordinator checks availability → If local: Inventory manager prepares → If global: Maintenance manager sets price → Customer approves/rejects
8. Customer can rate engineer after completion

#### Payment Workflow
1. Payment created for maintenance request or spare part
2. Customer selects payment method (gateway, cash, bank transfer)
3. Payment processed based on method
4. For cash/bank transfer: Accounting staff confirms or rejects
5. Customer receives notification about payment status

#### Sales Workflow
1. Salesman creates client record
2. Salesman creates offer request or directly creates offer
3. Offer includes equipment, pricing, terms, and payment plans
4. Client reviews and responds to offer
5. Deal created upon acceptance
6. Sales reports generated for tracking

### System Benefits

- **Streamlined Operations**: Automated workflows reduce manual coordination
- **Real-time Tracking**: Customers and staff can track request status in real-time
- **Comprehensive Reporting**: Detailed analytics for sales, maintenance, and payments
- **Multi-role Support**: System supports various user roles with appropriate permissions
- **Mobile Support**: Mobile applications for customers and engineers
- **Flexible Payment Options**: Multiple payment methods to accommodate different customer preferences
- **Equipment Lifecycle Management**: Complete tracking from purchase to maintenance

