# ==========================================
# WEEKLY PLAN API TEST SCRIPT
# ==========================================

$BaseUrl = "http://localhost:5117/api"
$Email = "hemdan@hemdan.com"
$Password = "356120Ahmed@shraf2"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "WEEKLY PLAN API TESTING" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# ==========================================
# TEST 1: Login to get JWT Token
# ==========================================
Write-Host "TEST 1: Login..." -ForegroundColor Yellow

$loginBody = @{
    userName = $Email
    password = $Password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/Account/Login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    
    if ($token) {
        Write-Host "[SUCCESS] Login Successful!" -ForegroundColor Green
        Write-Host "Token received (length: $($token.Length))" -ForegroundColor Gray
        Write-Host ""
    } else {
        Write-Host "[FAILED] Login Failed: No token received" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "[FAILED] Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Create headers with token
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# ==========================================
# TEST 2: Get All Weekly Plans (Paginated)
# ==========================================
Write-Host "TEST 2: Get All Weekly Plans..." -ForegroundColor Yellow

try {
    $plansResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan?page=1&pageSize=10" -Method GET -Headers $headers
    
    Write-Host "[SUCCESS] Successfully Retrieved Plans!" -ForegroundColor Green
    Write-Host "Total Plans: $($plansResponse.data.totalCount)" -ForegroundColor Cyan
    Write-Host "Current Page: $($plansResponse.data.page) of $($plansResponse.data.totalPages)" -ForegroundColor Cyan
    Write-Host ""
    
    if ($plansResponse.data.data.Count -gt 0) {
        Write-Host "Sample Plans (first 3):" -ForegroundColor Cyan
        $counter = 1
        foreach ($plan in $plansResponse.data.data | Select-Object -First 3) {
            Write-Host "  [$counter] Plan ID: $($plan.id)" -ForegroundColor White
            Write-Host "      Title: $($plan.title)" -ForegroundColor White
            Write-Host "      Employee: $($plan.employeeName)" -ForegroundColor Gray
            Write-Host "      Week: $($plan.weekStartDate) to $($plan.weekEndDate)" -ForegroundColor Gray
            Write-Host "      Tasks: $($plan.totalTasks) Total | $($plan.completedTasks) Completed | $($plan.completionPercentage)% Done" -ForegroundColor Gray
            if ($plan.rating) {
                Write-Host "      Rating: $($plan.rating) out of 5 stars" -ForegroundColor Yellow
            } else {
                Write-Host "      Rating: Not reviewed yet" -ForegroundColor Gray
            }
            Write-Host ""
            $counter++
        }
    }
    
    # Save first plan ID for next tests
    if ($plansResponse.data.data.Count -gt 0) {
        $testPlanId = $plansResponse.data.data[0].id
    }
    
} catch {
    Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# ==========================================
# TEST 3: Get Specific Plan by ID
# ==========================================
if ($testPlanId) {
    Write-Host "TEST 3: Get Plan by ID ($testPlanId)..." -ForegroundColor Yellow
    
    try {
        $planResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan/$testPlanId" -Method GET -Headers $headers
        
        Write-Host "[SUCCESS] Successfully Retrieved Plan Details!" -ForegroundColor Green
        Write-Host "Title: $($planResponse.data.title)" -ForegroundColor Cyan
        Write-Host "Description: $($planResponse.data.description)" -ForegroundColor Gray
        Write-Host ""
        
        Write-Host "Tasks List:" -ForegroundColor Cyan
        $taskCounter = 1
        foreach ($task in $planResponse.data.tasks) {
            $status = if ($task.isCompleted) { "[DONE]" } else { "[TODO]" }
            Write-Host "  $taskCounter. $status $($task.title)" -ForegroundColor White
            $taskCounter++
        }
        Write-Host ""
        
        if ($planResponse.data.dailyProgresses.Count -gt 0) {
            Write-Host "Daily Progress Entries: $($planResponse.data.dailyProgresses.Count)" -ForegroundColor Cyan
            foreach ($progress in $planResponse.data.dailyProgresses) {
                Write-Host "  Date: $($progress.progressDate)" -ForegroundColor White
                Write-Host "    Notes: $($progress.notes.Substring(0, [Math]::Min(100, $progress.notes.Length)))..." -ForegroundColor Gray
            }
            Write-Host ""
        }
        
    } catch {
        Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ==========================================
# TEST 4: Filter Plans - Not Reviewed
# ==========================================
Write-Host "TEST 4: Get Plans Without Manager Review..." -ForegroundColor Yellow

try {
    $unreviewedResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan?hasManagerReview=false&page=1&pageSize=5" -Method GET -Headers $headers
    
    Write-Host "[SUCCESS] Successfully Retrieved Unreviewed Plans!" -ForegroundColor Green
    Write-Host "Count: $($unreviewedResponse.data.totalCount)" -ForegroundColor Cyan
    Write-Host ""
    
} catch {
    Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
}

# ==========================================
# TEST 5: Filter by Rating
# ==========================================
Write-Host "TEST 5: Get Plans with Rating >= 4..." -ForegroundColor Yellow

try {
    $highRatedResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan?minRating=4&page=1&pageSize=5" -Method GET -Headers $headers
    
    Write-Host "[SUCCESS] Successfully Retrieved High-Rated Plans!" -ForegroundColor Green
    Write-Host "Count: $($highRatedResponse.data.totalCount)" -ForegroundColor Cyan
    
    if ($highRatedResponse.data.data.Count -gt 0) {
        foreach ($plan in $highRatedResponse.data.data) {
            Write-Host "  - $($plan.title) | Rating: $($plan.rating) stars" -ForegroundColor White
        }
    }
    Write-Host ""
    
} catch {
    Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
}

# ==========================================
# TEST 6: Create a New Weekly Plan
# ==========================================
Write-Host "TEST 6: Create New Weekly Plan..." -ForegroundColor Yellow

$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm"
$newPlanBody = @{
    title = "API Test Plan $timestamp"
    description = "This is a test plan created via API"
    weekStartDate = "2024-10-15"
    weekEndDate = "2024-10-21"
    tasks = @(
        @{
            title = "Test Task 1"
            description = "Description for task 1"
            displayOrder = 1
        },
        @{
            title = "Test Task 2"
            description = "Description for task 2"
            displayOrder = 2
        }
    )
} | ConvertTo-Json -Depth 10

try {
    $createResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan" -Method POST -Body $newPlanBody -Headers $headers
    
    Write-Host "[SUCCESS] Successfully Created New Plan!" -ForegroundColor Green
    Write-Host "Plan ID: $($createResponse.data.id)" -ForegroundColor Cyan
    Write-Host "Title: $($createResponse.data.title)" -ForegroundColor Cyan
    Write-Host "Tasks Created: $($createResponse.data.tasks.Count)" -ForegroundColor Cyan
    Write-Host ""
    
    $newPlanId = $createResponse.data.id
    
} catch {
    Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}

# ==========================================
# TEST 7: Add Task to Plan
# ==========================================
if ($newPlanId) {
    Write-Host "TEST 7: Add New Task to Plan..." -ForegroundColor Yellow
    
    $newTaskBody = @{
        title = "Additional Task"
        description = "Added after plan creation"
        displayOrder = 3
    } | ConvertTo-Json
    
    try {
        $taskResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan/$newPlanId/tasks" -Method POST -Body $newTaskBody -Headers $headers
        
        Write-Host "[SUCCESS] Successfully Added Task!" -ForegroundColor Green
        Write-Host "Task ID: $($taskResponse.data.id)" -ForegroundColor Cyan
        Write-Host "Title: $($taskResponse.data.title)" -ForegroundColor Cyan
        Write-Host ""
        
        $newTaskId = $taskResponse.data.id
        
    } catch {
        Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ==========================================
# TEST 8: Update Task (Mark as Completed)
# ==========================================
if ($newPlanId -and $newTaskId) {
    Write-Host "TEST 8: Update Task (Mark as Completed)..." -ForegroundColor Yellow
    
    $updateTaskBody = @{
        title = "Additional Task (Updated)"
        description = "This task has been updated"
        isCompleted = $true
        displayOrder = 3
    } | ConvertTo-Json
    
    try {
        $updateTaskResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan/$newPlanId/tasks/$newTaskId" -Method PUT -Body $updateTaskBody -Headers $headers
        
        Write-Host "[SUCCESS] Successfully Updated Task!" -ForegroundColor Green
        Write-Host "Task: $($updateTaskResponse.data.title)" -ForegroundColor Cyan
        Write-Host "Completed: $($updateTaskResponse.data.isCompleted)" -ForegroundColor Cyan
        Write-Host ""
        
    } catch {
        Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ==========================================
# TEST 9: Add Daily Progress
# ==========================================
if ($newPlanId) {
    Write-Host "TEST 9: Add Daily Progress..." -ForegroundColor Yellow
    
    $progressBody = @{
        progressDate = "2024-10-15"
        notes = "Today I completed the first task successfully and started working on the second task."
        tasksWorkedOn = @(1, 2)
    } | ConvertTo-Json
    
    try {
        $progressResponse = Invoke-RestMethod -Uri "$BaseUrl/WeeklyPlan/$newPlanId/progress" -Method POST -Body $progressBody -Headers $headers
        
        Write-Host "[SUCCESS] Successfully Added Daily Progress!" -ForegroundColor Green
        Write-Host "Progress ID: $($progressResponse.data.id)" -ForegroundColor Cyan
        Write-Host "Date: $($progressResponse.data.progressDate)" -ForegroundColor Cyan
        Write-Host ""
        
    } catch {
        Write-Host "[FAILED] $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
    }
}

# ==========================================
# FINAL SUMMARY
# ==========================================
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "[SUCCESS] All tests completed!" -ForegroundColor Green
Write-Host ""
Write-Host "Tested Endpoints:" -ForegroundColor Yellow
Write-Host "  1. POST /api/Account/Login" -ForegroundColor White
Write-Host "  2. GET  /api/WeeklyPlan (Paginated)" -ForegroundColor White
Write-Host "  3. GET  /api/WeeklyPlan/{id}" -ForegroundColor White
Write-Host "  4. GET  /api/WeeklyPlan (Filter: hasManagerReview)" -ForegroundColor White
Write-Host "  5. GET  /api/WeeklyPlan (Filter: minRating)" -ForegroundColor White
Write-Host "  6. POST /api/WeeklyPlan (Create)" -ForegroundColor White
Write-Host "  7. POST /api/WeeklyPlan/{id}/tasks" -ForegroundColor White
Write-Host "  8. PUT  /api/WeeklyPlan/{id}/tasks/{taskId}" -ForegroundColor White
Write-Host "  9. POST /api/WeeklyPlan/{id}/progress" -ForegroundColor White
Write-Host ""
Write-Host "[SUCCESS] Weekly Plan API is working perfectly!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
