# Test script for password reset functionality
$baseUrl = "http://localhost:5117/api/Account"

Write-Host "Testing SoitMed Password Reset API" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Test 1: Forgot Password
Write-Host "`n1. Testing Forgot Password Endpoint" -ForegroundColor Yellow
$forgotPasswordData = @{
    email = "test@example.com"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/forgot-password" -Method POST -Body $forgotPasswordData -ContentType "application/json"
    Write-Host "Response: " -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

# Test 2: Verify Code (this will fail since we don't have a real code)
Write-Host "`n2. Testing Verify Code Endpoint" -ForegroundColor Yellow
$verifyCodeData = @{
    email = "test@example.com"
    code = "123456"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/verify-code" -Method POST -Body $verifyCodeData -ContentType "application/json"
    Write-Host "Response: " -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

# Test 3: Reset Password (this will fail since we don't have a valid code)
Write-Host "`n3. Testing Reset Password Endpoint" -ForegroundColor Yellow
$resetPasswordData = @{
    email = "test@example.com"
    verificationCode = "123456"
    newPassword = "NewPassword123!"
    confirmPassword = "NewPassword123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/reset-password" -Method POST -Body $resetPasswordData -ContentType "application/json"
    Write-Host "Response: " -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 3
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Red
    }
}

Write-Host "`nTest completed!" -ForegroundColor Green
