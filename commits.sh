#!/bin/bash

echo "================================================================================
                            BACKEND PROJECT - CATEGORIZED COMMITS
================================================================================
Generated: $(date)
"

echo "--------------------------------------------------------------------------------
FEATURES - Chat System
--------------------------------------------------------------------------------
- Enhance Chat System with Image Message Support
- Implement Role-Based Chat Conversation Filtering
- Add chat type categorization for conversation routing
- Implement chat type-based conversation routing and assignment
- Update chat API to support chat type categorization
- Add database migration for chat type column
- Add Chat System implementation
- Add chat media files
- Add SQL script for chat image fields migration
"

echo "--------------------------------------------------------------------------------
FEATURES - Weekly Plans
--------------------------------------------------------------------------------
- Fix type conversion error: cast weeklyPlanId from long to int
- Fix int and long id in weeklyplan
- Fix type conversion errors in WeeklyPlanService and update weekly plan models
- Update Weekly Plan functionality
- Enable date filtering for salesmen on their own weekly plans
- Add weekly plan filtering with viewed tracking
- Enhance Weekly Plans API with complete sales funnel data and employee information
- Include tasks in weekly plan responses and fix pdf export service
- Implement Weekly Plan System and Mobile Device Support
"

echo "--------------------------------------------------------------------------------
FEATURES - Sales Module
--------------------------------------------------------------------------------
- Enhance deal and sales module functionality
- Implement comprehensive sales module with complete CRUD operations
- Add SalesSupport data seeding script with user Ahmed_Hemdan_Engineering_001
- Update sales offer models and repositories for enhanced offer management
- Refactor offer and sales offer business logic
- Update sales module data transfer objects
- Enhance product management and repository layer
"

echo "--------------------------------------------------------------------------------
FEATURES - Offers & Deals
--------------------------------------------------------------------------------
- Optimize Offer PDF Pagination and Layout
- Enhance Offer PDF Generation and Customer Response Authorization
- Implement Discount Calculation Logic in Offer Service
- Add Discount Amount Support to Offer DTOs
- Add on-demand PDF generation endpoint for offers
- Enhance offer PDF generation with on-demand streaming
- Implement offer PDF generation service
- Update Offer Management system
- Add SalesManager rejection reason fields to OfferResponseDTO and mapping methods
"

echo "--------------------------------------------------------------------------------
FEATURES - Products & Categories
--------------------------------------------------------------------------------
- Add product documents and images
- Add Product Category feature with controller, DTOs, models, repositories and services
- Enhance product management and repository layer
"

echo "--------------------------------------------------------------------------------
FEATURES - Client Management
--------------------------------------------------------------------------------
- Enhance client module with additional functionality
- Update Client Management functionality
- Make client type field optional and enhance notification logging
"

echo "--------------------------------------------------------------------------------
FEATURES - User Management & Authentication
--------------------------------------------------------------------------------
- Add PersonalMail field to all roles except Doctor and Technician
- Replace the user email in mail body to user name and last name
- Implement password reset functionality with email verification
- Update User APIs to use user ID and improve architecture
- Add sales manager user image uploads
- Add role seeding and update sales service
- Add comprehensive role-specific user creation APIs and user data management
- Add role-specific user creation DTOs with image support
- Add comprehensive user image management system
- Add UserImage and SalesReport entities with DTOs
- Add Inventory Manager and Spare Parts Coordinator DTOs with image support
- Update DELETE User API to use userId parameter and add comprehensive error handling
- Fix user image CRUD operations and resolve unique constraint violations
- Add missing image upload handling to doctor creation endpoint
- Standardize Role Naming Convention (Salesman to SalesMan)
- Implement Case-Insensitive Role Authorization
- Add SuperAdmin password update API and improve login error messages
- Add SuperAdmin management and database migration scripts
- Add UserIdGenerationService for custom ID pattern
- Update DTOs to use email as username
- Update Controllers to use custom ID generation and email as username
- Update Program.cs with service registrations and Identity configuration
- Add JWT authentication
"

