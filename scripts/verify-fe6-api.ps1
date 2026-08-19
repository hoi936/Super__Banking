$ErrorActionPreference = 'Stop'

$API_BASE_URL = 'http://localhost:8080/api/v1'

function Write-Step ($msg) {
    Write-Host ""
    Write-Host "=== $msg ===" -ForegroundColor Cyan
}

function Write-Pass ($msg) {
    Write-Host "[PASS] $msg" -ForegroundColor Green
}

function Write-Fail ($msg) {
    Write-Host "[FAIL] $msg" -ForegroundColor Red
    exit 1
}

function Invoke-Api {
    param (
        [string]$Method,
        [string]$Endpoint,
        [string]$Token,
        [object]$Body
    )

    $headers = @{}
    if ($Token) {
        $headers['Authorization'] = 'Bearer ' + $Token
    }

    $uri = $API_BASE_URL + $Endpoint
    
    $params = @{
        Method = $Method
        Uri = $uri
        Headers = $headers
    }

    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 10)
        $params.ContentType = 'application/json'
    }

    try {
        $response = Invoke-RestMethod @params
        return @{
            Status = 200
            Data = $response
        }
    } catch {
        if ($_.Exception.Response) {
            return @{
                Status = [int]$_.Exception.Response.StatusCode
                Data = $null
            }
        }
        return @{
            Status = 500
            Data = $null
        }
    }
}

Write-Step 'LOCALINK M8 — FE6 API VERIFICATION SCRIPT'

# 1. Customer Flow
Write-Step 'Testing Customer Capabilities'
$loginRes = Invoke-Api -Method POST -Endpoint '/auth/login' -Body @{ email = 'customer1@locallink.local'; password = 'LocalLink@123' }
if ($loginRes.Status -ne 200) { Write-Fail 'Customer login failed' }
$customerToken = $loginRes.Data.accessToken
Write-Pass 'Customer login successful'

$res = Invoke-Api -Method GET -Endpoint '/auth/me' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /auth/me failed' }
Write-Pass 'GET /auth/me successful'

$res = Invoke-Api -Method GET -Endpoint '/customers/me' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /customers/me failed' }
Write-Pass 'GET /customers/me successful'

$res = Invoke-Api -Method GET -Endpoint '/accounts' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /accounts failed' }
Write-Pass 'GET /accounts successful'

$res = Invoke-Api -Method GET -Endpoint '/transactions' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /transactions failed' }
Write-Pass 'GET /transactions successful'

$res = Invoke-Api -Method GET -Endpoint '/bills' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /bills failed' }
Write-Pass 'GET /bills successful'

$res = Invoke-Api -Method GET -Endpoint '/payments' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /payments failed' }
Write-Pass 'GET /payments successful'

$res = Invoke-Api -Method GET -Endpoint '/notifications' -Token $customerToken
if ($res.Status -ne 200) { Write-Fail 'GET /notifications failed' }
Write-Pass 'GET /notifications successful'


# 2. Staff Flow
Write-Step 'Testing Staff Capabilities'
$loginRes = Invoke-Api -Method POST -Endpoint '/auth/login' -Body @{ email = 'staff@locallink.local'; password = 'LocalLink@123' }
if ($loginRes.Status -ne 200) { Write-Fail 'Staff login failed' }
$staffToken = $loginRes.Data.accessToken
Write-Pass 'Staff login successful'

$res = Invoke-Api -Method GET -Endpoint '/admin/dashboard' -Token $staffToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/dashboard failed for Staff' }
Write-Pass 'GET /admin/dashboard successful for Staff'

$res = Invoke-Api -Method GET -Endpoint '/admin/customers' -Token $staffToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/customers failed for Staff' }
Write-Pass 'GET /admin/customers successful for Staff'

$res = Invoke-Api -Method GET -Endpoint '/admin/transactions' -Token $staffToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/transactions failed for Staff' }
Write-Pass 'GET /admin/transactions successful for Staff'

$res = Invoke-Api -Method GET -Endpoint '/admin/payments' -Token $staffToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/payments failed for Staff' }
Write-Pass 'GET /admin/payments successful for Staff'

# Verify Staff is forbidden from Users & Audit Logs
$res = Invoke-Api -Method GET -Endpoint '/admin/users' -Token $staffToken
if ($res.Status -ne 403) { Write-Fail 'Expected 403 for Staff accessing /admin/users' }
Write-Pass 'Staff is correctly forbidden from /admin/users'

$res = Invoke-Api -Method GET -Endpoint '/admin/audit-logs' -Token $staffToken
if ($res.Status -ne 403) { Write-Fail 'Expected 403 for Staff accessing /admin/audit-logs' }
Write-Pass 'Staff is correctly forbidden from /admin/audit-logs'


# 3. Admin Flow
Write-Step 'Testing Admin Capabilities'
$loginRes = Invoke-Api -Method POST -Endpoint '/auth/login' -Body @{ email = 'admin@locallink.local'; password = 'LocalLink@123' }
if ($loginRes.Status -ne 200) { Write-Fail 'Admin login failed' }
$adminToken = $loginRes.Data.accessToken
Write-Pass 'Admin login successful'

$res = Invoke-Api -Method GET -Endpoint '/admin/dashboard' -Token $adminToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/dashboard failed for Admin' }
Write-Pass 'GET /admin/dashboard successful for Admin'

$res = Invoke-Api -Method GET -Endpoint '/admin/users' -Token $adminToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/users failed for Admin' }
Write-Pass 'GET /admin/users successful for Admin'

$res = Invoke-Api -Method GET -Endpoint '/admin/audit-logs' -Token $adminToken
if ($res.Status -ne 200) { Write-Fail 'GET /admin/audit-logs failed for Admin' }
Write-Pass 'GET /admin/audit-logs successful for Admin'

Write-Step 'ALL FE6 REGRESSION TESTS PASSED'
exit 0
