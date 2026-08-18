$ErrorActionPreference = "Stop"

$BaseUrl = "http://localhost:8080"
$StaffEmail = "staff@locallink.local"
$AdminEmail = "admin@locallink.local"
$Password = "LocalLink@123"

function Write-Test {
    param([string]$Message)
    Write-Host "-> $Message" -ForegroundColor Cyan
}

function Write-Pass {
    param([string]$Message)
    Write-Host "[PASS] $Message" -ForegroundColor Green
}

function Write-Fail {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
    exit 1
}

function Login {
    param([string]$Email)
    $LoginBody = @{
        email = $Email
        password = $Password
    } | ConvertTo-Json
    $Response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method Post -Body $LoginBody -ContentType "application/json" -ErrorAction Stop
    return $Response.accessToken
}

try {
    Write-Test "1. STAFF API VERIFICATION"
    $StaffToken = Login -Email $StaffEmail
    $StaffHeaders = @{ Authorization = "Bearer $StaffToken" }

    # Staff allowed routes
    $Dashboard = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/dashboard" -Method Get -Headers $StaffHeaders
    if ($Dashboard.customers.total -ge 0) { Write-Pass "Staff can access Dashboard" } else { Write-Fail "Staff Dashboard failed" }

    $Customers = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/customers" -Method Get -Headers $StaffHeaders
    if ($null -ne $Customers.items) { Write-Pass "Staff can access Customers" } else { Write-Fail "Staff Customers failed" }

    $Transactions = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/transactions" -Method Get -Headers $StaffHeaders
    if ($null -ne $Transactions.items) { Write-Pass "Staff can access Transactions" } else { Write-Fail "Staff Transactions failed" }

    $Payments = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/payments" -Method Get -Headers $StaffHeaders
    if ($null -ne $Payments.items) { Write-Pass "Staff can access Payments" } else { Write-Fail "Staff Payments failed" }

    # Staff denied routes
    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users" -Method Get -Headers $StaffHeaders -ErrorAction Stop
        Write-Fail "Staff should NOT access Users"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 403) { Write-Pass "Staff denied from Users" }
        else { Write-Fail "Staff denied from Users failed: $_" }
    }

    try {
        Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/audit-logs" -Method Get -Headers $StaffHeaders -ErrorAction Stop
        Write-Fail "Staff should NOT access Audit Logs"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 403) { Write-Pass "Staff denied from Audit Logs" }
        else { Write-Fail "Staff denied from Audit Logs failed: $_" }
    }


    Write-Test "2. ADMIN API VERIFICATION"
    $AdminToken = Login -Email $AdminEmail
    $AdminHeaders = @{ Authorization = "Bearer $AdminToken" }

    # Admin allowed routes
    $Users = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users" -Method Get -Headers $AdminHeaders
    if ($null -ne $Users.items) { Write-Pass "Admin can access Users" } else { Write-Fail "Admin Users failed" }

    $AuditLogs = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/audit-logs" -Method Get -Headers $AdminHeaders
    if ($null -ne $AuditLogs.items) { Write-Pass "Admin can access Audit Logs" } else { Write-Fail "Admin Audit Logs failed" }

    # Mutations
    Write-Test "3. ADMIN MUTATION VERIFICATION"
    
    # 3.1 User Suspension
    # Find a customer user (we don't want to suspend the admin)
    $TargetUser = $Users.items | Where-Object { $_.roles -contains "CUSTOMER" } | Select-Object -First 1
    if ($TargetUser) {
        $OriginalStatus = $TargetUser.status
        $UserId = $TargetUser.id
        
        # Suspend
        $SuspendBody = @{ status = "SUSPENDED" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users/$UserId/status" -Method Patch -Body $SuspendBody -ContentType "application/json" -Headers $AdminHeaders | Out-Null
        $CheckUser = Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users/$UserId" -Method Get -Headers $AdminHeaders
        if ($CheckUser.status -eq "SUSPENDED") { Write-Pass "Admin suspended User" } else { Write-Fail "User suspension failed" }
        
        # Restore
        $RestoreBody = @{ status = $OriginalStatus } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users/$UserId/status" -Method Patch -Body $RestoreBody -ContentType "application/json" -Headers $AdminHeaders | Out-Null
        Write-Pass "User restored to original status"
    } else {
        Write-Host "No target user found for suspension test. Skipping." -ForegroundColor Yellow
    }

    # 3.2 Self-Suspension Prevention
    # Try to suspend the admin themselves
    $AdminId = ($Users.items | Where-Object { $_.email -eq $AdminEmail }).id
    try {
        $SelfSuspendBody = @{ status = "SUSPENDED" } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/api/v1/admin/users/$AdminId/status" -Method Patch -Body $SelfSuspendBody -ContentType "application/json" -Headers $AdminHeaders -ErrorAction Stop
        Write-Fail "Admin should NOT be able to suspend themselves"
    } catch {
        Write-Pass "Admin self-suspension prevented"
    }

    Write-Host "`nAll API verifications passed successfully!" -ForegroundColor Green
    exit 0
} catch {
    Write-Fail "Script failed: $($_.Exception.Message)"
}
