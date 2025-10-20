## Sales Module API Documentation

This document consolidates the current Sales module APIs and flows implemented in the backend. It covers Sales Reports and the Sales Request Workflow used between Salesman, Sales Support, and Legal roles.

### Roles and Access
- Salesman: create/update/delete own sales reports; create request workflows; view own sent requests.
- SalesManager, SuperAdmin: view/rate all sales reports.
- SalesSupport: receive and update assigned request workflows.
- LegalManager, LegalEmployee: receive and update assigned request workflows when legal involvement is required.

---

## 1) Sales Reports

Base route: `api/SalesReport`

DTOs: `CreateSalesReportDto`, `UpdateSalesReportDto`, `FilterSalesReportsDto`, `RateSalesReportDto`, `SalesReportResponseDto`, `PaginatedSalesReportsResponseDto`.

### 1.1 Create Report
- Method: POST `api/SalesReport`
- Auth: Salesman
- Body (CreateSalesReportDto):
```json
{
  "title": "string (<=100)",
  "body": "string (<=2000)",
  "type": "string",
  "reportDate": "YYYY-MM-DD"
}
```
- Responses:
  - 201 Created: `{ data: SalesReportResponseDto, message: "Report created successfully" }`
  - 400 Validation errors
  - 409 If a report with same type and date already exists for the employee

### 1.2 Update Report
- Method: PUT `api/SalesReport/{id}`
- Auth: Salesman (own reports only)
- Body (UpdateSalesReportDto) same shape as create
- Responses:
  - 200 OK: `{ data: SalesReportResponseDto, message: "Report updated successfully" }`
  - 400 Validation errors
  - 404 Not found or no permission

### 1.3 Delete Report
- Method: DELETE `api/SalesReport/{id}`
- Auth: Salesman (own reports only)
- Responses:
  - 200 OK: `{ success: true, message: "Report deleted successfully" }`
  - 404 Not found or no permission

### 1.4 Get Report By Id
- Method: GET `api/SalesReport/{id}`
- Auth: Any authenticated user; access control is enforced:
  - Managers can access any report; Salesman can access own report
- Responses:
  - 200 OK: `{ data: SalesReportResponseDto, message: "Report retrieved successfully" }`
  - 404 Not found or no permission

### 1.5 List Reports (Filter & Paginate)
- Method: GET `api/SalesReport`
- Auth: Any authenticated user; results depend on role:
  - SalesManager/SuperAdmin: all reports
  - Salesman: own reports only
- Query (FilterSalesReportsDto):
  - `employeeId` (optional, manager only)
  - `startDate` (YYYY-MM-DD)
  - `endDate` (YYYY-MM-DD)
  - `type` (string)
  - `page` (int, default 1)
  - `pageSize` (int, default 10)
- Responses:
  - 200 OK: `{ data: PaginatedSalesReportsResponseDto, message: "Found {TotalCount} report(s)" }`
  - 400 Validation errors

### 1.6 Rate Report
- Method: POST `api/SalesReport/{id}/rate`
- Auth: SalesManager, SuperAdmin
- Body (RateSalesReportDto):
```json
{
  "rating": 1,
  "comment": "string (<=500)"
}
```
- Responses:
  - 200 OK: `{ data: SalesReportResponseDto, message: "Report rated successfully" }`
  - 400 Validation errors
  - 404 Not found

---

## 2) Sales Request Workflows

Base route: `api/RequestWorkflows`

Purpose: Orchestrate requests from Salesman to SalesSupport (offers) or to Legal (deals), track status updates, and provide sent/assigned views.

DTOs: `CreateWorkflowRequestDto`, `UpdateWorkflowRequestStatusDto`, `RequestWorkflowResponseDto` and related delivery/payment term DTOs.

### 2.1 Create Request Workflow
- Method: POST `api/RequestWorkflows`
- Auth: Salesman
- Body (CreateWorkflowRequestDto):
```json
{
  "activityLogId": 0,
  "offerId": 0, // optional, specify either offerId or dealId
  "dealId": 0,  // optional
  "toUserId": "string",
  "comment": "string",
  "deliveryTermsId": 0, // optional
  "paymentTermsId": 0  // optional
}
```
- Notes: Exactly one of `offerId` or `dealId` must be provided.
- Responses:
  - 200 OK: `{ data: RequestWorkflowResponseDto, message: "Request workflow created successfully" }`
  - 400 Invalid request (e.g., neither or both of offerId/dealId)
  - 403 Unauthorized for the action

### 2.2 Get Sent Requests (by current salesman)
- Method: GET `api/RequestWorkflows/sent`
- Auth: Salesman
- Query: `status` (optional, enum RequestStatus)
- Responses:
  - 200 OK: `{ data: RequestWorkflowResponseDto[] }`

### 2.3 Get Assigned Requests (by current user)
- Method: GET `api/RequestWorkflows/assigned`
- Auth: SalesSupport, LegalManager, LegalEmployee
- Query: `status` (optional, enum RequestStatus)
- Responses:
  - 200 OK: `{ data: RequestWorkflowResponseDto[] }`

### 2.4 Update Request Workflow Status
- Method: PUT `api/RequestWorkflows/{workflowId}/status`
- Auth: SalesSupport, LegalManager, LegalEmployee
- Body (UpdateWorkflowRequestStatusDto):
```json
{
  "status": "Pending | Approved | Rejected | InProgress | Completed", // see RequestStatus enum
  "comment": "string"
}
```
- Responses:
  - 200 OK: `{ data: RequestWorkflowResponseDto, message: "Request workflow status updated successfully" }`
  - 400 Invalid request
  - 403 Unauthorized
  - 404 Not found or not permitted

---

## 3) Models Involved (high level)
- ActivityLog: records sales activities; linked to either an Offer or a Deal.
- Offer: sales support workflow target; linked via ActivityLog.
- Deal: legal workflow target; linked via ActivityLog.
- RequestWorkflow: tracks a submitted request (from user/role to another), status, optional Delivery/Payment terms.
- SalesReport: daily/periodic reports created by salesmen and optionally rated by managers.

---

## 4) Security and Authorization
- All endpoints require authentication.
- Role-based access checks enforce who can create/read/update/delete specific resources as indicated above.

---

## 5) Pagination and Filtering
- Sales reports listing supports pagination: `page`, `pageSize` and filter fields `employeeId` (manager only), `startDate`, `endDate`, `type`.
- Response includes `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage`.

---

## 6) Standard Responses
- Success response envelopes typically include `{ data, message }` or `{ success, message }`.
- Error responses include appropriate HTTP status codes with messages; validation errors return field-level messages.

---

## 7) Notes and Best Practices
- Ensure the requester supplies either `offerId` or `dealId` in workflow creation, not both.
- Use role-appropriate tokens when testing endpoints (e.g., Salesman vs SalesManager).
- For DTO field constraints (length, required, ranges), refer to the annotations in the DTO classes.

---

## 8) Related Documentation
- See `Documentation/SALES_REPORT_API_DOCUMENTATION.md` for extended examples on report filtering and rating.
- See `Documentation/SALES_SUPPORT_API_EXAMPLES.md` and `Documentation/SALES_SUPPORT_API_EXAMPLES_ARABIC.md` for request workflow usage patterns.
- See `Documentation/REACT_SALES_MODULE_IMPLEMENTATION.md` and `Documentation/REACT_NATIVE_SALES_MODULE_IMPLEMENTATION.md` for frontend integration.


