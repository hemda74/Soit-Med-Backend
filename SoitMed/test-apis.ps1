# PowerShell script to test Finance Sales Report APIs

# Base URL
$baseUrl = "http://localhost:5117"

# Test token (you'll need to replace this with a valid token)
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaGVtZGFuQGhlbWRhbi5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkhlbWRhbl9UZXN0X0FkbWluaXN0cmF0aW9uXzAwMiIsImp0aSI6IjJhNjE5OGE2LWVmZGEtNDc4OC05N2UzLTBhZTg1MzkzNDZjOSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlN1cGVyQWRtaW4iLCJleHAiOjE5MTY0MjcyNzQsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTg4NjgiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjQyMDB9.zHmlKADNyozBJYdACaBVLo91r7-fqWoI10xFbbThv38"

# Headers
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "Testing Finance Sales Report APIs..." -ForegroundColor Green

# Test 1: Seed Finance Data
Write-Host "`n1. Testing Data Seeding..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/DataSeeding/finance-all" -Method POST -Headers $headers
    Write-Host "✅ Data Seeding Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Data Seeding Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

# Test 2: Get All Finance Reports (FinanceManager)
Write-Host "`n2. Testing Get All Finance Reports..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport/all?page=1&pageSize=5" -Method GET -Headers $headers
    Write-Host "✅ Get All Reports Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Get All Reports Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

# Test 3: Create Finance Report (FinanceEmployee)
Write-Host "`n3. Testing Create Finance Report..." -ForegroundColor Yellow
$createData = @{
    title = "Test Daily Financial Analysis - $(Get-Date -Format 'yyyy-MM-dd')"
    body = "This is a test financial analysis report created via API testing. The report contains sample financial data and analysis for testing purposes."
    type = "daily"
    reportDate = (Get-Date -Format 'yyyy-MM-dd')
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport" -Method POST -Headers $headers -Body $createData
    Write-Host "✅ Create Report Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
    $reportId = $response.data.id
} catch {
    Write-Host "❌ Create Report Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
    $reportId = 1  # Use default ID for further tests
}

# Test 4: Rate Report (FinanceManager)
Write-Host "`n4. Testing Rate Report..." -ForegroundColor Yellow
$rateData = @{
    rating = 4
    comment = "Excellent financial analysis with clear insights and actionable recommendations. Great work on the detailed breakdown!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport/$reportId/rate" -Method POST -Headers $headers -Body $rateData
    Write-Host "✅ Rate Report Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Rate Report Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

# Test 5: Get Specific Report
Write-Host "`n5. Testing Get Specific Report..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport/$reportId" -Method GET -Headers $headers
    Write-Host "✅ Get Specific Report Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Get Specific Report Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

# Test 6: Update Report (FinanceEmployee)
Write-Host "`n6. Testing Update Report..." -ForegroundColor Yellow
$updateData = @{
    title = "Updated Test Daily Financial Analysis - $(Get-Date -Format 'yyyy-MM-dd')"
    body = "This is an updated test financial analysis report. The report has been modified to include additional insights and recommendations for better financial management."
    type = "daily"
    reportDate = (Get-Date -Format 'yyyy-MM-dd')
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport/$reportId" -Method PUT -Headers $headers -Body $updateData
    Write-Host "✅ Update Report Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Update Report Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

# Test 7: Get Reports with Filtering
Write-Host "`n7. Testing Get Reports with Filtering..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/FinanceSalesReport?type=daily&page=1&pageSize=10" -Method GET -Headers $headers
    Write-Host "✅ Get Reports with Filtering Success:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "❌ Get Reports with Filtering Error:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
    }
}

Write-Host "`nAPI Testing Complete!" -ForegroundColor Green
