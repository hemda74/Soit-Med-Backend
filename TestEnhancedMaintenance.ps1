# Enhanced Maintenance Service Implementation Test Script
# Tests the Customer → Equipment → Visits Workflow

# =====================================================
# API ENDPOINT TESTS
# =====================================================

# Base URL (adjust as needed)
$baseUrl = "http://localhost:5000/api/EnhancedMaintenance"

# Test 1: Search Customers
Write-Host "=== Test 1: Search Customers ==="
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/customers/search?searchTerm=Ahmed&includeLegacy=true" -Method Get
    Write-Host "✓ Customer search successful"
    Write-Host "Found $($response.Items.Count) customers"
    $response.Items | ForEach-Object { Write-Host "  - $($_.Name) ($($_.Source))" }
} catch {
    Write-Host "✗ Customer search failed: $($_.Exception.Message)"
}

# Test 2: Get Customer Equipment and Visits
Write-Host "`n=== Test 2: Get Customer Equipment and Visits ==="
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/customer/1/equipment-visits?includeLegacy=true" -Method Get
    Write-Host "✓ Customer equipment and visits retrieved"
    Write-Host "Customer: $($response.Customer.Name)"
    Write-Host "Equipment count: $($response.Equipment.Count)"
    Write-Host "Visits count: $($response.Visits.Count)"
} catch {
    Write-Host "✗ Customer equipment and visits failed: $($_.Exception.Message)"
}

# Test 3: Get Equipment Visits
Write-Host "`n=== Test 3: Get Equipment Visits ==="
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/equipment/SN12345/visits?includeLegacy=true" -Method Get
    Write-Host "✓ Equipment visits retrieved"
    Write-Host "Equipment: $($response.Equipment.Model)"
    Write-Host "Visits count: $($response.Visits.Count)"
    $response.Visits | ForEach-Object { Write-Host "  - $($_.VisitDate): $($_.Status)" }
} catch {
    Write-Host "✗ Equipment visits failed: $($_.Exception.Message)"
}

# Test 4: Get Customer Visit Statistics
Write-Host "`n=== Test 4: Get Customer Visit Statistics ==="
try {
    $startDate = Get-Date -Year 2024 -Month 1 -Day 1
    $endDate = Get-Date -Year 2024 -Month 12 -Day 31
    $response = Invoke-RestMethod -Uri "$baseUrl/customer/1/visit-stats?startDate=$startDate&endDate=$endDate" -Method Get
    Write-Host "✓ Customer visit statistics retrieved"
    Write-Host "Total visits: $($response.TotalVisits)"
    Write-Host "Completed visits: $($response.CompletedVisits)"
    Write-Host "Completion rate: $($response.CompletionRate)%"
} catch {
    Write-Host "✗ Customer visit statistics failed: $($_.Exception.Message)"
}

# Test 5: Complete Visit (Engineer Role Required)
Write-Host "`n=== Test 5: Complete Visit ==="
try {
    $visitData = @{
        VisitId = "1"
        Source = "New"
        Report = "Maintenance completed successfully. All systems operational."
        ActionsTaken = "Replaced faulty sensor, calibrated system, tested all functions."
        PartsUsed = "Sensor X-123, Calibration Kit"
        ServiceFee = 150.00
        Outcome = "Completed"
        Notes = "Customer satisfied with service."
    }
    
    $json = $visitData | ConvertTo-Json -Depth 10
    $response = Invoke-RestMethod -Uri "$baseUrl/visits/complete" -Method Post -Body $json -ContentType "application/json"
    Write-Host "✓ Visit completion successful"
    Write-Host "Visit ID: $($response.VisitId)"
    Write-Host "Status: $($response.Status)"
} catch {
    Write-Host "✗ Visit completion failed: $($_.Exception.Message)"
}

# Test 6: Data Consistency Verification (Admin Role Required)
Write-Host "`n=== Test 6: Data Consistency Verification ==="
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/admin/data-consistency" -Method Get
    Write-Host "✓ Data consistency verification retrieved"
    Write-Host "Status: $($response.Status)"
    $response.Recommendations | ForEach-Object { Write-Host "  - $_" }
} catch {
    Write-Host "✗ Data consistency verification failed: $($_.Exception.Message)"
}

# Test 7: Workflow Test
Write-Host "`n=== Test 7: Workflow Test ==="
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/test/workflow?customerId=1" -Method Get
    Write-Host "✓ Workflow test successful"
    Write-Host "Workflow: $($response.Workflow)"
    Write-Host "Status: $($response.Status)"
} catch {
    Write-Host "✗ Workflow test failed: $($_.Exception.Message)"
}

Write-Host "`n=== Test Summary ==="
Write-Host "All API endpoint tests completed."
Write-Host "Check the results above for any failures."
Write-Host "Note: Some endpoints require specific user roles (Engineer, Admin, Manager)"

# =====================================================
# SQL DATABASE TESTS
# =====================================================

Write-Host "`n=== SQL Database Tests ==="
Write-Host "Run the following SQL scripts to verify database consistency:"
Write-Host "1. VerifyDataConsistency.sql - Check data consistency between TBS and itiwebapi44"
Write-Host "2. CheckPlanCompletionCalculation.sql - Verify weekly plan completion calculations"
Write-Host "3. TestMarkTaskCompleted.sql - Test task completion logic"

# =====================================================
# INTEGRATION TESTS
# =====================================================

Write-Host "`n=== Integration Tests ==="
Write-Host "Test the complete workflow:"
Write-Host "1. Search for a customer"
Write-Host "2. Get customer's equipment"
Write-Host "3. Get equipment's visit history"
Write-Host "4. Complete a new visit"
Write-Host "5. Verify visit statistics updated"

Write-Host "`n=== Expected Behavior ==="
Write-Host "• Customer search should return results from both databases"
Write-Host "• Equipment lookup should work with serial numbers"
Write-Host "• Visit history should include both legacy and new visits"
Write-Host "• Visit completion should update status and statistics"
Write-Host "• Data consistency should be maintained across databases"

Write-Host "`n=== Troubleshooting ==="
Write-Host "• Check connection strings in appsettings.json"
Write-Host "• Verify TBS database permissions"
Write-Host "• Ensure user roles are properly assigned"
Write-Host "• Check logs for detailed error messages"
Write-Host "• Run SQL scripts to verify database integrity"

Write-Host "`nTest script completed!"
