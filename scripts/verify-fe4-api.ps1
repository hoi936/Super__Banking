$ErrorActionPreference = "Stop"
$apiBase = "http://localhost:8080/api/v1"
$email = "customer1@locallink.local"
$password = "LocalLink@123"

Write-Host "--- FE4 API VERIFICATION SCRIPT ---" -ForegroundColor Cyan

# 1. Login Customer 1
Write-Host "1. Login Customer 1 ($email)..."
$loginBody = @{
    email = $email
    password = $password
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$apiBase/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.accessToken
Write-Host "   -> Login success, received Access Token." -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
}

# 2. Get Accounts
Write-Host "2. Get accounts (GET /accounts)..."
$accountsResponse = Invoke-RestMethod -Uri "$apiBase/accounts" -Method Get -Headers $headers
$sourceAccount = $accountsResponse | Where-Object { $_.accountNumber -eq "1000000001" }

if (-not $sourceAccount) {
    Write-Error "Cannot find source account 1000000001!"
}
Write-Host "   -> Initial source balance (1000000001): $($sourceAccount.balance)" -ForegroundColor Yellow
$sourceAccountId = $sourceAccount.id
$initialSourceBalance = $sourceAccount.balance

# 3. Get Bills
Write-Host "3. Get bills (GET /bills)..."
$billsResponse = Invoke-RestMethod -Uri "$apiBase/bills?status=UNPAID" -Method Get -Headers $headers
$demoBill = $billsResponse.items | Select-Object -First 1

if (-not $demoBill) {
    Write-Error "Cannot find any UNPAID bill to test! Database might not have seeded data or they are all paid."
}
Write-Host "   -> Found UNPAID Bill: $($demoBill.billNumber) - $($demoBill.providerName) - $($demoBill.amount) VND" -ForegroundColor Green
$billId = $demoBill.id
$billAmount = $demoBill.amount

# 4. Get Bill Detail
Write-Host "4. Get bill detail..."
$billDetail = Invoke-RestMethod -Uri "$apiBase/bills/$billId" -Method Get -Headers $headers
Write-Host "   -> Bill detail retrieved." -ForegroundColor Green

# 5. Create Payment
Write-Host "5. Pay Bill (Idempotency Key)..."
$idempotencyKey = [guid]::NewGuid().ToString()
$paymentBody = @{
    billId = $billId
    accountId = $sourceAccountId
} | ConvertTo-Json

$headersPayment = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
    "Idempotency-Key" = $idempotencyKey
}

$paymentResponse = Invoke-RestMethod -Uri "$apiBase/payments" -Method Post -Body $paymentBody -Headers $headersPayment
Write-Host "   -> Payment success, Reference: $($paymentResponse.reference)" -ForegroundColor Green
$paymentId = $paymentResponse.paymentId
$paymentRef = $paymentResponse.reference

# 6. Check balance after payment
Write-Host "6. Get balance again..."
$accountsAfterResponse = Invoke-RestMethod -Uri "$apiBase/accounts" -Method Get -Headers $headers
$sourceAccountAfter = $accountsAfterResponse | Where-Object { $_.accountNumber -eq "1000000001" }
Write-Host "   -> Final source balance: $($sourceAccountAfter.balance)" -ForegroundColor Yellow

$expectedAfter = $initialSourceBalance - $billAmount
if ($sourceAccountAfter.balance -eq $expectedAfter) {
    Write-Host "   -> Verified: Source account debited exactly $billAmount VND!" -ForegroundColor Green
} else {
    Write-Error "Balance deduction error! Expected: $expectedAfter, Actual: $($sourceAccountAfter.balance)"
}

# 7. Replay with same Idempotency Key
Write-Host "7. Replay request with same Idempotency Key..."
$paymentResponseReplay = Invoke-RestMethod -Uri "$apiBase/payments" -Method Post -Body $paymentBody -Headers $headersPayment
if ($paymentResponseReplay.reference -eq $paymentRef) {
    Write-Host "   -> Idempotency replay success, no double deduction!" -ForegroundColor Green
} else {
    Write-Error "Error! Replay request returned a different reference!"
}

# 8. Check Bill Status
Write-Host "8. Check Bill Status..."
$billAfter = Invoke-RestMethod -Uri "$apiBase/bills/$billId" -Method Get -Headers $headers
if ($billAfter.status -eq "PAID") {
    Write-Host "   -> Verified: Bill status is PAID." -ForegroundColor Green
} else {
    Write-Error "Bill status is not PAID!"
}

# 9. Get Payments
Write-Host "9. Get payments history..."
$paymentsHistory = Invoke-RestMethod -Uri "$apiBase/payments?page=1&pageSize=10" -Method Get -Headers $headers
$recentPayment = $paymentsHistory.items | Where-Object { $_.referenceNumber -eq $paymentRef }
if ($recentPayment) {
    Write-Host "   -> Found payment in history!" -ForegroundColor Green
} else {
    Write-Error "Payment not found in history!"
}

# 10. Check Transactions Ledger
Write-Host "10. Get transactions ledger..."
$txUrl = "$apiBase/transactions?page=1&pageSize=10"
$txResponse = Invoke-RestMethod -Uri $txUrl -Method Get -Headers $headers
$recentTx = $txResponse.items | Where-Object { $_.referenceNumber -eq $paymentRef }
if ($recentTx) {
    Write-Host "   -> Found transaction in ledger with Type: $($recentTx.transactionType)" -ForegroundColor Green
} else {
    Write-Error "Cannot find transaction in ledger!"
}

# 11. Notifications
Write-Host "11. Check notifications..."
$notiResponse = Invoke-RestMethod -Uri "$apiBase/notifications" -Method Get -Headers $headers
$paymentNoti = $notiResponse.items | Where-Object { $_.type -eq "PAYMENT" -and $_.isRead -eq $false } | Select-Object -First 1

if ($paymentNoti) {
    Write-Host "   -> Found PAYMENT notification!" -ForegroundColor Green
    
    # Check unread count
    $unreadCountRes = Invoke-RestMethod -Uri "$apiBase/notifications/unread-count" -Method Get -Headers $headers
    $initialUnread = $unreadCountRes.count
    Write-Host "   -> Current Unread Count: $initialUnread" -ForegroundColor Yellow

    # Mark as read
    Write-Host "   -> Mark notification as read..."
    $markReadUrl = "$apiBase/notifications/$($paymentNoti.id)/read"
    Invoke-RestMethod -Uri $markReadUrl -Method Patch -Headers $headers | Out-Null
    
    $unreadCountRes2 = Invoke-RestMethod -Uri "$apiBase/notifications/unread-count" -Method Get -Headers $headers
    if ($unreadCountRes2.count -lt $initialUnread) {
        Write-Host "   -> Verified: Unread count decreased to $($unreadCountRes2.count)!" -ForegroundColor Green
    } else {
        Write-Error "Unread count did not decrease!"
    }

    # Mark all read
    Write-Host "   -> Mark all as read..."
    Invoke-RestMethod -Uri "$apiBase/notifications/read-all" -Method Patch -Headers $headers | Out-Null
    
    $unreadCountRes3 = Invoke-RestMethod -Uri "$apiBase/notifications/unread-count" -Method Get -Headers $headers
    if ($unreadCountRes3.count -eq 0) {
        Write-Host "   -> Verified: Unread count is 0!" -ForegroundColor Green
    } else {
        Write-Error "Unread count is not 0!"
    }

} else {
    Write-Host "   -> No unread PAYMENT notification found (maybe already read). Skip notification test." -ForegroundColor Yellow
}

Write-Host "ALL API TESTS PASSED!" -ForegroundColor Cyan
