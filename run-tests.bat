@echo off
REM ========================================
REM SCRIPT CHẠY UNIT TESTS LOCAL
REM ========================================

setlocal

echo ========================================
echo 🧪 Running Unit Tests
echo ========================================
echo.

REM Restore dependencies
echo [1/3] Restoring dependencies...
dotnet restore
if errorlevel 1 (
    echo ❌ Failed to restore dependencies
    pause
    exit /b 1
)
echo ✅ Dependencies restored
echo.

REM Build test project
echo [2/3] Building test project...
dotnet build src\DoAn.Tests\DoAn.Tests.csproj --no-restore
if errorlevel 1 (
    echo ❌ Failed to build test project
    pause
    exit /b 1
)
echo ✅ Test project built
echo.

REM Run tests
echo [3/3] Running tests...
dotnet test src\DoAn.Tests\DoAn.Tests.csproj ^
    --no-build ^
    --verbosity normal ^
    --logger "console;verbosity=detailed"

if errorlevel 1 (
    echo.
    echo ========================================
    echo ❌ TESTS FAILED!
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo ✅ ALL TESTS PASSED!
echo ========================================
echo.
pause

endlocal
