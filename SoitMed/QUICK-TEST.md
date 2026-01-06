# Quick Test Guide

## Step 1: Start the Backend Server

Open a terminal and run:

```bash
cd backend/SoitMed
dotnet run
```

Wait until you see:
```
Now listening on: http://localhost:5117
```

## Step 2: Run the Test Script

Open **another terminal** and run:

```bash
cd backend/SoitMed
./test-with-token.sh
```

This script will:
1. ✅ Find which port the server is running on (5117 or 58868)
2. ✅ Test connection to legacy media API
3. ✅ Test file access
4. ✅ Check if file exists
5. ✅ Test file download

## Expected Results

### ✅ Success:
```
✓ Found server running on port 5117
✓ Success (HTTP 200)
✓ File download successful (HTTP 200)
File can be accessed from the other device!
```

### ❌ If Server Not Running:
```
✗ Server is not running!
Please start the server first:
  cd backend/SoitMed
  dotnet run
```

### ❌ If File Not Found:
Check:
1. `soitmed_data_backend` is running on `http://10.10.9.104:5266`
2. File exists in legacy paths
3. Network connectivity

## Manual Testing (Alternative)

If you prefer to test manually:

```bash
# Set token
export TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiaGVtZGFuQGhlbWRhbi5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkhlbWRhbl9UZXN0X0FkbWluaXN0cmF0aW9uXzAwMiIsImp0aSI6IjdhYzI4ZDM2LWExNDItNDE1OC04N2VlLWQzMTNlNDU5ODJmNyIsImFwcGxpY2F0aW9uIjoiU29pdC1NZWQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdXBlckFkbWluIiwiZXhwIjoxOTI1Mzg5NDg4LCJpc3MiOiJodHRwOi8vbG9jYWxob3N0OjU4ODY4IiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdDo0MjAwIn0.gQf0yFlyY5QYXbDN3Vx50IPvZE37Df6HpNkfJpTDjiY"

# Test verify
curl -X GET "http://localhost:5117/api/LegacyMedia/verify" \
  -H "Authorization: Bearer $TOKEN" | jq '.'

# Test file
curl -X GET "http://localhost:5117/api/LegacyMedia/test/074477c6d8164e17af7967847fdfbb97.jfif" \
  -H "Authorization: Bearer $TOKEN" | jq '.'
```

## Troubleshooting

### Port 5117 not working?
Try port 58868:
```bash
curl -X GET "http://localhost:58868/api/LegacyMedia/verify" \
  -H "Authorization: Bearer $TOKEN"
```

### Connection refused?
Make sure the server is running:
```bash
# Check if server is running
curl http://localhost:5117/health
```

### 401 Unauthorized?
Token might be expired. Get a new token from the frontend.

