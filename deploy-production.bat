@echo off
REM ========================================
REM SCRIPT DEPLOY TRÊN MÁY PRODUCTION (Windows)
REM ========================================
REM Script này chạy trên máy production để pull image từ registry và deploy

setlocal enabledelayedexpansion

REM Configuration - THAY ĐỔI IP NÀY
set REGISTRY_URL=192.168.1.108:5000
set IMAGE_NAME=football-api
set IMAGE_TAG=latest

echo ========================================
echo Deploying Football Field Booking API
echo Registry: %REGISTRY_URL%
echo Image: %IMAGE_NAME%:%IMAGE_TAG%
echo ========================================

REM Step 1: Login to registry
echo.
echo Step 1: Login to Docker Registry...
echo Password: admin123
docker login %REGISTRY_URL% -u admin -p admin123
if errorlevel 1 (
    echo.
    echo ========================================
    echo ERROR: Failed to login to registry!
    echo ========================================
    echo.
    echo Possible issues:
    echo 1. Cannot connect to %REGISTRY_URL%
    echo 2. Docker is not running
    echo 3. Need to add insecure-registry to Docker config
    echo.
    echo To fix: Docker Desktop ^> Settings ^> Docker Engine
    echo Add: "insecure-registries": ["%REGISTRY_URL%"]
    echo.
    pause
    exit /b 1
)
echo ✅ Login successful!

REM Step 2: Pull latest image
echo.
echo Step 2: Pulling latest image...
docker pull %REGISTRY_URL%/%IMAGE_NAME%:%IMAGE_TAG%
if errorlevel 1 (
    echo.
    echo ========================================
    echo ERROR: Failed to pull image!
    echo ========================================
    echo.
    pause
    exit /b 1
)
echo ✅ Image pulled successfully!

REM Step 3: Stop existing containers
echo.
echo Step 3: Stopping existing containers...
docker compose -f docker-compose.prod.yml --env-file .env.prod down
echo ✅ Containers stopped!

REM Step 4: Start new containers
echo.
echo Step 4: Starting new containers...
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d --force-recreate
if errorlevel 1 (
    echo.
    echo ========================================
    echo ERROR: Failed to start containers!
    echo ========================================
    echo.
    echo Check if docker-compose.prod.yml and .env.prod exist
    echo.
    pause
    exit /b 1
)
echo ✅ Containers started successfully!

REM Step 5: Check health
echo.
echo Step 5: Checking container health...
timeout /t 5 /nobreak >nul
docker compose -f docker-compose.prod.yml ps

echo.
echo ========================================
echo ✅ Deployment completed successfully!
echo ========================================
echo.
echo Press any key to exit...
pause >nul

endlocal
