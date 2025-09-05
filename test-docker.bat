@echo off
echo Checking Docker status...
docker --version
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker is not available
    echo Please start Docker Desktop first
    pause
    exit /b 1
)

echo Testing Docker daemon...
docker ps >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Docker daemon is not running
    echo Please make sure Docker Desktop is started and running
    pause
    exit /b 1
)

echo ✅ Docker is ready!
echo Building image...

docker build --no-cache --progress=plain -t myapi .

if %ERRORLEVEL% EQU 0 (
    echo ✅ Build successful!
    echo Running container...
    docker run -d -p 8080:8080 --name myapi-container myapi
    echo Container started at http://localhost:8080
) else (
    echo ❌ Build failed
)

pause
