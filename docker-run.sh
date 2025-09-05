#!/bin/bash

# Build the Docker image
echo "Building Docker image..."
docker build -t chefapi:latest .

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "✅ Docker image built successfully!"
    
    # Run the container
    echo "Starting container..."
    docker run -p 8080:8080 \
        -e ASPNETCORE_ENVIRONMENT=Development \
        -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=ChefApiDB;Integrated Security=false;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;" \
        -e JWTOptions__SecretKey="YourSuperSecretKeyThatIsAtLeast256Bits!" \
        --name chefapi-container \
        chefapi:latest
else
    echo "❌ Docker build failed!"
    exit 1
fi
