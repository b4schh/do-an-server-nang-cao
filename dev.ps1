Write-Host "==> Starting Docker services (db, minio, nginx)..." -ForegroundColor Green
docker compose -f docker-compose.dev.yml up -d db minio nginx

Start-Sleep -Seconds 3

Write-Host "==> Starting API with dotnet watch run..." -ForegroundColor Cyan
dotnet watch run
