# Apply EF Core migrations to the local database
Write-Host "Applying EF Core migrations..." -ForegroundColor Yellow
dotnet dotnet-ef database update --project src/LocalLink.Infrastructure --startup-project src/LocalLink.API
Write-Host "Database update complete." -ForegroundColor Green
