@echo off
REM Script to add users to Docker Registry with htpasswd authentication
REM Usage: add-user.bat <username> <password>

if "%1"=="" (
    echo Usage: add-user.bat ^<username^> ^<password^>
    echo Example: add-user.bat admin admin123
    exit /b 1
)

if "%2"=="" (
    echo Usage: add-user.bat ^<username^> ^<password^>
    echo Example: add-user.bat admin admin123
    exit /b 1
)

set USERNAME=%1
set PASSWORD=%2
set HTPASSWD_FILE=.\auth\htpasswd

echo Adding user: %USERNAME%
echo.

REM Create auth directory if it doesn't exist
if not exist ".\auth" mkdir ".\auth"

REM Use docker to run htpasswd
docker run --rm --entrypoint htpasswd httpd:2 -Bbn %USERNAME% %PASSWORD% >> %HTPASSWD_FILE%

echo.
echo ✅ User '%USERNAME%' added successfully!
echo 📁 Password file: %HTPASSWD_FILE%
echo.
echo To apply changes, restart the registry:
echo   docker compose restart registry
echo.
echo To login:
echo   docker login localhost:5000 -u %USERNAME%
