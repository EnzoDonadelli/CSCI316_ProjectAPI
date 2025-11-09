@echo off
echo Starting VisaoAPI with Sample Data...
echo.
cd /d "%~dp0VisaoAPI"
echo Current directory: %CD%
echo.
echo Building project...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)
echo.
echo Starting API server...
echo The API will be available at: https://localhost:5001 or http://localhost:5000
echo Swagger documentation will be at the root URL when running
echo.
dotnet run
pause