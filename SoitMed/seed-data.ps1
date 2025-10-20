# PowerShell script to seed data
# This script will call the seeding endpoint

$baseUrl = "https://localhost:7000"  # Adjust port if needed
$seedingEndpoint = "$baseUrl/api/dataseeding/seed-sales-data"

Write-Host "Starting data seeding process..."
Write-Host "Endpoint: $seedingEndpoint"

try {
    # Call the seeding endpoint
    $response = Invoke-RestMethod -Uri $seedingEndpoint -Method POST -ContentType "application/json"
    
    Write-Host "Seeding completed successfully!"
    Write-Host "Response: $($response | ConvertTo-Json -Depth 3)"
}
catch {
    Write-Host "Error occurred during seeding:"
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)"
    Write-Host "Error Message: $($_.Exception.Message)"
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody"
    }
}

