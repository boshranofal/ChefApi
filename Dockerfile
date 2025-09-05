# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln ./

# Copy all project files
COPY ChefApi.PL/*.csproj ./ChefApi.PL/
COPY ChefApi.BLL/*.csproj ./ChefApi.BLL/
COPY ChefApi.DAL/*.csproj ./ChefApi.DAL/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build and publish the application
WORKDIR /src/ChefApi.PL
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChefApi.PL.dll"]

ENTRYPOINT ["dotnet", "ChefApi.PL.dll"]
