@echo off
rem Automate backend and frontend startup for the Vietnamese workspace
cd /d "%~dp0"

rem Verify required tools
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet CLI is not found on PATH.
    pause
    exit /b 1
)

where npm >nul 2>&1
if errorlevel 1 (
    echo ERROR: npm is not found on PATH.
    pause
    exit /b 1
)

set BACKEND_DIR=%~dp0Backend\Backend
set FRONTEND_DIR=%~dp0Fontend

echo Starting backend in %BACKEND_DIR%...
start "Backend" cmd /k "cd /d "%BACKEND_DIR%" && dotnet run"

echo Starting frontend in %FRONTEND_DIR%...
start "Frontend" cmd /k "cd /d "%FRONTEND_DIR%" && if not exist node_modules npm install && npm start"

echo Launched backend and frontend. Check the two cmd windows for logs.
pause
