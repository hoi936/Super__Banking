# Reset LocalLink development database and rebuild container stack from scratch
Write-Host "Resetting LocalLink Database and Docker volumes..." -ForegroundColor Yellow
docker compose down -v
Write-Host "Rebuilding and starting fresh containers..." -ForegroundColor Green
docker compose up --build -d
Write-Host "Database reset complete. Follow logs with: docker compose logs -f" -ForegroundColor Cyan
