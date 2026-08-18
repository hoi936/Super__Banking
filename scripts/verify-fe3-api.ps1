$ErrorActionPreference = "Stop"
$apiBase = "http://localhost:8080/api/v1"
$email = "customer1@locallink.local"
$password = "LocalLink@123"

Write-Host "--- FE3 API VERIFICATION SCRIPT ---" -ForegroundColor Cyan

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

# 3. Lookup destination account
Write-Host "3. Lookup destination account 1000000002..."
$lookupResponse = Invoke-RestMethod -Uri "$apiBase/accounts/lookup/1000000002" -Method Get -Headers $headers
Write-Host "   -> Destination name: $($lookupResponse.accountName)" -ForegroundColor Green

# 4. Create Transfer
Write-Host "4. Transfer 100,000 VND (Idempotency Key)..."
$idempotencyKey = [guid]::NewGuid().ToString()
$transferBody = @{
    sourceAccountId = $sourceAccountId
    destinationAccountNumber = "1000000002"
    amount = 100000
    description = "Test Transfer FE3"
} | ConvertTo-Json

$headersTransfer = @{
    "Authorization" = "Bearer $token"
    "Content-Type"  = "application/json"
    "Idempotency-Key" = $idempotencyKey
}

$transferResponse = Invoke-RestMethod -Uri "$apiBase/transfers" -Method Post -Body $transferBody -Headers $headersTransfer
Write-Host "   -> Transfer success, Reference: $($transferResponse.reference)" -ForegroundColor Green

# 5. Check balance after transfer
Write-Host "5. Get balance again..."
$accountsAfterResponse = Invoke-RestMethod -Uri "$apiBase/accounts" -Method Get -Headers $headers
$sourceAccountAfter = $accountsAfterResponse | Where-Object { $_.accountNumber -eq "1000000001" }
Write-Host "   -> Final source balance: $($sourceAccountAfter.balance)" -ForegroundColor Yellow

$diff = $initialSourceBalance - $sourceAccountAfter.balance
if ($diff -eq 100000) {
    Write-Host "   -> Verified: Source account debited exactly 100,000 VND!" -ForegroundColor Green
} else {
    Write-Error "Balance deduction error! Actual difference: $diff"
}

# 6. Replay with same Idempotency Key
Write-Host "6. Replay request with same Idempotency Key..."
$transferResponseReplay = Invoke-RestMethod -Uri "$apiBase/transfers" -Method Post -Body $transferBody -Headers $headersTransfer
if ($transferResponseReplay.reference -eq $transferResponse.reference) {
    Write-Host "   -> Idempotency replay success, no double deduction!" -ForegroundColor Green
} else {
    Write-Error "Error! Replay request returned a different reference!"
}

# 7. Check Transactions
Write-Host "7. Get transactions..."
$txUrl = "$apiBase/transactions?page=1&pageSize=10"
$txResponse = Invoke-RestMethod -Uri $txUrl -Method Get -Headers $headers
$recentTx = $txResponse.items | Where-Object { $_.referenceNumber -eq $transferResponse.reference }

if ($recentTx) {
    Write-Host "   -> Found transaction in ledger!" -ForegroundColor Green
    
    # 8. Get Transaction Detail
    Write-Host "8. Get transaction detail..."
    $txDetail = Invoke-RestMethod -Uri "$apiBase/transactions/$($recentTx.id)" -Method Get -Headers $headers
    Write-Host "   -> Transaction Type: $($txDetail.transactionType)"
    Write-Host "   -> Source: $($txDetail.sourceAccountName)"
    Write-Host "   -> Destination: $($txDetail.destinationAccountName)"
    Write-Host "   -> Amount: $($txDetail.amount)"
    Write-Host "   -> Transaction detail retrieved successfully!" -ForegroundColor Green
} else {
    Write-Error "Cannot find transaction in ledger!"
}

Write-Host "ALL API TESTS PASSED!" -ForegroundColor Cyan
