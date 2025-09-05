@echo off
echo ========================================
echo      ChefAPI Docker Build & Run
echo ========================================

echo Cleaning up any existing containers...
docker stop chefapi-container 2>nul
docker rm chefapi-container 2>nul

echo.
echo Building Docker image...
docker build -t chefapi:latest .

if %ERRORLEVEL% EQU 0 (
    echo ✅ Docker image built successfully!
    echo.
    echo Starting container...
    docker run -d ^
        -p 8080:8080 ^
        --name chefapi-container ^
        -e "ASPNETCORE_ENVIRONMENT=Development" ^
        -e "ConnectionStrings__DefaultConnection=Data Source=host.docker.internal,1433;Initial Catalog=ChefApiDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;" ^
        -e "JWTOptions__SecretKey=YourSuperSecretKeyForJWTSigningThatIsAtLeast256BitsLongForSecurity!" ^
        chefapi:latest

    if %ERRORLEVEL% EQU 0 (
        echo ✅ Container started successfully!
        echo.
        echo 🌐 API is running at: http://localhost:8080
        echo 📚 Swagger UI: http://localhost:8080/swagger
        echo 💚 Health Check: http://localhost:8080/health
        echo.
        echo Container logs:
        timeout /t 3 /nobreak >nul
        docker logs chefapi-container
    ) else (
        echo ❌ Failed to start container
    )
) else (
    echo ❌ Docker build failed!
    echo.
    echo Common solutions:
    echo 1. Make sure Docker Desktop is running
    echo 2. Check if all project files exist
    echo 3. Try: docker system prune -f
)

echo.
echo Press any key to exit...
pause >nul
