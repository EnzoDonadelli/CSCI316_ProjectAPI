@echo off
echo ========================================
echo Starting Photo Sharing API with Sample Data
echo ========================================
echo.

cd /d "%~dp0VisaoAPI"
echo Current directory: %CD%
echo.

echo Building project...
dotnet build --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed!
    pause
    exit /b 1
)
echo ✅ Build successful!
echo.

echo Starting API...
echo 🚀 The API will start and automatically create sample data
echo 📊 Sample data includes:
echo   • 2 Users (johndoe, janesmith)
echo   • 2 Albums (Nature Escapes, Forever Moments)  
echo   • 4 Photos with descriptions
echo   • Tags, Likes, Comments, and Followers
echo.
echo 🌐 Once running, access Swagger at: https://localhost:5001
echo    or the HTTP version at: http://localhost:5000
echo.
echo 🔍 Test endpoints like:
echo   • GET /api/users (to see johndoe and janesmith)
echo   • GET /api/photos (to see all 4 photos)
echo   • GET /api/albums (to see the 2 albums)
echo.
echo Press Ctrl+C to stop the server when done testing
echo ========================================
echo.

dotnet run