echo "--------------------------------------------------------------------------------
FEATURES - Notifications
--------------------------------------------------------------------------------
- Implement comprehensive real-time notification system with push notification support
- Enhance notification logging in OfferRequestService and NotificationService for better debugging
"

echo "--------------------------------------------------------------------------------
FEATURES - Reports & Analytics
--------------------------------------------------------------------------------
- Add monthly report type support
- Add SuperAdmin access to sales reports
- Restore SalesReportValidators with monthly support
- Add Finance Sales Report API with comprehensive functionality and data seeding
- Fix LINQ translation error in FinanceSalesReportSeedingService
"

echo "--------------------------------------------------------------------------------
FEATURES - Maintenance & Equipment
--------------------------------------------------------------------------------
- Major System Overhaul: Equipment Management & Model Restructure
- Complete Hospital Management System with QR Code Generation
"

echo "--------------------------------------------------------------------------------
FEATURES - Configuration & Infrastructure
--------------------------------------------------------------------------------
- Add Firebase admin SDK configuration
- Add wwwroot directories for static files and uploads
- Update launch settings and add Google services configuration
- Add Static File CORS Middleware
- Add Voice Upload Service and Task Progress voice description support
- Add PDF upload service for document management
- Update connection strings in appsettings for development and production environments
- Updates the new configs for db connection
- Add new network IP address
- Add company support mail
"

echo "--------------------------------------------------------------------------------
FIXES
--------------------------------------------------------------------------------
- Fix type conversion error: cast weeklyPlanId from long to int
- Fix int and long id in weeklyplan
- Fix type conversion errors in WeeklyPlanService and update weekly plan models
- Fix image upload crash issue in RoleSpecificUserController
- Fix image upload path: Save images to wwwroot/uploads instead of AppData
- Fix activity creation API validation and transaction handling
- Fix LINQ translation error in FinanceSalesReportSeedingService
- Fix user image CRUD operations and resolve unique constraint violations
- Fix missing image upload handling to doctor creation endpoint
"

echo "--------------------------------------------------------------------------------
REFACTORING
--------------------------------------------------------------------------------
- Update Controllers with Improved Role Handling
- Update Repositories and Unit of Work
- Update Services and Service Interfaces
- Update Models and Database Context
- Update DTOs and Data Transfer Objects
- Implement comprehensive Repository Pattern
- Update core services and project configuration
- Update service collection extensions for new services
"

echo "--------------------------------------------------------------------------------
DOCUMENTATION
--------------------------------------------------------------------------------
- Update Documentation and Configuration Files
- Add Chat System documentation
- Add comprehensive README with business and technical details for dev branch
- Add business-focused README for main branch
- Add comprehensive User Registration Documentation
- Add quick start guide for salesmen weekly plan API
- Add complete weekly plan testing guide for frontend teams
- Add complete sales module integration guide
- Add comprehensive weekly plan guide for frontend teams
- Add consolidated Sales Module API documentation (reports and workflows)
- Update README.md to include Sales Module features and API documentation references
- Remove unnecessary markdown documentation files, keep only role-specific API documentation files
- Remove outdated weekly plan documentation files
- Remove obsolete commit commands documentation
- Clean up old documentation files
"

echo "--------------------------------------------------------------------------------
CLEANUP & MAINTENANCE
--------------------------------------------------------------------------------
- Remove obsolete scripts and files related to network profile and weekly plans
- Remove unused documentation, scripts, and SQL files
- Remove unused APIs and SuperAdmin scripts
- Remove Migration APIs and related code
- Remove seeding data
- Clean up codebase: Remove emojis and test files
- Remove emojis from documentation for professional appearance
- Delete unused files
- Remove obsolete commit commands documentation
- Cleanup: move uploads outside project and map /uploads; replace Console.WriteLine with ILogger; add global exception handlers; remove console writes from Program; exclude uploads from file watching
- Remove development data seeding and dead code; align Doctor model with DB schema; fix CreateDoctor to set HospitalId; unregister seeding services
"

echo "--------------------------------------------------------------------------------
TESTS
--------------------------------------------------------------------------------
- Add unit test and test the app in current image
"

echo "================================================================================
                        END OF BACKEND COMMITS
================================================================================
"

