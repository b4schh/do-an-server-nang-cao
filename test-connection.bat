@echo off
REM ========================================
REM SCRIPT TEST CONNECTIVITY TO CI SERVER
REM ========================================

setlocal

set REGISTRY_URL=192.168.1.108:5000
set REGISTRY_HOST=192.168.1.108

echo ========================================
echo Testing Connection to CI Server
echo Registry: %REGISTRY_URL%
echo ========================================
echo.

REM Test 1: Ping host
echo [Test 1/5] Pinging CI server %REGISTRY_HOST%...
ping -n 2 -w 1000 %REGISTRY_HOST%
if errorlevel 1 (
    echo.
    echo ❌ FAILED: Cannot ping %REGISTRY_HOST%
    echo.
    echo    Possible issues:
    echo    1. Machines not on same network
    echo    2. Firewall blocking ICMP
    echo    3. Wrong IP address
    echo.
    echo    Your IP addresses:
    ipconfig | findstr "IPv4"
    echo.
    echo    Try ping from Command Prompt manually:
    echo    ping %REGISTRY_HOST%
    echo.
    goto :error
) else (
    echo ✅ PASSED: Can ping %REGISTRY_HOST%
)
echo.

REM Test 2: Check Docker is running
echo [Test 2/5] Checking Docker status...
docker version >nul 2>&1
if errorlevel 1 (
    echo ❌ FAILED: Docker is not running
    echo    Start Docker Desktop first
    goto :error
) else (
    echo ✅ PASSED: Docker is running
)
echo.

REM Test 3: Test registry connectivity
echo [Test 3/5] Testing registry connectivity...
echo    Trying: http://%REGISTRY_URL%/v2/_catalog
curl -s -u admin:admin123 --connect-timeout 5 http://%REGISTRY_URL%/v2/_catalog
if errorlevel 1 (
    echo.
    echo ❌ FAILED: Cannot connect to registry at %REGISTRY_URL%
    echo.
    echo    Possible issues:
    echo    1. Registry not running on CI server
    echo    2. Firewall blocking port 5000
    echo    3. Wrong IP or port
    echo.
    echo    On CI server, check registry is running:
    echo    docker ps | findstr registry
    echo.
    goto :error
) else (
    echo.
    echo ✅ PASSED: Can connect to registry
)
echo.

REM Test 4: Check insecure-registry config
echo [Test 4/5] Checking Docker insecure-registry config...
docker info | findstr /C:"%REGISTRY_URL%" >nul 2>&1
if errorlevel 1 (
    echo ⚠️  WARNING: %REGISTRY_URL% not in insecure-registries
    echo.
    echo    To fix:
    echo    1. Open Docker Desktop
    echo    2. Settings ^> Docker Engine
    echo    3. Add this line:
    echo       "insecure-registries": ["%REGISTRY_URL%"]
    echo    4. Apply ^& Restart
docker login %REGISTRY_URL% -u admin -p admin123
if errorlevel 1 (
    echo. any key to continue anyway...
    pause >nul
) else (
    echo ✅ PASSED: Insecure registry configured
)
echo.

REM Test 5: Test login
echo [Test 5/5] Testing registry login...
echo admin123 | docker login %REGISTRY_URL% -u admin --password-stdin >nul 2>&1
if errorlevel 1 (
    echo ❌ FAILED: Cannot login to registry
    echo    Check credentials (admin / admin123)
    goto :error
) else (
    echo ✅ PASSED: Login successful
)
echo.

echo ========================================
echo ✅ ALL TESTS PASSED!
echo ========================================
echo.
echo You can now run deploy-production.bat
echo.
pause
exit /b 0

:error
echo.
echo ========================================
echo ❌ TESTS FAILED!
echo ========================================
echo.
echo Fix the issues above before deploying
echo.
pause
exit /b 1

endlocal
