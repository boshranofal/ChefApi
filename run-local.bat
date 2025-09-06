@echo off
echo ========================================
echo      ChefAPI Local Development Run
echo ========================================

echo Setting up environment...
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://localhost:5000

echo.
echo Starting ChefAPI...
echo 🌐 API will be available at: http://localhost:5000
echo 📚 Swagger UI: http://localhost:5000/swagger
echo 💚 Health Check: http://localhost:5000/health
echo.

cd /d "%~dp0ChefApi.PL"

echo Restoring packages...
dotnet restore

echo.
echo Running application...
dotnet run

pause
