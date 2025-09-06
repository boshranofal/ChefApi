# Multi-stage Dockerfile for ASP.NET Core 8 Web API
# Production: PostgreSQL via DATABASE_URL | Development: SQL Server
# Optimized for Render deployment

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for better layer caching
COPY *.sln ./
COPY ChefApi.PL/*.csproj ./ChefApi.PL/
COPY ChefApi.BLL/*.csproj ./ChefApi.BLL/
COPY ChefApi.DAL/*.csproj ./ChefApi.DAL/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish the main project (ChefApi.PL)
WORKDIR /src/ChefApi.PL
RUN dotnet publish -c Release -o /app/publish

# Runtime stage for production deployment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Create non-root user for security
RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --create-home appuser && \
    chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Set production environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port 8080 (Render will override with PORT env var)
EXPOSE 8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

# Environment variables that Render will set:
# - PORT (dynamic port assignment)
# - DATABASE_URL (PostgreSQL connection string)
# - JWTOptions__SecretKey (JWT secret key)

# Run the application using the correct DLL name
ENTRYPOINT ["dotnet", "ChefApi.PL.dll"]
